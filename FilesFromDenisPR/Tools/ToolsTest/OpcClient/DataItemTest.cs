using System;
using Core.Api.DataSource;
using Core.Api.Tools;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class DataItemTest
    {
        private IToolManager m_ToolManagerStub;
        private IDataSourceContainer m_DataSourceContainerStub;
        private IDataItemDataSource m_DataItemDataSourceStub;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_ToolManagerStub = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_ToolManagerStub.Runtime.Returns(false);
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            m_DataSourceContainerStub = null;
            m_DataItemDataSourceStub = null;
            TestHelper.ClearServices();
        }

        [Test]
        public void RegularUpdateDoesNotUpdateDataSourceIfSameValue()
        {
            m_ToolManagerStub.Runtime.Returns(true);

            VariantValue value = new VariantValue(5);
            IDataItemDataSource dataSource = CreateDataSource();
            
            dataSource.GetValue(Arg.Any<int>()).Returns(value.Value);
            dataSource.GetDataType(1).Returns(BEDATATYPE.DT_INTEGER4);

            IDataItem dataItem = CreateDataItem(value.Value, BEDATATYPE.DT_INTEGER4);

            Assert.AreEqual(((VariantValue)dataItem.Value).Value, value.Value);
            dataItem.Value = value;

            dataSource.DidNotReceiveWithAnyArgs().SetValue(Arg.Any<int>(), Arg.Any<object>());
            dataSource.Received().GetValue(Arg.Any<int>());
            dataSource.Received(1).SetDataType(1, BEDATATYPE.DT_INTEGER4);
            dataSource.Received(1).GetDataType(1).Returns(BEDATATYPE.DT_INTEGER4);
        }

        [Test]
        public void ForcedUpdateUpdatesDataSource()
        {
            m_ToolManagerStub.Runtime.Returns(true);

            VariantValue value = new VariantValue(5);
            IDataItemDataSource dataSource = CreateDataSource();
            dataSource.GetValue(0).Returns(value.Value);
            dataSource.GetDataType(1).Returns(BEDATATYPE.DT_INTEGER4);

            IDataItem dataItem = CreateDataItem(value.Value, BEDATATYPE.DT_INTEGER4);

            Assert.AreEqual(((VariantValue)dataItem.Value).Value, value.Value);
            dataItem.SetValueForced(value);

            dataSource.ReceivedWithAnyArgs().Write(Arg.Any<int>(), Arg.Any<object>());
            dataSource.Received().GetValue(Arg.Any<int>());
            dataSource.Received(1).SetDataType(1, BEDATATYPE.DT_INTEGER4);
            dataSource.Received(1).GetDataType(1).Returns(BEDATATYPE.DT_INTEGER4);
        }

        [Test]
        public void ChangingNonZeroIntegerValueToZeroFiresValueChangeAndValueOffEvents()
        {
            Int32 initialValue = 1;
            Int32 newValue = 0;
            bool valueChangeEventFired = false;
            bool valueOnEventFired = false;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeEventFired);
            Assert.AreEqual(false, valueOnEventFired);
            Assert.AreEqual(true, valueOffEventFired);
        }

        [Test]
        public void ChangingZeroIntegerValueToNonZeroFiresValueChangeAndValueOnEvents()
        {
            Int32 initialValue = 0;
            Int32 newValue = 1;
            bool valueChangeEventFired = false;
            bool valueOnEventFired = false;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeEventFired);
            Assert.AreEqual(true, valueOnEventFired);
            Assert.AreEqual(false, valueOffEventFired);
        }

        [Test]
        public void ChangingZeroIntegerValueToNonZeroFiresValueChangeOrErrorEvent()
        {
            Int32 initialValue = 0;
            Int32 newValue = 1;
            bool valueChangeOrErrorEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChangeOrError += (sender, e) => valueChangeOrErrorEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeOrErrorEventFired);
        }

        [Test]
        public void ChangingToQualityBadFiresValueChangeOrErrorEvent()
        {
            Int32 initialValue = 1;
            Int32 newValue = 1;
            bool valueChangeOrErrorEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChangeOrError += (sender, e) => valueChangeOrErrorEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Bad);

            Assert.AreEqual(true, valueChangeOrErrorEventFired);
        }

        [Test]
        public void SettingStringValuesDoesNotFireValueOnOrValueOffEvents()
        {
            string initialValue = "dummy value 1";
            string newValue = "dummy value 2";
            bool valueChangeEventFired = false;
            bool valueOnEventFired = false;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_STRING);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeEventFired);
            Assert.AreEqual(false, valueOnEventFired);
            Assert.AreEqual(false, valueOffEventFired);
        }

        [Test]
        public void ChangingNonZeroNonIntegerValueToZeroDoesNotFireValueOffEvent()
        {
            double initialValue = 1.0;
            double newValue = 0.0;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_REAL8);
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(false, valueOffEventFired);
        }

        [Test]
        public void ChangingZeroNonIntegerValueToNonZeroDoesNotFireValueOnEvent()
        {
            double initialValue = 0.0;
            double newValue = 1.0;
            bool valueOnEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_REAL8);
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            IDataItem iDataItem = dataItem;

            iDataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(false, valueOnEventFired);
        }

        [Test]
        public void ChangingFalseBoolValueToTrueBoolValueShouldFireValueOnEvent()
        {
            bool initialValue = false;
            bool newValue = true;

            bool valueChangeEventFired = false;
            bool valueOnEventFired = false;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_BOOLEAN);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeEventFired);
            Assert.AreEqual(true, valueOnEventFired);
            Assert.AreEqual(false, valueOffEventFired);
        }

        [Test]
        public void ChangingTrueBoolValueToFalseBoolValueShouldFireValueOnEvent()
        {
            bool initialValue = true;
            bool newValue = false;

            bool valueChangeEventFired = false;
            bool valueOnEventFired = false;
            bool valueOffEventFired = false;

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_BOOLEAN);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;
            dataItem.ValueOn += (sender, e) => valueOnEventFired = true;
            dataItem.ValueOff += (sender, e) => valueOffEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(true, valueChangeEventFired);
            Assert.AreEqual(false, valueOnEventFired);
            Assert.AreEqual(true, valueOffEventFired);
        }

        [Test]
        public void ChangingAnyValueToNullThrowsArgumentNullException()
        {
            Int32 initialValue = 1;
            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);

            Assert.Throws<ArgumentNullException>(() => dataItem.SetValueSilent(null, DataQuality.Unknown));
        }

        [Test]
        public void CallingSetValueSilentWithSameValueDoesNotFireValueChangedEvent()
        {
            int initialValue = 1;
            VariantValue newValue = initialValue;
            
            bool valueChangeEventFired = false;
            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(newValue, dataItem.Value);
            Assert.AreEqual(false, valueChangeEventFired);
        }

        [Test]
        public void CallingSetValueSilentWithSameValueDoesFireValueChangedEventWhenChangingComparisonBehavior()
        {
            int initialValue = 1;
            VariantValue newValue = initialValue;

            bool valueChangeEventFired = false;
            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.PreventDuplicateEvents = false;

            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(newValue, dataItem.Value);
            Assert.AreEqual(true, valueChangeEventFired);
        }

        [Test]
        public void CallingSetValueSilentWithDifferentValueFiresValueChangedEvent()
        {
            int initialValue = 1;
            VariantValue newValue = initialValue + 10;

            bool valueChangeEventFired = false;
            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER4);
            dataItem.ValueChange += (sender, e) => valueChangeEventFired = true;

            dataItem.SetValueSilent(newValue, DataQuality.Good);

            Assert.AreEqual(newValue, dataItem.Value);
            Assert.AreEqual(true, valueChangeEventFired);
        }

        [Test]
        public void ToogleWillNotChangeValueOfBooleanWhenControllerIsDisconnected()
        {
            VariantValue initialValue = true;

            IDataSourceContainer dataSourceContainer = Substitute.For<IDataSourceContainer>();
            dataSourceContainer.IsControllerConnected.Returns(false);

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_BOOLEAN);
            dataItem.DataSourceContainer = dataSourceContainer;

            dataItem.Toggle();

            Assert.AreEqual(initialValue, dataItem.Value);
        }

        [Test]
        public void IncreaseWillNotChangeValueOfIntegerWhenControllerIsDisconnected()
        {
            VariantValue initialValue = 10;
            VariantValue incValue = 5;

            IDataSourceContainer dataSourceContainer = Substitute.For<IDataSourceContainer>();
            dataSourceContainer.IsControllerConnected.Returns(false);

            IDataItem dataItem = CreateDataItem(initialValue, BEDATATYPE.DT_INTEGER2);
            dataItem.DataSourceContainer = dataSourceContainer;

            dataItem.IncrementAnalogValue(incValue);

            Assert.AreEqual(initialValue, dataItem.Value);
        }

        private IDataItem CreateDataItem(object initialValue, BEDATATYPE beDataType)
        {
            IDataItem dataItem = new DataItem();
            
            if (m_DataSourceContainerStub != null)
            {
                dataItem.DataSourceContainer = m_DataSourceContainerStub;
                dataItem.BeDataItemCookie = 1;
            }

            dataItem.DataType = beDataType;
            dataItem.SetValueSilent(initialValue, DataQuality.Good);

            return dataItem;
        }

        private IDataItemDataSource CreateDataSource()
        {
            m_DataSourceContainerStub = Substitute.For<IDataSourceContainer>();
            m_DataItemDataSourceStub = Substitute.For<IDataItemDataSource>();
            m_DataItemDataSourceStub.IsValidDevice(Arg.Is<int>(x => x != 0)).Returns(true);
            m_DataSourceContainerStub.DataSource.Returns(m_DataItemDataSourceStub);

            return m_DataItemDataSourceStub;
        }
    }
}
