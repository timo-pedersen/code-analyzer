using System;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Utilities.Threading
{
    [TestFixture]
    public class TaskTest
    {
        private Task m_VoidTask;

        private ITaskScheduler m_TaskScheduler;

        [SetUp]
        public void Setup()
        {
            m_VoidTask = new Task(() => { });
            m_TaskScheduler = MockRepository.GenerateMock<ITaskScheduler>();

            m_TaskScheduler
                .Stub(m => m.Schedule(null))
                .IgnoreArguments()
                .WhenCalled(invocation => ((Task)invocation.Arguments[0]).Execute());
        }

        [Test]
        public void Should_execute_continuation_after_run_to_completion()
        {
            bool continuationWasRun = false;
            m_VoidTask.ContinueWith(t => { continuationWasRun = true; });

            m_VoidTask.Start(m_TaskScheduler);
            
            Assert.That(continuationWasRun, Is.True);
        }

        [Test]
        public void Should_execute_continuation_when_task_has_run_to_completion()
        {
            bool continuationWasRun = false;

            m_VoidTask.Start(m_TaskScheduler);
            m_VoidTask.ContinueWith(t => { continuationWasRun = true; });

            Assert.That(continuationWasRun, Is.True);
        }
        
        [Test]
        public void Should_have_ran_to_completion_status_when_task_was_executed_succesfully()
        {
            m_VoidTask.Start(m_TaskScheduler);

            Assert.That(m_VoidTask.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        
        [Test]
        public void Should_have_faulted_status_when_task_threw_exception()
        {
            Task task = new Task(() => { throw new Exception("darn"); });

            task.Start(m_TaskScheduler);

            Assert.That(task.Status, Is.EqualTo(TaskStatus.Faulted));
        }
        
        [Test]
        public void Should_have_exception_when_task_threw_exception()
        {
            Exception exceptionThrown = new Exception("darn");
            Task task = new Task(() => { throw exceptionThrown; });

            task.Start(m_TaskScheduler);

            Assert.That(task.Exception, Is.SameAs(exceptionThrown));
        }
 
        [Test]
        public void Should_have_running_status_when_task_is_running()
        {
            TaskStatus status = TaskStatus.Created;
            Task task = null;
            task = new Task(() => { status = task.Status; });

            task.Execute();

            Assert.That(status, Is.EqualTo(TaskStatus.Running));
        }

        [Test]
        public void Should_rethrow_when_waiting_ot_task_that_faults()
        {
            Task task = new Task(() => { throw new Exception("darn"); });

            task.Start(m_TaskScheduler);

            Assert.Throws<Exception>(() => task.Wait());
        }

        [Test]
        public void Should_return_result_of_task()
        {
            Task<bool> task = new Task<bool>(() => { return true; });

            task.Start(m_TaskScheduler);

            Assert.That(task.Result, Is.True);
        }
        
        [Test]
        public void Should_allow_task_to_be_started_only_once()
        {
            m_VoidTask.Start(m_TaskScheduler);

            Assert.Throws<InvalidOperationException>(() => m_VoidTask.Start(m_TaskScheduler));
        }
        
        [Test]
        public void Should_require_a_task_scheduler_when_starting()
        {
            Assert.Throws<ArgumentNullException>(() => m_VoidTask.Start(null));
        }
    }
}
