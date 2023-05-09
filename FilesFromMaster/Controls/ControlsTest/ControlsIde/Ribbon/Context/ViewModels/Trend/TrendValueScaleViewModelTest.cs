using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Trend;
using Neo.ApplicationFramework.Controls.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels.Trend
{
    [TestFixture]
    public class TrendValueScaleViewModelTest : RibbonViewModelTestBase
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
        public void UpdateContentSetsTheProperties()
        {

            GlobalCommandServiceStub.Stub(x => x.GetProperty(Arg<string>.Is.Equal(TrendViewer.ValueScaleMinimumProperty.Name), Arg<object>.Is.Anything)).Repeat.Once().Return((double)1D);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(Arg<string>.Is.Equal(TrendViewer.ValueScaleMaximumProperty.Name), Arg<object>.Is.Anything)).Repeat.Once().Return((double)100D);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(Arg<string>.Is.Equal(TrendViewer.ValueScaleMajorTickCountProperty.Name), Arg<object>.Is.Anything)).Repeat.Once().Return((int)10);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(Arg<string>.Is.Equal(TrendViewer.ValueScaleMinorTickCountProperty.Name), Arg<object>.Is.Anything)).Repeat.Once().Return((int)1);

            ExtendedTrendValueScaleViewModel viewModel = new ExtendedTrendValueScaleViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.ValueScaleMinorTickCount, Is.EqualTo(1));
            Assert.That(viewModel.ValueScaleMajorTickCount, Is.EqualTo(10));
            Assert.That(viewModel.ValueScaleMaximum, Is.EqualTo(100D));
            Assert.That(viewModel.ValueScaleMinimum, Is.EqualTo(1D));
        }

        [Test]
        public void SettingTheMajorTicksUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            int expectedNewMajorTicks = TrendLimits.MaxValueScaleMajorTickCount;

            new TrendValueScaleViewModel { ValueScaleMajorTickCount = expectedNewMajorTicks };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMajorTickCountProperty.Name, expectedNewMajorTicks, CommandTextsIde.MajorTicks);
        }

        [Test]
        public void SettingTheValueScaleMajorTickCountWithinTrendLimitsSetsValue()
        {
            int expectedNewMajorTicks = TrendLimits.MaxValueScaleMajorTickCount-1;

            new TrendValueScaleViewModel { ValueScaleMajorTickCount = expectedNewMajorTicks };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMajorTickCountProperty.Name, expectedNewMajorTicks, CommandTextsIde.MajorTicks);
        }

        [Test]
        public void SettingTheValueScaleMinorTickCountWithinTrendLimitsSetsValue()
        {
            int expectedNewMinorTicks = TrendLimits.MaxValueScaleMinorTickCount-1;

            new TrendValueScaleViewModel { ValueScaleMinorTickCount = expectedNewMinorTicks };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMinorTickCountProperty.Name, expectedNewMinorTicks, CommandTextsIde.MinorTicks);
        }


        [Test]
        public void SettingTheValueScaleMajorTickCountAboveTrendLimitDoesntSetValue()
        {
            int expectedNewMajorTicks = TrendLimits.MaxValueScaleMajorTickCount + 1;

            new TrendValueScaleViewModel { ValueScaleMajorTickCount = expectedNewMajorTicks };

            AssertSetPropertyInCommandServiceWasNotCalled(TrendViewer.ValueScaleMajorTickCountProperty.Name, expectedNewMajorTicks, CommandTextsIde.MajorTicks);
        }

        [Test]
        public void SettingTheValueScaleMajorTickCountBelowTrendLimitDoesntSetValue()
        {
            int expectedNewMajorTicks = TrendLimits.MinValueScaleMajorTickCount -1;

            new TrendValueScaleViewModel { ValueScaleMajorTickCount = expectedNewMajorTicks };

            AssertSetPropertyInCommandServiceWasNotCalled(TrendViewer.ValueScaleMajorTickCountProperty.Name, expectedNewMajorTicks, CommandTextsIde.MajorTicks);
        }

        [Test]
        public void SettingTheValueScaleMinorTickCountAboveTrendLimitDoesntSetValue()
        {
            int expectedNewMinorTicks = TrendLimits.MaxValueScaleMinorTickCount + 1;

            new TrendValueScaleViewModel { ValueScaleMinorTickCount = expectedNewMinorTicks };

            AssertSetPropertyInCommandServiceWasNotCalled(TrendViewer.ValueScaleMinorTickCountProperty.Name, expectedNewMinorTicks, CommandTextsIde.MinorTicks);
        }

        [Test]
        public void SettingTheValueScaleMinorTickCountBelowTrendLimitDoesntSetValue()
        {
            int expectedNewMinorTicks = TrendLimits.MinValueScaleMinorTickCount -1;

            new TrendValueScaleViewModel { ValueScaleMinorTickCount = expectedNewMinorTicks };

            AssertSetPropertyInCommandServiceWasNotCalled(TrendViewer.ValueScaleMinorTickCountProperty.Name, expectedNewMinorTicks, CommandTextsIde.MinorTicks);
        }

        [Test]
        public void SettingTheMinorTicksUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            int expectedNewMinorTicks = 1;

            new TrendValueScaleViewModel { ValueScaleMinorTickCount = expectedNewMinorTicks };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMinorTickCountProperty.Name, expectedNewMinorTicks, CommandTextsIde.MinorTicks);
        }

        [Test]
        public void SettingMinValueUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            double expectedNewMinValue = 10.1D;

            new TrendValueScaleViewModel { ValueScaleMinimum = expectedNewMinValue };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMinimumProperty.Name, expectedNewMinValue, CommandTextsIde.MinValue);
        }

        [Test]
        public void SettingMaxValueUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            double expectedNewMaxValue = 10.9D;

            new TrendValueScaleViewModel { ValueScaleMaximum = expectedNewMaxValue };

            AssertSetPropertyInCommandServiceWasCalled(TrendViewer.ValueScaleMaximumProperty.Name, expectedNewMaxValue, CommandTextsIde.MaxValue);
        }

        internal class ExtendedTrendValueScaleViewModel : TrendValueScaleViewModel
        {
            public ExtendedTrendValueScaleViewModel ExecuteUpdateContent()
            {
                base.UpdateContent();
                return this;
            }
        }
    }
}
