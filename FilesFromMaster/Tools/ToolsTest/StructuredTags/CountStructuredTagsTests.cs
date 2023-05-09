using System;
using System.Text;
using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Entities;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class CountStructuredTagsTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            TestHelper.AddService<IDataItemCountingService>(new OpcClientToolIde());
            var provider = TestHelper.AddServiceStub<IGlobalDataItemMappingFactory>();
            provider.Stub(inv => inv.CreateNew()).WhenCalled(inv =>{inv.ReturnValue = new GlobalDataItemMappingCF();}).Return(default(GlobalDataItemMappingCF));
        }
       
        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        private static StructuredTagInstance CreateInitializedStructuredTagInstance()
        {
            StructuredTagInstanceMapping rootMapping = new StructuredTagInstanceMapping{ Name="rootMapping", TypeName = "NA"};
            
            IGlobalDataItemMapping item1 = rootMapping.GlobalDataItemMappings.AddNew();
            item1.Name = "item1";

            var level1 = rootMapping.StructuredTagMappings.AddNew();
            level1.Name = "level1Mapping";

            var level2 = level1.StructuredTagMappings.AddNew();
            level2.Name = "level2Mapping";

            var item2 = level2.GlobalDataItemMappings.AddNew();
            item2.Name = "item2";

            return new StructuredTagInstance("test", "typeName", rootMapping);
        }

        [Test]
        public void CountAGivenStructure()
        {
            CreateInitializedStructuredTagInstance();

            int connectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(2, connectedItems);
        }

        [Test]
        public void RemoveLevel1ItemFromInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();
            IDataItemCountingService countingService = ServiceContainerCF.GetService<IDataItemCountingService>();
 
            int initiallyConnectedItems = countingService.ConnectedDataItems;

            tagInstance.InstanceMapping.GlobalDataItemMappings.RemoveAt(0); //remove the only one

            int connectedItemsAfterLevel1Removal = countingService.ConnectedDataItems;

            Assert.AreEqual(initiallyConnectedItems - 1, connectedItemsAfterLevel1Removal);
        }

        [Test]
        public void AddLevel1ItemToInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();

            int initiallyConnectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            IGlobalDataItemMapping mapping = tagInstance.InstanceMapping.GlobalDataItemMappings.AddNew(); 
            mapping.Name = "newItemAtLevel1";

            int connectedItemsAfterLevel1Add = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(initiallyConnectedItems + 1, connectedItemsAfterLevel1Add);
        }

        [Test]
        public void RemoveLevel2ItemFromInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();

            int initiallyConnectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            tagInstance.InstanceMapping.StructuredTagMappings[0].StructuredTagMappings[0].GlobalDataItemMappings.RemoveAt(0); 

            int connectedItemsAfterLevel1Removal = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(initiallyConnectedItems - 1, connectedItemsAfterLevel1Removal);
        }


        [Test]
        public void AddLevel2ItemToInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();

            int initiallyConnectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            IGlobalDataItemMapping mapping = tagInstance.InstanceMapping.StructuredTagMappings[0].StructuredTagMappings[0].GlobalDataItemMappings.AddNew(); 
            mapping.Name = "newItemAtLevel2";

            int connectedItemsAfterLevel1Add = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(initiallyConnectedItems + 1, connectedItemsAfterLevel1Add);
        }

        [Test]
        public void RemoveLevel2StructuredNodeFromInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();

            int initiallyConnectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(2, initiallyConnectedItems);

            tagInstance.InstanceMapping.StructuredTagMappings[0].StructuredTagMappings.RemoveAt(0);

            int connectedItemsAfterCompleteLevelRemove = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems; 

            Assert.AreEqual(1, connectedItemsAfterCompleteLevelRemove);
        }

        [Test]
        public void RemoveLevel1StructuredNodeFromInstance()
        {
            StructuredTagInstance tagInstance = CreateInitializedStructuredTagInstance();

            int initiallyConnectedItems = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(2, initiallyConnectedItems);

            tagInstance.InstanceMapping.StructuredTagMappings.RemoveAt(0);

            int connectedItemsAfterCompleteLevelRemove = ServiceContainerCF.GetService<IDataItemCountingService>().ConnectedDataItems;

            Assert.AreEqual(1, connectedItemsAfterCompleteLevelRemove);
        }


    }
}
