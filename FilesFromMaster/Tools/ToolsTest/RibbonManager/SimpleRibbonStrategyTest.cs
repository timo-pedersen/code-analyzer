using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Common.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Ribbon.Strategies
{
    [TestFixture]
    public class SimpleRibbonStrategyTest 
    {
        protected IRibbonContextManager m_RibbonContextManager;
        protected IRibbonStrategy RibbonStrategy { get; set; }
        protected object m_PrimarySelection;
        protected List<object> m_SelectedObjects;
        protected List<IRibbonContextContainer> m_RibbonContextContainers;

        protected void CommonSetup()
        {
            m_RibbonContextContainers = new List<IRibbonContextContainer>();
            m_PrimarySelection = null;
            m_SelectedObjects = new List<object>();
        }

        [SetUp]
        public void Setup()
        {
            CommonSetup();
            m_RibbonContextManager = MockRepository.GenerateMock<IRibbonContextManager>();
            RibbonStrategy = new SimpleRibbonStrategyDouble(m_RibbonContextManager);
        }

        [Test]
        public void ShouldSelectHomeTabWhenScreenIsSelected()
        {
            m_RibbonContextManager.Expect(x => x.SelectRibbonTab(RibbonConstants.RibbonTabNameHome));
            m_PrimarySelection = MockRepository.GenerateStub<IScreen>();

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.VerifyAllExpectations();
        }

        private RibbonTab RibbonTabStub(string tabName)
        {
            RibbonTab tab = MockRepository.GenerateStub<RibbonTab>();
            tab.Name = tabName;
            return tab;
        }

        private void StubContextContainerAndSelectedRibbonTab(string tabName)
        {
            m_RibbonContextManager.Stub(x => x.SetVisibleContextContainersForSelection(null)).IgnoreArguments().Return(m_RibbonContextContainers);
            m_RibbonContextManager.Stub(x => x.SelectedRibbonTab).Return(RibbonTabStub(tabName));
        }

        private IRibbonContextContainer ContextualViewModelStub(string belongsToTabName)
        {
            IRibbonContextContainer container = MockRepository.GenerateStub<IRibbonContextContainer>();
            container.TabName = belongsToTabName;
            return container;
        }

        [Test]
        public void ShouldSetTabBasedOnTheRibbonContextContainerWhenTheContainerBelongsToTheSameTab()
        {
            m_RibbonContextContainers.Add(ContextualViewModelStub("TabA"));
            StubContextContainerAndSelectedRibbonTab("TabA");
            m_PrimarySelection = MockRepository.GenerateStub<IArc>();

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.AssertWasCalled(x => x.SelectRibbonTab("TabA"), x => x.Repeat.Once());
        }

        [Test]
        public void WhenTheContainerNotBelongsToTheSameTabAndTheTabIsNotADefaultTabItShouldUseTheDefaultRibbonContext()
        {
            StubContextContainerAndSelectedRibbonTab("TabB");
            m_PrimarySelection = MockRepository.GenerateStub<IArc>();
            m_RibbonContextManager.Stub(x => x.GetDefaultRibbonContext(m_SelectedObjects)).Return("TheDefaultContextualTab");
            
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.AssertWasCalled(x => x.SelectRibbonTab("TheDefaultContextualTab"), x => x.Repeat.Once());
        }

        [Test]
        public void WhenTheContainerBelongsToTheSameTabAndTheresNoChangeInSelectedTypeItShallUpdateTheContent()
        {
            SetupFirstSelectionOfObject<IArc>();
            m_PrimarySelection = MockRepository.GenerateStub<IArc>();
            m_RibbonContextContainers.Add(ContextualViewModelStub("TabA"));
            StubContextContainerAndSelectedRibbonTab("TabA");

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.AssertWasCalled(x => x.UpdateContent(m_RibbonContextContainers), x => x.Repeat.Once());
        }

        [Test]
        public void WhenSelectionDiffersOnTypeComparedToPreviousSelectionItShouldShowContextualContainersForTheNewSelection()
        {
            SetupFirstSelectionOfObject<IArc>();
            m_PrimarySelection = MockRepository.GenerateStub<IScreen>();
            m_RibbonContextManager.Expect(x => x.SetVisibleContextContainersForSelection(m_SelectedObjects)).Return(m_RibbonContextContainers);
          
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.VerifyAllExpectations();
        }

        [Test]
        public void WhenSelectionDoesNotDifferOnTypeComparedToPreviousSelectionItShouldShowTheSameContextualContainersAsBefore()
        {
            SetupFirstSelectionOfObject<IScreen>();
            m_PrimarySelection = MockRepository.GenerateStub<IScreen>();
            
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.AssertWasNotCalled(x => x.SetVisibleContextContainersForSelection(m_SelectedObjects));
        }

        private void SetupFirstSelectionOfObject<TTypeOfObjectToBeSelected>() where TTypeOfObjectToBeSelected:class
        {
            m_PrimarySelection = MockRepository.GenerateStub<TTypeOfObjectToBeSelected>();
            m_RibbonContextManager.Stub(x => x.SelectedRibbonTab).Return(RibbonTabStub(""));
            m_RibbonContextManager.Stub(x => x.SetVisibleContextContainersForSelection(null)).IgnoreArguments().Return(m_RibbonContextContainers);
            SelectPrimaryObjectAndUpdateRibbonStrategy();
            ClearSelection();
            m_RibbonContextManager = MockRepository.GenerateMock<IRibbonContextManager>();

            //Theres no good possibility to reset the mock and we dont want the setup to be so coupled to the implementation.
            ((SimpleRibbonStrategyDouble)RibbonStrategy).UpdateContextManager(m_RibbonContextManager);
          
        }

        private void ClearSelection()
        {
            m_SelectedObjects.Clear();
            m_PrimarySelection = null;
        }

        private void SelectPrimaryObjectAndUpdateRibbonStrategy()
        {
            m_SelectedObjects.Add(m_PrimarySelection);
            RibbonStrategy.Update(m_SelectedObjects, m_PrimarySelection);
        }


        internal class SimpleRibbonStrategyDouble: SimpleRibbonStrategy
        {
            public void UpdateContextManager(IRibbonContextManager contextManager)
            {
                m_RibbonContextManager = contextManager;
            }

            public SimpleRibbonStrategyDouble(IRibbonContextManager ribbonContextManager) : base(ribbonContextManager)
            {
            }
        }
    }
}
