using System;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class AlarmEventTest
    {
        private const string AlarmItemName = "Some alarm item name";
        private MockRepository m_Mocks;
        private IAlarmServer m_AlarmServerMock;
        private IAlarmItem m_AlarmItemMock;


        private IAlarmServerStateService m_AlarmServerStateService;

        [SetUp]
        public void SetUp()
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(true);
            m_AlarmServerStateService = MockRepository.GenerateMock<IAlarmServerStateService>();
            TestHelper.AddService<IAlarmServerStateService>(m_AlarmServerStateService);

            m_AlarmItemMock = MockRepository.GenerateMock<IAlarmItem>();

            m_Mocks = new MockRepository();
            var alarmServerMock = m_Mocks.StrictMultiMock<IAlarmServer>(typeof(IAlarmServerEventState));
            alarmServerMock.Stub<IAlarmServer>(alarmServer => alarmServer.GetAlarmItem(AlarmItemName))
                           .Return(m_AlarmItemMock)
                           .Repeat.Any();
            alarmServerMock.Replay();

            m_AlarmServerMock = alarmServerMock;

            ISecurityServiceCF securityService = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityService.Stub(x => x.CurrentUser).Return(string.Empty);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.Stub(x => x.LocalTime).Return(DateTime.Now);
        }

        [TearDown]
        public void TearDown()
        {
            m_Mocks.VerifyAll();
            TestHelper.ClearServices();
        }

        #region AlarmOff / IAlarmServerEventState Tests

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnActiveAlarmEvent()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Active));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Inactive));

            alarmEvent.AlarmOff();
            m_AlarmServerStateService.VerifyAllExpectations();
        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnActiveAlarmEventWhichDoesNotrequireAcknowledge()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(false);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Active));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Inactive));
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Inactive));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Normal));

            alarmEvent.AlarmOff();

            m_AlarmServerStateService.VerifyAllExpectations();
            m_AlarmItemMock.VerifyAllExpectations();
        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenShuttingOffAnAcknowledgedAlarmEvent()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Acknowledge);

            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Acknowledge));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Inactive));
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Inactive));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Normal));

            alarmEvent.AlarmOff();
            m_AlarmServerStateService.VerifyAllExpectations();
        }

        [Test]
        public void NoEventStateRegistrationsWhenShuttingOffAnInactiveAlarm()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Inactive);

            using (m_Mocks.Record())
            {
            }

            using (m_Mocks.Playback())
            {
                alarmEvent.AlarmOff();
            }

        }

        [Test]
        public void NoEventStateRegistrationsAreCorrectWhenShuttingOffANormalAlarm()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Normal);

            using (m_Mocks.Record())
            {
            }

            using (m_Mocks.Playback())
            {
                alarmEvent.AlarmOff();
            }
        }

        #endregion

        #region Acknowledge / IAlarmServerEventState Tests

        [Test]
        public void EventStateRegistrationsAreCorrectWhenAcknowledgingAnActiveAlarmEvent()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Active);

            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Active));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Acknowledge));

            alarmEvent.Acknowledge();
            m_AlarmServerStateService.VerifyAllExpectations();

        }

        [Test]
        public void EventStateRegistrationsAreCorrectWhenAcknowledgingAnInactiveAlarmEvent()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Inactive);
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Inactive));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Acknowledge));
            m_AlarmServerStateService.Expect(x => x.UnRegisterAlarmEventState(AlarmState.Acknowledge));
            m_AlarmServerStateService.Expect(x => x.RegisterAlarmEventState(AlarmState.Normal));

            alarmEvent.Acknowledge();
            m_AlarmServerStateService.VerifyAllExpectations();
        }

        [Test]
        public void NoEventStateRegistrationsWhenAcknowledgingAnAcknowledgedAlarm()
        {
            m_AlarmItemMock.Expect(alarmItem => alarmItem.AcknowledgeRequired).Return(true);

            IAlarmEvent alarmEvent = CreateAlarmEvent(AlarmState.Acknowledge);

            using (m_Mocks.Record())
            {
            }

            using (m_Mocks.Playback())
            {
                alarmEvent.Acknowledge();
            }
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
