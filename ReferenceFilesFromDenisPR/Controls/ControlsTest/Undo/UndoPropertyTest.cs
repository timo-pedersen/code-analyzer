#if!VNEXT_TARGET
using System.Windows;
using System.Windows.Shapes;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Undo
{
    [TestFixture]
    public class UndoPropertyTest : UndoTestBase
    {
        [Test]
        public void UndosOfSameTypeShouldBeMerged()
        {
            UndoProperty undoPropertyOne = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);
            IUndoUnit undoPropertyTwo = new UndoProperty(RectangleTwo, Rectangle.VisibilityProperty.Name, DesignerHost);

            bool isMerged = undoPropertyTwo.Merge(undoPropertyOne);

            Assert.IsTrue(isMerged);
            Assert.AreEqual(2, undoPropertyOne.ElementNames.Count);
            Assert.AreEqual(2, undoPropertyOne.UndoPropertyValues.Count);
        }

        [Test]
        public void UndosOfDifferentTypesShouldNotBeMerged()
        {
            UndoProperty undoPropertyOne = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);
            IUndoUnit undoPropertyTwo = new UndoResize(RectangleTwo, Corner.TopLeft, DesignerHost);

            bool isMerged = undoPropertyTwo.Merge(undoPropertyOne);

            Assert.IsFalse(isMerged);
            Assert.AreEqual(1, undoPropertyOne.ElementNames.Count);
            Assert.AreEqual(1, undoPropertyOne.UndoPropertyValues.Count);
        }

        [Test]
        public void UndosOfSameTypeForDifferentElementsShouldBeAdded()
        {
            IUndoUnit undoPropertyOne = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);
            IUndoUnit undoPropertyTwo = new UndoProperty(RectangleTwo, Rectangle.VisibilityProperty.Name, DesignerHost);

            bool isToBeAdded = undoPropertyTwo.ShouldBeAddedToUndoStack(undoPropertyOne);

            Assert.IsTrue(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsShouldNotBeAdded()
        {
            IUndoUnit undoPropertyOne = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);
            IUndoUnit undoPropertyTwo = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);

            bool isToBeAdded = undoPropertyTwo.ShouldBeAddedToUndoStack(undoPropertyOne);

            Assert.IsFalse(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsWithDifferentValuesShouldBeAdded()
        {
            IUndoUnit undoPropertyOne = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);
            RectangleOne.Visibility = Visibility.Hidden;
            IUndoUnit undoPropertyTwo = new UndoProperty(RectangleOne, Rectangle.VisibilityProperty.Name, DesignerHost);

            bool isToBeAdded = undoPropertyTwo.ShouldBeAddedToUndoStack(undoPropertyOne);

            Assert.IsTrue(isToBeAdded);
        }
    }
}
#endif
