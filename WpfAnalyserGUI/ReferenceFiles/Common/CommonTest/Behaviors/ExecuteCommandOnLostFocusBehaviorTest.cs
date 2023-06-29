using System.Windows.Input;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Common.Commands;
using Neo.ApplicationFramework.Common.TestHelpers;
using Neo.ApplicationFramework.Common.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Behaviors
{
    [TestFixture]
    public class ExecuteCommandOnLostFocusBehaviorTest
    {
        private ExecuteCommandOnLostFocusBehavior m_ExecuteCommandOnLostFocusBehavior;
        private RibbonTextBox m_TextBox;
        private LocalViewModel m_LocalViewModel;

        private ICommand m_Command;

        [SetUp]
        public void RunBeforeEachTest()
        {
            m_ExecuteCommandOnLostFocusBehavior = new ExecuteCommandOnLostFocusBehavior();
            m_TextBox = new RibbonTextBox();
            m_ExecuteCommandOnLostFocusBehavior.Attach(m_TextBox);
            m_LocalViewModel = new LocalViewModel(Substitute.For<ICommandManagerService>());
        }

        [Test]
        public void ShouldNotExecuteCommandIfCanExecuteIsFalse()
        {
            bool commandHasBeenRun = false;
            m_Command = new LocalDelegateCommand(m_LocalViewModel,() => { commandHasBeenRun = true; }, () => false);
            m_TextBox.Command = m_Command;

            m_TextBox.TriggerLostKeyBoardFocus();

            Assert.That(commandHasBeenRun, Is.False);
        }

        [Test]
        public void ShouldExecuteCommandIfCanExecuteIsTrue()
        {
            bool commandHasBeenRun = false;
            m_Command = new LocalDelegateCommand(m_LocalViewModel,() => { commandHasBeenRun = true; }, () => true);
            m_TextBox.Command = m_Command;

            m_TextBox.TriggerLostKeyBoardFocus();

            Assert.That(commandHasBeenRun, Is.True);
        }


        [Test]
        public void CommandCanBeNullOnTextBox()
        {
            m_TextBox.Command = null;

            m_TextBox.TriggerLostKeyBoardFocus();
        }
    }

}