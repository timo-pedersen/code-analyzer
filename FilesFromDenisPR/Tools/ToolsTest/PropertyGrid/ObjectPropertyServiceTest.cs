using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Attributes;
using Neo.ApplicationFramework.Controls.ActionMenu;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.PropertyGrid
{
    [TestFixture]
    public class ObjectPropertyServiceTest
    {
        private IObjectPropertyService m_ObjectPropertyService;
        private Dictionary<string, string> m_DisplayNames;
        private Dictionary<string, bool> m_IsBindableProperties;
        private ITarget m_CurrentTargetStub;

        [SetUp]
        public void SetUp()
        {
            m_DisplayNames = new Dictionary<string, string>();
            m_IsBindableProperties = new Dictionary<string, bool>();

            m_CurrentTargetStub = Substitute.For<ITarget>();
            
            var targetServiceStub = Substitute.For<ITargetService>();
            targetServiceStub.CurrentTarget = m_CurrentTargetStub;

            m_ObjectPropertyService = new ObjectPropertyService(
                targetServiceStub.ToILazy(),
                m_DisplayNames,
                m_IsBindableProperties);
        }

        [Test]
        public void DisplayIsNotChangedForSupportedProperty()
        {
            Type rectangleType = typeof(Rectangle);
            Assert.AreEqual("Stretch", m_ObjectPropertyService.GetDisplayName(rectangleType, TypeDescriptor.GetProperties(rectangleType)["Stretch"]));
        }

        [Test]
        public void DisplayNameIsEmptyForGeneralUnsupportedProperty()
        {
            m_DisplayNames.Add("Stretch", string.Empty);

            Type rectangleType = typeof(Rectangle);
            Assert.IsEmpty(m_ObjectPropertyService.GetDisplayName(rectangleType, TypeDescriptor.GetProperties(rectangleType)["Stretch"]));
        }

        [Test]
        public void DisplayNameIsChangedForGeneralStrokeProperty()
        {
            m_DisplayNames.Add("Stroke", "Outline");

            Type rectangleType = typeof(Rectangle);
            Assert.AreEqual("Outline", m_ObjectPropertyService.GetDisplayName(rectangleType, TypeDescriptor.GetProperties(rectangleType)["Stroke"]));
        }

        [Test]
        public void DisplayNameIsOnlyEmptyForSpecificUnsupportedProperty()
        {
            m_DisplayNames.Add("System.Windows.Controls.Button.BorderBrush", string.Empty);

            Type type = typeof(System.Windows.Controls.Button);
            Assert.IsEmpty(m_ObjectPropertyService.GetDisplayName(type, TypeDescriptor.GetProperties(type)["BorderBrush"]));
            type = typeof(Border);
            Assert.AreEqual("BorderBrush", m_ObjectPropertyService.GetDisplayName(type, TypeDescriptor.GetProperties(type)["BorderBrush"]));
        }

        [Test]
        public void DisplayNameIsOnlyChangedForSpecificProperty()
        {
            m_DisplayNames.Add("Neo.ApplicationFramework.Controls.Controls.Rectangle.Stroke", "Outline");

            Type type = typeof(Rectangle);
            Assert.AreEqual("Outline", m_ObjectPropertyService.GetDisplayName(type, TypeDescriptor.GetProperties(type)["Stroke"]));
            type = typeof(Ellipse);
            Assert.AreEqual("Stroke", m_ObjectPropertyService.GetDisplayName(type, TypeDescriptor.GetProperties(type)["Stroke"]));
        }

        [Test]
        public void DisplayNameIsEmptyWhenLocalDisplayNameAttributeOnPropertyIsEmpty()
        {
            Type actionListViewType = typeof(ActionMenuHost);
            Assert.AreEqual(string.Empty, m_ObjectPropertyService.GetDisplayName(actionListViewType, TypeDescriptor.GetProperties(actionListViewType)["ScaleColor"]));
        }

        [Test]
        public void IsBindableReturnsTrueForSupportedProperty()
        {
            Type rectangleType = typeof(Rectangle);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(rectangleType, TypeDescriptor.GetProperties(rectangleType)["Width"]));
        }

        [Test]
        public void IsBindableReturnsFalseForGeneralUnsupportedProperty()
        {
            m_IsBindableProperties.Add("Stretch", false);

            Type rectangleType = typeof(Rectangle);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(rectangleType, TypeDescriptor.GetProperties(rectangleType)["Stretch"]));
        }

        [Test]
        public void IsBindableOnlyReturnsFalseForSpecificUnsupportedProperty()
        {
            m_IsBindableProperties.Add("System.Windows.Controls.Button.BorderBrush", false);

            Type type = typeof(System.Windows.Controls.Button);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BorderBrush"]));
            type = typeof(Border);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BorderBrush"]));
        }

        [Test]
        public void IsBindableReturnsFalseForUnsupportedPropertyType()
        {
            Type type = typeof(Neo.ApplicationFramework.Controls.Button);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["Margin"]));
        }

        [Test]
        public void IsBindableReturnsTrueForSupportedPropertyType()
        {
            Type type = typeof(Neo.ApplicationFramework.Controls.Button);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["Width"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenPropertyDisplayNameIsEmpty()
        {
            m_DisplayNames.Add("Stroke", string.Empty);

            Type type = typeof(Rectangle);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["Stroke"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsCEAndIsBindableAttributeIsSetForPC()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["OnlyBindableInPC"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsPCAndIsBindableAttributeIsSetForPC()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["OnlyBindableInPC"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsPCAndIsBindableAttributeIsSetForCE()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["OnlyBindableInCE"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsCEAndIsBindableAttributeIsSetForCE()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["OnlyBindableInCE"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsPCAndIsBindableAttributeIsSetForAll()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableForAll"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsCEAndIsBindableAttributeIsSetForAll()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableForAll"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsCEAndIsBindableAttributeIsNotSpecified()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["DefaultBindableForAll"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsPCAndIsBindableAttributeIsNotSpecified()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["DefaultBindableForAll"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsCEAndIsBindableAttributeIsNone()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["NotBindable"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsPCAndIsBindableAttributeIsNone()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["NotBindable"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsCEAndIsBindableAttributeIsSetForPCOnNewInheritedProperty()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(MoreInheritedBindableAttributeStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["DefaultBindableForAll"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsCEAndBindableAttributeIsSetToFalse()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["NotBindableByMS"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsPCAndBindableAttributeIsSetToFalse()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["NotBindableByMS"]));
        }

        public void IsBindableReturnsTrueWhenTargetIsPCAndIsBindableAttributeIsSetForPCPCOnNewInheritedPropertyEvenThoughBasePropertyHasBindableAttributeSetToFalse()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(InheritedBindableAttributeStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["NotBindableByMS"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsCEAndBindableAttributeIsSetToTrue()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableByMS"]));
        }

        [Test]
        public void IsBindableReturnsTrueWhenTargetIsPCAndBindableAttributeIsSetToTrue()
        {
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsTrue(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableByMS"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsCEForUnsupportedPropertyEvenThoughBindableAttributeIsSetToTrue()
        {
            m_IsBindableProperties.Add("BindableByMS", false);
            m_CurrentTargetStub.Id.Returns(TargetPlatform.WindowsCE);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableByMS"]));
        }

        [Test]
        public void IsBindableReturnsFalseWhenTargetIsPCForUnsupportedPropertyEvenThoughBindableAttributeIsSetToTrue()
        {
            m_IsBindableProperties.Add("BindableByMS", false);
            m_CurrentTargetStub.Id.Returns(TargetPlatform.Windows);

            Type type = typeof(BindableAttributesStub);
            Assert.IsFalse(m_ObjectPropertyService.IsBindable(type, TypeDescriptor.GetProperties(type)["BindableByMS"]));
        }
    }

    internal class BindableAttributesStub
    {
        [IsBindable(BindableTargets.PC)]
        public string OnlyBindableInPC { get; set; }

        [IsBindable(BindableTargets.CE)]
        public string OnlyBindableInCE { get; set; }

        [IsBindable(BindableTargets.All)]
        public string BindableForAll { get; set; }

        [IsBindable(BindableTargets.None)]
        public string NotBindable { get; set; }

        public string DefaultBindableForAll { get; set; }

        [Bindable(BindableSupport.No)]
        public string NotBindableByMS { get; set; }

        [Bindable(BindableSupport.Yes)]
        public string BindableByMS { get; set; }
    }

    internal class InheritedBindableAttributeStub : BindableAttributesStub
    {
        [IsBindable(BindableTargets.PC)]
        public new string DefaultBindableForAll { get; set; }

        [IsBindable(BindableTargets.PC)]
        public new string NotBindableByMS { get; set; }
    }

    internal class MoreInheritedBindableAttributeStub : InheritedBindableAttributeStub
    {
    }
}

