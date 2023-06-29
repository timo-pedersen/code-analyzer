using System;
using System.IO;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.Website.Shared;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.Website.File
{
    [TestFixture]
    public class EmbeddedFileReaderTest
    {
        private EmbeddedFileReader m_EmbeddedFileReader;

        private FileHelperCF m_FileHelper;

        private IRequest m_RequestMock;

        private IResponse m_ResponseMock;

        [SetUp]
        public void Setup()
        {
            m_FileHelper = Substitute.For<FileHelperCF>();

            m_FileHelper.Open(Arg.Any<string>(), Arg.Is(FileMode.Open), Arg.Is(FileAccess.Read))
                .Returns(new MemoryStream());

            m_ResponseMock = Substitute.For<IResponse>();
            m_RequestMock = Substitute.For<IRequest>();

            DateTime lastWriteTime = new DateTime(2000, 1, 1, 0, 0, 0, 123);
            m_FileHelper.GetLastWriteTime(Arg.Any<string>()).Returns(lastWriteTime);

            m_EmbeddedFileReader = new EmbeddedFileReader(m_RequestMock, m_ResponseMock, m_FileHelper, "Neo.ApplicationFramework.Tools.WebServer.Website.Assets.");
        }

        [Test]
        public void Should_return_not_found_when_no_embedded_file_mathing_filename_is_found()
        {
            OperationResult result = m_EmbeddedFileReader.Get("a_file_not_embedded.js") as OperationResult;

            Assert.That(result.HttpStatusCode, Is.EqualTo(404));
        }

        [Test]
        public void Should_return_not_modified_when_client_provides_last_modified_equal_to_last_write_time()
        {
            string lastModified = new DateTime(2000, 1, 1).ToString("G");
            
            m_RequestMock.GetHeader("If-Modified-Since").Returns(lastModified);


            OperationResult result = m_EmbeddedFileReader.Get("iX.js") as OperationResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HttpStatusCode, Is.EqualTo(304));
        }

        [Test]
        public void Should_return_file_resource_when_it_exists()
        {
            StreamResource result = m_EmbeddedFileReader.Get("iX.js") as StreamResource;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Stream, Is.Not.Null);
        }      
    }
}