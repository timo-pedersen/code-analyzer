#if !VNEXT_TARGET
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient;
using NSubstitute;
using NUnit.Framework;

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

            m_SecurityExtenderStub = Substitute.For<ISecurityExtenderCF>();
            m_SecurityExtenderStub.GetSecurityGroups(m_AnalogNumeric).Returns(SecurityGroups.Group_01);

            m_Controls = new object[] { m_AnalogNumeric };

            m_SecurityUser = Substitute.For<ISecurityUser>();

            var securityManager = Substitute.For<ISecurityManager>();
            m_SecurityVisibilityHelper = new SecurityVisibilityHelper(m_SecurityUser, securityManager);

            m_GlobalDataItem = Substitute.For<GlobalDataItem>();
          
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Disabled);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Enabled", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Enabled"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(false);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsHiddenKeepsVisibilityBinding()
        {
            m_SecurityExtenderStub.GetVisibilityMode(m_AnalogNumeric).Returns(VisibilityModes.Hidden);

            m_AnalogNumeric.DataBindings.Add("Visible", m_GlobalDataItem, "Value", true);

            m_SecurityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(true);

            m_SecurityVisibilityHelper.UpdateControlsVisibility(m_Controls, m_SecurityExtenderStub);

            Assert.That(GetBinding("Visible"), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibility()
        {
            var form = Substitute.For<IForm>();
            var securityExtender = Substitute.For<ISecurityExtenderCF>();
            var securityUser = Substitute.For<ISecurityUser>();
            var securityManager = Substitute.For<ISecurityManager>();

            Control control = new Control();
            ArrayList controls = new ArrayList();
            controls.Add(control);
            form.Controls.Returns(controls);

            securityExtender.GetSecurityGroups(control).Returns(SecurityGroups.Group_01);

            securityUser.BelongsToGroup(SecurityGroups.Group_01).Returns(false);

            SecurityVisibilityHelper securityVisibilityHelper = new SecurityVisibilityHelper(securityUser, securityManager);
            securityVisibilityHelper.UpdateControlsVisibility(form.Controls, securityExtender);

            Assert.IsTrue(control.Enabled);
        }


        [Test]
        public void ChangePasswordUsesPasswordComplexity()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();

            m_SecurityManagerTool.PasswordComplexity = passwordComplexity;
            m_SecurityService.ChangePassword(UserName, "pw");

            passwordComplexity.Received().AssertPasswordCriteria("pw");
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
#endif
