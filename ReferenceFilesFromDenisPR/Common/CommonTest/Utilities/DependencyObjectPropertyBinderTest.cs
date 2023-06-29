using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Core.Controls.Api.Bindings;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Controls.Chart;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Bindings;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Utilities
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class DependencyObjectPropertyBinderTest
    {
        private DataItemProxyProviderMock m_ProxyProvider;
        private IPropertyBinderWpf m_PropertyBinder;

        [SetUp]
        public void SetUp()
        {
            TestHelper.Bindings.Wpf.RegisterSimpleDataItemBindingSourceProvider();
            m_ProxyProvider = new DataItemProxyProviderMock();

            m_PropertyBinder = new DependencyObjectPropertyBinder(m_ProxyProvider);
            TestHelper.AddServiceStub<IEventBrokerService>();
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.Bindings.Wpf.ClearProviders();
            TestHelper.ClearServices();
        }

        [Test]
        public void GetBindingReturnsAValidBindingWhenBoundToADataItemProxy()
        {
            Binding binding = new Binding("[Controller1.D0].Value");
            binding.Source = DataItemProxyFactory.Instance;

            Button button = new Button();
            button.SetBinding(ContentControl.ContentProperty, binding);

            object dataItemBinding = m_PropertyBinder.GetBinding(button, button.GetDefaultDependencyProperty());
            Assert.IsNotNull(dataItemBinding);
        }

        [Test]
        public void BindProjectObjectBindsAgainstVariantValueArrayWhenTargetIsAnArrayProperty()
        {
            string tagName = StringConstants.TagsRoot + "D0";
            IDataItemProxy dataItemProxyMock = new DataItemProxyMock<int>(tagName);
            m_ProxyProvider.ProxyList.Add(tagName, dataItemProxyMock);

            Series series = new Series();

            m_PropertyBinder.BindToDataItem(series, tagName, Series.XValuesProperty.Name);

            BindingExpression bindingExpression =
                m_PropertyBinder.GetBinding(series, Series.XValuesProperty.Name) as BindingExpression;
            Assert.That(bindingExpression, Is.Not.Null);
            Assert.That(bindingExpression.ParentBinding.Path.Path, Does.EndWith("Values"));
        }

        [Test]
        public void GetBindingReturnsNullWhenBoundToAMultiLanguageResourceItem()
        {
            IDesignerResourceItem resourceItemStub = Substitute.For<IDesignerResourceItem>();

            Binding binding = new Binding("CurrentValue");
            binding.Source = resourceItemStub;

            Button button = new Button();
            button.SetBinding(ContentControl.ContentProperty, binding);

            object dataItemBinding = m_PropertyBinder.GetBinding(button, button.GetDefaultDependencyProperty());
            Assert.IsNull(dataItemBinding);
        }
    }

    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class DependencyObjectPropertyBinder_GetTypeOfSource_Tests
    {
        private IBindingSourceDescriptionProviderWpf m_Provider;

        [SetUp]
        public void SetUp()
        {
            var bindingService = TestHelper.AddServiceStub<IBindingService>();
            bindingService.IsSupporting(default(Binding)).ReturnsForAnyArgs(true);

            m_Provider = Substitute.For<IBindingSourceDescriptionProviderWpf>();
            DependencyObjectPropertyBinder.RegisterBindingSourceDescriptionProvider(m_Provider);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.Bindings.Wpf.ClearProviders();
            TestHelper.ClearServices();
        }

        [Test]
        public void GetTypeOfSourceReturnsWhatTheFirstRelevantProviderReturns()
        {
            Binding binding = new Binding("Member");
            #region Binding hack, since GetBinding still contains unmockable code!
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(IScreenWindow), 1);
            #endregion
            Button button = new Button();
            BindingOperations.SetBinding(button, UIElement.VisibilityProperty, binding);

            Type expectedType = typeof(Int16);
            m_Provider.TryGetTypeOfSource(binding, button, out Arg.Any<Type>())
                .Returns(x =>
                {
                    x[2] = expectedType;
                    return true;
                });

            IPropertyBinder binder = new DependencyObjectPropertyBinder(Substitute.For<IDataItemProxyProvider>());
            Type type = binder.GetDataSourceType(button, UIElement.VisibilityProperty.Name);

            Assert.That(type, Is.EqualTo(expectedType));
        }

        [Test]
        public void GetTypeOfSourceReturnsNullIfNoProvidersFindsSomething()
        {
            Binding binding = new Binding("Member");
            #region Binding hack, since GetBinding still contains unmockable code!
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(IScreenWindow), 1);
            #endregion
            Button button = new Button();
            BindingOperations.SetBinding(button, UIElement.VisibilityProperty, binding);

            Type dummy;
            m_Provider.TryGetTypeOfSource(binding, button, out dummy).Returns(false);

            IPropertyBinder binder = new DependencyObjectPropertyBinder(Substitute.For<IDataItemProxyProvider>());
            Type type = binder.GetDataSourceType(button, "Visibility");

            Assert.That(type, Is.Null);
        }
    }
}
