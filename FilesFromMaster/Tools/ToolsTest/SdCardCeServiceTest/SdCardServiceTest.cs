using Core.Api.Service;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Storage.Common;
using Storage.Settings;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.SDCard;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CodeInSanityTest.Utilities;
using Neo.ApplicationFramework.Common.Utilities;

namespace Neo.ApplicationFramework.Tools.SdCardCeServiceTest
{
    [TestFixture]
    public class SdCardServiceTest
    {
        private INativeAPI m_nativeAPI;

        [SetUp]
        public void Setup()
        {
            m_nativeAPI = MockRepository.GenerateMock<INativeAPI>();
        }

        private ISdCardCeService ISdCardCeServiceConstructor(INativeAPI mockedNativeAPI)
        {
            return new SdCardCeService(ServiceContainerCF.GetServiceLazy<IMessageBoxServiceCF>(),
                  mockedNativeAPI,
                  ServiceContainerCF.GetServiceLazy<IScreenManager>(),
                  ServiceContainerCF.GetServiceLazy<IStorageService>(),
                  ServiceContainerCF.GetServiceLazy<ISplashService>(),
                  new FileHelperCF());
        }

        [TestCase(ulong.MaxValue)]
        [TestCase(ulong.MinValue)]
        public void GetAvailableDiskSpace(ulong availableMemoryOnSD)
        {
            var mockedDiskSpace = new FREEDISKSPACE();
            mockedDiskSpace.TotalNumberOfFreeBytes = availableMemoryOnSD;
            m_nativeAPI.Stub(x => x.GetDiskFreeSpaceEx(Arg<string>.Is.Anything, ref Arg<FREEDISKSPACE>.Ref(Rhino.Mocks.Constraints.Is.Anything(), mockedDiskSpace).Dummy)).Return(true);

            ulong result = ISdCardCeServiceConstructor(m_nativeAPI).GetAvailableMemory();
            Assert.IsTrue(result == availableMemoryOnSD);
        }

        [TestCase(FormatExternalStorageCardResult.NotPresent)]
        [TestCase(FormatExternalStorageCardResult.Ok)]
        public void IsExternalStorageCardPresent(FormatExternalStorageCardResult returnValue)
        {
            m_nativeAPI.Stub(x => x.IsExternalStorageCardPresent(Arg<string>.Is.Anything)).Return(returnValue);

            FormatExternalStorageCardResult result = ISdCardCeServiceConstructor(m_nativeAPI).IsExternalStorageCardPresent();
            Assert.IsTrue(result == returnValue);
        }

        [TestCase(ApplicationConstantsCF.StorageCardPathUnslashed)]
        [TestCase(ApplicationConstantsCF.FlashDriveProjectPath)]
        public void ShouldDbBeStoredOnSD(string databaseLocation)
        {
            var mockedSettings = MockRepository.GenerateMock<ISettings>();
            var mockedLocallyHostedProjectStorageProviderSettings = MockRepository.GenerateMock<ILocallyHostedStorageProviderSettings>();
            mockedLocallyHostedProjectStorageProviderSettings.Stub(x => x.RootDirectory).Return(databaseLocation);
            //Need to make a 'fake' object IEnumerable list to avoid null reference in the ProjectStorageProviderSettings constructor 
            IEnumerable<MockStorageProviderSetting> mockObjects = new List<MockStorageProviderSetting>() {new MockStorageProviderSetting()};

            mockedLocallyHostedProjectStorageProviderSettings.Stub(x => x.Settings).Return(mockObjects);
            IStorageProviderSettings storageProviderSettings = new LocallyHostedProjectStorageProviderSettings(mockedLocallyHostedProjectStorageProviderSettings);
            mockedSettings.Stub(x => x.ProjectSettings.StorageProviderSettings).Return(storageProviderSettings);

            bool result = ISdCardCeServiceConstructor(m_nativeAPI).ShouldDbBeStoredOnSD(mockedSettings);

            if (databaseLocation.Equals(ApplicationConstantsCF.StorageCardPathUnslashed))
                Assert.IsTrue(result);
            else
                Assert.IsFalse(result);
        }

        [Test]
        public void HasBlackListBeenUpdatedWithCorrectDatabaseExtension()
        {
#if DEBUG
            const string buildConfig = "OutPut\\Debug";
#else
            const string buildConfig = "Output\\Release";
#endif
            
            List<string> ignoreExtensions = BlacklistFileStream();
            Assert.IsFalse(ignoreExtensions.IsNullOrEmpty());
            
            AppDomain dom = AppDomain.CreateDomain("localAssemblyDomain");

            List<Assembly> types = new List<Assembly>();
            foreach (var file in Directory.GetFiles(Path.Combine(TestHelper.SolutionDirectory, buildConfig), "*.dll"))
            {
                try
                {
                    types.Add(dom.Load(file));
                }
                catch (Exception)
                { }
            }

            List<Assembly> exceptionCheckedTypes = new List<Assembly>();
            foreach (var type in types)
            {
                try
                {
                    type.GetTypes();
                }
                catch (Exception)
                {
                    continue;
                }
                exceptionCheckedTypes.Add(type);
            }

            AppDomain.Unload(dom);

            var filteredTypes = exceptionCheckedTypes.SelectMany(x => x.GetTypes()).Where(y => typeof(IStorageProvider).IsAssignableFrom(y) && !y.IsInterface);
            List<string> extensions = new List<string>();
            foreach (Type type in filteredTypes)
            {
                var instance = Activator.CreateInstance(type);
                string extension = "." + ((IStorageProvider)instance).FileExtension;
                if (!extensions.Contains(extension))
                    extensions.Add(extension);
            }

            foreach (var extension in extensions)
                Assert.IsTrue(ignoreExtensions.Contains(extension));
        }

        private List<string> BlacklistFileStream()
        {
            List<string> ignoreExtensions = new List<string>();
            string blackListPath = Path.Combine(PathTools.GetParentDir(AppDomain.CurrentDomain.BaseDirectory, 4).FullName, 
                @"Tools\ToolsIde\Configurations\BlacklistCommonFiles.xml"); 

            if (!File.Exists(blackListPath))
                return ignoreExtensions;

            using (FileStream fileStreamXml = File.OpenRead(blackListPath))
            {
                XmlReader xmlReader = XmlReader.Create(fileStreamXml);
                while (xmlReader.Read())
                {
                    if (xmlReader.Name == "IgnoreExtension")
                    {
                        ignoreExtensions.Add(xmlReader.ReadElementString().ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            return ignoreExtensions;
        }

        private class MockStorageProviderSetting : IStorageProviderSetting
        {
            private string m_Key;
            private object m_Value;

            public MockStorageProviderSetting()
            {
                m_Key = "dummy text";
                m_Value = new object();
            }

            string IStorageProviderSetting.Key => m_Key;

            object IStorageProviderSetting.Value { get => m_Value; set => new object(); }

            string IStorageProviderSetting.DisplayName => throw new NotImplementedException();

            string IStorageProviderSetting.ToolTip => throw new NotImplementedException();
        }
    }
}
