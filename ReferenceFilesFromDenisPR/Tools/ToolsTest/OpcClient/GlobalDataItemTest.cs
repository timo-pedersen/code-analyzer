using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Core.Api.DataSource;
using Core.Api.Lifecycle;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class GlobalDataItemTest
    {
        private IDataItemCountingService m_DataItemCountingService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            var toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(false);

            m_DataItemCountingService = TestHelper.CreateAndAddServiceStub<IDataItemCountingService>();
        }

        #region Setting properties

        [Test]
        public void ChangingDataTypeOnGlobalDataItemPropagatesToAllDependentDataItems()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller 1");
            IDataItem dataItemTwo = CreateDataItemWithController("Controller 2");

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItemOne);
            globalDataItem[0].DataItems.Add(dataItemTwo);

            globalDataItem.DataType = BEDATATYPE.DT_DATETIME;

            Assert.AreEqual(BEDATATYPE.DT_DATETIME, dataItemOne.DataType);
            Assert.AreEqual(BEDATATYPE.DT_DATETIME, dataItemTwo.DataType);
        }

        [Test]
        public void ChangingSizeOnGlobalDataItemPropagatesToAllDependentDataItems()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller 1");
            IDataItem dataItemTwo = CreateDataItemWithController("Controller 2");

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItemOne);
            globalDataItem[0].DataItems.Add(dataItemTwo);

            globalDataItem.Size = 16;

            Assert.AreEqual(16, dataItemOne.Size);
            Assert.AreEqual(16, dataItemTwo.Size);
        }

        [Test]
        public void ChangingDataTypeToBooleanOnGlobalDataItemAsInternalVariableChangesTypeOfValueToMatch()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            globalDataItem.Value = 1;

            globalDataItem.DataType = BEDATATYPE.DT_BOOLEAN;

            Assert.AreEqual(typeof(bool), globalDataItem.Value.Value.GetType());
        }

        [Test]
        public void ChangingDataTypeToStringOnGlobalDataItemAsInternalVariableChangesTypeOfValueToMatch()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = 0;

            globalDataItem.DataType = BEDATATYPE.DT_STRING;

            Assert.AreEqual(typeof(string), globalDataItem.Value.Value.GetType());
        }

        [Test]
        public void ChangingDataTypeToFromDateTimeToShortUpdatesValueToDefaultValueForNewDataType()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_DATETIME;
            globalDataItem.Value = DateTime.Now;

            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            Assert.AreEqual(typeof(short), globalDataItem.Value.Value.GetType());
            Assert.AreEqual(0, globalDataItem.Value.Value);
        }

        [Test]
        public void ChangingDataTypeToFromshortToDateTimeUpdatesValueToDefaultValueForNewDataType()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.Value = 0;

            globalDataItem.DataType = BEDATATYPE.DT_DATETIME;

            Assert.AreEqual(typeof(DateTime), globalDataItem.Value.Value.GetType());
        }

        private IDataItem CreateDataItemWithController(string controllerName)
        {
            IDataSourceContainer controller = Substitute.For<IDataSourceContainer>();
            controller.Name = controllerName;

            IDataItem dataItem = Substitute.For<IDataItem>();
            dataItem.DataSourceContainer = controller;

            return dataItem;
        }

        #endregion

        #region Data value changes

        [Test]
        public void SettingValueOnGlobalDataItemDoesNotFireValueChangeUntilUnderlyingDataItemReportsValueChange()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            globalDataItem.Value = 10;

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void SettingValueOnGlobalDataItemFiresValueChangeWhenUnderlyingDataItemReportsValueChange()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            globalDataItem.Value = 10;

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsTrue(wasRaised);
            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
        }

        [Test]
        public void SettingValueOnGlobalDataItemAsInternalVariableFiresValueChangeImmediately()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            globalDataItem.Value = 10;

            Assert.IsTrue(wasRaised);
            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
        }

        [Test]
        public void SettingValueOnGlobalDataItemAsInternalVariableWillHaveItsValueBeTypedCorrectly()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 100;

            Assert.AreEqual(typeof(Int16), globalDataItem.Value.Value.GetType());
        }

        [Test]
        public void SettingValueOnGlobalDataItemWillSetValueOnAllUnderlyingDataItemsNoMatterWhatAccessRight()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            IDataSourceContainer controllerFour = null;
            IDataItem dataItemOneControllerFour = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerFour, out dataItemOneControllerFour, "Controller4", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            //ARRAYTAG
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);
            globalDataItem.DataItems.Add(dataItemOneControllerFour);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            globalDataItem.AccessRights[controllerFour.Name] = AccessRights.None;

            globalDataItem.Value = 10;

            Assert.AreEqual(new VariantValue(10), dataItemOneControllerOne.Value);
            Assert.AreEqual(new VariantValue(10), dataItemOneControllerTwo.Value);
            Assert.AreEqual(new VariantValue(10), dataItemOneControllerThree.Value);
            Assert.AreEqual(new VariantValue(10), dataItemOneControllerFour.Value);
        }

        [Test]
        public void DataIsExchangedOnValueChangedWhenANoneTriggerIsUsed()
        {
            IDataSourceContainer controllerOne = Substitute.For<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
            IDataItem dataItemOneControllerOne = Substitute.For<IDataItem>();
            dataItemOneControllerOne.DataSourceContainer.Returns(controllerOne);

            IDataSourceContainer controllerTwo = Substitute.For<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
            IDataItem dataItemOneControllerTwo = Substitute.For<IDataItem>();
            dataItemOneControllerTwo.DataSourceContainer.Returns(controllerTwo);

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            //ARRAYTAG
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;

            globalDataItem.Trigger = DataTrigger.None;

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerOne, new ValueChangedEventArgs(variantValue));

            dataItemOneControllerOne.DidNotReceive().Value = null;
            dataItemOneControllerTwo.Received(1).Value = null;
        }

        [Test]
        public void DataIsNotExchangedOnValueChangedWhenATriggerIsUsed()
        {
            IDataSourceContainer controllerOne = Substitute.For<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
            IDataItem dataItemOneControllerOne = Substitute.For<IDataItem>();
            dataItemOneControllerOne.DataSourceContainer.Returns(controllerOne);

            IDataSourceContainer controllerTwo = Substitute.For<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
            IDataItem dataItemOneControllerTwo = Substitute.For<IDataItem>();
            dataItemOneControllerTwo.DataSourceContainer.Returns(controllerTwo);

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            //ARRAYTAG
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;

            globalDataItem.Trigger = new DataTrigger();

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerOne, new ValueChangedEventArgs(variantValue));

            dataItemOneControllerOne.DidNotReceive().Value = null;
            dataItemOneControllerTwo.DidNotReceive().Value = null;
        }

        [Test]
        public void SettingValueOnGlobalDataItemWillSetValueOnAllUnderlyingDataItemsEvenIfATriggerIsUsed()
        {
            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Trigger = new DataTrigger();
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;

            globalDataItem.Value = 10;

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
            Assert.AreEqual(new VariantValue(10), dataItem.Value);
        }

        [Test]
        public void CallingBatchReadOnGlobalDataItemWillCallBatchReadOnDataItemsWithReadAccessAsWellAsNoneAccess()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            IDataSourceContainer controllerFour = null;
            IDataItem dataItemOneControllerFour = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerFour, out dataItemOneControllerFour, "Controller4", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            //ARRAYTAG
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);
            globalDataItem.DataItems.Add(dataItemOneControllerFour);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            globalDataItem.AccessRights[controllerFour.Name] = AccessRights.None;

            globalDataItem.BatchRead();

            dataItemOneControllerOne.Received().BatchRead();
            dataItemOneControllerTwo.DidNotReceive().BatchRead();
            dataItemOneControllerThree.Received().BatchRead();
            dataItemOneControllerFour.Received().BatchRead();
        }

        [Test]
        public void CallingBatchWriteOnGlobalDataItemWillCallBatchWriteOnDataItemsWithWriteAndNoneAccess()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.None;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.None;

            globalDataItem.BatchWrite(10);

            dataItemOneControllerOne.Received().BatchWrite(10);
            dataItemOneControllerTwo.Received().BatchWrite(10);
            dataItemOneControllerThree.Received().BatchWrite(10);
        }

        [Test]
        public void CallingBatchWriteOnGlobalDataItemWithReadAccessWillFireAccessDenied()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { AccessRight = AccessRights.Read };

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            bool success = globalDataItem.BatchWrite(10);

            Assert.IsFalse(success);
            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void CallingBatchWriteForDataExchangeOnGlobalDataItemWillOnlyCallBatchWriteOnDataItemsWithWriteAccess()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            IDataSourceContainer controllerFour = null;
            IDataItem dataItemOneControllerFour = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerFour, out dataItemOneControllerFour, "Controller4", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);
            globalDataItem.DataItems.Add(dataItemOneControllerFour);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            globalDataItem.AccessRights[controllerFour.Name] = AccessRights.None;

            globalDataItem.BatchWriteForDataExchange(10);

            dataItemOneControllerOne.DidNotReceive().BatchWrite(10);
            dataItemOneControllerTwo.Received().BatchWrite(10);
            dataItemOneControllerThree.Received().BatchWrite(10);
            dataItemOneControllerFour.DidNotReceive().BatchWrite(10);
        }

        [Test]
        public void CallingBatchWriteForDataExchangeOnGlobalDataItemWithReadAccessWillNotFireAccessDenied()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { AccessRight = AccessRights.Read };

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            bool success = globalDataItem.BatchWriteForDataExchange(10);

            Assert.IsTrue(success);
            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void CallingBatchWriteForDataExchangeOnGlobalDataItemAsInternalVariableWillUpdateValueAndFireValueChange()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            globalDataItem.BatchWriteForDataExchange(10);

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void CallingBatchWriteForDataExchangeOnGlobalDataItemAsInternalVariableWillNotFireValueChangeWhenUpdatingToSameValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(10);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            globalDataItem.BatchWriteForDataExchange(10);

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWillUpdateTriggerValueInGlobalDataItemEvenWhenAccessRightIsRead()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);
            globalDataItem.Trigger = new DataTrigger();

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(10), globalDataItem.TriggerValue);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithNoneAccessWillUpdateValueInGlobalDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.None, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithReadAccessWillUpdateValueInGlobalDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithReadWriteAccessWillUpdateValueInGlobalDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithWriteAccessWillNotUpdateValueInGlobalDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Write, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in underlying data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithReadAccessWillNotUpdateValueInGlobalDataItemWithWriteAccess()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            bool wasRaised = false;
            globalDataItem.ValueChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in data item.
            dataItem.Value = new VariantValue(10);
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithReadAccessWillOnlyUpdateValueInOtherUnderlyingDataItemsSetupForDataExchange()
        {
            IDataSourceContainer controllerOne = Substitute.For<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
            IDataItem dataItemOneControllerOne = Substitute.For<IDataItem>();
            dataItemOneControllerOne.DataSourceContainer.Returns(controllerOne);
            // Value on dataitem, which raised the value change event, should not have its value set again.

            IDataSourceContainer controllerTwo = Substitute.For<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
            IDataItem dataItemOneControllerTwo = Substitute.For<IDataItem>();
            dataItemOneControllerTwo.DataSourceContainer.Returns(controllerTwo);

            IDataSourceContainer controllerThree = Substitute.For<IDataSourceContainer>();
            controllerThree.Name = "Controller3";
            IDataItem dataItemOneControllerThree = Substitute.For<IDataItem>();
            dataItemOneControllerThree.DataSourceContainer.Returns(controllerThree);

            IDataSourceContainer controllerFour = Substitute.For<IDataSourceContainer>();
            controllerFour.Name = "Controller4";
            IDataItem dataItemOneControllerFour = Substitute.For<IDataItem>();
            dataItemOneControllerFour.DataSourceContainer.Returns(controllerFour);

            IDataSourceContainer controllerFive = Substitute.For<IDataSourceContainer>();
            controllerFive.Name = "Controller5";
            IDataItem dataItemOneControllerFive = Substitute.For<IDataItem>();
            dataItemOneControllerFive.DataSourceContainer.Returns(controllerFive);

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);
            globalDataItem.DataItems.Add(dataItemOneControllerFour);
            globalDataItem.DataItems.Add(dataItemOneControllerFive);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.None;
            globalDataItem.AccessRights[controllerFour.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerFive.Name] = AccessRights.ReadWrite;

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerOne, new ValueChangedEventArgs(variantValue));

            dataItemOneControllerOne.DidNotReceive().Value = null;
            dataItemOneControllerTwo.Received(1).Value = null;
            dataItemOneControllerThree.DidNotReceive().Value = null;
            dataItemOneControllerFour.DidNotReceive().Value = null;
            dataItemOneControllerFive.Received(1).Value = null;
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingDataItemWithNoneAccessWillNotUpdateValueInOtherUnderlyingDataItemsSetupForDataExchange()
        {
            IDataSourceContainer controllerOne = Substitute.For<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
            IDataItem dataItemOneControllerOne = Substitute.For<IDataItem>();
            dataItemOneControllerOne.DataSourceContainer.Returns(controllerOne);

            IDataSourceContainer controllerTwo = Substitute.For<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
            IDataItem dataItemOneControllerTwo = Substitute.For<IDataItem>();
            dataItemOneControllerTwo.DataSourceContainer.Returns(controllerTwo);

            IDataSourceContainer controllerThree = Substitute.For<IDataSourceContainer>();
            controllerThree.Name = "Controller3";
            IDataItem dataItemOneControllerThree = Substitute.For<IDataItem>();
            dataItemOneControllerThree.DataSourceContainer.Returns(controllerThree);

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.None;

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerThree, new ValueChangedEventArgs(variantValue));

            dataItemOneControllerOne.DidNotReceive().Value = null;
            dataItemOneControllerTwo.DidNotReceive().Value = null;
            dataItemOneControllerThree.DidNotReceive().Value = null;
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingReadableDataItemWillUpdateOtherUnderlyingWriteableDataItemsEvenWhenAccessRightForGlobalDataItemsIsWrite()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;

            globalDataItem.AccessRight = AccessRights.Write;

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerOne, new ValueChangedEventArgs(variantValue));

            Assert.AreEqual(new VariantValue(10), dataItemOneControllerTwo.Value);
        }

        [Test]
        public void UpdatingValueOnAnUnderlyingReadableDataItemWillUpdateOtherUnderlyingWriteableDataItemsEvenWhenAccessRightForGlobalDataItemsIsRead()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;

            globalDataItem.AccessRight = AccessRights.Read;

            // Simulate a value change in data item.
            VariantValue variantValue = new VariantValue(10);
            Raise.EventWith(dataItemOneControllerOne, new ValueChangedEventArgs(variantValue));

            Assert.AreEqual(new VariantValue(10), dataItemOneControllerTwo.Value);
        }

        [Test]
        public void AccessDeniedEventIsNotFiredAndValueIsNotSetOnGlobalDataItemWithReadAccessWhenValueIsUpdatedFromAnUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Read;

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            // Simulate a value change in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void AccessDeniedEventOnGlobalDataItemWithReadAccessIsFiredWhenSettingValue()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Read;

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            globalDataItem.Value = new VariantValue(10);

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void AccessDeniedEventOnGlobalDataItemWithReadAccessIsFiredOnBatchWrite()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Read;

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            bool success = globalDataItem.BatchWrite(10);

            Assert.IsTrue(wasRaised);
            Assert.IsFalse(success);
        }

        [Test]
        public void ValueOnGlobalDataItemWithWriteAccessReturnsLastWrittenInternalValue()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            globalDataItem.Value = new VariantValue(10);

            // Simulate a change in data source.
            dataItem.Value = new VariantValue(20);

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
        }

        [Test]
        public void ValueOnGlobalDataItemWithWriteAccessReturnsDefaultValueIfNoValueHasBeenWritten()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
        }

        [Test]
        public void ValueOnEventOnGlobalDataItemIsFiredForNoneAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOn += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOnEventOnGlobalDataItemIsFiredForReadAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.None, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOn += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOnEventOnGlobalDataItemIsFiredForReadWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOn += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
           Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOnEventOnGlobalDataItemIsNotFiredForWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Write, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOn += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void ValueOnEventOnGlobalDataItemIsNotFiredForReadAccessOnValueChangeInUnderlyingDataItemWhenGlobalDataItemHasWriteAccess()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            bool wasRaised = false;
            globalDataItem.ValueOn += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void ValueOffEventOnGlobalDataItemIsFiredForNoneAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.None, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOff += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOffEventOnGlobalDataItemIsFiredForReadAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOff += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOffEventOnGlobalDataItemIsFiredForReadWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOff += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueOffEventOnGlobalDataItemIsNotFiredForWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Write, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueOff += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void ValueOffEventOnGlobalDataItemIsNotFiredForReadAccessOnValueChangeInUnderlyingDataItemWhenGlobalDataItemHasWriteAccess()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            bool wasRaised = false;
            globalDataItem.ValueOff += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.Event();

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void ValueChangeOrErrorEventOnGlobalDataItemIsFiredForNoneAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.None, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChangeOrError += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueChangeOrErrorEventOnGlobalDataItemIsFiredForReadAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChangeOrError += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueChangeOrErrorEventOnGlobalDataItemIsFiredForReadWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChangeOrError += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void ValueChangeOrErrorEventOnGlobalDataItemIsNotFiredForWriteAccessOnValueChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Write, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.ValueChangeOrError += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void ValueChangeOrErrorEventOnGlobalDataItemIsNotFiredForReadAccessOnValueChangeInUnderlyingDataItemWhenGlobalDataItemHasWriteAccess()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.Read, out globalDataItem, out dataItem);
            globalDataItem.AccessRight = AccessRights.Write;

            bool wasRaised = false;
            globalDataItem.ValueChangeOrError += (sender, eventArgs) => wasRaised = true;

            // Simulate a value on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10)));

            Assert.IsFalse(wasRaised);
        }

        [Test]
        public void QualityChangeOnGlobalDataItemIsFiredForQualityChangeInUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            bool wasRaised = false;
            globalDataItem.QualityChange += (sender, eventArgs) => wasRaised = true;

            // Simulate a quality on in data item.
            Raise.EventWith(dataItem, new ValueChangedEventArgs(new VariantValue(10, DataQuality.Bad)));

            Assert.IsTrue(wasRaised);
        }

        private void CreateDataItemWithSpecifiedAccessRight(AccessRights accessRight, out IGlobalDataItem globalDataItem, out IDataItem dataItem)
        {
            IDataSourceContainer controller = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "Controller1", "DataItem1");

            globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = accessRight;
        }

        #endregion

        #region Actions

        [Test]
        public void CallingIncrementAnalogOnReadonlyGlobalDataItemFiresAccessDeniedAndDoesNotIncrementValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem() { AccessRight = AccessRights.Read };

            bool wasRaised = false;
            globalDataItem.AccessDenied += (sender, eventArgs) => wasRaised = true;

            globalDataItem.IncrementAnalog(5);

            Assert.IsTrue(wasRaised);
            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
        }

        [Test]
        public void CallingIncrementAnalogOnGlobalDataItemIncrementsRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(15);
            CallAction(globalDataItem, dataItem => dataItem.IncrementAnalog(5), 10, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingDecrementAnalogOnGlobalDataItemDecrementsRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(5);
            CallAction(globalDataItem, dataItem => dataItem.DecrementAnalog(5), 10, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingSetAnalogOnGlobalDataItemSetsAnalogRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(10);
            CallAction(globalDataItem, dataItem => globalDataItem.SetAnalog(10), 0, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingSetTagOnGlobalDataItemSetsRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(1);
            CallAction(globalDataItem, dataItem => dataItem.SetTag(), 0, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingResetTagOnGlobalDataItemResetsRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(0);
            CallAction(globalDataItem, dataItem => dataItem.ResetTag(), 1, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingToggleTagOnGlobalDataItemTogglesValueFromZeroToOneRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(1);
            CallAction(globalDataItem, dataItem => dataItem.ToggleTag(), 0, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingToggleTagOnGlobalDataItemTogglesValueFromOneToZeroRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            VariantValue resultValue = new VariantValue(0);
            CallAction(globalDataItem, dataItem => dataItem.ToggleTag(), 1, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingSetStringOnGlobalDataItemSetsStringRecursively()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem() { DataType = BEDATATYPE.DT_STRING };

            VariantValue resultValue = new VariantValue("Ooh my!");
            CallAction(globalDataItem, dataItem => dataItem.SetString("Ooh my!"), string.Empty, resultValue, false);

            Assert.AreEqual(resultValue, globalDataItem.Value);
        }

        [Test]
        public void CallingIncrementAnalogOnGlobalDataItemAsInternalVariableIncrementsValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);

            globalDataItem.IncrementAnalog(10);

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
        }

        [Test]
        public void CallingDecrementAnalogOnGlobalDataItemAsInternalVariableDecrementsValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);

            globalDataItem.DecrementAnalog(10);

            Assert.AreEqual(new VariantValue(-10), globalDataItem.Value);
        }

        [Test]
        public void CallingSetAnalogOnGlobalDataItemAsInternalVariableSetsAnalogValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);

            globalDataItem.SetAnalog(10);

            Assert.AreEqual(new VariantValue(10), globalDataItem.Value);
        }

        [Test]
        public void CallingSetTagOnGlobalDataItemAsInternalVariableSetsValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);

            globalDataItem.SetTag();

            Assert.AreEqual(new VariantValue(1), globalDataItem.Value);
        }

        [Test]
        public void CallingResetTagOnGlobalDataItemAsInternalVariableResetsValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(1);

            globalDataItem.ResetTag();

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
        }

        [Test]
        public void CallingToggleTagOnGlobalDataItemAsInternalVariableTogglesValueFromZeroToOne()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(0);

            globalDataItem.ToggleTag();

            Assert.AreEqual(new VariantValue(1), globalDataItem.Value);
        }

        [Test]
        public void CallingToggleTagOnGlobalDataItemAsInternalVariableTogglesValueFromOneToZero()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Value = new VariantValue(1);

            globalDataItem.ToggleTag();

            Assert.AreEqual(new VariantValue(0), globalDataItem.Value);
        }

        [Test]
        public void CallingSetStringOnGlobalDataItemAsInternalVariableSetsStringValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem() { DataType = BEDATATYPE.DT_STRING };


            globalDataItem.SetString("Ooh my, ooh my!");

            Assert.AreEqual(new VariantValue("Ooh my, ooh my!"), globalDataItem.Value);
        }

        private void CallAction(IGlobalDataItem globalDataItem, Action<IGlobalDataItem> action, object initialValue, VariantValue result, bool requiresRead)
        {
            SetupDefaultSetOfControllersAndDataItems(globalDataItem);

            globalDataItem.Value = initialValue;

            action(globalDataItem);

            if (requiresRead)
            {
                globalDataItem.DataItems[0].Received().Read();
                globalDataItem.DataItems[1].DidNotReceive().Read();
                globalDataItem.DataItems[2].Received().Read();
                globalDataItem.DataItems[3].Received().Read();
            }
            else
            {
                globalDataItem.DataItems.ToList().ForEach(dataItem => dataItem.DidNotReceive().Read());
            }

        }

        #endregion

        #region Activate dataitems

        [Test]
        public void GlobalDataItemsWithAtLeastOneReadAndOneWriteWillOnlyMakeDataItemsWithReadAccessActive()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            IDataSourceContainer controllerFour = null;
            IDataItem dataItemOneControllerFour = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerFour, out dataItemOneControllerFour, "Controller4", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);
            globalDataItem.DataItems.Add(dataItemOneControllerThree);
            globalDataItem.DataItems.Add(dataItemOneControllerFour);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            globalDataItem.AccessRights[controllerFour.Name] = AccessRights.None;

            ((IStartup)globalDataItem).Run();

            Assert.AreEqual(BEACTIVETYPE.ACTIVE_TRUE, dataItemOneControllerOne.ActiveState);
            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerTwo.ActiveState);
            Assert.AreEqual(BEACTIVETYPE.ACTIVE_TRUE, dataItemOneControllerThree.ActiveState);
            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerFour.ActiveState);
        }

        [Test]
        public void GlobalDataItemsWithOnlyOneReadWillNotActivateDataItem()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;

            ((IStartup)globalDataItem).Run();

            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerOne.ActiveState);
        }

        [Test]
        public void GlobalDataItemsWithMultipleReadWillNotActivateDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Read;

            ((IStartup)globalDataItem).Run();

            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerOne.ActiveState);
            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerTwo.ActiveState);
        }

        [Test]
        public void GlobalDataItemsWithOneWriteWillNotActivateDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Write;

            ((IStartup)globalDataItem).Run();

            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerOne.ActiveState);
        }

        [Test]
        public void GlobalDataItemsWithMultipleWritesWillNotActivateDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;

            ((IStartup)globalDataItem).Run();

            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerOne.ActiveState);
            Assert.AreEqual(BEACTIVETYPE.ACTIVE_FALSE, dataItemOneControllerTwo.ActiveState);
        }

        #endregion

        #region Disposing global dataitems

        [Test]
        public void RemovingGlobalDataItemAlsoRemovesItsDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItemOneControllerOne);
            globalDataItem[0].DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.Dispose();

            controllerOne.Received().RemoveDataItem(dataItemOneControllerOne);
            controllerTwo.Received().RemoveDataItem(dataItemOneControllerTwo);
            Assert.AreEqual(0, globalDataItem[0].DataItems.Count);
        }

        #endregion

        #region Dataitem counting

        [Test]
        public void AddingTheFirstUnderlyingDataItemWhenNotInGlobalControllerDoesNotIncreaseDataItemCount()
        {
            IDataItem dataItem = Substitute.For<IDataItem>();
            GlobalDataItem globalDataItem = new GlobalDataItem();

            globalDataItem[0].DataItems.Add(dataItem);

            m_DataItemCountingService.Received(1).AddConnectedDataItems(1);
        }

        [Test]
        public void AddingGlobalDataItemWithAtLeastOneUnderlyingDataItemToGlobalControllerIncreasesDataItemCount()
        {
            IDataItem dataItem = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItem);

            globalDataItem.Controller = Substitute.For<IGlobalController>();

            m_DataItemCountingService.Received(1).AddConnectedDataItems(1);
        }

        [Test]
        public void AddingTheFirstUnderlyingDataItemWhenInGlobalControllerIncreasesDataItemCount()
        {
            IDataItem dataItem = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();

            globalDataItem.DataItems.Add(dataItem);

            m_DataItemCountingService.Received(1).AddConnectedDataItems(1);
        }

        [Test]
        public void AddingTheSecondUnderlyingDataItemWhenInGlobalControllerDoesNotIncreaseDataItemCount()
        {
            IDataItem dataItemOne = Substitute.For<IDataItem>();
            IDataItem dataItemTwo = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();
            globalDataItem.DataItems.Add(dataItemOne);

            globalDataItem.DataItems.Add(dataItemTwo);

            m_DataItemCountingService.Received(1).AddConnectedDataItems(1);
        }

        [Test]
        public void RemovingTheLastUnderlyingDataItemWhenNotInGlobalControllerDoesNotDecreaseDataItemCount()
        {
            IDataItem dataItem = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItem);

            globalDataItem.DataItems.Remove(dataItem);

            m_DataItemCountingService.DidNotReceive().RemoveConnectedDataItems(1);
        }

        [Test]
        public void RemovingTheLastUnderlyingDataItemWhenInGlobalControllerDecreasesDataItemCount()
        {
            IDataItem dataItem = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();
            globalDataItem.DataItems.Add(dataItem);

            globalDataItem.DataItems.Remove(dataItem);

            m_DataItemCountingService.Received().RemoveConnectedDataItems(1);
        }

        [Test]
        public void RemovingTheSecondLastUnderlyingDataItemWhenInGlobalControllerDoesNotDecreaseDataItemCount()
        {
            IDataItem dataItemOne = Substitute.For<IDataItem>();
            IDataItem dataItemTwo = Substitute.For<IDataItem>();
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();
            globalDataItem.DataItems.Add(dataItemOne);
            globalDataItem.DataItems.Add(dataItemTwo);

            globalDataItem.DataItems.Remove(dataItemTwo);

            m_DataItemCountingService.DidNotReceive().RemoveConnectedDataItems(1);
        }

        [Test]
        public void DisposingGlobalDataItemWithNoUnderlyingDataItemsWhenInGlobalControllerDoesNotDecreaseDataItemCount()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();

            globalDataItem.Dispose();

            m_DataItemCountingService.DidNotReceive().RemoveConnectedDataItems(1);
        }

        [Test]
        public void DisposingGlobalDataItemWithOneUnderlyingDataItemWhenNotInGlobalControllerDoesNotDecreaseDataItemCount()
        {
            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataItems.Add(dataItem);

            globalDataItem.Dispose();

            m_DataItemCountingService.DidNotReceive().RemoveConnectedDataItems(1);
        }

        [Test]
        public void DisposingGlobalDataItemWithOneUnderlyingDataItemWhenInGlobalControllerDoesDecreaseDataItemCount()
        {
            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();
            globalDataItem.DataItems.Add(dataItem);

            globalDataItem.Dispose();

            m_DataItemCountingService.Received(1).RemoveConnectedDataItems(1);
        }

        [Test]
        public void DisposingGlobalDataItemWithMoreThanOneUnderlyingDataItemWhenInGlobalControllerDoesOnlyDecreaseDataItemCountOnce()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Controller = Substitute.For<IGlobalController>();
            globalDataItem.DataItems.Add(dataItemOneControllerOne);
            globalDataItem.DataItems.Add(dataItemOneControllerTwo);

            globalDataItem.Dispose();

            m_DataItemCountingService.Received(1).RemoveConnectedDataItems(1);
        }

        #endregion

        #region Poll groups

        [Test]
        public void PollGroupNameReturnsEmptyStringWhenNoPollGroupsExist()
        {
            IGlobalController globalController = Substitute.For<IGlobalController>();
            globalController.PollGroups.Returns(new BindingList<IPollGroup>());

            GlobalDataItem globalDataItem = new GlobalDataItem();
            ((IGlobalDataItem)globalDataItem).Controller = globalController;

            Assert.AreEqual(string.Empty, globalDataItem.PollGroupName);
        }

        [Test]
        public void PollGroupNameReturnsPreviouslySetPollGroup()
        {
            IPollGroup pollGroup = Substitute.For<IPollGroup>();
            pollGroup.Name = "PollGroup2";

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.PollGroup = pollGroup;

            Assert.AreEqual("PollGroup2", globalDataItem.PollGroupName);
        }

        [Test]
        public void PollGroupNameReturnsNameOfPollGroupWhenSetToOtherValue()
        {
            IPollGroup pollGroup = Substitute.For<IPollGroup>();
            pollGroup.Name = "PollGroup2";

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.PollGroup = pollGroup;
            globalDataItem.PollGroupName = "SomethingElse";

            Assert.AreEqual("PollGroup2", globalDataItem.PollGroupName);
        }

        [Test]
        public void PollGroupNameReturnsFirstExistingPollGroupWhenNoneHasBeenExplicitlySet()
        {
            IPollGroup pollGroup = Substitute.For<IPollGroup>();
            pollGroup.Name = "PollGroup1";

            IGlobalController globalController = Substitute.For<IGlobalController>();
            globalController.PollGroups.Returns(new BindingList<IPollGroup>() { pollGroup });

            GlobalDataItem globalDataItem = new GlobalDataItem();
            ((IGlobalDataItem)globalDataItem).Controller = globalController;

            Assert.AreEqual("PollGroup1", globalDataItem.PollGroupName);
        }

        [Test]
        public void PollGroupIsSetFromNameWhenCallingInitPollGroup()
        {
            IPollGroup firstPollGroup = Substitute.For<IPollGroup>();
            firstPollGroup.Name = "PollGroup1";

            IPollGroup secondPollGroup = Substitute.For<IPollGroup>();
            secondPollGroup.Name = "PollGroup2";

            IGlobalController globalController = Substitute.For<IGlobalController>();
            globalController.PollGroups.Returns(new BindingList<IPollGroup>() { firstPollGroup, secondPollGroup });

            GlobalDataItem globalDataItem = new GlobalDataItem();
            ((IGlobalDataItem)globalDataItem).Controller = globalController;
            globalDataItem.PollGroupName = secondPollGroup.Name;

            globalDataItem.InitPollGroup();

            Assert.IsNotNull(globalDataItem.PollGroup);
            Assert.AreEqual("PollGroup2", globalDataItem.PollGroup.Name);
        }

        #endregion

        #region Global Data Type

        [Test]
        public void GlobalDataTypeReturnsDefaultForNewGlobalDataItems()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem();

            Assert.That(globalDataItem.GlobalDataType, Is.EqualTo(BEDATATYPE.DT_DEFAULT));
        }

        [Test]
        public void GlobalDataTypeReturnsDefaultEvenWhenDataTypeIsSpecific()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2 };

            Assert.That(globalDataItem.GlobalDataType, Is.EqualTo(BEDATATYPE.DT_DEFAULT));
        }

        [Test]
        public void GlobalDataTypeOrDataTypeIfDefaultReturnsValueOfDataTypePropertyWhenDefault()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2 };

            Assert.That(globalDataItem.GlobalDataTypeOrDataTypeIfDefault, Is.EqualTo(globalDataItem.DataType));
        }

        [Test]
        public void GlobalDataTypeOrDataTypeIfDefaultReturnsItsOwnDataTypeWhenNotDefault()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2, GlobalDataType = BEDATATYPE.DT_REAL4 };

            Assert.That(globalDataItem.GlobalDataTypeOrDataTypeIfDefault, Is.EqualTo(BEDATATYPE.DT_REAL4));
        }

        [Test]
        public void ValueReturnsTheDataTypedValueWhenNoGlobalDataTypeHasBeenSet()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2 };

            Assert.That(globalDataItem.Value.Value, Is.TypeOf(typeof(short)));
        }

        [Test]
        public void ValueReturnsTheGlobalDataTypedValueWhenExplicitlySet()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2, GlobalDataType = BEDATATYPE.DT_REAL4 };

            Assert.That(globalDataItem.Value.Value, Is.TypeOf(typeof(float)));
        }

        [Test]
        public void ValueIsSetToTheDataTypedValueWhenNoGlobalDataTypeHasBeenSet()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2 };

            globalDataItem.Value = 10.5;

            Assert.That(globalDataItem.InternalValue.Value, Is.TypeOf(typeof(short)));
            Assert.That(globalDataItem.InternalValue.Value, Is.EqualTo(10));
        }

        [Test]
        public void ValueIsSetToTheGlobalDataTypedValueWhenExplicitlySet()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2, GlobalDataType = BEDATATYPE.DT_REAL4 };

            globalDataItem.Value = 10.5;

            Assert.That(globalDataItem.InternalValue.Value, Is.TypeOf(typeof(float)));
            Assert.That(globalDataItem.InternalValue.Value, Is.EqualTo(10.5));
        }

        [Test]
        public void ValueCanBeRetrievedAsStringWhenDataTypeIsShort()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_STRING;

            VariantValue variantValue = new VariantValue(10.5);
            dataItem.Value = variantValue;
            Raise.EventWith(dataItem, new ValueChangedEventArgs(variantValue));

            variantValue = (VariantValue)globalDataItem.Value;
            Assert.That(variantValue.Value, Is.TypeOf(typeof(string)));
            // JRS: Using ToString here to avoid issues when testing on systems whose culture is not Swedish
            Assert.That(variantValue.Value, Is.EqualTo(10.5.ToString()));
        }

        [Test]
        public void ValueCanBeRetrievedAsStringWhenDataTypeIsDateTime()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.DataType = BEDATATYPE.DT_DATETIME;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_STRING;

            VariantValue variantValue = new VariantValue(new DateTime(2010, 01, 01));
            dataItem.Value = variantValue;
            Raise.EventWith(dataItem, new ValueChangedEventArgs(variantValue));

            variantValue = (VariantValue)globalDataItem.Value;
            Assert.That(variantValue.Value, Is.TypeOf(typeof(string)));
            // JRS: Using DateTime.ToString here to avoid issues when testing on systems whose culture is not Swedish
            Assert.That(variantValue.Value, Is.EqualTo(new DateTime(2010, 1, 1).ToString()));
        }

        [Test]
        public void ValueIsDataTypedBeforeBeingWrittenToUnderlyingDataItem()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;

            globalDataItem.Value = 10.5;

            VariantValue variantValue = (VariantValue)dataItem.Value;
            Assert.That(variantValue.Value, Is.TypeOf(typeof(short)));
            Assert.That(variantValue.Value, Is.EqualTo(10));
        }

        [Test]
        public void TriggerValueReturnsDataTypedValueInsteadOfGlobalDataTypedValue()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { DataType = BEDATATYPE.DT_INTEGER2, GlobalDataType = BEDATATYPE.DT_REAL4 };

            globalDataItem.Value = 10.5;

            Assert.That(globalDataItem.TriggerValue.Value, Is.TypeOf(typeof(short)));
            Assert.That(globalDataItem.TriggerValue.Value, Is.EqualTo(10));
        }

        #endregion

        #region Offset and Gain

        [Test]
        public void SettingValueOnGlobalDataItemInversesOffset()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Offset = 100;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 200;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(100)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(200)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesDecimalOffsetAndKeepsInternalValueTypedToInteger()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Offset = 50.5;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 100;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(50)));
            Assert.That(dataItem.Value, Is.EqualTo(new VariantValue(50)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(100)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesDecimalOffsetWhenGlobalDataTypeIsFloatButValueGetsTruncated()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Offset = 50.5;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;

            globalDataItem.Value = 100;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(49.5)));
            Assert.That(dataItem.Value, Is.EqualTo(new VariantValue(50)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(100.5)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesGain()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Gain = 2;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 200;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(100)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(200)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesDecimalGainAndKeepsInternalValueTypedToInteger()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Gain = 1.2;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 10005;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(8338)));
            Assert.That(dataItem.Value, Is.EqualTo(new VariantValue(8338)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(10006)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesDecimalGainWhenGlobalDataTypeIsFloatButValueGetsTruncated()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Gain = 1.2;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL8;

            globalDataItem.Value = 10005;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(8337.5)));
            Assert.That(dataItem.Value, Is.EqualTo(new VariantValue(8338)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(10005.6)));
        }

        [Test]
        public void SettingValueOnGlobalDataItemInversesDecimalGainWhenGlobalDataTypeIsFloat()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Gain = 0.1;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;

            globalDataItem.Value = 105.5;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(1055)));
            Assert.That(dataItem.Value, Is.EqualTo(new VariantValue(1055)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(105.5)));
        }

        [Test]
        public void OffsetAndGainCanBeInversed()
        {
            IDataItem dataItem = null;
            IGlobalDataItem globalDataItem = null;
            CreateDataItemWithSpecifiedAccessRight(AccessRights.ReadWrite, out globalDataItem, out dataItem);

            globalDataItem.Offset = 100;
            globalDataItem.Gain = 2;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

            globalDataItem.Value = 200;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(50)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(200)));
        }

        [Test]
        public void OffsetHasNoEffectWhenSettingValueOnInternalVariable()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { Offset = 100, DataType = BEDATATYPE.DT_INTEGER2 };

            globalDataItem.Value = 200;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(200)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(200)));
        }

        [Test]
        public void GainHasNoEffectWhenSettingValueOnInternalVariable()
        {
            IGlobalDataItem globalDataItem = new GlobalDataItem { Gain = 2, DataType = BEDATATYPE.DT_INTEGER2 };

            globalDataItem.Value = 200;

            Assert.That(globalDataItem.InternalValue, Is.EqualTo(new VariantValue(200)));
            Assert.That(globalDataItem.Value, Is.EqualTo(new VariantValue(200)));
        }

        #endregion

        #region ArrayTags

        [Test]
        public void GettingValueReturnsUnknownValueQualityWhenItsAnArrayTag()
        {
            VariantValue expectedValue = new VariantValue(0, DataQuality.Unknown);

            GlobalDataItem arrayTag = new GlobalDataItem();
            arrayTag.ArraySize = 2;

            Assert.That(arrayTag.Value, Is.EqualTo(expectedValue), "The value from the array tag was not the expected one!");
        }

        [Test]
        public void GettingValueReturnsValueWhenItsNotAnArrayTag()
        {
            VariantValue expectedValue = new VariantValue(10, DataQuality.Good);

            GlobalDataItem regularTag = new GlobalDataItem { Value = expectedValue };

            Assert.That(regularTag.Value, Is.EqualTo(expectedValue), "The value from the regular tag was not the expected one!");
        }

        [Test]
        public void CanSetValueOnANonArrayTag()
        {
            VariantValue expectedValue = new VariantValue(10, DataQuality.Good);

            GlobalDataItem regularTag = new GlobalDataItem { Value = expectedValue };

            Assert.That(regularTag[0].Value, Is.EqualTo(expectedValue), "The value from the regular tag was not the expected one!");
        }

        [Test]
        public void CantSetValueOnAArrayTag()
        {
            VariantValue valueThatShouldNotBeSet = new VariantValue(999, DataQuality.Unknown);
            VariantValue originalValue = new VariantValue(1, DataQuality.Good);

            GlobalDataItem arrayTag = SetupInternalArrayTag(new[] { originalValue, originalValue });
            arrayTag[0].Value = originalValue;
            arrayTag[1].Value = originalValue;

            arrayTag.Value = valueThatShouldNotBeSet;

            Assert.That(arrayTag[0].Value, Is.EqualTo(originalValue), "The value from the array tag was somehow set! It should not have been set! Can't alter value directly on an array tag");
            Assert.That(arrayTag[1].Value, Is.EqualTo(originalValue), "The value from the array tag was somehow set! It should not have been set! Can't alter value directly on an array tag");
        }

        [Test]
        public void ANormalTagIsAnArrayTagWithLength1AfterConstruction()
        {
            //A normal tag is really a array index of length 
            Assert.That(new GlobalDataItem().ArraySize, Is.EqualTo(1));
        }

        [Test]
        public void ValuesPropertyReturnsAllArrayValuesForInternalArrayTag()
        {
            IEnumerable<VariantValue> expectedValues = new[] { new VariantValue(111), new VariantValue(222), new VariantValue(333) };
            GlobalDataItem globalDataItemArray = SetupInternalArrayTag(expectedValues);

            IEnumerable<VariantValue> actualValues = globalDataItemArray.Values;

            Assert.That(actualValues, Is.EquivalentTo(expectedValues));
        }

        private static GlobalDataItem SetupInternalArrayTag(IEnumerable<VariantValue> values)
        {
            GlobalDataItem globalDataItemArray = new GlobalDataItem();
            globalDataItemArray.ArraySize = (short)values.Count();

            //Add GlobalDataSubItem manually due to no real OpcClientServiceCF
            for (int index = 1; index < globalDataItemArray.ArraySize; index++)
            {
                globalDataItemArray.GlobalDataSubItems.Add(new GlobalDataSubItem());
            }

            for (int i = 0; i < globalDataItemArray.ArraySize; i++)
            {
                globalDataItemArray[i].Value = values.ToArray()[i];
            }
            return globalDataItemArray;
        }

        [Test]
        public void IndexerPropertyOnArrayTagReturnsCorrectArrayValue()
        {
            VariantValue expectedValueOnFirstDataItem = 1;
            VariantValue expectedValueOnSecondDataItem = 2;
            VariantValue expectedValueOnThirdDataItem = 3;

            IDataSourceContainer controller;
            IDataItem firstDataItem = null;
            IDataItem secondDataItem = null;
            IDataItem thirdDataItem = null;
            GlobalDataItem firstGlobalDataItemTag;
            GlobalDataItem secondGlobalDataItemTag;
            GlobalDataItem thirdGlobalDataItemTag;
            IGlobalDataItem globalDataItemArrayTag;

            ControllerHelper.CreateStubController(out controller, "Controller1");
            CreateDataItemsInController(controller, ref firstDataItem, ref secondDataItem, ref thirdDataItem);
            CreateControllerArrayTag(out globalDataItemArrayTag, firstDataItem, secondDataItem, thirdDataItem);
            CreateInternalTags(firstDataItem, secondDataItem, thirdDataItem, out firstGlobalDataItemTag, out secondGlobalDataItemTag, out thirdGlobalDataItemTag);

            firstGlobalDataItemTag.Value = expectedValueOnFirstDataItem;
            secondGlobalDataItemTag.Value = expectedValueOnSecondDataItem;
            thirdGlobalDataItemTag.Value = expectedValueOnThirdDataItem;

            Assert.That(globalDataItemArrayTag[0].Value, Is.EqualTo(expectedValueOnFirstDataItem));
            Assert.That(globalDataItemArrayTag[1].Value, Is.EqualTo(expectedValueOnSecondDataItem));
            Assert.That(globalDataItemArrayTag[2].Value, Is.EqualTo(expectedValueOnThirdDataItem));
        }

        private static void CreateDataItemsInController(IDataSourceContainer controller, ref IDataItem firstDataItem, ref IDataItem secondDataItem, ref IDataItem thirddDataItem)
        {
            ControllerHelper.CreateStubDataItemInStubController(controller, out firstDataItem, "DataItem1");
            ControllerHelper.CreateStubDataItemInStubController(controller, out secondDataItem, "DataItem2");
            ControllerHelper.CreateStubDataItemInStubController(controller, out thirddDataItem, "DataItem3");
        }

        private static void CreateInternalTags(IDataItem firstDataItem, IDataItem secondDataItem, IDataItem thirddDataItem, out GlobalDataItem firstGlobalDataItemTag, out GlobalDataItem secondGlobalDataItemTag, out GlobalDataItem thirdGlobalDataItemTag)
        {
            firstGlobalDataItemTag = new GlobalDataItem();
            secondGlobalDataItemTag = new GlobalDataItem();
            thirdGlobalDataItemTag = new GlobalDataItem();
            firstGlobalDataItemTag.DataItems.Add(firstDataItem);
            secondGlobalDataItemTag.DataItems.Add(secondDataItem);
            thirdGlobalDataItemTag.DataItems.Add(thirddDataItem);
        }

        public static void CreateControllerArrayTag(out IGlobalDataItem globalDataItemArrayTag, IDataItem firstDataItem, IDataItem secondDataItem, IDataItem thirdDataItem)
        {
            globalDataItemArrayTag = new GlobalDataItem();
            globalDataItemArrayTag.ArraySize = 3;

            globalDataItemArrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem());
            globalDataItemArrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem());

            globalDataItemArrayTag[0].DataItems.Add(firstDataItem);
            globalDataItemArrayTag[1].DataItems.Add(secondDataItem);
            globalDataItemArrayTag[2].DataItems.Add(thirdDataItem);
        }
        #endregion

        #region AuditTrail

        [Test]
        public void CallingActionsOnGlobalDataItemLogsToAuditTrail()
        {
            CallingAllActionsOnGlobalDataItemLogsToAuditTrail(false);
        }

        [Test]
        public void CallingActionsOnGlobalDataItemAsInternalVariableLogsToAuditTrail()
        {
            CallingAllActionsOnGlobalDataItemLogsToAuditTrail(true);
        }

        private void CallingAllActionsOnGlobalDataItemLogsToAuditTrail(bool isInternalVariable)
        {
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.IncrementAnalog(5), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.DecrementAnalog(5), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.SetAnalog(5), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.SetTag(), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.ResetTag(), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.ToggleTag(), isInternalVariable);
            CallingActionOnGlobalDataItemLogsToAuditTrail(dataItem => dataItem.SetString("Ooh my!"), isInternalVariable);
        }

        private void CallingActionOnGlobalDataItemLogsToAuditTrail(Expression<Action<IGlobalDataItem>> expression, bool isInternalVariable)
        {
            // Arrange
            var auditTrailService = Substitute.For<IAuditTrailService>();
            TestHelper.AddService(auditTrailService);

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.LogToAuditTrail = true;
            if (isInternalVariable)
            {
                globalDataItem.Value = new VariantValue(0);
            }
            else
            {
                SetupDefaultSetOfControllersAndDataItems(globalDataItem);
            }

            // Act
            Action<IGlobalDataItem> globalDataItemAction = expression.Compile();
            globalDataItemAction(globalDataItem);

            // Assert
            var methodCallExp = (MethodCallExpression)expression.Body;
            string methodName = methodCallExp.Method.Name;
            string errorMessage = string.Format("GlobalDataItem.{0} didn't log to AuditTrail", methodName);

            auditTrailService.ReceivedWithAnyArgs(1).LogDataItemChanged(null, null, null, null);
            TestHelper.RemoveService<IAuditTrailService>();
        }

        #endregion

        private static void SetupDefaultSetOfControllersAndDataItems(IGlobalDataItem globalDataItem)
        {
            ControllerHelper.CreateStubControllerWithDataItem(globalDataItem, "Controller1", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(globalDataItem, "Controller2", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(globalDataItem, "Controller3", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(globalDataItem, "Controller4", "DataItem1");

            globalDataItem.AccessRights["Controller1"] = AccessRights.Read;
            globalDataItem.AccessRights["Controller2"] = AccessRights.Write;
            globalDataItem.AccessRights["Controller3"] = AccessRights.ReadWrite;
            globalDataItem.AccessRights["Controller4"] = AccessRights.None;
        }
    }
}
