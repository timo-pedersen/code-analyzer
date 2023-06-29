using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.FileLogic
{
    [TestFixture]
    public class FilePathLogicCFTest
    {
        private IFilePathLogic m_FilePathLogicCF;

        private const string m_iXPath = @"C:\SomeFolder\iX\";
        private const string m_ProjectFilesPath = m_iXPath + @"Project Files\";

        [SetUp]
        public void SetUp()
        {
#if !VNEXT_TARGET
            m_FilePathLogicCF = new FilePathLogicCF();
#else
            m_FilePathLogicCF = new FilePathLogic();
#endif

            TestHelper.CreateAndAddServiceStub<IMessageBoxServiceCF>();

            ICoreApplication coreApp = TestHelper.CreateAndAddServiceStub<ICoreApplication>();
            coreApp.StartupPath.Returns(m_iXPath);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetFtpFriendlyPathForFile()
        {
            string result = m_FilePathLogicCF.GetTargetPathForFile(FileDirectory.ProjectFiles, "Data Logger 123.4", "csv", true, true, "DatabaseExports\\Data .Loggers.", true);

            Assert.IsFalse(Path.GetFileName(result).Contains(" "));
            Assert.AreEqual(m_ProjectFilesPath + @"DatabaseExports\Data_-Loggers-\Data_Logger_123-4.csv", result);
        }

        [Test]
        public void GetPathForFileWithoutFtpFriendlyNames()
        {
            string result = m_FilePathLogicCF.GetTargetPathForFile(FileDirectory.ProjectFiles, "Data Logger 123.4", "csv", true, true, "DatabaseExports\\Data .Loggers.", false);

            Assert.IsTrue(Path.GetFileName(result).Contains(" "));
            Assert.AreEqual(m_ProjectFilesPath + @"DatabaseExports\Data .Loggers.\Data Logger 123.4.csv", result);
        }

        [Test]
        public void GetFtpFriendlyPathForFileWithoutSubfolders()
        {
            string result = m_FilePathLogicCF.GetTargetPathForFile(FileDirectory.ProjectFiles, "Data Logger 123.4", "csv", true, false, "DatabaseExports\\Data .Loggers.", true);

            Assert.IsFalse(Path.GetFileName(result).Contains(" "));
            Assert.AreEqual(m_ProjectFilesPath + "Data_Logger_123-4.csv", result);
        }

        [Test]
        public void GetPathForFileWithoutFtpFriendlyNamesWithoutSubfolders()
        {
            string result = m_FilePathLogicCF.GetTargetPathForFile(FileDirectory.ProjectFiles, "Data Logger 123.4", "csv", true, false, "DatabaseExports\\Data .Loggers.", false);

            Assert.IsTrue(Path.GetFileName(result).Contains(" "));
            Assert.AreEqual(m_ProjectFilesPath + "Data Logger 123.4.csv", result);
        }

        [Test]
        public void GetFtpFriendlyPathForFileWithWrongFileDirectory()
        {
            string result = m_FilePathLogicCF.GetTargetPathForFile(FileDirectory.NotApplicable, "Data Logger 123.4", "csv", true, false, "DatabaseExports\\Data .Loggers.", false);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
