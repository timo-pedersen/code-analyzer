using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.UndoManager
{
    [TestFixture]
    public class ParentUndoUnitTest
    {
        private IParentUndoUnit m_ParentUndoUnit;

        private IUndoUnit m_UndoUnitMock;

        private IUndoUnit m_UndoUnitMockTwo;

        [SetUp]
        public void SetupParentUndoUnit()
        {
            m_ParentUndoUnit = new ParentUndoUnit("", null);

            m_UndoUnitMock = MockRepository.GenerateMock<IUndoUnit>();
            m_UndoUnitMockTwo = MockRepository.GenerateMock<IUndoUnit>();
        }

        [Test]
        public void CreateParentUndoUnit()
        {
            Assert.IsNotNull(m_ParentUndoUnit);
        }

        [Test]
        public void UndoOnTwoUnits()
        {
            m_UndoUnitMock.Stub(x => x.ShouldBeAddedToUndoStack(null)).Return(true);
            m_UndoUnitMockTwo.Stub(x => x.ShouldBeAddedToUndoStack(m_UndoUnitMock)).Return(true);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            //Now check that both undo units are undone on one call to Undo
            m_UndoUnitMock.Expect(x => x.Undo());
            m_UndoUnitMockTwo.Expect(x => x.Undo());

            m_ParentUndoUnit.Undo();

            m_UndoUnitMock.VerifyAllExpectations();
            m_UndoUnitMockTwo.VerifyAllExpectations();
        }

        [Test]
        public void UndoIsNotRegisteredTwiceForSameUndoType()
        {
            m_UndoUnitMock.Stub(x => x.ShouldBeAddedToUndoStack(null)).Return(true);
            m_UndoUnitMockTwo.Stub(x => x.ShouldBeAddedToUndoStack(m_UndoUnitMock)).Return(false);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(1, m_ParentUndoUnit.UndoUnitCount);
        }

        [Test]
        public void UndoUnitsAreMergedForMergableUndoUnits()
        {
            m_UndoUnitMock.Stub(x => x.ShouldBeAddedToUndoStack(null)).Return(true);
            m_UndoUnitMockTwo.Stub(x => x.ShouldBeAddedToUndoStack(m_UndoUnitMock)).Return(true);

            m_UndoUnitMockTwo.Stub(x => x.Merge(null)).IgnoreArguments().Return(true);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(1, m_ParentUndoUnit.UndoUnitCount);
        }

        [Test]
        public void UndoUnitsAreNotMergedForUnmergableUndoUnits()
        {
            m_UndoUnitMock.Stub(x => x.ShouldBeAddedToUndoStack(null)).Return(true);
            m_UndoUnitMockTwo.Stub(x => x.ShouldBeAddedToUndoStack(m_UndoUnitMock)).Return(true);

            m_UndoUnitMockTwo.Stub(x => x.Merge(null)).IgnoreArguments().Return(false);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(2, m_ParentUndoUnit.UndoUnitCount);
        }
    }
}
