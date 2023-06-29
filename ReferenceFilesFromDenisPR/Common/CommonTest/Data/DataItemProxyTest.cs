using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture]
    public class DataItemProxyTest
    {
        private IGlobalReferenceService m_GlobalReferenceServiceStub;

        private IDataItemProxySource m_DataItemStub;
        private int m_InitialValue;
        private int m_NewValue;
        private string m_DataItemName;

        [SetUp]
        public void SetUp()
        {
            m_DataItemName = "MyController.MyDataItem";
            m_InitialValue = 12;
            m_NewValue = 24;

            m_DataItemStub = Substitute.For<IDataItemProxySource>();
            m_DataItemStub.Value = m_InitialValue;
            m_DataItemStub.Values = new [] { new VariantValue(m_InitialValue) };

            m_GlobalReferenceServiceStub = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_GlobalReferenceServiceStub.GetObject<IDataItemProxySource>(m_DataItemName).Returns(m_DataItemStub);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetValueAfterSetValueShouldNotReturnTheNewlySetValueButRatherTheLatestValueFromTheDataItem()
        {
            IDataItemProxy dataItemProxy = new DataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);
            dataItemProxy.Value = m_NewValue;

            Assert.AreEqual(new VariantValue(m_InitialValue), dataItemProxy.Value);
        }

        [Test]
        public void GetValueAfterSetValueAndRefreshShouldReturnTheNewlySetValue()
        {
            IDataItemProxy dataItemProxy = new DataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);
            dataItemProxy.Value = m_NewValue;
            m_DataItemStub.Values = new VariantValue[] { new VariantValue(m_NewValue) };

            ((DataItemProxy)dataItemProxy).RefreshValue(true);

            Assert.AreEqual(new VariantValue(m_NewValue), dataItemProxy.Value);
        }

        [Test]
        public void ValueIsRetrievedFromDataItemOnConnect()
        {
            IDataItemProxy dataItemProxy = new DataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);

            Assert.AreEqual(new VariantValue(m_InitialValue), dataItemProxy.Value);
        }
    }
}
