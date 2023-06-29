using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Alarm;
using NSubstitute;
using NUnit.Framework;

#pragma warning disable 67 //The event is never used

namespace Neo.ApplicationFramework.Controls.Logic
{
    [TestFixture]
    public class AlarmViewerViewModelTest
    {
        private IAlarmClient m_AlarmClient;
        private AlarmViewerViewModel m_AlarmViewerLogic;

        private IAlarmViewerGUI m_AlarmViewerGui;
        private IBindingList m_AlarmEventListOneAlarm;
        private IBindingList m_AlarmEventListFourAlarms;
        private bool m_PropertyChangeFired;
        private IAlarmClientService m_AlarmClientService;
        private IAlarmServer m_AlarmServer;

        [SetUp]
        public void Setup()
        {
            m_AlarmViewerGui = Substitute.For<IAlarmViewerGUI>();
            m_PropertyChangeFired = false;

            m_AlarmServer = Substitute.For<IAlarmServer>();
            var globalReferenceService = Substitute.For<IGlobalReferenceService>();
            var lazyMessageBoxService = Substitute.For<IMessageBoxServiceCF>().ToILazy();

            globalReferenceService.GetObject<IAlarmServer>(ApplicationConstantsCF.AlarmServerReferenceName).Returns(m_AlarmServer);

            m_AlarmViewerLogic = new AlarmViewerViewModel(1000, globalReferenceService, lazyMessageBoxService);
            m_AlarmViewerLogic.AlarmViewerGui = m_AlarmViewerGui;
            ((INotifyPropertyChanged)m_AlarmViewerLogic).PropertyChanged += OnAlarmViewerLogicPropertyChanged;

            m_AlarmClient = Substitute.For<IAlarmClient>();

            m_AlarmClientService = Substitute.For<IAlarmClientService>();
            m_AlarmClientService.GetClient(Arg.Any<string>()).Returns(m_AlarmClient);
            m_AlarmViewerLogic.AlarmClientService = m_AlarmClientService;

            SetupAlarmEventLists();
        }

        [TearDown]
        public void TearDown()
        {
            ((INotifyPropertyChanged)m_AlarmViewerLogic).PropertyChanged -= OnAlarmViewerLogicPropertyChanged;
        }

        [Test]
        public void AlarmViewerSupportsInitialize()
        {
            Assert.IsNotNull(m_AlarmViewerLogic, "Could not create AlarmViewerLogic");
        }

        [Test]
        public void TestGetEventList()
        {
            m_AlarmClient.GetAllAlarmEvents().Returns(new List<IAlarmEventLight>());
            m_AlarmViewerLogic.ClientId = "Screen1.MyAlarmServer";

            Assert.IsNull(m_AlarmViewerLogic.AlarmEvents);
        }

        [Test]
        public void TestAckAllAlarmsWithNoAlarmServer()
        {
            m_AlarmViewerLogic.Acknowledge(m_AlarmEventListOneAlarm);
            Assert.IsTrue(true, "Tests that no exceptions are thrown");
        }

        [Test]
        public void TestAckAllAlarms()
        {
            InitializeClient();

            m_AlarmViewerLogic.Acknowledge(m_AlarmEventListOneAlarm);

            Assert.IsTrue(m_PropertyChangeFired, "No property changed fired");
            m_AlarmClient.Received().Acknowledge(((IAlarmEvent)m_AlarmEventListOneAlarm[0]).Id);
        }

        [Test]
        public void TestAckMultipleAlarm()
        {
            InitializeClient();

            m_AlarmViewerLogic.Acknowledge(m_AlarmEventListFourAlarms);

            Assert.IsTrue(m_PropertyChangeFired, "No property changed fired");
            m_AlarmClient.Received().Acknowledge(((IAlarmEvent)m_AlarmEventListFourAlarms[0]).Id);
            m_AlarmClient.Received().Acknowledge(((IAlarmEvent)m_AlarmEventListFourAlarms[1]).Id);
            m_AlarmClient.Received().Acknowledge(((IAlarmEvent)m_AlarmEventListFourAlarms[2]).Id);
            m_AlarmClient.Received().Acknowledge(((IAlarmEvent)m_AlarmEventListFourAlarms[3]).Id);
        }

