using System.Collections;
using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Controls.Ribbon.Context.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Ribbon;
using Neo.ApplicationFramework.Tools.Ribbon.Strategies;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.RibbonManager
{
    [TestFixture]
    public class RibbonContentSelectionChangesTest
    {
        [Test]
        public void TestCaseSingleNonScreenObjectIsSelected()
        {
            var ribbonContextManagerMock = Substitute.For<IRibbonContextManager>();
            var containers = new List<IRibbonContextContainer>();

            var selectedObjects = new ArrayList { "I AM NOT A SCREEN" };
            
            RibbonTab selectedRibbonTab = CreateTab("Dynamics");
            IRibbonStrategy ribbonStrategy = new SimpleRibbonStrategy(ribbonContextManagerMock);
            
            var selectedObjectDefaultRibbonTabItem = CreateGeneralTab();
            ribbonContextManagerMock.SetVisibleContextContainersForSelection(selectedObjects).Returns(containers);
            ribbonContextManagerMock.SelectedRibbonTab.Returns(selectedRibbonTab);
            ribbonContextManagerMock.GetDefaultRibbonContext(selectedObjects).Returns(selectedObjectDefaultRibbonTabItem.Name);

            ribbonStrategy.Update(selectedObjects, null);

            ribbonContextManagerMock.Received().SetVisibleContextContainersForSelection(selectedObjects);
            ribbonContextManagerMock.Received().GetDefaultRibbonContext(selectedObjects);
            ribbonContextManagerMock.Received().SelectRibbonTab(selectedObjectDefaultRibbonTabItem.Name);
        }

        [Test]
        public void TestCaseNonScreenObjectIsSelectedWhenOtherObjectOfSameTypeIsAlreadySelected()
        {
            var ribbonContextManagerMock = Substitute.For<IRibbonContextManager>();
            var containers = new List<IRibbonContextContainer>();

            var selectedObjects = new ArrayList { "I AM NOT A SCREEN" };
            var selectedObjects2 = new ArrayList { "I AM ALSO A STRING" };

            RibbonTab selectedRibbonTab = CreateTab("Dynamics");
            IRibbonStrategy ribbonStrategy = new SimpleRibbonStrategy(ribbonContextManagerMock);
            var tab = new RibbonContextTab { Name = "General" };

            ribbonContextManagerMock.SetVisibleContextContainersForSelection(selectedObjects).Returns(containers);
            ribbonContextManagerMock.SelectedRibbonTab.Returns(selectedRibbonTab);
            ribbonContextManagerMock.GetDefaultRibbonContext(selectedObjects).Returns(tab.Name);

            ribbonContextManagerMock.SelectedRibbonTab.Returns(tab);
            ribbonContextManagerMock.GetDefaultRibbonContext(selectedObjects2).Returns(tab.Name);

            ribbonContextManagerMock.SelectedRibbonTab = tab;
            ribbonContextManagerMock.UpdateContent(containers);

            ribbonStrategy.Update(selectedObjects, null);
            ribbonStrategy.Update(selectedObjects2, null);

            ribbonContextManagerMock.Received().SetVisibleContextContainersForSelection(selectedObjects);
            ribbonContextManagerMock.Received().GetDefaultRibbonContext(selectedObjects);
            ribbonContextManagerMock.Received().SelectRibbonTab(tab.Name);
        }

        private static RibbonTab CreateTab(string name)
        {
            return new RibbonTab { Name = name };
        }

        private static RibbonContextTab CreateGeneralTab()
        {
            return new RibbonContextTab { Name = "General" };
        }

        private static void SetupExpectationsForFirstCallToUpdate(
            IRibbonContextManager ribbonContextManagerMock,
            List<IRibbonContextContainer> containers,
            ArrayList selectedObjects,
            RibbonTab selectedRibbonTab,
            RibbonContextTab selectedObjectDefaultRibbonTabItem)
        {
            ribbonContextManagerMock.SetVisibleContextContainersForSelection(selectedObjects).Returns(containers);
            ribbonContextManagerMock.SelectedRibbonTab.Returns(selectedRibbonTab);
            ribbonContextManagerMock.GetDefaultRibbonContext(selectedObjects).Returns(selectedObjectDefaultRibbonTabItem.Name);
            ribbonContextManagerMock.SelectRibbonTab(selectedObjectDefaultRibbonTabItem.Name);
        }
    }
}
