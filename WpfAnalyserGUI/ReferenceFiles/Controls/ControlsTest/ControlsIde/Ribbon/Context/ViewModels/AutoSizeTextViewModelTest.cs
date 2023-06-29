using System.ComponentModel.Design;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Controls.RibbonContent;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class AutoSizeTextViewModelTest : RibbonViewModelTestBase
    {
        private const string TestText = "the text";

        private string AutoStretchFontPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Label.AutoStretchFontProperty.Name; }
        }

        private string AutoSizePropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Label.AutoSizeProperty.Name; }
        }

        private string MultiLinePropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Label.MultiLineProperty.Name; }
        }

        private string WordWrapPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Label.WordWrapProperty.Name; }
        }

        private string TextPropertyName
        {
            get { return Neo.ApplicationFramework.Controls.Label.TextProperty.Name; }
        }

        private IDesignerEventService DesignerEventServiceStub { get; set; }
        private IScreenDesignerView ScreenDesignerViewStub { get; set; }

        [SetUp]
        protected override void Setup()
        {
            base.Setup();
            AddServices();
        }

        private void AddServices()
        {
            SetupDesignerStubs();
            TestHelper.AddService(DesignerEventServiceStub);
            TestHelper.AddService(ScreenDesignerViewStub);
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
        }

        private void SetupDesignerStubs()
        {
            ScreenDesignerViewStub = Substitute.For<IScreenDesignerView>();
            INeoDesignerHost designerHost = Substitute.For<INeoDesignerHost>();
            designerHost.GetService<IScreenDesignerView>().Returns(ScreenDesignerViewStub);

            DesignerEventServiceStub = Substitute.For<IDesignerEventService>();
            DesignerEventServiceStub.ActiveDesigner.Returns(designerHost);

        }

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            GlobalCommandServiceStub.GetProperty(TextPropertyName, string.Empty).Returns(TestText);
            GlobalCommandServiceStub.GetProperty(MultiLinePropertyName, false).Returns(true);
            GlobalCommandServiceStub.GetProperty(WordWrapPropertyName, false).Returns(true);
            GlobalCommandServiceStub.GetProperty(AutoStretchFontPropertyName, false).Returns(true);
            GlobalCommandServiceStub.GetProperty(AutoSizePropertyName, false).Returns(true);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();
            autoSizeTextViewModel.ExecuteUpdateContent();

            Assert.That(autoSizeTextViewModel.AutoSize, Is.EqualTo(true));
            Assert.That(autoSizeTextViewModel.AutoStretchFont, Is.EqualTo(true));
        }

        [Test]
        public void EnablingAutoSizeDisablesAutoStretchFont()
        {
            GlobalCommandServiceStub.GetProperty(AutoSizePropertyName, false).Returns(true);
            GlobalCommandServiceStub.GetProperty(AutoStretchFontPropertyName, false).Returns(false);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();

            Assert.That(autoSizeTextViewModel.IsAutoStretchFontEnabled, Is.EqualTo(false));
            Assert.That(autoSizeTextViewModel.IsMultiLineEnabled, Is.EqualTo(true));
        }

        [Test]
        public void EnablingAutoStretchFontDisablesMultiLineAndWordWrap()
        {
            GlobalCommandServiceStub.GetProperty(AutoSizePropertyName, false).Returns(false);
            GlobalCommandServiceStub.GetProperty(AutoStretchFontPropertyName, false).Returns(true);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();

            Assert.That(autoSizeTextViewModel.IsMultiLineEnabled, Is.EqualTo(false));
            Assert.That(autoSizeTextViewModel.IsWordWrapEnabled, Is.EqualTo(false));
        }

        [Test]
        public void DisablingAutoStretchFontAndAutoSizeEnablesWordWrapAndMultiLine()
        {
            GlobalCommandServiceStub.GetProperty(AutoSizePropertyName, false).Returns(false);
            GlobalCommandServiceStub.GetProperty(AutoStretchFontPropertyName, false).Returns(false);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();

            Assert.That(autoSizeTextViewModel.IsMultiLineEnabled, Is.EqualTo(true));
            Assert.That(autoSizeTextViewModel.IsWordWrapEnabled, Is.EqualTo(true));
        }

        internal class ExtendedAutoSizeTextBaseViewModel : AutoSizeTextViewModel
        {
            public ExtendedAutoSizeTextBaseViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }

            protected override MultiLabelEditorControl CreateMultiLabelEditorControl()
            {
                return null;
            }
        }
    }
}