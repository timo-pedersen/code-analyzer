using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;
using NUnit.Framework;

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
            m_SessionManagerMock = Substitute.For<ISessionManager>();
            m_SessionResolver = new SessionResolver(m_SessionManagerMock);
        }

        [Test]
        public void Should_start_new_session_when_client_has_no_session_cookie()
        {
            m_SessionManagerMock.GetSessionAndRenewLease(Arg.Any<string>()).Returns(x => null);

            m_SessionResolver.Process(m_CommunicationContextStub);
            
            m_SessionManagerMock.Received().StartNewSession();
        }
        
        [Test]
        public void Should_start_new_session_when_client_has_no_valid_session_cookie()
        {
            string SID = "12345";
            StubSIDCookie(SID);
            m_SessionManagerMock.GetSessionAndRenewLease(Arg.Any<string>()).Returns(x => null);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_SessionManagerMock.Received().StartNewSession();
        }

        [Test]
        public void Should_create_a_session_cookie_when_new_session_is_created()
        {
            var sessionId = "123";
            ISession sessionStub = Substitute.For<ISession>();
            sessionStub.Id.Returns(sessionId);
            m_SessionManagerMock.StartNewSession().Returns(sessionStub);
            m_SessionManagerMock.GetSessionAndRenewLease(Arg.Any<string>()).Returns(x => null);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_CommunicationContextStub.Response.Received().AddHeader("Set-Cookie", Arg.Is<string>(x => x.StartsWith($"SID={sessionId}")));
        }

        [Test]
        public void Should_continue_pipeline_when_session_can_be_created()
        {
            ISession sessionStub = Substitute.For<ISession>();
            sessionStub.Id.Returns("123");
            m_SessionManagerMock.GetSessionAndRenewLease(Arg.Any<string>()).Returns(x => null);
            m_SessionManagerMock.StartNewSession().Returns(sessionStub);

            var pipelineContinuation = m_SessionResolver.Process(m_CommunicationContextStub);

            Assert.That(pipelineContinuation, Is.EqualTo(PipelineContinuation.Continue));
        }
        
        [Test]
        public void Should_ignore_malformed_cookie()
        {
            ISession sessionStub = Substitute.For<ISession>();
            m_CommunicationContextStub.Request.GetHeader("Cookie").Returns("SID=123;hasse=");
            m_SessionManagerMock.StartNewSession().Returns(sessionStub);
            m_SessionManagerMock.GetSessionAndRenewLease(Arg.Any<string>()).Returns(x => null);

            m_SessionResolver.Process(m_CommunicationContextStub);

            m_SessionManagerMock.Received().StartNewSession();
        }

        private void StubSIDCookie(string sid)
        {
            m_CommunicationContextStub.Request.GetHeader("Cookie").Returns(string.Format("SID={0};", sid));
        }
    }
}
