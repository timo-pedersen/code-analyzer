using System.Windows;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Meter;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels.Meter
{
    public class MeterVisibilityViewModelTest : RibbonViewModelTestBase
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void UpdateContentsSetVisibilityAndOfScaleVisabilityBasedOnPrimarySelection()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(Controls.RenderableControl.ScaleVisibleProperty.Name, Visibility.Visible)).Return(Visibility.Visible);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(RenderableControl.TransparentProperty.Name, Visibility.Visible)).Return(Visibility.Visible);

            MeterVisibilityViewModel viewModel = new ExtendedMeterVisibilityViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsScaleVisible, Is.EqualTo(Visibility.Visible));
            Assert.That(viewModel.Transparent, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void SettingTheVisibilityOfScaleForTheSelectedObjectSetsItWithUndoInformation()
        {
            Visibility expectedNewVisibitilty = Visibility.Hidden;

            MeterVisibilityViewModel viewModel = new MeterVisibilityViewModel() { IsScaleVisible = Visibility.Visible };
            viewModel.IsScaleVisible = expectedNewVisibitilty;

            AssertSetPropertyInCommandServiceWasCalled(RenderableControl.ScaleVisibleProperty.Name, expectedNewVisibitilty, CommandTextsIde.Scale);
        }


        [Test]
        public void SettingTheTransperacyTheSelectedObjectSetsItWithUndoInformation()
        {
            Visibility expectedNewTransperency = Visibility.Hidden;

            MeterVisibilityViewModel viewModel = new MeterVisibilityViewModel() { Transparent = Visibility.Visible };
            viewModel.Transparent = expectedNewTransperency;

            AssertSetPropertyInCommandServiceWasCalled(RenderableControl.TransparentProperty.Name, expectedNewTransperency, CommandTextsIde.Transparent);
        }

        internal class ExtendedMeterVisibilityViewModel : MeterVisibilityViewModel
        {
            public MeterVisibilityViewModel ExecuteUpdateContent()
            {
                base.UpdateContent();
                return this;
            }
        }
    }
}
