using System;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class AlarmEventTest
    {
        private const string AlarmItemName = "Some alarm item name";
        private IAlarmServer m_AlarmServerMock;
        private IAlarmItem m_AlarmItemMock;


        private IAlarmServerStateService m_AlarmServerStateService;

        [SetUp]
        public void SetUp()
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(true);
            m_AlarmServerStateService = Substitute.For<IAlarmServerStateService>();
            TestHelper.AddService<IAlarmServerStateService>(m_AlarmServerStateService);

            m_AlarmItemMock = Substitute.For<IAlarmItem>();

            var alarmServerMock = Substitute.For<IAlarmServer>();
            alarmServerMock.GetAlarmItem(AlarmItemName)
                           .Returns(m_AlarmItemMock);

            m_AlarmServerMock = alarmServerMock;

            ISecurityServiceCF securityService = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityService.CurrentUser.Returns(string.Empty);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.LocalTime.Returns(DateTime.Now);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        #region AlarmOff / IAlarmServerEventState Tests

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnActiveAlarmEvent()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);

            alarmEvent.AlarmOff();
            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Active);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Inactive);
        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnActiveAlarmEventWhichDoesNotrequireAcknowledge()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(false);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);

            alarmEvent.AlarmOff();

            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Active);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Inactive);
            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Inactive);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Normal);
        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnAcknowledgedAlarmEvent()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Acknowledge);

            alarmEvent.AlarmOff();

            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Acknowledge);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Inactive);
            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Inactive);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Normal);
        }

        [Test]
        public void NoEventStateRegistrationsWhenShuttingOffAnInactiveAlarm()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Inactive);
            alarmEvent.AlarmOff();

            // Assertion ?
        }

        [Test]
        public void NoEventStateRegistrationsAreCorrectWhenShuttingOffANormalAlarm()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Normal);
            alarmEvent.AlarmOff();

            // Assertion ?
        }

        #endregion

        #region Acknowledge / IAlarmServerEventState Tests

        [Test]
        public void EventStateRegistrationsAreCorrectWhenAcknowledgingAnActiveAlarmEvent()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);
            alarmEvent.Acknowledge();

            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Active);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Acknowledge);

        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenAcknowledgingAnInactiveAlarmEvent()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Inactive);
            alarmEvent.Acknowledge();

            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Inactive);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Acknowledge);
            m_AlarmServerStateService.UnRegisterAlarmEventState(AlarmState.Acknowledge);
            m_AlarmServerStateService.RegisterAlarmEventState(AlarmState.Normal);
        }

        [Test]
        public void NoEventStateRegistrationsWhenAcknowledgingAnAcknowledgedAlarm()
        {
            m_AlarmItemMock.AcknowledgeRequired.Returns(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Acknowledge);
            alarmEvent.Acknowledge();

            // Assertion ?
        }

        #endregion

        private AlarmEvent CreateAlarmEvent(AlarmState alarmState)
        {
            return new AlarmEvent(
                m_AlarmServerMock,
                Guid.NewGuid(),
                AlarmItemName,
                alarmState,
                alarmState == AlarmState.Acknowledge ? (DateTime?)DateTime.Now : null,
                alarmState == AlarmState.Inactive ? (DateTime?)DateTime.Now : null,
                alarmState == AlarmState.Normal ? (DateTime?)DateTime.Now : null,
                DateTime.Now, string.Empty, 1);
        }
    }
}
