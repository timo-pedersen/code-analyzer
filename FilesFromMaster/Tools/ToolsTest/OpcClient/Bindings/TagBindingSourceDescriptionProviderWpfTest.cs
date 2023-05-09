using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Bindings;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_DataItemProxyProvider = MockRepository.GenerateStub<IDataItemProxyProvider>();
            m_Provider = new TagBindingSourceDescriptionProviderWpf(m_DataItemProxyProvider);
        }

        [Test]
        public void TryGetTypeOfSourceReturnsProxyTypeForDataItemBinding()
        {
            Type expectedType = typeof(Int16);

            IDataItemProxy dataItemProxy = MockRepository.GenerateStub<IDataItemProxy>();
            dataItemProxy.Stub(x => x.Type).Return(expectedType);

            m_DataItemProxyProvider.Stub(x => x[StringConstants.TagsRoot + "MyTag"]).Return(dataItemProxy);

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