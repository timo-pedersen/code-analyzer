using System;
using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.RibbonManager;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Ribbon.Strategies
{
    [TestFixture]
    public class GotoDefaultRibbonStrategyTest : RibbonStrategyBaseTest
    {
        [SetUp]
        public void Setup()
        {
            base.CommonSetup();
            RibbonStrategy = new GotoDefaultRibbonStrategy(m_RibbonContextManager);
        }

        [TearDown]
        public void TearDown()
        {
        }

        protected override void SetupExpectationsForRibbonSelection(string ribbonToReturnFromGet, string ribbonToMerge, string ribbonToSelect)
        {
            base.SetupExpectationsForRibbonSelection(ribbonToReturnFromGet, ribbonToMerge, ribbonToSelect);

            m_RibbonContextManager.SetVisibleContextContainersForSelection(m_Selection).Returns(m_RibbonContextContainerList);
        }

        [Test]
        public void TestCreateRibbonStrategy()
        {
            Assert.IsNotNull(RibbonStrategy);
        }

        [Test]
        public void TestThatAnEmptySelectionResultsInNoContextContainers()
        {
            m_DefaultRibbonContext = String.Empty;
            m_Selection = new List<object>();
            m_PrimarySelection = null;
            m_RibbonContextContainerList = new List<IRibbonContextContainer>();
            m_RibbonContextManager.SetVisibleContextContainersForSelection(m_Selection).Returns(m_RibbonContextContainerList);
            
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        [Test]
        public void TestThatANonEmptySelectionResultsInContextContainers()
        {
            m_DefaultRibbonContext = "General";

            m_RibbonContextManager.SelectedRibbonTab.Returns(new RibbonTab() { Name = "General" });
            m_RibbonContextContainerList = new List<IRibbonContextContainer>();
            m_RibbonContextContainerList.Add(new TestRibbonContextContainer("General"));
            m_RibbonContextManager.SetVisibleContextContainersForSelection(m_Selection).Returns(m_RibbonContextContainerList);
            
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("General");
        }

        [Test]
        public void TestThatTabChangeWillHappenWhenPreviousPropertyIsSelected()
        {
            SetupExpectationsForRibbonSelection("Appearance", "Appearance", "Appearance");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Appearance");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        [Test]
        public void TestThatTabChangeWillHappenWhenPreviousPropertyIsSelectedDifferentPropertyName()
        {
            SetupExpectationsForRibbonSelection("Appearance", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Actions");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        #region Go to Standard tab
        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenHomeTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("Home", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Actions");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenViewTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("View", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Actions");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenInsertTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("Insert", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Actions");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenToolBoxTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("ToolBox", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();

            m_RibbonContextManager.Received(1).SelectRibbonTab("Actions");
            m_RibbonContextManager.Received(1).SetVisibleContextContainersForSelection(m_Selection);
        }
        #endregion
    }
}
