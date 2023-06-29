using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication.Forms
{
    [TestFixture]
    public class FormsAuthenticationTest
    {
        private Dictionary<string, string> m_Credentials;

        private FormsAuthentication m_FormsAuthentication;

        private ISession m_SessionMock;

        [SetUp]
        public void Setup()
        {
            m_Credentials  = new Dictionary<string, string>();
            m_FormsAuthentication = new FormsAuthentication(m_Credentials);

            m_SessionMock = Substitute.For<ISession>();
        }

        [Test]
        public void Should_fail_authentication_when_no_user_is_provided()
        {
            bool authenticationResult = m_FormsAuthentication.Login(null, null, null);

            Assert.That(authenticationResult, Is.False);
        }
        
        [Test]
        public void Should_fail_authentication_when_no_credentials_exists_for_user()
        {
            bool authenticationResult = m_FormsAuthentication.Login("mrfoo", null, null);

            Assert.That(authenticationResult, Is.False);
        }
        
        [Test]
        public void Should_fail_authentication_when_credentials_for_user_do_not_match()
        {
            m_Credentials.Add("mrfoo", "toomanyletterstoremember");

            bool authenticationResult = m_FormsAuthentication.Login("mrfoo", "bar", null);

            Assert.That(authenticationResult, Is.False);
        }

        [Test]
        public void Should_be_authenticated_when_credentials_match()
        {
            m_Credentials.Add("mrfoo", "bar");

            bool authenticationResult = m_FormsAuthentication.Login("mrfoo", "bar", m_SessionMock);

            Assert.That(authenticationResult, Is.True);
        }

        [Test]
        public void Should_store_authentication_flag_in_session_when_successful()
        {
            m_Credentials.Add("mrfoo", "bar");

            m_FormsAuthentication.Login("mrfoo", "bar", m_SessionMock);

            m_SessionMock.Received().PutValue("FormsAuthenticator.IsAuthenticated", true);
        }

        [Test]
        public void Should_add_logout_route_to_route_table_when_initilized()
        {
            RouteTable routeTable = new RouteTable();

            m_FormsAuthentication.Initialize(routeTable);
            RouteData logoutRoute = routeTable.Routes.FirstOrDefault(r => r.Path.Contains("logout"));

            Assert.That(logoutRoute, Is.Not.Null);
            Assert.That(logoutRoute.Path, Is.EqualTo("^/logout$"));
            Assert.That(logoutRoute.HandlerType, Is.EqualTo(typeof(LogoutHandler)));
            Assert.That(logoutRoute.AllowAnonymous, Is.True);
        }      
        
        [Test]
        public void Should_add_login_route_to_route_table_when_initilized()
        {
            RouteTable routeTable = new RouteTable();

            m_FormsAuthentication.Initialize(routeTable);
            RouteData logoutRoute = routeTable.Routes.FirstOrDefault(r => r.Path.Contains("login"));

            Assert.That(logoutRoute, Is.Not.Null);
            Assert.That(logoutRoute.Path, Is.EqualTo("^/login$"));
            Assert.That(logoutRoute.HandlerType, Is.EqualTo(typeof(LoginHandler)));
            Assert.That(logoutRoute.AllowAnonymous, Is.True);
        }

        [Test]
        public void Should_be_authenticated_when_session_has_no_authentication_flag()
        {
            var communicationContext = CommunicationContextFixture.Empty;
            communicationContext.Session.TryGetValue("FormsAuthenticator.IsAuthenticated", false).Returns(true);

            bool authResult = m_FormsAuthentication.IsAuthenticatedWithSideEffects(communicationContext);

            Assert.That(authResult, Is.True);
        }

        [Test]
        public void Should_add_location_header_to_login_with_redirect_when_user_is_not_authenticated()
        {
            var communicationContext = CommunicationContextFixture.AfterRouteResolve;
            communicationContext.Request.Path.Returns("/aPath");

            m_FormsAuthentication.IsAuthenticatedWithSideEffects(communicationContext);

            communicationContext.Response.Received().AddHeader("Location", "/login?continueTo=/aPath");
        }

        [Test]
        public void Should_respond_302_Found_when_authentication_fails_and_route_is_marked_redirect_to_login()
        {
            var communicationContext = CommunicationContextFixture.AfterRouteResolve;
            communicationContext.PipelineData.Route.RedirectToLogin = true;

            m_FormsAuthentication.IsAuthenticatedWithSideEffects(communicationContext);

            Assert.That(communicationContext.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(302));
        }

        [Test]
        public void Should_response_401_Not_Authorized_when_authentication_fails_and_route_is_not_marked_redirect_to_login()
        {
            var communicationContext = CommunicationContextFixture.AfterRouteResolve;
            communicationContext.PipelineData.Route.RedirectToLogin = false;

            m_FormsAuthentication.IsAuthenticatedWithSideEffects(communicationContext);

            Assert.That(communicationContext.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(401));
        }

        [Test]
        public void Should_delete_authorization_data_when_logging_out()
        {
            ISession session = Substitute.For<ISession>();
            
            m_FormsAuthentication.Logout(session);

            session.Received().DeleteValue("FormsAuthenticator.IsAuthenticated");
        }
    }
}
