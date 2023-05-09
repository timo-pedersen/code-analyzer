using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class SessionResolverTest
    {
        private ICommunicationContext m_CommunicationContextStub;

        private SessionResolver m_SessionResolver;

        private ISessionManager m_SessionManagerMock;

        [SetUp]
        public void Setup()
        {
            m_CommunicationContextStub = CommunicationContextFixture.Empty;
            m_SessionManagerMock = MockRepository.GenerateMock<ISessionManager>();
            m_SessionResolver = new SessionResolver(m_SessionManagerMock);
        }

        [Test]
        public void Should_start_new_session_when_client_has_no_session_cookie()
        {
            m_SessionResolver.Process(m_CommunicationContextStub);
            
            m_SessionManagerMock.AssertWasCalled(m => m.StartNewSession());
        }
        
        [Test]
        public void Should_start_new_session_when_client_has_no_valid_session_cookie()
        {
            string SID = "12345";
            StubSIDCookie(SID);
            m_SessionManagerMock.Stub(m => m.GetSessionAndRenewLease(SID)).Return(null);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_SessionManagerMock.AssertWasCalled(m => m.StartNewSession());
        }

        [Test]
        public void Should_create_a_session_cookie_when_new_session_is_created()
        {
            ISession sessionStub = MockRepository.GenerateStub<ISession>();
            sessionStub.Stub(s => s.Id).Return("123");
            m_SessionManagerMock.Stub(m => m.StartNewSession()).Return(sessionStub);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_CommunicationContextStub.Response.AssertWasCalled(m => m.AddHeader(Arg.Is("Set-Cookie"), Arg.Text.StartsWith("SID=123")));
        }

        [Test]
        public void Should_continue_pipeline_when_session_can_be_created()
        {
            ISession sessionStub = MockRepository.GenerateStub<ISession>();
            sessionStub.Stub(s => s.Id).Return("123");
            m_SessionManagerMock.Stub(m => m.StartNewSession()).Return(sessionStub);

            var pipelineContinuation = m_SessionResolver.Process(m_CommunicationContextStub);

            Assert.That(pipelineContinuation, Is.EqualTo(PipelineContinuation.Continue));
        }
        
        [Test]
        public void Should_ignore_malformed_cookie()
        {
            ISession sessionStub = MockRepository.GenerateStub<ISession>();
            m_CommunicationContextStub.Request.Stub(m => m.GetHeader("Cookie")).Return("SID=123;hasse=");
            m_SessionManagerMock.Stub(m => m.StartNewSession()).Return(sessionStub);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_SessionManagerMock.AssertWasCalled(m => m.StartNewSession());
        }

        private void StubSIDCookie(string sid)
        {
            m_CommunicationContextStub.Request.Stub(m => m.GetHeader("Cookie")).Return(string.Format("SID={0};", sid));
        }
    }
}
