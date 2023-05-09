using System.Collections;
using System.Collections.Generic;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Controls.Ribbon.Context.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Ribbon;
using Neo.ApplicationFramework.Tools.Ribbon.Strategies;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.RibbonManager
{
    [TestFixture]
    public class RibbonContentSelectionChangesTest
    {
        [Test]
        public void TestCaseSingleNonScreenObjectIsSelected()
        {
            var ribbonContextManagerMock = MockRepository.GenerateMock<IRibbonContextManager>();
            var containers = new List<IRibbonContextContainer>();

            var selectedObjects = new ArrayList { "I AM NOT A SCREEN" };
            
            RibbonTab selectedRibbonTab = CreateTab("Dynamics");
            IRibbonStrategy ribbonStrategy = new SimpleRibbonStrategy(ribbonContextManagerMock);

            SetupExpectationsForFirstCallToUpdate(ribbonContextManagerMock, containers, selectedObjects, selectedRibbonTab, CreateGeneralTab());

            ribbonStrategy.Update(selectedObjects, null);

            ribbonContextManagerMock.VerifyAllExpectations();
        }

        [Test]
        public void TestCaseNonScreenObjectIsSelectedWhenOtherObjectOfSameTypeIsAlreadySelected()
        {
            var ribbonContextManagerMock = MockRepository.GenerateMock<IRibbonContextManager>();
            var containers = new List<IRibbonContextContainer>();

            var selectedObjects = new ArrayList { "I AM NOT A SCREEN" };
            var selectedObjects2 = new ArrayList { "I AM ALSO A STRING" };

            RibbonTab selectedRibbonTab = CreateTab("Dynamics");
            IRibbonStrategy ribbonStrategy = new SimpleRibbonStrategy(ribbonContextManagerMock);
            var tab = new RibbonContextTab { Name = "General" };

            SetupExpectationsForFirstCallToUpdate(ribbonContextManagerMock, containers, selectedObjects, selectedRibbonTab, tab);

            ribbonContextManagerMock.Expect(x => x.SelectedRibbonTab).Return(tab);
            ribbonContextManagerMock.Expect(x => x.GetDefaultRibbonContext(selectedObjects2)).Return(tab.Name);

            ribbonContextManagerMock.SelectedRibbonTab = tab;
            ribbonContextManagerMock.UpdateContent(containers);

            ribbonStrategy.Update(selectedObjects, null);
            ribbonStrategy.Update(selectedObjects2, null);

            ribbonContextManagerMock.VerifyAllExpectations();
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
            ribbonContextManagerMock.Expect(x => x.SetVisibleContextContainersForSelection(selectedObjects)).Return(containers).Repeat.Once();
            ribbonContextManagerMock.Expect(x => x.SelectedRibbonTab).Return(selectedRibbonTab).Repeat.Once();
            ribbonContextManagerMock.Expect(x => x.GetDefaultRibbonContext(selectedObjects)).Return(selectedObjectDefaultRibbonTabItem.Name).Repeat.Once();
            ribbonContextManagerMock.Expect(x => x.SelectRibbonTab(selectedObjectDefaultRibbonTabItem.Name)).Repeat.Once();
        }
    }
}
