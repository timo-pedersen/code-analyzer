using System.Windows.Forms;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Logic
{
    [TestFixture]
    public class AlphaNumericLogicTest
    {
        private IAlphaNumericGUI m_AlphaNumericGui;
        private AlphaNumericLogic m_AlphaNumericLogic;
        private SelectSwedishTestingCulture m_SwedishCulture;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            m_SwedishCulture = new SelectSwedishTestingCulture();
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            m_SwedishCulture.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_AlphaNumericGui = Substitute.For<IAlphaNumericGUI>();
            m_AlphaNumericGui.Prefix.Returns(string.Empty);
            m_AlphaNumericGui.Suffix.Returns(string.Empty);
            m_AlphaNumericGui.DataItem.Returns(x => null);

            m_AlphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void CreateAlphaNumericUsingCtorWithParams()
        {
            var alphaNumericGUI = Substitute.For<IAlphaNumericGUI>();
            var alphaNumericLogic = new AlphaNumericLogic(alphaNumericGUI);

            Assert.IsNotNull(alphaNumericLogic);
        }

        [Test]
        public void StartEditWithFocus()
        {
            AlphaNumericLogic alphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);

            m_AlphaNumericGui.Text.Returns("123");
            m_AlphaNumericGui.IsFocused().Returns(true);
            m_AlphaNumericGui.GetTypeOfValueSource().Returns(typeof(uint));

            alphaNumericLogic.StartEdit();
            Assert.IsTrue(alphaNumericLogic.Editing);
            m_AlphaNumericGui.Received(1).IsFocused();
            m_AlphaNumericGui.Received(1).SelectAll();
            m_AlphaNumericGui.Received(1).GetTypeOfValueSource();
        }

        [Test]
        public void StartEditWithoutFocus()
        {
            AlphaNumericLogic alphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            alphaNumericLogic.Value = new VariantValue(10);

            m_AlphaNumericGui.IsFocused().Returns(false);
            m_AlphaNumericGui.GetTypeOfValueSource().Returns(typeof(uint));

            alphaNumericLogic.StartEdit();
            
            //start edit does no longer set text, trather text is set on value change
            alphaNumericLogic.EndEdit();

            alphaNumericLogic.StartEdit();
            Assert.IsTrue(alphaNumericLogic.Editing);
            m_AlphaNumericGui.Received(1).IsFocused();
            m_AlphaNumericGui.Received(1).Focus();
            m_AlphaNumericGui.Received(1).SelectAll();
            m_AlphaNumericGui.Received(1).GetTypeOfValueSource();
        }

        [Test]
        public void EndEditWhileEditingWhileFocusRemains()
        {
            var alphaNumericGUI = Substitute.For<IAlphaNumericGUI>();
            var alphaNumericLogic = new AlphaNumericLogic(alphaNumericGUI);

            alphaNumericLogic.Editing = true;

            alphaNumericLogic.EndEdit();

            Assert.IsFalse(alphaNumericLogic.Editing);
        }
        
        [Test]
        public void EnterKeyHandled()
        {
            var alphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);

            Assert.IsTrue(alphaNumericLogic.KeyHandled((int)Keys.Enter));
        }

        [Test]
        public void EscapeKeyHandled()
        {
            var alphaNumericGui = Substitute.For<IAlphaNumericGUI>();
            alphaNumericGui.Text.Returns("123");
            alphaNumericGui.Prefix.Returns(string.Empty);
            alphaNumericGui.Suffix.Returns(string.Empty);
            
            var alphaNumericLogic = new AlphaNumericLogic(alphaNumericGui);
            alphaNumericGui.IsFocused().Returns(true);
            m_AlphaNumericGui.GetTypeOfValueSource().Returns(typeof(uint));
            alphaNumericLogic.StartEdit();

            Assert.IsTrue(alphaNumericLogic.KeyHandled((int)Keys.Escape));
            alphaNumericGui.Received(1).IsFocused();
            alphaNumericGui.Received(1).SelectAll();
            m_AlphaNumericGui.Received(1).GetTypeOfValueSource();
        }

        [Test]
        public void BackKeyNotHandled()
        {
            var alphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);

            Assert.IsFalse(alphaNumericLogic.KeyHandled((int)Keys.Back));
        }

        [Test]
        public void TabKeyNotHandled()
        {
            var alphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);

            Assert.IsFalse(alphaNumericLogic.KeyHandled((int)Keys.Tab));
        }

        #region String

        [Test]
        [TestCase(10, "10")]
        [TestCase(0, "0")]
        [TestCase(-10, "-10")]
        public void TestThatStringFormatsInteger(int value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(10.99, "10,99")]
        [TestCase(-10.99, "-10,99")]
        public void TestThatStringFormatsDecimal(decimal value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void TestThatStringFormatsBooleanTrue(bool value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text.ToLower());
        }

        #endregion

        #region Integer

        [Test]
        [TestCase(10, "10")]
        [TestCase(0, "0")]
        [TestCase(-10, "-10")]
        public void TestThatIntegerFormatsInteger(int value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(10.99, "11")]
        [TestCase(10.499, "10")]
        [TestCase(-10.499, "-10")]
        [TestCase(-10.99, "-11")]
        public void TestThatIntegerFormatsDecimal(decimal value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatIntegerFormatsString()
        {
            m_AlphaNumericLogic.Value = "10";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;

            Assert.AreEqual("10", m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase("10,99", "11")]
        [TestCase("10,49", "10")]
        [TestCase("-10,49", "-10")]
        [TestCase("-10,49", "-10")]
        public void TestThatIntegerFormatsStringWithDecimals(string value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(true, "1")]
        [TestCase(false, "0")]
        public void TestThatIntegerFormatsBoolean(bool value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatOnlyIntegersAreAccepted()
        {
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D9));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad9));

            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.A));
            //Integer format allows decimal
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.Decimal));
        }

        #endregion

        #region Decimal

        [Test]
        [TestCase(10, 0, "10")]
        [TestCase(10, 2, "10,00")]
        [TestCase(0, 2, "0,00")]
        [TestCase(-10, 1, "-10,0")]
        public void TestThatDecimalFormatsInteger(int value, int numberOfDecimals, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.NumberOfDecimals = numberOfDecimals;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalFormatsIntegerTwoDecimals2()
        {
            m_AlphaNumericGui.When(x => x.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                    {
                        m_AlphaNumericLogic.Value = value;
                    });

            m_AlphaNumericGui.FireOnValueChange(Arg.Any<object>());
            m_AlphaNumericGui.GetTypeOfValueSource().Returns(typeof(string));

            m_AlphaNumericLogic.NumberOfDecimals = 2;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            m_AlphaNumericLogic.Editing = true;
            m_AlphaNumericGui.Text.Returns("10.1");
            Assert.IsTrue(m_AlphaNumericLogic.EndEdit());

            m_AlphaNumericLogic.Editing = true;
            m_AlphaNumericGui.Text.Returns("9");
            Assert.IsTrue(m_AlphaNumericLogic.EndEdit());

            m_AlphaNumericLogic.Editing = true;
            m_AlphaNumericGui.Text.Returns("10.5");
            Assert.IsTrue(m_AlphaNumericLogic.EndEdit());

            Assert.AreEqual("10,50", m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(10.1, 1, "10,1")]
        [TestCase(10.1499, 1, "10,1")]
        [TestCase(10.159, 1, "10,2")]
        [TestCase(10.159, 2, "10,16")]
        [TestCase(10.123456789, 8, "10,12345679")]
        public void TestThatDecimalFormatsDecimal(decimal value, int numberOfDecimals, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.NumberOfDecimals = numberOfDecimals;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase("10,1", 1, "10,1")]
        [TestCase("10,1", 2, "10,10")]
        [TestCase("10,15", 1, "10,2")]
        [TestCase("10,149", 1, "10,1")]
        public void TestThatDecimalFormatsString(string value, int numberOfDecimals, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.NumberOfDecimals = numberOfDecimals;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        [TestCase(true, "1,0")]
        [TestCase(false, "0,0")]
        public void TestThatDecimalFormatsBool(bool value, string expectedFormattedValue)
        {
            m_AlphaNumericLogic.Value = value;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual(expectedFormattedValue, m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalsAreAccepted()
        {
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.Decimal));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D9));

            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.A));
        }

        [Test]
        public void TestThatDecimalFormatsTwoDecimalsDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_AlphaNumericLogic.Value = "#";
            m_AlphaNumericLogic.NumberOfDecimals = 2;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual("#,##", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalFormatsLessDecimalsDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_AlphaNumericLogic.Value = "#,####";
            m_AlphaNumericLogic.NumberOfDecimals = 2;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual("#,##", m_AlphaNumericLogic.Text);
        }

        #endregion

        #region Hex

        [Test]
        public void TestThatHexFormatsInteger()
        {
            m_AlphaNumericLogic.Value = 27;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Hex;

            Assert.AreEqual("1B", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatHexFormatsDecimal()
        {
            m_AlphaNumericLogic.Value = 26.9;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Hex;

            Assert.AreEqual("1B", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatHexFormatsAnotherString()
        {
            m_AlphaNumericLogic.Value = "10";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Hex;

            Assert.AreEqual("A", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatHexFormatsBool()
        {
            m_AlphaNumericLogic.Value = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Hex;

            Assert.AreEqual("1", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatOnlyHexCharsAreAllowed()
        {
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Hex;

            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.A));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.B));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.C));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.E));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.F));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D9));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad9));

            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.G));
            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.Enter));
            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.T));
        }

        #endregion

        #region Binary

        [Test]
        public void TestThatBinaryFormatsInteger()
        {
            m_AlphaNumericLogic.Value = 27;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.AreEqual("11011", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatBinaryFormatsDecimal()
        {
            m_AlphaNumericLogic.Value = 26.9;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.AreEqual("11011", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatBinaryFormatsString()
        {
            m_AlphaNumericLogic.Value = "26,9";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.AreEqual("11011", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatBinaryFormatsBool()
        {
            m_AlphaNumericLogic.Value = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.AreEqual("1", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatOnlyBinaryCharsAreAllowed()
        {
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.D1));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad0));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.NumPad1));

            Assert.IsTrue(m_AlphaNumericLogic.KeyHandled((int)Keys.A));
            Assert.IsFalse(m_AlphaNumericLogic.KeyHandled((int)Keys.Decimal));
        }

        #endregion

        #region Display number of characters

        [Test]
        public void TestThatStringIsNotTruncatedWhenNumberOfCharactersLimitIsHigh()
        {
            m_AlphaNumericLogic.Value = "1";
            m_AlphaNumericLogic.MaxNumberOfCharacters = 4;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("1", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatStringIsNotTruncatedWhenNumberOfCharactersLimitIsEqualToStringLength()
        {
            m_AlphaNumericLogic.Value = "1234";
            m_AlphaNumericLogic.MaxNumberOfCharacters = 4;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("1234", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatStringIsTruncatedWhenNumberOfCharactersLimitIsLow()
        {
            m_AlphaNumericLogic.Value = "12345";
            m_AlphaNumericLogic.MaxNumberOfCharacters = 4;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("####", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDoubleIsTruncatedWhenNumberOfCharactersLimitIsLow()
        {
            m_AlphaNumericLogic.Value = 123.45;
            m_AlphaNumericLogic.NumberOfDecimals = 2;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual("123,4", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDoubleIsTruncatedAndDecimalSeparatorIsRemoved()
        {
            m_AlphaNumericLogic.Value = 1234.5;
            m_AlphaNumericLogic.NumberOfDecimals = 2;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;

            Assert.AreEqual("1234", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatTooLongDoubleValueGeneratesHashString()
        {
            m_AlphaNumericLogic.Value = 123.45;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("##", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDoubleValueIsTruncatedWithThreeCharacters()
        {
            m_AlphaNumericLogic.Value = 123.45;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 3;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("123", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDoubleValueIsTruncatedWithFourCharacters()
        {
            m_AlphaNumericLogic.Value = 123.45;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 4;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("123", m_AlphaNumericLogic.Text);
        }


        [Test]
        public void TestThatDoubleValueIsTruncatedWithFiveCharacters()
        {
            m_AlphaNumericLogic.Value = 123.45;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("123,4", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatTooLongBinaryValueShowsHash()
        {
            m_AlphaNumericLogic.Value = 16;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 4;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Binary;

            Assert.AreEqual("####", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatTooLongBoolValueShowsHash()
        {
            m_AlphaNumericLogic.Value = true;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 3;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.String;

            Assert.AreEqual("###", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatPressingAKeyIsOkWhenLimitIsNotReached()
        {
            m_AlphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            m_AlphaNumericGui.SelectedText.Returns(string.Empty);

            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.Value = 1;

            bool result = m_AlphaNumericLogic.KeyHandled((int)Keys.D1);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestThatPressingAKeyIsSetAsHandledWhenLimitIsReached()
        {
            m_AlphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            m_AlphaNumericGui.Text = "1";
            m_AlphaNumericGui.SelectedText.Returns(string.Empty);

            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.Value = 1;

            m_AlphaNumericLogic.KeyHandled((int)Keys.D1);
            m_AlphaNumericGui.Text = "11";

            bool result = m_AlphaNumericLogic.KeyHandled((int)Keys.D1);
            Assert.IsTrue(result);
        }


        [Test]
        public void TestThatPressingANonNumericKeyIsSetNotSetAsHandledWhenLimitIsReached()
        {
            m_AlphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            m_AlphaNumericGui.Text = "1";
            m_AlphaNumericGui.SelectedText.Returns(string.Empty);

            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.Value = 1;

            m_AlphaNumericLogic.KeyHandled((int)Keys.D1);

            bool result = m_AlphaNumericLogic.KeyHandled((int)Keys.Tab);
            Assert.IsFalse(result);
        }

        [Test]
        public void TestThatPressingAKeyIsNotSetAsHandledWhenLimitIsReachedButTextIsSelected()
        {
            m_AlphaNumericLogic = new AlphaNumericLogic(m_AlphaNumericGui);
            m_AlphaNumericGui.Text = "11";

            m_AlphaNumericGui.SelectedText.Returns("11");

            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;
            m_AlphaNumericLogic.Value = 11;

            bool result = m_AlphaNumericLogic.KeyHandled((int)Keys.D1);

            Assert.IsFalse(result);
        }

        [Test]
        public void TestThatDecimalValueIsPaddedWithNumberSignDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_AlphaNumericLogic.Value = "#,#";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("###,#", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalsValueIsPaddedWithNumberSignDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_AlphaNumericLogic.Value = "#,#";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Decimal;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 7;
            m_AlphaNumericLogic.NumberOfDecimals = 3;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("###,###", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatIntegerValueIsPaddedWithNumberSignDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_AlphaNumericLogic.Value = "#";
            m_AlphaNumericLogic.DisplayFormat = AnalogNumericDisplayFormat.Integer;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("#####", m_AlphaNumericLogic.Text);
        }
        
        #endregion

        #region Zero fill

        [Test]
        public void TestThatDecimalValueIsPaddedWithZeroes()
        {
            m_AlphaNumericLogic.Value = 12.3;
            m_AlphaNumericLogic.ZeroFill = true;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 5;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("012,3", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalValueIsPaddedWithZeroesWithoutDecimalSeparator()
        {
            m_AlphaNumericLogic.Value = 12.3;
            m_AlphaNumericLogic.ZeroFill = true;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 3;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("012", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatDecimalValueIsNotPaddedWithZeroes()
        {
            m_AlphaNumericLogic.Value = 12.3;
            m_AlphaNumericLogic.ZeroFill = true;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 2;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("12", m_AlphaNumericLogic.Text);
        }

        [Test]
        public void TestThatNegativeValuesArePadded()
        {
            m_AlphaNumericLogic.Value = -12.3;
            m_AlphaNumericLogic.ZeroFill = true;
            m_AlphaNumericLogic.MaxNumberOfCharacters = 3;
            m_AlphaNumericLogic.LimitNumberOfCharacters = true;

            Assert.AreEqual("-012", m_AlphaNumericLogic.Text);
        }

        #endregion

        #region Prefix/suffix

        [Test]
        public void TestThatSuffixIsAdded()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.Suffix = " kr";

            var logic = new AlphaNumericLogic(gui);
            logic.Value = 123;
            
            Assert.AreEqual("123 kr", logic.Text);
        }

        [Test]
        public void TestThatPrefixIsAdded()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.Prefix = "$";

            var logic = new AlphaNumericLogic(gui);
            logic.Value = 123;
            
            Assert.AreEqual("$123", logic.Text);
        }

        [Test]
        public void TestThatPrefixAndSuffixIsAdded()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.Prefix = "Pris: ";
            gui.Suffix = " kr";

            var logic = new AlphaNumericLogic(gui);
            logic.Value = 123;

            Assert.AreEqual("Pris: 123 kr", logic.Text);
        }

        [Test]
        public void TestThatTruncationWorksWithSuffix()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.Suffix = " kr";

            var logic = new AlphaNumericLogic(gui);
            
            logic.Value = 123.45;
            logic.MaxNumberOfCharacters = 3;
            logic.LimitNumberOfCharacters = true;

            Assert.AreEqual("123 kr", logic.Text);
        }

        #endregion

        #region Value Range

        [Test]
        public void AllowedValueCanBeSetWhenValidateOnInputIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnInput = true;
            logic.Editing = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value[0];
                });

            gui.Text = "85";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(85), logic.Value);
        }

        [Test]
        public void ForbiddenValueCantBeSetWhenValidateOnInputIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnInput = true;
            logic.Editing = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value;
                });

            gui.Text = "110";
            logic.EndEdit();
            Assert.AreNotEqual("110", logic.Value);
        }

        [Test]
        public void ValueOutOfRangeLowIsSetForForbiddenValueWhenValidateOnDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.Editing = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value[0];
                });

            gui.Text = "-11";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(-11), logic.Value);
            Assert.AreEqual(false, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(true, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ValueOutOfRangeHighIsSetForForbiddenValueWhenValidateOnDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.Editing = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value[0];
                });

            gui.Text = "101";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(101), logic.Value);
            Assert.AreEqual(true, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(false, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ValueOutOfRangeIsNotSetForAllowedValueWhenValidateOnDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.Editing = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value[0];
                });

            gui.Text = "85";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(85), logic.Value);
            Assert.AreEqual(false, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(false, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ForbiddenValueCantBeSetWhenValidateOnInputAndDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.ValidateValueOnInput = true;
            logic.Editing = true;
            gui.Text = "110";
            logic.EndEdit();
            Assert.AreNotEqual(110, logic.Value);
        }

        [Test]
        public void ValueIsOutOfRangeHighIfForbiddenValueIsSetFromOutsideWhenValidateOnInputAndDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.ValidateValueOnInput = true;
            logic.Value = 110;
            Assert.AreEqual(new VariantValue(110), logic.Value);
            Assert.AreEqual(true, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(false, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ValueIsOutOfRangeLowIfForbiddenValueIsSetFromOutsideWhenValidateOnInputAndDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = 50;
            gui.ValidateValueOnDisplay = true;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.ValidateValueOnInput = true;

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value;
                });

            logic.Value = -25;
            Assert.AreEqual(new VariantValue(-25), logic.Value);
            Assert.AreEqual(false, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(true, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ValueIsNotOutOfRangeIfAllowedValueIsSetFromOutsideWhenValidateOnInputAndDisplayIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 100;
            gui.MinValue = -50;
            gui.ValidateValueOnDisplay = true;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.ValidateValueOnDisplay = true;
            logic.ValidateValueOnInput = true;
            logic.Value = -25;
            Assert.AreEqual(new VariantValue(-25), logic.Value);
            Assert.AreEqual(false, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(false, gui.ValueOutOfRangeLow);
        }

        [Test]
        public void ZeroCanBeSetWhenBothMaxAndMinIsZeroWhenValidateOnInputIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 0;
            gui.MinValue = 0;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui) { Value = 35, ValidateValueOnInput = true, Editing = true };

            gui
                .When(mock => mock.FireOnValueChange(Arg.Any<object>()))
                .Do(value =>
                {
                    logic.Value = value[0];
                });

            gui.Text = "0";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(0), logic.Value);
            Assert.AreEqual(false, gui.ValueOutOfRangeHigh);
            Assert.AreEqual(false, gui.ValueOutOfRangeLow);
        }

        //No longer valid, non numeric value can be set on control, but will be reset since it otherwise will kill the panel application. 
        //A warning message will be displayed for user and value will be reset in all cases
        [Test]
        public void NonZeroValueCantBeSetWhenBothMaxAndMinIsZeroWhenValidateOnInputIsSet()
        {
            var gui = Substitute.For<IAlphaNumericGUI>();
            gui.MaxValue = 0;
            gui.MinValue = 0;
            gui.ValidateValueOnInput = true;

            var logic = new AlphaNumericLogic(gui);
            logic.Value = 35;
            logic.ValidateValueOnInput = true;
            logic.Editing = true;
            gui.Text = "2";
            logic.EndEdit();
            Assert.AreEqual(new VariantValue(35), logic.Value);
        }
        #endregion

        #region ValueChanged

        [Test]
        public void SettingValueWithAVariantValueWhenNewValueIsAnIntegerWithSameValueWillNotFireValueChanged()
        {
            m_AlphaNumericLogic.Value = 10;

            bool wasRaised = false;
            m_AlphaNumericLogic.ValueChanged += (sender, args) => wasRaised = true;

            m_AlphaNumericLogic.Value = new VariantValue(10);

            Assert.IsFalse(wasRaised);
        }

        #endregion
    }
}
