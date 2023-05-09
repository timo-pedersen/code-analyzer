using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Undo
{
    [TestFixture]
    public class UndoResizeTest : UndoTestBase
    {
        [Test]
        public void UndosOfSameTypeShouldBeMerged()
        {
            UndoResize undoResizeOne = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);
            IUndoUnit undoResizeTwo = new UndoResize(RectangleTwo, Corner.TopLeft, DesignerHost);

            bool isMerged = undoResizeTwo.Merge(undoResizeOne);

            Assert.IsTrue(isMerged);
            Assert.AreEqual(2, undoResizeOne.ElementNames.Count);
            Assert.AreEqual(2, undoResizeOne.Sizes.Count);
        }

        [Test]
        public void UndosOfDifferentTypesShouldNotBeMerged()
        {
            UndoResize undoResizeOne = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);
            IUndoUnit undoResizeTwo = new UndoMove(RectangleTwo, DesignerHost);

            bool isMerged = undoResizeTwo.Merge(undoResizeOne);

            Assert.IsFalse(isMerged);
            Assert.AreEqual(1, undoResizeOne.ElementNames.Count);
            Assert.AreEqual(1, undoResizeOne.Sizes.Count);
        }

        [Test]
        public void UndosOfSameTypeForDifferentElementsShouldBeAdded()
        {
            IUndoUnit undoResizeOne = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);
            IUndoUnit undoResizeTwo = new UndoResize(RectangleTwo, Corner.TopLeft, DesignerHost);

            bool isToBeAdded = undoResizeTwo.ShouldBeAddedToUndoStack(undoResizeOne);

            Assert.IsTrue(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsShouldNotBeAdded()
        {
            IUndoUnit undoResizeOne = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);
            IUndoUnit undoResizeTwo = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);

            bool isToBeAdded = undoResizeTwo.ShouldBeAddedToUndoStack(undoResizeOne);

            Assert.IsFalse(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsWithDifferentValuesShouldBeAdded()
        {
            IUndoUnit undoResizeOne = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);
            RectangleOne.Width = 100;
            IUndoUnit undoResizeTwo = new UndoResize(RectangleOne, Corner.TopLeft, DesignerHost);

            bool isToBeAdded = undoResizeTwo.ShouldBeAddedToUndoStack(undoResizeOne);

            Assert.IsTrue(isToBeAdded);
        }
    }
}
