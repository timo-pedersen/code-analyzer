using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Core.Api.DataSource;
using Core.Api.Feature;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Api.Tools;
using Core.Controls.Api.AsmMeta;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.OpcClient;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Serialization.Converters;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Serialization
{
    [TestFixture]
    public class OldTypeObjectSerializerTest
    {
        private IOpcClientServiceCF m_OpcClientService;
        private ExtendedBindingList<IDataSourceContainer> m_Controllers;
        private IGlobalController m_GlobalController;
        private IGlobalReferenceService m_GlobalReferenceService;
        private TypeDescriptionProvider m_MappingTypeDescriptorProvider;
        private IDataItem m_DataItem;

        private const string TempFileName = "\\OldTypeObjectSerializerTest.xml";

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.ClearServices();   
            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            var pathProvider = MockRepository.GenerateStub<IFeatureXmlPathProvider>().ToLazy();

            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(false);

            m_Controllers = new ExtendedBindingList<IDataSourceContainer>();

            m_DataItem = MockRepository.GenerateStub<IDataItem>();

            TestHelper.AddService(typeof(INativeAPI), new NativeAPI());

            IFeatureSecurityService featureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            TestHelper.AddService(typeof(IFeatureSecurityService), featureSecurityService);

            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceCF>();
            m_OpcClientService.Stub(x => x.Controllers).Return(m_Controllers);
            m_OpcClientService.Stub(x => x.AddNewDataItem(string.Empty, string.Empty, null)).IgnoreArguments().Return(MockRepository.GenerateStub<IDataItem>());
            m_OpcClientService.Stub(x => x.AddNewDataItem(string.Empty, string.Empty, null, true, 0.0, 0.0, BEDATATYPE.DT_BIT, 0)).IgnoreArguments().Return(m_DataItem);
            m_GlobalController = MockRepository.GenerateStub<IGlobalController>();
            m_GlobalController.Name = "GlobalController";
            
            IPollGroup pollGroup = MockRepository.GenerateStub<IPollGroup>();
            pollGroup.Name = "DefaultPollGroup";
            m_GlobalController.Stub(x => x.PollGroups).Return(new BindingList<IPollGroup>() { pollGroup });

            m_OpcClientService.Stub(x => x.GlobalController).Return(m_GlobalController);

            IDataSourceContainer controller = null;
            IDataItem dataItem = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controller, out dataItem, "MyController", "DataItem1");
            dataItem.ItemID = "D0";
            m_Controllers.Add(controller);

            TestHelper.AddServiceStub<IFastLoggingFeatureLogicService>();

            IGlobalDataItem globalDataItem = new GlobalDataItem();
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
            //ARRAYTAG
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
            m_GlobalReferenceService.Stub(x => x.GetObject<IGlobalDataItem>(StringConstants.TagsRoot + "Tag1")).Return(globalDataItem).Repeat.Any();

            m_MappingTypeDescriptorProvider = new AsmMetaTypeDescriptionProviderBuilder(typeof(object))
                .Build();
            
            TypeDescriptor.AddProvider(m_MappingTypeDescriptorProvider, typeof(object));

            var tagTraverser = MockRepository.GenerateStub<ITagTraverser>();

            var emptyEnumerable = Enumerable.Empty<IGlobalDataItemBase>().ToArray();
            tagTraverser.Stub(inv => inv.GetFlattenedDataItems(Arg<ITag>.Is.Anything)).Return(emptyEnumerable);
            tagTraverser.Stub(inv => inv.GetFlattenedDataItems(Arg<IEnumerable<ITag>>.Is.Anything)).Return(emptyEnumerable);
            tagTraverser.Stub(inv => inv.GetAllPaths(Arg<ITag[]>.Is.Anything)).Return(Enumerable.Empty<string>().ToArray());
            ServiceContainerCF.Instance.AddService<ITagTraverser>(tagTraverser);
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TypeDescriptor.RemoveProvider(m_MappingTypeDescriptorProvider, typeof(object));
            TestHelper.ClearServices();
        }

        [Test]
        public void ReadOldTypeTags()
        {
            try
            {
                using (new SelectSwedishTestingCulture())
                {
                    IObjectSerializer objectSerializer = new ObjectSerializer();


                    //This is needed due because there is no old converter for tags.
                    //Prior conversions was made by objectXMLSerializer.
                    //Now the code goes through XmlArrayTagsConverter before object serialization in design time.
                    IXmlConverter arrayTagConverter = new XmlArrayTagsConverter();
                    XDocument convertedTagFile = XDocument.Parse(FileResources.Tags);
                    arrayTagConverter.ConvertDesigner("", convertedTagFile);

                    IGlobalController globalController = objectSerializer.DeseralizeString(convertedTagFile.ToString()) as IGlobalController;
                    IGlobalDataItem globaldataItem = globalController.DataItemBases[0] as IGlobalDataItem;

                    Assert.AreEqual("TestTag1", globaldataItem.Name);
                    Assert.AreEqual(BEDATATYPE.DT_REAL4, globaldataItem.DataType);
                    Assert.AreEqual(1, globaldataItem.Size);
                    Assert.AreEqual(12.34, globaldataItem.Offset);
                    Assert.AreEqual(5.67, globaldataItem.Gain);
                    Assert.AreEqual(2, globaldataItem.IndexRegisterNumber);
                    Assert.AreEqual(false, globaldataItem.LogToAuditTrail);
                    Assert.AreEqual("Value Change", globaldataItem.Trigger.Name);
                    Assert.AreEqual(AccessRights.Read, globaldataItem.AccessRight);
                    Assert.AreEqual("PollGroup3", globaldataItem.PollGroup.Name);
                    Assert.AreEqual(true, globaldataItem.AlwaysActive);
                    Assert.AreEqual(true, globaldataItem.NonVolatile);
                    Assert.AreEqual(987.65, globaldataItem.InitialValue.Decimal);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void ReadOldTypeAlarmServer()
        {
            IObjectSerializer objectSerializer = new ObjectSerializer();



            IAlarmServer alarmServer = objectSerializer.DeseralizeString(FileResources.AlarmServer) as IAlarmServer;
            IAlarmItem alarmItem = alarmServer.AlarmGroups[0].AlarmItems[0];

            Assert.AreEqual("TestGroup1", alarmItem.GroupName);
            Assert.AreEqual("TestItem1", alarmItem.DisplayName);
            Assert.AreEqual("TestGroup1_TestItem1", alarmItem.Name);
            Assert.AreEqual("Test alarm 1", alarmItem.Text);
            Assert.AreEqual(ComparerTypes.EqualToGreaterThan, alarmItem.ComparerType);
            Assert.AreEqual(StringConstants.TagsRoot + "TestTag1", alarmItem.DataConnection);
            Assert.AreEqual(StringConstants.TagsRoot + "TestTag2", alarmItem.RemoteAcknowledge);
            Assert.AreEqual(123, alarmItem.TriggerValue);
            Assert.AreEqual(false, alarmItem.IsDigitalValue);
            Assert.AreEqual(true, alarmItem.History);
            Assert.AreEqual(true, alarmItem.AcknowledgeRequired);
            Assert.AreEqual(false, alarmItem.EnableDistribution);
        }
    }
}
