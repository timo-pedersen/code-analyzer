using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Core.Api.Application;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.MultiLanguage;
using Neo.ApplicationFramework.Common.Security;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityManagerTest
    {
        private SecurityManager m_SecurityManager;
        private ISecurityManager m_ISecurityManager;
        private readonly byte[] m_PasswordHash = new byte[] { 123 };

        private readonly string m_StartupPath = Path.Combine(Path.GetTempPath(), typeof(SecurityManagerTest).Name);

        [SetUp]
        public void SetUp()
        {
            var coreApplication = TestHelper.CreateAndAddServiceMock<ICoreApplication>();
            coreApplication.Stub(inv => inv.StartupPath).Return(m_StartupPath);
            Directory.CreateDirectory(m_StartupPath);
            m_ISecurityManager = m_SecurityManager = new SecurityManager();

            //This user was previously added in the constructor but was moved to the LoadCompleted() method of the SecurityManagerRootDesigner class.
            //Adds him here to simulate runtime behaviour.
            ISecurityUser securityUser = m_ISecurityManager.AddUser("Administrator", m_PasswordHash);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(m_StartupPath, true);
        }


        #region "Security Group Tests"

        [Test]
        public void AddGroupWhenEmpty()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);

            Assert.IsTrue(m_ISecurityManager.AddGroup(securityGroup));
            Assert.AreEqual(1, m_ISecurityManager.Groups.Count);
        }

        [Test]
        public void AddGroupWhenNotEmpty()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);
            m_ISecurityManager.AddGroup(securityGroup);

            securityGroup = new SecurityGroup("Operators", SecurityGroups.Group_02);
            Assert.IsTrue(m_ISecurityManager.AddGroup(securityGroup));
            Assert.AreEqual(2, m_ISecurityManager.Groups.Count);
        }

        [Test]
        public void AddExistingGroup()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);
            m_ISecurityManager.AddGroup(securityGroup);

            securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);
            Assert.IsFalse(m_ISecurityManager.AddGroup(securityGroup));
            Assert.AreEqual(1, m_ISecurityManager.Groups.Count);
        }

        [Test]
        public void GroupsAddedInSortedOrder()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_03);
            m_ISecurityManager.AddGroup(securityGroup);

            securityGroup = new SecurityGroup("Operators", SecurityGroups.Group_01);
            m_ISecurityManager.AddGroup(securityGroup);

            securityGroup = new SecurityGroup("Service", SecurityGroups.Group_02);
            m_ISecurityManager.AddGroup(securityGroup);

            securityGroup = new SecurityGroup("Guests", SecurityGroups.Group_04);
            m_ISecurityManager.AddGroup(securityGroup);

            Assert.AreEqual(SecurityGroups.Group_01, m_ISecurityManager.Groups[0].Group);
            Assert.AreEqual(SecurityGroups.Group_02, m_ISecurityManager.Groups[1].Group);
            Assert.AreEqual(SecurityGroups.Group_03, m_ISecurityManager.Groups[2].Group);
            Assert.AreEqual(SecurityGroups.Group_04, m_ISecurityManager.Groups[3].Group);
        }

        [Test]
        public void RemoveExistingGroup()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);
            m_ISecurityManager.AddGroup(securityGroup);

            Assert.AreEqual(1, m_ISecurityManager.Groups.Count);
            Assert.AreEqual(true, m_ISecurityManager.RemoveGroup(securityGroup));
            Assert.AreEqual(0, m_ISecurityManager.Groups.Count);
        }

        [Test]
        public void RemoveNoneExistingGroup()
        {
            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_01);

            Assert.AreEqual(false, m_ISecurityManager.RemoveGroup(securityGroup));
            Assert.AreEqual(0, m_ISecurityManager.Groups.Count);
        }

        [Test]
        public void GetUsersFromGroup()
        {
            IToolManager toolManagerStub = TestHelper.AddServiceStub<IToolManager>();
            toolManagerStub.Stub(x => x.Runtime).Return(false);

            ISecurityGroup securityGroup = new SecurityGroup("Administrators", SecurityGroups.Group_03);
            m_ISecurityManager.AddGroup(securityGroup);

            ISecurityUser securityUser = m_ISecurityManager.AddUser("Nisse", m_PasswordHash);
            securityUser.AddToGroup(securityGroup.Group);

            ISecurityServiceCF securityServiceCF = new SecurityManagerToolCF();
            securityServiceCF.SecurityManager = m_SecurityManager;

            List<ISecurityUser> groupUsers = securityServiceCF.GetUsers(securityGroup);
            Assert.AreEqual(1, groupUsers.Count);
            Assert.AreEqual(securityUser, groupUsers[0]);
        }

        #endregion

        #region "Security User Tests"

        [Test]
        public void DefaultUserTest()
        {
            Assert.AreEqual(1, m_ISecurityManager.Users.Count);
        }

        [Test]
        public void AddUserWhenNotEmpty()
        {
            ISecurityUser securityUser = m_ISecurityManager.AddUser("Nisse", m_PasswordHash);
            Assert.IsNotNull(securityUser);
            Assert.AreEqual(2, m_ISecurityManager.Users.Count);
        }

        [Test]
        public void AddExistingUser()
        {
            ISecurityUser securityUser = m_ISecurityManager.AddUser("Administrator", m_PasswordHash);
            Assert.IsNull(securityUser);
            Assert.AreEqual(1, m_ISecurityManager.Users.Count);
        }

        [Test]
        public void RemoveExistingUser()
        {
            ISecurityUser securityUser = m_ISecurityManager.AddUser("Nisse", m_PasswordHash);

            Assert.AreEqual(2, m_ISecurityManager.Users.Count);
            Assert.AreEqual(true, m_ISecurityManager.RemoveUser(securityUser));
            Assert.AreEqual(1, m_ISecurityManager.Users.Count);
        }

        [Test]
        public void RemoveNoneExistingUser()
        {
            ISecurityUser securityUser = new SecurityUser("Nisse", m_PasswordHash);

            Assert.AreEqual(false, m_ISecurityManager.RemoveUser(securityUser));
            Assert.AreEqual(1, m_ISecurityManager.Users.Count);
        }

        #endregion

        [Test]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void AutomaticTimeoutValueGreaterThanZero(int newValue, bool changeExpected)
        {
            var messageBoxServiceStub = MockRepository.GenerateMock<IMessageBoxServiceCF>();
            messageBoxServiceStub.Stub(x => x.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(DialogResult.OK);
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceStub);

            var securityServiceStub = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityServiceStub.Stub(x => x.UserActive());

            int previousValue = m_ISecurityManager.AutomaticLogoutMinutes;
            m_ISecurityManager.AutomaticLogoutMinutes = newValue;

            if (changeExpected)
            {
                Assert.AreEqual(m_ISecurityManager.AutomaticLogoutMinutes, newValue);
                messageBoxServiceStub.AssertWasNotCalled(x => x.Show(TextsCF.SecurityLogoutTimeWrongInputValue.GetTranslation(), TextsCF.Error.GetTranslation()));
            }
            else
            {
                Assert.AreEqual(m_ISecurityManager.AutomaticLogoutMinutes, previousValue);
                messageBoxServiceStub.AssertWasCalled(x => x.Show(TextsCF.SecurityLogoutTimeWrongInputValue.GetTranslation(), TextsCF.Error.GetTranslation()));
            }
        }

        [Test]
        public void LoggingInUserMissingUserNameAndPasswordShowsLoginDialog()
        {
            //ARRANGE
            ILoginParams loginParams = new LoginParams();
            var m_mockSecurityServiceCF = TestHelper.AddServiceStub<ISecurityServiceCF>();

            //ACT
            m_SecurityManager.Login(loginParams);

            //ASSERT
            m_mockSecurityServiceCF.AssertWasCalled(s => s.ShowLoginDialog(loginParams));
            m_mockSecurityServiceCF.AssertWasNotCalled(s => s.LoginUser(loginParams.UserName, loginParams.Password, loginParams.ShowLoginSucceededDialog));
        }

        [Test]
        public void LoggingInUserWithUserNameAndPasswordDoesNotShowLoginDialog()
        {
            //ARRANGE
            ILoginParams loginParams = new LoginParams
            {
                UserName = "Peter",
                Password = "pwd123"
            };
            var m_mockSecurityServiceCF = TestHelper.AddServiceStub<ISecurityServiceCF>();

            //ACT
            m_SecurityManager.Login(loginParams);

            //ASSERT
            m_mockSecurityServiceCF.AssertWasNotCalled(s => s.ShowLoginDialog(loginParams));
            m_mockSecurityServiceCF.AssertWasCalled(s => s.LoginUser(loginParams.UserName, loginParams.Password, loginParams.ShowLoginSucceededDialog, loginParams.ShowLoginFailedDialog));
        }

    }
}