        [Test]
        public void TestClearNormalAlarms()
        {
            InitializeClient();

            m_AlarmViewerLogic.ClearNormalAlarms();

            Assert.IsTrue(m_PropertyChangeFired, "No property changed fired");
            m_AlarmClient.Received().Clear();
        }

        [Test]
        public void AcknowledgeNegativeTest()
        {
            m_AlarmViewerLogic.Acknowledge(null);
        }

        #region MaxAlarmViewerRows

        [Test]
        public void FiredEventInServerGeneratesRowInLogic()
        {
            InitializeClient();

            Assert.AreEqual(m_AlarmEventListFourAlarms.Count, m_AlarmViewerLogic.FilteredAlarmEvents.Count);
        }

        [Test]
        public void ListShallNotExceedMaximumNumberOfAlarmViewerRows()
        {
            Initialize(2);
            Assert.AreEqual(2, m_AlarmViewerLogic.FilteredAlarmEvents.Count);
            Assert.AreEqual(m_AlarmEventListFourAlarms.Count, m_AlarmViewerLogic.AlarmEvents.Count);
        }

        [Test]
        public void ShownRowsHasRightStatus()
        {
            Initialize(4);
            Assert.AreEqual(AlarmState.Active, ((IAlarmEventLight)m_AlarmViewerLogic.FilteredAlarmEvents[0]).State);
            Assert.AreEqual(AlarmState.Active, ((IAlarmEventLight)m_AlarmViewerLogic.FilteredAlarmEvents[1]).State);
            Assert.AreEqual(AlarmState.Inactive, ((IAlarmEventLight)m_AlarmViewerLogic.FilteredAlarmEvents[2]).State);
            Assert.AreEqual(AlarmState.Normal, ((IAlarmEventLight)m_AlarmViewerLogic.FilteredAlarmEvents[3]).State);
        }

        [Test]
        public void RightNumberOfDifferentAlarmTypesEvenIfLimitedRows()
        {
            Initialize(2);
            Assert.AreEqual(2, m_AlarmViewerLogic.NumberOfActive, "Wrong number of active alarms");
            Assert.AreEqual(1, m_AlarmViewerLogic.NumberOfInactive, "Wrong number of inactive alarms");
            Assert.AreEqual(1, m_AlarmViewerLogic.NumberOfNormal, "Wrong number of normal alarms");
        }

        [Test]
        public void DoNotShowErrorIfAlarmServerIsLocal()
        {
            Assert.False(m_AlarmViewerLogic.ShowErrorIfAlarmServerIsRemote(string.Empty, "Test Error Message"));
        }

        [Test]
        public void ShowErrorIfAlarmServerIsRemote()
        {
            string errorMessage = "Test Error Message";
            TestHelper.AddServiceStub<IMessageBoxServiceCF>();
            var messageBoxService = ServiceContainerCF.GetServiceSafe<IMessageBoxServiceCF>();

            Assert.True(m_AlarmViewerLogic.ShowErrorIfAlarmServerIsRemote("1.1.1.1", "Test Error Message"));

            messageBoxService.Received().Show(errorMessage, "Error", MessageBoxButtons.OK, DialogResult.OK);
        }

        #endregion

        #region Pause/refresh

        [Test]
        public void ChangeInPauseRefreshShallFirePropertyChanged()
        {
            m_AlarmViewerLogic.PauseRefresh = !m_AlarmViewerLogic.PauseRefresh;
            Assert.IsTrue(m_PropertyChangeFired, "No property changed fired");
        }

        [Test]
        public void WhenClearThePausedAlarmViewShallBeUnPaused()
        {
            Initialize(2);

            m_AlarmViewerLogic.PauseRefresh = false;
            m_AlarmViewerLogic.ClearNormalAlarms();

            Assert.IsFalse(m_AlarmViewerLogic.PauseRefresh);
            m_AlarmClient.Received().Clear();
        }

        #endregion


        #region Enable/Disable

