using System.ComponentModel;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Alarm
{
    [TestFixture]
    public class AlarmItemSorterTest
    {
        private class TestAlarmItem : AlarmItem
        {
            private readonly IAlarmGroup m_AlarmGroup;

            public TestAlarmItem(IAlarmGroup alarmGroup)
            {
                m_AlarmGroup = alarmGroup;
            }

            protected override IAlarmGroup AlarmGroup
            {
                get { return m_AlarmGroup; }
            }

            public int TestExpectedSortIndex { get; set; }
        }

        private class TestAlarmGroup : AlarmGroup
        {
            public int TestGroupIndex { get; set; }
        }

        private IAlarmServer m_AlarmServer;
        private BindingList<IAlarmGroup> m_AlarmGroups;
        private BindingList<IAlarmItem> m_AlarmItems;
        private IGlobalReferenceService m_GlobalReferenceService;
        private IMultiLanguageServiceCF m_MultiLanguageServiceCF;

        [SetUp]
        public void Setup()
        {
            m_MultiLanguageServiceCF = TestHelper.CreateAndAddServiceStub<IMultiLanguageServiceCF>();

            m_AlarmGroups = new BindingList<IAlarmGroup>();
            m_AlarmItems = new BindingList<IAlarmItem>();

            m_AlarmServer = Substitute.For<IAlarmServer>();
            m_AlarmServer.AlarmGroups.Returns(m_AlarmGroups);

            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_GlobalReferenceService.GetObject<IAlarmServer>(ApplicationConstantsCF.AlarmServerReferenceName).Returns(m_AlarmServer);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetSortIndexReturnsCorrectValues()
        {
            using (new SelectSwedishTestingCulture())
            {
                SetupAlarmItems();

                foreach (IAlarmItem alarmItem in m_AlarmItems)
                {
                    int sortIndex = AlarmItemSorter.Instance.GetSortIndex(alarmItem.Id);
                    var testAlarmItem = alarmItem as TestAlarmItem;
                    Assert.AreEqual(testAlarmItem.TestExpectedSortIndex, sortIndex);
                }
            }
        }

        private void SetupAlarmItems()
        {
            m_AlarmGroups.Clear();
            m_AlarmItems.Clear();

            AddAlarmGroup("AlarmGroup_Ä1", 1);
            AddAlarmGroup("AlarmGroup_Ä11", 3);
            AddAlarmGroup("AlarmGroup_Ä2", 2);
            AddAlarmGroup("AlarmGroup_Ö", 4);
            AddAlarmGroup("AlarmGroup_Å", 0);

            foreach (IAlarmGroup alarmGroup in m_AlarmGroups)
            {
                var testAlarmGroup = alarmGroup as TestAlarmGroup;
                AddAlarmItem(testAlarmGroup, "AlarmItem_Ä1", 2);
                AddAlarmItem(testAlarmGroup, "AlarmItem_Ä222", 5);
                AddAlarmItem(testAlarmGroup, "AlarmItem_Ä11", 4);
                AddAlarmItem(testAlarmGroup, "AlarmItem_Ä2", 3);
                AddAlarmItem(testAlarmGroup, "AlarmItem_Ö", 6);
                AddAlarmItem(testAlarmGroup, "AlarmItem_Å", 1);
            }

            AlarmItemSorter.Instance.Clear();
            AlarmItemSorter.Instance.Init(m_GlobalReferenceService, m_MultiLanguageServiceCF);
        }

        private IAlarmGroup AddAlarmGroup(string text, int groupIndex)
        {
            var alarmGroup = new TestAlarmGroup { Text = text, TestGroupIndex = groupIndex };
            m_AlarmGroups.Add(alarmGroup);
            return alarmGroup;
        }

        private IAlarmItem AddAlarmItem(TestAlarmGroup alarmGroup, string text, int itemIndex)
        {
            int sortIndex = alarmGroup.TestGroupIndex * 6 + itemIndex;
            var alarmItem = new TestAlarmItem(alarmGroup) { Text = text, TestExpectedSortIndex = sortIndex };
            alarmGroup.AlarmItems.Add(alarmItem);
            m_AlarmItems.Add(alarmItem);
            return alarmItem;
        }
    }
}
