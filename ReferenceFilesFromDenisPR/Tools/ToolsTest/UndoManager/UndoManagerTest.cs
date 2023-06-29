using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.UndoManager
{
    [TestFixture]
    public class UndoManagerTest
    {
        private IUndoManager m_UndoManager;
        private IUndoService m_UndoServiceMock;

        [SetUp]
        public void SetupUndoManager()
        {
            m_UndoManager = new UndoManager();

            m_UndoServiceMock = Substitute.For<IUndoService>();
        }

        [Test]
        public void CreateUndoManager()
        {
            Assert.IsNotNull(m_UndoManager);
        }

        [Test]
        public void CreateUndoService()
        {
            IUndoService undoService = m_UndoManager.CreateUndoService(null);
            
            Assert.IsNotNull(undoService, "Failed to create UndoService");
        }

        [Test]
        public void ActiveUndoService()
        {
            Assert.IsNull(m_UndoManager.ActiveUndoService);

            m_UndoManager.ActiveUndoService = m_UndoServiceMock;
            Assert.AreSame(m_UndoServiceMock, m_UndoManager.ActiveUndoService);
        }

        [Test]
        public void NoActiveUndoService()
        {
            Assert.IsNull(m_UndoManager.ActiveUndoService);
            IUndoService undoService = (IUndoService)m_UndoManager;

            //These calls should not through an exception
            undoService.Redo();
            undoService.Undo();
            undoService.RegisterUndoUnit(null);

            Assert.IsTrue(string.IsNullOrEmpty(undoService.RedoDescription));
            Assert.IsTrue(string.IsNullOrEmpty(undoService.UndoDescription));
            Assert.IsFalse(undoService.IsUndoAvailable());
            Assert.IsFalse(undoService.IsRedoAvailable());
        }
    }
}
