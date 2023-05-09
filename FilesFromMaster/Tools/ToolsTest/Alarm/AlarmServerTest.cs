using System;
using System.Threading;
using System.Windows.Forms;
using Core.Api.DataSource;
using Core.Api.Tools;
using Core.Component.Api.Instantiation;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Storage.Threading;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class AlarmServerTest
    {
        private AlarmServer m_AlarmServer;
        private AlarmServerStateService m_AlarmServerStateService;
        private MockRepository m_MockRepository;
        private IAlarmServerStorage m_AlarmServerStorage;
        private AlarmGroup m_AlarmGroup;

        private AlarmItem CreateNewAlarmItem()
        {
            AlarmItem alarmItem = new AlarmItem();
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);
            alarmItem.Value = 0;
            ((IAlarmItem)alarmItem).EnableValueInput = true;
            return alarmItem;
        }

        [SetUp]
        public void Setup()
        {
            m_MockRepository = new MockRepository();

            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(true);

            ISecurityServiceCF securityService = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityService.Stub(x => x.CurrentUser).Return(string.Empty);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.Stub(x => x.LocalTime).WhenCalled(y => y.ReturnValue = DateTime.Now).Return(DateTime.Now);

            m_AlarmServerStateService = new AlarmServerStateService();
            TestHelper.AddService<IAlarmServerStateService>(m_AlarmServerStateService);
            TestHelper.AddService<IRemoteAlarmServerStateService>(m_AlarmServerStateService);
            TestHelper.AddServiceStub<IStorageCacheService>();

            m_AlarmServerStorage = m_MockRepository.Stub<IAlarmServerStorage>();
            var eventFactory = new AlarmEventFactory();
            eventFactory.AddAlarmEventTypeProvider("AlarmEvent", () => new AlarmEvent()); // register default AlarmEvent
            m_AlarmServer = new ExtendedAlarmServer(m_AlarmServerStorage, eventFactory) { IsEnabled = true };
            m_AlarmServer.MinimumItemsMultiplier = 1.0;
            m_AlarmServer.IsEnabled = true;
            m_AlarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(m_AlarmGroup);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();

            m_AlarmServer.Dispose();
        }

        [Test]
        public void TestCreateInstanceOfAlarmServer()
        {
            Assert.IsNotNull(m_AlarmServer, "Could not create an instance of AlarmServer");
        }

        [Test]
        public void TestAddAlarmEvent()
        {
            Assert.AreEqual(0, m_AlarmServer.EventCount);
            AlarmItem alarmItem = CreateNewAlarmItem();
            AlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            Assert.AreEqual(1, m_AlarmServer.EventCount);
        }

        [Test]
        public void TestMultipleAddAlarmEvent()
        {
            Assert.AreEqual(0, m_AlarmServer.EventCount);

            for (int alarmCount = 0; alarmCount < 10; alarmCount++)
            {
                AlarmItem alarmItem = CreateNewAlarmItem();
                AlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            }

            Assert.AreEqual(10, m_AlarmServer.EventCount);
        }

        [Test]
        public void TestAddEventFiresActiveEvent()
        {
            bool eventReceived = false;

            m_AlarmServer.AlarmActive += delegate(object sender, EventArgs e)
            {
                eventReceived = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            AlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            Application.DoEvents();
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void TestAckEventFiresAckEvent()
        {
            bool eventReceived = false;

            m_AlarmServer.AlarmAcknowledge += delegate(object sender, EventArgs e)
            {
                eventReceived = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.Acknowledge();
            Application.DoEvents();
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void TestDeleteAllNormalAlarms()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEventFirst = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEventSecond = new AlarmEvent(alarmItem);

            Assert.AreEqual(2, m_AlarmServer.EventCount);

            alarmEventFirst.AlarmOff();
            alarmEventSecond.AlarmOff();

            alarmEventFirst.Acknowledge();
            alarmEventSecond.Acknowledge();

            Assert.AreEqual(2, m_AlarmServer.EventCount);
            ((IAlarmServer)m_AlarmServer).ClearNormalAlarms();
            Assert.AreEqual(0, m_AlarmServer.EventCount);
            Assert.AreEqual(0, m_AlarmServer.EventList.Count);
        }

        [Test]
        public void DeleteNormalAlarmsBeforeAcknowledged()
        {
            m_AlarmServer.MaxNumberOfAlarms = 2;

            AlarmItem alarmItem1 = CreateNewAlarmItem();
            IAlarmEvent alarmEventAcknowledged = new AlarmEvent(alarmItem1);
            Thread.Sleep(50);
            alarmEventAcknowledged.Acknowledge();

            AlarmItem alarmItem2 = CreateNewAlarmItem();
            IAlarmEvent alarmEventNormal1 = new AlarmEvent(alarmItem2);
            Thread.Sleep(50);
            alarmEventNormal1.AlarmOff();
            alarmEventNormal1.Acknowledge();

            AlarmItem alarmItem3 = CreateNewAlarmItem();
            AlarmEvent alarmEventActive = new AlarmEvent(alarmItem3);

            Assert.AreEqual(2, m_AlarmServer.EventCount);
            Assert.IsTrue(m_AlarmServer.EventList.Contains(alarmEventAcknowledged));
            Assert.IsTrue(m_AlarmServer.EventList.Contains(alarmEventActive));

        }

        [Test]
        public void DeleteInactiveAlarmsBeforeAcknowledged()
        {
            m_AlarmServer.MaxNumberOfAlarms = 2;

            AlarmItem alarmItem1 = CreateNewAlarmItem();
            IAlarmEvent alarmEventInactive = new AlarmEvent(alarmItem1);
            Thread.Sleep(100);
            alarmEventInactive.AlarmOff();

            AlarmItem alarmItem2 = CreateNewAlarmItem();
            IAlarmEvent alarmEventAcknowledged = new AlarmEvent(alarmItem2);
            Thread.Sleep(100);
            alarmEventAcknowledged.Acknowledge();

            AlarmItem alarmItem3 = CreateNewAlarmItem();
            IAlarmEvent alarmEventActive = new AlarmEvent(alarmItem3);
            Thread.Sleep(100);

            Assert.AreEqual(2, m_AlarmServer.EventCount);

            Assert.IsTrue(m_AlarmServer.EventList.Contains(alarmEventAcknowledged));
            Assert.IsTrue(m_AlarmServer.EventList.Contains(alarmEventActive));

        }

        [Test]
        public void TestOldestActiveAlarmIsDeletedAllActive()
        {
            m_AlarmServer.MaxNumberOfAlarms = 2;
            AlarmItem alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            Assert.AreEqual(2, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            Assert.AreEqual(2, m_AlarmServer.EventCount);

            foreach (IAlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                if (alarmEvent == alarmEvent1)
                    Assert.Fail("Oldest active alarm not removed");
            }
        }

        [Test]
        public void TestOldestAcknowledgedAlarmIsDeletedAllAcknowledged()
        {
            m_AlarmServer.MaxNumberOfAlarms = 2;
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmEvent1.Acknowledge();
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmEvent2.Acknowledge();
            Assert.AreEqual(2, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent3.Acknowledge();
            Assert.AreEqual(2, m_AlarmServer.EventCount);


            foreach (AlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                if (alarmEvent == alarmEvent1)
                    Assert.Fail("Oldest acknowledged alarm not removed");
            }
        }

        [Test]
        public void TestOldestNormalAlarmIsDeletedAllNormal()
        {
            m_AlarmServer.MaxNumberOfAlarms = 2;
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmEvent1.AlarmOff();
            alarmEvent1.Acknowledge();
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmEvent2.AlarmOff();
            alarmEvent2.Acknowledge();
            Assert.AreEqual(2, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            Thread.Sleep(50);
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent3.AlarmOff();
            alarmEvent3.Acknowledge();
            Assert.AreEqual(2, m_AlarmServer.EventCount);


            foreach (IAlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                if (alarmEvent == alarmEvent1)
                    Assert.Fail("Oldest normal alarm not removed");
            }
        }

        [Test]
        public void TestOldestNormalAlarmIsDeletedWhenNewestIsOnlyNormal()
        {
            m_AlarmServer.MaxNumberOfAlarms = 3;

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent3.AlarmOff();
            alarmEvent3.Acknowledge();
            Assert.AreEqual(3, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent4 = new AlarmEvent(alarmItem);
            Assert.AreEqual(3, m_AlarmServer.EventCount);


            foreach (IAlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                if (alarmEvent == alarmEvent3)
                    Assert.Fail("Oldest normal alarm not removed");
            }
        }

        [Test]
        public void TestOldestNormalAlarmIsDeletedWhenTwoNewestIsAcknowledgedAndNormal()
        {
            m_AlarmServer.MaxNumberOfAlarms = 3;

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent3.Acknowledge();
            Thread.Sleep(100);
            alarmEvent2.AlarmOff();
            alarmEvent2.Acknowledge();
            Assert.AreEqual(3, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent4 = new AlarmEvent(alarmItem);
            Assert.AreEqual(3, m_AlarmServer.EventCount);


            foreach (AlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                // AlarmEvent2 is the only normal alarm
                if (alarmEvent == alarmEvent2)
                    Assert.Fail("Oldest normal alarm not removed when one ack and one normal alarm exists");
            }
        }

        [Test]
        public void TestOldestNormalAlarmIsDeletedWhenTwoNewestIsNormalAndAcknowledged()
        {
            m_AlarmServer.MaxNumberOfAlarms = 3;

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent2.Acknowledge();
            Thread.Sleep(100);
            alarmEvent3.AlarmOff();
            alarmEvent3.Acknowledge();
            Assert.AreEqual(3, m_AlarmServer.EventCount);
            alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent4 = new AlarmEvent(alarmItem);
            Assert.AreEqual(3, m_AlarmServer.EventCount);


            foreach (IAlarmEvent alarmEvent in m_AlarmServer.EventList)
            {
                // AlarmEvent3 is the only normal alarm
                if (alarmEvent == alarmEvent3)
                    Assert.Fail("Oldest normal alarm not removed when one ack and one normal alarm exists");
            }
        }

        [Test]
        public void TestThatANonEnabledAlarmServerDoesNotRecieveAnAlarmEvent()
        {
            m_AlarmServer.IsEnabled = false;
            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            IAlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.TriggerValue = 10;
            alarmItem.ComparerType = ComparerTypes.GreaterThan;

            alarmGroup.AlarmItems.Add(alarmItem);

            alarmItem.Value = 15;

            Assert.AreEqual(0, m_AlarmServer.EventCount);
        }

        [Test]
        public void TestThatANonEnabledAlarmItemDoesSendEventsToAlarmServer()
        {
            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            IAlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.IsEnabled = false;
            alarmItem.TriggerValue = 10;
            alarmItem.ComparerType = ComparerTypes.GreaterThan;

            alarmGroup.AlarmItems.Add(alarmItem);

            alarmItem.Value = 15;

            Assert.AreEqual(0, m_AlarmServer.EventCount);
        }

        [Test]
        public void TestThatAlarmEventFromANonEnabledAlarmGroupDoesNotSendEventsToAlarmServer()
        {
            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);
            alarmGroup.IsEnabled = false;

            IAlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.TriggerValue = 10;
            alarmItem.ComparerType = ComparerTypes.GreaterThan;

            alarmGroup.AlarmItems.Add(alarmItem);

            alarmItem.Value = 15;

            Assert.AreEqual(0, m_AlarmServer.EventCount);
        }

        [Test]
        public void TestThatNoNewEventIsSentWhenUsingRepeatCountAndStoreLastIsEnabled()
        {
            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            IAlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.TriggerValue = 10;
            alarmItem.RepeatCount = true;
            alarmItem.IsEnabledItem = true;
            alarmItem.ComparerType = ComparerTypes.GreaterThan;
            m_AlarmServer.StoreType = StoreTypes.StoreLast;

            alarmGroup.AlarmItems.Add(alarmItem);

            alarmItem.Value = 15;
            Assert.AreEqual(1, m_AlarmServer.EventCount);
            DateTime firstActiveTime = ((AlarmEvent)m_AlarmServer.EventList[0]).ActiveTime ?? DateTime.MaxValue;
            Thread.Sleep(50);

            alarmItem.Value = 5;
            alarmItem.Value = 15;
            Assert.AreEqual(1, m_AlarmServer.EventCount, "Wrong number of events in alarm server");
            DateTime secondActiveTime = ((AlarmEvent)m_AlarmServer.EventList[0]).ActiveTime ?? DateTime.MaxValue;

            Assert.IsTrue(firstActiveTime < secondActiveTime, "Active time has not been updated");
        }

        [Test]
        public void TestThatNoNewEventIsSentWhenUsingRepeatCountAndStoreFirstIsEnabled()
        {
            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            IAlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.TriggerValue = 10;
            alarmItem.RepeatCount = true;
            alarmItem.IsEnabledItem = true;
            alarmItem.ComparerType = ComparerTypes.GreaterThan;
            m_AlarmServer.StoreType = StoreTypes.StoreFirst;

            alarmGroup.AlarmItems.Add(alarmItem);

            alarmItem.Value = 15;
            Assert.AreEqual(1, m_AlarmServer.EventCount);
            DateTime firstActiveTime = ((AlarmEvent)m_AlarmServer.EventList[0]).ActiveTime ?? DateTime.MaxValue;
            Thread.Sleep(10);

            alarmItem.Value = 5;
            alarmItem.Value = 15;
            Assert.AreEqual(1, m_AlarmServer.EventCount);
            DateTime secondActiveTime = ((AlarmEvent)m_AlarmServer.EventList[0]).ActiveTime ?? DateTime.MinValue;

            Assert.AreEqual(firstActiveTime, secondActiveTime);
        }

        [Test]
        public void TestNumberOfUnacknowledgedAlarms()
        {
            int expectedResult = 1;

            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItem);

            IAlarmEvent alarmEvent1 = new AlarmEvent(alarmItem);
            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent2.Acknowledge();
            Thread.Sleep(100);
            alarmEvent3.AlarmOff();
            alarmEvent3.Acknowledge();

            Assert.AreEqual(expectedResult, m_AlarmServer.GetUnAcknowledgedAlarmEvents(alarmItem.Id).Count);
        }

        [Test]
        public void TestThatAnyActiveEventIsFired()
        {
            bool eventIsFired = false;
            bool eventParam = false;

            m_AlarmServer.AnyActive += delegate(object o, ValueChangedEventArgs args)
            {
                eventParam = (bool)args.Value;
                eventIsFired = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            AlarmEvent alarmEvent = new AlarmEvent(alarmItem);

            Thread.Sleep(10);

            Assert.IsTrue(eventIsFired, "AnyActive event is not fired");
            Assert.IsTrue(eventParam, "The alarm server does not have any active alarms");
        }

        [Test]
        public void TestThatAnyAcknowledgedEventIsFired()
        {
            bool eventIsFired = false;
            bool eventParam = false;

            m_AlarmServer.AnyAcknowledged += delegate(object o, ValueChangedEventArgs args)
            {
                eventParam = (bool)args.Value;
                eventIsFired = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.Acknowledge();

            Thread.Sleep(10);

            Assert.IsTrue(eventIsFired, "AnyAcknowledge event is not fired");
            Assert.IsTrue(eventParam, "The alarm server does not have any acknowledged alarms");
        }

        [Test]
        public void TestThatAnyInactiveEventIsFired()
        {
            bool eventIsFired = false;
            bool eventParam = false;

            m_AlarmServer.AnyInactive += delegate(object o, ValueChangedEventArgs args)
            {
                eventParam = (bool)args.Value;
                eventIsFired = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.AlarmOff();

            Thread.Sleep(10);

            Assert.IsTrue(eventIsFired, "AnyInactive event is not fired");
            Assert.IsTrue(eventParam, "The alarm server does not have any inactive alarms");
        }

        [Test]
        public void TestThatNoActiveInactiveAcknowledgeEventExistAfterSequenceActiveInactiveAcknowledge()
        {
            bool activeEventParam = true;
            bool inactiveEventParam = true;
            bool acknowledgedEventParam = true;

            m_AlarmServer.AnyActive += delegate(object o, ValueChangedEventArgs args)
            {
                activeEventParam = (bool)args.Value;
            };

            m_AlarmServer.AnyAcknowledged += delegate(object o, ValueChangedEventArgs args)
            {
                acknowledgedEventParam = (bool)args.Value;
            };

            m_AlarmServer.AnyInactive += delegate(object o, ValueChangedEventArgs args)
            {
                inactiveEventParam = (bool)args.Value;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.AlarmOff();
            alarmEvent.Acknowledge();
            Thread.Sleep(10);

            Assert.IsFalse(activeEventParam);
            Assert.IsFalse(inactiveEventParam);
            Assert.IsFalse(acknowledgedEventParam);
        }

        [Test]
        public void TestThatNoActiveInactiveAcknowledgeEventExistAfterSequenceActiveAcknowledgeInActive()
        {
            bool activeEventParam = true;
            bool inactiveEventParam = true;
            bool acknowledgedEventParam = true;

            m_AlarmServer.AnyActive += delegate(object o, ValueChangedEventArgs args)
            {
                activeEventParam = (bool)args.Value;
            };

            m_AlarmServer.AnyAcknowledged += delegate(object o, ValueChangedEventArgs args)
            {
                acknowledgedEventParam = (bool)args.Value;
            };

            m_AlarmServer.AnyInactive += delegate(object o, ValueChangedEventArgs args)
            {
                inactiveEventParam = (bool)args.Value;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.Acknowledge();
            alarmEvent.AlarmOff();
            Thread.Sleep(10);

            Assert.IsFalse(activeEventParam);
            Assert.IsFalse(inactiveEventParam);
            Assert.IsFalse(acknowledgedEventParam);
        }

        [Test]
        public void TestThatClearNormalAlarmsFiresEvent()
        {
            bool eventIsFired = false;
            m_AlarmServer.AlarmDeleted += delegate(object o, EventArgs args)
            {
                eventIsFired = true;
            };

            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.AlarmOff();
            alarmEvent.Acknowledge();
            Thread.Sleep(10);

            ((IAlarmServer)m_AlarmServer).ClearNormalAlarms();

            Assert.IsTrue(eventIsFired);
        }

        [Test]
        public void TestSummaryAcknowledgedFirstAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.Acknowledge();
            alarmEvent.AlarmOff();

            Application.DoEvents();
            Assert.AreEqual(m_AlarmServer.NumberOfAcknowledgedAlarms, 0);
        }

        [Test]
        public void TestSummaryAcknowledgedFirstInactive()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.AlarmOff();
            alarmEvent.Acknowledge();

            Application.DoEvents();
            Assert.AreEqual(m_AlarmServer.NumberOfAcknowledgedAlarms, 0);
        }

        [Test]
        public void TestSummaryAcknowledgedInactiveAcknowledged()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            IAlarmEvent alarmEvent = new AlarmEvent(alarmItem);
            alarmEvent.AlarmOff();

            IAlarmEvent alarmEvent2 = new AlarmEvent(alarmItem);
            alarmEvent2.AlarmOff();

            IAlarmEvent alarmEvent3 = new AlarmEvent(alarmItem);
            alarmEvent3.AlarmOff();

            ((IAlarmServer)m_AlarmServer).Acknowledge();

            IAlarmEvent alarmEvent4 = new AlarmEvent(alarmItem);
            alarmEvent4.Acknowledge();

            Application.DoEvents();
            Assert.AreEqual(m_AlarmServer.NumberOfAcknowledgedAlarms, 1);
        }

        [Test]
        public void TestCheckingEnableAlarmDistributorServerAlarmItem()
        {
            m_AlarmServer.EnableSendingToAlarmDistributorServer = true;

            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);

            IAlarmItem alarmItemA = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItemA);
            alarmItemA.EnableDistribution = true;

            IAlarmItem alarmItemB = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItemB);
            alarmItemB.EnableDistribution = false;

            IAlarmItem alarmItemC = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItemC);
            alarmItemC.EnableDistribution = true;

            AlarmEvent alarmEventA = new AlarmEvent(alarmItemA);
            AlarmEvent alarmEventB = new AlarmEvent(alarmItemB);
            AlarmEvent alarmEventC = new AlarmEvent(alarmItemC);

            Assert.IsTrue(m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventA));
            Assert.IsTrue(!m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventB));
            Assert.IsFalse(!m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventC));
        }

        [Test]
        public void TestCheckingEnableAlarmDistributorServerAlarmGroupWithAlarmItem()
        {
            m_AlarmServer.EnableSendingToAlarmDistributorServer = true;

            AlarmGroup alarmGroupA = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroupA);
            alarmGroupA.EnableDistribution = true;

            AlarmGroup alarmGroupB = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroupB);
            alarmGroupB.EnableDistribution = false;

            IAlarmItem alarmItemA = CreateNewAlarmItem();
            alarmGroupA.AlarmItems.Add(alarmItemA);
            alarmItemA.EnableDistribution = true;

            IAlarmItem alarmItemB = CreateNewAlarmItem();
            alarmGroupA.AlarmItems.Add(alarmItemB);
            alarmItemB.EnableDistribution = false;

            IAlarmItem alarmItemC = CreateNewAlarmItem();
            alarmGroupB.AlarmItems.Add(alarmItemC);
            alarmItemC.EnableDistribution = true;

            IAlarmItem alarmItemD = CreateNewAlarmItem();
            alarmGroupB.AlarmItems.Add(alarmItemD);
            alarmItemD.EnableDistribution = false;

            AlarmEvent alarmEventA = new AlarmEvent(alarmItemA);
            AlarmEvent alarmEventB = new AlarmEvent(alarmItemB);
            AlarmEvent alarmEventC = new AlarmEvent(alarmItemC);
            AlarmEvent alarmEventD = new AlarmEvent(alarmItemD);

            Assert.IsTrue(m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventA));
            Assert.IsTrue(m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventB));
            Assert.IsTrue(m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventC));
            Assert.IsFalse(m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventD));
        }

        [Test]
        public void TestCheckingEnableSendingToAlarmServer()
        {
            m_AlarmServer.EnableSendingToAlarmDistributorServer = false;

            AlarmGroup alarmGroup = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(alarmGroup);
            alarmGroup.EnableDistribution = true;

            IAlarmItem alarmItemA = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItemA);
            alarmItemA.EnableDistribution = true;

            IAlarmItem alarmItemB = CreateNewAlarmItem();
            alarmGroup.AlarmItems.Add(alarmItemB);
            alarmItemB.EnableDistribution = false;

            AlarmEvent alarmEventA = new AlarmEvent(alarmItemA);
            AlarmEvent alarmEventB = new AlarmEvent(alarmItemB);

            Assert.IsTrue(!m_AlarmServer.IsEnableAlarmDistributorServer(alarmEventA));
        }

        [Test]
        public void TestIfAnyAcknowledgedAfterAckingOneAlarmEvent()
        {
            Assert.IsFalse(m_AlarmServer.IsAnyAcknowledged);

            AlarmItem alarmItem = CreateNewAlarmItem();
            m_AlarmGroup.AlarmItems.Add(alarmItem);
            IAlarmEvent alarmEvent = new AlarmEvent(m_AlarmServer, Guid.NewGuid(), alarmItem.Name, AlarmState.Active, null, null, null, DateTime.Now, string.Empty, 1);

            Assert.IsFalse(m_AlarmServer.IsAnyAcknowledged);

            alarmEvent.Acknowledge();

            Assert.IsTrue(m_AlarmServer.IsAnyAcknowledged);
        }

        [Test]
        public void TestIsAnyActiveBeforeAndAfterAcknowledge()
        {
            Assert.IsFalse(m_AlarmServer.IsAnyActive);

            AlarmItem alarmItem = CreateNewAlarmItem();
            m_AlarmGroup.AlarmItems.Add(alarmItem);
            IAlarmEvent alarmEvent = new AlarmEvent(m_AlarmServer, Guid.NewGuid(), alarmItem.Name, AlarmState.Active, null, null, null, DateTime.Now, string.Empty, 1);

            Assert.IsTrue(m_AlarmServer.IsAnyActive);

            alarmEvent.Acknowledge();

            Assert.IsFalse(m_AlarmServer.IsAnyActive);
        }

        [Test]
        public void TestIsAnyInActive()
        {
            Assert.IsFalse(m_AlarmServer.IsAnyInactive);

            AlarmItem alarmItem = CreateNewAlarmItem();
            m_AlarmGroup.AlarmItems.Add(alarmItem);
            IAlarmEvent alarmEvent = new AlarmEvent(m_AlarmServer, Guid.NewGuid(), alarmItem.Name, AlarmState.Active, null, null, null, DateTime.Now, string.Empty, 1);

            Assert.IsFalse(m_AlarmServer.IsAnyInactive);

            alarmEvent.Acknowledge();

            Assert.IsFalse(m_AlarmServer.IsAnyInactive);
        }

        [Test]
        public void TestIsAnyInAcknowledgeAfterAckingAlarmServer()
        {
            Assert.IsFalse(m_AlarmServer.IsAnyAcknowledged);

            AlarmItem alarmItem = CreateNewAlarmItem();
            m_AlarmGroup.AlarmItems.Add(alarmItem);
            IAlarmEvent alarmEvent = new AlarmEvent(m_AlarmServer, Guid.NewGuid(), alarmItem.Name, AlarmState.Active, null, null, null, DateTime.Now, string.Empty, 1);

            Assert.IsFalse(m_AlarmServer.IsAnyAcknowledged);

            m_AlarmServer.Acknowledge();

            Assert.IsTrue(m_AlarmServer.IsAnyAcknowledged);
            Assert.IsFalse(m_AlarmServer.IsAnyActive);
            Assert.IsTrue(alarmEvent.AcknowledgeTime != null);
        }

        internal class ExtendedAlarmServer : AlarmServer
        {
            public ExtendedAlarmServer(IAlarmServerStorage alarmServerStorage, IAlarmEventFactory alarmEventFactory)
                : base(alarmServerStorage, 
                      new LazyCF<IActionConsumer>(() => new InvokeDirectActionConsumer(ActionConsumerName) as IActionConsumer), 
                      MockRepository.GenerateStub<IRootComponentService>().ToILazy(),
                      MockRepository.GenerateStub<ISystemTagServiceCF>().ToILazy(),
                      alarmEventFactory.ToILazy())
            {
                base.m_Initialized = true;
            }
        }
    }
}
