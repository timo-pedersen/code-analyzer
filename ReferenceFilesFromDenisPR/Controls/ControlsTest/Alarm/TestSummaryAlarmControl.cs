using Core.Api.DataSource;
using Core.Api.Tools;
using Neo.ApplicationFramework.Controls.Alarm;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

#pragma warning disable 67 //The event is never used

namespace Neo.ApplicationFramework.Controls
{
    [TestFixture]
    public class TestSummaryAlarmControl
    {
        private SummaryAlarmControl m_SummaryAlarmControl;
        private IAlarmServerStateService m_AlarmServerStateServiceStub;
        private IRemoteAlarmServerStateService m_RemoteAlarmServerStateServiceStub;

        [SetUp]
        public void Setup()
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(true);

            m_AlarmServerStateServiceStub = Substitute.For<IAlarmServerStateService>();
            m_RemoteAlarmServerStateServiceStub = Substitute.For<IRemoteAlarmServerStateService>();

            m_SummaryAlarmControl = new SummaryAlarmControl(m_AlarmServerStateServiceStub, m_RemoteAlarmServerStateServiceStub);
        }

        #region AlarmServerEventsState.None

        [Test]
        public void AlarmServerEventsStateNone_TestSummaryAlarmControlIsNotVisibleAnyAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Never;

            m_AlarmServerStateServiceStub.AnyActive += (s, args) => { };
            Raise.EventWith(this, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
        }

        [Test]
        public void AlarmServerEventsStateNone_TestSummaryAlarmControlIsNotVisibleNoAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Never;

            m_AlarmServerStateServiceStub.AnyActive += (s, args) => { };
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
        }

        [Test]
        public void AlarmServerEventsStateNone_TestSummaryAlarmControlIsNotVisibleAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Never;

            m_AlarmServerStateServiceStub.AnyActive += (s, args) => { };
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
        }

        [Test]
        public void AlarmServerEventsStateNone_TestSummaryAlarmControlIsNotVisibleAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Never;

            m_AlarmServerStateServiceStub.AnyActive += (s, args) => { };
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
        }

        #endregion

        #region AlarmServerEventsState.All
        
        [Test]
        public void AlarmServerEventsStateAll_TestCreateSummaryAlarmControl()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Assert.IsNotNull(m_SummaryAlarmControl);
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsVisibleAndActiveWhenAnyAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenNoAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsVisibleAndInactiveWhenAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsVisibleAndInactiveWhenAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsVisibleAndActiveWhenInActiveAndActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateAll_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAllFalse()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Always;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        #endregion

        #region AlarmServerEventsState.Active

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsVisibleAndActiveWhenAnyAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenNoAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsVisibleAndActiveWhenInActiveAndActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            m_AlarmServerStateServiceStub.AnyActive += (s, args) => { };

            Raise.EventWith(new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateActive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAllFalse()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Active;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        #endregion

        #region AlarmServerEventsState.Inactive

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsNotVisibleAndActiveWhenAnyAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsVisibleAndInactiveWhenAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenNoAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsVisibleAndActiveWhenInActiveAndActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateInactive_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAllFalse()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Inactive;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        #endregion

        #region AlarmServerEventsState.Acknowledged
        
        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsNotVisibleAndActiveWhenAnyAlarmActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAlarmInactive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsVisibleAndInactiveWhenAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenNoAlarmAcknowledge()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsVisibleAndActiveWhenAcknowledgedAndActive()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));

            Assert.IsTrue(m_SummaryAlarmControl.Notified, "SummaryAlarm is not shown");
            Assert.IsTrue(m_SummaryAlarmControl.IsActive, "Summary alarm is not active");
        }

        [Test]
        public void AlarmServerEventsStateAcknowledged_TestSummaryAlarmControlIsNotVisibleAndInactiveWhenAllFalse()
        {
            m_SummaryAlarmControl.Mode = AlarmServerAlarmEventsState.Acknowledged;

            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(true));
            Raise.EventWith(m_AlarmServerStateServiceStub, new ValueChangedEventArgs(false));

            Assert.IsFalse(m_SummaryAlarmControl.Notified, "SummaryAlarm is shown");
            Assert.IsFalse(m_SummaryAlarmControl.IsActive, "Summary alarm is active");
        }

        #endregion
    }
}