using System.Windows.Input;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Input
{
    [TestFixture]
    public class KeyCommandInvokerTest
    {
        private IScreenEditorCommands m_ScreenEditorCommandsMock;
        private IKeyCommandInvoker m_KeyCommandInvoker;
        private Key m_Key;
        private ModifierKeys m_ModifierKey;

        [SetUp]
        public void CreateKeyCommandInvoker()
        {
            m_ScreenEditorCommandsMock = Substitute.For<IScreenEditorCommands>();
            m_KeyCommandInvoker = new KeyCommandInvoker(m_ScreenEditorCommandsMock);
            Assert.IsNotNull(m_KeyCommandInvoker);            
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void MoveObjectRight()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.MoveRight().Returns(true);

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsTrue(result, "Result should be true");
            m_ScreenEditorCommandsMock.Received().MoveRight();
            m_ScreenEditorCommandsMock.ClearReceivedCalls();

            result = m_KeyCommandInvoker.KeyDown(m_Key, ModifierKeys.Alt);

            Assert.IsFalse(result, "Result should be false");
            m_ScreenEditorCommandsMock.DidNotReceive().MoveRight();
        }

        [Test]
        public void MoveObjectRightFailed()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.Control;

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsFalse(result, "Result should be false");
            m_ScreenEditorCommandsMock.DidNotReceive().MoveRight();
        }

        [Test]
        public void MoveObjectLeft()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.MoveLeft().Returns(true);
            
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().MoveLeft();
        }

        [Test]
        public void MoveObjectLeftFailed()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.Control;

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsFalse(result, "Result should be false");
            m_ScreenEditorCommandsMock.DidNotReceive().MoveLeft();
        }

        [Test]
        public void MoveObjectUp()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.MoveUp().Returns(true);
            
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().MoveUp();
        }

        [Test]
        public void MoveObjectUpFailed()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.Control;

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsFalse(result, "Result should be false");
            m_ScreenEditorCommandsMock.DidNotReceive().MoveUp();
        }

        [Test]
        public void MoveObjectDown()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.MoveDown().Returns(true);

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().MoveDown();
        }

        [Test]
        public void MoveObjectDownFailed()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.Control;

            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);

            Assert.IsFalse(result, "Result should be false");
            m_ScreenEditorCommandsMock.DidNotReceive().MoveDown();
        }

        [Test]
        public void IncreaseObjectWidth()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.IncreaseWidth().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().IncreaseWidth();
        }

        [Test]
        public void DecreaseObjectWidth()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.DecreaseWidth().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().DecreaseWidth();
        }

        [Test]
        public void IncreaseObjectHeight()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.IncreaseHeight().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().IncreaseHeight();
        }

        [Test]
        public void DecreaseObjectHeight()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.DecreaseHeight().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().DecreaseHeight();
        }

        [Test]
        public void UndoCommand()
        {
            m_Key = Key.Z;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.CanExecuteUndo().Returns(true);
            m_ScreenEditorCommandsMock.Undo().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().CanExecuteUndo();
            m_ScreenEditorCommandsMock.Received().Undo();
        }

        [Test]
        public void RedoCommand()
        {
            m_Key = Key.Y;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Redo().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().Redo();
        }

        [Test]
        public void DeleteCommand()
        {
            m_Key = Key.Delete;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.CanExecuteDelete().Returns(true);
            m_ScreenEditorCommandsMock.Delete().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
            m_ScreenEditorCommandsMock.Received().CanExecuteDelete();
            m_ScreenEditorCommandsMock.Received().Delete();
        }

        [Test]
        public void Copy()
        {
            m_Key = Key.C;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Copy().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Copy should return true");
            m_ScreenEditorCommandsMock.Received().Copy();
        }

        [Test]
        public void Cut()
        {
            m_Key = Key.X;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.CanExecuteCut().Returns(true);
            m_ScreenEditorCommandsMock.Cut().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Cut should return true");
            m_ScreenEditorCommandsMock.Received().CanExecuteCut();
            m_ScreenEditorCommandsMock.Received().Cut();
        }

        [Test]
        public void Paste()
        {
            m_Key = Key.V;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.CanExecutePaste().Returns(true);
            m_ScreenEditorCommandsMock.Paste().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Paste should return true");
            m_ScreenEditorCommandsMock.Received().CanExecutePaste();
            m_ScreenEditorCommandsMock.Received().Paste();
        }

        [Test]
        public void LockObjects()
        {
            m_Key = Key.E;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.LockSelection().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Editor should return true");
            m_Key = Key.L;
            result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Lock should return true");
            m_ScreenEditorCommandsMock.Received().LockSelection();
        }

        [Test]
        public void HideObjects()
        {
            m_Key = Key.E;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.HideSelection().Returns(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Editor should return true");
            m_Key = Key.H;
            result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Hide should return true");
            m_ScreenEditorCommandsMock.Received().HideSelection();
        }

    }
}
