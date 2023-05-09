using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
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
            toolManager.Stub(x => x.Runtime).Return(true);

            m_Counter = 0;
            m_EventWasFired = false;
            m_Canvas = MockRepository.GenerateStub<Canvas>();
            m_ScreenWindow = MockRepository.GenerateStub<IScreenWindow>();
            m_ScreenWindow.Stub(x => x.Canvas).Return(m_Canvas);
            m_ScreenWindow.Stub(x => x.Name).Return(STR_TestScreen);
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
            IEventRaiser eventRaiser = m_ScreenWindow.GetEventRaiser(x => x.Closed += null);
            eventRaiser.Raise(m_ScreenWindow, new EventArgs());

            Assert.IsTrue(m_EventWasFired);
        }

        [Test]
        public void ClosedEventFiresOnlyOnce()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;

            screenWindowAdapter.Closed += OnEventFired;
            IEventRaiser eventRaiser = m_ScreenWindow.GetEventRaiser(x => x.Closed += null);
            eventRaiser.Raise(m_ScreenWindow, new EventArgs());
            eventRaiser.Raise(m_ScreenWindow, new EventArgs());

            Assert.AreEqual(1, m_Counter);
        }

        [Test]
        public void ClosingEventFires()
        {
            ScreenWindowAdapter screenWindowAdapter = new ScreenWindowAdapter();
            screenWindowAdapter.AdaptedObject = m_ScreenWindow;
            
            screenWindowAdapter.Closing += (sender, e) => m_EventWasFired = true;
            IEventRaiser eventRaiser = m_ScreenWindow.GetEventRaiser(x => x.BeforeClosing += null);
            eventRaiser.Raise(m_ScreenWindow, new CancelEventArgs());

            Assert.IsTrue(m_EventWasFired);
        }

        private void OnEventFired(object sender, EventArgs e)
        {
            m_Counter += 1;
        }
    }
}
