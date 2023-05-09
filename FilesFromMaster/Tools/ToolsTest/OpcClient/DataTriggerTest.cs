using System;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class DataTriggerTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IBatchService>();
        }

        private IGlobalDataSubItem CreateSubItem(int triggerValue)
        {
            IGlobalDataSubItem firstSubItem = MockRepository.GenerateStub<IGlobalDataSubItem>();
            firstSubItem.Stub(x => x.TriggerValue).Return(new VariantValue(triggerValue));
            return firstSubItem;
        }

        [Test]
        public void BatchWriteIsTriggeredOnAllTheSubItems()
        {
            IGlobalDataItem globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            IGlobalDataSubItem firstSubItem = CreateSubItem(11);
            IGlobalDataSubItem secondSubItem = CreateSubItem(22);
            globalDataItem.Stub(x => x.GlobalDataSubItems).Return(new ExtendedBindingList<IGlobalDataSubItem>() { firstSubItem, secondSubItem });

            TriggerBatchWriteThruValueChange(globalDataItem);

            firstSubItem.AssertWasCalled(x=> x.BatchWriteForDataExchange(11));
            secondSubItem.AssertWasCalled(x => x.BatchWriteForDataExchange(22));
        }   

        [Test]
        public void BatchWriteIsTriggeredOnValueChange()
        {
            IGlobalDataItem globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            IGlobalDataSubItem subItem = CreateSubItem(10);
            globalDataItem.Stub(x => x.GlobalDataSubItems).Return(new ExtendedBindingList<IGlobalDataSubItem>(){subItem});

            TriggerBatchWriteThruValueChange(globalDataItem);

            subItem.AssertWasCalled(x => x.BatchWriteForDataExchange(10));
        }

        private void TriggerBatchWriteThruValueChange(IGlobalDataItem globalDataItem)
        {
            IDataTrigger dataTrigger = new DataTrigger();
            dataTrigger.GlobalDataItems.Add(globalDataItem);

            ((IValue)dataTrigger).Value = 1;
        }

        public void CalculateNextStartTimeWhenItTakesShorterTimeThanSpecifiedTriggerInterval()
        {
            DateTime startTime = new DateTime(2009, 3, 18, 12, 0, 10);
            DateTime endTime = new DateTime(2009, 3, 18, 12, 0, 18);
            TimeSpan triggerInterval = TimeSpan.FromSeconds(10);

            TimeSpan nextStartTime = DataTrigger.CalculateNextStartTime(startTime, endTime, triggerInterval);

            Assert.AreEqual(TimeSpan.FromSeconds(2), nextStartTime);
        }

        public void CalculateNextStartTimeWhenItTakesLongerTimeThanSpecifiedTriggerInterval()
        {
            DateTime startTime = new DateTime(2009, 3, 18, 12, 0, 10);
            DateTime endTime = new DateTime(2009, 3, 18, 12, 0, 22);
            TimeSpan triggerInterval = TimeSpan.FromSeconds(10);

            TimeSpan nextStartTime = DataTrigger.CalculateNextStartTime(startTime, endTime, triggerInterval);

            Assert.AreEqual(TimeSpan.FromSeconds(0), nextStartTime);
        }

        public void CalculateNextStartTimeWhenItTakesSameTimeAsSpecifiedTriggerInterval()
        {
            DateTime startTime = new DateTime(2009, 3, 18, 12, 0, 10);
            DateTime endTime = new DateTime(2009, 3, 18, 12, 0, 20);
            TimeSpan triggerInterval = TimeSpan.FromSeconds(10);

            TimeSpan nextStartTime = DataTrigger.CalculateNextStartTime(startTime, endTime, triggerInterval);

            Assert.AreEqual(TimeSpan.FromSeconds(0), nextStartTime);
        }
       
    }
}
