#if !VNEXT_TARGET
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    class TagPathLookupIndexIdeTest
    {
        [Test]
        public void TestGetObjectWithPath()
        {
            // ARRANGE
            const string tagPart = "Tags", structPart = "Struct", memberPart = "Member";
            var projectManager = Substitute.For<IProjectManager>();

            var designerProjectItem = Substitute.For<IDesignerProjectItem>();
            designerProjectItem.Name = tagPart;
            var leaf = Substitute.For<IName>();
            leaf.Name = memberPart;

            var structObject = new StructFake { Name = structPart };
            var items = new List<object>{ leaf };
            structObject.Items = items;

            var root = new StructFake { Name = tagPart, Items = new List<object>(new[]{structObject}) };

            designerProjectItem.ContainedObject.Returns(root);
            var project = Substitute.For<IProject>();
            project.GetDesignerProjectItems().Returns(new[]{ designerProjectItem });

            projectManager.Project = project;

            var tagNotificationService = Substitute.For<ITagChangedNotificationServiceCF>();
            var globalController = Substitute.For<IGlobalController>();

            // ACT 
            var tagsPathLookupIndex = new TagsPathLookupIndexIde(globalController, projectManager, tagNotificationService);
            var result = tagsPathLookupIndex.GetObject<IName>($"{tagPart}.{structPart}.{memberPart}");

            //ASSERT
            Assert.IsTrue(result.Name == leaf.Name);
        }

        [Test]
        public void TestRootName()
        {
            // ARRANGE
            var tagNotificationService = Substitute.For<ITagChangedNotificationServiceCF>();
            var globalController = Substitute.For<IGlobalController>();
            var projectManager = Substitute.For<IProjectManager>();


            // ACT 
            var tagsPathLookupIndex = new TagsPathLookupIndexIde(globalController, projectManager, tagNotificationService);
            var result = tagsPathLookupIndex.RootName;

            // ASSERT
            Assert.IsTrue(result == StringConstants.Tags);
        }


        [Test]
        public void TestGetObjects()
        {
            // ARRANGE
            var tagNotificationService = Substitute.For<ITagChangedNotificationServiceCF>();
            var globalController = Substitute.For<IGlobalController>();
            globalController.Name = "globalController";
            var projectManager = Substitute.For<IProjectManager>();
            projectManager.Project = Substitute.For<IProject>();

            // ACT 
            var tagsPathLookupIndex = new TagsPathLookupIndexIde(globalController, projectManager, tagNotificationService);
            var result = tagsPathLookupIndex.GetObjects<IGlobalController>().Single();

            // ASSERT
            Assert.IsTrue(result.Name == "globalController");
        }

        private class StructFake : ISubItemsBase, IName
        {
            public ICollection Items { get; set; }
            public string Name { get; set; }
        }

    }
}
#endif
