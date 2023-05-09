using System.Drawing;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels.Screen
{
    [TestFixture]
    public class ScreenViewModelTest : RibbonViewModelTestBase
    {
        protected string ScreenTitlePropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.TitleProperty.Name; }
        }

        protected string PopupPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.PopupScreenProperty.Name; }
        }

        protected string ModalPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.ModalScreenProperty.Name; }
        }

        protected string ScreenPositionPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.ScreenPositionProperty.Name; }
        }

        protected string ScreenSizePropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.ScreenSizeProperty.Name; }
        }

        protected string IsCacheablePropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Screen.ScreenWindow.IsCacheableProperty.Name; }
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
            GlobalCommandServiceStub.Stub(x => x.ActiveScreen).Return(null);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            SetScreenPropertiesAndUpdateContent(true, true, new Point(0, 0), new Size(800, 600), false);

            ExtendedTextBaseViewModel viewModel = new ExtendedTextBaseViewModel();
            viewModel.ExecuteUpdateContent();

            Assert.That(viewModel.Popup, Is.EqualTo(true));
            Assert.That(viewModel.Modal, Is.EqualTo(true));
            Assert.That(viewModel.XPosition, Is.EqualTo(0));
            Assert.That(viewModel.YPosition, Is.EqualTo(0));
            Assert.That(viewModel.Width, Is.EqualTo(800));
            Assert.That(viewModel.Height, Is.EqualTo(600));
            Assert.That(viewModel.IsCacheable, Is.EqualTo(false));
        }

        [Test]
        public void VerifyIsPopupEnabledWhenScreenIsPopup()
        {
            SetScreenPropertiesAndUpdateContent(true, true, new Point(0, 0), new Size(800, 600), false);

            ExtendedTextBaseViewModel viewModel = new ExtendedTextBaseViewModel();
            viewModel.ExecuteUpdateContent();

            Assert.That(viewModel.IsPopupEnabled, Is.EqualTo(true));
        }

        [Test]
        public void VerifyCacheableSetToFalseWhenPopupEnabled()
        {
            SetScreenPropertiesAndUpdateContent(true, true, new Point(0, 0), new Size(800, 600), true);

            ExtendedTextBaseViewModel viewModel = new ExtendedTextBaseViewModel();
            viewModel.ExecuteUpdateContent();

            Assert.That(viewModel.IsCacheable, Is.EqualTo(false));
        }

        private void SetScreenPropertiesAndUpdateContent(bool popupValue, bool modalValue, Point pointValue, Size sizeValue, bool isCacheableValue)
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(IsCacheablePropertyName, true)).Return(isCacheableValue);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(PopupPropertyName, false)).Repeat.Any().Return(popupValue);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(ModalPropertyName, false)).Repeat.Once().Return(modalValue);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(ScreenPositionPropertyName, new Point(0, 0))).Repeat.Once().Return(pointValue);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(ScreenSizePropertyName, new Size(0, 0))).Repeat.Any().Return(sizeValue);
        }

        internal class ExtendedTextBaseViewModel : ScreenViewModel
        {
            public ExtendedTextBaseViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }
        }
    }
}
