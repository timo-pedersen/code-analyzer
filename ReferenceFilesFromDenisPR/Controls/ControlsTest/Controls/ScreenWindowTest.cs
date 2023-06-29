using System.Windows;
using Neo.ApplicationFramework.Common.Graphics.Controls;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class ScreenWindowTest
    {
        private ScreenWindow m_ScreenWindow;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            m_ScreenWindow = new ScreenWindow();
        }

        #region Style

        [Test]
        public void StyleNameReturnsDefaultNameWhenNoStyleIsSet()
        {
            m_ScreenWindow.Style = null;

            Assert.AreEqual(ScreenWindow.DefaultScreenWindowStyleName, ((IScreen)m_ScreenWindow).StyleName);
        }

        [Test]
        public void StyleIsnotSetWhenStyleNameIsSetAndScreenIsPopup()
        {
            StyleHelper styleHelperStub = Substitute.For<StyleHelper>();
            Style roundedStyle = new Style();
            styleHelperStub.LoadStyle(m_ScreenWindow, "Rounded").Returns(roundedStyle);
            m_ScreenWindow.StyleHelper = styleHelperStub;

            ((IScreen)m_ScreenWindow).PopupScreen = true;
            ((IScreen)m_ScreenWindow).StyleName = "Rounded";

            styleHelperStub.DidNotReceive().SetStyle(m_ScreenWindow, roundedStyle);
        }

        [Test]
        public void StyleIsNotSetWhenStyleNameIsSetAndScreenIsNotPopup()
        {
            StyleHelper styleHelperStub = Substitute.For<StyleHelper>();
            m_ScreenWindow.StyleHelper = styleHelperStub;

            ((IScreen)m_ScreenWindow).PopupScreen = false;
            ((IScreen)m_ScreenWindow).StyleName = "Rounded";

            styleHelperStub.DidNotReceive().SetStyle(Arg.Is(m_ScreenWindow), Arg.Any<Style>());
        }

        [TestCase(ScreenWindow.DefaultScreenWindowStyleName)]
        [TestCase("")]
        [TestCase(null)]
        public void StyleNameIsChangedBackToDefault(string newStyleName)
        {
            m_ScreenWindow.Style = new Style();
            ((IScreen)m_ScreenWindow).PopupScreen = true;

            ((IScreen)m_ScreenWindow).StyleName = "Rounded";
            Assert.That(((IScreen)m_ScreenWindow).StyleName, Is.EqualTo("Rounded"));

            ((IScreen)m_ScreenWindow).StyleName = newStyleName;
            Assert.That(((IScreen)m_ScreenWindow).StyleName, Is.EqualTo(ScreenWindow.DefaultScreenWindowStyleName));
        }

        #endregion

    }
}
