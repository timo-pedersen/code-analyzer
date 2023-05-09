using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class ResponseCoderTest
    {
        private ResponseCoder m_ResponseCoder;

        private ICommunicationContext m_CommunicationContextStub;

        private IRequest m_RequestStub;

        private PipelineData m_PipelineData;

        private RouteTable m_RouteTable;

        private IResponse m_ResponseMock;

        private MediaWriterSelector m_MediaWriterSelector;

        [SetUp]
        public void Setup()
        {
            m_RouteTable = new RouteTable();
            
            m_CommunicationContextStub = Substitute.For<ICommunicationContext>();
            m_RequestStub = Substitute.For<IRequest>();
            m_ResponseMock = Substitute.For<IResponse>();
            m_PipelineData = new PipelineData();

            m_CommunicationContextStub = Substitute.For<ICommunicationContext>();
            m_CommunicationContextStub.Request = m_RequestStub;
            m_CommunicationContextStub.Response = m_ResponseMock;
            m_CommunicationContextStub.PipelineData = m_PipelineData;

            m_RouteTable.AddRoute(new RouteData());

            m_MediaWriterSelector = Substitute.For<MediaWriterSelector>();
            IMediaTypeWriter mediaTypeWriter = Substitute.For<IMediaTypeWriter>();
            m_MediaWriterSelector.GetBestMatchingMediaWriter(Arg.Any<System.Type>()).Returns(mediaTypeWriter);

            m_ResponseCoder = new ResponseCoder(m_MediaWriterSelector);
        }

        [Test]
        public void Should_continue_pipeline_when_no_operation_result_is_available()
        {
            m_PipelineData.OperationResult = null;

            PipelineContinuation pipelineContinuation = m_ResponseCoder.Process(m_CommunicationContextStub);

            Assert.That(pipelineContinuation, Is.EqualTo(PipelineContinuation.Continue));
        }
        
        [Test]
        public void Should_continue_pipeline_when_operation_result_is_empty()
        {
            m_PipelineData.OperationResult = OperationResult.Ok(null);

            PipelineContinuation pipelineContinuation = m_ResponseCoder.Process(m_CommunicationContextStub);

            Assert.That(pipelineContinuation, Is.EqualTo(PipelineContinuation.Continue));
        }
        
        [Test]
        public void Should_set_http_status_to_status_of_operation_result()
        {
            m_PipelineData.OperationResult = OperationResult.NotFound();

            m_ResponseCoder.Process(m_CommunicationContextStub);

            m_ResponseMock.Received().StatusCode = 404;
        }

        [Test]
        public void Should_respond_with_not_acceptable_if_no_media_writer_can_be_found_for_operation_result()
        {
            m_PipelineData.OperationResult = OperationResult.Ok(new object());
            m_MediaWriterSelector
                .GetBestMatchingMediaWriter(Arg.Any<string>(), Arg.Any<System.Type>(), Arg.Any<System.Collections.Generic.IEnumerable<IMediaTypeWriter>>())
                .Returns(x => null);

            m_ResponseCoder.Process(m_CommunicationContextStub);

            m_ResponseMock.Received().StatusCode = 406;
        }
    }
}