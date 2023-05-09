using System;
using System.ComponentModel;
using Core.Api.Service;
using Core.Component.Api.Instantiation;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Threading;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.DataLogger
{
    [TestFixture]
    public class DataLoggerTest
    {
        private IContainer m_Container;
        private DataLoggerFake m_DataLogger;
        private IStorageService m_StorageService;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;
            m_StorageService = new StorageServiceCF();
            ServiceContainerCF.Instance.AddService(typeof(IStorageService), m_StorageService, true);

            TestHelper.CreateAndAddServiceStub<IDateTimeEditService>();

            m_Container = new Container();
            m_DataLogger = new DataLoggerFake();
            m_DataLogger.Name = "DataLogger1";
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            m_DataLogger = null;
            m_StorageService.DisposeDisposable();

            TestHelper.ClearServices();
        }

        [Test]
        public void CheckDefaultValues()
        {
            Assert.IsNotNull(m_DataLogger.LogItems, "Should not be null");
            Assert.AreEqual(LogTriggerType.LogDataOnObjectEvent, m_DataLogger.LogTrigger, "Should be triggered on an objectevent default");
            Assert.AreEqual(TimeSpan.FromSeconds(10), m_DataLogger.LogInterval, "Should be 10 second default");
            Assert.IsTrue(m_DataLogger.LogChangesOnly, "Should be true default");
        }

        [Test]
        public void NamingDataLoggerPutsTableName()
        {
            const string dataLoggerName = "DataLogger1";
            m_DataLogger.Name = dataLoggerName;
            Assert.AreEqual(dataLoggerName, m_DataLogger.GetDatabaseTableName, "The DataLogger.Name should be put as the default tablename");
        }

        [Test]
        public void ChangeName()
        {
            const string dataLoggerName = "DataLogger1";
            m_DataLogger.Name = dataLoggerName;
            Assert.AreEqual(dataLoggerName, m_DataLogger.Name, "The DataLogger.Name should keep it's original name.");
        }

        private class DataLoggerFake : DataLogger
        {
            public DataLoggerFake()
                : base(ServiceContainerCF.GetServiceLazy<IScopeService>(), null, null, ServiceContainerCF.GetServiceLazy<IDateTimeEditService>(), ServiceContainerCF.GetServiceLazy<IRootComponentService>(), null)
            { }

            public string GetDatabaseTableName { get { return DatabaseTableName; } }
        }
    }
}