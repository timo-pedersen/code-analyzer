using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class BatchServiceTest
    {
        private IBatchService m_BatchService;

        [SetUp]
        public void SetUp()
        {
            m_BatchService = new BatchService();
            IOpcClientServiceCF opcClientServiceCF = TestHelper.AddServiceStub<IOpcClientServiceCF>();
        }
        
        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void BatchStartThrowsExceptionWhenControllerHasNotBeenRegistered()
        {
            IDataSourceContainer controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.Name = "Controller1";
	        controller.IsActive = true;

			Assert.Throws<ArgumentException>(() => m_BatchService.BatchStart(controller));
        }

        [Test]
        public void BatchCommitThrowsExceptionWhenControllerHasNotBeenRegistered()
        {
            IDataSourceContainer controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.Name = "Controller1";
	        controller.IsActive = true;


			Assert.Throws<ArgumentException>(() => m_BatchService.BatchCommit(controller));
        }

        [Test]
        public void InitiatingBatchOnUniqueControllersDoesNotBlockAnyControllers()
        {
            IDataSourceContainer controllerOne = MockRepository.GenerateStub<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
	        controllerOne.IsActive = true;
			IDataSourceContainer controllerTwo = MockRepository.GenerateStub<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
	        controllerTwo.IsActive = true;


			m_BatchService.RegisterController(controllerOne);
            m_BatchService.RegisterController(controllerTwo);

            Action<IDataSourceContainer> batchStartAction = (IDataSourceContainer controller) => m_BatchService.BatchStart(controller);

            AsyncResult asyncResultOne = (AsyncResult)batchStartAction.BeginInvoke(controllerOne, null, null);
            AsyncResult asyncResultTwo = (AsyncResult)batchStartAction.BeginInvoke(controllerTwo, null, null);

            asyncResultOne.AsyncWaitHandle.WaitOne();
            ((Action<IDataSourceContainer>)asyncResultOne.AsyncDelegate).EndInvoke(asyncResultOne);
            asyncResultTwo.AsyncWaitHandle.WaitOne();
            ((Action<IDataSourceContainer>)asyncResultTwo.AsyncDelegate).EndInvoke(asyncResultTwo);

            m_BatchService.BatchCommit(controllerOne);
            m_BatchService.BatchCommit(controllerTwo);

            controllerOne.AssertWasCalled(x => x.BatchStart());
            controllerTwo.AssertWasCalled(x => x.BatchStart());
            controllerOne.AssertWasCalled(x => x.BatchCommit());
            controllerTwo.AssertWasCalled(x => x.BatchCommit());
        }

        [Test]
        public void GetControllersInUseReturnsUniqueControllersForAllUnderlyingDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            GlobalDataItem globalDataItemOne = new GlobalDataItem();
            globalDataItemOne[0].DataItems.Add(dataItemOneControllerOne);
            globalDataItemOne[0].DataItems.Add(dataItemOneControllerTwo);
            globalDataItemOne.AccessRights["Controller1"] = AccessRights.Read;
            globalDataItemOne.AccessRights["Controller2"] = AccessRights.Write;

            IDataItem dataItemTwoControllerOne = MockRepository.GenerateStub<IDataItem>();
            dataItemTwoControllerOne.DataSourceContainer = controllerOne;

            IDataItem dataItemTwoControllerTwo = MockRepository.GenerateStub<IDataItem>();
            dataItemTwoControllerTwo.DataSourceContainer = controllerTwo;

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            GlobalDataItem globalDataItemTwo = new GlobalDataItem();
            globalDataItemTwo[0].DataItems.Add(dataItemTwoControllerOne);
            globalDataItemTwo[0].DataItems.Add(dataItemTwoControllerTwo);
            globalDataItemTwo[0].DataItems.Add(dataItemOneControllerThree);
            globalDataItemTwo.AccessRights["Controller1"] = AccessRights.ReadWrite;
            globalDataItemTwo.AccessRights["Controller2"] = AccessRights.Write;
            globalDataItemTwo.AccessRights["Controller3"] = AccessRights.Write;

            IGlobalDataItem[] globalDataItems = { globalDataItemOne, globalDataItemTwo };
            var controllersInUse = m_BatchService.GetControllersInUse(globalDataItems);

            Assert.AreEqual(3, controllersInUse.Count());
            Assert.AreSame(controllerOne, controllersInUse.ElementAt(0));
            Assert.AreSame(controllerTwo, controllersInUse.ElementAt(1));
            Assert.AreSame(controllerThree, controllersInUse.ElementAt(2));
        }

        [Test]
        public void GetControllersInUseForWriteReturnsUniqueWritableControllersForAllUnderlyingDataItems()
        {
            IDataSourceContainer controllerOne = null;
            IDataItem dataItemOneControllerOne = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerOne, out dataItemOneControllerOne, "Controller1", "DataItem1");

            IDataSourceContainer controllerTwo = null;
            IDataItem dataItemOneControllerTwo = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerTwo, out dataItemOneControllerTwo, "Controller2", "DataItem1");

            
            GlobalDataItem globalDataItemOne = new GlobalDataItem();
            globalDataItemOne[0].DataItems.Add(dataItemOneControllerOne);
            globalDataItemOne[0].DataItems.Add(dataItemOneControllerTwo);
            globalDataItemOne.AccessRights["Controller1"] = AccessRights.Read;
            globalDataItemOne.AccessRights["Controller2"] = AccessRights.Write;

            IDataItem dataItemTwoControllerOne = MockRepository.GenerateStub<IDataItem>();
            dataItemTwoControllerOne.DataSourceContainer = controllerOne;

            IDataItem dataItemTwoControllerTwo = MockRepository.GenerateStub<IDataItem>();
            dataItemTwoControllerTwo.DataSourceContainer = controllerTwo;

            IDataSourceContainer controllerThree = null;
            IDataItem dataItemOneControllerThree = null;
            ControllerHelper.CreateStubControllerWithDataItem(out controllerThree, out dataItemOneControllerThree, "Controller3", "DataItem1");

            GlobalDataItem globalDataItemTwo = new GlobalDataItem();
            globalDataItemTwo[0].DataItems.Add(dataItemTwoControllerOne);
            globalDataItemTwo[0].DataItems.Add(dataItemTwoControllerTwo);
            globalDataItemTwo[0].DataItems.Add(dataItemOneControllerThree);
            globalDataItemTwo.AccessRights["Controller1"] = AccessRights.ReadWrite;
            globalDataItemTwo.AccessRights["Controller2"] = AccessRights.Read;
            globalDataItemTwo.AccessRights["Controller3"] = AccessRights.Read;

            IGlobalDataItem[] globalDataItems = { globalDataItemOne, globalDataItemTwo };
            var controllersInUse = m_BatchService.GetWritableControllersInUse(globalDataItems);

            Assert.AreEqual(2, controllersInUse.Count());
            Assert.AreSame(controllerOne, controllersInUse.ElementAt(0));
            Assert.AreSame(controllerTwo, controllersInUse.ElementAt(1));
        }

   
    }
}
