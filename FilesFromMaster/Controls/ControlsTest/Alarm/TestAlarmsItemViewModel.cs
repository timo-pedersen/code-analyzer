using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Alarm
{
    [TestFixture]
    public class TestAlarmsItemViewModel
    {
        private object m_ViewerObject;
        private AlarmItemsViewModel m_ViewModel;
        private List<AlarmItemWrapper> m_GenericItemList;

        [SetUp]
        public void Setup()
        {
            m_ViewerObject = new object();
            m_GenericItemList = new List<AlarmItemWrapper>
            {
                SetupStubItem(false, "Text #1", "Alarm1", "Group1"),
                SetupStubItem(true, "HaHa", "Alarm2", "Group1a"),
                SetupStubItem(true, "Woe is me", "Alarm1a", "Group2"),
            };

            var alarmServer = MockRepository.GenerateStub<IAlarmServer>();
            var alarmItemRepository = MockRepository.GenerateStub<IAlarmItemRepository>();
            alarmServer.Stub(x => x.AlarmItemRepository).Return(alarmItemRepository);

            // using a null invoker blocks deferring filtering to the thread pool
            // clone the generic item list so that sorting and stuff in the view model doesn't affect the original
            m_ViewModel = new AlarmItemsViewModel(m_ViewerObject, null, m_GenericItemList.ToList(), alarmServer);
        }

        public static AlarmItemWrapper SetupStubItem(bool enabled, string text, string name, string group, DateTime? enabledDate = null, DateTime? disabledDate = null)
        {
            var item = MockRepository.GenerateStub<IAlarmItem>();
            item.IsEnabledItem = enabled;
            item.Text = text;
            item.DisplayName = name;
            item.Stub(x => x.GroupName).Return(group);
            item.Stub(x => x.ItemEnabledTime).Return(enabledDate);
            item.Stub(x => x.ItemDisabledTime).Return(disabledDate);
            return new AlarmItemWrapper(item);
        }

        [TearDown]
        public void TearDown()
        {
            m_ViewModel.Dispose();
            m_ViewModel = null;
        }

        [Test]
        public void UnfilteredReturnsAllItems()
        {
            m_ViewModel.FilterString = "";
            m_ViewModel.ShowOnlyDisabledAlarmItems = false;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems.Count == m_GenericItemList.Count);
        }

        [Test]
        public void NullFilterReturnsAllItems()
        {
            m_ViewModel.FilterString = null;
            m_ViewModel.ShowOnlyDisabledAlarmItems = false;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems.Count == m_GenericItemList.Count);
        }

        [Test]
        public void DisabledAlarmCountIncludesFilterExcludedItems()
        {
            m_ViewModel.FilterString = "";
            m_ViewModel.ShowOnlyDisabledAlarmItems = false;
            int originalCount = m_ViewModel.DisabledAlarmItemCount;
            m_ViewModel.FilterString = "RandomStringToExcludeAllItems";
            Assert.IsTrue(m_ViewModel.DisabledAlarmItemCount == originalCount);
            Assert.IsTrue(m_ViewModel.DisabledAlarmItemCount == 1);
        }

        [Test]
        public void AllItemAlarmCountIncludesFilterExcludedItems()
        {
            m_ViewModel.FilterString = "";
            m_ViewModel.ShowOnlyDisabledAlarmItems = false;
            int originalCount = m_ViewModel.AllAlarmItemCount;
            m_ViewModel.FilterString = "RandomStringToExcludeAllItems";
            Assert.IsTrue(m_ViewModel.AllAlarmItemCount == originalCount);
            Assert.IsTrue(m_ViewModel.AllAlarmItemCount == 3);
        }

        [Test]
        public void ViewerTemplateIsReturned()
        {
            Assert.IsTrue(m_ViewModel.ViewerTemplate == m_ViewerObject);
        }

        [Test]
        public void ShowOnlyDisabledAlarmItems()
        {
            m_ViewModel.FilterString = "";
            m_ViewModel.ShowOnlyDisabledAlarmItems = true;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems.Count == 1);
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems.All(alarm => !alarm.IsEnabledItem));
        }

        [Test]
        [TestCase("Group2", 1, 2)]
        [TestCase("Group1", 2, 0)]
        [TestCase("#1", 1, 0)]
        [TestCase("Alarm", 3, 1)]
        [TestCase("m2", 1, 1)]
        [TestCase("is", 1, 2)]
        [TestCase("Aaaaaughibbrgubugbugrguburgle", 0, -1)]
        public void FilterString(string filterString, int count, int indexWhichMustBePresent)
        {
            m_ViewModel.FilterString = filterString;
            m_ViewModel.ShowOnlyDisabledAlarmItems = false;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems.Count == count);
            Assert.IsTrue(indexWhichMustBePresent == -1 || m_ViewModel.FilteredAlarmItems.Contains(m_GenericItemList[indexWhichMustBePresent]));
        }

        [Test]
        [TestCase(AlarmItemsViewModel.IAlarmIsEnabledPropertyName, 0)]
        [TestCase(AlarmItemsViewModel.IAlarmTextNamePropertyName, 1)]
        [TestCase(AlarmItemsViewModel.IAlarmNamePropertyName, 0)]
        [TestCase(AlarmItemsViewModel.IAlarmGroupNamePropertyName, 0)]
        public void SortPropertyName(string propertyName, int indexOfFirstItem)
        {
            m_ViewModel.SortPropertyName = propertyName;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems[0] == m_GenericItemList[indexOfFirstItem]);
        }

        [Test]
        public void NoExceptionWithInvalidSortPropertyName()
        {
            m_ViewModel.SortPropertyName = "BlahBlahBlah";
        }

        [Test]
        public void SortDescending()
        {
            m_ViewModel.SortPropertyName = AlarmItemsViewModel.IAlarmTextNamePropertyName;
            m_ViewModel.SortDirection = ListSortDirection.Descending;
            Assert.IsTrue(m_ViewModel.FilteredAlarmItems[0] == m_GenericItemList[2]);
        }

        [Test]
        public void SetAllToDisabled()
        {
            m_ViewModel.FilterString = "Aaaaaughibbrgubugbugrguburgle";
            m_ViewModel.SetAllAlarmItemsEnabledState(false);
            Assert.IsTrue(m_GenericItemList.All(x => !x.IsEnabledItem));
        }

        [Test]
        public void SetRangeToDisabled()
        {
            m_ViewModel.FilterString = "Aaaaaughibbrgubugbugrguburgle";
            m_ViewModel.SetAllAlarmItemsEnabledState(true);
            m_ViewModel.SetAlarmItemsEnableState(new[] { m_GenericItemList[1], m_GenericItemList[2] }, false);
            Assert.IsTrue(m_GenericItemList[0].IsEnabledItem);
            Assert.IsFalse(m_GenericItemList[1].IsEnabledItem);
            Assert.IsFalse(m_GenericItemList[2].IsEnabledItem);
        }

        [Test]
        public void CoverageForNewValueEarlyRetur()
        {
            m_ViewModel.SortPropertyName = AlarmItemsViewModel.IAlarmTextNamePropertyName;
            m_ViewModel.SortPropertyName = AlarmItemsViewModel.IAlarmTextNamePropertyName;
            m_ViewModel.SortDirection= ListSortDirection.Ascending;
            m_ViewModel.SortDirection = ListSortDirection.Ascending;
            m_ViewModel.FilterString = "Aaaaaughibbrgubugbugrguburgle";
            m_ViewModel.FilterString = "Aaaaaughibbrgubugbugrguburgle";

            Assert.IsTrue(m_ViewModel.SortPropertyName == AlarmItemsViewModel.IAlarmTextNamePropertyName);
            Assert.IsTrue(m_ViewModel.FilterString == "Aaaaaughibbrgubugbugrguburgle");
        }
    }
}
