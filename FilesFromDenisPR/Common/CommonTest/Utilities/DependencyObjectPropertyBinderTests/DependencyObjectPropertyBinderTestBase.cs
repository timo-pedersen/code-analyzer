using System;
using System.Windows;
using System.Windows.Controls;
using Neo.ApplicationFramework.Common.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Utilities.DependencyObjectPropertyBinderTests
{
    [Apartment(System.Threading.ApartmentState.STA)]
    public abstract class DependencyObjectPropertyBinderTestBase<TDataSourceType>
    {
        private DataItemProxyProviderMock m_ProxyProvider;
        private IPropertyBinderWpf m_PropertyBinder;
        protected IDataItemProxy m_DataItemProxy;
        protected DependencyObject m_DependencyObject;

        [SetUp]
        public void TestSetup()
        {
            m_ProxyProvider = new DataItemProxyProviderMock();
            m_PropertyBinder = new DependencyObjectPropertyBinder(m_ProxyProvider);
            TestHelper.AddServiceStub<IEventBrokerService>();
        }

        [TearDown]
        public void TestTeardown()
        {
            m_DependencyObject = null;
            m_DataItemProxy = null;
            TestHelper.ClearServices();
        }

        protected void SetValueOnDataItem<TSubscriber>(TDataSourceType sourceTestValue, bool skipAsserts = false)
        {
            SetupObjects<TSubscriber>();

            m_DataItemProxy.Value = sourceTestValue;
            CheckBindingErrors(m_DependencyObject);
            object value1 = ((VariantValue)m_DataItemProxy.Value).Value;
            object value2 = Convert.ChangeType(((FrameworkElementMock<TSubscriber>)m_DependencyObject).Value, typeof(TDataSourceType));

            if (skipAsserts)
                return;

            Assert.AreEqual(value1, value2);
        }

        protected void SetValueOnDependencyProperty<TSubscriber>(TSubscriber destinationTestValue, bool skipAsserts = false)
        {
            using (var swedishCulture = new SelectSwedishTestingCulture())
            {
                SetupObjects<TSubscriber>();

                ((FrameworkElementMock<TSubscriber>)m_DependencyObject).Value = destinationTestValue;
                CheckBindingErrors(m_DependencyObject);

                if (skipAsserts)
                    return;

                Assert.AreEqual(Convert.ChangeType(((FrameworkElementMock<TSubscriber>)m_DependencyObject).Value, typeof(TDataSourceType)), ((VariantValue)m_DataItemProxy.Value).Value);
            }
        }

        private void SetupObjects<TSubscriber>()
        {
            if (m_DependencyObject == null)
            {
                m_DependencyObject = new FrameworkElementMock<TSubscriber>();
            }
            if (m_DataItemProxy == null)
            {
                m_DataItemProxy = new DataItemProxyMock<TDataSourceType>(string.Format("D{0}", m_ProxyProvider.ProxyList.Count));
                if (typeof(TSubscriber).IsValueType)
                {
                    m_DataItemProxy.Value = default(TSubscriber);
                }
                m_ProxyProvider.ProxyList.Add(m_DataItemProxy.FullName, m_DataItemProxy);
                m_PropertyBinder.BindToDataItem(m_DependencyObject, FrameworkElementMock<TSubscriber>.ValueProperty, m_DataItemProxy.FullName, null);
            }
        }

        private static void CheckBindingErrors(DependencyObject dependencyObject)
        {
            if (Validation.GetHasError(dependencyObject))
            {
                Exception thrownException = Validation.GetErrors(dependencyObject)[0].Exception;
                if (thrownException != null)
                {
                    Exception baseException = thrownException.GetBaseException();
                    if (baseException != null)
                    {
                        throw baseException;
                    }
                    else
                    {
                        throw thrownException;
                    }
                }
            }
        }
    }
}
