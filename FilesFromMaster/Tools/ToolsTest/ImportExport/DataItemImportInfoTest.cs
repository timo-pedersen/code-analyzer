using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Api.Tools;
using Core.Component.Api.Design;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.OpcUaServer;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Tools.MultiLanguage;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Selection;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ImportExport
{
    [TestFixture]
    public class DataItemImportInfoTest
    {
        private IOpcClientServiceIde m_OpcClientService;
        private ExtendedBindingList<IDataSourceContainer> m_Controllers;
        private IGlobalController m_GlobalController;
        private IDataItem m_DataItem;
        private IDesignerHost m_DesignerHost;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IFastLoggingFeatureLogicService>();

            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(false);

            m_Controllers = new ExtendedBindingList<IDataSourceContainer>();

            m_DataItem = MockRepository.GenerateStub<IDataItem>();

            TestHelper.AddServiceStub<ITagChangedNotificationServiceCF>();
            var opcUaServerRootComponent = MockRepository.GenerateStub<IOpcUaServerRootComponent>();
            opcUaServerRootComponent.ExposureOption = OpcUaServerTagExposureOption.AllTagsVisible;
            var projectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            projectItem.Stub(x => x.ContainedObject).Return(opcUaServerRootComponent);
            var projectItemFinder = TestHelper.AddServiceStub<IProjectItemFinder>();
            projectItemFinder.Stub(x => x.GetProjectItems(typeof(IOpcUaServerRootComponent))).Return(new[] { projectItem });

            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_OpcClientService.Stub(x => x.Controllers).Return(m_Controllers);
            m_OpcClientService.Stub(x => x.AddNewDataItem(string.Empty, string.Empty, null)).IgnoreArguments().Return(MockRepository.GenerateStub<IDataItem>());
            m_OpcClientService.Stub(x => x.AddNewDataItem(string.Empty, string.Empty, null, true, 0.0, 0.0, BEDATATYPE.DT_BIT, 0)).IgnoreArguments().Return(m_DataItem);
            m_GlobalController = MockRepository.GenerateStub<IGlobalController>();
            m_GlobalController.Name = "GlobalController";

            IPollGroup pollGroup = MockRepository.GenerateStub<IPollGroup>();
            pollGroup.Name = "DefaultPollGroup";
            m_GlobalController.Stub(x => x.PollGroups).Return(new BindingList<IPollGroup>() { pollGroup });

            m_OpcClientService.Stub(x => x.GlobalController).Return(m_GlobalController);

            m_Controllers.Clear();

            var testSite = new TestSite();
            IDesignerDocument designerDocument = new DesignerDocument(
                testSite,
                MockRepository.GenerateStub<IDesignerPersistenceService>(),
                MockRepository.GenerateStub<System.ComponentModel.Design.Serialization.INameCreationService>().ToILazy(),
                () => new SelectionService(),
                new LazyWrapper<IReferenceProvider>(
                    () => new GlobalReferenceToReferenceAdapter(ServiceContainerCF.GetService<IGlobalReferenceService>())),
                new IDesignerSerializationProvider[] { new CodeDomMultiLanguageProvider(CodeDomLocalizationModel.PropertyReflection) }
            );
            m_DesignerHost = designerDocument.DesignerHost;
            ((IExtenderProviderService)m_DesignerHost).AddExtenderProvider((IExtenderProvider)Activator.CreateInstance(typeof(ExposureExtenderProvider)));
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CreateDataItemInfoForExportOneController()
        {
            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "MyController", "DataItem1");
            dataItem.ItemID = "D0";
            m_Controllers.Add(controller);

            IGlobalDataItem globalDataItem = CreateGlobalDataItem();
            globalDataItem.Name = "Tag1";
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "PollGroup2" };
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
            ExposureExtenderProvider.SetExtendedPropertyValue(globalDataItem, ExposureExtenderProvider.IsExposedPropertyName, true);

            DataItemImportInfo dataItemImportInfo = DataItemImportExportHelper.CreateDataItemImportInfo(globalDataItem, dataItem);
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(dataItemImportInfo);
            Assert.AreEqual("Tag1", dataItemImportInfo.Name);
            Assert.AreEqual(AccessRights.Read, dataItemImportInfo.AccessRight);
            Assert.AreEqual(BEDATATYPE.DT_INTEGER2, dataItemImportInfo.DataType);
            Assert.AreEqual(BEDATATYPE.DT_REAL4, dataItemImportInfo.GlobalDataType);
            Assert.AreEqual(10, dataItemImportInfo.Size);
            Assert.AreEqual(10, dataItemImportInfo.Offset);
            Assert.AreEqual(2, dataItemImportInfo.Gain);
            Assert.AreEqual(1, dataItemImportInfo.IndexRegisterNumber);
            Assert.AreEqual("Some description", dataItemImportInfo.Description);
            Assert.IsTrue(dataItemImportInfo.LogToAuditTrail);
            Assert.AreEqual("PollGroup2", dataItemImportInfo.PollGroupName);
            Assert.AreEqual(AccessRights.ReadWrite, propertyDescriptors["AccessRight_1"].GetValue(dataItemImportInfo));
            Assert.AreEqual("D0", propertyDescriptors["Address_1"].GetValue(dataItemImportInfo));
            Assert.IsTrue(dataItemImportInfo.IsOpcVisible == true);
        }

        [Test]
        public void CreateDataItemInfoForExportThreeControllers()
        {
            IDataSourceContainer controllerOne = null;
            IDataSourceContainer controllerTwo = null;
            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOne = null;
            IDataItem dataItemTwo = null;
            IDataItem dataItemThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOne, "MyControllerOne", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemTwo, "MyControllerTwo", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemThree, "MyControllerThree", "DataItem1");
            dataItemOne.ItemID = "D0";
            dataItemTwo.ItemID = "M0";
            dataItemThree.ItemID = "C0";
            m_Controllers.Add(controllerOne);
            m_Controllers.Add(controllerTwo);
            m_Controllers.Add(controllerThree);

            IGlobalDataItem globalDataItem = CreateGlobalDataItem();
            globalDataItem.Name = "Tag1";
            globalDataItem.AccessRight = AccessRights.Write;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "PollGroup1" };
            globalDataItem.DataItems.Add(dataItemOne);
            globalDataItem.DataItems.Add(dataItemTwo);
            globalDataItem.DataItems.Add(dataItemThree);
            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerTwo.Name] = AccessRights.Write;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            ExposureExtenderProvider.SetExtendedPropertyValue(globalDataItem, ExposureExtenderProvider.IsExposedPropertyName, false);

            IDictionary<int, IDataItem> dataItemDictionary = new Dictionary<int, IDataItem>();
            dataItemDictionary.Add(0, dataItemOne);
            dataItemDictionary.Add(1, dataItemTwo);
            dataItemDictionary.Add(2, dataItemThree);
            DataItemImportInfo dataItemImportInfo = DataItemImportExportHelper.CreateDataItemImportInfo(globalDataItem, dataItemDictionary);
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(dataItemImportInfo);
            Assert.AreEqual("Tag1", dataItemImportInfo.Name);
            Assert.AreEqual(AccessRights.Write, dataItemImportInfo.AccessRight);
            Assert.AreEqual(BEDATATYPE.DT_INTEGER2, dataItemImportInfo.DataType);
            Assert.AreEqual(BEDATATYPE.DT_REAL4, dataItemImportInfo.GlobalDataType);
            Assert.AreEqual(10, dataItemImportInfo.Size);
            Assert.AreEqual(10, dataItemImportInfo.Offset);
            Assert.AreEqual(2, dataItemImportInfo.Gain);
            Assert.AreEqual(1, dataItemImportInfo.IndexRegisterNumber);
            Assert.AreEqual("Some description", dataItemImportInfo.Description);
            Assert.IsTrue(dataItemImportInfo.LogToAuditTrail);
            Assert.AreEqual("PollGroup1", dataItemImportInfo.PollGroupName);
            Assert.AreEqual(AccessRights.Read, propertyDescriptors["AccessRight_1"].GetValue(dataItemImportInfo));
            Assert.AreEqual(AccessRights.Write, propertyDescriptors["AccessRight_2"].GetValue(dataItemImportInfo));
            Assert.AreEqual(AccessRights.ReadWrite, propertyDescriptors["AccessRight_3"].GetValue(dataItemImportInfo));
            Assert.AreEqual("D0", propertyDescriptors["Address_1"].GetValue(dataItemImportInfo));
            Assert.AreEqual("M0", propertyDescriptors["Address_2"].GetValue(dataItemImportInfo));
            Assert.AreEqual("C0", propertyDescriptors["Address_3"].GetValue(dataItemImportInfo));
            Assert.IsTrue(dataItemImportInfo.IsOpcVisible == false);
        }

        [Test]
        public void CreateDataItemInfoForExportThreeControllersAndGap()
        {
            IDataSourceContainer controllerOne = null;
            IDataSourceContainer controllerTwo = null;
            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOne = null;
            IDataItem dataItemTwo = null;
            IDataItem dataItemThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOne, "MyControllerOne", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemTwo, "MyControllerTwo", "DataItem1");
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemThree, "MyControllerThree", "DataItem1");
            dataItemOne.ItemID = "D0";
            dataItemTwo.ItemID = "M0";
            dataItemThree.ItemID = "C0";
            m_Controllers.Add(controllerOne);
            m_Controllers.Add(controllerTwo);
            m_Controllers.Add(controllerThree);

            IGlobalDataItem globalDataItem = CreateGlobalDataItem();
            globalDataItem.Name = "Tag1";
            globalDataItem.AccessRight = AccessRights.Write;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
            globalDataItem.GlobalDataType = BEDATATYPE.DT_REAL4;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "DefaultPollGroup" };
            globalDataItem.DataItems.Add(dataItemOne);
            globalDataItem.DataItems.Add(dataItemThree);
            globalDataItem.AccessRights[controllerOne.Name] = AccessRights.Read;
            globalDataItem.AccessRights[controllerThree.Name] = AccessRights.ReadWrite;
            ExposureExtenderProvider.SetExtendedPropertyValue(globalDataItem, ExposureExtenderProvider.IsExposedPropertyName, true);

            IDictionary<int, IDataItem> dataItemDictionary = new Dictionary<int, IDataItem>();
            dataItemDictionary.Add(0, dataItemOne);
            dataItemDictionary.Add(1, null);
            dataItemDictionary.Add(2, dataItemThree);
            DataItemImportInfo dataItemImportInfo = DataItemImportExportHelper.CreateDataItemImportInfo(globalDataItem, dataItemDictionary);
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(dataItemImportInfo);
            Assert.AreEqual("Tag1", dataItemImportInfo.Name);
            Assert.AreEqual(AccessRights.Write, dataItemImportInfo.AccessRight);
            Assert.AreEqual(BEDATATYPE.DT_INTEGER2, dataItemImportInfo.DataType);
            Assert.AreEqual(BEDATATYPE.DT_REAL4, dataItemImportInfo.GlobalDataType);
            Assert.AreEqual(10, dataItemImportInfo.Size);
            Assert.AreEqual(10, dataItemImportInfo.Offset);
            Assert.AreEqual(2, dataItemImportInfo.Gain);
            Assert.AreEqual(1, dataItemImportInfo.IndexRegisterNumber);
            Assert.AreEqual("Some description", dataItemImportInfo.Description);
            Assert.IsTrue(dataItemImportInfo.LogToAuditTrail);
            Assert.AreEqual("DefaultPollGroup", dataItemImportInfo.PollGroupName);
            Assert.AreEqual(AccessRights.Read, propertyDescriptors["AccessRight_1"].GetValue(dataItemImportInfo));
            Assert.AreEqual(AccessRights.ReadWrite, propertyDescriptors["AccessRight_3"].GetValue(dataItemImportInfo));
            Assert.AreEqual("D0", propertyDescriptors["Address_1"].GetValue(dataItemImportInfo));
            Assert.AreEqual("C0", propertyDescriptors["Address_3"].GetValue(dataItemImportInfo));
            Assert.IsTrue(dataItemImportInfo.IsOpcVisible == true);
        }

        [Test]
        public void CreateDataItemsFromDataItemInfoInControllerTest()
        {
            IDataSourceContainer controller = null;
            ControllerHelper.CreateStubController(out controller, "MyController");
            IGlobalDataItem globalDataItem = CreateGlobalDataItem("Tag1");
            globalDataItem.PollGroup = new PollGroup() { Name = "DefaultPollGroup" };
            DataItemImportInfo dataItemImportInfo = CreateDataItemImportInfo(globalDataItem, true, "DataItem1",
                10, 2, BEDATATYPE.DT_STRING, 10, "D0", 1);
            ((IImportMergeInfo)dataItemImportInfo).MergeAction = MergeAction.Add;
            List<DataItemImportInfo> dataItemImportInfos = new List<DataItemImportInfo>();
            Dictionary<string, IList<IDataItemBase>> controllerItems = new Dictionary<string, IList<IDataItemBase>>();
            IControllerBase controllerBase = controller;
            dataItemImportInfos.Add(dataItemImportInfo);

            (IList<IDataItemBase> dataItems, IList<string> logs) = DataItemImportExportHelper.CreateDataItemsFromDataItemInfo(dataItemImportInfos, controllerItems, controllerBase);
            Assert.AreEqual(1, dataItems.Count);
            Assert.AreEqual(1, ((IGlobalDataItem)dataItems[0]).DataItems.Count);
            IDataItem dataItem = ((IGlobalDataItem)dataItems[0]).DataItems[0];
            Assert.AreEqual(true, dataItem.LogToAuditTrail);
            Assert.AreEqual(10, dataItem.Offset);
            Assert.AreEqual(2, dataItem.Gain);
            Assert.AreEqual(BEDATATYPE.DT_STRING, dataItem.DataType);
            Assert.AreEqual(10, dataItem.Size);
            Assert.AreEqual("D0", dataItem.ItemID);
        }

        [Test]
        public void CreateDataItemsFromDataItemInfoInGlobalControllerTest()
        {
            IDataSourceContainer controller = null;
            ControllerHelper.CreateStubController(out controller, "MyController");
            m_Controllers.Add(controller);
            IGlobalDataItem globalDataItem = CreateGlobalDataItem("Tag1");
            globalDataItem.PollGroup = new PollGroup() { Name = "DefaultPollGroup" };
            DataItemImportInfo dataItemImportInfo = CreateDataItemImportInfo(globalDataItem, true, "DataItem1",
                10, 2, BEDATATYPE.DT_STRING, 10, "D0", 1);
            ((IImportMergeInfo)dataItemImportInfo).MergeAction = MergeAction.Add;
            List<DataItemImportInfo> dataItemImportInfos = new List<DataItemImportInfo>();
            Dictionary<string, IList<IDataItemBase>> controllerItems = new Dictionary<string, IList<IDataItemBase>>();
            dataItemImportInfos.Add(dataItemImportInfo);

            (IList<IDataItemBase> dataItems, IList<string> logs) = DataItemImportExportHelper.CreateDataItemsFromDataItemInfo(dataItemImportInfos, controllerItems, m_GlobalController);
            Assert.AreEqual(1, dataItems.Count);
            IGlobalDataItem dataItem = dataItems[0] as IGlobalDataItem;
            Assert.AreEqual(true, dataItem.LogToAuditTrail);
            Assert.AreEqual("DataItem1", dataItem.Name);
            Assert.AreEqual(10, dataItem.Offset);
            Assert.AreEqual(2, dataItem.Gain);
            Assert.AreEqual(BEDATATYPE.DT_STRING, dataItem.DataType);
            Assert.AreEqual(10, dataItem.Size);
        }

        [Test]
        public void CreateDataItemsFromDataItemInfoInGlobalControllerMultipleControllersTest()
        {
            IDataSourceContainer controller = null;
            ControllerHelper.CreateStubController(out controller, "MyController");
            m_Controllers.Add(controller);

            IGlobalDataItem globalDataItem = CreateGlobalDataItem("Tag1");
            globalDataItem.PollGroup = m_GlobalController.PollGroups[0];
            DataItemImportInfo dataItemImportInfo = CreateDataItemImportInfo(globalDataItem, true, "DataItem1",
                10, 2, BEDATATYPE.DT_STRING, 10, "D0", 1);
            ((IImportMergeInfo)dataItemImportInfo).MergeAction = MergeAction.Add;
            List<DataItemImportInfo> dataItemImportInfos = new List<DataItemImportInfo>();
            Dictionary<string, IList<IDataItemBase>> controllerItems = new Dictionary<string, IList<IDataItemBase>>();
            dataItemImportInfos.Add(dataItemImportInfo);

            (IList<IDataItemBase> dataItems, IList<string> logs) = DataItemImportExportHelper.CreateDataItemsFromDataItemInfo(dataItemImportInfos, controllerItems, m_GlobalController);
            Assert.AreEqual(1, dataItems.Count);
            IGlobalDataItem gDI = dataItems[0] as IGlobalDataItem;
            Assert.AreEqual(true, gDI.LogToAuditTrail);
            Assert.AreEqual(10, gDI.Offset);
            Assert.AreEqual(2, gDI.Gain);
            Assert.AreEqual(BEDATATYPE.DT_STRING, gDI.DataType);
            Assert.AreEqual(10, gDI.Size);
            Assert.AreEqual("DataItem1", gDI.Name);
            Assert.AreEqual("DefaultPollGroup", gDI.PollGroup.Name);
            //This only seems to work when running test in debugging.
            IDataItem dataItem = gDI.DataItems[0];
            Assert.AreEqual(true, dataItem.LogToAuditTrail);
            Assert.AreEqual(10, dataItem.Offset);
            Assert.AreEqual(2, dataItem.Gain);
            Assert.AreEqual(BEDATATYPE.DT_STRING, dataItem.DataType);
            Assert.AreEqual(10, dataItem.Size);
            Assert.AreEqual("D0", dataItem.ItemID);

        }

        [Test]
        public void CreateDataItemsFromArrayTagDataItemInfoInControllerTest()
        {
            IDataSourceContainer controller = null;
            ControllerHelper.CreateStubController(out controller, "MyController");
            IGlobalDataItem globalDataItem = CreateGlobalDataItemWithArraySize("Tag1", 4);
            globalDataItem.PollGroup = new PollGroup() { Name = "DefaultPollGroup" };
            DataItemImportInfo dataItemImportInfo = CreateDataItemImportInfo(globalDataItem, true, "DataItem1",
                10, 2, BEDATATYPE.DT_STRING, 10, "D0", 4);
            ((IImportMergeInfo)dataItemImportInfo).MergeAction = MergeAction.Add;
            List<DataItemImportInfo> dataItemImportInfos = new List<DataItemImportInfo>();
            Dictionary<string, IList<IDataItemBase>> controllerItems = new Dictionary<string, IList<IDataItemBase>>();
            IControllerBase controllerBase = controller;
            dataItemImportInfos.Add(dataItemImportInfo);

            (IList<IDataItemBase> dataItems, IList<string> logs) = DataItemImportExportHelper.CreateDataItemsFromDataItemInfo(dataItemImportInfos, controllerItems, controllerBase);
            Assert.AreEqual(4, ((IGlobalDataItem)dataItems[0]).ArraySize);
            Assert.AreEqual(1, dataItems.Count);
            Assert.AreEqual(1, ((IGlobalDataItem)dataItems[0]).GlobalDataSubItems[0].DataItems.Count);
            IDataItem dataItem1 = ((IGlobalDataItem)dataItems[0]).GlobalDataSubItems[0].DataItems[0];
            Assert.AreEqual(true, dataItem1.LogToAuditTrail);
            Assert.AreEqual(10, dataItem1.Offset);
            Assert.AreEqual(2, dataItem1.Gain);
            Assert.AreEqual(BEDATATYPE.DT_STRING, dataItem1.DataType);
            Assert.AreEqual(10, dataItem1.Size);
            Assert.AreEqual("D0", dataItem1.ItemID);
            //The CreateArrayTags logic is not applicable when running from test, so there will be no dataItem2, 3 & 4

        }

        private DataItemImportInfo CreateDataItemImportInfo(IGlobalDataItem globalDataItem,
            bool logToAuditTrail, string name, double offset, int gain,
            BEDATATYPE dataType, short size, string itemID, short arraySize)
        {
            DataItemImportInfo dataItemImportInfo = new DataItemImportInfo(globalDataItem);
            dataItemImportInfo.Name = name;
            dataItemImportInfo.LogToAuditTrail = logToAuditTrail;
            dataItemImportInfo.DataType = dataType;
            dataItemImportInfo.Gain = gain;
            dataItemImportInfo.Offset = offset;
            dataItemImportInfo.Size = size;
            dataItemImportInfo.ArraySize = arraySize;
            dataItemImportInfo.Addresses.Add(itemID);
            dataItemImportInfo.DataItems.Add(0, null);
            m_DataItem.LogToAuditTrail = logToAuditTrail;
            m_DataItem.DataType = dataType;
            m_DataItem.Gain = gain;
            m_DataItem.Offset = offset;
            m_DataItem.Size = size;
            m_DataItem.ItemID = itemID;

            return dataItemImportInfo;
        }

        private IGlobalDataItem CreateGlobalDataItem(string name)
        {
            IGlobalDataItem globalDataItem = CreateGlobalDataItem();
            globalDataItem.Name = name;
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.DataType = BEDATATYPE.DT_STRING;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;

            return globalDataItem;
        }

        private IGlobalDataItem CreateGlobalDataItemWithArraySize(string name, short arraySize)
        {
            IGlobalDataItem globalDataItem = CreateGlobalDataItem(name);
            globalDataItem.ArraySize = arraySize;
            return globalDataItem;
        }

        private IGlobalDataItem CreateGlobalDataItem() => (IGlobalDataItem)m_DesignerHost.CreateComponent(typeof(TestGlobalDataItem));
    }

    sealed class TestGlobalDataItem : GlobalDataItem
    {
        public TestGlobalDataItem()
        {
            PollGroup = new PollGroup();
        }

        protected override string ControllerName => "";
        public override IPollGroup PollGroup { get; set; }
    }
}
