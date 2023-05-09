using System;
using Core.Api.DataSource;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class DataItemLogicTest
    {
        private DataItemLogic m_DataItemLogic;
        private IOpcClientServiceIde m_OpcClientServiceCF;
        
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(false);

            m_OpcClientServiceCF = TestHelper.AddServiceStub<IOpcClientServiceIde>();

            m_DataItemLogic = new DataItemLogic();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.ClearServices();
        }

        #region Access rights

        [Test]
        public void AddingDataItemToGlobalDataItemRecreateTheArrayTags()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller1", "DataItem1");

            IGlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItemOne);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItemOne, true);


            m_OpcClientServiceCF.AssertWasCalled(x => x.CreateArrayTags(globalDataItem));
        }

        [Test]
        public void AddingDataItemToAGlobalSystemDataItemSetsTheDataItemsDataTypeToTheTypeInTheSystemDataItem ()
        {
            BEDATATYPE dataTypeOfSystemDataItem = BEDATATYPE.DT_BOOLEAN;
            BEDATATYPE dataTypeOfTheDataItem = BEDATATYPE.DT_DATETIME;

            SystemDataItem systemDataItem = new SystemDataItem {DataType = dataTypeOfSystemDataItem};
            IDataItem dataItem = CreateDataItemWithController("", "");
            dataItem.DataType = dataTypeOfTheDataItem;

            m_DataItemLogic.ItemAddedToGlobalDataItem(systemDataItem, dataItem, true);
          
            Assert.That(dataItem.DataType, Is.EqualTo(dataTypeOfSystemDataItem), "The data type in the data item was not the same as in the system data item!");

        }

        [Test]
        public void AddingDataItemToAGlobalDataItemWithOneDataItemSetsTheGlobalDataItemsTypeToTheTypeInAddedDataItem()
        {
            BEDATATYPE dataTypeOfGlobalDataItem = BEDATATYPE.DT_BOOLEAN;
            BEDATATYPE dataTypeOfTheDataItem = BEDATATYPE.DT_DATETIME;

            GlobalDataItem globalDataItem = new GlobalDataItem { DataType = dataTypeOfGlobalDataItem };
            globalDataItem.DataItems.Add(MockRepository.GenerateStub<IDataItem>());

            IDataItem dataItem = CreateDataItemWithController("", "");
            dataItem.DataType = dataTypeOfTheDataItem;

            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.That(globalDataItem.DataType, Is.EqualTo(dataTypeOfTheDataItem), "The data type in the global data item was not the same as in the added data item!");
        }

        [Test]
        public void AddingDataItemToAGlobalDataItemSetsTheDataItemsTypeToTheTypeInGlobalDataItem()
        {
            BEDATATYPE dataTypeOfGlobalDataItem = BEDATATYPE.DT_BOOLEAN;
            BEDATATYPE dataTypeOfTheDataItem = BEDATATYPE.DT_DATETIME;

            GlobalDataItem globalDataItem = new GlobalDataItem { DataType = dataTypeOfGlobalDataItem };
            IDataItem dataItem = CreateDataItemWithController("", "");
            dataItem.DataType = dataTypeOfTheDataItem;

            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.That(dataItem.DataType, Is.EqualTo(dataTypeOfGlobalDataItem), "The data type in the added data item was not the same as in the global data item!");
        }

        [Test]
        public void RemovingDataItemsFromGlobalDataItemRemovesDataItemNamesAndAccessRights()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller1", "DataItem1");
            IDataItem dataItemTwo = CreateDataItemWithController("Controller2", "DataItem2");

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItemOne);
            globalDataItem[0].DataItems.Add(dataItemTwo);
            
            globalDataItem[0].DataItems.Remove(dataItemOne);
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItem, true);
            globalDataItem[0].DataItems.Remove(dataItemTwo);
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItem, true);

            Assert.AreEqual(0, globalDataItem.GlobalDataSubItems[0].DataItemNames.Count);
            Assert.AreEqual(0, globalDataItem.AccessRights.Count);
        }

        [Test]
        public void RemovingDataItemsFromGlobalDataUpdatesDataTypeToDefaultIfItDoesNotContainsAnyDataItems()
        {
            BEDATATYPE expectedDefaultDataType = BEDATATYPE.DT_DEFAULT;
            BEDATATYPE originaldataTypeInGlobalDataItems = BEDATATYPE.DT_BOOLEAN;

            GlobalDataItem globalDataItemWithDataItem = new GlobalDataItem();
            globalDataItemWithDataItem.DataType = originaldataTypeInGlobalDataItems;            
            globalDataItemWithDataItem[0].DataItems.Add(CreateDataItemWithController("Controller1", "DataItem1"));
            GlobalDataItem globalDataItemWithNoDataItem = new GlobalDataItem();
            globalDataItemWithNoDataItem.DataType = originaldataTypeInGlobalDataItems;
            
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItemWithDataItem, true);
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItemWithNoDataItem, true);

            Assert.That(globalDataItemWithDataItem.DataType, Is.EqualTo(originaldataTypeInGlobalDataItems), "The data type should not have been changed!");
            Assert.That(globalDataItemWithNoDataItem.DataType, Is.EqualTo(expectedDefaultDataType), "The data type should have been changed to the default data type!");
        }

        [Test]
        public void RemovingDataItemsFromGlobalDataNeverUpdatesDataTypeToDefaultWhenItsASystemDataItem()
        {
            BEDATATYPE originaldataTypeInSystemDataItem = BEDATATYPE.DT_BOOLEAN;
            SystemDataItem systemDataItem = new SystemDataItem("", null);
            systemDataItem.DataType = originaldataTypeInSystemDataItem;

            m_DataItemLogic.ItemRemovedFromGlobalDataItem(systemDataItem, true);

            Assert.That(systemDataItem.DataType, Is.EqualTo(originaldataTypeInSystemDataItem), "The data type should not have been changed!");
        }


        #endregion

        #region Properties

        [Test]
        public void DataItemReceivesSizeFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.Size = 1;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Size = 4;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(4, dataItem.Size);
        }

        [Test]
        public void DataItemReceivesOffsetFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.Offset = 1;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Offset = 4;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(4, dataItem.Offset);
        }

        [Test]
        public void DataItemReceivesGainFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.Gain = 1;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Gain = 4;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(4, dataItem.Gain);
        }

        [Test]
        public void DataItemReceivesLogToAuditTrailFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.Gain = 1;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.LogToAuditTrail = true;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.IsTrue(dataItem.LogToAuditTrail);
        }

        [Test]
        public void DataItemReceivesIndexRegisterNumberFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.Gain = 1;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(1, dataItem.IndexRegisterNumber);
        }

        #endregion

        #region DataType initialization

        [Test]
        public void GlobalDataItemReceivesDataTypeFromFirstValidDataItem()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.DataType = BEDATATYPE.DT_INTEGER4;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, globalDataItem.DataType);
        }

        [Test]
        public void GlobalDataItemDoesNotReceiveDataTypeFromSecondValidDataItem()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller1", "DataItem1");
            dataItemOne.DataType = BEDATATYPE.DT_INTEGER4;
            IDataItem dataItemTwo = CreateDataItemWithController("Controller2", "DataItem1");
            dataItemTwo.DataType = BEDATATYPE.DT_REAL8;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            globalDataItem[0].DataItems.Add(dataItemOne);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItemOne, true);
            globalDataItem[0].DataItems.Add(dataItemTwo);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItemTwo, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, globalDataItem.DataType);
        }

        [Test]
        public void GlobalDataItemResetsDataTypeWhenAllDataItemsAreRemovedAndToldToReset()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_REAL4;
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItem, true);

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, globalDataItem.DataType);
        }

        [Test]
        public void GlobalDataItemDoesntResetDataTypeWhenAllDataItemsAreRemovedAndNotToldToReset()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_REAL4;
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(globalDataItem, false);

            Assert.AreEqual(BEDATATYPE.DT_REAL4, globalDataItem.DataType);
        }

        [Test]
        public void GlobalDataItemKeepsCurrentDataTypeDuringProjectLoad()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.DataType = BEDATATYPE.DT_INTEGER4;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, false);

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, globalDataItem.DataType);
        }

        [Test]
        public void SecondValidDataItemReceivesDataTypeFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller1", "DataItem1");
            dataItemOne.DataType = BEDATATYPE.DT_INTEGER4;
            IDataItem dataItemTwo = CreateDataItemWithController("Controller2", "DataItem1");
            dataItemTwo.DataType = BEDATATYPE.DT_REAL8;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            globalDataItem[0].DataItems.Add(dataItemOne);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItemOne, true);
            globalDataItem[0].DataItems.Add(dataItemTwo);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItemTwo, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, dataItemTwo.DataType);
        }

        [Test]
        public void FirstValidDataItemDoesNotReceiveDataTypeFromGlobalDataItemWhenAdded()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.DataType = BEDATATYPE.DT_INTEGER4;

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_REAL4;
            globalDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(globalDataItem, dataItem, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, dataItem.DataType);
        }

        [Test]
        public void SystemDataItemDoesNotReceiveDataTypeFromFirstValidDataItem()
        {
            IDataItem dataItem = CreateDataItemWithController("Controller1", "DataItem1");
            dataItem.DataType = BEDATATYPE.DT_INTEGER4;

            SystemDataItem systemDataItem = new SystemDataItem();
            systemDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            systemDataItem[0].DataItems.Add(dataItem);
            m_DataItemLogic.ItemAddedToGlobalDataItem(systemDataItem, dataItem, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER2, systemDataItem.DataType);
        }

        [Test]
        public void SystemDataItemDoesNotResetDataTypeWhenAllDataItemsAreRemoved()
        {
            SystemDataItem systemDataItem = new SystemDataItem();
            systemDataItem.DataType = BEDATATYPE.DT_REAL4;
            m_DataItemLogic.ItemRemovedFromGlobalDataItem(systemDataItem, true);

            Assert.AreEqual(BEDATATYPE.DT_REAL4, systemDataItem.DataType);
        }

        [Test]
        public void UnderlyingDataItemsReceivesDataTypeFromSystemDataItem()
        {
            IDataItem dataItemOne = CreateDataItemWithController("Controller1", "DataItem1");
            dataItemOne.DataType = BEDATATYPE.DT_INTEGER2;

            IDataItem dataItemTwo = CreateDataItemWithController("Controller2", "DataItem1");
            dataItemTwo.DataType = BEDATATYPE.DT_REAL4;

            SystemDataItem systemDataItem = new SystemDataItem();
            systemDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            systemDataItem[0].DataItems.Add(dataItemOne);
            systemDataItem[0].DataItems.Add(dataItemTwo);
            m_DataItemLogic.ItemAddedToGlobalDataItem(systemDataItem, dataItemOne, true);
            m_DataItemLogic.ItemAddedToGlobalDataItem(systemDataItem, dataItemTwo, true);

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, systemDataItem.DataType);
            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, dataItemOne.DataType);
            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, dataItemTwo.DataType);
        }

        #endregion

        #region Rename controller

        [Test]
        public void RenamingControllerAlsoUpdatesControllerKeyInDictionaries()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.AccessRights["Controller1"] = AccessRights.Read;
            globalDataItem.GlobalDataSubItems[0].DataItemNames["Controller1"] = "DataItem1";

            m_DataItemLogic.ControllerRenamed(globalDataItem, "Controller1", "MyController");

            Assert.AreEqual(1, globalDataItem.AccessRights.Count);
            Assert.AreEqual(1, globalDataItem.GlobalDataSubItems[0].DataItemNames.Count);
            Assert.AreEqual(AccessRights.Read, globalDataItem.AccessRights["MyController"]);
            Assert.AreEqual("DataItem1", globalDataItem.GlobalDataSubItems[0].DataItemNames["MyController"]);
        }

        [Test]
        public void RenamingControllerThrowsExceptionWhenControllerKeyExistsInAccessRightsDictionary()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.AccessRights["MyController"] = AccessRights.Read;

            Assert.Throws<ArgumentException>(() => m_DataItemLogic.ControllerRenamed(globalDataItem, "Controller1", "MyController"));
        }

        [Test]
        public void RenamingControllerThrowsExceptionWhenControllerKeyExistsInDataItemNamesDictionary()
        {
            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.GlobalDataSubItems[0].DataItemNames["MyController"] = "DataItem1";

            Assert.Throws<ArgumentException>(() => m_DataItemLogic.ControllerRenamed(globalDataItem, "Controller1", "MyController"));
        }

        #endregion

        private IDataItem CreateDataItemWithController(string controllerName, string dataItemName)
        {
            IDataSourceContainer controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.Name = controllerName;

            IDataItem dataItem = MockRepository.GenerateStub<IDataItem>();
            dataItem.DataSourceContainer = controller;
            dataItem.Name = dataItemName;

            return dataItem;
        }

    }
}
