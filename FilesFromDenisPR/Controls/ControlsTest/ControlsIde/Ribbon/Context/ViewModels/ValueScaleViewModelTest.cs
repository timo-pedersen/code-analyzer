using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class ValueScaleViewModelTest : RibbonViewModelTestBase
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
        }

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            GlobalCommandServiceStub.GetProperty(Controls.Meter.MajorTickCountProperty.Name, 0).Returns((int)10);
            GlobalCommandServiceStub.GetProperty(Controls.Meter.MinorTickCountProperty.Name, 0).Returns((int)1);
            GlobalCommandServiceStub.GetProperty(Controls.Meter.MaximumProperty.Name, 0.0D).Returns((double)100D);
            GlobalCommandServiceStub.GetProperty(Controls.Meter.MinimumProperty.Name, 0.0D).Returns((double)1D);

            ExtendedValueScaleViewModel viewModel = new ExtendedValueScaleViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.MinorTicks, Is.EqualTo(1));
            Assert.That(viewModel.MajorTicks, Is.EqualTo(10));
            Assert.That(viewModel.MaxValue, Is.EqualTo(100D));
            Assert.That(viewModel.MinValue, Is.EqualTo(1D));
        }

        [Test]
        public void SettingTheMajorTicksUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            int expectedNewMajorTicks = 100;

            new ValueScaleViewModel { MajorTicks = expectedNewMajorTicks };

            AssertSetPropertyInCommandServiceWasCalled(Controls.Meter.MajorTickCountProperty.Name, expectedNewMajorTicks, CommandTextsIde.MajorTicks);
        }

        [Test]
        public void SettingTheMinorTicksUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            int expectedNewMinorTicks = 1;

            new ValueScaleViewModel { MinorTicks = expectedNewMinorTicks };

            AssertSetPropertyInCommandServiceWasCalled(Neo.ApplicationFramework.Controls.Controls.Meter.MinorTickCountProperty.Name, expectedNewMinorTicks, CommandTextsIde.MinorTicks);
        }

        [Test]
        public void SettingMinValueUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            double expectedNewMinValue = 10.1D;

            new ValueScaleViewModel { MinValue = expectedNewMinValue };

            AssertSetPropertyInCommandServiceWasCalled(Controls.Meter.MinimumProperty.Name, expectedNewMinValue, CommandTextsIde.MinValue);
        }

        [Test]
        public void SettingMaxValueUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            double expectedNewMaxValue = 10.9D;

            new ValueScaleViewModel { MaxValue = expectedNewMaxValue };

            AssertSetPropertyInCommandServiceWasCalled(Controls.Meter.MaximumProperty.Name, expectedNewMaxValue, CommandTextsIde.MaxValue);
        }

        internal class ExtendedValueScaleViewModel : ValueScaleViewModel
        {
            public ExtendedValueScaleViewModel ExecuteUpdateContent()
            {
                base.UpdateContent();
                return this;
            }
        }
    }
}
