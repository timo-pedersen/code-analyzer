using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using Rhino.Mocks;

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
            
            m_CommunicationContextStub = MockRepository.GenerateStub<ICommunicationContext>();
            m_RequestStub = MockRepository.GenerateStub<IRequest>();
            m_ResponseMock = MockRepository.GenerateMock<IResponse>();
            m_PipelineData = new PipelineData();

            m_CommunicationContextStub = MockRepository.GenerateStub<ICommunicationContext>();
            m_CommunicationContextStub.Request = m_RequestStub;
            m_CommunicationContextStub.Response = m_ResponseMock;
            m_CommunicationContextStub.PipelineData = m_PipelineData;

            m_RouteTable.AddRoute(new RouteData());

            m_MediaWriterSelector = MockRepository.GenerateStub<MediaWriterSelector>();
            IMediaTypeWriter mediaTypeWriter = MockRepository.GenerateMock<IMediaTypeWriter>();
            m_MediaWriterSelector.Stub(m => m.GetBestMatchingMediaWriter(null)).IgnoreArguments().Return(mediaTypeWriter);

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

            m_ResponseMock.AssertWasCalled(m => m.StatusCode = 404);
        }

        [Test]
        public void Should_respond_with_not_acceptable_if_no_media_writer_can_be_found_for_operation_result()
        {
            m_PipelineData.OperationResult = OperationResult.Ok(new object());
            m_MediaWriterSelector.Stub(m => m.GetBestMatchingMediaWriter(null, null, null)).IgnoreArguments().Return(null);

            m_ResponseCoder.Process(m_CommunicationContextStub);

            m_ResponseMock.AssertWasCalled(m => m.StatusCode = 406);
        }
    }
}