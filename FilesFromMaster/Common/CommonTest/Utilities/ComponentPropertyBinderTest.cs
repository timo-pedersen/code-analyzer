using System;
using System.Windows.Forms;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Bindings;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Utilities
{
    [TestFixture]
    public class ComponentPropertyBinderTest
    {
        private IPropertyBinder m_PropertyBinder;

        [SetUp]
        public void SetUp()
        {
            m_PropertyBinder = new ComponentPropertyBinder();
        }

        [Test]
        public void GetBindingReturnsAValidBindingWhenBoundToADataItemProxy()
        {
            IDataItemProxy dataItemProxyStub = MockRepository.GenerateStub<IDataItemProxy>();

            DynamicBinding binding = new DynamicBinding("Text", dataItemProxyStub, "Value");

            Button button = new Button();
            button.DataBindings.Add(binding);

            object dataItemBinding = m_PropertyBinder.GetBinding(button, "Text");
            Assert.IsNotNull(dataItemBinding);
        }

        [Test]
        public void GetBindingReturnsNullWhenBoundToAMultiLanguageResourceItem()
        {
            IDesignerResourceItem resourceItemStub = MockRepository.GenerateStub<IDesignerResourceItem>();

            Binding binding = new Binding("Text", resourceItemStub, "CurrentValue");

            Button button = new Button();
            button.DataBindings.Add(binding);

            object dataItemBinding = m_PropertyBinder.GetBinding(button, "Text");
            Assert.IsNull(dataItemBinding);
        }

        [TestFixture]
        public class ComponentPropertyBinder_GetTypeOfSource_Tests
        {
            private IBindingSourceDescriptionProvider m_Provider;

            [OneTimeSetUp]
            public void TestFixtureSetUp()
            {
                var irrelevantProvider = MockRepository.GenerateStub<IBindingSourceDescriptionProvider>();
                m_Provider = MockRepository.GenerateStub<IBindingSourceDescriptionProvider>();

                ComponentPropertyBinderCF.RegisterBindingSourceDescriptionProvider(irrelevantProvider);
                ComponentPropertyBinderCF.RegisterBindingSourceDescriptionProvider(m_Provider);
            }
   

            [Test]
            public void GetTypeOfSourceReturnsWhatTheFirstRelevantProviderReturns()
            {
                DynamicBinding binding = new DynamicBinding("Visible", new object(), "Member");
                Button button = new Button();
                button.DataBindings.Add(binding);

                Type expectedType = typeof(Int16);
                Type dummy;
                m_Provider.Stub(x => x.TryGetTypeOfSource(binding, out dummy)).Return(true).OutRef(expectedType);

                IPropertyBinder binder = new ComponentPropertyBinder();
                Type type = binder.GetDataSourceType(button, "Visible");

                Assert.That(type, Is.EqualTo(expectedType));
            }

            [Test]
            public void GetTypeOfSourceThrowsNotSupportedExceptionIfNoProvidersFindsSomething()
            {
                DynamicBinding binding = new DynamicBinding("Visible", new object(), "Member");
                Button button = new Button();
                button.DataBindings.Add(binding);

                Type dummy;
                m_Provider.Stub(x => x.TryGetTypeOfSource(binding, out dummy)).Return(false);

                IPropertyBinder binder = new ComponentPropertyBinder();
                Assert.Throws<NotSupportedException>(() => binder.GetDataSourceType(button, "Visible"));               
            }
        }

    }

}
