using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.UndoManager
{
    [TestFixture]
    public class UndoServiceTest
    {
        private UndoService m_UndoServiceInstance;
        private IUndoService m_UndoService;
        
        private IUndoUnit m_UndoUnitMock;
        private IUndoUnit m_UndoUnitMockTwo;
        private IUndoUnit m_RedoUnitMock;

        [SetUp]
        public void SetupUndoService()
        {
            m_UndoServiceInstance = new UndoService();
            m_UndoService = (IUndoService)m_UndoServiceInstance;

            m_UndoUnitMock = Substitute.For<IUndoUnit>();
            m_UndoUnitMockTwo = Substitute.For<IUndoUnit>();
            m_RedoUnitMock = Substitute.For<IUndoUnit>();

            Assert.IsNotNull(m_UndoUnitMock, "The creation of the Mock failed somehow");
            Assert.IsNotNull(m_UndoUnitMockTwo, "The creation of the Mock failed somehow");
        }

        [Test]
        public void Create()
        {
            Assert.IsNotNull(m_UndoService, "Failed to create UndoService");
        }

        [Test]
        public void IsUndoDisabledAtCreation()
        {
            Assert.IsFalse(m_UndoService.IsUndoAvailable());
        }

        [Test]
        public void IsUndoEnabledWithOneUndoUnit()
        {
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);

            Assert.IsTrue(m_UndoService.IsUndoAvailable(), "Undo should be enabled with one undo unit");
        }

        [Test]
        public void IsUndoCalledOnUndoUnit()
        {
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            m_UndoService.Undo();

            Assert.IsFalse(m_UndoService.IsUndoAvailable(), "Undo should not be enabled");
            m_UndoUnitMock.Received().Undo();
        }

        [Test]
        public void UndoCalledOnEmptyUndoStack()
        {
            //No exceptions should be thrown...
            m_UndoService.Undo();        
        }

        [Test]
        public void UndoDescriptionAtCreation()
        {
            Assert.IsTrue(string.IsNullOrEmpty(m_UndoService.UndoDescription), "Description should be empty at startup");
        }

        [Test]
        public void UndoDescriptionFromUndoUnit()
        {
            string undoDescription = "UndoDescription";
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            m_UndoUnitMock.UndoDescription.Returns(undoDescription);

            Assert.AreEqual(undoDescription, m_UndoService.UndoDescription);
            Assert.IsTrue(m_UndoService.IsUndoAvailable(), "Undo should still enabled");
        }

        [Test]
        public void AddUndoUnitQueryReturnsTrue()
        {
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            m_UndoUnitMock.UndoDescription.Returns("Dummy");

            m_UndoUnitMock.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(true);
            m_UndoUnitMock.Merge(Arg.Any<IUndoUnit>()).Returns(false);
            //m_UndoUnitMock.SetupResult("Merge", false, typeof(IUndoUnit));

            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            
            Assert.AreEqual(2, m_UndoServiceInstance.UndoUnitCount, "Undo should still enabled");
        }

        [Test]
        public void AddUndoUnitReturnsFalse()
        {
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);

            m_UndoUnitMock.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(false);
            m_UndoUnitMock.Merge(Arg.Any<IUndoUnit>()).Returns(false);

            //Register
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);

            Assert.AreEqual(1, m_UndoServiceInstance.UndoUnitCount, "Undo should still enabled");
        }

        [Test]
        public void IsRedoDisabledAtCreation()
        {
            Assert.IsFalse(m_UndoService.IsRedoAvailable());
        }

        [Test]
        public void IsRedoEnabledAfterUndo()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);
            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.Undo();

            Assert.IsFalse(m_UndoService.IsUndoAvailable(), "Undo should not be enabled anymore");
            Assert.IsTrue(m_UndoService.IsRedoAvailable(), "Redo should be enabled");
        }

        /// <summary>
        /// When a UndoUnit is undone the Redo description should be set from that undo unit.
        /// The Redo description should NOT come from the redo unit itself.
        /// E.g when an Add Object is undone it should say Redo Add Object but actually the redo unit will Add an object
        /// and therefore actually is an Delete Object undo unit. Easy eh?
        /// </summary>
        [Test]
        public void UndoSetsRedoDescription()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);
            string undoDescription = "UndoDescription";
            undoUnit.UndoDescription = undoDescription;

            m_UndoService.RegisterUndoUnit(undoUnit);
            m_RedoUnitMock.UndoDescription.Returns(undoDescription);

            m_UndoService.Undo();

            Assert.AreEqual(undoDescription, m_UndoService.RedoDescription);
        }

        [Test]
        public void UndoIsCalledOnRedoUnit()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);
            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.IsFalse(m_UndoService.IsUndoAvailable(), "Undo should not be enabled anymore");
            m_RedoUnitMock.Received().Undo();
        }

        [Test]
        public void RedoIsDisabledAfterRegisterNewUndoUnit()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);
            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.Undo();

            Assert.IsTrue(m_UndoService.IsRedoAvailable(), "Redo should be enabled");

            //A new undo operation is registered so Redo should no longer be possible
            m_UndoService.RegisterUndoUnit(undoUnit);

            Assert.IsFalse(m_UndoService.IsRedoAvailable(), "Redo should not be enabled");
        }

        [Test]
        public void RedoTwiceWhenRedoRegistersUndoUnit()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);
            IUndoUnit undoUnitThatRegistersRedoOnUndo = new UndoUnitWithRedo(undoUnit, m_UndoService);
            
            //Register "two" undo units
            m_UndoService.RegisterUndoUnit(undoUnitThatRegistersRedoOnUndo);
            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.Undo();
            m_UndoService.Undo();

            //It should be possible do Redo twice now...
            Assert.IsTrue(m_UndoService.IsRedoAvailable(), "Redo should be enabled");
            m_UndoService.Redo();
            Assert.IsTrue(m_UndoService.IsRedoAvailable(), "Redo should be enabled");
            m_UndoService.Redo();
            Assert.IsFalse(m_UndoService.IsRedoAvailable(), "Redo should not be enabled");           
        }

        [Test]
        public void UndoSeveralTimesWithUndoParameter()
        {
            IUndoUnit undoUnit = new UndoUnitWithRedo(m_RedoUnitMock, m_UndoService);

            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.RegisterUndoUnit(undoUnit);
            m_UndoService.RegisterUndoUnit(undoUnit);

            //Check that three undo units are registered
            Assert.AreEqual(3, m_UndoServiceInstance.UndoUnitCount, "There should be three undo units");

            m_UndoService.Undo(2);

            //Check that two undo units are unregistered
            Assert.AreEqual(1, m_UndoServiceInstance.UndoUnitCount, "There should be one undo unit");
        }
        
        //If we are in Redo we should always add Undo units to the Undo queue
        //That means we should add undo units during redo even if AddToUndoStack returns false
        [Test]
        public void DontCheckAddUndoUnitWhenInRedo()
        {
            //m_UndoUnitMock.ExpectNoCall("ShouldBeAddedToUndoStack", typeof(IUndoUnit));

            m_UndoUnitMock.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(false);
            m_UndoUnitMock.Merge(Arg.Any<IUndoUnit>()).Returns(false);
            //Complex setup of Mocks...
            IUndoUnit redoUnitThatRegistersUndo = new UndoUnitWithRedo(m_UndoUnitMock, m_UndoService);
            IUndoUnit undoUnitThatRegistersRedo = new UndoUnitWithRedo(redoUnitThatRegistersUndo, m_UndoService);

            m_UndoService.RegisterUndoUnit(undoUnitThatRegistersRedo);
            m_UndoService.RegisterUndoUnit(undoUnitThatRegistersRedo);
            
            //Now there are two undo operations on the undo stack
            m_UndoService.Undo();
            m_UndoService.Undo();
              
            //Now there are two redo operations on the redo stack
            m_UndoService.Redo();
            m_UndoService.Redo();

            //Check that both redo units registered their undo units
            Assert.AreEqual(2, m_UndoServiceInstance.UndoUnitCount, "There should be two undo units");
        }
        [Test]
        public void ParentUndoIsNotRegisteredWhenEmpty()
        {
            string parentUndodescription = "ParentUndo";
            IParentUndoUnit parentUndoUnit = m_UndoService.OpenParentUndo(parentUndodescription);
            parentUndoUnit.Close();

            Assert.IsFalse(m_UndoService.IsUndoAvailable());
        }

        [Test]
        public void ParentUndoSetsDescription()
        {
            string parentUndodescription = "ParentUndo";
            IParentUndoUnit parentUndoUnit = m_UndoService.OpenParentUndo(parentUndodescription);
            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            parentUndoUnit.Close();

            Assert.AreEqual(parentUndodescription, m_UndoService.UndoDescription, "Undodescription was not set from parentundo");
            Assert.IsTrue(m_UndoService.IsUndoAvailable(), "Undo should be enabled with one undo unit");
        }

        [Test]
        public void ParentUndoOnTwoUndoUnits()
        {
            IParentUndoUnit parentUndoUnit = m_UndoService.OpenParentUndo("");

            m_UndoUnitMock.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(true);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(true);
            m_UndoUnitMockTwo.Merge(Arg.Any<IUndoUnit>()).Returns(false);

            m_UndoService.RegisterUndoUnit(m_UndoUnitMock);
            m_UndoService.RegisterUndoUnit(m_UndoUnitMockTwo);

            parentUndoUnit.Close();

            m_UndoService.Undo();

            m_UndoUnitMock.Received().Undo();
            m_UndoUnitMockTwo.Received().Undo();
        }

        /// <summary>
        /// Check that it is the first parent undo uit that has to be closed for the active
        /// parent undo unit to be closed.
        /// </summary>
        [Test]
        public void FirstParentUndoMustBeClosed()
        {
            IParentUndoUnit parentUndoUnit = m_UndoService.OpenParentUndo("");
            IParentUndoUnit parentUndoUnitTwo = m_UndoService.OpenParentUndo("");

            parentUndoUnitTwo.Close();

            //A parentUnit should still be open
            Assert.IsTrue(m_UndoServiceInstance.IsParentUndoUnitOpen, "ParentUndoUnit should still be open");
        }

        /// <summary>
        /// If two undo units is grouped with a parent undo unit they will be undone together.
        /// Do a test to see if they are redone together also.
        /// </summary>
        [Test]
        public void RedoOnParentedUndoUnits()
        {
            IParentUndoUnit parentUndoUnit = m_UndoService.OpenParentUndo("");
            m_UndoUnitMock.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(true);
            m_UndoUnitMock.Merge(Arg.Any<IUndoUnit>()).Returns(false);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(Arg.Any<IUndoUnit>()).Returns(true);
            m_UndoUnitMockTwo.Merge(Arg.Any<IUndoUnit>()).Returns(false);

            UndoUnitWithRedo undoUnitWithRedo = new UndoUnitWithRedo(m_UndoUnitMock, m_UndoService);
            m_UndoService.RegisterUndoUnit(undoUnitWithRedo);

            UndoUnitWithRedo undoUnitWithRedoTwo = new UndoUnitWithRedo(m_UndoUnitMockTwo, m_UndoService);
            m_UndoService.RegisterUndoUnit(undoUnitWithRedoTwo);

            parentUndoUnit.Close();

            m_UndoService.Undo();
            Assert.IsTrue(undoUnitWithRedo.UndoCalled, "Undo was not called");
            Assert.IsTrue(undoUnitWithRedoTwo.UndoCalled, "Undo was not called");

            //Now check that both undo units are undone on one call to Redo
            Assert.IsTrue(m_UndoService.IsRedoAvailable());

            m_UndoService.Redo();

            m_UndoUnitMock.Received().Undo();
            m_UndoUnitMockTwo.Received().Undo();
        }
    }
}
