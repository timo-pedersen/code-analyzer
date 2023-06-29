using System;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication
{
    [TestFixture]
    public class SessionManagerTest
    {
        private readonly TimeSpan m_YearsFromNow = TimeSpan.FromHours(100);

        private IWebServerConfigService SetupWebServerConfigService(int numberOfSessions)
        {
            var webServerConfigService = Substitute.For<IWebServerConfigService>();
            webServerConfigService.MaxSessions.Returns(numberOfSessions);
            return webServerConfigService;
        }

        [Test]
        public void Should_be_able_to_retrieve_a_new_session()
        {
            var service = SetupWebServerConfigService(1);
            SessionManager sessionManager = new SessionManager(service, m_YearsFromNow);

            ISession newSession = sessionManager.StartNewSession();
            ISession retrievedSession = sessionManager.GetSessionAndRenewLease(newSession.Id);

            Assert.That(newSession, Is.SameAs(retrievedSession));
        }
        
        [Test]
        public void Should_not_retrieve_a_session_when_it_has_not_been_created()
        {
            var service = SetupWebServerConfigService(1);
            SessionManager sessionManager = new SessionManager(service, m_YearsFromNow);

            ISession retrievedSession = sessionManager.GetSessionAndRenewLease("evil_session_id");

            Assert.That(retrievedSession, Is.Null);
        }
        
        [Test]
        public void Should_not_retrieve_a_session_after_it_has_expired()
        {
            var service = SetupWebServerConfigService(1);
            // We use a negative session timeout to make the session expired instantly
            SessionManager sessionManager = new SessionManager(service, TimeSpan.FromSeconds(-100));
            
            ISession newSession = sessionManager.StartNewSession();
            ISession retrievedSession = sessionManager.GetSessionAndRenewLease(newSession.Id);

            Assert.That(retrievedSession, Is.Null);
        }
        
        [Test]
        public void Should_generate_a_unique_id_for_each_new_session()
        {
            var service = SetupWebServerConfigService(10);
            SessionManager sessionManager = new SessionManager(service, m_YearsFromNow);
            
            ISession newSession1 = sessionManager.StartNewSession();
            ISession newSession2 = sessionManager.StartNewSession();
            ISession newSession3 = sessionManager.StartNewSession();
            
            Assert.That(newSession1.Id, Is.Not.EqualTo(newSession2.Id));
            Assert.That(newSession1.Id, Is.Not.EqualTo(newSession3.Id));
            Assert.That(newSession2.Id, Is.Not.EqualTo(newSession1.Id));
            Assert.That(newSession2.Id, Is.Not.EqualTo(newSession3.Id));
        }

        [Test]
        public void Should_not_start_new_session_when_max_concurrent_sessions_has_been_reached()
        {
            var service = SetupWebServerConfigService(1);
            SessionManager sessionManager = new SessionManager(service, m_YearsFromNow);

            ISession newSession1 = sessionManager.StartNewSession();
            ISession newSession2 = sessionManager.StartNewSession();

            Assert.That(newSession1, Is.Not.Null);
            Assert.That(newSession2, Is.Null);
        }
    }
}
