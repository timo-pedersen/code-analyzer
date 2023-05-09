using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Bindings;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient.Bindings
{
    [TestFixture]
    public class TagBindingSourceDescriptionProviderWpfTest
    {
        private IBindingSourceDescriptionProviderWpf m_Provider;
        private IDataItemProxyProvider m_DataItemProxyProvider;

        [SetUp]
        public void SetUp()
        {
            m_DataItemProxyProvider = Substitute.For<IDataItemProxyProvider>();
            m_Provider = new TagBindingSourceDescriptionProviderWpf(m_DataItemProxyProvider);
        }

        [Test]
        public void TryGetTypeOfSourceReturnsProxyTypeForDataItemBinding()
        {
            Type expectedType = typeof(Int16);

            IDataItemProxy dataItemProxy = Substitute.For<IDataItemProxy>();
            dataItemProxy.Type.Returns(expectedType);

            m_DataItemProxyProvider[StringConstants.TagsRoot + "MyTag"].Returns(dataItemProxy);

            Binding binding = new Binding("[" + StringConstants.TagsRoot + "MyTag].Value");
            binding.Source = m_DataItemProxyProvider;

            Button button = new Button();
            BindingOperations.SetBinding(button, UIElement.VisibilityProperty, binding);

            Type type;
            bool result = m_Provider.TryGetTypeOfSource(binding, button, out type);

            Assert.That(result, Is.True);
            Assert.That(type, Is.EqualTo(expectedType));
        }

        [Test]
        public void TryGetTypeOfSourceReturnsFalseForUnknownBinding()
        {
            Binding binding = new Binding("Path");
            binding.Source = new object();

            Button button = new Button();
            BindingOperations.SetBinding(button, UIElement.VisibilityProperty, binding);

            Type type;
            bool result = m_Provider.TryGetTypeOfSource(binding, button, out type);

            Assert.That(result, Is.False);
        }
    }
}