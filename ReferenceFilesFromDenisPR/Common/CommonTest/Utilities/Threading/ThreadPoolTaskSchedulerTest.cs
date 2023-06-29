using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Utilities.Threading
{
    [TestFixture]
    public class ThreadPoolTaskSchedulerTest
    {
        private ExtendedThreadPool m_ThredPoolMock;

        private List<Action> m_RecordedThreadPoolQueue;

        private Task m_VoidTask;

        [SetUp]
        public void Setup()
        {
            m_ThredPoolMock = Substitute.For<ExtendedThreadPool>();
            m_ThredPoolMock.QueueUserWorkItem(Arg.Any<ITask>(), Arg.Any<Action<ITask>>())
                .ReturnsForAnyArgs(x =>
                {
                    ((Action<ITask>)x[1]).Invoke((ITask)x[0]);
                    return true;
                });

            m_RecordedThreadPoolQueue = new List<Action>();
            m_VoidTask = new Task(() => { });
        }

        [Test]
        public void Should_not_execute_scheduled_task()
        {
            bool taskWasExecuted = false;
            ThreadPoolTaskScheduler scheduler = new ThreadPoolTaskScheduler(1, m_ThredPoolMock);

            scheduler.Schedule(new Task(() => { taskWasExecuted = true; }));

            Assert.That(taskWasExecuted, Is.True);
        }
        
        [Test]
        public void Should_not_execute_tasks_simultaneously_when_max_concurrency_level_is_reached()
        {
            bool taskWasExecuted = false;
            MockRecordingThreadPool();
            
            ThreadPoolTaskScheduler scheduler = new ThreadPoolTaskScheduler(1, m_ThredPoolMock);

            scheduler.Schedule(m_VoidTask);
            scheduler.Schedule(new Task(() => { taskWasExecuted = true; }));
            
            Assert.That(taskWasExecuted, Is.False);
            Assert.That(m_RecordedThreadPoolQueue.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_execute_a_queued_task_when_concurrency_level_is_below_limit()
        {
            bool taskWasExecuted = false;
            MockRecordingThreadPool();
            ThreadPoolTaskScheduler scheduler = new ThreadPoolTaskScheduler(1, m_ThredPoolMock);

            scheduler.Schedule(m_VoidTask);
            scheduler.Schedule(new Task(() => { taskWasExecuted = true; }));
            m_RecordedThreadPoolQueue[0]();
            
            Assert.That(m_RecordedThreadPoolQueue.Count, Is.EqualTo(2));
            m_RecordedThreadPoolQueue[1]();
            Assert.That(taskWasExecuted, Is.True);
        }

        private void MockRecordingThreadPool()
        {
            m_ThredPoolMock = Substitute.For<ExtendedThreadPool>();
            m_ThredPoolMock.QueueUserWorkItem(Arg.Any<ITask>(), Arg.Any<Action<ITask>>())
                .ReturnsForAnyArgs(x =>
                {
                    m_RecordedThreadPoolQueue.Add(() => ((Action<ITask>)x[1]).Invoke((ITask)x[0]));
                    return true;
                });
        }
    }
}