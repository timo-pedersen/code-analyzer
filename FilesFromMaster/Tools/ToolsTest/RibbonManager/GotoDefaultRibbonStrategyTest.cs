using System;
using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.RibbonManager;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_RibbonContextManager.VerifyAllExpectations();
        }

        protected override void SetupExpectationsForRibbonSelection(string ribbonToReturnFromGet, string ribbonToMerge, string ribbonToSelect)
        {
            base.SetupExpectationsForRibbonSelection(ribbonToReturnFromGet, ribbonToMerge, ribbonToSelect);

            m_RibbonContextManager.Expect(x => x.SelectRibbonTab(ribbonToMerge)).Repeat.Once();
            m_RibbonContextManager.Expect(x => x.SetVisibleContextContainersForSelection(m_Selection)).Return(m_RibbonContextContainerList).Repeat.Once();
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
            m_RibbonContextManager.Expect(x => x.SelectRibbonTab("")).Repeat.Once();
            m_Selection = new List<object>();
            m_PrimarySelection = null;
            m_RibbonContextContainerList = new List<IRibbonContextContainer>();
            m_RibbonContextManager.Expect(x => x.SetVisibleContextContainersForSelection(m_Selection)).Return(m_RibbonContextContainerList).Repeat.Once();
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatANonEmptySelectionResultsInContextContainers()
        {
            m_DefaultRibbonContext = "General";
            m_RibbonContextManager.Expect(x => x.SelectRibbonTab("General")).Repeat.Once();

            m_RibbonContextManager.Stub(x => x.SelectedRibbonTab).Return(new RibbonTab() { Name = "General" });
            m_RibbonContextContainerList = new List<IRibbonContextContainer>();
            m_RibbonContextContainerList.Add(new TestRibbonContextContainer("General"));
            m_RibbonContextManager.Expect(x => x.SetVisibleContextContainersForSelection(m_Selection)).Return(m_RibbonContextContainerList);
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatTabChangeWillHappenWhenPreviousPropertyIsSelected()
        {
            SetupExpectationsForRibbonSelection("Appearance", "Appearance", "Appearance");
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatTabChangeWillHappenWhenPreviousPropertyIsSelectedDifferentPropertyName()
        {
            SetupExpectationsForRibbonSelection("Appearance", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();
        }

        #region Go to Standard tab
        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenHomeTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("Home", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenViewTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("View", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenInsertTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("Insert", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();
        }

        [Test]
        public void TestThatTabChangeWillGotoDefaultWhenToolBoxTabIsSelected()
        {
            SetupExpectationsForRibbonSelection("ToolBox", "Actions", "Actions");
            SetSelectionAndUpdateRibbon();
        }
        #endregion
    }
}
