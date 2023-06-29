using System.Windows;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class ElementDrawerTest
    {
        private ElementDrawer m_ElementDrawer;
        private IModifierKeysInfo m_ModifierKeysInfoStub;
        private ILayoutObjectAdapter m_LayoutObjectAdapterStub;

        [SetUp]
        public void Setup()
        {
            m_ModifierKeysInfoStub = Substitute.For<IModifierKeysInfo>();
            m_LayoutObjectAdapterStub = Substitute.For<ILayoutObjectAdapter>();
            m_ElementDrawer = new ElementDrawer(m_ModifierKeysInfoStub);
        }

        private void StubShiftKeyDown()
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(true);
        }

        [Test]
        public void ShouldNotRecalculateSizeWhenShiftKeyIsNotPressed()
        {
            m_ModifierKeysInfoStub.ShiftKeyDown.Returns(false);

            m_ElementDrawer.Resize(m_LayoutObjectAdapterStub, 10, 100, Corner.TopLeft);

            m_LayoutObjectAdapterStub.Received().Resize(10, 100, Corner.TopLeft);
        }

        [Test]
        public void WhenRecalculationOccursTheResizedWidthAndHeightIsDecidedByTheLargestOfTheTwo()
        {
            StubShiftKeyDown();

            double expectedWidthAndHeight = 20;
            double widthParameterToResizeMethod = 10;
            double heightParameterToResizeMethod = 20;
            
            m_ElementDrawer.Resize(m_LayoutObjectAdapterStub, widthParameterToResizeMethod, heightParameterToResizeMethod, Corner.TopLeft);

            m_LayoutObjectAdapterStub.WhenForAnyArgs(x => x.Resize(0, 0, Corner.TopLeft))
                .Do(y =>
                {
                    Assert.That((double)y[0], Is.EqualTo(expectedWidthAndHeight));
                    Assert.That((double)y[1], Is.EqualTo(expectedWidthAndHeight));
                });
        }

        [Test]
        public void ToggleEqualSidesResizesTheObjectBasedOnItsCurrentWidthAndHeight()
        {
            double expectedWidthAndHeight = 20;
            StubShiftKeyDown();
            m_LayoutObjectAdapterStub.Width = 20;
            m_LayoutObjectAdapterStub.Height = 10;

            m_ElementDrawer.ToggleEqualSidesOn(m_LayoutObjectAdapterStub, Corner.TopLeft);

            m_LayoutObjectAdapterStub.WhenForAnyArgs(x => x.Resize(0, 0, Corner.TopLeft))
                .Do(y =>
                {
                    Assert.That((double)y[0], Is.EqualTo(expectedWidthAndHeight));
                    Assert.That((double)y[1], Is.EqualTo(expectedWidthAndHeight));
                });
        }

        [Test]
        public void ToggleEqualSidesOffResizesTheElementBasedOnTheDeltaOfTheStartPositionAndTheCurrentPosition()
        {
            double expectedWidthAndHeight = 22;
            Point startPosition = new Point(10,10);
            Point currentPosition = new Point(32, 32);

            m_ElementDrawer.ToggleEqualSidesOn(m_LayoutObjectAdapterStub, Corner.TopLeft);
            m_ElementDrawer.ToggleEqualSidesOff(startPosition,currentPosition);

            var callNum = 1;
            m_LayoutObjectAdapterStub.WhenForAnyArgs(x => x.Resize(0, 0, Corner.TopLeft))
                .Do(y =>
                {
                    if (callNum == 2)
                    { 
                        Assert.That((double)y[0], Is.EqualTo(expectedWidthAndHeight));
                        Assert.That((double)y[1], Is.EqualTo(expectedWidthAndHeight));
                    }
                    callNum++;
                });
        }
    }
}