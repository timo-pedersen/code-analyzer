using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Controls;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.ObjectBrowser;
using Neo.ApplicationFramework.Controls.Screen.Design;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Screen.ScreenDesign;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Neo.ApplicationFramework.Tools.ObjectBrowser
{
    [TestFixture]
    public class ElementObjectTest
    {
        private ISelectionService m_SelectionService;
        private IObject m_ElementObject;
        private Rectangle m_Rectangle;

        [SetUp]
        public void SetUp()
        {
            m_SelectionService = MockRepository.GenerateStub<ISelectionService>();

            SetupElementObject(m_SelectionService, null);
        }

        #region Creating and disposing element objects

        [Test]
        public void CreatingAnElementObjectHooksupSelectionChanged()
        {
            m_SelectionService = MockRepository.GenerateMock<ISelectionService>();
            m_SelectionService.Expect(x => x.SelectionChanged += null).IgnoreArguments();

            SetupElementObject(m_SelectionService, null);

            m_SelectionService.VerifyAllExpectations();
        }

        [Test]
        public void DisposingAnElementObjectUnhooksSelectionChanged()
        {
            m_SelectionService = MockRepository.GenerateMock<ISelectionService>();
            m_SelectionService.Expect(x => x.SelectionChanged -= null).IgnoreArguments();
            SetupElementObject(m_SelectionService, null);

            m_ElementObject.Dispose();

            m_SelectionService.VerifyAllExpectations();
        }

        [Test]
        public void CreatingAnElementObjectHooksupElementChanged()
        {
            IElementChangeService elementChangeService = MockRepository.GenerateMock<IElementChangeService>();
            elementChangeService.Expect(x => x.ElementChanged += null).IgnoreArguments();

            SetupElementObject(null, elementChangeService);

            elementChangeService.VerifyAllExpectations();
        }

        [Test]
        public void DisposingAnElementObjectUnhooksElementChanged()
        {
            IElementChangeService elementChangeService = MockRepository.GenerateMock<IElementChangeService>();
            elementChangeService.Expect(x => x.ElementChanged -= null).IgnoreArguments();
            SetupElementObject(null, elementChangeService);

            m_ElementObject.Dispose();

            elementChangeService.VerifyAllExpectations();
        }

        #endregion

        #region Selecting objects

        [Test]
        public void SettingIsSelectedToTrueAddsElementToSelectedComponents()
        {
            m_ElementObject.IsSelected = true;

            m_SelectionService.AssertWasCalled(x => x.SetSelectedComponents(new object[] { m_Rectangle }, SelectionTypes.Add));
        }

        [Test]
        public void SettingIsSelectedToFalseRemovesElementFromSelectedComponents()
        {
            m_ElementObject.IsSelected = true;

            m_ElementObject.IsSelected = false;

            m_SelectionService.AssertWasCalled(x => x.SetSelectedComponents(new object[] { m_Rectangle }, SelectionTypes.Remove));
        }

        [Test]
        public void SettingIsSelectedToTrueHasNoEffectWhenElementIsLocked()
        {
            m_ElementObject.IsLocked = true;

            m_ElementObject.IsSelected = true;

            Assert.IsFalse(m_ElementObject.IsSelected);
            m_SelectionService.AssertWasNotCalled(x => x.SetSelectedComponents(new object[] { m_Rectangle }, SelectionTypes.Add));
        }

        [Test]
        public void IsSelectedIsSetToTrueWhenSelectionChangedIsFiredAndContainsElement()
        {
            m_SelectionService.Stub(x => x.GetComponentSelected(m_Rectangle)).Return(true);

            IEventRaiser eventRaiser = m_SelectionService.GetEventRaiser(x => x.SelectionChanged += null);
            eventRaiser.Raise(m_SelectionService, EventArgs.Empty);

            Assert.IsTrue(m_ElementObject.IsSelected);
        }

        [Test]
        public void IsSelectedIsSetToFalseWhenSelectionChangedIsFiredAndNotContainsElement()
        {
            m_ElementObject.IsSelected = true;

            IEventRaiser eventRaiser = m_SelectionService.GetEventRaiser(x => x.SelectionChanged += null);
            eventRaiser.Raise(m_SelectionService, EventArgs.Empty);

            Assert.IsFalse(m_ElementObject.IsSelected);
        }

        #endregion

        #region Locking objects

        [Test]
        public void CreatingAnElementObjectInitializesIsLocked()
        {
            Rectangle rectangle = new Rectangle() { Name = "Rectangle1" };
            EditorProperties.SetIsLocked(rectangle, true);

            SetupElementObject(m_SelectionService, null, rectangle);

            Assert.IsTrue(m_ElementObject.IsLocked);
        }

        [Test]
        public void SettingIsLockedToTrueLocksElement()
        {
            m_ElementObject.IsLocked = true;

            Assert.IsTrue(EditorProperties.GetIsLocked(m_Rectangle));
        }

        [Test]
        public void SettingIsLockedToFalseUnlocksElement()
        {
            m_ElementObject.IsLocked = true;

            m_ElementObject.IsLocked = false;

            Assert.IsFalse(EditorProperties.GetIsLocked(m_Rectangle));
        }

        [Test]
        public void SettingIsLockedToTrueUnselectsElement()
        {
            m_ElementObject.IsSelected = true;

            m_ElementObject.IsLocked = true;

            Assert.IsFalse(m_ElementObject.IsSelected);
        }

        #endregion

        #region Changing ZIndex

        [Test]
        public void CreatingAnElementObjectInitializesZIndex()
        {
            Rectangle rectangle = new Rectangle() { Name = "Rectangle1" };
            Panel.SetZIndex(rectangle, 10);

            SetupElementObject(m_SelectionService, null, rectangle);

            Assert.AreEqual(10, m_ElementObject.ZIndex);
        }

        [Test]
        public void SettingZIndexOnElementObjectSetsZIndexOnElement()
        {
            m_ElementObject.ZIndex = 33;

            Assert.AreEqual(33, Panel.GetZIndex(m_Rectangle));
        }

        [Test]
        public void SettingZIndexOnElementSetsZIndexOnElementObject()
        {
            IElementChangeService elementChangeService = MockRepository.GenerateStub<IElementChangeService>();
            SetupElementObject(null, elementChangeService);

            Panel.SetZIndex(m_Rectangle, 22);
            IEventRaiser eventRaiser = elementChangeService.GetEventRaiser(x => x.ElementChanged += null);
            eventRaiser.Raise(m_Rectangle, new PropertyChangedEventArgs("ZIndex"));

            Assert.AreEqual(22, m_ElementObject.ZIndex);
        }

        [Test]
        public void ChangingElementNameWillCausePropertyChanged()
        {
            Rectangle rectangle = new Rectangle { Name = "Rectangle1" };
            INeoDesignerHost designerHostStub = MockRepository.GenerateStub<INeoDesignerHost>();
            ElementChangeService elementChangeService = new ElementChangeService(designerHostStub);
            SetupElementObject(null, elementChangeService, rectangle);

            bool wasCalled = false;
            string propertyName = string.Empty;
            var elementObject = m_ElementObject as ElementObject;

            PropertyChangedEventHandler propertyChanged = (sender, args) =>
            {
                wasCalled = true;
                propertyName = args.PropertyName;
            };

            elementObject.PropertyChanged += propertyChanged;
            ((IElementChangeNotificationService)elementChangeService).NotifyElementChanged(m_Rectangle, "Name");

            elementObject.PropertyChanged -= propertyChanged;

            Assert.IsTrue(wasCalled);
            Assert.AreEqual(nameof(elementObject.DisplayName), propertyName);
        }

        #endregion

        private void SetupElementObject(ISelectionService selectionService, IElementChangeService elementChangeService)
        {
            m_Rectangle = new Rectangle() { Name = "Rectangle1" };
            SetupElementObject(selectionService, elementChangeService, m_Rectangle);
        }

        private void SetupElementObject(ISelectionService selectionService, IElementChangeService elementChangeService, Rectangle rectangle)
        {
            IServiceContainer serviceContainer = new TestServiceContainer();
            if (selectionService != null)
            {
                serviceContainer.AddService(selectionService);
            }
            
            if (elementChangeService != null)
            {
                serviceContainer.AddService(elementChangeService);
            }

            m_Rectangle = rectangle;
            m_ElementObject = new ElementObject(rectangle, serviceContainer);
        }
    }
}
