#if!VNEXT_TARGET
using System.Reflection;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Validators
{
    [TestFixture]
    public class TextBoxValidatorTest
    {
        private TextBoxValidator m_Validator;

        [SetUp]
        public void SetUp()
        {
            m_Validator = new TextBoxValidator();
        }

        [Test]
        public void ValidatingDoubleSucceeds()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("12", "SomeDouble", m_Validator);

            AssertThatTextBoxValidationSucceeded(txtBox);
        }

        [Test]
        public void ValidatingDoubleFailsOnInvalidInput()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("a", "SomeDouble", m_Validator);

            AssertThatTextBoxValidationFailed(txtBox);
        }

        [Test]
        public void ValidatingIntFailsOnInvalidInput()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("a", "SomeInt", m_Validator);

            AssertThatTextBoxValidationFailed(txtBox);
        }

        [Test]
        public void ValidatingStringSucceeds()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("abc", "SomeString", m_Validator);

            AssertThatTextBoxValidationSucceeded(txtBox);
        }

        [Test]
        public void ValidatingStringUsesStringValidatorInvalidCharacters()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("ab;c", "SomeString", m_Validator);

            AssertThatTextBoxValidationFailed(txtBox);
        }

        [Test]
        public void ValidatingStringUsesStringValidatorMinLength()
        {
            TextBoxMock txtBox = GetTextBoxAndTriggerValidatingEvent("a", "SomeString", m_Validator);

            AssertThatTextBoxValidationFailed(txtBox);
        }

        [Test]
        public void ValidatorClearWorks()
        {
            TextBoxMock txtBox = new TextBoxMock();

            m_Validator.AddTextBox(txtBox, null);

            m_Validator.ClearTextBoxes();

            Assert.AreEqual(0, m_Validator.GetNrOfTextBoxes());
        }

        private static void AssertThatTextBoxValidationFailed(TextBoxMock txtBox)
        {
            Assert.IsTrue(txtBox.ValidatingCancelled);
            Assert.AreEqual(txtBox.Text.Length, txtBox.SelectionLength);
        }

        private static void AssertThatTextBoxValidationSucceeded(TextBoxMock txtBox)
        {
            Assert.IsFalse(txtBox.ValidatingCancelled);
        }

        private static TextBoxMock GetTextBoxAndTriggerValidatingEvent(string textBoxText, string propertyName, TextBoxValidator validator)
        {
            TextBoxMock textBox = new TextBoxMock();
            textBox.Text = textBoxText;

            PropertyInfo property = typeof(IObjectToValidate).GetProperty(propertyName);

            validator.AddTextBox(textBox, property);
            textBox.ValidatingCancelled = !validator.ValidateTextbox(textBox);

            //textBox.TriggerValidatingEvent();
            return textBox;
        }
    }

}
#endif
