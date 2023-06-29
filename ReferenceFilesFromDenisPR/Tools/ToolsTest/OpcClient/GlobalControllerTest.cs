#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.Lifecycle;
using Core.Api.Tools;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.OpcClient;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient.Utilities;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using Neo.ApplicationFramework.Tools.StructuredTags.Common;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class GlobalControllerTest
    {
        private IDataItemCountingService m_DataItemCountingService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            NeoDesignerProperties.IsInDesignMode = true;

            IOpcClientServiceCF opcClientService = TestHelper.AddServiceStub<IOpcClientServiceCF>();
            opcClientService.Controllers.Returns(new ExtendedBindingList<IDataSourceContainer>());

            m_DataItemCountingService = TestHelper.CreateAndAddServiceStub<IDataItemCountingService>();

            StructuredTagsTestBase.CreateStructuredTagServiceStub();
        }

        [Test]
        public void DisposingGlobalControllerNoConnectedGlobalDataItemsDoesNotDecreasesDataItemCount()
        {
            using (new GlobalController())
                m_DataItemCountingService.ConnectedDataItems.Returns(0);

            m_DataItemCountingService.Received(1).RemoveConnectedDataItems(0);
        }

        [Test]
        public void DisposingGlobalControllerWithOneConnectedGlobalDataItemDecreasesDataItemCount()
        {
            using (new GlobalController())
                m_DataItemCountingService.ConnectedDataItems.Returns(1);

            m_DataItemCountingService.Received(1).RemoveConnectedDataItems(1);
        }

        [Test]
        public void DisposingGlobalControllerWithTwoConnectedGlobalDataItemDecreasesDataItemCountByTwo()
        {
            using (new GlobalController())
                m_DataItemCountingService.ConnectedDataItems.Returns(2);

            m_DataItemCountingService.Received(1).RemoveConnectedDataItems(2);
        }

        [Test]
        public void GetAllOrdinaryTagsTest()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags).ToArray();

                string[] tagsToFind = GetExpectedOrdinaryTags();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllStructuredTagsTest()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.StructuredTags).ToArray();

                string[] tagsToFind = GetExpectedStructuredTags();
                ValidateTagsExists(tagsToFind, result);
            }
        }
        
        [Test]
        public void GetSumOffAllTagsUnflattened()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.StructuredTags | TagsPredicate.Tags).ToArray();

                string[] tagsToFind = GetExpectedOrdinaryTags().Concat(GetExpectedStructuredTags()).ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetSumOffAllTagsFlattened()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.StructuredTags | TagsPredicate.Tags | TagsPredicate.FlattenHierarchy).ToArray();

                string[] tagsToFind = GetExpectedOrdinaryTags().Concat(
                    new[]
                    {
                        "tag1", "tag2"
                    }).ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrdinaryTagsIgnoreSystemTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreSystemTags).ToArray();
                string[] tagsToFind = GetExpectedOrdinaryTags().Except(GetExpectedSystemTags()).ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrdinaryTagsExcludeArrayTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreArrayTags).ToArray();
                string[] tagsToFind = GetExpectedOrdinaryTags().Except(GetExpectedArrayTags()).ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrinaryTagsExcludingOneValueTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreOneValueTags).ToArray();
                string[] tagsToFind = GetExpectedArrayTags().ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrdinaryTagsExcludeArrayTagsAndSystemTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreArrayTags | TagsPredicate.IgnoreSystemTags).ToArray();
                string[] tagsToFind = new[] { "OrdinaryTag1", "OrdinaryTag2" };
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrdinaryTagsExcludeArrayTagsAndOneValueTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreArrayTags | TagsPredicate.IgnoreOneValueTags).ToArray();
                Assert.That(result.Count(), Is.EqualTo(0));
            }
        }

        [Test]
        public void GetAllOrdinaryTagsExcludeSystemTagsAndOneValueTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(TagsPredicate.Tags | TagsPredicate.IgnoreSystemTags | TagsPredicate.IgnoreOneValueTags).ToArray();
                string[] tagsToFind = GetExpectedArrayTags().ToArray();
                ValidateTagsExists(tagsToFind, result);
            }
        }

        [Test]
        public void GetAllOrdinaryTagsExcludeArrayTagsAndSystemTagsAndOneValueTags()
        {
            using (IGlobalController globalController = GetInitializedIGlobalController())
            {
                ITag[] result = globalController.GetAllTags<ITag>(
                    TagsPredicate.Tags |
                    TagsPredicate.IgnoreArrayTags |
                    TagsPredicate.IgnoreSystemTags |
                    TagsPredicate.IgnoreOneValueTags).ToArray();
                Assert.That(result.Count(), Is.EqualTo(0));
            }
        }

        [Test]
        public void ExceptionIsThrownInRunForInternalTagsWhichTriggersExceptionOnValueChange()
        {
            using (var globalController = new GlobalController())
            {
                var tagTraverser = new RuntimeTagTraverser();
                var internalVariable = new GlobalDataItem { Name = "InternalTag" };

                TestHelper.AddService<ITagTraverser>(tagTraverser);
                internalVariable.ValueChange += (sender, args) => { throw new Exception(); };
                globalController.GlobalDataItems.Add(internalVariable);

                //Needs to be removed to make it possible to set RunTime = true (needs to be false from beginning)
                TestHelper.RemoveService<IToolManager>();

                var toolManager = Substitute.For<IToolManager>();
                toolManager.Runtime.Returns(true);
                TestHelper.AddService(toolManager);

                var startup = globalController as IStartup;

                NeoDesignerProperties.IsInDesignMode = false;
                Assert.Throws<Exception>(startup.Run);
            }
        }

        private static void ValidateTagsExists(string[] tagsToFind, ITag[] result)
        {
            Assert.AreEqual(tagsToFind.Count(), result.Count());
            foreach (string tagName in tagsToFind)
            {
                Assert.AreEqual(1, result.Count(item => item.Name == tagName));
            }
        }

        private static string[] GetExpectedOrdinaryTags()
        {
            return new[] { "OrdinaryTag1", "OrdinaryTag2" }.Concat(GetExpectedArrayTags()).Concat(GetExpectedSystemTags()).ToArray();
        }

        private static IEnumerable<string> GetExpectedSystemTags()
        {
            return new [] {"SystemTag1", "SystemTag2"};
        }

        private static IEnumerable<string> GetExpectedArrayTags()
        {
            yield return "arrayTag1WhichIsAlsoAnOrdinaryTag";
        }

        private static string[] GetExpectedStructuredTags()
        {
            return new[] { "struct1", "struct2" };
        }

        private static IGlobalController GetInitializedIGlobalController()
        {
            var globalController = new GlobalController();

            // Ordinary tag
            globalController.GlobalDataItems.Add(new GlobalDataItem { Name = "OrdinaryTag1" });
            globalController.GlobalDataItems.Add(new GlobalDataItem { Name = "OrdinaryTag2" });

            // Ordinary array tag
            var arrayTag = new GlobalDataItem { Name = "arrayTag1WhichIsAlsoAnOrdinaryTag", ArraySize = 2 };
            arrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem(0));
            arrayTag.GlobalDataSubItems.Add(new GlobalDataSubItem(1));
            globalController.GlobalDataItems.Add(arrayTag);

            // System tags
            globalController.GlobalDataItems.Add(new SystemDataItem { Name = "SystemTag1" });
            globalController.GlobalDataItems.Add(new SystemDataItem { Name = "SystemTag2" });

            // Structured tags instance
            var struct1 = new StructuredTagInstance { Name = "struct1" };
            struct1.InstanceMapping = new StructuredTagInstanceMapping("struct1");
            var tag1InStruct1 = new GlobalDataItemMappingCF("tag1");
            var tag2InStruct1 = new GlobalDataItemMappingCF("tag2");
            struct1.InstanceMapping.GlobalDataItemMappings.Add(tag1InStruct1);
            struct1.InstanceMapping.GlobalDataItemMappings.Add(tag2InStruct1);

            var tagTraverser = Substitute.For<ITagTraverser>();
            TestHelper.AddService<ITagTraverser>(tagTraverser);
            tagTraverser
                .GetFlattenedDataItems(Arg.Any<ITag[]>())
                .Returns(new[]
                {
                    new GlobalDataItem { Name = tag1InStruct1.Name },
                    new GlobalDataItem { Name = tag2InStruct1.Name }
                });

            globalController.GlobalStructuredTags.Add(struct1);
            globalController.GlobalStructuredTags.Add(new StructuredTagInstance { Name = "struct2" });
            return globalController;
        }
    }
}
#endif
