using System;
using Core.Component.Api.Instantiation;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Threading;
using Neo.ApplicationFramework.Threading;
using Neo.ApplicationFramework.Utilities.Lazy;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class AlarmItemAcknowledgedTest
    {
        private AlarmServer m_AlarmServer;
        private bool m_AlarmAcknowledged;
        private bool m_GroupAcknowledged;
        private bool m_ServerAcknowledged;
        private bool m_AlarmDeleted;
        private IAlarmServerStorage m_AlarmServerStorage;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.AddServiceStub<IAlarmServerStateService>();
            TestHelper.AddServiceStub<IStorageCacheService>();

            ISecurityServiceCF securityService = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityService.CurrentUser.Returns(string.Empty);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.LocalTime.Returns(DateTime.Now);

            m_AlarmServerStorage = Substitute.For<IAlarmServerStorage>();
            var eventFactory = new AlarmEventFactory();
            eventFactory.AddAlarmEventTypeProvider("AlarmEvent", () => new AlarmEvent()); // register default AlarmEvent
            m_AlarmServer = new ExtendedAlarmServer(m_AlarmServerStorage, eventFactory) { IsEnabled = true };
            m_AlarmServer.AlarmAcknowledge += OnAlarmServerAlarmAcknowledge;
            m_AlarmServer.AlarmDeleted += OnAlarmServerAlarmDeleted;
            AlarmGroup alarmGroup = m_AlarmServer.AlarmGroups.AddNew() as AlarmGroup;
            alarmGroup.AlarmAcknowledge += OnAlarmGroupAlarmAcknowledge;
            m_AlarmAcknowledged = false;
            m_GroupAcknowledged = false;
            m_ServerAcknowledged = false;
            m_AlarmDeleted = false;
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
            m_AlarmServer.Dispose();
        }

        private AlarmItem CreateNewAlarmItem()
        {
            AlarmItem alarmItem = new AlarmItem();
            alarmItem.Server = m_AlarmServer;
            alarmItem.Value = 0;
            alarmItem.IsDigitalValue = true;
            alarmItem.ComparerType = ComparerTypes.RisingEdge;
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);
            alarmItem.AcknowledgeRequired = true;
            alarmItem.AlarmAcknowledge += OnAlarmItemAlarmAcknowledged;
            ((IAlarmItem)alarmItem).EnableValueInput = true;
            return alarmItem;
        }

        [Test]
        public void NewAlarmItemNotAcknowledgedFired()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();

            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AcknowledgeNewAlarmItemNoAcknowledgeFired()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AlarmItemActiveDoesNotFireAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();

            alarmItem.Value = true;

            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AlarmItemChangingStatusDoesNotFireAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;

            alarmItem.Value = false;

            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AckActiveAlarmFiresAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_AlarmAcknowledged);
        }

        [Test]
        public void AckInactiveAlarmFiresAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;
            alarmItem.Value = false;

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_AlarmAcknowledged);
        }

        [Test]
        public void AckAlreadyAckedActiveAlarmDoesFireAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;
            alarmItem.Acknowledge();
            m_AlarmAcknowledged = false;

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_AlarmAcknowledged);
        }

        [Test]
        public void AckAlreadyAckedInactiveAlarmDoesntFireAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;
            alarmItem.Value = false;
            alarmItem.Acknowledge();
            m_AlarmAcknowledged = false;

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AckAlreadyAckedActiveAlarmDoesFireAcknowledgedWithAckRequiredFalse()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.AcknowledgeRequired = false;
            alarmItem.Value = true;
            alarmItem.Acknowledge();
            Assert.AreEqual(true, m_AlarmAcknowledged);
            m_AlarmAcknowledged = false;

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_AlarmAcknowledged);
        }

        [Test]
        public void AckNormalAlarmDoesntFireAcknowledgedWithAckRequiredFalse()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.AcknowledgeRequired = false;
            alarmItem.Value = true;
            alarmItem.Value = false;
            m_AlarmAcknowledged = false;
            alarmItem.Acknowledge();
            Assert.AreEqual(false, m_AlarmAcknowledged);
        }

        [Test]
        public void AckActiveAlarmItemAcksGroupAndServer()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;
            Assert.AreEqual(false, m_GroupAcknowledged);
            Assert.AreEqual(false, m_ServerAcknowledged);

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_GroupAcknowledged);
            Assert.AreEqual(true, m_ServerAcknowledged);
        }

        [Test]
        public void AckInActiveAlarmItemAcksGroupAndServer()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.Value = true;
            alarmItem.Value = false;
            Assert.AreEqual(false, m_GroupAcknowledged);
            Assert.AreEqual(false, m_ServerAcknowledged);

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_GroupAcknowledged);
            Assert.AreEqual(true, m_ServerAcknowledged);
        }

        [Test]
        public void AckActiveAlarmItemAcksGroupAndServerWithoutRequiredAck()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.AcknowledgeRequired = false;
            alarmItem.Value = true;
            Assert.AreEqual(false, m_GroupAcknowledged);
            Assert.AreEqual(false, m_ServerAcknowledged);

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_GroupAcknowledged);
            Assert.AreEqual(true, m_ServerAcknowledged);
        }

        [Test]
        public void AckInActiveAlarmItemAcksGroupAndServerWithoutRequiredAck()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.AcknowledgeRequired = false;
            alarmItem.Value = true;
            alarmItem.Value = false;
            Assert.AreEqual(false, m_GroupAcknowledged);
            Assert.AreEqual(false, m_ServerAcknowledged);

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_GroupAcknowledged);
            Assert.AreEqual(false, m_ServerAcknowledged);
        }

        [Test]
        public void AckInactiveAlarmWithHistoryDoesNotDeleteIt()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.History = true;
            alarmItem.Value = true;
            alarmItem.Value = false;
            Assert.AreEqual(false, m_AlarmDeleted);

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_AlarmDeleted);
        }

        [Test]
        public void AckInactiveAlarmWithoutHistoryDeletesIt()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.History = false;
            alarmItem.Value = true;
            alarmItem.Value = false;
            Assert.AreEqual(false, m_AlarmDeleted);

            alarmItem.Acknowledge();

            Assert.AreEqual(true, m_AlarmDeleted);
        }

        [Test]
        public void AckActiveAlarmWithoutHistoryDoesNotDeleteIt()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.History = false;
            alarmItem.Value = true;
            Assert.AreEqual(false, m_AlarmDeleted);

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_AlarmDeleted);
        }

        [Test]
        public void AckActiveAlarmWithHistoryDoesNotDeleteIt()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.History = false;
            alarmItem.Value = true;
            Assert.AreEqual(false, m_AlarmDeleted);

            alarmItem.Acknowledge();

            Assert.AreEqual(false, m_AlarmDeleted);
        }

        private void OnAlarmItemAlarmAcknowledged(object sender, EventArgs e)
        {
            m_AlarmAcknowledged = true;
        }

        private void OnAlarmGroupAlarmAcknowledge(object sender, EventArgs e)
        {
            m_GroupAcknowledged = true;
        }

        private void OnAlarmServerAlarmAcknowledge(object sender, EventArgs e)
        {
            m_ServerAcknowledged = true;
        }

        private void OnAlarmServerAlarmDeleted(object sender, EventArgs e)
        {
            m_AlarmDeleted = true;
        }

        internal class ExtendedAlarmServer : AlarmServer
        {
            public ExtendedAlarmServer(IAlarmServerStorage alarmServerStorage, IAlarmEventFactory alarmEventFactory)
               : base(alarmServerStorage, 
                     new LazyCF<IActionConsumer>(() => new InvokeDirectActionConsumer(ActionConsumerName) as IActionConsumer), 
                     Substitute.For<IRootComponentService>().ToILazy(),
                     Substitute.For<ISystemTagServiceCF>().ToILazy(),
                     alarmEventFactory.ToILazy())
            {
                base.m_Initialized = true;
            }
        }
    }
}
