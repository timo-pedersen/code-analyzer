using System;
using System.IO;
using System.Linq;
using Core.Api.Application;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Brand;
using Neo.ApplicationFramework.Tools.Settings;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.FileUtilities
{
    [TestFixture]
    public class FileSettingsToolTest
    {
        private const string STR_Testxml = "TempTest.xml";
        private const string STR_TestxmlCopy = "TempTest.org.xml";

        private bool m_RemoveDirectoryAfterTest;

        private string m_Company, m_Product;
        private readonly string m_StartupPath = Path.Combine(Path.GetTempPath(), typeof(FileSettingsToolTest).Name);

        [SetUp]
        public void Setup()
        {
            ICoreApplication coreApplication = Substitute.For<ICoreApplication>();
            
            Directory.CreateDirectory(m_StartupPath);
            coreApplication.StartupPath.Returns(m_StartupPath);
            TestHelper.AddService<ICoreApplication>(coreApplication);
            IBrandService brandService = new BrandTool();
            TestHelper.AddService(brandService);

            m_Company = brandService.CompanyName;
            m_Product = brandService.ProductFamilyName;


            var applicationService = Substitute.For<IApplicationSettings>();
            var folder = Substitute.For<IFolderInfo>();
            folder.AppDataFolder.Returns(AppDataFolder);
            folder.GetAppDataFiles(string.Empty).Returns(Enumerable.Empty<FileInfo>().ToArray());
            applicationService.FolderInfo.Returns(folder);



            IFileSettingsService fileSettingsService = new FileSettingsTool(applicationService.ToILazy());
            TestHelper.AddService(fileSettingsService);
            m_RemoveDirectoryAfterTest = !Directory.Exists(AppDataFolder);

            CreateDefaultFile();

        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(DefaultFileName))
                File.Delete(DefaultFileName);
            if (File.Exists(UserFileName))
                File.Delete(UserFileName);
            if (File.Exists(DefaultFileCopyName))
                File.Delete(DefaultFileCopyName);
            // Other tests/previous runs could have written to the directory
            if(m_RemoveDirectoryAfterTest)
                Directory.Delete(AppDataFolder);

            Directory.Delete(m_StartupPath, true);
            TestHelper.ClearServices();
        }
        private string UserFileName
        {
            get
            {
                return String.Format(@"{0}\{1}", AppDataFolder, STR_Testxml);
            }
        }
        private string AppDataFolder
        {
            get
            {
                
                return string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), m_Company, m_Product);
            }
        }
        private string DefaultFileCopyName
        {
            get
            {
                return String.Format(@"{0}\{1}", AppDataFolder, STR_TestxmlCopy);
            }
        }
        [Test]
        public void GetUsefullFileName()
        {
            string fileName = ServiceContainerCF.GetService<IFileSettingsService>().FileName(STR_Testxml);
            Assert.AreEqual(UserFileName, fileName);
        }

        private string DefaultFileName
        {
            get
            {
                return NeoApplication.StartupPath + @"\" + STR_Testxml;
            }
        }
        private void CreateDefaultFile()
        {
            using (StreamWriter streamWriter = File.CreateText(DefaultFileName))
            {
                streamWriter.Write("Nisse");
            }
        }
        [Test]
        public void IfNewFileInProgramFolderGetEqualFileFromAppData()
        {
            string fileName = ServiceContainerCF.GetService<IFileSettingsService>().FileName(STR_Testxml);
            Assert.IsTrue(FileComparer.Compare(DefaultFileName, fileName));
        }

        [Test]
        public void OldUserFileShouldBeOverWritten()
        {
            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);
            //Create userfile to overwrite            
            using (StreamWriter streamWriter = File.CreateText(UserFileName))
            {
                streamWriter.Write("Old content");
            }
            File.SetLastWriteTime(UserFileName, DateTime.Now - TimeSpan.FromDays(1));
            File.Copy(UserFileName, DefaultFileCopyName);
            string fileName = ServiceContainerCF.GetService<IFileSettingsService>().FileName(STR_Testxml);
            Assert.IsTrue(FileComparer.Compare(DefaultFileName, fileName));
        }

        [Test]
        public void userFileShouldNotBeOverWritten()
        {
            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);
            //Create userfile
            File.Copy(DefaultFileName, DefaultFileCopyName);
            using (StreamWriter streamWriter = File.CreateText(UserFileName))
            {
                streamWriter.Write("New content");
            }
            string fileName = ServiceContainerCF.GetService<IFileSettingsService>().FileName(STR_Testxml);
            Assert.IsFalse(FileComparer.Compare(DefaultFileName, fileName));
        }

        [Test]
        public void NormalUse()
        {
            string fileName = ServiceContainerCF.GetService<IFileSettingsService>().FileName(STR_Testxml);
            Assert.IsTrue(FileComparer.Compare(DefaultFileName, fileName));
            using (StreamWriter streamWriter = File.CreateText(fileName))
            {
                streamWriter.Write("New content");
            }
            Assert.IsFalse(FileComparer.Compare(DefaultFileName, fileName));
        }
    }
}