        [Test]
        public void UpdateNumberOfDisabledAlarmsShallFirePropertyChanged()
        {
            m_AlarmViewerLogic.UpdateNumberOfDisabledAlarms(2);
            Assert.IsTrue(m_PropertyChangeFired, "CombinedAlarmStatusText");
        }

        [Test]
        public void UpdateNumberOfDisabledAlarmsShallChangePropertyOnAlarmServer()
        {
            m_AlarmViewerLogic.UpdateNumberOfDisabledAlarms(32);
            Assert.IsTrue(m_AlarmServer.NumberOfDisabledAlarms == 32);
        }

        [Test]
        public void TestChangeIsEnabledStateOnSelectedToTrue()
        {
            m_AlarmViewerLogic.ChangeIsEnabledOnSelected(m_AlarmEventListFourAlarms, true);
            foreach (IAlarmEvent alarmEvent in m_AlarmEventListFourAlarms)
            {
                Assert.IsTrue(alarmEvent.AlarmItem.IsEnabledItem);
            }
        }

        [Test]
        public void TestChangeIsEnabledStateOnSelectedToFalse()
        {
            m_AlarmViewerLogic.ChangeIsEnabledOnSelected(m_AlarmEventListFourAlarms, false);
            foreach (IAlarmEvent alarmEvent in m_AlarmEventListFourAlarms)
            {
                Assert.IsFalse(alarmEvent.AlarmItem.IsEnabledItem);
            }
        }

        [Test]
        public void AssertNumberOfDisabledAlarmsInsAlarmServerAfterFourAlarmEventsAreDisabled()
        {
            m_AlarmServer.NumberOfDisabledAlarms = 0;

            Assert.IsTrue(m_AlarmServer.NumberOfDisabledAlarms == 0);

            m_AlarmViewerLogic.ChangeIsEnabledOnSelected(m_AlarmEventListFourAlarms, false);

            Assert.IsTrue(m_AlarmServer.NumberOfDisabledAlarms == 4);
        }

        #endregion

        private void SetupAlarmEventLists()
        {
            m_AlarmEventListOneAlarm = new BindingList<IAlarmEventLight>();
            m_AlarmEventListOneAlarm.Add(new TestAlarmEvent(Guid.NewGuid()));

            m_AlarmEventListFourAlarms = new BindingList<IAlarmEventLight>();
            m_AlarmEventListFourAlarms.Add(new TestAlarmEvent(Guid.NewGuid(), AlarmState.Inactive));
            m_AlarmEventListFourAlarms.Add(new TestAlarmEvent(Guid.NewGuid()));
            m_AlarmEventListFourAlarms.Add(new TestAlarmEvent(Guid.NewGuid()));
            m_AlarmEventListFourAlarms.Add(new TestAlarmEvent(Guid.NewGuid(), AlarmState.Normal));
        }

        private void Initialize(int maximumAlarmViewerRows)
        {
            IList<IAlarmEventLight> alarmEventListFourAlarmsGeneric = m_AlarmEventListFourAlarms.CastTo<IList<IAlarmEventLight>>();
            m_AlarmClient.AlarmEvents.Returns(m_AlarmEventListFourAlarms);
            m_AlarmClient.GetAllAlarmEvents().Returns(alarmEventListFourAlarmsGeneric);

            var alarmGroup = Substitute.For<IAlarmGroupInfo>();
            alarmGroup.Name.Returns("AlarmGroup1");

            m_AlarmClient.AlarmGroups.Returns(new List<IAlarmGroupInfo>() { alarmGroup });

            m_AlarmViewerLogic.MaximumAlarmViewerRows = maximumAlarmViewerRows;
            m_AlarmViewerLogic.ClientId = "Screen1.MyAlarmServer";
        }

        private void InitializeClient()
        {
            Initialize(100);
        }

        private void OnAlarmViewerLogicPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            m_PropertyChangeFired = true;
        }

        private class TestAlarmEvent : IAlarmEvent
        {
            private readonly Guid m_Guid;
            private AlarmState m_State;
            private IAlarmItem m_AlarmItem;

            public TestAlarmEvent(Guid guid, AlarmState state)
            {
                m_Guid = guid;
                m_State = state;

                m_AlarmItem = new AlarmItem();
            }

