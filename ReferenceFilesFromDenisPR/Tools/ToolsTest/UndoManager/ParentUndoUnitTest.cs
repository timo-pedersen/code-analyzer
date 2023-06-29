using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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

            m_UndoUnitMock = Substitute.For<IUndoUnit>();
            m_UndoUnitMockTwo = Substitute.For<IUndoUnit>();
        }

        [Test]
        public void CreateParentUndoUnit()
        {
            Assert.IsNotNull(m_ParentUndoUnit);
        }

        [Test]
        public void UndoOnTwoUnits()
        {
            m_UndoUnitMock.ShouldBeAddedToUndoStack(null).Returns(true);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(m_UndoUnitMock).Returns(true);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            m_ParentUndoUnit.Undo();

            m_UndoUnitMock.Received().Undo();
            m_UndoUnitMockTwo.Received().Undo();
        }

        [Test]
        public void UndoIsNotRegisteredTwiceForSameUndoType()
        {
            m_UndoUnitMock.ShouldBeAddedToUndoStack(null).Returns(true);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(m_UndoUnitMock).Returns(false);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(1, m_ParentUndoUnit.UndoUnitCount);
        }

        [Test]
        public void UndoUnitsAreMergedForMergableUndoUnits()
        {
            m_UndoUnitMock.ShouldBeAddedToUndoStack(null).Returns(true);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(m_UndoUnitMock).Returns(true);

            m_UndoUnitMockTwo.Merge(Arg.Any<IUndoUnit>()).Returns(true);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(1, m_ParentUndoUnit.UndoUnitCount);
        }

        [Test]
        public void UndoUnitsAreNotMergedForUnmergableUndoUnits()
        {
            m_UndoUnitMock.ShouldBeAddedToUndoStack(null).Returns(true);
            m_UndoUnitMockTwo.ShouldBeAddedToUndoStack(m_UndoUnitMock).Returns(true);

            m_UndoUnitMockTwo.Merge(Arg.Any<IUndoUnit>()).Returns(false);

            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMock);
            m_ParentUndoUnit.RegisterUndoUnit(m_UndoUnitMockTwo);

            Assert.AreEqual(2, m_ParentUndoUnit.UndoUnitCount);
        }
    }
}
