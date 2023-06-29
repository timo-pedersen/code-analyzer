using System;
using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MessageRecorder
{
    [TestFixture]
    public class MessageRecorderServiceWithMessagesTest
    {
        private const string m_Message = "SomeMessage";
        private const MessageBoxImage m_MessageBoxImage = MessageBoxImage.None;
        private const string m_Caption = "SomeCaption";
        private const int m_InitialMessageCount = 3;
        private IMessageRecorderService m_MessageRecorderService;
        private IMessageRecorderViewFactory m_MessageRecorderViewFactory;
        private IMessageRecorderView m_MessageRecorderView;

        [SetUp]
        public void SetUp()
        {
            m_MessageRecorderView = Substitute.For<IMessageRecorderView>();
            m_MessageRecorderViewFactory = Substitute.For<IMessageRecorderViewFactory>();
            m_MessageRecorderViewFactory.CreateMessageRecorderView().Returns(m_MessageRecorderView);
            m_MessageRecorderService = new MessageRecorderService(m_MessageRecorderViewFactory);
            m_MessageRecorderService.Begin(m_Caption);
            m_MessageRecorderService.AddMessage("WarningMessage", MessageBoxImage.Warning);
            m_MessageRecorderService.AddMessage("InformationMessage", MessageBoxImage.Information);
            m_MessageRecorderService.AddMessage("ErrorMessage", MessageBoxImage.Error);
        }

        [Test]
        public void MessageRecorderServiceHasThreeMessagesInitially()
        {
            Assert.AreEqual(m_InitialMessageCount, m_MessageRecorderService.MessageCount);
        }

        [Test]
        public void CallingAddMessageWillIncreaseMessageCount()
        {
            m_MessageRecorderService.AddMessage(m_Message, m_MessageBoxImage);
            Assert.AreEqual(m_InitialMessageCount + 1, m_MessageRecorderService.MessageCount);
        }

        [Test]
        public void MessageCountIsZeroAfterCallingCancel()
        {
            m_MessageRecorderService.Cancel();
            Assert.AreEqual(0, m_MessageRecorderService.MessageCount);
        }

        [Test]
        public void EndAndShowWillNotCallShowMessagesOnViewIfNoMessagesToShow()
        {
            m_MessageRecorderService.Cancel();
            m_MessageRecorderService.EndAndShow();

            m_MessageRecorderView.DidNotReceiveWithAnyArgs().ShowMessages(m_Caption);
        }

        [Test]
        public void CallingEndAndShowShouldCallShowMessagesOnViewWithCaptionFromCallToBegin()
        {
            m_MessageRecorderService.EndAndShow();

            m_MessageRecorderView.Received().ShowMessages(m_Caption);
        }

        [Test]
        public void CallingBeginASecondTimeWillThrowInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => m_MessageRecorderService.Begin(m_Caption));
        }
    }
}