            public TestAlarmEvent(Guid guid)
                : this(guid, AlarmState.Active)
            {
            }

            #region IAlarmEvent Members

            void IAlarmEvent.Acknowledge()
            {
                throw new NotImplementedException();
            }

            void IAlarmEvent.Acknowledge(string userName)
            {
                throw new NotImplementedException();
            }

            bool IAlarmEvent.SetupNewAlarmEvent(IAlarmServer alarmServer, IAlarmEventEntry alarmEventEntry)
            {
                throw new NotImplementedException();
            }

            IAlarmItem IAlarmEvent.AlarmItem
            {
                get { return m_AlarmItem; }
            }

            DateTime? IAlarmEventLight.AcknowledgeTime
            {
                get { return null; }
                set { throw new NotImplementedException(); }
            }

            DateTime? IAlarmEventLight.ActiveTime
            {
                get { return null; }
                set { throw new NotImplementedException(); }
            }

            Guid IAlarmEvent.AlarmItemId
            {
                get { return m_Guid; }
                set { throw new NotImplementedException(); }
            }

            Guid IAlarmEventLight.Id
            {
                get { return m_Guid; }
            }

            DateTime? IAlarmEventLight.InActiveTime
            {
                get { return null; }
                set { throw new NotImplementedException(); }
            }

            DateTime? IAlarmEventLight.NormalTime
            {
                get { return null; }
                set { throw new NotImplementedException(); }
            }

            AlarmState IAlarmEventLight.State
            {
                get { return m_State; }
                set { m_State = value; }
            }

            string IAlarmEventLight.DisplayState
            {
                get { return m_State.ToString(); }
            }

            bool IAlarmEvent.History
            {
                get { throw new NotImplementedException(); }

            }

            string IAlarmEvent.Text
            {
                get { throw new NotImplementedException(); }
            }

            string IAlarmEventLight.DisplayText
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            string IAlarmEventLight.AlarmGroupName
            {
                get { return "AlarmGroup1"; }
            }

            string IAlarmEventLight.AlarmGroupText
            {
                get { return "AlarmGroup1"; }
            }
            string IAlarmEventLight.AlarmItemDisplayName
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            void IAlarmEvent.Init()
            {
                throw new NotImplementedException();
            }

            void IAlarmEvent.Add()
            {
                throw new NotImplementedException();
            }

            int IAlarmEventLight.Count
            {
                get { return 1; }
                set { throw new NotImplementedException(); }
            }

            int IAlarmEventLight.SortIndex
            {
                get { return 0; }
                set { throw new NotImplementedException(); }
            }

            void IAlarmEvent.AlarmOff()
            {
                throw new NotImplementedException();
            }

            System.Drawing.SolidBrush IAlarmEventLight.BackBrush
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            System.Drawing.SolidBrush IAlarmEventLight.ForeBrush
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            string IAlarmEvent.ToString()
            {
                throw new NotImplementedException();
            }

            void IAlarmEvent.Delete()
            {
                throw new NotImplementedException();
            }

            bool IAlarmEvent.RepeatCount
            {
                get { throw new NotImplementedException(); }
            }

            bool IAlarmEvent.EnableDistribution
            {
                get { throw new NotImplementedException(); }
            }

            bool IAlarmEvent.ForceToList
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Type AlarmEventEntryType
            {
                get { throw new NotImplementedException(); }
            }

            DateTime IAlarmEvent.GetDateTimeForAlarmState()
            {
                throw new NotImplementedException();
            }

            bool IAlarmEvent.KeepAtStartup
            {
                get { throw new NotImplementedException(); }
            }

            void IAlarmEvent.PrepareForDatabaseExport()
            {
                throw new NotImplementedException();
            }

            bool IAlarmEvent.FireAlarmEventInfoRequestedEvent()
            {
                throw new NotImplementedException();
            }

            string IAlarmEvent.UpdateDisplayText()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region ICloneable Members

            object ICloneable.Clone()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IStorableItem Members

            string IStorableItem.TableName
            {
                get { throw new NotImplementedException(); }
            }

            object IStorableItem.PrimaryKey
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}
