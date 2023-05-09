using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Component.Api.Design;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Commands;
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
using INameCreationService = Neo.ApplicationFramework.Interfaces.INameCreationService;

namespace Neo.ApplicationFramework.Tools.CreateSeries
{
    [TestFixture]
    public class CreateSeriesTest
    {
        private ICreateSeriesService m_CreateSeriesService;

        private IList<string> m_IgnorePropertiesAlreadyTested;
        private IList<string> m_NonEqualPropertiesToIgnore;
        private IList<string> m_RuntimePropertiesToIgnore;
        private IList<string> m_PropertiesToIgnore;

        private IOpcClientServiceIde m_OpcClientService;
        private IDesignerHost m_DesignerHost;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IFastLoggingFeatureLogicService>();

            IGlobalController controllerStub = MockRepository.GenerateStub<IGlobalController>();
            controllerStub.Stub(x => x.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(new IDataItemBase[] { }));

            var dataCommandFacade = MockRepository.GenerateMock<IDataCommandFacade>();
            var propertyBinderFactory = MockRepository.GenerateMock<IPropertyBinderFactory>().ToILazy();

            TestHelper.AddService<INameCreationService>(new NameCreationService());

            m_CreateSeriesService = new CreateSeriesService(dataCommandFacade, propertyBinderFactory);
            
            TestHelper.AddServiceStub<IOpcClientServiceCF>();

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

            TestHelper.AddServiceStub<ITagChangedNotificationServiceCF>();
            var opcUaServerRootComponent = MockRepository.GenerateStub<IOpcUaServerRootComponent>();
            opcUaServerRootComponent.ExposureOption = OpcUaServerTagExposureOption.AllTagsVisible;
            var projectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            projectItem.Stub(x => x.ContainedObject).Return(opcUaServerRootComponent);
            var projectItemFinder = TestHelper.AddServiceStub<IProjectItemFinder>();
            projectItemFinder.Stub(x => x.GetProjectItems(typeof(IOpcUaServerRootComponent))).Return(new [] { projectItem });

            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_OpcClientService.Stub(x => x.GlobalController).Return(controllerStub);
            m_OpcClientService.Stub(x => x.AddNewDataItem(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<IControllerBase>.Is.Anything)).Repeat.Any().Do(
                new Func<string, string, IControllerBase, IDataItemBase>(
                    (_, __, ___) => (IGlobalDataItem)m_DesignerHost.CreateComponent(typeof(TestGlobalDataItem))));
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void ShouldCalculateNextOffsetBasedOnLastArrayPositionOnCurrentGlobalDataItem()
        {
            int expectedValueOffsetForArrayIndex1 = 1;
            int expectedValueOffsetForArrayIndex2 = 4;

            IGlobalDataItem currentDataItem;
            IDataItem firstDataItem = new DataItem();
            IDataItem secondDataItem = new DataItem();
            IDataItem thirdDataItem = new DataItem();

            IDataSourceContainer firstDataItemsDataSource = MockRepository.GenerateStub<IDataSourceContainer>();
            IDataSourceContainer secondDataItemsDataSource = MockRepository.GenerateStub<IDataSourceContainer>();
            IDataSourceContainer thirdDataItemsDataSource = MockRepository.GenerateStub<IDataSourceContainer>();
            firstDataItem.DataSourceContainer = firstDataItemsDataSource;
            secondDataItem.DataSourceContainer = secondDataItemsDataSource;
            thirdDataItem.DataSourceContainer = thirdDataItemsDataSource;

            CreateControllerArrayTag(out currentDataItem, firstDataItem, secondDataItem, thirdDataItem);
            m_CreateSeriesService.CurrentDataItem = currentDataItem;

            m_CreateSeriesService.CreateSeriesOfTags(2, 1);

            Calls calls = thirdDataItemsDataSource.GetCallsMadeOn(x => x.GetNextItemID(null, 0));
            Call firstCall = calls.First();
            Call secondCall = calls.ElementAt(1);

            Assert.That(int.Parse(firstCall.Arguments.ElementAt(1).ToString()), Is.EqualTo(expectedValueOffsetForArrayIndex1));
            Assert.That(int.Parse(secondCall.Arguments.ElementAt(1).ToString()), Is.EqualTo(expectedValueOffsetForArrayIndex2));
        }

