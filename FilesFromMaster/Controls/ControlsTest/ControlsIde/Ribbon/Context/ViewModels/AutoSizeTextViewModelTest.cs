using System.ComponentModel.Design;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Controls.RibbonContent;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

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
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
        }

        private void SetupDesignerStubs()
        {
            ScreenDesignerViewStub = MockRepository.GenerateStub<IScreenDesignerView>();
            INeoDesignerHost designerHost = MockRepository.GenerateStub<INeoDesignerHost>();
            designerHost.Stub(x => x.GetService<IScreenDesignerView>()).Return(ScreenDesignerViewStub);

            DesignerEventServiceStub = MockRepository.GenerateStub<IDesignerEventService>();
            DesignerEventServiceStub.Stub(x => x.ActiveDesigner).Return(designerHost);

        }

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(TextPropertyName, string.Empty)).Repeat.Once().Return(TestText);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(MultiLinePropertyName, false)).Repeat.Once().Return(true);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(WordWrapPropertyName, false)).Repeat.Once().Return(true);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoStretchFontPropertyName, false)).Repeat.Once().Return(true);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoSizePropertyName, false)).Repeat.Once().Return(true);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();
            autoSizeTextViewModel.ExecuteUpdateContent();

            Assert.That(autoSizeTextViewModel.AutoSize, Is.EqualTo(true));
            Assert.That(autoSizeTextViewModel.AutoStretchFont, Is.EqualTo(true));
        }

        [Test]
        public void EnablingAutoSizeDisablesAutoStretchFont()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoSizePropertyName, false)).Repeat.Once().Return(true);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoStretchFontPropertyName, false)).Repeat.Once().Return(false);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();

            Assert.That(autoSizeTextViewModel.IsAutoStretchFontEnabled, Is.EqualTo(false));
            Assert.That(autoSizeTextViewModel.IsMultiLineEnabled, Is.EqualTo(true));
        }

        [Test]
        public void EnablingAutoStretchFontDisablesMultiLineAndWordWrap()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoSizePropertyName, false)).Repeat.Once().Return(false);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoStretchFontPropertyName, false)).Repeat.Twice().Return(true);

            ExtendedAutoSizeTextBaseViewModel autoSizeTextViewModel = new ExtendedAutoSizeTextBaseViewModel();

            Assert.That(autoSizeTextViewModel.IsMultiLineEnabled, Is.EqualTo(false));
            Assert.That(autoSizeTextViewModel.IsWordWrapEnabled, Is.EqualTo(false));
        }

        [Test]
        public void DisablingAutoStretchFontAndAutoSizeEnablesWordWrapAndMultiLine()
        {
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoSizePropertyName, false)).Repeat.Once().Return(false);
            GlobalCommandServiceStub.Stub(x => x.GetProperty(AutoStretchFontPropertyName, false)).Repeat.Twice().Return(false);

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