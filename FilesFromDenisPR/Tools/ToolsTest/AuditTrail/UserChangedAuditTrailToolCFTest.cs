using System;
using System.Collections.Generic;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.AuditTrail
{
    [TestFixture]
    public class UserChangedAuditTrailToolCFTest
    {
        int m_NumberOfTimesCalled;
        private IAuditTrailService m_AuditTrailService;
        private IAuditTrail m_IAuditTrail;
        private ISecurityServiceCF m_SecurityService;
#pragma warning disable 169
        private UserChangedAuditTrailToolCF m_UserChangedAuditTrailToolCF;
#pragma warning restore 169

        [SetUp]
        public void SetUp()
        {
            m_NumberOfTimesCalled = 0;
            m_AuditTrailService = Substitute.For<IAuditTrailService>();
            m_IAuditTrail = Substitute.For<IAuditTrail>();
            m_AuditTrailService
                .When(x => x.LogDataItemChanged(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<VariantValue>(), Arg.Any<VariantValue>()))
                .Do(y => m_NumberOfTimesCalled++);
            m_AuditTrailService.AuditTrail.Returns(m_IAuditTrail);
            m_SecurityService = Substitute.For<ISecurityServiceCF>();
            m_SecurityService.CurrentUser.Returns("Kalle");
            m_UserChangedAuditTrailToolCF = new UserChangedAuditTrailToolCF(m_AuditTrailService.ToILazy(), m_SecurityService.ToILazy());
        }

        [Test]
        public void AuditTrailOnLogin()
        {
            // ARRANGE
            m_IAuditTrail.SuppressedLogActionNameList.Returns(new List<string>());
            m_AuditTrailService.LoggingEnabled.Returns(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            Raise.EventWith(m_IAuditTrail, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Login));

            // ASSERT
            Assert.AreEqual(1, m_NumberOfTimesCalled);
        }

        [Test]
        public void AuditTrailOnLogout()
        {
            // ARRANGE
            m_IAuditTrail.SuppressedLogActionNameList.Returns(new List<string>());
            m_AuditTrailService.LoggingEnabled.Returns(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            Raise.EventWith(new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(1, m_NumberOfTimesCalled);
        }

        [Test]
        public void NoAuditTrailOnLogin()
        {
            // ARRANGE
            m_IAuditTrail.SuppressedLogActionNameList.Returns(new List<string> { "Login" }); //User has unselected AuditTrail on Login action
            m_AuditTrailService.LoggingEnabled.Returns(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            Raise.EventWith(new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Login));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }

        [Test]
        public void NoAuditTrailOnLogout()
        {
            // ARRANGE
            m_IAuditTrail.SuppressedLogActionNameList.Returns(new List<string> { "Logout" }); //User has unselected AuditTrail on Logout action
            m_AuditTrailService.LoggingEnabled.Returns(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            Raise.EventWith(new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }

        [Test]
        public void AuditTrailLoggingDisabled()
        {
            // ARRANGE
            m_IAuditTrail.SuppressedLogActionNameList.Returns(new List<string>());
            m_AuditTrailService.LoggingEnabled.Returns(false);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            Raise.EventWith(new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }
    }
}