        // This test's main purpose is to assure that new properties in GlobalDataItem and IGlobalDataItem, that should be included in a copy of a GlobalDataItem,
        // are added to CreateSeries.CopyGlobalDataItemProperties.
        // The CopyGlobalDataItemProperties method is also tested.
        // Note that the new property perhaps also should be added to Import/Export DataItems and Information Designer I import.
        [Test]
        public void CopyGlobalDataItemPropertiesTest()
        {
            IGlobalDataItem fromGlobalDataItem = SetupGlobalDataItemModel();
            IGlobalDataItem toGlobalDataItem = CreateGlobalDataItem();
            m_CreateSeriesService.CopyGlobalDataItemProperties(fromGlobalDataItem, toGlobalDataItem);

            AssertGlobalDataItemPropertiesAreProperlyCopied(fromGlobalDataItem, toGlobalDataItem);

            SetupIgnorePropertiesLists();

            AssertNoNewPropertiesAreAddedToGlobalControllerWithoutUpdatingCreateSeriesServiceCopyAndItsTest();
        }

        [Test]
        public void CopyGlobalDataItemExtendedPropertiesTest()
        {
            IGlobalDataItem fromGlobalDataItem = SetupGlobalDataItemModel();
            IGlobalDataItem toGlobalDataItem = CreateGlobalDataItem();
            m_CreateSeriesService.CopyGlobalDataItemProperties(fromGlobalDataItem, toGlobalDataItem);

            AssertGlobalDataItemExtendedPropertiesAreProperlyCopied(fromGlobalDataItem, toGlobalDataItem);
        }

        private static void AssertGlobalDataItemExtendedPropertiesAreProperlyCopied(IGlobalDataItem fromGlobalDataItem, IGlobalDataItem toGlobalDataItem) => Assert.AreEqual(
            ExposureExtenderProvider.GetExtendedPropertyValue(fromGlobalDataItem, ExposureExtenderProvider.IsExposedPropertyName),
            ExposureExtenderProvider.GetExtendedPropertyValue(toGlobalDataItem, ExposureExtenderProvider.IsExposedPropertyName),
            "GlobalDataItem's IsExposed-Extended-Property should be copied");

        private void AssertNoNewPropertiesAreAddedToGlobalControllerWithoutUpdatingCreateSeriesServiceCopyAndItsTest()
        {
            Type globalDataItemType = typeof(GlobalDataItem);
            PropertyInfo[] globalDataItemPropertyInfos = globalDataItemType.GetProperties();
            VerifyNoNewPropertiesAdded(globalDataItemPropertyInfos, "GlobalDataItem");

            Type iGlobalDataItemType = typeof(IGlobalDataItem);
            PropertyInfo[] iGlobalDataItemPropertyInfos = iGlobalDataItemType.GetProperties();
            VerifyNoNewPropertiesAdded(iGlobalDataItemPropertyInfos, "IGlobalDataItem");
        }

        private void VerifyNoNewPropertiesAdded(PropertyInfo[] propertyInfos, string name)
        {
            // Loops through all properties. All properties should exist in one of the lists.
            // If a new property is added, the test fails. The property should then be added to one of the lists.
            // If the property is relevant to be added in a GlobalDataItem copy, add a row to CreateSeriesService.CopyGlobalDataItemProperties.
            // Also add it to the SetupGlobalDataItemModel and AssertGlobalDataItemPropertiesAreProperlyCopied in this test.
            foreach (PropertyInfo info in propertyInfos.Where(info => info.CanRead && info.CanWrite))
            {
                if (m_IgnorePropertiesAlreadyTested.Contains(info.Name))
                    continue;

                if (m_NonEqualPropertiesToIgnore.Contains(info.Name))
                    continue;

                if (m_RuntimePropertiesToIgnore.Contains(info.Name))
                    continue;

                if (m_PropertiesToIgnore.Contains(info.Name))
                    continue;

                Assert.Fail("New properties in " + name + " should be copied in CreateSeriesService.CopyGlobalDataItemProperties and added to this test: " + info.Name);
            }
        }

