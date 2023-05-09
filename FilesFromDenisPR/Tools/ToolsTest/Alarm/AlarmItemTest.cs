using System;
using System.ComponentModel;
using Core.Api.DataSource;
using Core.Api.Tools;
using Core.Component.Api.Instantiation;
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
    public class AlarmItemTest
    {
        private bool m_ActiveAlarm;
        private AlarmServer m_AlarmServer;
        private AlarmServerStateService m_AlarmServerStateService;
        private IAlarmServerStorage m_AlarmServerStorage;

        private AlarmItem CreateNewAlarmItem()
        {
            AlarmItem alarmItem = new AlarmItem();
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);
            alarmItem.Value = 0;
            ((IAlarmItem)alarmItem).EnableValueInput = true;
            return alarmItem;
        }

        [SetUp]
        public void SetUp()
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(true);
            m_AlarmServerStateService = new AlarmServerStateService();
            TestHelper.AddService<IAlarmServerStateService>(m_AlarmServerStateService);
            TestHelper.AddService<IRemoteAlarmServerStateService>(m_AlarmServerStateService);
            TestHelper.AddServiceStub<IStorageCacheService>();

            ISecurityServiceCF securityService = TestHelper.AddServiceStub<ISecurityServiceCF>();
            securityService.CurrentUser.Returns(string.Empty);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.LocalTime.Returns(DateTime.Now);

            m_AlarmServerStorage = Substitute.For<IAlarmServerStorage>();
            var eventFactory = new AlarmEventFactory();
            eventFactory.AddAlarmEventTypeProvider("AlarmEvent", () => new AlarmEvent()); // register default AlarmEvent
            m_AlarmServer = new ExtendedAlarmServer(m_AlarmServerStorage, eventFactory);
            m_AlarmServer.AlarmGroups.AddNew();
            m_AlarmServer.IsEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
            m_AlarmServer.Dispose();
        }

        [Test]
        public void ActiveAlarm()
        {
            m_ActiveAlarm = false;
            m_AlarmServer.AnyActive += OnAlarmServerAnyActive;

            AlarmItem alarmItemOne = CreateNewAlarmItem();
            alarmItemOne.TriggerValue = 10;
            alarmItemOne.ComparerType = ComparerTypes.GreaterThan;

            AlarmItem alarmItemTwo = CreateNewAlarmItem();
            alarmItemTwo.TriggerValue = 20;
            alarmItemTwo.ComparerType = ComparerTypes.EqualTo;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemOne);
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemTwo);

            Assert.AreEqual(false, m_ActiveAlarm);

            alarmItemOne.Value = 15;
            Assert.AreEqual(true, m_ActiveAlarm);

            alarmItemOne.Value = 5;
            Assert.AreEqual(false, m_ActiveAlarm);

            alarmItemOne.Value = 15;
            alarmItemTwo.Value = 20;
            Assert.AreEqual(true, m_ActiveAlarm);

            alarmItemOne.Value = 5;
            Assert.AreEqual(true, m_ActiveAlarm);

            alarmItemTwo.Value = 5;
            Assert.AreEqual(false, m_ActiveAlarm);
        }

        private void OnAlarmServerAnyActive(object sender, ValueChangedEventArgs e)
        {
            m_ActiveAlarm = (bool)e.Value;
        }

        [Test]
        public void AddAlarmToGroup()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);
            Assert.AreEqual(m_AlarmServer.AlarmGroups[0].Id, alarmItem.GroupId, "Item group Id should be set");
        }

        [Test]
        public void RemoveAlarmFromGroup()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups[0].AlarmItems.Count);
            foreach (AlarmItem item in m_AlarmServer.AlarmGroups[0].AlarmItems)
            {
                Assert.AreSame(alarmItem, item);
            }
            m_AlarmServer.AlarmGroups[0].AlarmItems.Remove(alarmItem);
            Assert.AreEqual(0, m_AlarmServer.AlarmGroups[0].AlarmItems.Count);
        }

        [Test]
        public void MoveAlarmFromGroup()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            AlarmGroup group = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(group);
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups[0].AlarmItems.Count);
            Assert.AreEqual(m_AlarmServer.AlarmGroups[0].Id, alarmItem.GroupId, "Item group Id should be set");
            Assert.AreEqual(0, m_AlarmServer.AlarmGroups[1].AlarmItems.Count);
            foreach (AlarmItem item in m_AlarmServer.AlarmGroups[0].AlarmItems)
            {
                Assert.AreSame(alarmItem, item);
            }
            m_AlarmServer.AlarmGroups[0].AlarmItems.Remove(alarmItem);
            m_AlarmServer.AlarmGroups[1].AlarmItems.Add(alarmItem);
            foreach (AlarmItem item in m_AlarmServer.AlarmGroups[1].AlarmItems)
            {
                Assert.AreSame(alarmItem, item);
            }
            Assert.AreEqual(0, m_AlarmServer.AlarmGroups[0].AlarmItems.Count);
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups[1].AlarmItems.Count);
            Assert.AreEqual(m_AlarmServer.AlarmGroups[1].Id, alarmItem.GroupId, "Item group Id should be set to new group");
        }

        [Test]
        public void AddGroup()
        {
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups.Count, "There should be a default group");
            AlarmGroup group = new AlarmGroup();
            m_AlarmServer.AlarmGroups.Add(group);
            Assert.AreEqual(2, m_AlarmServer.AlarmGroups.Count);
        }

        [Test]
        public void RemoveGroup()
        {
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups.Count, "There should be a default group");
            m_AlarmServer.AlarmGroups.Add(new AlarmGroup());
            Assert.AreEqual(2, m_AlarmServer.AlarmGroups.Count, "There should be two groups");
            m_AlarmServer.AlarmGroups.Remove(m_AlarmServer.AlarmGroups[0]);
            Assert.AreEqual(1, m_AlarmServer.AlarmGroups.Count);
        }

        [Test]
        public void AlarmEventState()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.IsDigitalValue = true;
            alarmItem.ComparerType = ComparerTypes.RisingEdge;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);

            alarmItem.Value = true;
            alarmItem.AcknowledgeRequired = false;
            AlarmEventList list = (AlarmEventList)m_AlarmServer.EventList;
            IAlarmEventLight eventOne = list[0];
            Assert.AreEqual(eventOne.State, AlarmState.Active);
            alarmItem.Value = false;
            Assert.AreEqual(eventOne.State, AlarmState.Normal);
        }

        [Test]
        public void AlarmEventStateWithAck()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.IsDigitalValue = true;
            alarmItem.ComparerType = ComparerTypes.RisingEdge;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);

            alarmItem.AcknowledgeRequired = true;
            alarmItem.Value = true;
            AlarmEventList list = (AlarmEventList)m_AlarmServer.EventList;
            IAlarmEventLight eventOne = list[0];
            Assert.AreEqual(eventOne.State, AlarmState.Active);
            alarmItem.Value = false;
            Assert.AreEqual(eventOne.State, AlarmState.Inactive);
        }

        [Test]
        public void AlarmEventMatchesAlarmItem()
        {
            AlarmItem alarmItem = CreateNewAlarmItem();
            alarmItem.IsDigitalValue = true;
            alarmItem.ComparerType = ComparerTypes.RisingEdge;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItem);

            alarmItem.Text = alarmItem.DynamicString.Text = "satans jävla varmt i motor helvetet";
            alarmItem.Value = true;

            AlarmEventList list = (AlarmEventList)m_AlarmServer.EventList;
            IAlarmEventLight eventOne = list[0];
            Assert.AreEqual("satans jävla varmt i motor helvetet", eventOne.DisplayText);
            Assert.AreEqual(1, m_AlarmServer.EventList.Count);
            Assert.AreEqual(AlarmState.Active, eventOne.State);
        }

        [Test]
        public void GetEventsFromAllItems()
        {

            AlarmItem alarmItemOne = CreateNewAlarmItem();
            alarmItemOne.IsDigitalValue = true;
            alarmItemOne.ComparerType = ComparerTypes.RisingEdge;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemOne);
            AlarmItem alarmItemTwo = CreateNewAlarmItem();
            alarmItemTwo.IsDigitalValue = true;
            alarmItemTwo.ComparerType = ComparerTypes.RisingEdge;

            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemTwo);

            alarmItemOne.DynamicString.Text = "satans jävla varmt i motor helvetet";
            alarmItemOne.Value = true;

            alarmItemTwo.DynamicString.Text = "satans jävla kallt i motor helvetet";
            alarmItemTwo.Value = true;
            Assert.AreEqual(2, m_AlarmServer.EventList.Count);
        }

        [Test]
        public void CheckAlarmEventTimes()
        {
            AlarmItem alarmItemOne = CreateNewAlarmItem();
            alarmItemOne.IsDigitalValue = true;
            alarmItemOne.AcknowledgeRequired = false;
            alarmItemOne.ComparerType = ComparerTypes.RisingEdge;
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemOne);

            alarmItemOne.Value = true;
            IBindingList list = m_AlarmServer.EventList;
            Assert.AreEqual(1, list.Count);
            IAlarmEvent eventTwo = (IAlarmEvent)list[0];
            DateTime time = new DateTime();
            Assert.IsTrue(eventTwo.ActiveTime != time, "Time is not set");
            Assert.IsTrue(eventTwo.ActiveTime <= DateTime.Now, "Check that event has a time set");
            alarmItemOne.Value = false;
            Assert.IsTrue(eventTwo.NormalTime != time, "Time is not set");
            Assert.IsTrue(eventTwo.NormalTime <= DateTime.Now, "Check that event has a time set");
        }

        [Test]
        public void CheckAlarmAckRequiredAckOnInactive()
        {
            AlarmItem alarmItemOne = CreateNewAlarmItem();
            alarmItemOne.IsDigitalValue = true;
            alarmItemOne.ComparerType = ComparerTypes.RisingEdge;
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemOne);

            alarmItemOne.AcknowledgeRequired = true;
            alarmItemOne.Value = true;
            alarmItemOne.Value = false;
            DateTime time = new DateTime();
            AlarmEventList list = (AlarmEventList)m_AlarmServer.EventList;
            IAlarmEvent eventOne = list[0] as IAlarmEvent;
            Assert.AreEqual(AlarmState.Inactive, eventOne.State);
            Assert.IsTrue(eventOne.InActiveTime != time, "Time is not set");
            Assert.IsTrue(eventOne.InActiveTime <= DateTime.Now, "Check that event has a inactive time set");
            eventOne.Acknowledge();
            Assert.AreEqual(AlarmState.Normal, eventOne.State);
            Assert.IsTrue(eventOne.NormalTime != time, "Time is not set");
            Assert.IsTrue(eventOne.NormalTime <= DateTime.Now, "Check that event has a inactive time set");
        }

        [Test]
        public void CheckAlarmAckRequiredAckOnActive()
        {
            AlarmItem alarmItemOne = CreateNewAlarmItem();
            alarmItemOne.IsDigitalValue = true;
            alarmItemOne.ComparerType = ComparerTypes.RisingEdge;
            m_AlarmServer.AlarmGroups[0].AlarmItems.Add(alarmItemOne);

            alarmItemOne.AcknowledgeRequired = true;
            alarmItemOne.Value = true;
            DateTime time = new DateTime();
            AlarmEventList list = (AlarmEventList)m_AlarmServer.EventList;
            IAlarmEvent eventOne = list[0] as IAlarmEvent;
            Assert.AreEqual(AlarmState.Active, eventOne.State);
            Assert.IsTrue(eventOne.ActiveTime != time, "Time is not set");
            Assert.IsTrue(eventOne.ActiveTime <= DateTime.Now, "Check that event has a inactive time set");
            eventOne.Acknowledge();
            Assert.AreEqual(AlarmState.Acknowledge, eventOne.State);
            Assert.IsTrue(eventOne.AcknowledgeTime != time, "Time is not set");
            Assert.IsTrue(eventOne.AcknowledgeTime <= DateTime.Now, "Check that event has a acknowledge time set");
            alarmItemOne.Value = false;
            Assert.AreEqual(AlarmState.Normal, eventOne.State);
            Assert.IsTrue(eventOne.NormalTime != time, "Time is not set");
            Assert.IsTrue(eventOne.NormalTime <= DateTime.Now, "Check that event has a inactive time set");
            Assert.AreEqual(1, m_AlarmServer.EventList.Count);
        }

        [Test]
        public void TextIsNotCutIfTooLong()
        {
            string longText = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345";
            AlarmItem alarmItemOne = CreateNewAlarmItem();

            alarmItemOne.Text = longText;

            Assert.That(alarmItemOne.Text.Length == longText.Length);
            Assert.That(alarmItemOne.Text == longText);
        }

        [Test]
        public void TextSetterShouldAcceptNull()
        {
            AlarmItem item = new AlarmItem();

            item.Text = null;
        }

        // This is required for language changes to be propagated to AlarmEvents for AlarmItems with dynamic texts. See Case #56860.
        [TestCase("{0}", "{0}")]
        [TestCase("ABC{0}", "DEF{0}")]
        public void AlarmItemDynamicStringTextFiresChangeWhenStaticTextSet(string staticTextBefore, string staticTextAfter)
        {
            // ARRANGE
            var item = new AlarmItem
            {
                StaticText = staticTextBefore
            };

            bool textChanged = false;
            item.DynamicString.Changed += delegate(object sender, EventArgs args) { textChanged = true; };

            // ACT
            item.StaticText = staticTextAfter;

            // ASSERT
            Assert.IsTrue(textChanged);
        }
        

        internal class ExtendedAlarmServer : AlarmServer
        {
            public ExtendedAlarmServer(IAlarmServerStorage alarmServerStorage, IAlarmEventFactory eventFactory)
                : base(alarmServerStorage,
                      new LazyCF<IActionConsumer>(() => new InvokeDirectActionConsumer(ActionConsumerName) as IActionConsumer),
                      Substitute.For<IRootComponentService>().ToILazy(),
                      Substitute.For<ISystemTagServiceCF>().ToILazy(),
                      eventFactory.ToILazy())
            {
                base.m_Initialized = true;
            }
        }
    }
}
