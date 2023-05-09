using System.Windows.Controls;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Undo
{
    [TestFixture]
    public class UndoMoveTest : UndoTestBase
    {
        [Test]
        public void UndosOfSameTypeShouldBeMerged()
        {
            UndoMove undoMoveOne = new UndoMove(RectangleOne, DesignerHost);
            IUndoUnit undoMoveTwo = new UndoMove(RectangleTwo, DesignerHost);

            bool isMerged = undoMoveTwo.Merge(undoMoveOne);

            Assert.IsTrue(isMerged);
            Assert.AreEqual(2, undoMoveOne.ElementNames.Count);
            Assert.AreEqual(2, undoMoveOne.Positions.Count);
        }

        [Test]
        public void UndosOfDifferentTypesShouldNotBeMerged()
        {
            UndoMove undoMoveOne = new UndoMove(RectangleOne, DesignerHost);
            IUndoUnit undoMoveTwo = new UndoResize(RectangleTwo, Corner.TopLeft, DesignerHost);

            bool isMerged = undoMoveTwo.Merge(undoMoveOne);

            Assert.IsFalse(isMerged);
            Assert.AreEqual(1, undoMoveOne.ElementNames.Count);
            Assert.AreEqual(1, undoMoveOne.Positions.Count);
        }

        [Test]
        public void UndosOfSameTypeForDifferentElementsShouldBeAdded()
        {
            IUndoUnit undoMoveOne = new UndoMove(RectangleOne, DesignerHost);
            IUndoUnit undoMoveTwo = new UndoMove(RectangleTwo, DesignerHost);

            bool isToBeAdded = undoMoveTwo.ShouldBeAddedToUndoStack(undoMoveOne);

            Assert.IsTrue(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsShouldNotBeAdded()
        {
            IUndoUnit undoMoveOne = new UndoMove(RectangleOne, DesignerHost);
            IUndoUnit undoMoveTwo = new UndoMove(RectangleOne, DesignerHost);

            bool isToBeAdded = undoMoveTwo.ShouldBeAddedToUndoStack(undoMoveOne);

            Assert.IsFalse(isToBeAdded);
        }

        [Test]
        public void UndosOfSameTypeForSameElementsWithDifferentValuesShouldBeAdded()
        {
            IUndoUnit undoMoveOne = new UndoMove(RectangleOne, DesignerHost);
            Canvas.SetLeft(RectangleOne, 10);
            IUndoUnit undoMoveTwo = new UndoMove(RectangleOne, DesignerHost);

            bool isToBeAdded = undoMoveTwo.ShouldBeAddedToUndoStack(undoMoveOne);

            Assert.IsTrue(isToBeAdded);
        }
    }
}
