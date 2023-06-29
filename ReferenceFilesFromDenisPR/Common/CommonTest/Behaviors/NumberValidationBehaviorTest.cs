using System;
using System.Globalization;
using System.Windows.Controls;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.TestHelpers;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Behaviors
{
    [TestFixture]
    public class NumberValidationBehaviorTest
    {
        private ITerminal m_Terminal;
        private ITargetService m_TargetService;
        private ITargetInfo m_TargetInfo;
        private ExtendedNumberValidationBehavior m_NumberValidationBehavior;
        private TextBox m_TextBox;

        [SetUp]
        public void RunBeforeEachTest()
        {
            TestHelper.ClearServices();
            m_TargetInfo = Substitute.For<ITargetInfo>();
            m_Terminal = Substitute.For<ITerminal>();
            m_TargetInfo.TerminalDescription = m_Terminal;
            m_TargetService = Substitute.For<ITargetService>();
            m_TargetService.CurrentTargetInfo.Returns(m_TargetInfo);

            TestHelper.AddService(typeof(ITargetService), m_TargetService);
            TestHelper.AddService(typeof(ITerminalTargetChangeService), Substitute.For<ITerminalTargetChangeService>());

            m_NumberValidationBehavior = new ExtendedNumberValidationBehavior();
            m_TextBox = new TextBox();
            m_NumberValidationBehavior.Attach(m_TextBox);
        }

        [TearDown]
        public void AfterEachTest()
        {
            TestHelper.ClearServices();
            m_TargetInfo.TerminalDescription = null;
        }

        private void StubChangeInIntValue(int valueBeforeUpdate)
        {
            m_NumberValidationBehavior.InputType = typeof(int);
            m_NumberValidationBehavior.Value = valueBeforeUpdate;
        }

        private void StubChangeInDoubleValue(double valueBeforeUpdate)
        {
            m_NumberValidationBehavior.InputType = typeof(double);
            m_NumberValidationBehavior.Value = valueBeforeUpdate;
        }

        [Test]
        public void DoesNotUpdateValueWhenValueIsBreakingTheMinValueLimit()
        {
            int valueBeforeUpdate = 10;
            int minValueLimit = 0;
            int valueThatBreaksTheMinValueLimit = -1;
            StubChangeInIntValue(valueBeforeUpdate);
            m_NumberValidationBehavior.MinValue = minValueLimit;

            m_TextBox.Text = valueThatBreaksTheMinValueLimit.ToString();
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueBeforeUpdate.ToString()));
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void DoesNotUpdateValueIfItsLargerOrSmallerThenInt32MaxAndMinValues(int rangeValue)
        {
            int valueBeforeUpdate = 10;
            string valueThatBreaksTheLimit = String.Format("{0}1", rangeValue);
            StubChangeInIntValue(valueBeforeUpdate);

            m_TextBox.Text = valueThatBreaksTheLimit;
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueBeforeUpdate.ToString()));
        }

        [Test]
        public void UpdatesValueWhenValueIsntBreakingTheMinValueLimit()
        {
            int valueBeforeUpdate = 10;
            int minValueLimit = 0;
            int valueThatDoenstBreakTheMinValueLimit = 1;
            StubChangeInIntValue(valueBeforeUpdate);
            m_NumberValidationBehavior.MinValue = minValueLimit;

            m_TextBox.Text = valueThatDoenstBreakTheMinValueLimit.ToString();
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueThatDoenstBreakTheMinValueLimit.ToString()));
        }

        [Test]
        public void DoesNotUpdateValueWhenValueIsBreakingTheMaxValueLimit()
        {
            int valueBeforeUpdate = 5;
            int maxValueLimit = 10;
            int valueThatBreaksTheMaxValueLimit = 11;
            StubChangeInIntValue(valueBeforeUpdate);
            m_NumberValidationBehavior.MaxValue = maxValueLimit;

            m_TextBox.Text = valueThatBreaksTheMaxValueLimit.ToString();
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueBeforeUpdate.ToString()));
        }

        [Test]
        public void UpdatesValueWhenValueIsntBreakingTheMaxValueLimit()
        {
            int valueBeforeUpdate = 5;
            int maxValueLimit = 10;
            int valueThatDoesntBreakTheMaxValueLimit = 9;
            StubChangeInIntValue(valueBeforeUpdate);
            m_NumberValidationBehavior.MaxValue = maxValueLimit;

            m_TextBox.Text = valueThatDoesntBreakTheMaxValueLimit.ToString();
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueThatDoesntBreakTheMaxValueLimit.ToString()));
        }

        [Test]
        public void DoesNotUpdateValueWhenTextInTextBoxIsntANumber()
        {
            int valueBeforeUpdate = 5;
            StubChangeInIntValue(valueBeforeUpdate);

            m_TextBox.Text = "Chuck Norris is not a number!";
            TriggerValidationOfBehavior();

            Assert.That(m_TextBox.Text, Is.EqualTo(valueBeforeUpdate.ToString()));
        }

        [Test]
        public void AllowDecimalsOnCEDefaultsToTrueIfNotSet()
        {
            Assert.That(m_NumberValidationBehavior.AllowDecimalsOnCE == true);
        }

        [Test]
        [TestCase("en-US", "1,123", false)]
        [TestCase("sv", "1,123", true)]
        [TestCase("en-US", "1.123", true)]
        [TestCase("sv", "1.123", false)]
        public void DoesNotUpdateValueWhenWrongDecimalDelimiterIsUsedForTheCurrentCulture(string cultureInfoString, string value, bool expectedReturnBool)
        {
            double returnValue;
            CultureInfo cultureInfo = new CultureInfo(cultureInfoString);
            m_TextBox.Text = value;

            Assert.That(m_NumberValidationBehavior.InternalIsTextValidNumber(cultureInfo, out returnValue) == expectedReturnBool);
        }

        private void TriggerValidationOfBehavior()
        {
            m_TextBox.TriggerLostKeyBoardFocus();
        }
    }

    internal class ExtendedNumberValidationBehavior : NumberValidationBehavior
    {
        public ExtendedNumberValidationBehavior()
        {
        }

        public new string FormatInput(object valueToFormat)
        {
            return base.FormatInput(valueToFormat);
        }

        public bool InternalIsTextValidNumber(CultureInfo currentCultureInfo, out double value)
        {
            return IsTextValidNumber(currentCultureInfo, out value);
        }


        protected override bool IsTextValidNumber(CultureInfo currentCultureInfo, out double value)
        {
            return base.IsTextValidNumber(currentCultureInfo, out value);
        }

        public new object FormatValue(double value)
        {
            return base.FormatValue(value);
        }
    }
}
