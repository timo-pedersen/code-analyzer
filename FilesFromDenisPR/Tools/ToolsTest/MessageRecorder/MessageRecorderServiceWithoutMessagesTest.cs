using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.MessageRecorder
{
    [TestFixture]
    public class MessageRecorderServiceWithoutMessagesTest
    {
        private IMessageRecorderService m_MessageRecorderService;
        private IMessageRecorderViewFactory m_MessageRecorderViewFactory;
        private IMessageRecorderView m_MessageRecorderView;
        private readonly string m_Caption = "SomeCaption";

        [SetUp]
        public void SetUp()
        {
            m_MessageRecorderView = Substitute.For<IMessageRecorderView>();
            m_MessageRecorderViewFactory = Substitute.For<IMessageRecorderViewFactory>();
            m_MessageRecorderViewFactory.CreateMessageRecorderView().Returns(m_MessageRecorderView);
            m_MessageRecorderService = new MessageRecorderService(m_MessageRecorderViewFactory);
        }

        [Test]
        public void CallingEndAndShowWithoutCallingBeginWillNotCallShow()
        {
            m_MessageRecorderService.EndAndShow();

            m_MessageRecorderView.DidNotReceiveWithAnyArgs().ShowMessages(Arg.Any<string>());
        }

        [Test]
        public void CallingBeginThenEndAndShowWithoutAddingMessagesWillNotCallShow()
        {
            m_MessageRecorderService.Begin(m_Caption);
            m_MessageRecorderService.EndAndShow();

            m_MessageRecorderView.DidNotReceiveWithAnyArgs().ShowMessages(Arg.Any<string>());
        }

        [Test]
        public void AddingMessageWithoutCallingBeginWillNotAddMessage()
        {
            m_MessageRecorderService.AddMessage(string.Empty, MessageBoxImage.None);
            Assert.AreEqual(0, m_MessageRecorderService.MessageCount);
        }
    }
}