using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class GroupProjectItemTest
    {
        private IProjectItem m_GroupProjectItem;
        private IProjectItemFactory m_ProjectItemFactory;
        private MockRepository m_Mocks;
        private IProjectManager m_ProjectManagerStub;

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
        public void Setup()
        {
            TestHelper.ClearServices();

            m_Mocks = new MockRepository();
            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManagerStub.Stub(projectManager => projectManager.ProjectActivity).Return(ProjectActivities.Inactive);

            m_GroupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");
            ((ProjectItem)m_GroupProjectItem).ProjectManager = m_ProjectManagerStub;

            var nameCreationService = TestHelper.AddServiceStub<INameCreationService>();
            nameCreationService.Stub(x => x.IsValidFileName(Arg<string>.Is.Anything, ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), "").Dummy)).Return(true);
        }

        [Test]
        public void AddDesigner()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.Stub(projectManager => projectManager.IsProjectOpen).Return(true);

            iProjectItem.Name = "Test";
            m_GroupProjectItem.Add(iProjectItem);

            Assert.AreEqual(false, m_GroupProjectItem.IsEmpty);
            Assert.AreEqual(1, m_GroupProjectItem.Count);
        }

        [Test]
        public void DeleteDesigner()
        {
            IProjectItem designerProjectItem = m_Mocks.StrictMultiMock<IProjectItem>(typeof(IDisposable));

            SetupResult.For(designerProjectItem.Name).Return("Test");
            SetupResult.For(designerProjectItem.Group).Return(null);
            SetupResult.For(designerProjectItem.DependentUpon).Return(null);

            using (m_Mocks.Record())
            {
                designerProjectItem.Close();
                designerProjectItem.NotifyToBeDeleted();
                LastCall.Repeat.Once();
                designerProjectItem.PropertyChanged += null;
                LastCall.IgnoreArguments();
                designerProjectItem.PropertyChanged -= null;
                LastCall.IgnoreArguments();
                Expect.Call(designerProjectItem.ShouldRemoveParentGroup).Repeat.Once().Return(true);
            }

            using (m_Mocks.Playback())
            {
                m_GroupProjectItem.Add(designerProjectItem);
                Assert.IsFalse(m_GroupProjectItem.IsEmpty);

                m_GroupProjectItem.DeleteChildItem(designerProjectItem);
            }
            m_Mocks.VerifyAll();

            Assert.IsTrue(m_GroupProjectItem.IsEmpty);
        }

        [Test]
        public void AddGroup()
        {
            IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("SubForms");

            m_GroupProjectItem.Add(groupProjectItem);

            Assert.IsFalse(m_GroupProjectItem.IsEmpty);
            Assert.AreEqual(m_GroupProjectItem.Count, 1);
        }

        [Test]
        public void DeleteGroup()
        {
            using (m_Mocks.Record())
            {
            }

            using (m_Mocks.Playback())
            {
                IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("SubForms");

                m_GroupProjectItem.Add(groupProjectItem);
                Assert.IsFalse(m_GroupProjectItem.IsEmpty);

                m_GroupProjectItem.DeleteChildItem(groupProjectItem);
                Assert.IsTrue(m_GroupProjectItem.IsEmpty);
            }
        }

        [Test]
        public void ItemHasGroup()
        {
            IProjectItem groupItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Keys");

            Assert.AreEqual(false, m_GroupProjectItem.HasGroup("Keys"));

            m_GroupProjectItem.Add(groupItem);

            Assert.AreEqual(true, m_GroupProjectItem.HasGroup("Keys"));
        }

        [Test]
        public void ItemHasChild()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.Stub(projectManager => projectManager.IsProjectOpen).Return(true);

            iProjectItem.Name = "Form1";
            Assert.AreEqual(false, m_GroupProjectItem.HasChild("Form1"));
            m_GroupProjectItem.Add(iProjectItem);
            Assert.AreEqual(true, m_GroupProjectItem.HasChild("Form1"));
        }
    }
}
