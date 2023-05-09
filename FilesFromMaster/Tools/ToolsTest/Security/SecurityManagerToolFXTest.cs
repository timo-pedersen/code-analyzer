using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Blink;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityManagerToolFXTest
    {
        private ISecurityUser m_SecurityUser;
        private ISecurityServiceCF m_SecurityService;
        private Canvas m_Canvas;
        private AnalogNumericFX m_AnalogNumeric;

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_AnalogNumeric = new AnalogNumericFX();
            SecurityProperties.SetSecurityGroupsRequired(m_AnalogNumeric, SecurityGroups.Group_01);

            m_Canvas = new Canvas();
            m_Canvas.Children.Add(m_AnalogNumeric);

            m_SecurityService = new SecurityManagerTool();
            TestHelper.AddService<ISecurityServiceCF>(m_SecurityService);

            m_SecurityUser = MockRepository.GenerateStub<ISecurityUser>();
            TestHelper.SetSingleInstanceField(typeof(SecurityManagerToolCF), m_SecurityService, typeof(ISecurityUser), m_SecurityUser);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Disabled);

            Binding binding = new Binding("Value");
            
            m_AnalogNumeric.SetBinding(BlinkProperties.VisibleDynamicsValueProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(BlinkProperties.VisibleDynamicsValueProperty), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledKeepsVisibilityBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Disabled);

            Binding binding = new Binding("Value");
            
            m_AnalogNumeric.SetBinding(BlinkProperties.VisibleDynamicsValueProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(BlinkProperties.VisibleDynamicsValueProperty), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsDisabledRemovesIsEnabledBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Disabled);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(AnalogNumericFX.IsEnabledProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(AnalogNumericFX.IsEnabledProperty), Is.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsDisabledRemovesIsEnabledBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Disabled);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(AnalogNumericFX.IsEnabledProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(AnalogNumericFX.IsEnabledProperty), Is.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(EnabledProperties.EnabledDynamicsValueProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(EnabledProperties.EnabledDynamicsValueProperty), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIsHiddenKeepsIsEnabledBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(EnabledProperties.EnabledDynamicsValueProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(EnabledProperties.EnabledDynamicsValueProperty), Is.Not.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessDeniedWhenModeIsHiddenRemovesVisibilityBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(AnalogNumericFX.VisibilityProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(AnalogNumericFX.VisibilityProperty), Is.Null);
        }

        [Test]
        public void UpdateVisibilityForAccessGrantedWhenModeIHiddenRemovesVisibilityBinding()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            Binding binding = new Binding("Value");
            m_AnalogNumeric.SetBinding(AnalogNumericFX.VisibilityProperty, binding);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.That(m_AnalogNumeric.GetBindingExpression(AnalogNumericFX.VisibilityProperty), Is.Null);
        }

        [Test]
        public void MakeSureVisibilitySetToHiddenForAccessDeniedWhenModeIsHidden()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(false);

            m_SecurityService.UpdateVisibility(m_Canvas);

            // check that security sets visibility to hidden
            Assert.AreEqual(Visibility.Hidden, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));

            // check that regardless of BlinkProperties.VisibleDynamicsValueProperty, the security is the dominant player
            m_AnalogNumeric.SetValue(BlinkProperties.VisibleDynamicsValueProperty, true);
            Assert.AreEqual(Visibility.Hidden, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));

            // check that regardless of BlinkProperties.VisibleDynamicsValueProperty, the security is the dominant player
            m_AnalogNumeric.SetValue(BlinkProperties.VisibleDynamicsValueProperty, false);
            Assert.AreEqual(Visibility.Hidden, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));
        }

        [Test]
        public void MakeSureVisibilitySetToVisibleForAccessGrantedWhenModeIsHidden()
        {
            SecurityProperties.SetVisibilityOnAccessDenied(m_AnalogNumeric, VisibilityModes.Hidden);

            m_SecurityUser.Stub(x => x.BelongsToGroup(SecurityGroups.Group_01)).Return(true);

            m_SecurityService.UpdateVisibility(m_Canvas);

            Assert.AreEqual(Visibility.Visible, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));

            // check that regardless of BlinkProperties.VisibleDynamicsValueProperty, the security is the dominant player
            m_AnalogNumeric.SetValue(BlinkProperties.VisibleDynamicsValueProperty, false);
            Assert.AreEqual(Visibility.Hidden, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));

            // check that regardless of BlinkProperties.VisibleDynamicsValueProperty, the security is the dominant player
            m_AnalogNumeric.SetValue(BlinkProperties.VisibleDynamicsValueProperty, true);
            Assert.AreEqual(Visibility.Visible, m_AnalogNumeric.GetValue(AnalogNumericFX.VisibilityProperty));
        }
    }
}
