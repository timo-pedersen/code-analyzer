using System.ComponentModel;
using System.Windows.Forms;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    public class SecurityManagerToolTestBase
    {
        protected const string UserName = "test";
        protected const string Password1 = "mycurrentpassword";
        protected const string Password2 = "anewpassword";
        protected MockRepository m_Mocks;
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

            m_Mocks = new MockRepository();

            m_ISecurityManagerToolCF = m_SecurityManagerToolCF = CreateSecurityManagerTool();

            var passwordHasher = MockRepository.GenerateStub<PasswordHasherCF>();
            passwordHasher.Expect(mock => mock.GetPasswordHash(Password1))
                          .Return(m_PasswordHash1);

            passwordHasher.Expect(mock => mock.GetPasswordHash(Password2))
                          .Return(m_PasswordHash2);

            passwordHasher.Expect(mock => mock.GetPasswordHash(string.Empty))
                          .Return(m_EmptyPasswordHash);

            m_SecurityManagerToolCF.PasswordHasher = passwordHasher;

            FileHelperCF fileHelper = MockRepository.GenerateStub<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            var v = MockRepository.GenerateStub<IRunTimeXmlSerializerFactory>();

            m_SecurityUserSerializer = MockRepository.GenerateMock<SecurityUserSerializer>(v);
            m_SecurityManagerToolCF.SecurityUserSerializer = m_SecurityUserSerializer;

            SetPasswordComplexityStub();

            SetSecurityManagerMock();
        }

        protected virtual SecurityManagerToolCF CreateSecurityManagerTool()
        {
            var globalReferenceService = MockRepository.GenerateStub<IGlobalReferenceService>();
            globalReferenceService.Stub(service => service.GetObjects<Form>()).Return(new Form[0]);
            
            return new SecurityManagerToolCF(new LazyWrapper<IGlobalReferenceService>(() => globalReferenceService));
        }

        private void SetPasswordComplexityStub()
        {
            var passwordComplexity = MockRepository.GenerateStub<PasswordComplexity>();
            passwordComplexity.Stub(x => x.MeetCriteria(null)).IgnoreArguments().Return(true);
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

            ISecurityManager securityManagerStub = MockRepository.GenerateStub<ISecurityManager>();
            securityManagerStub.Stub(x => x.Groups).Return(m_GroupsList).Repeat.Any();
            securityManagerStub.Stub(x => x.Users).Return(m_UsersList).Repeat.Any();
            return securityManagerStub;
        }
    }
}
