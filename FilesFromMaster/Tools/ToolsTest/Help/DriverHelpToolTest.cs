using System;
using System.IO;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Help.HelpGenerator.Common;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Help
{
    [TestFixture]
    public class DriverHelpToolTest
    {
        private IDriverHelpService m_DriverHelpService;
        private IApplicationSettings m_ApplicationSettings;
        private IControllerManagerServiceCF m_ControllerManagerServiceCf;
        private IDriverHelpGeneratorFactory m_DriverHelpGeneratorFactory;

        private readonly string m_HelpVersionDirectoryPath;
        private readonly string m_HelpVersionFilePath;
        private const string VersionTestString = "5.01.05";

        public DriverHelpToolTest()
        {
            m_HelpVersionDirectoryPath = Path.Combine(Path.GetTempPath(), "DriverHelpTest");
            m_HelpVersionFilePath = Path.Combine(m_HelpVersionDirectoryPath, "TestDriver.txt");
        }

        [SetUp]
        public void SetUp()
        {
            m_ApplicationSettings = MockRepository.GenerateMock<IApplicationSettings>();
            var folder = MockRepository.GenerateMock<IFolderInfo>();
            folder.Stub(f => f.AppDataFolder).Return(Path.GetTempPath());
            m_ApplicationSettings.Stub(inv => inv.FolderInfo).Return(folder);

            m_ControllerManagerServiceCf = MockRepository.GenerateMock<IControllerManagerServiceCF>();
            var controllerProtocol = MockRepository.GenerateMock<IControllerProtocol>();
            controllerProtocol.Stub(x => x.FileName).Return("Test");
            controllerProtocol.Stub(x => x.BrandName).Return("Test");
            m_ControllerManagerServiceCf.Stub(x => x.GetControllersProtocol(null)).IgnoreArguments().Return(controllerProtocol);

            m_DriverHelpGeneratorFactory = MockRepository.GenerateMock<IDriverHelpGeneratorFactory>();

            m_DriverHelpService = new DriverHelpService(m_ApplicationSettings, m_ControllerManagerServiceCf, m_DriverHelpGeneratorFactory);
            DeleteTestVersionFile();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestVersionFile();
        }

        [Test]
        public void HelpFileGeneratedWhenVersionFileIsMissing()
        {
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, VersionTestString);

            Assert.IsFalse(File.Exists(m_HelpVersionFilePath));
            Assert.IsFalse(returnValue);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("5.")]
        [TestCase("5.00.")]
        public void HelpFileGeneratedWhenVersionFileInfoIsCorrupt(string versionString)
        {
            CreateTestVersionFile(versionString);
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, VersionTestString);

            Assert.IsTrue(File.Exists(m_HelpVersionFilePath));
            Assert.IsFalse(returnValue);
        }

        [Test]
        public void HelpFileGeneratedWhenDriverVersionInfoIsCorrupt()
        {
            CreateTestVersionFile(VersionTestString);
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, string.Empty);

            Assert.IsTrue(File.Exists(m_HelpVersionFilePath));
            Assert.IsFalse(returnValue);
        }

        [Test]
        [TestCase("4.00.00")]
        [TestCase("5.00.00")]
        [TestCase("5.01.01")]
        public void HelpFileGeneratedWhenVersionFileInfoIsLower(string versionString)
        {
            CreateTestVersionFile(versionString);
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, VersionTestString);

            Assert.IsTrue(File.Exists(m_HelpVersionFilePath));
            Assert.IsFalse(returnValue);
        }

        [Test]
        [TestCase("6.00.00")]
        [TestCase("5.02.00")]
        [TestCase("5.01.09")]
        public void HelpFileGeneratedWhenVersionFileInfoIsHigher(string versionString)
        {
            CreateTestVersionFile(versionString);
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, VersionTestString);

            Assert.IsTrue(File.Exists(m_HelpVersionFilePath));
            Assert.IsFalse(returnValue);
        }

        [Test]
        public void HelpFileNotGeneratedWhenVersionFileInfoIsEqualToDriverVersion()
        {
            CreateTestVersionFile(VersionTestString);
            bool returnValue = m_DriverHelpService.UseCachedDriverHelp(m_HelpVersionFilePath, VersionTestString);

            Assert.IsTrue(File.Exists(m_HelpVersionFilePath));
            Assert.IsTrue(returnValue);
        }

        private void CreateTestVersionFile(string fileVersionString)
        {
            try
            {
                if (!Directory.Exists(m_HelpVersionDirectoryPath))
                    Directory.CreateDirectory(m_HelpVersionDirectoryPath);

                File.WriteAllText(m_HelpVersionFilePath, fileVersionString);
            }
            catch (Exception exception)
            {
                Assert.Fail("CreateTestVersionFile failed ({0})", exception);
            }
        }

        private void DeleteTestVersionFile()
        {
            try
            {
                if (File.Exists(m_HelpVersionFilePath))
                    File.Delete(m_HelpVersionFilePath);

                if (Directory.Exists(m_HelpVersionDirectoryPath))
                    Directory.Delete(m_HelpVersionDirectoryPath);
            }
            catch (Exception exception)
            {
                Assert.Fail("DeleteTestVersionFile failed ({0})", exception);
            }
        }
    }
}
