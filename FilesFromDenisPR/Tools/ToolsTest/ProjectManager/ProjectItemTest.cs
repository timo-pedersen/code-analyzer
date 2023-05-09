using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectItemTest
    {
        private IEventSubscriber m_Subscriber;
        private IProjectTreeItem m_ProjectTreeItem;
        private IProjectItem m_ProjectItem;
        private IProjectManager m_ProjectManagerStub;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_ProjectManagerStub = Substitute.For<IProjectManager>();
            m_ProjectManagerStub.ProjectActivity.Returns(ProjectActivities.Inactive);

            m_ProjectTreeItem = new ProjectItem();
            ((ProjectItem)m_ProjectTreeItem).ProjectManager = m_ProjectManagerStub;

            m_ProjectItem = new ProjectItem();
            ((ProjectItem)m_ProjectItem).ProjectManager = m_ProjectManagerStub;

            m_Subscriber = Substitute.For<IEventSubscriber>();
        }

        [Test]
        public void NameSetterFiresItemChanged()
        {
            m_ProjectTreeItem.ItemChanged += m_Subscriber.Handler;

            m_Subscriber.Handler(m_ProjectTreeItem, EventArgs.Empty);
            
            m_ProjectTreeItem.Name = "olle";

            // Assert ??
        }

        [Test]
        public void ItemChangedEventFiresDirty()
        {
            m_ProjectItem.Dirty += m_Subscriber.Handler;

            m_Subscriber.Handler(m_ProjectItem, EventArgs.Empty);

            m_ProjectItem.FireItemChanged();
            
            // Assert ???
        }

        [Test]
        public void TwoItemChangedEventsFiresDirtyOnce()
        {
            m_ProjectItem.Dirty += m_Subscriber.Handler;

            m_Subscriber.Handler(m_ProjectItem, EventArgs.Empty);

            m_ProjectItem.FireItemChanged();
            m_ProjectItem.FireItemChanged();
            
            // Assert ???
        }

        [Test]
        public void ResetDirtySetsDirtyFalseOnChildren()
        {
            var projectItemChild = new ProjectItem("child");
            projectItemChild.ProjectManager = m_ProjectManagerStub;
            IProjectItem child = projectItemChild;

            m_ProjectItem.Add(child);
            child.FireItemChanged();

            m_ProjectItem.ResetDirty();

            Assert.IsFalse(child.IsDirty);
        }

        [Test]
        public void SaveResetsDirty()
        {
            m_ProjectItem.Save();

            Assert.IsFalse(m_ProjectItem.IsDirty);
        }

        [Test]
        public void LoadResetsDirty()
        {
            m_ProjectItem.Load("");

            Assert.IsFalse(m_ProjectItem.IsDirty);
        }

        [Test]
        public void ProjectItemIsDirtyIfChildIsDirty()
        {
            var projectItemChild = new ProjectItem("child");
            projectItemChild.ProjectManager = m_ProjectManagerStub;
            IProjectItem child = projectItemChild;

            m_ProjectItem.Add(child);
            m_ProjectItem.ResetDirty();

            child.FireItemChanged();

            Assert.IsTrue(m_ProjectItem.IsDirty);
        }

        [Test]
        public void ProjectItemDoesNotLoadChildrenThatSupportsUnloading()
        {
            // Arrange
            IProjectItem projectItemSupportingUnload = Substitute.For<ProjectItem>();
            projectItemSupportingUnload.Name.Returns("SomeLazyProjectItem");

            projectItemSupportingUnload.SupportsUnloading.Returns(true);

            // Act
            m_ProjectItem.Add(projectItemSupportingUnload);
            m_ProjectItem.Load(null);

            // Assert
            projectItemSupportingUnload.DidNotReceive().Load();
            projectItemSupportingUnload.DidNotReceive().Load(null);
        }

        [Test]
        public void ProjectItemLoadsChildrenThatSupportsDoesNotSupportUnloading()
        {
            // Arrange
            IProjectItem projectItemSupportingUnload = Substitute.For<ProjectItem>();
            projectItemSupportingUnload.Name.Returns("ProjectItem123");
            projectItemSupportingUnload.SupportsUnloading.Returns(false);

            // Act
            m_ProjectItem.Add(projectItemSupportingUnload);
            m_ProjectItem.Load(null);
            
            // Assert
            projectItemSupportingUnload.Received(1).Load();
        }
    }
}
