using System;
using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.Website.Shared
{
    [TestFixture]
    public class FileReaderTest
    {
        private FileReader m_FileReader;

        private FileHelperCF m_FileHelper;

        private IRequest m_RequestMock;

        private IResponse m_ResponseMock;

        private readonly string m_StartupPath = Path.Combine(Path.GetTempPath(), typeof(FileReaderTest).Name);

        [SetUp]
        public void Setup()
        {
            var coreApplication = TestHelper.CreateAndAddServiceStub<ICoreApplication>();
            coreApplication.StartupPath.Returns(m_StartupPath);
            Directory.CreateDirectory(m_StartupPath);

            m_FileHelper = Substitute.For<FileHelperCF>();

            m_FileHelper.Open(Arg.Any<string>(), Arg.Is(FileMode.Open), Arg.Is(FileAccess.Read))
                .Returns(new MemoryStream());

            m_ResponseMock = Substitute.For<IResponse>();
            m_RequestMock = Substitute.For<IRequest>();

            m_FileHelper.Exists(Arg.Any<string>()).Returns(true);

            m_FileReader = new FileReader(m_FileHelper, m_RequestMock, m_ResponseMock);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(m_StartupPath, true);
        }


        [Test]
        public void Should_return_file_not_found_when_requested_file_escapes_project_file_folder()
        {
            OperationResult operationResult = m_FileReader.Get("../index.html") as OperationResult;

            Assert.That(operationResult.HttpStatusCode, Is.EqualTo(404));
        }

        [Test]
        public void Should_return_file_not_found_when_requested_file_does_not_exist()
        {
            m_FileHelper.Exists(Arg.Any<string>()).Returns(false);

            OperationResult operationResult = m_FileReader.Get("index.html") as OperationResult;

            Assert.That(operationResult.HttpStatusCode, Is.EqualTo(404));
            m_FileHelper.Received().Exists(Arg.Any<string>());
        }

        [Test]
        public void Should_return_absolute_file_path()
        {
            m_RequestMock.GetHeader(Arg.Any<string>()).Returns(x => null);
            m_FileHelper.GetLastWriteTime(Arg.Any<string>()).Returns(DateTime.Today);

            string expectedPath = Path.Combine(
                NeoApplication.StartupPath,
                DirectoryConstants.ProjectFilesFolderName,
                DirectoryConstants.WebSiteFolderName,
                "index.html");

            FileResource file = m_FileReader.Get("index.html") as FileResource;

            Assert.That(file.FilePath, Is.EqualTo(expectedPath));
        }

        [Test]
        public void Should_respond_not_modified_when_file_modification_date_is_before_modified_since_header()
        {
            m_RequestMock.GetHeader("If-Modified-Since").Returns("2000-01-01 01:01:01");
            DateTime fileModificationDate = new DateTime(2000, 1, 1, 1, 1, 1);
            m_FileHelper.GetLastWriteTime(Arg.Any<string>()).Returns(fileModificationDate);

            OperationResult result = m_FileReader.Get("file.txt") as OperationResult;

            Assert.That(result.HttpStatusCode, Is.EqualTo(304));
        }
    }
}