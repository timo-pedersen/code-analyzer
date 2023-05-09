using System;
using System.Collections.Generic;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_AuditTrailService = MockRepository.GenerateMock<IAuditTrailService>();
            m_IAuditTrail = MockRepository.GenerateMock<IAuditTrail>();
            m_AuditTrailService.Stub(x => x.LogDataItemChanged(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<VariantValue>.Is.Anything, Arg<VariantValue>.Is.Anything))
                .IgnoreArguments()
                .WhenCalled(
                    delegate
                    {
                        m_NumberOfTimesCalled++;
                    });
            m_AuditTrailService.Stub(x => x.AuditTrail).Return(m_IAuditTrail);
            m_SecurityService = MockRepository.GenerateStub<ISecurityServiceCF>();
            m_SecurityService.Stub(x => x.CurrentUser).Return("Kalle");
            m_UserChangedAuditTrailToolCF = new UserChangedAuditTrailToolCF(m_AuditTrailService.ToILazy(), m_SecurityService.ToILazy());
        }

        [Test]
        public void AuditTrailOnLogin()
        {
            // ARRANGE
            m_IAuditTrail.Stub(x => x.SuppressedLogActionNameList).Return(new List<string>());
            m_AuditTrailService.Stub(x => x.LoggingEnabled).Return(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            m_SecurityService.Raise(s => s.CurrentUserChanged += null, null, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Login));

            // ASSERT
            Assert.AreEqual(1, m_NumberOfTimesCalled);
        }

        [Test]
        public void AuditTrailOnLogout()
        {
            // ARRANGE
            m_IAuditTrail.Stub(x => x.SuppressedLogActionNameList).Return(new List<string>());
            m_AuditTrailService.Stub(x => x.LoggingEnabled).Return(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            m_SecurityService.Raise(s => s.CurrentUserChanged += null, null, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(1, m_NumberOfTimesCalled);
        }

        [Test]
        public void NoAuditTrailOnLogin()
        {
            // ARRANGE
            m_IAuditTrail.Stub(x => x.SuppressedLogActionNameList).Return(new List<string> { "Login" }); //User has unselected AuditTrail on Login action
            m_AuditTrailService.Stub(x => x.LoggingEnabled).Return(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            m_SecurityService.Raise(s => s.CurrentUserChanged += null, null, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Login));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }

        [Test]
        public void NoAuditTrailOnLogout()
        {
            // ARRANGE
            m_IAuditTrail.Stub(x => x.SuppressedLogActionNameList).Return(new List<string> { "Logout" }); //User has unselected AuditTrail on Logout action
            m_AuditTrailService.Stub(x => x.LoggingEnabled).Return(true);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            m_SecurityService.Raise(s => s.CurrentUserChanged += null, null, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }

        [Test]
        public void AuditTrailLoggingDisabled()
        {
            // ARRANGE
            m_IAuditTrail.Stub(x => x.SuppressedLogActionNameList).Return(new List<string>());
            m_AuditTrailService.Stub(x => x.LoggingEnabled).Return(false);

            // ACT
            ((ITool)m_UserChangedAuditTrailToolCF).Init();
            m_SecurityService.Raise(s => s.CurrentUserChanged += null, null, new CurrentUserChangedEventArgs("Adam", "Bertil", CurrentUserChangedEventArgs.UserChangedTypes.Logout));

            // ASSERT
            Assert.AreEqual(0, m_NumberOfTimesCalled);
        }
    }
}
