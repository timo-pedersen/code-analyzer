using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityManagerToolTest : SecurityManagerToolTestBase
    {
        private SecurityManagerToolIde m_SecurityManagerTool;
        private ISecurityService m_SecurityService;
        private ISecurityUser m_SecurityUser;
        private IEnumerable m_Controls;
        private AnalogNumeric m_AnalogNumeric;
        private SecurityVisibilityHelper m_SecurityVisibilityHelper;
        private ISecurityExtenderCF m_SecurityExtenderStub;
        private IGlobalDataItem m_GlobalDataItem;

        protected override SecurityManagerToolCF CreateSecurityManagerTool()
        {
            return new SecurityManagerToolIde();
        }
   
        [SetUp]
        public override void SetUp()
        {
            // A lot of mocks and stubs have already been setup for SecurityManagerToolCF so we'll use that here too
            base.SetUp();

            m_SecurityService = m_SecurityManagerTool = (SecurityManagerToolIde)m_SecurityManagerToolCF;

            m_AnalogNumeric = new AnalogNumeric();

            m_SecurityExtenderStub = MockRepository.GenerateStub<ISecurityExtenderCF>();
            m_SecurityExtenderStub.Stub(x => x.GetSecurityGroups(m_AnalogNumeric)).Return(SecurityGroups.Group_01);

            m_Controls = new object[] { m_AnalogNumeric };

            m_SecurityUser = MockRepository.GenerateStub<ISecurityUser>();

            var securityManager = MockRepository.GenerateStub<ISecurityManager>();
            m_SecurityVisibilityHelper = new SecurityVisibilityHelper(m_SecurityUser, securityManager);

            m_GlobalDataItem = MockRepository.GenerateStub<GlobalDataItem>();
          
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsHiddenKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.Stub(x => x.GetVisibilityMode(m_AnalogNumeric)).Return(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibility()
        {
            var form = MockRepository.GenerateStub<IForm>();
            var securityExtender = MockRepository.GenerateStub<ISecurityExtenderCF>();
            var securityUser = MockRepository.GenerateStub<ISecurityUser>();
            var securityManager = MockRepository.GenerateStub<ISecurityManager>();

            Control control = new Control();
            ArrayList controls = new ArrayList();
            controls.Add(control);
            form.Stub(x => x.Controls).Return(controls).Repeat.Any();

            securityExtender.Stub(x => x.GetSecurityGroups(control))
                            .Return(SecurityGroups.Group_01)
                            .Repeat.Any();

            securityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01))
                        .Return(false);

            SecurityVisibilityHelper securityVisibilityHelper = new SecurityVisibilityHelper(securityUser, securityManager);
            securityVisibilityHelper.UpdateControlsVisibility(form.Controls, securityExtender);

            Assert.IsTrue(control.Enabled);
        }


        [Test]
        public void ChangePasswordUsesPasswordComplexity()
        {
            var passwordComplexity = MockRepository.GenerateMock<PasswordComplexity>();
            passwordComplexity.Expect(x => x.AssertPasswordCriteria("pw"));

            m_SecurityManagerTool.PasswordComplexity = passwordComplexity;
            m_SecurityService.ChangePassword(UserName, "pw");

            passwordComplexity.VerifyAllExpectations();
        }

        [Test]
        public void ChangePasswordThrowsOnInvalidUser()
        {
            Assert.Throws<InvalidUsernameOrPasswordException>(() => m_SecurityService.ChangePassword("nonexistinguser", "pw"));
        }

        private Binding GetBinding(string propertyName)
        {
            return m_AnalogNumeric.DataBindings.OfType<Binding>().Where(currentBinding => currentBinding.PropertyName == propertyName).FirstOrDefault();
        }
    }
}
