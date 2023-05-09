using System;
using System.IO;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.Website.Shared;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_FileHelper = MockRepository.GenerateMock<FileHelperCF>();

            m_FileHelper
                .Stub(m => m.Open(
                    Arg<string>.Is.Anything,
                    Arg<FileMode>.Is.Equal(FileMode.Open),
                    Arg<FileAccess>.Is.Equal(FileAccess.Read)))
                .Return(new MemoryStream());

            m_ResponseMock = MockRepository.GenerateMock<IResponse>();
            m_RequestMock = MockRepository.GenerateMock<IRequest>();

            DateTime lastWriteTime = new DateTime(2000, 1, 1, 0, 0, 0, 123);
            m_FileHelper.Stub(m => m.GetLastWriteTime(null)).IgnoreArguments().Return(lastWriteTime);

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
            
            m_RequestMock.Stub(m => m.GetHeader("If-Modified-Since")).IgnoreArguments().Return(lastModified);


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
