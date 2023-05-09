using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            var projectManager = MockRepository.GenerateStub<IProjectManager>();

            var designerProjectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            designerProjectItem.Name = tagPart;
            var leaf = MockRepository.GenerateStub<IName>();
            leaf.Name = memberPart;

            var structObject = new StructFake { Name = structPart };
            var items = new List<object>{ leaf };
            structObject.Items = items;

            var root = new StructFake { Name = tagPart, Items = new List<object>(new[]{structObject}) };

            designerProjectItem.Stub(pi => pi.ContainedObject).Return(root);
            var project = MockRepository.GenerateStub<IProject>();
            project.Stub(p => p.GetDesignerProjectItems()).Return(new[]{ designerProjectItem });

            projectManager.Project = project;

            var tagNotificationService = MockRepository.GenerateStub<ITagChangedNotificationServiceCF>();
            var globalController = MockRepository.GenerateStub<IGlobalController>();

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
            var tagNotificationService = MockRepository.GenerateStub<ITagChangedNotificationServiceCF>();
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            var projectManager = MockRepository.GenerateStub<IProjectManager>();


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
            var tagNotificationService = MockRepository.GenerateStub<ITagChangedNotificationServiceCF>();
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            globalController.Name = "globalController";
            var projectManager = MockRepository.GenerateStub<IProjectManager>();
            projectManager.Project = MockRepository.GenerateStub<IProject>();

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
