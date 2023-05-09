using System;
using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
        private MockRepository m_MockRepository;
        private IMessageRecorderViewFactory m_MessageRecorderViewFactory;
        private IMessageRecorderView m_MessageRecorderView;

        [SetUp]
        public void SetUp()
        {
            m_MockRepository = new MockRepository();
            m_MessageRecorderView = m_MockRepository.StrictMock<IMessageRecorderView>();
            m_MessageRecorderViewFactory = m_MockRepository.Stub<IMessageRecorderViewFactory>();
            m_MessageRecorderViewFactory.CreateMessageRecorderView();
            LastCall.Return(m_MessageRecorderView);
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
            using (m_MockRepository.Record())
            {
                m_MessageRecorderView.ShowMessages(m_Caption);
                LastCall.IgnoreArguments().Repeat.Never();
            }

            using (m_MockRepository.Playback())
            {
                m_MessageRecorderService.Cancel();
                m_MessageRecorderService.EndAndShow();
            }
        }

        [Test]
        public void CallingEndAndShowShouldCallShowMessagesOnViewWithCaptionFromCallToBegin()
        {
            using (m_MockRepository.Record())
            {
                Expect.Call(m_MessageRecorderView.ImageMessages).PropertyBehavior();
                m_MessageRecorderView.ShowMessages(m_Caption);
            }

            using (m_MockRepository.Playback())
            {
                m_MessageRecorderService.EndAndShow();
            }
        }

        [Test]
        public void CallingBeginASecondTimeWillThrowInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => m_MessageRecorderService.Begin(m_Caption));
        }
    }
}
