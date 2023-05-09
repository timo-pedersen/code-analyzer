using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Core.Api.Tools;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Controls.Script;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.EventMapper;
using Neo.ApplicationFramework.Tools.Script.Editor;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ScriptEditor
{
    [TestFixture]
    public class ScriptEventServiceTest
    {
        private const string ClickEventName = "Click";
        private IScriptEventService m_ScriptEventService;

        [SetUp]
        public void TestFixtureSetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;
            TestHelper.AddServiceStub<INeoDesignerEventService>();

            Target target = new Target(TargetPlatform.WindowsCE, string.Empty, string.Empty);

            ITargetService targetService = MockRepository.GenerateMock<ITargetService>();
            targetService.Stub(x => x.CurrentTarget).Return(target);
            TestHelper.AddService(targetService);

            IToolManager toolManager = MockRepository.GenerateMock<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(false);
            TestHelper.AddService(toolManager);

            var namingConstraints = MockRepository.GenerateMock<INamingConstraints>();
            namingConstraints.Stub(inv => inv.IsNameLengthValid(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(true);
            namingConstraints.Stub(inv => inv.ReservedApplicationNames).Return(new HashSet<string>());
            namingConstraints.Stub(inv => inv.ReservedSystemNames).Return(new HashSet<string>());

            ServiceContainer serviceContainer = new ServiceContainer();

            var componentContainerService = MockRepository.GenerateStub<IComponentContainerService>();
            componentContainerService.Stub(inv => inv.AddComponent(Arg<IComponent>.Is.Anything)).WhenCalled(
                inv =>
                {
                    var component = (IComponent)inv.Arguments[0];
                    component.Site = new ScriptSite(component, new Container());
                }
            );
            TestHelper.AddService<IEventMapperService>(new EventMapperService(targetService.ToILazy()));
            serviceContainer.AddService(typeof(IComponentContainerService), componentContainerService);
            serviceContainer.AddService(typeof(IElementAdapterService), new ElementAdapterService(serviceContainer));
            var refService = MockRepository.GenerateStub<IReferenceService>();
            refService.Stub(inv => inv.GetComponent(Arg<object>.Is.Anything)).WhenCalled(inv => inv.ReturnValue = inv.Arguments[0]).Return(null);
            serviceContainer.AddService(typeof(IReferenceService), refService);
            serviceContainer.AddService(typeof(IEventBindingService), new SampleEventBindingService(serviceContainer));
            m_ScriptEventService = new ScriptEventService(serviceContainer);
            serviceContainer.AddService(typeof(IScriptEventService), m_ScriptEventService);
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
        }

        [Test]
        public void GetButtonAdapterForButton()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            object scriptObject = m_ScriptEventService.GetScriptObject(button);
            Assert.IsTrue(scriptObject is ButtonAdapter);
        }

        [Test]
        public void GetNoAdapterForDataItem()
        {
            Neo.ApplicationFramework.Tools.Recipe.Recipe recipe = new Neo.ApplicationFramework.Tools.Recipe.Recipe();
            object scriptObject = m_ScriptEventService.GetScriptObject(recipe);
            Assert.IsTrue(scriptObject is Neo.ApplicationFramework.Tools.Recipe.Recipe);
        }

        [Test]
        public void GetEventsForButton()
        {
            var button = new Neo.ApplicationFramework.Controls.Button();
            EventDescriptorCollection eventDescriptors = m_ScriptEventService.GetEvents(button);

            Assert.AreEqual(5, eventDescriptors.Count, "Wrong number of expected events for button.");
            Assert.AreEqual(ClickEventName, eventDescriptors[0].Name, "Missing expected click event.");
        }

        [Test]
        public void GetEventHandlerForButtonWithNoHookedUpEvent()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();

            string eventHandlerName = m_ScriptEventService.GetEventHandlerName(button, ClickEventName);
            Assert.IsNull(eventHandlerName);
        }

        [Test]
        public void GetEventHandlerForButtonWithHookedUpClickEvent()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            button.Name = "Button1";
            m_ScriptEventService.HookupEvent(button, ClickEventName);
            string generatedEventHandlerName = m_ScriptEventService.GetEventHandlerName(button, ClickEventName);

            string eventHandlerName = string.Format("{0}_{1}", button.Name, ClickEventName);
            Assert.AreEqual(eventHandlerName, generatedEventHandlerName, "No matching event handler found.");
        }

        [Test]
        public void GetHookedUpEventsWhenThereAreNone()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();

            EventDescriptorCollection eventDescriptors = m_ScriptEventService.GetHookedUpEvents(button);
            Assert.AreEqual(0, eventDescriptors.Count);
        }

        [Test]
        public void GetHookedUpEvents()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            button.Name = "Button1";

            m_ScriptEventService.HookupEvent(button, ClickEventName);

            EventDescriptorCollection eventDescriptors = m_ScriptEventService.GetHookedUpEvents(button);
            Assert.AreEqual(1, eventDescriptors.Count);
            Assert.AreEqual(ClickEventName, eventDescriptors[0].Name);
        }

        [Test]
        public void IsClickEventHookedUpWhenThereIsNone()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();

            bool isEventHookedUp = m_ScriptEventService.IsEventHookedUp(button, ClickEventName);
            Assert.IsFalse(isEventHookedUp);
        }

        [Test]
        public void IsClickEventHookedUp()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            button.Name = "Button1";

            m_ScriptEventService.HookupEvent(button, ClickEventName);

            bool isEventHookedUp = m_ScriptEventService.IsEventHookedUp(button, ClickEventName);
            Assert.IsTrue(isEventHookedUp);
        }
    }
}
