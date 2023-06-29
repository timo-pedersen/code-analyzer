using System.Windows.Controls;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Behaviors.Formatters
{
    [TestFixture]
    public class NumberValidationBehaviorFormattingTest
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

        private void StubTargetingPC()
        {
            m_Terminal.IsPC.Returns(true);
        }

        private void StubTargetingNonPC()
        {
            m_Terminal.IsPC.Returns(false);
        }

        [Test]
        public void FormatsValueToIntByDefault()
        {
            int expectedValue = 12;
            object formattedValue = m_NumberValidationBehavior.FormatValue(12.43D);

            Assert.That(formattedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void FormatsTextWithoutDecimalsByDefault()
        {
            string expectedText = "12";
            string formattedText = m_NumberValidationBehavior.FormatInput(12.43D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormatsValueToDoubleIfInputTypeIsDouble()
        {
            double expectedValue = 10.199D;
            m_NumberValidationBehavior.InputType = typeof(double);
            object formattedValueWithDecimalsIntact = m_NumberValidationBehavior.FormatValue(10.199D);

            Assert.That(formattedValueWithDecimalsIntact, Is.EqualTo(expectedValue));
        }

        [Test]
        public void FormattingTextDoesNotShowDecimalIfDecimalIsZero()
        {
            string expectedText = "10";
            m_NumberValidationBehavior.InputType = typeof(double);
            string formattedText = m_NumberValidationBehavior.FormatInput(10.00D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormattingTextShowAllDecimalsByDefault()
        {
            string expectedText = 10.123.ToString();
            m_NumberValidationBehavior.InputType = typeof(double);
            string formattedText = m_NumberValidationBehavior.FormatInput(10.123D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormattingTextShowDecimalsWhenTargetingPCAndAllowDecimalsOnCEIsTrue()
        {
            StubTargetingPC();
            string expectedText = 10.123.ToString();
            m_NumberValidationBehavior.InputType = typeof(double);
            m_NumberValidationBehavior.AllowDecimalsOnCE = true;
            string formattedText = m_NumberValidationBehavior.FormatInput(10.123D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormattingTextShowDecimalsWhenTargetingPCAndAllowDecimalsOnCEIsFalse()
        {
            StubTargetingPC();
            string expectedText = 10.123.ToString();
            m_NumberValidationBehavior.InputType = typeof(double);
            m_NumberValidationBehavior.AllowDecimalsOnCE = false;
            string formattedText = m_NumberValidationBehavior.FormatInput(10.123D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormattingTextShowDecimalsWhenTargetingCEAndAllowDecimalsOnCEIsTrue()
        {
            StubTargetingNonPC();
            string expectedText = 10.123.ToString();
            m_NumberValidationBehavior.InputType = typeof(double);
            m_NumberValidationBehavior.AllowDecimalsOnCE = true;
            string formattedText = m_NumberValidationBehavior.FormatInput(10.123D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

        [Test]
        public void FormattingTextDoesNOTShowDecimalsWhenTargetingCEAndAllowDecimalsOnCEIsFalse()
        {
            StubTargetingNonPC();
            string expectedText = "10";
            m_NumberValidationBehavior.InputType = typeof(double);
            m_NumberValidationBehavior.AllowDecimalsOnCE = false;
            string formattedText = m_NumberValidationBehavior.FormatInput(10.123D);

            Assert.That(formattedText, Is.EqualTo(expectedText));
        }

    }
}
