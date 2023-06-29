using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class RequestDecoderTest
    {
        private ICommunicationContext m_CommunicationContextStub;

        private RequestDecoding m_RequestDecoding;

        private RouteTable m_RouteTable;

        private MediaTypeReaderFactory m_MediaTypeReaderFactoryMock;

        [SetUp]
        public void Setup()
        {
            m_CommunicationContextStub = CommunicationContextFixture.AfterOperationCandidateGeneration;

            m_RouteTable = new RouteTable();
            m_RouteTable.AddRoute(new RouteData());

            m_MediaTypeReaderFactoryMock = Substitute.For<MediaTypeReaderFactory>();

            m_RequestDecoding = new RequestDecoding(m_MediaTypeReaderFactoryMock);
        }

        [Test]
        public void Should_continue_when_content_length_is_zero()
        {
            m_CommunicationContextStub.Request.ContentLength.Returns(0);

            PipelineContinuation pipelineContinuation = m_RequestDecoding.Process(m_CommunicationContextStub);
            
            Assert.That(pipelineContinuation, Is.EqualTo(PipelineContinuation.Continue));
        }

        [Test]
        public void Should_use_media_type_reader_factory_to_create_a_read_for_the_content_type()
        {
            m_CommunicationContextStub.Request.ContentLength.Returns(1);
            m_CommunicationContextStub.Request.ContentType.Returns("application/json");
            m_CommunicationContextStub.PipelineData.Route.ResourceType = typeof(string);
            IOperation fooWithBarStringParameter = OperationFixture.FooWithUnsetBarStringParameter;
            m_CommunicationContextStub.PipelineData.CandidateOperations = new[] { fooWithBarStringParameter };

            m_RequestDecoding.Process(m_CommunicationContextStub);

            m_MediaTypeReaderFactoryMock.Received().Create("application/json");
        }

        [Test]
        public void Should_hydrate_object_using_media_type_reader()
        {
            m_CommunicationContextStub.Request.ContentLength.Returns(1);
            m_CommunicationContextStub.Request.ContentType.Returns("application/json");
            m_CommunicationContextStub.PipelineData.Route.ResourceType = typeof(string);
            IOperation fooWithBarStringParameter = OperationFixture.FooWithUnsetBarStringParameter;
            m_CommunicationContextStub.PipelineData.CandidateOperations = new[] { fooWithBarStringParameter };
            IMediaTypeReader mediaTypeReader = Substitute.For<IMediaTypeReader>();
            m_MediaTypeReaderFactoryMock.Create("application/json").Returns(mediaTypeReader);

            m_RequestDecoding.Process(m_CommunicationContextStub);

            mediaTypeReader.Received().ReadFrom(m_CommunicationContextStub.Request, typeof(string), "bar");
        }
    }
}