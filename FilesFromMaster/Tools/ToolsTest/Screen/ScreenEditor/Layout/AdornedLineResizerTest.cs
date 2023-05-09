using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class AdornedLineResizerTest
    {
        private LayoutObjectAdapterLine m_LayoutObjectAdapterLine;
        private Line m_Line;
        private ISnapService m_SnapService;
        private IModifierKeysInfo m_ModifierKeysInfoStub;

        private void StubShiftKeyDown()
        {
            m_ModifierKeysInfoStub.Stub(x => x.ShiftKeyDown).Return(true);
        }

        [SetUp]
        public void Setup()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            m_Line = new Line();
            m_LayoutObjectAdapterLine = new LayoutObjectAdapterLine(m_Line);
            m_ModifierKeysInfoStub = MockRepository.GenerateStub<IModifierKeysInfo>();
            m_SnapService = MockRepository.GenerateStub<ISnapService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        private AdornedLineResizer AdornedLineResizer
        {
            get
            {
                return new AdornedLineResizer(m_ModifierKeysInfoStub, m_LayoutObjectAdapterLine, m_SnapService);
            }
        }

        private void StubStraightVerticalLine(Line originalLine)
        {
            originalLine.X1 = 10;
            originalLine.X2 = 10;
            m_LayoutObjectAdapterLine = new LayoutObjectAdapterLine(originalLine);
        }

        private void StubStraightHorizontalLine(Line originalLine)
        {
            originalLine.Y1 = 10;
            originalLine.Y2 = 10;
            m_LayoutObjectAdapterLine = new LayoutObjectAdapterLine(originalLine);
        }

        [Test]
        public void IfShiftNotIsPressedItShallUseTheSnapServiceToCalculateTheNewSize()
        {
            Point originalPosition = new Point(1, 1);
            Point updatedPosition = new Point(10, 100);
            m_ModifierKeysInfoStub.Stub(x => x.ShiftKeyDown).Return(false);

            AdornedLineResizer.Resize(originalPosition, updatedPosition, PointInLine.Start);

            Calls calls = m_SnapService.GetCallsMadeOn(x => x.GetSnapPosition(new Point()));
            Point pointSentToSnapService = (Point)calls.First().Arguments.First();
            Assert.That(pointSentToSnapService.X, Is.EqualTo(updatedPosition.X));
            Assert.That(pointSentToSnapService.Y, Is.EqualTo(updatedPosition.Y));
        }

        [Test]
        [TestCase(50, 100, 60, 100, PointInLine.Start)]
        [TestCase(50, 100, 50, 120, PointInLine.End)]
        public void WhenItsAResizeOfAHorizontalStraightLineItShouldExpandOnlyTheDraggedSide
            (double originalX1, int originalX2, double newX1position, double newX2position, PointInLine draggedSide)
        {
            Line originalLine = new Line() { X1 = originalX1, X2 = originalX2 };
            StubShiftKeyDown();
            StubStraightHorizontalLine(originalLine);

            double originalXValue = (draggedSide == PointInLine.Start) ? originalLine.X1 : originalLine.X2;
            double originalYValue = (draggedSide == PointInLine.Start) ? originalLine.Y1 : originalLine.Y2;
            double newXValue = (draggedSide == PointInLine.Start) ? newX1position : newX2position;
            Point originalPosition = new Point(originalXValue, originalYValue);
            Point updatedPosition = new Point(newXValue, originalPosition.Y);
            m_SnapService.Stub(x => x.GetSnapPosition(updatedPosition)).Return(updatedPosition);

            AdornedLineResizer resizer = AdornedLineResizer;
            resizer.SetSlopeCoefficient(0);
            resizer.Resize(originalPosition, updatedPosition, draggedSide);

            Assert.That(m_LayoutObjectAdapterLine.X1, Is.EqualTo(newX1position));
            Assert.That(m_LayoutObjectAdapterLine.X2, Is.EqualTo(newX2position));
        }

        [Test]
        [TestCase(50, 100, 60, 100, PointInLine.Start)]
        [TestCase(50, 100, 50, 120, PointInLine.End)]
        public void WhenItsAResizeOfAVerticalStraightLineItShouldExpandOnlyTheDraggedSide
            (double originalY1, int originalY2, double newY1position, double newY2position, PointInLine draggedSide)
        {
            Line originalLine = new Line() { Y1 = originalY1, Y2 = originalY2 };
            StubShiftKeyDown();
            StubStraightVerticalLine(originalLine);
            double originalXValue = (draggedSide == PointInLine.Start) ? originalLine.X1 : originalLine.X2;
            double originalYValue = (draggedSide == PointInLine.Start) ? originalLine.Y1 : originalLine.Y2;
            double newYValue = (draggedSide == PointInLine.Start) ? newY1position : newY2position;
            Point originalPosition = new Point(originalXValue, originalYValue);
            Point updatedPosition = new Point(originalPosition.X, newYValue);
            m_SnapService.Stub(x => x.GetSnapPosition(updatedPosition)).Return(updatedPosition);

            AdornedLineResizer resizer = AdornedLineResizer;
            resizer.SetSlopeCoefficient(double.NegativeInfinity);
            resizer.Resize(originalPosition, updatedPosition, draggedSide);

            Assert.That(m_LayoutObjectAdapterLine.Y1, Is.EqualTo(newY1position));
            Assert.That(m_LayoutObjectAdapterLine.Y2, Is.EqualTo(newY2position));
        }

        private double CalculateSlope(LayoutObjectAdapterLine layoutObjectAdapterLine)
        {
            return (layoutObjectAdapterLine.Y2 - layoutObjectAdapterLine.Y1) / (layoutObjectAdapterLine.X2 - layoutObjectAdapterLine.X1);
        }

        [Test]
        public void CalculatesPositionBasedOnUpdatedXWhenUpdatedXIsCloserToOriginalPositionThanUpdatedY()
        {
            StubShiftKeyDown();
            Point originalPosition = new Point(10, 20);
            m_LayoutObjectAdapterLine.X1 = originalPosition.X;
            m_LayoutObjectAdapterLine.Y1 = originalPosition.Y;
            m_LayoutObjectAdapterLine.X2 = originalPosition.X + 1;
            m_LayoutObjectAdapterLine.Y2 = originalPosition.Y + 1;
            double slopeCoefficient = CalculateSlope(m_LayoutObjectAdapterLine);
            Point updatedPosition = new Point(originalPosition.X + 1, originalPosition.Y + 10);
            double expectedUpdatedY = slopeCoefficient * (updatedPosition.X - originalPosition.X) + originalPosition.Y;

            AdornedLineResizer resizer = AdornedLineResizer;
            resizer.SetSlopeCoefficient(slopeCoefficient);
            resizer.Resize(originalPosition, updatedPosition, PointInLine.Start);

            Assert.That(resizer.ResizedPosition.X, Is.EqualTo(updatedPosition.X));
            Assert.That(resizer.ResizedPosition.Y, Is.EqualTo(expectedUpdatedY));
        }


        [Test]
        public void CalculatesPositionBasedOnUpdatedYWhenUpdatedYIsCloserToOriginalPositionThanUpdatedX()
        {
            StubShiftKeyDown();
            Point originalPosition = new Point(10, 20);
            m_LayoutObjectAdapterLine.X1 = originalPosition.X;
            m_LayoutObjectAdapterLine.Y1 = originalPosition.Y;
            m_LayoutObjectAdapterLine.X2 = originalPosition.X + 1;
            m_LayoutObjectAdapterLine.Y2 = originalPosition.Y + 1;
            double slopeCoefficient = CalculateSlope(m_LayoutObjectAdapterLine);
            Point updatedPosition = new Point(originalPosition.X + 10, originalPosition.Y + 1);
            double expectedUpdatedX = (updatedPosition.Y - originalPosition.Y + slopeCoefficient * originalPosition.X) / slopeCoefficient;

            AdornedLineResizer resizer = AdornedLineResizer;
            resizer.SetSlopeCoefficient(slopeCoefficient);
            resizer.Resize(originalPosition, updatedPosition, PointInLine.Start);

            Assert.That(resizer.ResizedPosition.X, Is.EqualTo(expectedUpdatedX));
            Assert.That(resizer.ResizedPosition.Y, Is.EqualTo(updatedPosition.Y));
        }

    }
}
