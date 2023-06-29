#if!VNEXT_TARGET
using System.Windows.Shapes;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Undo
{
    [TestFixture]
    public class UndoMoveLinePointsTest
    {
        private INeoDesignerHost m_NeoDesignerHost;
        private IScreenRootDesigner m_ScreenRootDesigner;

        [SetUp]
        public void Setup()
        {
            m_NeoDesignerHost = Substitute.For<INeoDesignerHost>();
            m_ScreenRootDesigner = Substitute.For<IScreenRootDesigner>();
            m_NeoDesignerHost.RootDesigner.Returns(m_ScreenRootDesigner);
        }

        private void StubPreviousAndCurrenUndoLineUnit(Line previous, Line current)
        {
            previous.Name = "previous";
            current.Name = "current";
            m_ScreenRootDesigner.FindElementByName(previous.Name).Returns(previous);
            m_ScreenRootDesigner.FindElementByName(current.Name).Returns(current);
        }

        [Test]
        public void ShouldRegisterUndoIfStartPointDiffersFromPrevious()
        {
            Line previousLine = new Line() { X1 = 0, X2 = 0, Y1 = 0, Y2 = 0 };
            Line currentLine = new Line() { X1 = 10, X2 = 0, Y1 = 0, Y2 = 0 };
            StubPreviousAndCurrenUndoLineUnit(previousLine, currentLine);
            UndoMoveLinePoints previousUndoPoint = new UndoMoveLinePoints(previousLine, m_NeoDesignerHost);

            IUndoUnit newUndoPoint = new UndoMoveLinePoints(currentLine, m_NeoDesignerHost);

            Assert.That(newUndoPoint.ShouldBeAddedToUndoStack(previousUndoPoint), Is.True);
        }

        [Test]
        public void ShouldRegisterUndoIfEndPointDiffersFromPrevious()
        {
            Line previousLine = new Line() { X1 = 0, X2 = 0, Y1 = 0, Y2 = 0 };
            Line currentLine = new Line() { X1 = 0, X2 = 0, Y1 = 0, Y2 = 10 };
            StubPreviousAndCurrenUndoLineUnit(previousLine, currentLine);
            UndoMoveLinePoints previousUndoPoint = new UndoMoveLinePoints(previousLine, m_NeoDesignerHost);

            IUndoUnit newUndoPoint = new UndoMoveLinePoints(currentLine, m_NeoDesignerHost);

            Assert.That(newUndoPoint.ShouldBeAddedToUndoStack(previousUndoPoint), Is.True);
        }

        [Test]
        public void ShouldNotRegisterUndoIfEndAndStartPointIsTheSame()
        {
            Line previousLine = new Line() { X1 = 0, X2 = 0, Y1 = 0, Y2 = 10 };
            Line currentLine = previousLine;
            StubPreviousAndCurrenUndoLineUnit(previousLine, currentLine);
            UndoMoveLinePoints previousUndoPoint = new UndoMoveLinePoints(previousLine, m_NeoDesignerHost);

            IUndoUnit newUndoPoint = new UndoMoveLinePoints(currentLine, m_NeoDesignerHost);

            Assert.That(newUndoPoint.ShouldBeAddedToUndoStack(previousUndoPoint), Is.False);
        }
    }
}
#endif
