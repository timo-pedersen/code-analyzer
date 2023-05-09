using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class DesignerProjectItemTest
    {
        private IProjectItem m_DesignerProjectItem;
        private IProjectItemFactory m_ProjectItemFactory;
        private IProjectManager m_ProjectManagerStub;
        private MockRepository m_Mocks;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            m_ProjectItemFactory = ProjectItemFactory.Instance;
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.ClearServices();
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_Mocks = new MockRepository();

            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManagerStub.Stub(projectManager => projectManager.ProjectActivity).Return(ProjectActivities.Inactive);

            var designerProjectItem = new DesignerProjectItem();
            designerProjectItem.ProjectManager = m_ProjectManagerStub;
            m_DesignerProjectItem = designerProjectItem;

            var nameCreationService = TestHelper.AddServiceStub<INameCreationService>();
            nameCreationService.Stub(x => x.IsValidFileName(Arg<string>.Is.Anything, ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), "").Dummy)).Return(true);
        }

        [Test]
        public void AddAddsChildProjectItem()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.Stub(projectManager => projectManager.IsProjectOpen).Return(true);

            iProjectItem.Name = "TestDesignername1";
            m_DesignerProjectItem.Add(iProjectItem);

            Assert.AreEqual(false, m_DesignerProjectItem.IsEmpty);
            Assert.AreEqual(1, m_DesignerProjectItem.Count);
        }

        [Test]
        public void DeleteDesigner()
        {
            IProjectItem designerProjectItem = m_Mocks.StrictMultiMock<IProjectItem>(typeof(IDisposable));

            const string DesignerItemName = "Form1";
            SetupResult.For(designerProjectItem.Name).Return(DesignerItemName);
            SetupResult.For(designerProjectItem.Group).Return(null);
            SetupResult.For(designerProjectItem.DependentUpon).Return(null);

            using (m_Mocks.Record())
            {
                designerProjectItem.Close();
                LastCall.Repeat.Once();
                designerProjectItem.NotifyToBeDeleted();
                LastCall.Repeat.Once();
                designerProjectItem.PropertyChanged += null;
                LastCall.IgnoreArguments();
                designerProjectItem.PropertyChanged -= null;
                LastCall.IgnoreArguments();
                Expect.Call(designerProjectItem.ShouldRemoveParentGroup).Repeat.Once().Return(false);
                m_ProjectManagerStub.Expect(x => x.FlagForDeletion(designerProjectItem));
            }

            using (m_Mocks.Playback())
            {
                m_DesignerProjectItem.Add(designerProjectItem);
                Assert.AreEqual(true, m_DesignerProjectItem.HasChild(DesignerItemName));

                m_DesignerProjectItem.DeleteChildItem(designerProjectItem);
            }

            Assert.AreEqual(false, m_DesignerProjectItem.HasChild(DesignerItemName), "ProjectItem is not empty after delete");
        }

        [Test]
        public void AddGroup()
        {
            IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");

            m_DesignerProjectItem.Add(groupProjectItem);

            Assert.AreEqual(false, m_DesignerProjectItem.IsEmpty);
            Assert.AreEqual(1, m_DesignerProjectItem.Count);
        }

        [Test]
        public void DeleteGroup()
        {
            using (m_Mocks.Record())
            {
            }

            using (m_Mocks.Playback())
            {
                IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");

                m_DesignerProjectItem.Add(groupProjectItem);
                Assert.AreEqual(false, m_DesignerProjectItem.IsEmpty);

                m_DesignerProjectItem.DeleteChildItem(groupProjectItem);
                Assert.AreEqual(true, m_DesignerProjectItem.IsEmpty);
            }
        }

        [Test]
        public void ItemHasGroup()
        {
            IProjectItem groupItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Keys");

            Assert.AreEqual(false, m_DesignerProjectItem.HasGroup("Keys"));

            m_DesignerProjectItem.Add(groupItem);

            Assert.AreEqual(true, m_DesignerProjectItem.HasGroup("Keys"));
        }

        [Test]
        public void ItemHasChild()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.Stub(projectManager => projectManager.IsProjectOpen).Return(true);

            iProjectItem.Name = "Color";
            Assert.AreEqual(false, m_DesignerProjectItem.HasChild("Color"));
            m_DesignerProjectItem.Add(iProjectItem);
            Assert.AreEqual(true, m_DesignerProjectItem.HasChild("Color"));
        }
    }
}
