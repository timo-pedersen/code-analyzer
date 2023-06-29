using System.ComponentModel;
using System.Windows.Forms;
using Core.Api.GlobalReference;
using Core.TestUtilities.Utilitites;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    public class SecurityManagerToolTestBase
    {
        protected const string UserName = "test";
        protected const string Password1 = "mycurrentpassword";
        protected const string Password2 = "anewpassword";
        protected ISecurityServiceCF m_ISecurityManagerToolCF;
        protected SecurityManagerToolCF m_SecurityManagerToolCF;
        protected BindingList<ISecurityGroup> m_GroupsList;
        protected BindingList<ISecurityUser> m_UsersList;
        protected ISecurityManager m_SecurityManagerStub;
        protected byte[] m_PasswordHash1 = new byte[] { 123 };
        protected byte[] m_PasswordHash2 = new byte[] { 222 };
        protected byte[] m_EmptyPasswordHash = new byte[] { 0 };
        protected SecurityUserSerializer m_SecurityUserSerializer;

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_ISecurityManagerToolCF = m_SecurityManagerToolCF = CreateSecurityManagerTool();

            var passwordHasher = Substitute.For<PasswordHasherCF>();
            passwordHasher.GetPasswordHash(Password1).Returns(m_PasswordHash1);

            passwordHasher.GetPasswordHash(Password2).Returns(m_PasswordHash2);

            passwordHasher.GetPasswordHash(string.Empty).Returns(m_EmptyPasswordHash);

            m_SecurityManagerToolCF.PasswordHasher = passwordHasher;

            FileHelperCF fileHelper = Substitute.For<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            var v = Substitute.For<IRunTimeXmlSerializerFactory>();

            m_SecurityUserSerializer = Substitute.For<SecurityUserSerializer>(v);
            m_SecurityManagerToolCF.SecurityUserSerializer = m_SecurityUserSerializer;

            SetPasswordComplexityStub();

            SetSecurityManagerMock();
        }

        protected virtual SecurityManagerToolCF CreateSecurityManagerTool()
        {
            var globalReferenceService = Substitute.For<IGlobalReferenceService>();
            globalReferenceService.GetObjects<Form>().Returns(new Form[0]);
            
            return new SecurityManagerToolCF(new LazyWrapper<IGlobalReferenceService>(() => globalReferenceService));
        }

        private void SetPasswordComplexityStub()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            passwordComplexity.MeetCriteria(Arg.Any<string>()).Returns(true);
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;
        }

        protected void SetSecurityManagerMock()
        {
            m_SecurityManagerStub = CreateSecurityManagerMockWithGroupsAndUsers();
            m_ISecurityManagerToolCF.SecurityManager = m_SecurityManagerStub;
        }

        private ISecurityManager CreateSecurityManagerMockWithGroupsAndUsers()
        {
            m_GroupsList = new BindingList<ISecurityGroup>();

            m_UsersList = new BindingList<ISecurityUser>()
            {
              new SecurityUser(UserName, m_PasswordHash1)
            };

            ISecurityManager securityManagerStub = Substitute.For<ISecurityManager>();
            securityManagerStub.Groups.Returns(m_GroupsList);
            securityManagerStub.Users.Returns(m_UsersList);
            return securityManagerStub;
        }
    }
}