        private IGlobalDataItem SetupGlobalDataItemModel()
        {
            IGlobalDataItem fromGlobalDataItem = CreateGlobalDataItem();
            fromGlobalDataItem.Name = "Tag3";
            fromGlobalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            fromGlobalDataItem.Size = 1;
            fromGlobalDataItem.Offset = 5;
            fromGlobalDataItem.Gain = 10;
            fromGlobalDataItem.IndexRegisterNumber = 2;
            fromGlobalDataItem.LogToAuditTrail = false;
            fromGlobalDataItem.AccessRight = AccessRights.Read;
            fromGlobalDataItem.PollGroup.Name = string.Empty;
            fromGlobalDataItem.AlwaysActive = true;
            fromGlobalDataItem.NonVolatile = false;
            fromGlobalDataItem.GlobalDataSubItems.Add(new GlobalDataSubItem());
            fromGlobalDataItem.Description = "Some description";
            fromGlobalDataItem.PollGroup = new PollGroup() { Name = "DefaultPollGroup" };
            fromGlobalDataItem.Trigger = new DataTrigger() { Name = "DefaultTrigger" };
            fromGlobalDataItem.IsPublic = false;
            fromGlobalDataItem.ReadExpression = "ReadExpressionName";
            fromGlobalDataItem.ReadExpression = "WriteExpressionName";
            return fromGlobalDataItem;
        }

