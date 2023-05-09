using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.ControlsIde.TestHelpers;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class AngularViewModelTest : RibbonViewModelTestBase
    {
        private CircularMeter m_SelectedCircularBar;

        [SetUp]
        public void RunBeforeEachTest()
        {
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();

            m_SelectedCircularBar = new CircularMeter();
            GlobalSelectionServiceStub.Stub(x => x.PrimarySelection).Repeat.Any().Return(m_SelectedCircularBar);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(CircularMeter.EndAngleProperty.Name, 0D)).Repeat.Once().Return((double)1);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(CircularMeter.StartAngleProperty.Name, 0D)).Repeat.Once().Return((double)2);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(CircularMeter.ScaleTextRotationProperty.Name, TickRotation.None)).Repeat.Once().Return(TickRotation.None);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(CircularMeter.ClockwiseProperty.Name, true)).Repeat.Once().Return(false);

            ExtendedAngularViewModel viewModel = new ExtendedAngularViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.EndAngle.Value, Is.EqualTo(1));
            Assert.That(viewModel.StartAngle.Value, Is.EqualTo(2));
            Assert.That(viewModel.SelectedTextRotation.Value, Is.EqualTo(TickRotation.None));
            Assert.That(viewModel.Clockwise.Value, Is.False);
        }

        [Test]
        public void SettingTheStartAngleUpdatesThePropertyOfTheSelectedObjectWithUndoInformation()
        {
            double expectedNewStartAngleValue = 100;

            var viewModel = new AngularViewModel();
            viewModel.StartAngle.Value = expectedNewStartAngleValue;

            AssertSetPropertyInCommandServiceWasCalled(CircularMeter.StartAngleProperty.Name, expectedNewStartAngleValue, CommandTextsIde.StartAngle);
        }

        [Test]
        public void SettingTheEndAngleUpdatesThePropertyOfTheSelectedObjectWithUndoInformation()
        {
            double expectedNewEndAngleValue = 100;

            var viewModel = new AngularViewModel();
            viewModel.EndAngle.Value = expectedNewEndAngleValue;

            AssertSetPropertyInCommandServiceWasCalled(CircularMeter.EndAngleProperty.Name, expectedNewEndAngleValue, CommandTextsIde.EndAngle);
        }

        [Test]
        public void SettingCounterClockwiseUpdatesThePropertyOfTheSelectedObjectWithUndoInformation()
        {
            bool expectedNewClockwiseValue = false;

            var viewModel = new AngularViewModel();
            viewModel.Clockwise.Value = expectedNewClockwiseValue;

            AssertSetPropertyInCommandServiceWasCalled(CircularMeter.ClockwiseProperty.Name, expectedNewClockwiseValue, CommandTextsIde.Clockwise);
        }

        [Test]
        public void ShouldNotifyWhenUpdatingTheEndAngle()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.EndAngle.Value = 0;

            Assert.True(viewModel.EndAngle.NotifiesOn(x => x.Value).When(x => x.Value = 100));
        }

        [Test]
        public void ShouldNotNotifyWhenUpdatingTheEndAngleWithTheSameValue()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.EndAngle.Value = 100;

            Assert.False(viewModel.EndAngle.NotifiesOn(x => x.Value).When(x => x.Value = 100));
        }

        [Test]
        public void ShouldNotifyWhenUpdatingTheStartAngle()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.StartAngle.Value = 0;

            Assert.True(viewModel.StartAngle.NotifiesOn(x => x.Value).When(x => x.Value = 100));
        }

        [Test]
        public void ShouldNotNotifyWhenUpdatingTheStartAngleWithTheSameValue()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.StartAngle.Value = 100;

            Assert.False(viewModel.StartAngle.NotifiesOn(x => x.Value).When(x => x.Value = 100));
        }

        [Test]
        public void ShouldNotifyWhenUpdatingClockwise()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.Clockwise.Value = true;

            Assert.True(viewModel.Clockwise.NotifiesOn(x => x.Value).When(x => x.Value = false));
        }

        [Test]
        public void ShouldNotNotifyWhenUpdatingClockwiseWithTheSameValue()
        {
            AngularViewModel viewModel = new AngularViewModel();
            viewModel.Clockwise.Value = true;

            Assert.False(viewModel.Clockwise.NotifiesOn(x => x.Value).When(x => x.Value = true));
        }

        // Write test for angular text rotations when its solved with resourcing of enums!

        internal class ExtendedAngularViewModel : AngularViewModel
        {
            public ExtendedAngularViewModel ExecuteUpdateContent()
            {
                base.UpdateContent();
                return this;
            }
        }
    }
}
