using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class DesignerProjectItemTest
    {
        private IProjectItem m_DesignerProjectItem;
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
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_ProjectManagerStub = Substitute.For<IProjectManager>();
            m_ProjectManagerStub.ProjectActivity.Returns(ProjectActivities.Inactive);

            var designerProjectItem = new DesignerProjectItem();
            designerProjectItem.ProjectManager = m_ProjectManagerStub;
            m_DesignerProjectItem = designerProjectItem;

            var nameCreationService = TestHelper.AddServiceStub<INameCreationService>();
            nameCreationService.IsValidFileName(Arg.Any<string>(), ref Arg.Any<string>()).Returns(true);
        }

        [Test]
        public void AddAddsChildProjectItem()
        {
            IProjectItem iProjectItem = new DesignerProjectItem();
            DesignerProjectItem designerProjectItem = (DesignerProjectItem)iProjectItem;
            designerProjectItem.ProjectManager = m_ProjectManagerStub;

            m_ProjectManagerStub.IsProjectOpen.Returns(true);

            iProjectItem.Name = "TestDesignername1";
            m_DesignerProjectItem.Add(iProjectItem);

            Assert.AreEqual(false, m_DesignerProjectItem.IsEmpty);
            Assert.AreEqual(1, m_DesignerProjectItem.Count);
        }

        [Test]
        public void DeleteDesigner()
        {
            IProjectItem designerProjectItem = Substitute.For<IProjectItem, IDisposable>();

            const string DesignerItemName = "Form1";
            designerProjectItem.Name.Returns(DesignerItemName);
            designerProjectItem.Group.Returns(x => null);
            designerProjectItem.DependentUpon.Returns(x => null);

            designerProjectItem.PropertyChanged += null;
            designerProjectItem.PropertyChanged -= null;
            designerProjectItem.ShouldRemoveParentGroup.Returns(false);

            m_DesignerProjectItem.Add(designerProjectItem);
            Assert.AreEqual(true, m_DesignerProjectItem.HasChild(DesignerItemName));

            m_DesignerProjectItem.DeleteChildItem(designerProjectItem);

            Assert.AreEqual(false, m_DesignerProjectItem.HasChild(DesignerItemName), "ProjectItem is not empty after delete");
            designerProjectItem.Received(1).Close();
            designerProjectItem.Received(1).NotifyToBeDeleted();
            m_ProjectManagerStub.Received().FlagForDeletion(designerProjectItem);
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
            IProjectItem groupProjectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");

            m_DesignerProjectItem.Add(groupProjectItem);
            Assert.AreEqual(false, m_DesignerProjectItem.IsEmpty);

            m_DesignerProjectItem.DeleteChildItem(groupProjectItem);
            Assert.AreEqual(true, m_DesignerProjectItem.IsEmpty);
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

            m_ProjectManagerStub.IsProjectOpen.Returns(true);

            iProjectItem.Name = "Color";
            Assert.AreEqual(false, m_DesignerProjectItem.HasChild("Color"));
            m_DesignerProjectItem.Add(iProjectItem);
            Assert.AreEqual(true, m_DesignerProjectItem.HasChild("Color"));
        }
    }
}
