using System.Collections.Generic;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication.Forms
{
    [TestFixture]
    public class LogoutHandlerTest
    {
        private LogoutHandler m_LogoutHandler;

        private ICommunicationContext m_Context;

        private FormsAuthentication m_FormsAuthentication;

        [SetUp]
        public void Setup()
        {
            m_FormsAuthentication = Substitute.For<FormsAuthentication>(new Dictionary<string, string>());
        
            m_Context = CommunicationContextFixture.AfterOperationCandidateGeneration;
            m_LogoutHandler = new LogoutHandler(m_Context.Session, m_FormsAuthentication);
        }

        [Test]
        public void Should_return_200_OK_when_logged_out()
        {
            object post = m_LogoutHandler.Get();

            Assert.That(post, Is.TypeOf<OperationResult>());
            Assert.That(((OperationResult)post).HttpStatusCode, Is.EqualTo(200));
        }

        [Test]
        public void Should_logout_when_logging_out()
        {
            m_LogoutHandler.Get();

            m_FormsAuthentication.Received().Logout(m_Context.Session);
        }
    }
}