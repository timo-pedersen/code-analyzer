using System.Windows.Input;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ScreenEditorCommandsMock = MockRepository.GenerateStrictMock<IScreenEditorCommands>();
            m_KeyCommandInvoker = new KeyCommandInvoker(m_ScreenEditorCommandsMock);
            Assert.IsNotNull(m_KeyCommandInvoker);            
        }

        [TearDown]
        public void VerifyMocks()
        {
            m_ScreenEditorCommandsMock.VerifyAllExpectations(); 
        }

        [Test]
        public void MoveObjectRight()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveRight()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Result should be true");

            m_ScreenEditorCommandsMock.Expect(x => x.MoveRight()).Repeat.Never();
            result = m_KeyCommandInvoker.KeyDown(m_Key, ModifierKeys.Alt);
            Assert.IsFalse(result, "Result should be false");
        }

        [Test]
        public void MoveObjectRightFailed()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveRight()).Repeat.Never();
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsFalse(result, "Result should be false");
        }

        [Test]
        public void MoveObjectLeft()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveLeft()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveObjectLeftFailed()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveLeft()).Repeat.Never();
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsFalse(result, "Result should be false");
        }

        [Test]
        public void MoveObjectUp()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveUp()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveObjectUpFailed()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveUp()).Repeat.Never();
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsFalse(result, "Result should be false");
        }

        [Test]
        public void MoveObjectDown()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveDown()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveObjectDownFailed()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.MoveDown()).Repeat.Never();
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsFalse(result, "Result should be false");
        }

        [Test]
        public void IncreaseObjectWidth()
        {
            m_Key = Key.Right;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.Expect(x => x.IncreaseWidth()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseObjectWidth()
        {
            m_Key = Key.Left;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.Expect(x => x.DecreaseWidth()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseObjectHeight()
        {
            m_Key = Key.Down;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.Expect(x => x.IncreaseHeight()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseObjectHeight()
        {
            m_Key = Key.Up;
            m_ModifierKey = ModifierKeys.Shift;
            m_ScreenEditorCommandsMock.Expect(x => x.DecreaseHeight()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);    
        }

        [Test]
        public void UndoCommand()
        {
            m_Key = Key.Z;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.CanExecuteUndo()).Return(true);
            m_ScreenEditorCommandsMock.Expect(x => x.Undo()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void RedoCommand()
        {
            m_Key = Key.Y;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.Redo()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteCommand()
        {
            m_Key = Key.Delete;
            m_ModifierKey = ModifierKeys.None;
            m_ScreenEditorCommandsMock.Expect(x => x.CanExecuteDelete()).Return(true);
            m_ScreenEditorCommandsMock.Expect(x => x.Delete()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void Copy()
        {
            m_Key = Key.C;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.Copy()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Copy should return true");
        }

        [Test]
        public void Cut()
        {
            m_Key = Key.X;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.CanExecuteCut()).Return(true);
            m_ScreenEditorCommandsMock.Expect(x => x.Cut()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Cut should return true");
        }

        [Test]
        public void Paste()
        {
            m_Key = Key.V;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.CanExecutePaste()).Return(true);
            m_ScreenEditorCommandsMock.Expect(x => x.Paste()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Paste should return true");
        }

        [Test]
        public void LockObjects()
        {
            m_Key = Key.E;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.LockSelection()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Editor should return true");
            m_Key = Key.L;
            result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Lock should return true");
        }

        [Test]
        public void HideObjects()
        {
            m_Key = Key.E;
            m_ModifierKey = ModifierKeys.Control;
            m_ScreenEditorCommandsMock.Expect(x => x.HideSelection()).Return(true);
            bool result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Editor should return true");
            m_Key = Key.H;
            result = m_KeyCommandInvoker.KeyDown(m_Key, m_ModifierKey);
            Assert.IsTrue(result, "Hide should return true");
        }

    }
}
