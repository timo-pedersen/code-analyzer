using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class GroupProjectItemTest
    {
        private IProjectItem m_GroupProjectItem;
        private IProjectItemFactory m_ProjectItemFactory;
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

            m_ProjectManagerStub = Substitute.For<IProjectManager>();
            m_ProjectManagerStub.ProjectActivity.Returns(ProjectActivities.Inactive);

            m_GroupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");
            ((ProjectItem)m_GroupProjectItem).ProjectManager = m_ProjectManagerStub;

            var nameCreationService = TestHelper.AddServiceStub<INameCreationService>();
            nameCreationService.IsValidFileName(Arg.Any<string>(), ref Arg.Any<string>()).Returns(true);
        }

        [Test]
        public void AddDesigner()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.IsProjectOpen.Returns(true);

            iProjectItem.Name = "Test";
            m_GroupProjectItem.Add(iProjectItem);

            Assert.AreEqual(false, m_GroupProjectItem.IsEmpty);
            Assert.AreEqual(1, m_GroupProjectItem.Count);
        }

        [Test]
        public void DeleteDesigner()
        {
            IProjectItem designerProjectItem = Substitute.For<IProjectItem, IDisposable>();

            designerProjectItem.Name.Returns("Test");
            designerProjectItem.Group.Returns(x => null);
            designerProjectItem.DependentUpon.Returns(x => null);

            designerProjectItem.ShouldRemoveParentGroup.Returns(true);

            m_GroupProjectItem.Add(designerProjectItem);
            Assert.IsFalse(m_GroupProjectItem.IsEmpty);

            m_GroupProjectItem.DeleteChildItem(designerProjectItem);

            Assert.IsTrue(m_GroupProjectItem.IsEmpty);
            designerProjectItem.Received().Close();
            designerProjectItem.Received(1).NotifyToBeDeleted();
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
            IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("SubForms");

            m_GroupProjectItem.Add(groupProjectItem);
            Assert.IsFalse(m_GroupProjectItem.IsEmpty);

            m_GroupProjectItem.DeleteChildItem(groupProjectItem);
            Assert.IsTrue(m_GroupProjectItem.IsEmpty);
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

            m_ProjectManagerStub.IsProjectOpen.Returns(true);

            iProjectItem.Name = "Form1";
            Assert.AreEqual(false, m_GroupProjectItem.HasChild("Form1"));
            m_GroupProjectItem.Add(iProjectItem);
            Assert.AreEqual(true, m_GroupProjectItem.HasChild("Form1"));
        }
    }
}
