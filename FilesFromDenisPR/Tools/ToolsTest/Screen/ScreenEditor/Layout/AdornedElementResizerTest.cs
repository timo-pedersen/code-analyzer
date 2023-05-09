using System;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class AdornedElementResizerTest
    {
        private double firstArgument = -1.0;
        private double secondArgument = -1.0;
        private AdornedElementResizer m_AdornedElementResizer;
        private IModifierKeysInfo m_ModifierKeysInfoStub;
        private ILayoutObjectAdapter m_LayoutObjectAdapterStub;

        private void StubShiftKeyDown()
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(true);
        }

        [SetUp]
        public void Setup()
        {
            m_ModifierKeysInfoStub = Substitute.For<IModifierKeysInfo>();
            m_LayoutObjectAdapterStub = Substitute.For<ILayoutObjectAdapter>();
            m_AdornedElementResizer = new AdornedElementResizer(m_ModifierKeysInfoStub);

            m_LayoutObjectAdapterStub.WhenForAnyArgs(x => x.Resize(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<Corner>()))
                .Do(y =>
                {
                    firstArgument = (double)y[0];
                    secondArgument = (double)y[1];
                });
        }

        [Test]
        public void DoesNotCalculateAspectRatioWhenShiftKeyIsntDown()
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(false);

            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 10, 100, Corner.TopLeft);
            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 10, 200, Corner.TopLeft);

            m_LayoutObjectAdapterStub.Received().Resize(10, 200, Corner.TopLeft);
        }

        [Test]
        [TestCase(Corner.Left)]
        [TestCase(Corner.Right)]
        [TestCase(Corner.Bottom)]
        [TestCase(Corner.Top)]
        public void DoesNotCalculateAspectRatioWhenNoOuterCornerIsDraggedAndShiftKeyIsDown(Corner corner)
        {
            StubShiftKeyDown();

            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 10, 100, corner);
            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 10, 200, corner);

            m_LayoutObjectAdapterStub.Received().Resize(10, 200, corner);
        }

        [Test]
        public void WidthSetsTheAspectRatioWhenItsLargerThenHeight()
        {
            StubShiftKeyDown();

            double actualHeight = 20;
            double actualWidth = 30;
            double factor = actualHeight / actualWidth;
            double expectedWidth = 60;
            double expectedHeight = expectedWidth * factor;

            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, actualWidth, actualHeight, Corner.BottomLeft);
            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 60, 30, Corner.BottomLeft);

            AssertSizeOfElement(expectedWidth, expectedHeight, 2);
        }


        [Test]
        public void ResetsFactorWhenTogglingOfTheResize()
        {
            StubShiftKeyDown();

            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 60, 30, Corner.BottomLeft);
            m_AdornedElementResizer.ToggleOffActiveResize();
            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 110, 240, Corner.BottomLeft);

            AssertSizeOfElement(110, 240, 2);
        }

        [Test]
        public void HeightSetsTheAspectRatioWhenItsLargerThenWidth()
        {
            StubShiftKeyDown();

            double actualHeight = 30;
            double actualWidth = 20;
            double factor = actualWidth / actualHeight;
            double expectedHeight = 60;
            double expectedWidth = expectedHeight * factor;

            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, actualWidth, actualHeight, Corner.BottomLeft);
            m_AdornedElementResizer.Resize(m_LayoutObjectAdapterStub, 30, 60, Corner.BottomLeft);

            AssertSizeOfElement(expectedWidth, expectedHeight, 2);
        }

        private void AssertSizeOfElement(double expectedWidth, double expectedHeight, int numberOfTimesResizeWasCalled)
        {
            Assert.That(firstArgument, Is.EqualTo(expectedWidth), string.Format("The expected width after aspect ratio resize was not {0} as expected, but {1}.", expectedWidth, firstArgument));
            Assert.That(secondArgument, Is.EqualTo(expectedHeight), string.Format("The expected height after aspect ratio resize was not {0} as expected, but {1}.", expectedHeight, secondArgument));
        }
    }
}