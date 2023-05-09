using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.EncodingsList
{
    [TestFixture]
    public class EncodingsListManagerTest
    {
        private string m_TempFileName;
        private DictionarySerializer<string, int> m_Serializer;
        private IFileSettingsServiceIde m_FileSettings;
        private EncodingsListManager m_EncodingsListManager;

        [SetUp]
        public void TestSetUp()
        {
            m_TempFileName = Path.Combine(Path.GetTempPath(), EncodingConstants.EncodingsListFileName);
            m_Serializer = new DictionarySerializer<string, int>();
            m_FileSettings = Substitute.For<IFileSettingsServiceIde>();
            m_FileSettings.CommonApplicationDataFolder.Returns(".");
            m_EncodingsListManager = new EncodingsListManager(m_FileSettings, m_TempFileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(m_TempFileName))
                File.Delete(m_TempFileName);
        }

        [Test]
        public void FirstTimeEncodingsListManagerIsInstantiatedTheEncodingsListFileIsCreated()
        {
            //ASSERT
            Assert.That(File.Exists(m_TempFileName));
        }

        [Test]
        public void FirstItemInDefaultEncodingsListIsDefaultEncoding()
        {
            //ARRANGE
            var list = GetListFromFile();

            //ASSERT
            Assert.AreEqual(list.FirstOrDefault().Value, EncodingConstants.DefaultEncodingCodePage);
        }

        [Test]
        public void FileIsUpdatedCorrectlyWhenCallingMoveEncodingToTopOfList()
        {
            //ARRANGE
            var list = GetListFromFile();
            var random = new Random();
            int index = random.Next(list.Count);

            //ACT
            var randomEncoding = list[index];
            m_EncodingsListManager.UpdateEncodingsListIfNewEncodingIsChosen(randomEncoding.Value);
            var updatedList = GetListFromFile();

            //ASSERT
            Assert.AreEqual(updatedList.FirstOrDefault(), randomEncoding);
        }

        [Test]
        public void EncodingsListContainsAllSupportedEncoding()
        {
            //Info about encodings support in .NET can be found here:
            //https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding
            //Not all encodings supported in .NET Framework and .Net Core are supported in CE panels

            //ARRANGE
            var list = GetListFromFile();

            //ASSERT
            Assert.AreEqual(list.Count, 3);
        }

        private List<KeyValuePair<string, int>> GetListFromFile()
        {
           return m_Serializer.Deserialize(m_TempFileName).ToList();
        }
    }
}
