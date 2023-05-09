using System.Windows.Shapes;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_NeoDesignerHost = MockRepository.GenerateStub<INeoDesignerHost>();
            m_ScreenRootDesigner = MockRepository.GenerateStub<IScreenRootDesigner>();
            m_NeoDesignerHost.Stub(x => x.RootDesigner).Return(m_ScreenRootDesigner);
        }

        private void StubPreviousAndCurrenUndoLineUnit(Line previous, Line current)
        {
            previous.Name = "previous";
            current.Name = "current";
            m_ScreenRootDesigner.Stub(x => x.FindElementByName(previous.Name)).Return(previous);
            m_ScreenRootDesigner.Stub(x => x.FindElementByName(current.Name)).Return(current);
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
