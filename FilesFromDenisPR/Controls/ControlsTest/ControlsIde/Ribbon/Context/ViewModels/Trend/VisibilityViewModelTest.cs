using System.Windows;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Trend;
using Neo.ApplicationFramework.Controls.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels.Trend
{
    [TestFixture]
    public class VisibilityViewModelTest : RibbonViewModelTestBase
    {
        private const string GridVisiblePropertyName = "GridVisible";

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void SettingScaleVisibleUpdatesThePropertyAndIncludesUndoInformation()
        {
            Visibility visible = Visibility.Collapsed;

            VisibilityViewModel visibilityViewModel = new VisibilityViewModel { ScaleVisible = visible };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ScaleVisibleProperty.Name, visible, CommandTextsIde.Scale);

            visible = Visibility.Visible;
            visibilityViewModel.ScaleVisible = visible;
            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ScaleVisibleProperty.Name, visible, CommandTextsIde.Scale);
        }
        [Test]
        public void SettingGridVisibleUpdatesThePropertyAndIncludesUndoInformation()
        {
            Visibility visible = Visibility.Collapsed;

            VisibilityViewModel visibilityViewModel = new VisibilityViewModel { GridVisible = visible };

            AssertSetPropertyInCommandServiceWasCalled(GridVisiblePropertyName, visible, TextsIde.TrendGridVisible);

            visible = Visibility.Visible;
            visibilityViewModel.GridVisible = visible;
            AssertSetPropertyInCommandServiceWasCalled(GridVisiblePropertyName, visible, TextsIde.TrendGridVisible);
        }


        internal class ExtendedVisibilityViewModel : VisibilityViewModel
        {
            public ExtendedVisibilityViewModel ExecuteUpdateContent()
            {
                base.UpdateContent();
                return this;
            }
        }
    }
}
