using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectItemTest
    {
        private readonly MockRepository m_Mocks = new MockRepository();
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

            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManagerStub.Stub(projectManager => projectManager.ProjectActivity).Return(ProjectActivities.Inactive);

            m_ProjectTreeItem = new ProjectItem();
            ((ProjectItem)m_ProjectTreeItem).ProjectManager = m_ProjectManagerStub;

            m_ProjectItem = new ProjectItem();
            ((ProjectItem)m_ProjectItem).ProjectManager = m_ProjectManagerStub;

            m_Subscriber = m_Mocks.StrictMock<IEventSubscriber>();
        }

        [Test]
        public void NameSetterFiresItemChanged()
        {
            using (m_Mocks.Record())
            {
                m_ProjectTreeItem.ItemChanged += m_Subscriber.Handler;

                m_Subscriber.Handler(m_ProjectTreeItem, EventArgs.Empty);
            }

            using (m_Mocks.Playback())
            {
                m_ProjectTreeItem.Name = "olle";
            }
            m_Mocks.VerifyAll();
        }

        [Test]
        public void ItemChangedEventFiresDirty()
        {

            using (m_Mocks.Record())
            {
                m_ProjectItem.Dirty += m_Subscriber.Handler;

                m_Subscriber.Handler(m_ProjectItem, EventArgs.Empty);
            }

            using (m_Mocks.Playback())
            {
                m_ProjectItem.FireItemChanged();
            }
            m_Mocks.VerifyAll();
        }

        [Test]
        public void TwoItemChangedEventsFiresDirtyOnce()
        {
            using (m_Mocks.Record())
            {
                m_ProjectItem.Dirty += m_Subscriber.Handler;

                m_Subscriber.Handler(m_ProjectItem, EventArgs.Empty);
            }

            using (m_Mocks.Playback())
            {
                m_ProjectItem.FireItemChanged();
                m_ProjectItem.FireItemChanged();
            }
            m_Mocks.VerifyAll();
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
            IProjectItem projectItemSupportingUnload = m_Mocks.PartialMock<ProjectItem>();
            SetupResult.For(projectItemSupportingUnload.Name).Return("SomeLazyProjectItem");

            using (m_Mocks.Record())
            {
                Expect.Call(projectItemSupportingUnload.SupportsUnloading).Return(true);

                projectItemSupportingUnload.Load();
                LastCall.IgnoreArguments().Repeat.Never();

                projectItemSupportingUnload.Load(null);
                LastCall.IgnoreArguments().Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                m_ProjectItem.Add(projectItemSupportingUnload);

                m_ProjectItem.Load(null);
            }
            m_Mocks.VerifyAll();
        }

        [Test]
        public void ProjectItemLoadsChildrenThatSupportsDoesNotSupportUnloading()
        {
            IProjectItem projectItemSupportingUnload = m_Mocks.PartialMock<ProjectItem>();
            SetupResult.For(projectItemSupportingUnload.Name).Return("ProjectItem123");

            using (m_Mocks.Record())
            {
                Expect.Call(projectItemSupportingUnload.SupportsUnloading).Return(false);

                projectItemSupportingUnload.Load();
                LastCall.IgnoreArguments().Repeat.Once();
            }

            using (m_Mocks.Playback())
            {
                m_ProjectItem.Add(projectItemSupportingUnload);

                m_ProjectItem.Load(null);
            }
            m_Mocks.VerifyAll();
        }

    }
}
