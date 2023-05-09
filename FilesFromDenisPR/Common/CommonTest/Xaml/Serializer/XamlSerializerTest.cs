using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Data;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Converters;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Xaml.Serializer
{
    [TestFixture]
    public class XamlSerializerTest
    {
        private const int ButtonWidth = 50;
        private const int ButtonHeight = 25;
        private Button m_Button;
        private IXamlSerializer m_XamlSerializer;

        [SetUp]
        public void TestFixtureSetup()
        {
            TestHelper.AddService<IEventBindingService>(new SampleEventBindingService(ServiceContainerCF.Instance));

            var targetService = Substitute.For<ITargetService>();
            var target = Substitute.For<ITarget>();
            target.Id.Returns(TargetPlatform.Windows);
            targetService.CurrentTarget.Returns(target);
            targetService.CurrentTargetInfo.Returns(Substitute.For<ITargetInfo>());
            TestHelper.AddService<ITargetService>(targetService);

            m_XamlSerializer = new XamlSerializer(ServiceContainerCF.Instance);

            m_Button = new Button();
            m_Button.Width = ButtonWidth;
            m_Button.Height = ButtonHeight;
        }

        [Test]
        public void SerializeAndDeserialize()
        {
            Label label = new Label();
            label.Text = "Label";

            List<FrameworkElement> elements = new List<FrameworkElement>();
            elements.Add(label);

            m_XamlSerializer.UseNamespaces = false;
            string retVal = m_XamlSerializer.Serialize(elements);
            Assert.IsFalse(string.IsNullOrEmpty(retVal), "The Xaml property should contain the serialized button.");

            elements = m_XamlSerializer.Deserialize(retVal) as List<FrameworkElement>;

            Label resultLabel = elements[0] as Label;

            Assert.IsNotNull(resultLabel, "A label should have been created.");
            Assert.AreEqual(label.Height, resultLabel.Height);
            Assert.AreEqual(label.Width, resultLabel.Width);
            Assert.AreEqual(label.Text, resultLabel.Text);
        }

        [Test]
        public void SerializeAndDeserializeMultiple()
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            AddManyButtons(elements);

            m_XamlSerializer.UseNamespaces = false;
            string retVal = m_XamlSerializer.Serialize(elements);
            Assert.IsFalse(string.IsNullOrEmpty(retVal), "The Xaml property should contain the serialized button.");

            elements = m_XamlSerializer.Deserialize(retVal) as List<FrameworkElement>;

            AssertButtons(elements);
        }

        [Test]
        public void SerializeAndDeserializeWithBindingsWithoutNamespace()
        {
            List<FrameworkElement> elements = GetDataBoundElements();

            m_XamlSerializer.UseNamespaces = false;
            string serialized = m_XamlSerializer.Serialize(elements);
            Assert.IsFalse(string.IsNullOrEmpty(serialized), "The Xaml property should contain the serialized controls.");

            elements = m_XamlSerializer.Deserialize(serialized) as List<FrameworkElement>;
            Assert.AreEqual(2, elements.Count, "elements should contain two FrameworkElements.");
        }

        [Test]
        public void SerializeAndDeserializeWithBindingsWithNamespace()
        {
            List<FrameworkElement> elements = GetDataBoundElements();

            m_XamlSerializer.UseNamespaces = true;
            string serialized = m_XamlSerializer.Serialize(elements);
            Assert.IsFalse(string.IsNullOrEmpty(serialized), "The Xaml property should contain the serialized controls.");

            elements = m_XamlSerializer.Deserialize(serialized) as List<FrameworkElement>;
            Assert.AreEqual(2, elements.Count, "elements should contain two FrameworkElements.");
        }

        [Test]
        public void BindingsAndConvertersSurviveSerializationWithoutNamespace()
        {
            List<FrameworkElement> elements = GetDataBoundElements();

            m_XamlSerializer.UseNamespaces = false;
            string serialized = m_XamlSerializer.Serialize(elements);

            elements = m_XamlSerializer.Deserialize(serialized) as List<FrameworkElement>;

            Binding valueBinding = elements[0].GetBindingExpression(AnalogNumericFX.ValueProperty).ParentBinding;

            Assert.IsNotNull(valueBinding);
            Assert.IsInstanceOf<VariantValueConverter>(valueBinding.Converter);
        }

        [Test]
        public void BindingsAndConvertersSurviveSerializationWithNamespace()
        {
            List<FrameworkElement> elements = GetDataBoundElements();

            m_XamlSerializer.UseNamespaces = true;
            string serialized = m_XamlSerializer.Serialize(elements);

            elements = m_XamlSerializer.Deserialize(serialized) as List<FrameworkElement>;

            Binding valueBinding = elements[0].GetBindingExpression(AnalogNumericFX.ValueProperty).ParentBinding;

            Assert.IsNotNull(valueBinding);
            Assert.IsInstanceOf<VariantValueConverter>(valueBinding.Converter);
        }

        #region Helpers

        private List<FrameworkElement> GetDataBoundElements()
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            AnalogNumericFX analogNumericFXone = CreateAnalogNumericFX("Analog1");
            AnalogNumericFX analogNumericFXtwo = CreateAnalogNumericFX("Analog2");
            SetBinding(analogNumericFXone, "Tag1");
            SetBinding(analogNumericFXtwo, "Tag1");
            elements.Add(analogNumericFXone);
            elements.Add(analogNumericFXtwo);
            return elements;
        }
        private void AddManyButtons(List<FrameworkElement> elements)
        {
            for (int index = 1; index <= 100; index++)
            {
                Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
                button.Name = "Button" + index;
                button.Width = index;
                button.Height = index;

                elements.Add(button);
            }
        }

        private void AssertButtons(List<FrameworkElement> elements)
        {
            foreach (Neo.ApplicationFramework.Controls.Button button in elements)
            {
                Assert.IsNotNull(button, "A button should have been created.");
                int index = Convert.ToInt32(button.Name.Substring(6));
                Assert.AreEqual(index, button.Height);
                Assert.AreEqual(index, button.Width);
            }
        }

        private AnalogNumericFX CreateAnalogNumericFX(string name)
        {
            AnalogNumericFX analogNumericFX = new AnalogNumericFX();
            analogNumericFX.Name = name;
            return analogNumericFX;
        }

        private void SetBinding(FrameworkElement element, string bindingName)
        {
            DependencyProperty dependencyProperty = GetDefaultDependencyProperty(element);
            Binding binding = new Binding(bindingName);
            binding.Mode = BindingMode.TwoWay;
            binding.Source = DataItemProxyFactory.Instance;

            string converterKeyName = DependencyObjectPropertyBinder.GetConverterKeyName(dependencyProperty);
            VariantValueConverter converter = new VariantValueConverter();
            binding.Converter = converter;
            element.Resources.Add(converterKeyName, converter);
            element.SetBinding(dependencyProperty, binding);
        }

        private DependencyProperty GetDefaultDependencyProperty(FrameworkElement element)
        {
            string defaultPropertyName = GetDefaultProperty(element);
            if (string.IsNullOrEmpty(defaultPropertyName))
                return null;

            DependencyProperty dependencyProperty = GetDependencyProperty(element, defaultPropertyName);
            return dependencyProperty;
        }

        protected string GetDefaultProperty(FrameworkElement element)
        {
            Type type = element.GetType();
            object[] attributes = type.GetCustomAttributes(typeof(DefaultPropertyAttribute), true);
            if (attributes.Length == 0)
                return string.Empty;

            DefaultPropertyAttribute contentPropertyAttribute = (DefaultPropertyAttribute)attributes[0];
            return contentPropertyAttribute.Name;
        }

        private DependencyProperty GetDependencyProperty(DependencyObject dependencyObject, string propertyName)
        {
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(dependencyObject);
            PropertyDescriptor propertyDescriptor = propertyDescriptors[propertyName];
            if (propertyDescriptor == null)
                return null;

            DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(propertyDescriptor);
            if (dependencyPropertyDescriptor == null)
                return null;

            return dependencyPropertyDescriptor.DependencyProperty;
        }

        #endregion

    }
     
}
