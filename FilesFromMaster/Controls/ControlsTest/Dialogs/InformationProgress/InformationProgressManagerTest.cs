using Neo.ApplicationFramework.Controls.Dialogs.InformationProgress;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Dialogs.InformationProgress
{
    [TestFixture]
    public class InformationProgressManagerTest
    {
        private IInformationProgressService m_InformationProgressService;

        [SetUp]
        public void SetUp()
        {
            m_InformationProgressService = new InvisibleInformationProgressManager();
        }

        [Test]
        public void CurrentTaskWhenEmpty()
        {
            Assert.IsNull(m_InformationProgressService.CurrentTask);
        }

        [Test]
        public void AddSingleTask()
        {
            using (m_InformationProgressService.BeginTask("Saving"))
            {
                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.AreEqual("Saving", m_InformationProgressService.CurrentTask.Name);
            }

            Assert.AreEqual(0, m_InformationProgressService.TaskCount);
        }

        [Test]
        public void AddMultipleTasks()
        {
            using (m_InformationProgressService.BeginTask("Building"))
            {
                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.AreEqual("Building", m_InformationProgressService.CurrentTask.Name);

                using (m_InformationProgressService.BeginTask("Saving"))
                {
                    Assert.AreEqual(2, m_InformationProgressService.TaskCount);
                    Assert.AreEqual("Saving", m_InformationProgressService.CurrentTask.Name);
                }

                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.AreEqual("Building", m_InformationProgressService.CurrentTask.Name);

                using (m_InformationProgressService.BeginTask("Validating"))
                {
                    Assert.AreEqual(2, m_InformationProgressService.TaskCount);
                    Assert.AreEqual("Validating", m_InformationProgressService.CurrentTask.Name);
                }

                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.AreEqual("Building", m_InformationProgressService.CurrentTask.Name);
            }

            Assert.AreEqual(0, m_InformationProgressService.TaskCount);
        }

        [Test]
        public void AbortSingleTask()
        {
            using (IProgressTask progressTask = m_InformationProgressService.BeginTask("Saving"))
            {
                progressTask.Abort();

                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.IsTrue(progressTask.IsAborting);
            }
        }

        [Test]
        public void AbortMultipleTasks()
        {
            using (IProgressTask buildTask = m_InformationProgressService.BeginTask("Building"))
            {
                using (IProgressTask saveTask = m_InformationProgressService.BeginTask("Saving"))
                {
                    saveTask.Abort();

                    Assert.AreEqual(2, m_InformationProgressService.TaskCount);
                    Assert.IsTrue(saveTask.IsAborting);
                }

                //Assert.AreEqual(0, m_InformationProgressService.TaskCount);
                Assert.IsTrue(buildTask.IsAborting);
            }
        }

        [Test]
        public void AddTaskAfterAbortingParentTask()
        {
            using (IProgressTask buildTask = m_InformationProgressService.BeginTask("Building"))
            {
                buildTask.Abort();

                using (IProgressTask saveTask = m_InformationProgressService.BeginTask("Saving"))
                {
                    Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                    Assert.IsTrue(saveTask.IsAborting);
                }

                Assert.AreEqual(1, m_InformationProgressService.TaskCount);
                Assert.IsTrue(buildTask.IsAborting);
            }
        }

        [Test]
        public void EndParentTaskDoesNotEndChildTasks()
        {
            using (IProgressTask buildTask = m_InformationProgressService.BeginTask("Building"))
            {
                using (IProgressTask saveTask = m_InformationProgressService.BeginTask("Saving"))
                {
                    m_InformationProgressService.EndTask("Building");
                    Assert.IsFalse(saveTask.HasEnded);
                }

                Assert.IsTrue(buildTask.HasEnded);
            }
        }
    }
}
