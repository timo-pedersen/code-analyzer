using System;
using System.ComponentModel;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Controls.WebBrowser;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.EventMapper
{
    [TestFixture]
    public class EventMapperServiceTest
    {
        private IEventMapperService m_EventMapperService;
	    private ITargetService m_TargetServiceMock;

        [SetUp]
        public void SetUp()
        {
			m_TargetServiceMock = Substitute.For<ITargetService>();
            var targetService = m_TargetServiceMock.ToILazy();
			m_EventMapperService = new EventMapperService(targetService);	        
        }

        [Test]
        public void GetMappedClickEvent()
        {
            Type type = typeof(Button);
            EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(type)["Click"];
            string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);
            Assert.AreEqual("Click", mappedEvent);
        }

        [Test]
        public void GetMappedMouseDownEvent()
        {
            Type type = typeof(Button);
            EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(typeof(Button))["PreviewMouseDown"];
            string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);
            Assert.AreEqual("MouseDown", mappedEvent);
        }

        [Test]
        public void GetMappedMouseUpEvent()
        {
            Type type = typeof(Button);
            EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(typeof(Button))["PreviewMouseUp"];
            string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);
            Assert.AreEqual("MouseUp", mappedEvent);
        }

        [Test]
        public void GetMappedCustomEventWhenMappedThroughAttributeOnEvent()
        {
            Type type = typeof(WebBrowserControl);
            EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(type)["Navigating"];
            string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);
            Assert.AreEqual("Navigating", mappedEvent);
        }

        [Test]
        public void GetMappedCustomEventWhenMappedThroughAttributeOnClass()
        {
            Type type = typeof(ScreenWindow);
            EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(type)["Opened"];
            string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);
            Assert.AreEqual("Opened", mappedEvent);
        }

		[Test]
		public void GetMappedMouseEnterEventOnWindows()
		{
			// Arrange
            m_TargetServiceMock.CurrentTarget = new Target(TargetPlatform.Windows, "WindowsTarget", "ProjFileName");
			Type type = typeof(ApplicationFramework.Controls.WindowsControls.ComboBox);
			EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(type)["MouseEnter"];
			
			// Act
			string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);

			// Assert
			Assert.AreEqual("MouseEnter", mappedEvent);
		}

		[Test]
		public void GetNoMappedMouseEnterEventOnNonWindows()
		{
			// Arrange
            m_TargetServiceMock.CurrentTarget = new Target(TargetPlatform.WindowsCE, "WindowsTarget", "ProjFileName");
			Type type = typeof(ApplicationFramework.Controls.WindowsControls.ComboBox);
			EventDescriptor eventDescriptor = TypeDescriptor.GetEvents(type)["MouseEnter"];

			// Act
			string mappedEvent = m_EventMapperService.GetMappedEvent(type, eventDescriptor);

			// Assert
			Assert.AreEqual(null, mappedEvent);
		}
    }
}
