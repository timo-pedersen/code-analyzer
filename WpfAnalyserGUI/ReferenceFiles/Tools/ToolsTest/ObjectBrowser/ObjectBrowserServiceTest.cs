using System;
using System.Collections.Generic;
using System.Windows;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.ObjectBrowser;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.WindowManagement;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ObjectBrowser
{
    [TestFixture]
    public class ObjectBrowserServiceTest
    {
        private IElementChangeService m_ElementChangeService;
        private IScreenDesignerView m_ScreenDesignerView;
        private ObjectBrowserControl m_ObjectBrowserControl;
        private IObjectBrowserService m_ObjectBrowserService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<IProjectManager>();
            TestHelper.AddServiceStub<INeoDesignerEventService>();

            m_ScreenDesignerView = CreateScreenDesignerView();

            m_ObjectBrowserControl = Substitute.For<ObjectBrowserControl>();
            m_ObjectBrowserService = new ObjectBrowserService(TextsIde.ObjectBrowser);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        private IScreenDesignerView CreateScreenDesignerView()
        {
            IScreenDesignerView screenDesignerView = Substitute.For<IScreenDesignerView>();
            screenDesignerView.ObjectContextMenu.Returns(x => null);

            m_ElementChangeService = Substitute.For<IElementChangeService>();
            screenDesignerView.WhenForAnyArgs(x => x.AddElement(Arg.Any<FrameworkElement>()))
                .Do(x => Raise.Event<ElementEventArgs>(x, new ElementEventArgs((UIElement)x[0], "Screen1")));
            screenDesignerView.WhenForAnyArgs(x => x.RemoveElement(Arg.Any<FrameworkElement>()))
                .Do(x => Raise.Event<ElementEventArgs>(x, new ElementEventArgs((UIElement)x[0], "Screen1")));

            INeoDesignerHost designerHost = Substitute.For<INeoDesignerHost>();
            designerHost.GetService<IElementChangeService>().Returns(m_ElementChangeService);

            IScreenRootDesigner screenRootDesigner = Substitute.For<IScreenRootDesigner>();
            screenRootDesigner.DesignerHost.Returns(designerHost);

            IScreenWindow screenWindow = Substitute.For<IScreenWindow>();
            screenWindow.Name.Returns("Screen1");
            screenRootDesigner.ScreenWindow.Returns(screenWindow);
            screenRootDesigner.RootElements.Returns(screenDesignerView.RootElements);
            screenDesignerView.Designer = screenRootDesigner;

            return screenDesignerView;
        }



        [Test]
        public void SettingActiveScreenDesignerUpdatesObjects()
        {
            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };
            Ellipse ellipse = new Ellipse { Name = "m_Ellipse1" };
            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>() { rectangle, ellipse });

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Assert.AreEqual(2, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
            Assert.AreEqual("m_Rectangle1", ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[0].Name);
            Assert.AreEqual("m_Ellipse1", ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[1].Name);
        }

        [Test]
        public void AddingAnElementAddsItToObjects()
        {
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };

            m_ObjectBrowserService.AddElement(rectangle);

            Assert.AreEqual(1, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
            Assert.AreEqual("m_Rectangle1", ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[0].Name);
        }

        [Test]
        public void AddingElementDoesntAddToObjectsWhenObjectBrowserControlIsNull()
        {
            TestHelper.AddServiceStub<IWindowServiceIde>();
            m_ObjectBrowserService = new ObjectBrowserService(TextsIde.ObjectBrowser);

            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };

            m_ScreenDesignerView.AddElement(rectangle);

            Assert.AreEqual(0, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
        }

        [Test]
        public void AddingElementDoesntAddToObjectsWhenObjectBrowserControlIsNullAndThenSyncWithNotNull()
        {
            TestHelper.AddServiceStub<IWindowServiceIde>();
            m_ObjectBrowserService = new ObjectBrowserService(TextsIde.ObjectBrowser);

            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };

            m_ScreenDesignerView.AddElement(rectangle);

            Assert.AreEqual(0, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
        }

        [Test]
        public void RemovingAnElementRemovesItFromObjects()
        {
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };

            m_ObjectBrowserService.AddElement(rectangle);
            m_ObjectBrowserService.RemoveElement(rectangle);

            Assert.AreEqual(0, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
        }

        [Test]
        public void SettingActiveScreenDesignerToNullRemovesObjects()
        {
            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };

            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>() { rectangle });
            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            m_ObjectBrowserService.SyncWithDesignerView(null);

            Assert.AreEqual(0, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
        }

        [Test]
        public void SettingActiveScreenDesignerToAnotherScreenDesignerReplacesObjects()
        {
            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };
            Ellipse ellipse = new Ellipse { Name = "m_Ellipse1" };
            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement> { rectangle });
            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            IScreenDesignerView anotherScreenDesignerView = CreateScreenDesignerView();
            anotherScreenDesignerView.RootElements.Returns(new List<FrameworkElement>() { ellipse });
            m_ObjectBrowserService.SyncWithDesignerView(anotherScreenDesignerView);

            Assert.AreEqual(1, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
            Assert.AreEqual("m_Ellipse1", ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[0].Name);
        }

        [Test]
        public void AddingAnElementToScreenUpdatesObjects()
        {
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());
            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Rectangle rectangle = new Rectangle() { Name = "m_Rectangle" };
            m_ScreenDesignerView.AddElement(rectangle);

            Assert.AreEqual(1, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
            Assert.AreEqual("m_Rectangle", ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[0].Name);
        }

        [Test]
        public void RemovingAnElementFromScreenUpdatesObjects()
        {
            Rectangle rectangle = new Rectangle() { Name = "m_Rectangle1" };
            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>());
            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);
            m_ObjectBrowserService.AddElement(rectangle);

            m_ScreenDesignerView.RemoveElement(rectangle);

            Assert.AreEqual(0, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
        }

        [Test]
        public void GroupingElementsInScreenUpdatesObjects()
        {
            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };
            Ellipse ellipse = new Ellipse { Name = "m_Ellipse1" };

            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>() { rectangle, ellipse });

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            Group group = new Group() { Name = "m_Group1" };

            m_ScreenDesignerView.AddElement(group);

            group.Items.Add(rectangle);
            Raise.Event<ElementEventArgs>(m_ElementChangeService, new ElementEventArgs(rectangle, "Screen1"));

            group.Items.Add(ellipse);
            Raise.Event<ElementEventArgs>(m_ElementChangeService, new ElementEventArgs(rectangle, "Screen1"));

            Assert.AreEqual(1, ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items.Count);
            IObjectContainer objectContainer = ((ObjectBrowserService)m_ObjectBrowserService).RootObject.Items[0] as IObjectContainer;
            Assert.IsNotNull(objectContainer);
            Assert.AreEqual("m_Group1", objectContainer.Name);

            Assert.AreEqual(2, objectContainer.Items.Count);
            Assert.AreEqual("m_Rectangle1", objectContainer.Items[0].Name);
            Assert.AreEqual("m_Ellipse1", objectContainer.Items[1].Name);
        }

        [Test]
        public void UngroupingElementsInScreenUpdatesObjects()
        {
            Rectangle rectangle = new Rectangle { Name = "m_Rectangle1" };
            Ellipse ellipse = new Ellipse { Name = "m_Ellipse1" };

            Group group = new Group() { Name = "m_Group1" };
            group.Items.Add(rectangle);
            group.Items.Add(ellipse);
            m_ObjectBrowserService.CreateObjectBrowserControl();
            m_ScreenDesignerView.RootElements.Returns(new List<FrameworkElement>() { group });

            m_ObjectBrowserService.SyncWithDesignerView(m_ScreenDesignerView);

            group.Items.Remove(rectangle);
            Raise.Event<ElementEventArgs>(m_ElementChangeService, new ElementEventArgs(rectangle, "Screen1"));
            group.Items.Remove(ellipse);
            Raise.Event<ElementEventArgs>(m_ElementChangeService, new ElementEventArgs(ellipse, "Screen1"));
            Raise.Event<ElementEventArgs>(m_ElementChangeService, new ElementEventArgs(group, "Screen1"));

            IObjectContainer rootObject = ((ObjectBrowserService)m_ObjectBrowserService).RootObject;
            Assert.AreEqual(2, rootObject.Items.Count);
            Assert.AreEqual("m_Rectangle1", rootObject.Items[0].Name);
            Assert.AreEqual("m_Ellipse1", rootObject.Items[1].Name);
        }
    }
}
