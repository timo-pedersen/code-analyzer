using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class UserAuthenticationTest
    {
        private IAuthenticator m_AuthenticatorMock;

        private UserAuthentication m_UserAuthentication;

        [SetUp]
        public void Setup()
        {
            m_AuthenticatorMock = MockRepository.GenerateMock<IAuthenticator>();
            m_UserAuthentication = new UserAuthentication(m_AuthenticatorMock);
        }

        [Test]
        public void Should_continue_when_route_allow_anonymous()
        {
            ICommunicationContext context = CommunicationContextFixture.AfterRouteResolve;
            context.PipelineData.Route.AllowAnonymous = true;

            PipelineContinuation continuation = m_UserAuthentication.Process(context);

            Assert.That(continuation, Is.EqualTo(PipelineContinuation.Continue));
        }

        [Test]
        public void Should_skip_to_render_when_user_is_not_authenticated()
        {
            ICommunicationContext context = CommunicationContextFixture.AfterRouteResolve;
            m_AuthenticatorMock.Stub(m => m.IsAuthenticatedWithSideEffects(context)).Return(false);

            PipelineContinuation continuation = m_UserAuthentication.Process(context);

            Assert.That(continuation, Is.EqualTo(PipelineContinuation.RenderNow));
        }
        
        [Test]
        public void Should_continue_pipeline_when_user_is_authenticated()
        {
            ICommunicationContext context = CommunicationContextFixture.AfterRouteResolve;
            m_AuthenticatorMock.Stub(m => m.IsAuthenticatedWithSideEffects(context)).Return(true);

            PipelineContinuation continuation = m_UserAuthentication.Process(context);

            Assert.That(continuation, Is.EqualTo(PipelineContinuation.Continue));
        }


    }
}