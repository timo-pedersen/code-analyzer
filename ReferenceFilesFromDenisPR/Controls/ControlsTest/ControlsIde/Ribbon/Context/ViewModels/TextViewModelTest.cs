using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class TextViewModelTest : RibbonViewModelTestBase
    {
        private const string TestText = "the text";

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

        [Test]
        public void UpdateContentSetsTheProperties()
        {
            GlobalCommandServiceStub.GetProperty(TextPropertyName, string.Empty).Returns(TestText);
            GlobalCommandServiceStub.GetProperty(MultiLinePropertyName, false).Returns(true);
            GlobalCommandServiceStub.GetProperty(WordWrapPropertyName, false).Returns(true);

            ExtendedTextBaseViewModel viewModel = new ExtendedTextBaseViewModel();
            viewModel.ExecuteUpdateContent();

            Assert.That(viewModel.Text, Is.EqualTo(TestText));
            Assert.That(viewModel.MultiLine, Is.EqualTo(true));
            Assert.That(viewModel.WordWrap, Is.EqualTo(true));
        }

        [Test]
        public void SettingTheTextUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            string expectedValue = TestText;

            new TextViewModel { Text = expectedValue };

            AssertSetPropertyInCommandServiceWasCalled(TextPropertyName, expectedValue, CommandTextsIde.Text);
        }

        [Test]
        public void SettingMultiLineUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            bool expectedValue = true;

            new TextViewModel { MultiLine = expectedValue };

            AssertSetPropertyInCommandServiceWasCalled(MultiLinePropertyName, expectedValue, CommandTextsIde.TextMultiLine);
        }

        [Test]
        public void SettingWordWrapUpdatesThePropertyOfTheSelectedObjectAndIncludesUndoInformation()
        {
            bool expectedValue = true;

            new TextViewModel { WordWrap = expectedValue };

            AssertSetPropertyInCommandServiceWasCalled(WordWrapPropertyName, expectedValue, CommandTextsIde.TextWordWrap);
        }

        internal class ExtendedTextBaseViewModel : TextViewModel
        {
            public ExtendedTextBaseViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }
        }
    }
}
