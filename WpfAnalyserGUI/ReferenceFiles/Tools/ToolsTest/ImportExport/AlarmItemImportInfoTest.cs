using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Core.Api.Tools;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.ImportExport
{
    [TestFixture]
    public class AlarmItemImportExportInfoTest
    {
        private IOpcClientServiceIde m_OpcClientService;
        private ExtendedBindingList<IDataSourceContainer> m_Controllers;
        private IGlobalController m_GlobalController;
        private IDataItem m_DataItem;
        private IGlobalReferenceService m_GlobalReferenceService;
        private IMultiLanguageServiceCF m_MultiLanguageServiceCF;

        private List<IAlarmItem> m_ExistingAlarmItems;

        [SetUp]
        public void Setup()
        {
            TestHelper.AddServiceStub<IFastLoggingFeatureLogicService>();

            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_MultiLanguageServiceCF = TestHelper.CreateAndAddServiceStub<IMultiLanguageServiceCF>();

            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(false);

            m_Controllers = new ExtendedBindingList<IDataSourceContainer>();

            m_DataItem = Substitute.For<IDataItem>();

            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_OpcClientService.Controllers.Returns(m_Controllers);
            m_OpcClientService.AddNewDataItem(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IControllerBase>()).Returns(Substitute.For<IDataItem>());
            m_OpcClientService.AddNewDataItem(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IControllerBase>(), 
                Arg.Any<bool>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<BEDATATYPE>(), Arg.Any<short>())
                .Returns(m_DataItem);
            m_GlobalController = Substitute.For<IGlobalController>();
            m_GlobalController.Name = "GlobalController";

            IPollGroup pollGroup = Substitute.For<IPollGroup>();
            pollGroup.Name = "DefaultPollGroup";
            m_GlobalController.PollGroups.Returns(new BindingList<IPollGroup>() { pollGroup });

            m_OpcClientService.GlobalController.Returns(m_GlobalController);

            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "MyController", "DataItem1");
            dataItem.ItemID = "D0";
            m_Controllers.Add(controller);

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.Name = "Tag1";
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.DataType = BEDATATYPE.DT_STRING;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "PollGroup2" };
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
            m_GlobalReferenceService.GetObject<IDataItemProxySource>(StringConstants.TagsRoot + "Tag1").Returns(globalDataItem);

            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "MyController", "DataItem2");
            dataItem.ItemID = "D1";
            m_Controllers.Add(controller);

            GlobalDataItem globalDataItem2 = new GlobalDataItem();
            globalDataItem.Name = "Tag2";
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.DataType = BEDATATYPE.DT_STRING;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "PollGroup2" };
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
            m_GlobalReferenceService.GetObject<IDataItemProxySource>(StringConstants.TagsRoot + "Tag2").Returns(globalDataItem2);

            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "MyController", "DataItem3");
            dataItem.ItemID = "D1";
            m_Controllers.Add(controller);

            GlobalDataItem globalDataItem3 = new GlobalDataItem();
            globalDataItem.Name = "Tag3";
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.DataType = BEDATATYPE.DT_STRING;
            globalDataItem.Size = 10;
            globalDataItem.Offset = 10;
            globalDataItem.Gain = 2;
            globalDataItem.IndexRegisterNumber = 1;
            globalDataItem.Description = "Some description";
            globalDataItem.LogToAuditTrail = true;
            globalDataItem.PollGroup = new PollGroup() { Name = "PollGroup2" };
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
            m_GlobalReferenceService.GetObject<IDataItemProxySource>(StringConstants.TagsRoot + "Tag3").Returns(globalDataItem3);
            
            m_ExistingAlarmItems = new List<IAlarmItem>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void AddAlarmItemsTest()
        {
            List<AlarmItemImportExportInfo> mergedAlarmItems = new List<AlarmItemImportExportInfo>();

            // Alarm Item 1 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo1 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem0",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = true,
                ComparerType = ComparerTypes.EqualToGreaterThan,
                DataConnection = StringConstants.TagsRoot + "Tag1",
                History = true,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag2",
                Text = "Larmtext 1",
                TriggerValue = "12345"
            };
            IImportMergeInfo importMergeInfo = alarmItemImportInfo1;
            importMergeInfo.MergeAction = MergeAction.Add;
            mergedAlarmItems.Add(alarmItemImportInfo1);

            // Alarm Item 2 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo2 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem1",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = true,
                ComparerType = ComparerTypes.EqualTo,
                DataConnection = StringConstants.TagsRoot + "Tag1",
                History = true,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag2",
                Text = "Larmtext 2",
                TriggerValue = "123.45"
            };
            IImportMergeInfo importMergeInfo2 = alarmItemImportInfo2;
            importMergeInfo2.MergeAction = MergeAction.Add;
            mergedAlarmItems.Add(alarmItemImportInfo2);

            // Alarm Item 3 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo3 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem2",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = true,
                ComparerType = ComparerTypes.LessThan,
                DataConnection = StringConstants.TagsRoot + "Tag1",
                History = true,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag2",
                Text = "Larmtext 3",
                TriggerValue = "1.2345e20"
            };
            IImportMergeInfo importMergeInfo3 = alarmItemImportInfo3;
            importMergeInfo3.MergeAction = MergeAction.Add;
            mergedAlarmItems.Add(alarmItemImportInfo3);


            IList<IAlarmItem> newAlarmItems;
            AlarmItemImportExportHelper.UpdateAndCreateAlarmItems(m_ExistingAlarmItems, mergedAlarmItems, out newAlarmItems);

            Assert.AreEqual(3, newAlarmItems.Count);

            IAlarmItem alarmItem1 = newAlarmItems[0];
            Assert.AreEqual("AlarmItem0", alarmItem1.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem1.GroupName);
            Assert.AreEqual(true, alarmItem1.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.EqualToGreaterThan, alarmItem1.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag1", alarmItem1.DataConnection);
            Assert.AreEqual(true, alarmItem1.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem1.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 1", alarmItem1.Text);
            Assert.AreEqual(12345, alarmItem1.TriggerValue);
            Assert.AreEqual("AlarmGroup1_AlarmItem0", alarmItem1.Name);

            IAlarmItem alarmItem2 = newAlarmItems[1];
            Assert.AreEqual("AlarmItem1", alarmItem2.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem2.GroupName);
            Assert.AreEqual(true, alarmItem2.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.EqualTo, alarmItem2.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag1", alarmItem2.DataConnection);
            Assert.AreEqual(true, alarmItem2.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem2.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 2", alarmItem2.Text);
            Assert.AreEqual(123.45, Convert.ToDouble(alarmItem2.TriggerValue));
            Assert.AreEqual("AlarmGroup1_AlarmItem1", alarmItem2.Name);

            IAlarmItem alarmItem3 = newAlarmItems[2];
            Assert.AreEqual("AlarmItem2", alarmItem3.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem3.GroupName);
            Assert.AreEqual(true, alarmItem3.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.LessThan, alarmItem3.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag1", alarmItem3.DataConnection);
            Assert.AreEqual(true, alarmItem3.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem3.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 3", alarmItem3.Text);
            Assert.AreEqual(1.2345e20, Convert.ToDouble(alarmItem3.TriggerValue));
            Assert.AreEqual("AlarmGroup1_AlarmItem2", alarmItem3.Name);

            m_ExistingAlarmItems.Add(alarmItem1);
            m_ExistingAlarmItems.Add(alarmItem2);
            m_ExistingAlarmItems.Add(alarmItem3);
        }

        [Test]
        public void OverwriteAlarmItemsTest()
        {
            AddAlarmItemsTest();

            IList<AlarmItemImportExportInfo> mergedAlarmItems = new List<AlarmItemImportExportInfo>();

            // Alarm Item 1 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo1 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem0",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = false,
                ComparerType = ComparerTypes.NotEqualTo,
                DataConnection = StringConstants.TagsRoot + "Tag2",
                History = false,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag3",
                Text = "Larmtext 1 aaaa",
                TriggerValue = "987.65",
                Name = "AlarmGroup1_AlarmItem0"
            };
            IImportMergeInfo importMergeInfo = alarmItemImportInfo1;
            importMergeInfo.MergeAction = MergeAction.OverWrite;
            mergedAlarmItems.Add(alarmItemImportInfo1);

            IList<IAlarmItem> newAlarmItems;
            int originalAlarmItemCount = m_ExistingAlarmItems.Count;

            AlarmItemImportExportHelper.UpdateAndCreateAlarmItems(m_ExistingAlarmItems, mergedAlarmItems, out newAlarmItems);
            Assert.AreEqual(1, newAlarmItems.Count);
            Assert.AreEqual(originalAlarmItemCount - 1, m_ExistingAlarmItems.Count);
            IAlarmItem alarmItem = newAlarmItems[0];
            Assert.AreEqual("AlarmItem0", alarmItem.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem.GroupName);
            Assert.AreEqual(false, alarmItem.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.NotEqualTo, alarmItem.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem.DataConnection);
            Assert.AreEqual(false, alarmItem.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag3", alarmItem.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 1 aaaa", alarmItem.Text);
            Assert.AreEqual(987.65, Convert.ToDouble(alarmItem.TriggerValue));
            Assert.AreEqual("AlarmGroup1_AlarmItem0", alarmItem.Name);
        }

        [Test]
        public void MergeAlarmItemsTest()
        {
            AddAlarmItemsTest();

            List<AlarmItemImportExportInfo> mergedAlarmItems = new List<AlarmItemImportExportInfo>();

            // Alarm Item 1 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo1 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem0",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = false,
                ComparerType = ComparerTypes.NotEqualTo,
                DataConnection = StringConstants.TagsRoot + "Tag2",
                History = false,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag3",
                Text = "Larmtext 1 aaaa",
                TriggerValue = "987.65",
                Name = "AlarmGroup1_AlarmItem0"
            };
            IImportMergeInfo importMergeInfo = alarmItemImportInfo1;
            importMergeInfo.MergeAction = MergeAction.Merge;
            mergedAlarmItems.Add(alarmItemImportInfo1);

            IList<IAlarmItem> newAlarmItems;
            int originalAlarmItemCount = m_ExistingAlarmItems.Count;

            AlarmItemImportExportHelper.UpdateAndCreateAlarmItems(m_ExistingAlarmItems, mergedAlarmItems, out newAlarmItems);
            Assert.AreEqual(0, newAlarmItems.Count);
            Assert.AreEqual(originalAlarmItemCount, m_ExistingAlarmItems.Count);
            IAlarmItem alarmItem = m_ExistingAlarmItems.First(x => alarmItemImportInfo1.Name == x.Name);
            Assert.AreEqual("AlarmItem0", alarmItem.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem.GroupName);
            Assert.AreEqual(false, alarmItem.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.NotEqualTo, alarmItem.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem.DataConnection);
            Assert.AreEqual(false, alarmItem.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag3", alarmItem.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 1 aaaa", alarmItem.Text);
            Assert.AreEqual(987.65, Convert.ToDouble(alarmItem.TriggerValue));
            Assert.AreEqual("AlarmGroup1_AlarmItem0", alarmItem.Name);
        }

        [Test]
        public void ChangeNameAlarmItemsTest()
        {
            AddAlarmItemsTest();

            List<AlarmItemImportExportInfo> mergedAlarmItems = new List<AlarmItemImportExportInfo>();
            IList<IAlarmItem> newAlarmItems;

            // Alarm Item 1 to mergedAlarmItems
            AlarmItemImportExportInfo alarmItemImportInfo1 = new AlarmItemImportExportInfo()
            {
                DisplayName = "AlarmItem0",
                GroupName = "AlarmGroup1",
                AcknowledgeRequired = true,
                ComparerType = ComparerTypes.EqualToGreaterThan,
                DataConnection = StringConstants.TagsRoot + "Tag1",
                History = true,
                RemoteAcknowledge = StringConstants.TagsRoot + "Tag2",
                Text = "Larmtext 1",
                TriggerValue = "12345"
            };
            IImportMergeInfo importMergeInfo = alarmItemImportInfo1;
            importMergeInfo.MergeAction = MergeAction.ChangeName;
            mergedAlarmItems.Add(alarmItemImportInfo1);

            AlarmItemImportExportHelper.UpdateAndCreateAlarmItems(m_ExistingAlarmItems, mergedAlarmItems, out newAlarmItems);

            Assert.AreEqual(1, newAlarmItems.Count);

            IAlarmItem alarmItem = newAlarmItems[0];
            Assert.AreEqual("AlarmItem0", alarmItem.DisplayName);
            Assert.AreEqual("AlarmGroup1", alarmItem.GroupName);
            Assert.AreEqual(true, alarmItem.AcknowledgeRequired);
            Assert.AreEqual(ComparerTypes.EqualToGreaterThan, alarmItem.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag1", alarmItem.DataConnection);
            Assert.AreEqual(true, alarmItem.History);
            Assert.AreEqual(StringConstants.TagsRoot + "Tag2", alarmItem.RemoteAcknowledge);
            Assert.AreEqual("Larmtext 1", alarmItem.Text);
            Assert.AreEqual(12345, alarmItem.TriggerValue);
            Assert.AreEqual("AlarmGroup1_AlarmItem0", alarmItem.Name);
        }
     }
}
