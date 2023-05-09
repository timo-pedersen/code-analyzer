using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.OpcClient.Validation;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{

    [TestFixture]
    public class AllDataItemsBelongToATagTest
    {
        private List<string> m_Messages;
        private DataSourceContainer m_DataSourceContainer;
        private IGlobalController m_Controller;

        private const string ItemName = "DATA_ITEM_NAME_THAT_DOESNT_EXIST_IN_STRUCTURED_TAG_OR_IN_ORDINARY_TAGS";
        private const string ItemId = "D0";
        private const string MatchingName = ItemName;
        private const string NotMatchingName = "NOT_MATCHING_NAME";

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.ClearServices();
            m_Messages = new List<string>();
            var opcClient = MockRepository.GenerateStub<IOpcClientServiceIde>();
            m_Controller = MockRepository.GenerateStub<IGlobalController>();

            opcClient.Stub(item => item.GlobalController).Return(m_Controller);
            m_DataSourceContainer = new DataSourceContainer();

            opcClient.Stub(item => item.Controllers).Return(new ExtendedBindingList<IDataSourceContainer> { m_DataSourceContainer });
            ServiceContainerCF.Instance.AddService(opcClient);

            var errorListService = MockRepository.GenerateStub<IErrorListService>();
            errorListService.Stub(item => item.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).WhenCalled(x => m_Messages.Add(x.Arguments[0].ToString()));

            ServiceContainerCF.Instance.AddService(errorListService);

            TestHelper.AddService<IDataItemCountingService>(new OpcClientToolIde());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void TestValidationDoesntFindsCorrespondingStructuredTagWhenNoTagsExists()
        {
            m_Controller.Stub(item => item.GlobalStructuredTags).Return(new OwnedList<ITag>(MockRepository.GenerateStub<IComponent>()));
            m_Controller.Stub(item => item.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>()));

            CommonValidationDoesntFindCorrespondingTag();
        }


        [Test]
        public void TestValidationDoesntFindCorrespondingTagWhenAnotherStructuredTagsExists()
        {
            PrepareForValidationWhenAStructuredTagExists(NotMatchingName);
            CommonValidationDoesntFindCorrespondingTag();
        }


        [Test]
        public void TestValidationFindsCorrespondingTagWhenMatchingStructuredTagExists()
        {
            PrepareForValidationWhenAStructuredTagExists(MatchingName);
            CommonValidationFindsCorrespondingTag();
        }


        [Test]
        public void TestValidationDoesntFindCorrespondingTagWhenAnotherOrdinaryTagExists()
        {
            PrepareForValidationWhenAnOrdinaryTagExists(NotMatchingName);
            CommonValidationDoesntFindCorrespondingTag();
        }


        [Test]
        public void TestValidationFindsCorrespondingTagWhenMatchingOrdinaryTagExists()
        {
            PrepareForValidationWhenAnOrdinaryTagExists(MatchingName);
            CommonValidationFindsCorrespondingTag();
        }

        private void PrepareForValidationWhenAStructuredTagExists(string subItemDataItemName)
        {
            m_Controller.Stub(item => item.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>()));

            var instanceMapping = new StructuredTagInstanceMapping { Name = "testMapping", TypeName = "NA" };
            instanceMapping.GlobalDataItemMappings.Add(new GlobalDataItemMappingCF("testmapping"));
            instanceMapping.GlobalDataItemMappings[0].GlobalDataSubItems.Add(new GlobalDataSubItem(0, 0, new[] { "IrrelevantControllerName" }, new[] { subItemDataItemName }));

            var fakeTags = new OwnedList<ITag>(MockRepository.GenerateStub<IComponent>());
            var instance = new StructuredTagInstance("test", "Test", instanceMapping);
            fakeTags.Add(instance);

            m_Controller.Stub(item => item.GlobalStructuredTags).Return(fakeTags);
        }

        private void PrepareForValidationWhenAnOrdinaryTagExists(string subItemDataItemName)
        {
            m_Controller.Stub(item => item.GlobalStructuredTags).Return(new OwnedList<ITag>(MockRepository.GenerateStub<IComponent>()));
            IGlobalDataItem globalDataItemStub = MockRepository.GenerateStub<IGlobalDataItem>();

            var subItem = MockRepository.GenerateStub<IGlobalDataSubItem>();
            subItem.Stub(inv => inv.DataItems).Return(new DataItems(null) { new DataItem { Name = subItemDataItemName, ItemID = ItemId } });
            var subItems = new BindingList<IGlobalDataSubItem>() { subItem };
            globalDataItemStub.Stub(inv => inv.GlobalDataSubItems).Return(subItems);
            List<IGlobalDataItem> globaldataItems = new List<IGlobalDataItem>() { globalDataItemStub };
            m_Controller.Stub(item => item.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(globaldataItems.Cast<IDataItemBase>().ToList()));

            StructuredTagInstanceMapping instanceMapping = new StructuredTagInstanceMapping { Name = "testTagMapping", TypeName = "NA" };
            instanceMapping.GlobalDataItemMappings.Add(new GlobalDataItemMappingCF("testmapping"));

            OwnedList<ITag> fakeTags = new OwnedList<ITag>(MockRepository.GenerateStub<IComponent>());

            StructuredTagInstance structInstance = new StructuredTagInstance("test", "Test", instanceMapping);
            structInstance.InstanceMapping.GlobalDataItemMappings[0].GlobalDataSubItems.Add(new GlobalDataSubItem(0, 0, new[] { "IrrelevantControllerName" }, new[] { subItemDataItemName }));
            fakeTags.Add(structInstance);

            m_Controller.Stub(item => item.GlobalStructuredTags).Return(fakeTags);
        }

        private void CommonValidationDoesntFindCorrespondingTag()
        {

            m_Messages.Clear();
            try
            {
                m_DataSourceContainer.DataItems.Add(new DataItem { Name = ItemName, ItemID = ItemId });
                var validator = new AllDataItemsBelongToATagProjectValidator();
                bool result = validator.Validate();
                // this validator emits true even though errors exists because they are "warnings"
                Assert.IsTrue(result);
                // check that the validator emits the warning
                Assert.AreEqual(m_Messages.Count, 1);
                Assert.IsTrue(m_Messages[0].Contains(ItemName));
            }
            finally
            {
                m_Messages.Clear();
                m_DataSourceContainer.DataItems.Clear();
            }
        }

        private void CommonValidationFindsCorrespondingTag()
        {

            m_Messages.Clear();
            try
            {
                m_DataSourceContainer.DataItems.Add(new DataItem { Name = ItemName, ItemID = ItemId });
                var validator = new AllDataItemsBelongToATagProjectValidator();
                bool result = validator.Validate();
                // this validator emits true even though errors exists because they are "warnings"
                Assert.IsTrue(result);
                // check that the validator emits the warning
                Assert.AreEqual(m_Messages.Count, 0);
            }
            finally
            {
                m_Messages.Clear();
                m_DataSourceContainer.DataItems.Clear();
            }
        }
    }
}
