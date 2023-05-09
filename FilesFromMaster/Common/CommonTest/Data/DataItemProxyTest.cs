using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture]
    public class DataItemProxyTest
    {
        private IGlobalReferenceService m_GlobalReferenceServiceStub;

        private IDataItemProxySource m_DataItemStub;
        private VariantValue m_InitialValue;
        private VariantValue m_NewValueAndQuality;
        private VariantValue m_NewQuality;
        private string m_DataItemName;

        [SetUp]
        public void SetUp()
        {
            m_DataItemName = "MyController.MyDataItem";
            m_InitialValue = new VariantValue(12, DataQuality.Unknown);
            m_NewValueAndQuality = new VariantValue(24, DataQuality.Good);
            m_NewQuality = new VariantValue(12, DataQuality.Good);

            m_DataItemStub = MockRepository.GenerateStub<GlobalDataItem>();
            m_DataItemStub.Value = m_InitialValue;
            m_DataItemStub.Values = new [] { m_InitialValue };

            m_GlobalReferenceServiceStub = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_GlobalReferenceServiceStub.Stub(globalReferenceService => globalReferenceService.GetObject<IDataItemProxySource>(m_DataItemName))
                .Return(m_DataItemStub).Repeat.Any();
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
            dataItemProxy.Value = m_NewValueAndQuality;

            Assert.True(m_InitialValue.EqualsWithQuality(dataItemProxy.Value));
        }

        [Test]
        public void GetValueAfterSetValueAndRefreshShouldReturnTheNewlySetValue()
        {
            IDataItemProxy dataItemProxy = new DataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);
            dataItemProxy.Value = m_NewValueAndQuality;
            m_DataItemStub.Values = new[] { m_NewValueAndQuality };

            ((DataItemProxy)dataItemProxy).RefreshValue(true);
            
            Assert.True(m_NewValueAndQuality.EqualsWithQuality(dataItemProxy.Value));
        }

        [Test]
        public void ValueIsRetrievedFromDataItemOnConnect()
        {
            IDataItemProxy dataItemProxy = new DataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);

            Assert.True(m_InitialValue.EqualsWithQuality(dataItemProxy.Value));
        }

        [Test]
        public void SetValueWithNewQualityAndRaiseQualityChangeEventShouldChangeQualityAndRaisePropertyChangedEvent()
        {
            // ARRANGE
            IDataItemProxy dataItemProxy = new RealtimeDataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);
            dataItemProxy.Value = m_NewQuality;

            var wasPropertyChangeRaised = false;
            dataItemProxy.PropertyChanged += (_, _) => wasPropertyChangeRaised = true;

            // Initial data quality before value change event is raised
            Assert.AreEqual(m_InitialValue.Quality, ((VariantValue)dataItemProxy.Value).Quality);
            
            // ACT
            (m_DataItemStub as IGlobalDataItem).Raise(
                x => x.QualityChange += null,
                m_DataItemStub,
                new ValueChangedEventArgs(m_NewQuality));

            // ASSERT
            Assert.AreEqual(m_NewQuality.Quality, ((VariantValue)dataItemProxy.Value).Quality);
            Assert.IsTrue(wasPropertyChangeRaised);
        }

        [Test]
        public void SetValueAndRaiseValueChangeEventShouldChangeValueAndRaisePropertyChangedEvent()
        {
            // ARRANGE
            IDataItemProxy dataItemProxy = new RealtimeDataItemProxy();
            dataItemProxy.FullName = m_DataItemName;
            dataItemProxy.Connect(true);
            dataItemProxy.Value = m_NewValueAndQuality;

            bool wasPropertyChangeRaised = false;
            dataItemProxy.PropertyChanged += (_, _) => wasPropertyChangeRaised = true;

            // Initial value before value change event is raised
            Assert.IsTrue(m_InitialValue.EqualsWithQuality(dataItemProxy.Value));

            // ACT
            (m_DataItemStub as IGlobalDataItem).Raise(
                x => x.ValueChange += null,
                m_DataItemStub,
                new ValueChangedEventArgs(m_NewValueAndQuality));

            Assert.IsTrue(m_NewValueAndQuality.EqualsWithQuality(dataItemProxy.Value));
            Assert.IsTrue(wasPropertyChangeRaised);
        }
    }
}
