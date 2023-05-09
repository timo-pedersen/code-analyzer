using System.Windows;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class AutoSizeDependentTextAlignmentViewModelTest : RibbonViewModelTestBase
    {
        private readonly string AutoSizePropertyName = Neo.ApplicationFramework.Controls.Label.AutoSizeProperty.Name;
        private const string TextVerticalAlignmentPropertyName = "TextVerticalAlignment";
        private const string TextHorizontalAlignmentPropertyName = "TextHorizontalAlignment";

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

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void UpdateContentSetsTheIsEnabledProperty(bool autoSize, bool expectedIsTextAlignmentEnabled)
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoSizePropertyName, true)).Repeat.Once().Return(autoSize);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextVerticalAlignmentPropertyName, VerticalAlignment.Center)).Repeat.Once().Return(VerticalAlignment.Center);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextHorizontalAlignmentPropertyName, HorizontalAlignment.Center)).Repeat.Once().Return(HorizontalAlignment.Center);

            ExtendedAutoSizeDependentTextAlignmentViewModel viewModel = new ExtendedAutoSizeDependentTextAlignmentViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsTextAlignmentEnabled, Is.EqualTo(expectedIsTextAlignmentEnabled));
        }

        internal class ExtendedAutoSizeDependentTextAlignmentViewModel : AutoSizeDependentTextAlignmentViewModel
        {
            public ExtendedAutoSizeDependentTextAlignmentViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }
        }
    }
}
