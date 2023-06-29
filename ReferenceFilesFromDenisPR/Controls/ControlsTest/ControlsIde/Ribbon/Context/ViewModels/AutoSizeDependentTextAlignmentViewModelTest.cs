using System.Windows;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
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
            GlobalCommandServiceStub.GetProperty(AutoSizePropertyName, true).Returns(autoSize);
            GlobalCommandServiceStub.GetProperty(TextVerticalAlignmentPropertyName, VerticalAlignment.Center).Returns(VerticalAlignment.Center);
            GlobalCommandServiceStub.GetProperty(TextHorizontalAlignmentPropertyName, HorizontalAlignment.Center).Returns(HorizontalAlignment.Center);

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
