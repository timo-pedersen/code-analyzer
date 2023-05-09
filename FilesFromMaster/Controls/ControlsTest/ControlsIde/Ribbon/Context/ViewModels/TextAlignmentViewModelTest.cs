using System.Windows;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class TextAlignmentViewModelTest : RibbonViewModelTestBase
    {
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

        [TestCase(HorizontalAlignment.Left, true, false, false, false)]
        [TestCase(HorizontalAlignment.Center, false, true, false, false)]
        [TestCase(HorizontalAlignment.Right, false, false, true, false)]
        [TestCase(HorizontalAlignment.Stretch, false, false, false, true)]
        public void UpdateContentSetsTheHorizontalAlignmentProperties(HorizontalAlignment horizontalAlignment,
            bool expectedIsHorizontalLeft,
            bool expectedIsHorizontalCenter,
            bool expectedIsHorizontalRight,
            bool expectedIsHorizontalStretch)
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextHorizontalAlignmentPropertyName, HorizontalAlignment.Center)).Repeat.Once().Return(horizontalAlignment);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextVerticalAlignmentPropertyName, VerticalAlignment.Center)).Repeat.Once().Return(VerticalAlignment.Center);

            ExtendedTextAlignmentViewModel viewModel = new ExtendedTextAlignmentViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsHorizontalLeftAligned, Is.EqualTo(expectedIsHorizontalLeft));
            Assert.That(viewModel.IsHorizontalCenterAligned, Is.EqualTo(expectedIsHorizontalCenter));
            Assert.That(viewModel.IsHorizontalRightAligned, Is.EqualTo(expectedIsHorizontalRight));
            Assert.That(viewModel.IsHorizontalStretchAligned, Is.EqualTo(expectedIsHorizontalStretch));
        }

        [TestCase(HorizontalAlignment.Left, true, false, false, false)]
        [TestCase(HorizontalAlignment.Center, false, true, false, false)]
        [TestCase(HorizontalAlignment.Right, false, false, true, false)]
        [TestCase(HorizontalAlignment.Stretch, false, false, false, true)]
        public void UpdateContentSetsTheVerticalAlignmentProperties(VerticalAlignment verticalAlignment,
            bool expectedIsVerticalTop,
            bool expectedIsVerticalCenter,
            bool expectedIsVerticalBottom,
            bool expectedIsVerticalStretch)
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextHorizontalAlignmentPropertyName, HorizontalAlignment.Center)).Repeat.Once().Return(HorizontalAlignment.Center);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextVerticalAlignmentPropertyName, VerticalAlignment.Center)).Repeat.Once().Return(verticalAlignment);

            ExtendedTextAlignmentViewModel viewModel = new ExtendedTextAlignmentViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsVerticalTopAligned, Is.EqualTo(expectedIsVerticalTop));
            Assert.That(viewModel.IsVerticalCenterAligned, Is.EqualTo(expectedIsVerticalCenter));
            Assert.That(viewModel.IsVerticalBottomAligned, Is.EqualTo(expectedIsVerticalBottom));
            Assert.That(viewModel.IsVerticalStretchAligned, Is.EqualTo(expectedIsVerticalStretch));
        }

        internal class ExtendedTextAlignmentViewModel : TextAlignmentViewModel
        {
            public ExtendedTextAlignmentViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }
        }

    }
}
