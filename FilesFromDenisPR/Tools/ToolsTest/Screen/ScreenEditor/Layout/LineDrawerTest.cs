using System;
using System.Windows;
using System.Windows.Shapes;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class LineDrawerTest
    {
        private LineDrawer m_LineDrawer;
        private IModifierKeysInfo m_ModifierKeysInfoStub;

        [SetUp]
        public void Setup()
        {
            m_ModifierKeysInfoStub = Substitute.For<IModifierKeysInfo>();
            m_LineDrawer = new LineDrawer(m_ModifierKeysInfoStub);
        }

        [Test]
        public void CurrentDegreeLessThanHalfIntervalShouldSnapToLowerDegree()
        {
            float newDegree = m_LineDrawer.GetSnapAngleDegree(100);
            Assert.That(newDegree == -10);
        }

        [Test]
        public void CurrentDegreeLargerThanHalfIntervalShouldSnapToHigherDegree()
        {
            float newDegree = m_LineDrawer.GetSnapAngleDegree(125);
            Assert.That(newDegree == 10);
        }

        [Test]
        public void WhenShiftKeyIsNotPressedItDoesNotSnapTheLineToAFixedAngle()
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(false);
            Line lineToUpdate = new Line() { X1 = 0, X2 = 0, Y1 = 0, Y2 = 0 };
            Point newStartPoint = new Point(1, 10);
            Point newEndPoint = new Point(100, 1000);


            m_LineDrawer.DrawLine(lineToUpdate, newStartPoint, newEndPoint);

            Assert.That(lineToUpdate.X1, Is.EqualTo(newStartPoint.X));
            Assert.That(lineToUpdate.Y1, Is.EqualTo(newStartPoint.Y));
            Assert.That(lineToUpdate.X2, Is.EqualTo(newEndPoint.X));
            Assert.That(lineToUpdate.Y2, Is.EqualTo(newEndPoint.Y));
        }

        [Test]
        public void ToggleStraightLinesOffChangesTheEndpointPositioningOfTheLine()
        {
            double currentStartPointY = 10;
            double currentStartPointX = 100;
            double resetToEndPointY = 555;
            double resetToEndPointX = 666;
            Line lineToUpdate = new Line() { X1 = currentStartPointX, X2 = 0, Y1 = currentStartPointY, Y2 = 0 };

            m_LineDrawer.ToggleStraightLinesOff(lineToUpdate, new Point(resetToEndPointX, resetToEndPointY));

            Assert.That(lineToUpdate.X1, Is.EqualTo(currentStartPointX));
            Assert.That(lineToUpdate.Y1, Is.EqualTo(currentStartPointY));
            Assert.That(lineToUpdate.Y2, Is.EqualTo(resetToEndPointY));
            Assert.That(lineToUpdate.X2, Is.EqualTo(resetToEndPointX));
        }

        [Test]
        [TestCase(100, 100,0)]
        [TestCase(120, 200, 45)]
        [TestCase(80, 200, 90)]
        [TestCase(100, 60, -45)]
        [TestCase(50, 80, -90)]
        [TestCase(0, 50, -135)]
        public void DrawingTheLineWithShiftPressedSnapsTheEndPointBasedOn45DegreeAngleSteps(double newEndPointX, double newEndPointY, double expectedAngle)
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(true);
            double currentStartPointX = 50;
            double currentStartPointY = 100;
            double currentEndPointX = 50;
            double currentEndPointY = 50;
            
            Line lineToUpdate = new Line() { X1 = currentStartPointX, X2 = currentEndPointX, Y1 = currentStartPointY, Y2 = currentEndPointY };

            m_LineDrawer.DrawLine(lineToUpdate, new Point(currentStartPointX, currentStartPointY), new Point(newEndPointX, newEndPointY));

            double actualAngle = (Math.Atan2(lineToUpdate.Y2 - lineToUpdate.Y1, lineToUpdate.X2 - lineToUpdate.X1) * 180) / Math.PI;
            Assert.That((int)actualAngle, Is.EqualTo(expectedAngle));
        }


    }
}
