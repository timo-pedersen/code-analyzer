using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MessageRecorder
{
    [TestFixture]
    public class MessageRecorderServiceWithoutMessagesTest
    {
        private IMessageRecorderService m_MessageRecorderService;
        private MockRepository m_MockRepository;
        private IMessageRecorderViewFactory m_MessageRecorderViewFactory;
        private IMessageRecorderView m_MessageRecorderView;
        private readonly string m_Caption = "SomeCaption";

        [SetUp]
        public void SetUp()
        {
            m_MockRepository = new MockRepository();
            m_MessageRecorderView = m_MockRepository.StrictMock<IMessageRecorderView>();
            m_MessageRecorderViewFactory = m_MockRepository.Stub<IMessageRecorderViewFactory>();
            m_MessageRecorderViewFactory.CreateMessageRecorderView();
            LastCall.Return(m_MessageRecorderView);
            m_MessageRecorderService = new MessageRecorderService(m_MessageRecorderViewFactory);
        }

        [Test]
        public void CallingEndAndShowWithoutCallingBeginWillNotCallShow()
        {
            using (m_MockRepository.Record())
            {
                m_MessageRecorderView.ShowMessages(string.Empty);
                LastCall.IgnoreArguments().Repeat.Never();
            }

            using (m_MockRepository.Playback())
            {
                m_MessageRecorderService.EndAndShow();
            }
        }

        [Test]
        public void CallingBeginThenEndAndShowWithoutAddingMessagesWillNotCallShow()
        {
            using (m_MockRepository.Record())
            {
                m_MessageRecorderView.ShowMessages(string.Empty);
                LastCall.IgnoreArguments().Repeat.Never();
            }

            using (m_MockRepository.Playback())
            {
                m_MessageRecorderService.Begin(m_Caption);
                m_MessageRecorderService.EndAndShow();
            }
        }

        [Test]
        public void AddingMessageWithoutCallingBeginWillNotAddMessage()
        {
            m_MessageRecorderService.AddMessage(string.Empty, MessageBoxImage.None);
            Assert.AreEqual(0, m_MessageRecorderService.MessageCount);
        }
    }
}