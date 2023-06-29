using System;
using System.Windows.Forms;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Bindings;
using NSubstitute;
using NUnit.Framework;

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
            IDataItemProxy dataItemProxyStub = Substitute.For<IDataItemProxy>();

            DynamicBinding binding = new DynamicBinding("Text", dataItemProxyStub, "Value");

            Button button = new Button();
            button.DataBindings.Add(binding);

            object dataItemBinding = m_PropertyBinder.GetBinding(button, "Text");
            Assert.IsNotNull(dataItemBinding);
        }

        [Test]
        public void GetBindingReturnsNullWhenBoundToAMultiLanguageResourceItem()
        {
            IDesignerResourceItem resourceItemStub = Substitute.For<IDesignerResourceItem>();

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
                var irrelevantProvider = Substitute.For<IBindingSourceDescriptionProvider>();
                m_Provider = Substitute.For<IBindingSourceDescriptionProvider>();

                ComponentPropertyBinderCF.RegisterBindingSourceDescriptionProvider(irrelevantProvider);
                ComponentPropertyBinderCF.RegisterBindingSourceDescriptionProvider(m_Provider);
            }
   

            [Test]
            public void GetTypeOfSourceReturnsWhatTheFirstRelevantProviderReturns()
            {
                DynamicBinding binding = new DynamicBinding("Visible", new object(), "Member");
                Button button = new Button();
                button.DataBindings.Add(binding);

                Type expectedType = typeof(short);
                m_Provider.TryGetTypeOfSource(binding, out Arg.Any<Type>())
                    .Returns(x => 
                    {
                        x[1] = expectedType;
                        return true; 
                    });

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
                m_Provider.TryGetTypeOfSource(binding, out dummy).Returns(false);

                IPropertyBinder binder = new ComponentPropertyBinder();
                Assert.Throws<NotSupportedException>(() => binder.GetDataSourceType(button, "Visible"));               
            }
        }

    }

}
