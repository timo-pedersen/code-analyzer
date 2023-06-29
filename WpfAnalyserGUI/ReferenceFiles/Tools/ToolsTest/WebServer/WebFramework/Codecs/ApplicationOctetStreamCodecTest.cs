using System;
using System.IO;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs
{
    [TestFixture]
    public class ApplicationOctetStreamCodecTest
    {
        private ApplicationOctetStreamWriter _mWriter;

        private IResponse m_ResponseMock;

        private FileHelperCF m_FileHelperMock;

        [SetUp]
        public void Setup()
        {
            m_ResponseMock = Substitute.For<IResponse>();
            m_FileHelperMock = Substitute.For<FileHelperCF>();
            
            _mWriter = new ApplicationOctetStreamWriter(m_FileHelperMock);
        }

        [Test]
        public void Should_set_content_type_based_on_file_extensions()
        {
            FileResource file = new FileResource("image.png");

            _mWriter.WriteTo(file, m_ResponseMock);

            AssertContentType("image/png");
        }
        
        [Test]
        public void Should_set_content_type_to_octet_stream_for_unknown_file_extensions()
        {
            FileResource file = new FileResource("image.foobar");

            _mWriter.WriteTo(file, m_ResponseMock);

            AssertContentType("application/octet-stream");
        }
        
        [Test]
        public void Should_set_content_type_to_octet_stream_for_files_with_no_extension()
        {
            FileResource file = new FileResource("image");

            _mWriter.WriteTo(file, m_ResponseMock);

            AssertContentType("application/octet-stream");
        }

        [Test]
        public void Should_set_content_length_to_length_of_the_file()
        {
            FileResource file = new FileResource("image");
            MockFileSize(24);

            _mWriter.WriteTo(file, m_ResponseMock);

            AssertHeader("Content-Length", "24");
        }

        [Test]
        public void Should_open_streams_readonly()
        {
            FileResource file = new FileResource("image.jpg");
            
            _mWriter.WriteTo(file, m_ResponseMock);

            m_FileHelperMock.Received().Open("image.jpg", FileMode.Open, FileAccess.Read);
        }
        
        [Test]
        public void Should_set_response_to_file_stream()
        {
            FileResource file = new FileResource("image.jpg");
            MemoryStream fileStream = new MemoryStream();
            m_FileHelperMock.Open("image.jpg", FileMode.Open, FileAccess.Read).Returns(fileStream);

            _mWriter.WriteTo(file, m_ResponseMock);

            m_ResponseMock.Received().StreamContent = fileStream;
        }
        
        [Test]
        public void Should_only_allow_file_resource_entities()
        {
            Assert.Throws<ArgumentException>(() => _mWriter.WriteTo(new object(), m_ResponseMock));
        }       
        
        [Test]
        public void Should_set_response_to_stream_resources()
        {
            MemoryStream memstream = new MemoryStream();
            StreamResource stream = new StreamResource(memstream, "");
            
            _mWriter.WriteTo(stream, m_ResponseMock);

            m_ResponseMock.Received().StreamContent = memstream;
        }

        private void MockFileSize(int fileSize)
        {
            m_FileHelperMock.GetFileSize(Arg.Any<string>()).Returns(fileSize);
        }

        private void AssertContentType(string expectedContentType)
        {
            AssertHeader("Content-Type", expectedContentType);
        }
        
        private void AssertHeader(string headerName, string headerValue)
        {
            m_ResponseMock.Received().AddHeader(Arg.Is(headerName), Arg.Is(headerValue));
        }
    }
}
