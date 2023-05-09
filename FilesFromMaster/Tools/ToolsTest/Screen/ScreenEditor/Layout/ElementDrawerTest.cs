using System.Linq;
using System.Windows;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ModifierKeysInfoStub = MockRepository.GenerateStub<IModifierKeysInfo>();
            m_LayoutObjectAdapterStub = MockRepository.GenerateStub<ILayoutObjectAdapter>();
            m_ElementDrawer = new ElementDrawer(m_ModifierKeysInfoStub);
        }

        private void StubShiftKeyDown()
        {
            m_ModifierKeysInfoStub.Stub(x => x.ShiftKeyDown).Return(true);
        }

        [Test]
        public void ShouldNotRecalculateSizeWhenShiftKeyIsNotPressed()
        {
            m_ModifierKeysInfoStub.Stub(x => x.ShiftKeyDown).Return(false);

            m_ElementDrawer.Resize(m_LayoutObjectAdapterStub, 10, 100, Corner.TopLeft);

            m_LayoutObjectAdapterStub.AssertWasCalled(x => x.Resize(10, 100, Corner.TopLeft));
        }

        [Test]
        public void WhenRecalculationOccursTheResizedWidthAndHeightIsDecidedByTheLargestOfTheTwo()
        {
            StubShiftKeyDown();

            double expectedWidthAndHeight = 20;
            double widthParameterToResizeMethod = 10;
            double heightParameterToResizeMethod = 20;
            
            m_ElementDrawer.Resize(m_LayoutObjectAdapterStub, widthParameterToResizeMethod, heightParameterToResizeMethod, Corner.TopLeft);

            Calls calls = m_LayoutObjectAdapterStub.GetCallsMadeOn(x => x.Resize(0, 0, Corner.TopLeft));
            Call firstCall = calls.First();
            
            Assert.That(firstCall.Arguments.First(), Is.EqualTo(expectedWidthAndHeight));
            Assert.That(firstCall.Arguments.ElementAt(1), Is.EqualTo(expectedWidthAndHeight));                              
        }

        [Test]
        public void ToggleEqualSidesResizesTheObjectBasedOnItsCurrentWidthAndHeight()
        {
            double expectedWidthAndHeight = 20;
            StubShiftKeyDown();
            m_LayoutObjectAdapterStub.Width = 20;
            m_LayoutObjectAdapterStub.Height = 10;

            m_ElementDrawer.ToggleEqualSidesOn(m_LayoutObjectAdapterStub, Corner.TopLeft);

            Calls calls = m_LayoutObjectAdapterStub.GetCallsMadeOn(x => x.Resize(0, 0, Corner.TopLeft));
            Call firstCall = calls.First();
            
            Assert.That(firstCall.Arguments.First(), Is.EqualTo(expectedWidthAndHeight));
            Assert.That(firstCall.Arguments.ElementAt(1), Is.EqualTo(expectedWidthAndHeight));                
        }

        [Test]
        public void ToggleEqualSidesOffResizesTheElementBasedOnTheDeltaOfTheStartPositionAndTheCurrentPosition()
        {
            double expectedWidthAndHeight = 22;
            Point startPosition = new Point(10,10);
            Point currentPosition = new Point(32, 32);

            m_ElementDrawer.ToggleEqualSidesOn(m_LayoutObjectAdapterStub, Corner.TopLeft);
            m_ElementDrawer.ToggleEqualSidesOff(startPosition,currentPosition);

            Calls calls = m_LayoutObjectAdapterStub.GetCallsMadeOn(x => x.Resize(0, 0, Corner.TopLeft));
            Call secondCall = calls.ElementAt(1);
            Assert.That(secondCall.Arguments.First(), Is.EqualTo(expectedWidthAndHeight));
            Assert.That(secondCall.Arguments.ElementAt(1), Is.EqualTo(expectedWidthAndHeight));
        }

    }
}