        // Tests the copy functionality.
        private void AssertGlobalDataItemPropertiesAreProperlyCopied(IGlobalDataItem fromGlobalDataItem, IGlobalDataItem toGlobalDataItem)
        {
            Assert.AreNotEqual(toGlobalDataItem.Name, fromGlobalDataItem, "New GlobalDataItem's Name should not be a copied property");

            Assert.AreEqual(toGlobalDataItem.Size, fromGlobalDataItem.Size, "GlobalDataItem's Size should be copied");
            Assert.AreEqual(toGlobalDataItem.DataType, fromGlobalDataItem.DataType, "GlobalDataItem's DataType should be copied");
            //Assert.AreEqual(toGlobalDataItem.DataTypeFriendlyName, fromGlobalDataItem.DataTypeFriendlyName, "GlobalDataItem's DataTypeFriendlyName should be copied");

            Assert.AreEqual(toGlobalDataItem.GlobalDataType, fromGlobalDataItem.GlobalDataType, "GlobalDataItem's GlobalDataType should be copied");

            Assert.AreEqual(toGlobalDataItem.Offset, fromGlobalDataItem.Offset, "GlobalDataItem's Offset should be copied");
            Assert.AreEqual(toGlobalDataItem.Gain, fromGlobalDataItem.Gain, "GlobalDataItem's Gain should be copied");
            Assert.AreEqual(toGlobalDataItem.IndexRegisterNumber, fromGlobalDataItem.IndexRegisterNumber, "GlobalDataItem's IndexRegisterNumber should be copied");

            Assert.AreEqual(toGlobalDataItem.LogToAuditTrail, fromGlobalDataItem.LogToAuditTrail, "GlobalDataItem's LogToAuditTrail-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.Trigger, fromGlobalDataItem.Trigger, "GlobalDataItem's Trigger should be copied");

            Assert.AreEqual(toGlobalDataItem.AccessRight, fromGlobalDataItem.AccessRight, "GlobalDataItem's AccessRight should be copied");
            Assert.AreEqual(toGlobalDataItem.PollGroup, fromGlobalDataItem.PollGroup, "GlobalDataItem's PollGroup should be copied");

            Assert.AreEqual(toGlobalDataItem.AlwaysActive, fromGlobalDataItem.AlwaysActive, "GlobalDataItem's AlwaysActive-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.Description, fromGlobalDataItem.Description, "GlobalDataItem's PollGroup should be copied");

            Assert.AreEqual(toGlobalDataItem.InitialValue, fromGlobalDataItem.InitialValue, "GlobalDataItem's InitialValue-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.ArraySize, fromGlobalDataItem.ArraySize, "GlobalDataItem's ArraySize-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.PreventDuplicateEvents, fromGlobalDataItem.PreventDuplicateEvents, "GlobalDataItem's PreventDuplicateEvents-Property should be copied");

            Assert.AreEqual(toGlobalDataItem.IsPublic, fromGlobalDataItem.IsPublic, "GlobalDataItem's IsPublic-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.ReadExpression, fromGlobalDataItem.ReadExpression, "GlobalDataItem's ReadExpression-Property should be copied");
            Assert.AreEqual(toGlobalDataItem.WriteExpression, fromGlobalDataItem.WriteExpression, "GlobalDataItem's ReadExpression-Property should be copied");
        }

        // These lists contain all properties that IGlobalDataItem and GlobalDataItem contains
        private void SetupIgnorePropertiesLists()
        {
            //This list contains properties that are copied in the CreateSeriesService and tested earlier.
            m_IgnorePropertiesAlreadyTested = new List<string>();
            m_IgnorePropertiesAlreadyTested.Add("Size");
            m_IgnorePropertiesAlreadyTested.Add("DataType");
            m_IgnorePropertiesAlreadyTested.Add("GlobalDataType");
            m_IgnorePropertiesAlreadyTested.Add("Offset");
            m_IgnorePropertiesAlreadyTested.Add("Gain");
            m_IgnorePropertiesAlreadyTested.Add("IndexRegisterNumber");
            m_IgnorePropertiesAlreadyTested.Add("LogToAuditTrail");
            m_IgnorePropertiesAlreadyTested.Add("Trigger");
            m_IgnorePropertiesAlreadyTested.Add("TriggerName");
            m_IgnorePropertiesAlreadyTested.Add("AccessRight");
            m_IgnorePropertiesAlreadyTested.Add("PollGroup");
            m_IgnorePropertiesAlreadyTested.Add("PollGroupName");
            m_IgnorePropertiesAlreadyTested.Add("AlwaysActive");
            m_IgnorePropertiesAlreadyTested.Add("Description");
            m_IgnorePropertiesAlreadyTested.Add("TriggerValue");
            m_IgnorePropertiesAlreadyTested.Add("InitialValue");
            m_IgnorePropertiesAlreadyTested.Add("ArraySize");
            m_IgnorePropertiesAlreadyTested.Add("PreventDuplicateEvents");
            m_IgnorePropertiesAlreadyTested.Add("SelectOnCreated");
            m_IgnorePropertiesAlreadyTested.Add(nameof(GlobalDataItem.IsPublic));
            m_IgnorePropertiesAlreadyTested.Add(nameof(GlobalDataItem.ReadExpression));
            m_IgnorePropertiesAlreadyTested.Add(nameof(GlobalDataItem.WriteExpression));

            //List contains properties that shouldn't be equal in a real case.
            m_NonEqualPropertiesToIgnore = new List<string>();
            m_NonEqualPropertiesToIgnore.Add("Name");
            m_NonEqualPropertiesToIgnore.Add("FullName");
            m_NonEqualPropertiesToIgnore.Add("DisplayName");
            m_NonEqualPropertiesToIgnore.Add("DataItemNames");
            m_NonEqualPropertiesToIgnore.Add("DataItems");
            m_NonEqualPropertiesToIgnore.Add("AccessRights");
            m_NonEqualPropertiesToIgnore.Add("NonVolatile");

            //List contains runtime properties that are nonrelevant for a copy.
            m_RuntimePropertiesToIgnore = new List<string>();
            m_RuntimePropertiesToIgnore.Add("Value");
            m_RuntimePropertiesToIgnore.Add("Values");
            m_RuntimePropertiesToIgnore.Add("Offline");
            m_RuntimePropertiesToIgnore.Add("IsQualityChanged");
            m_RuntimePropertiesToIgnore.Add("InternalValue");
            m_RuntimePropertiesToIgnore.Add("SynchId");


            // Other properties to ignore at the moment. Actions and DataExchange info should be copied, but code was not prepared for this support.
            m_PropertiesToIgnore = new List<string>();
            m_PropertiesToIgnore.Add("ActionName");
            m_PropertiesToIgnore.Add("Controller");
            m_PropertiesToIgnore.Add("IsSetupForDataExchange");
            // From a base class. Irrelevant to test.
            m_PropertiesToIgnore.Add("Site");
            m_PropertiesToIgnore.Add("Container");
            m_PropertiesToIgnore.Add("DataTypeFriendlyName");
            m_PropertiesToIgnore.Add("GlobalDataTypeFriendlyName");
            m_PropertiesToIgnore.Add("AddressDescriptor");
            m_PropertiesToIgnore.Add(nameof(GlobalDataItem.ProjectGuid));
        }

        private void CreateControllerArrayTag(out IGlobalDataItem globalDataItemArrayTag, IDataItem firstDataItem, IDataItem secondDataItem, IDataItem thirdDataItem)
        {
            globalDataItemArrayTag = CreateGlobalDataItem();
            globalDataItemArrayTag.ArraySize = 3;

            globalDataItemArrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem());
            globalDataItemArrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem());

            globalDataItemArrayTag[0].DataItems.Add(firstDataItem);
            globalDataItemArrayTag[1].DataItems.Add(secondDataItem);
            globalDataItemArrayTag[2].DataItems.Add(thirdDataItem);
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

