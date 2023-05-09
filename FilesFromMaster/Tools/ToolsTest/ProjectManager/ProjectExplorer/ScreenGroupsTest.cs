using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager.ProjectExplorer
{
    [TestFixture]
    public class ScreenGroupsTest
    {
        private ScreenGroups m_ScreenGroups;
        private IScreenGroupServiceIde m_ScreenGroupServiceIde;
        private IProjectTreeItem m_Root;

        [SetUp]
        public void Setup()
        {
            m_Root = MockRepository.GenerateStub<IProjectTreeItem>();

            m_ScreenGroupServiceIde = TestHelper.AddServiceStub<IScreenGroupServiceIde>();
            m_ScreenGroupServiceIde.Stub(x => x.ScreenRoot).Return(m_Root);

            m_ScreenGroups = new ScreenGroups();
        }

        [TearDown]
        public void Teardown()
        {
            m_ScreenGroups.Clear();
            TestHelper.ClearServices();
        }

        [Test]
        public void WhenItemAddedToTheScreenRootTheListIsSorted()
        {
            m_ScreenGroups.Add(new ScreenGroupProjectItem("B"));
            m_ScreenGroups.Add(new ScreenGroupProjectItem("C"));

            Raise(x => x.ItemCreated += null,  new AddingNewEventArgs(new ScreenGroupProjectItem("A")));


            Assert.That(m_ScreenGroups[0].Name, Is.EqualTo("A"));
            Assert.That(m_ScreenGroups[1].Name, Is.EqualTo("B"));
            Assert.That(m_ScreenGroups[2].Name, Is.EqualTo("C"));
        }


        [Test]
        public void WhenItemAddedToTheScreenRootTheItemGetsContextualMenus()
        {
            ScreenGroupProjectItem itemToAdd = new ScreenGroupProjectItem("A");

            Assert.That(itemToAdd.ContextualMenuCommands, Is.Empty);

            Raise(x => x.ItemCreated += null, new AddingNewEventArgs(itemToAdd));

            AssertContextualMenusExistsInFirstScreenGroup();
        }

        private void AssertContextualMenusExistsInFirstScreenGroup()
        {
            AssertContextualMenuItemExists(m_ScreenGroups[0].ContextualCommands, TextsIde.ScreenGroupRenameTooltip);
            AssertContextualMenuItemExists(m_ScreenGroups[0].ContextualCommands, TextsIde.ScreenGroupDeleteToolTip);
            AssertContextualMenuItemExists(m_ScreenGroups[0].ContextualCommands, TextsIde.ScreenGroupAddScreenToGroupTooltip);
        }

        [Test]
        public void LoadsAndSortsScreenGroupsFromTheRootWhenLoadingGroups()
        {
            TestHelper.AddServiceStub<IProjectManager>();

            IScreenGroupProjectItem firstItem = new ScreenGroupProjectItem();
            firstItem.Name = "Item A";

            IScreenGroupProjectItem secondItem = new ScreenGroupProjectItem();
            secondItem.Name = "Item B";

            m_Root.Stub(x => x.ProjectItems).Return(new IProjectItem[] { secondItem, firstItem });

            m_ScreenGroups.LoadGroups();

            Assert.That(m_ScreenGroups.Count, Is.EqualTo(2), "It was not 2 screen groups in the collection!");
            Assert.That(m_ScreenGroups[0].Name, Is.EqualTo(firstItem.Name), "First screen group did not match the first item");
            Assert.That(m_ScreenGroups[1].Name, Is.EqualTo(secondItem.Name), "Second screen group did not match the second item");

        }

        [Test]
        public void AddsContextualMenusWhenLoadingGroups()
        {
            ScreenGroupProjectItem itemToLoad = new ScreenGroupProjectItem("A");
            m_Root.Stub(x => x.ProjectItems).Return(new IProjectItem[] { itemToLoad });

            m_ScreenGroups.LoadGroups();

            AssertContextualMenusExistsInFirstScreenGroup();
        }


        private static void AssertContextualMenuItemExists(IEnumerable<IMenuCommand> createdCommands, string title)
        {
            Assert.That(createdCommands.SingleOrDefault(x => x.Text.Equals(title)), Is.Not.Null);
        }


        private void Raise(Action<IProjectTreeItem> eventAction, EventArgs args) 
        {
            m_Root.Raise(eventAction, m_Root, args);
        }


    }
}
