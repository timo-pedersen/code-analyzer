using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Services;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTagsViewer;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTagsViewer.Private;
using Neo.ApplicationFramework.Tools.StructuredTags.Common;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class StructuredTagsViewerFacadeTest : StructuredTagsTestBase
    {
        private IStructuredTagsViewerFacade m_StructuredTagsViewerFacade;
        private IStructuredTagService m_StructuredTagService;
        private INameService m_NameService;
        private const string DoesNotExists = "DoesNotExists";

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            base.SetUpBase();
            //SetupFromOtherTest();
            m_StructuredTagService = MockRepository.GenerateStub<IStructuredTagService>();
            m_NameService = MockRepository.GenerateStub<INameService>();

            m_StructuredTagService.Stub(inv => inv.DeleteStructuredTagInstance(Arg<string>.Is.Equal(DoesNotExists))).Throw(new Exception("Tag does not exist"));
            m_StructuredTagService.Stub(
                inv => inv.DeleteStructuredTagInstance(Arg<string>.Is.NotEqual(DoesNotExists)))
                    .WhenCalled(
                        (arg) => m_StructuredTagService.Raise(x => x.TagsChanged += null, m_StructuredTagService, EventArgs.Empty));

            m_StructuredTagsViewerFacade = new StructuredTagsViewerFacade(OpcClientServiceIde, StructuredTypeService, m_StructuredTagService, m_NameService);
        }
        
        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
            base.TearDownBase();
        }

        [Test]
        [TestCase(new[] { 1, 2 }, new[] { 1, 2 })]
        [TestCase(new int[0], new int[0])]
        [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestGetTags(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            // setup the data
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();
            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);


            // do the work to be evaluated
            ITagEntity[] tagEntities = m_StructuredTagsViewerFacade.GetTags().ToArray();

            // check for validity
            Assert.AreEqual(1, tagEntities.Count());
            var entity = tagEntities.Single();
            var children = entity.Children.ToArray();
            int[] allItems = dataitemMemberPostFixNr.Concat(structuredMemberPostFixNr).ToArray();
            Assert.AreEqual(allItems.Count(), children.Count());
            for (int i = 0; i < children.Count(); i++)
            {
                string name = children[i].Name;
                var characters = new string(name.Where(Char.IsNumber).ToArray());
                int number = Int16.Parse(characters);
                Assert.Contains(number, allItems);
            }
        }

        [Test]
        [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 })]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestDeleteTag(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            // setup the data
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();
            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);

            // do the work to be evaluated
            Assert.Throws<Exception>(() => m_StructuredTagsViewerFacade.DeleteTag(DoesNotExists));

        }

        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestDeleteTagWhenOthersExists(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            // setup the data
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();
            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);

            // do the work to be evaluated
            Assert.Throws<Exception>(() => m_StructuredTagsViewerFacade.DeleteTag(DoesNotExists));
        }

        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestDeleteTagWhenNoExists(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            Assert.Throws<Exception>(() => m_StructuredTagsViewerFacade.DeleteTag(DoesNotExists));
        }

        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestDeleteTagWhenItExists(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            // setup the data
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();

            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);

            m_StructuredTagsViewerFacade.DeleteTag("tag1");

        }

        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestEventWhenManipulatingList(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            // setup the data
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();

            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);

            int eventFiredOnDelete = 0;

            m_StructuredTagsViewerFacade.TagsChanged += (sender, args) => eventFiredOnDelete++;

            m_StructuredTagsViewerFacade.DeleteTag("tag1");

            Assert.IsTrue(eventFiredOnDelete == 1);

        }


        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 })]
        public void TestEventIsFiredOnAdd(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg)
        {
            var dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            var structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();

            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);
        }

        [Test]
        public void TestTagsChangedRegistration()
        {
            m_StructuredTagsViewerFacade.TagsChanged += StructuredTagsViewerFacadeOnTagsChanged;
            m_StructuredTagService.AssertWasCalled(inv => inv.TagsChanged += Arg<EventHandler>.Is.Anything);
        }

        [Test]
        public void TestTagsChangedDeregistration()
        {
            m_StructuredTagsViewerFacade.TagsChanged -= StructuredTagsViewerFacadeOnTagsChanged;
            m_StructuredTagService.AssertWasCalled(inv => inv.TagsChanged -= Arg<EventHandler>.Is.Anything);
        }

        [Test]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 }, "tag1")]
        [TestCase(new[] { 1, 12, 16 }, new[] { 1, 12, 16 }, "aw2")]
        public void TestRenameTag(IEnumerable<int> dataitemMemberPostFixNrArg, IEnumerable<int> structuredMemberPostFixNrArg, string tagToRename)
        {
            int[] dataitemMemberPostFixNr = dataitemMemberPostFixNrArg.ToArray();
            int[] structuredMemberPostFixNr = structuredMemberPostFixNrArg.ToArray();

            PreprareTestWhenMembersAreTheSame(dataitemMemberPostFixNr, structuredMemberPostFixNr);

            string newTagName = "abc";
            bool newTagAlreadyExisted = m_StructuredTagsViewerFacade.GetTags().Any(x => x.Name == newTagName);
            bool renamedTagExistedBefore = m_StructuredTagsViewerFacade.GetTags().Any(x => x.Name == tagToRename);
            m_NameService.Stub(x => x.RenameObject(Arg<object>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).WhenCalled(x => ((ITag)x.Arguments[0]).Name = newTagName);

            bool renamed = m_StructuredTagsViewerFacade.RenameTag(tagToRename, newTagName);
            bool renamedTagExistsAfter = m_StructuredTagsViewerFacade.GetTags().Any(x => x.Name == tagToRename);

            Assert.IsTrue(renamed == renamedTagExistedBefore);
            Assert.IsTrue(renamedTagExistedBefore == renamed && !renamedTagExistsAfter);
            Assert.IsFalse(newTagAlreadyExisted);
        }

        private void StructuredTagsViewerFacadeOnTagsChanged(object sender, EventArgs eventArgs) { }

    }
}
