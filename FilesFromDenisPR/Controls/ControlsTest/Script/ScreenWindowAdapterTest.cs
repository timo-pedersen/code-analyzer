using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;

namespace Neo.ApplicationFramework.Controls.Script
{
    [TestFixture]
    public class ScreenWindowAdapterTest
    {
        private const string STR_TestScreen = "TestScreen";
        private bool m_EventWasFired;
        private IScreenWindow m_ScreenWindow;
        private Canvas m_Canvas;
        private int m_Counter;

        [SetUp]
        public void Setup()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            var toolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(true);

            m_Counter = 0;
            m_EventWasFired = false;
            m_Canvas = Substitute.For<Canvas>();
            m_ScreenWindow = Substitute.For<IScreenWindow>();
            m_ScreenWindow.Canvas.Returns(m_Canvas);
            m_ScreenWindow.Name.Returns(STR_TestScreen);
        }
        
        [Test]
        public void AdapterGetsScreenName()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            Assert.AreEqual(STR_TestScreen, screenWindowAdapter.Name);
        }

        [Test]
        public void OpenedEventFires()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            screenWindowAdapter.Opened += (sender, e) => m_EventWasFired = true;
            m_Canvas.RaiseEvent(new RoutedEventArgs(Canvas.LoadedEvent));
            Application.DoEvents();

            Assert.IsTrue(m_EventWasFired);
        }

        [Test]
        public void OpenedEventFiresOnlyOnce()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            screenWindowAdapter.Opened += OnEventFired;
            m_Canvas.RaiseEvent(new RoutedEventArgs(Canvas.LoadedEvent));
            m_Canvas.RaiseEvent(new RoutedEventArgs(Canvas.LoadedEvent));
            Application.DoEvents();

            Assert.AreEqual(1, m_Counter);
        }

        [Test]
        public void ClosedEventFires()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            screenWindowAdapter.Closed += (sender, e) => m_EventWasFired = true;
            Raise.Event();

            Assert.IsTrue(m_EventWasFired);
        }

        [Test]
        public void ClosedEventFiresOnlyOnce()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            screenWindowAdapter.Closed += OnEventFired;
            Raise.Event();
            Raise.Event();

            Assert.AreEqual(1, m_Counter);
        }

        [Test]
        public void ClosingEventFires()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;
            
            screenWindowAdapter.Closing += (sender, e) => m_EventWasFired = true;
            Raise.EventWith(m_ScreenWindow, new CancelEventArgs());

            Assert.IsTrue(m_EventWasFired);
        }

        private void OnEventFired(object sender, EventArgs e)
        {
            m_Counter += 1;
        }
    }
}
