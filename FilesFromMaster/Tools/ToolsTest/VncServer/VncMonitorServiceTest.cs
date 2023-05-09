using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.VncServer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.VncServer
{
    [TestFixture]
    class VncMonitorServiceTest
    {
        private IVncServerMonitorService m_VncService;

        private string NextMessage
        {
            get
            {
                string retVal;
                lock (m_Messages)
                {
                    if (m_Messages.Count == 0)
                        return null;

                    retVal = m_Messages.Dequeue();
                }
                UpdateMessageEmptyEvent();
                return retVal == "done" ? null : retVal;
            }
        }

        private ManualResetEvent m_MessagesEmptyEvent;
        private Queue<string> m_Messages;

        [SetUp]
        public void Setup()
        {
            var api = MockRepository.GenerateMock<INativeAPI>();
            api.Stub(x => x.CreateMsgQueue(Arg<string>.Is.Anything, ref Arg<MSGQUEUEOPTIONS>.Ref(Rhino.Mocks.Constraints.Is.Anything(), new MSGQUEUEOPTIONS()).Dummy))
                .Return(new IntPtr(1));
            api.Stub(x => x.CloseMsgQueue(Arg<IntPtr>.Is.Anything)).WhenCalled(
                call =>
                {
                    Debug.WriteLine("Closing message queue");
                    lock (m_Messages)
                    {
                        m_Messages.Clear();
                    }
                    UpdateMessageEmptyEvent();
                }).Return(true);
            api.Stub(x => x.ReadMsgQueue(Arg<IntPtr>.Is.Anything, out Arg<string>.Out(null).Dummy, Arg<uint>.Is.Anything, out Arg<uint>.Out(0).Dummy))
                .WhenCalled(
                    call =>
                    {
                        string theMessage = NextMessage;
                        call.Arguments[1] = theMessage;
                        call.Arguments[3] = 0u;
                        call.ReturnValue = theMessage != null ;
                        Debug.WriteLine("Read message: " + theMessage);
                    })
                .Return(true);

            var lazyApi = MockRepository.GenerateMock<ILazy<INativeAPI>>();
            lazyApi.Stub(x => x.Value).Return(api);

            m_VncService = new VncMonitorService(lazyApi, true);

            m_Messages = new Queue<string>();
            m_MessagesEmptyEvent = new ManualResetEvent(true);
        }

        [Test, Timeout(5000)]
        public void TestEventsSentInResponseToComm()
        {
            var eventCounts = new List<int>();
            SetMessages("COUNT=1", "COUNT=2", "block");
            m_VncService.RemoteSessionCountChanged += (_, countArgs) => eventCounts.Add(countArgs.Message);

            m_VncService.StartMonitoring(Process.GetCurrentProcess());
            WaitForMessageQueueToClear();
            m_VncService.StopMonitoring();

            Assert.IsTrue(eventCounts.Count == 2);
            Assert.IsTrue(eventCounts[0] == 1);
            Assert.IsTrue(eventCounts[1] == 2);
        }

        private void SetMessages(params string[] messages)
        {
            lock (m_Messages)
            {
                foreach (string message in messages)
                {
                    m_Messages.Enqueue(message);
                }
            }
            UpdateMessageEmptyEvent();
        }

        private void UpdateMessageEmptyEvent()
        {
            lock (m_Messages)
            {
                if (m_Messages.Count == 0)
                    m_MessagesEmptyEvent.Set();
                else
                    m_MessagesEmptyEvent.Reset();
            }
        }


        private void WaitForMessageQueueToClear()
        {
            m_MessagesEmptyEvent.WaitOne();
        }
    }
}
