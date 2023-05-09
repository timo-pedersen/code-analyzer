using System;
using Core.Api.Tools;
using Core.Component.Api.Instantiation;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Scheduler
{
    [TestFixture]
    public class SchedulerTest
    {

        private Scheduler m_Scheduler;
        private SchedulerJob m_SchedulerJob;
        private static object m_StateInfo;
        private IToolManager m_ToolManager;

        [SetUp]
        public void SetUp()
        {
            m_ToolManager = TestHelper.CreateAndAddServiceMock<IToolManager>();
            m_ToolManager.Stub(x => x.Runtime).Return(true);

            var rootComponentService = TestHelper.CreateAndAddServiceMock<IRootComponentService>().ToILazy();

            m_Scheduler = new Scheduler(true, rootComponentService);
            m_SchedulerJob = new SchedulerJob();
            m_StateInfo = new object();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void TestNameProperty()
        {
            m_Scheduler.Name = "TestScheduler";
            Assert.AreEqual(m_Scheduler.Name, "TestScheduler");
        }

        [Test]
        public void TestAddRemoveJobs()
        {
            SchedulerJob schedulerJob1 = new SchedulerJob();
            SchedulerJob schedulerJob2 = new SchedulerJob();
            SchedulerJob schedulerJob3 = new SchedulerJob();
            SchedulerJob schedulerJob4 = new SchedulerJob();

            m_Scheduler.SchedulerJobs.Add(schedulerJob1);
            m_Scheduler.SchedulerJobs.Add(schedulerJob2);
            m_Scheduler.SchedulerJobs.Add(schedulerJob3);
            m_Scheduler.SchedulerJobs.Add(schedulerJob4);

            Assert.AreEqual(4, m_Scheduler.SchedulerJobs.Count, "Four scheduled jobs...");

            m_Scheduler.SchedulerJobs.Remove(schedulerJob1);
            m_Scheduler.SchedulerJobs.Remove(schedulerJob2);
            m_Scheduler.SchedulerJobs.Remove(schedulerJob3);
            m_Scheduler.SchedulerJobs.Remove(schedulerJob4);

            Assert.AreEqual(0, m_Scheduler.SchedulerJobs.Count, "No scheduled jobs...");
        }

        [Test]
        public void TestRemoveJobFromSchedulerWhenStopTimeExpires()
        {
            m_SchedulerJob.StartDate = DateTime.MinValue;
            m_SchedulerJob.StopDate = DateTime.MinValue;
            m_SchedulerJob.StartTime = DateTime.MinValue;
            m_SchedulerJob.StopTimeEnabled = true;

            m_Scheduler.SchedulerJobs.Add(m_SchedulerJob);

            Assert.AreEqual(1, m_Scheduler.SchedulerJobs.Count, "One scheduled jo added...");

            m_Scheduler.CheckJobs(m_StateInfo);

            Assert.AreEqual(0, m_Scheduler.SchedulerJobs.Count, "Stop time expired and job is removed.");
        }
    }
}
