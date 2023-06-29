using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ScreenGroupProjectItemTest
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceStub<IProjectManager>();
        }

        [Test]
        public void CanMoveChildToParent()
        {
            ScreenGroupProjectItem group = new ScreenGroupProjectItem("Group A");
            ScreenDesignerProjectItem child = new ScreenDesignerProjectItem("Child");
            group.ProjectItems.Add(child);
            ScreenGroupProjectItem parent = new ScreenGroupProjectItem("Screens");
            group.Parent = parent;

            group.MoveChildToParent(child);

            Assert.That(group.ProjectItems.Contains(child), Is.False);
            Assert.That(parent.ProjectItems.Contains(child), Is.True);
            Assert.That(child.Group, Is.EqualTo(parent.Name));
        }

        [Test]
        public void CanMoveChildFromParentGroup()
        {
            ScreenGroupProjectItem group = new ScreenGroupProjectItem("Group A");
            ScreenDesignerProjectItem child = new ScreenDesignerProjectItem("Child");
            ScreenGroupProjectItem parent = new ScreenGroupProjectItem("Screens");
            parent.ProjectItems.Add(child);

            group.Parent = parent;

            group.MoveChildFromParentGroup(child);

            Assert.That(group.ProjectItems.Contains(child), Is.True);
        }

        [Test]
        public void CantMoveChildFromParentGroupWhenParentIsntAGroup()
        {
            ScreenGroupProjectItem group = new ScreenGroupProjectItem("Group A");

            ProjectItem parent = new ProjectItem("Parent");
            ScreenDesignerProjectItem child = new ScreenDesignerProjectItem("Child A");
            parent.ProjectItems.Add(child);
            group.Parent = parent;

            Assert.Throws<NotSupportedException>(() => group.MoveChildFromParentGroup(child));
        }
    }
}
