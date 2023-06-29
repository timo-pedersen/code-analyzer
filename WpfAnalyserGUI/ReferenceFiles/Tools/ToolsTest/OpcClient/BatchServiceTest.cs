using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            IDataSourceContainer controller = Substitute.For<IDataSourceContainer>();
            controller.Name = "Controller1";
	        controller.IsActive = true;

			Assert.Throws<ArgumentException>(() => m_BatchService.BatchStart(controller));
        }

        [Test]
        public void BatchCommitThrowsExceptionWhenControllerHasNotBeenRegistered()
        {
            IDataSourceContainer controller = Substitute.For<IDataSourceContainer>();
            controller.Name = "Controller1";
	        controller.IsActive = true;


			Assert.Throws<ArgumentException>(() => m_BatchService.BatchCommit(controller));
        }

        [Test]
        public async Task InitiatingBatchOnUniqueControllersDoesNotBlockAnyControllers()
        {
            IDataSourceContainer controllerOne = Substitute.For<IDataSourceContainer>();
            controllerOne.Name = "Controller1";
	        controllerOne.IsActive = true;
			IDataSourceContainer controllerTwo = Substitute.For<IDataSourceContainer>();
            controllerTwo.Name = "Controller2";
	        controllerTwo.IsActive = true;


			m_BatchService.RegisterController(controllerOne);
            m_BatchService.RegisterController(controllerTwo);

            Func<IDataSourceContainer, Task> batchStartAction = (IDataSourceContainer controller) =>
            {
                m_BatchService.BatchStart(controller);
                return Task.CompletedTask;
            };

            await batchStartAction(controllerOne).ConfigureAwait(false);
            await batchStartAction(controllerTwo).ConfigureAwait(false);

            m_BatchService.BatchCommit(controllerOne);
            m_BatchService.BatchCommit(controllerTwo);

            controllerOne.Received().BatchStart();
            controllerTwo.Received().BatchStart();
            controllerOne.Received().BatchCommit();
            controllerTwo.Received().BatchCommit();
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

            IDataItem dataItemTwoControllerOne = Substitute.For<IDataItem>();
            dataItemTwoControllerOne.DataSourceContainer = controllerOne;

            IDataItem dataItemTwoControllerTwo = Substitute.For<IDataItem>();
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

            IDataItem dataItemTwoControllerOne = Substitute.For<IDataItem>();
            dataItemTwoControllerOne.DataSourceContainer = controllerOne;

            IDataItem dataItemTwoControllerTwo = Substitute.For<IDataItem>();
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
