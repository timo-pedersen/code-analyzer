using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Common.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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
            m_RibbonContextManager = Substitute.For<IRibbonContextManager>();
            RibbonStrategy = new SimpleRibbonStrategyDouble(m_RibbonContextManager);
        }

        [Test]
        public void ShouldSelectHomeTabWhenScreenIsSelected()
        {
            m_PrimarySelection = Substitute.For<IScreen>();

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.Received().SelectRibbonTab(RibbonConstants.RibbonTabNameHome);
        }

        private RibbonTab RibbonTabStub(string tabName)
        {
            RibbonTab tab = Substitute.For<RibbonTab>();
            tab.Name = tabName;
            return tab;
        }

        private void StubContextContainerAndSelectedRibbonTab(string tabName)
        {
            m_RibbonContextManager.SetVisibleContextContainersForSelection(Arg.Any<System.Collections.ICollection>())
                .Returns(m_RibbonContextContainers);
            RibbonTab tab = RibbonTabStub(tabName);
            m_RibbonContextManager.SelectedRibbonTab.Returns(tab);
        }

        private IRibbonContextContainer ContextualViewModelStub(string belongsToTabName)
        {
            IRibbonContextContainer container = Substitute.For<IRibbonContextContainer>();
            container.TabName = belongsToTabName;
            return container;
        }

        [Test]
        public void ShouldSetTabBasedOnTheRibbonContextContainerWhenTheContainerBelongsToTheSameTab()
        {
            m_RibbonContextContainers.Add(ContextualViewModelStub("TabA"));
            StubContextContainerAndSelectedRibbonTab("TabA");
            m_PrimarySelection = Substitute.For<IArc>();

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.Received(1).SelectRibbonTab("TabA");
        }

        [Test]
        public void WhenTheContainerNotBelongsToTheSameTabAndTheTabIsNotADefaultTabItShouldUseTheDefaultRibbonContext()
        {
            StubContextContainerAndSelectedRibbonTab("TabB");
            m_PrimarySelection = Substitute.For<IArc>();
            m_RibbonContextManager.GetDefaultRibbonContext(m_SelectedObjects).Returns("TheDefaultContextualTab");
            
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.Received(1).SelectRibbonTab("TheDefaultContextualTab");
        }

        [Test]
        public void WhenTheContainerBelongsToTheSameTabAndTheresNoChangeInSelectedTypeItShallUpdateTheContent()
        {
            SetupFirstSelectionOfObject<IArc>();
            m_PrimarySelection = Substitute.For<IArc>();
            m_RibbonContextContainers.Add(ContextualViewModelStub("TabA"));
            StubContextContainerAndSelectedRibbonTab("TabA");

            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.Received(1).UpdateContent(m_RibbonContextContainers);
        }

        [Test]
        public void WhenSelectionDiffersOnTypeComparedToPreviousSelectionItShouldShowContextualContainersForTheNewSelection()
        {
            SetupFirstSelectionOfObject<IArc>();
            m_PrimarySelection = Substitute.For<IScreen>();
            m_RibbonContextManager.SetVisibleContextContainersForSelection(m_SelectedObjects).Returns(m_RibbonContextContainers);
          
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.Received().SetVisibleContextContainersForSelection(m_SelectedObjects);
        }

        [Test]
        public void WhenSelectionDoesNotDifferOnTypeComparedToPreviousSelectionItShouldShowTheSameContextualContainersAsBefore()
        {
            SetupFirstSelectionOfObject<IScreen>();
            m_PrimarySelection = Substitute.For<IScreen>();
            
            SelectPrimaryObjectAndUpdateRibbonStrategy();

            m_RibbonContextManager.DidNotReceive().SetVisibleContextContainersForSelection(m_SelectedObjects);
        }

        private void SetupFirstSelectionOfObject<TTypeOfObjectToBeSelected>() where TTypeOfObjectToBeSelected:class
        {
            m_PrimarySelection = Substitute.For<TTypeOfObjectToBeSelected>();
            m_RibbonContextManager.SelectedRibbonTab.Returns(RibbonTabStub(""));
            m_RibbonContextManager.SetVisibleContextContainersForSelection(Arg.Any<System.Collections.ICollection>())
                .Returns(m_RibbonContextContainers);
            SelectPrimaryObjectAndUpdateRibbonStrategy();
            ClearSelection();
            m_RibbonContextManager = Substitute.For<IRibbonContextManager>();

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
