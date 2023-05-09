using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ControllerManager.ServerBrowseProvider;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.OpcUaClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ControllerManager.ServerBrowserProvider
{
    [TestFixture]
    public class NamespaceSynchronizerTest
    {
        private IDataSourceContainer m_DataSourceContainerStub;
        private NamespaceSynchronizer m_Synchronizer;
        string[] m_Namespaces;

        [SetUp]
        public void SetUp()
        {
            var namingConstraints = MockRepository.GenerateMock<INamingConstraints>();
            namingConstraints.Stub(inv => inv.IsNameLengthValid(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(true);
            namingConstraints.Stub(inv => inv.ReservedApplicationNames).Return(new HashSet<string>());
            namingConstraints.Stub(inv => inv.ReservedSystemNames).Return(new HashSet<string>());

            INameCreationService nameCreationService = new NameCreationService(namingConstraints);
            TestHelper.AddService<INameCreationService>(nameCreationService);
            TestHelper.AddService<IProjectNameCreationServiceIde>(new ProjectNameCreationService(nameCreationService.ToILazy()));
            
            m_DataSourceContainerStub = MockRepository.GenerateMock<IDataSourceContainer>();
            IOpcUaNamespaceInfos opcUaNamespaceInfos = new OpcUaNamespaceInfos();
            for (int i = 0; i < 10; i++)
            {
                string uri = string.Empty;
                if (i < 5)
                {
                    uri = "Previous" + i;
                }
                opcUaNamespaceInfos.Add(new OpcUaNamespaceInfo("NS" + i, uri));

            }

            m_DataSourceContainerStub.Stub(m => m.OpcUaNamespaceInfos).Return(opcUaNamespaceInfos);
            m_DataSourceContainerStub.Stub(m => m.OpcUaNamespaceNameBrowseNameSeparator).Return(':');
            
            
            m_Namespaces = new string[] { "J0", "J1", "J2" };
           
            m_Synchronizer = new NamespaceSynchronizer(m_Namespaces, m_DataSourceContainerStub);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void NonExistingServerSideNamespacesWithMappedDataItemsAreNotRemoved()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>(){ new DataItem("A", "NS0:Previous0", BEDATATYPE.DT_BOOLEAN, 2, 2, 2) }));
            NamespaceSynchResult result = m_Synchronizer.Synchronize();
            bool exists = m_DataSourceContainerStub.OpcUaNamespaceInfos.Any(nsInfo => nsInfo.Name == "NS0" && nsInfo.Uri == "Previous0");
            bool isAddedToUnresolved = result.UnResolvedNamespaces.Any(nsInfo => nsInfo.Name == "NS0" && nsInfo.Uri == "Previous0");

            Assert.IsTrue(exists, "The OpcUaNamespaceInfo should not be removed from the controller");
            Assert.IsTrue(isAddedToUnresolved, "The OpcUaNamespaceInfo has dataitem mapped and should be added to the unresolved list");
        }

        [Test]
        public void NonExistingServerSideNamespacesWithoutMappedDataItemsAreRemoved()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>()));
            bool existsBefore = m_DataSourceContainerStub.OpcUaNamespaceInfos.Any(nsInfo => nsInfo.Name == "NS0" && nsInfo.Uri == "Previous0");

            NamespaceSynchResult result = m_Synchronizer.Synchronize();
            bool existsAfter = m_DataSourceContainerStub.OpcUaNamespaceInfos.Any(nsInfo => nsInfo.Name == "NS0" && nsInfo.Uri == "Previous0");
            bool isAddedToRemoved = result.RemovedNamespaces.Any(nsInfo => nsInfo.Name == "NS0" && nsInfo.Uri == "Previous0");

            Assert.IsFalse(existsAfter, "The OpcUaNamespaceInfo should be removed from the controller");
            Assert.IsTrue(existsBefore, "The OpcUaNamespaceInfo should exist before synchronizing");
            Assert.IsTrue(isAddedToRemoved, "The OpcUaNamespaceInfo has no dataitem mapped and should be added to the removed list");
            Assert.IsTrue(result.RemovedNamespaces.Count() == 5);

        }

        [Test]
        public void ExistingNamespacesAreUnchanged()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>()));
            OpcUaNamespaceInfo shouldRemainUnchanged = new OpcUaNamespaceInfo("NS" + 40, "J0");
            m_DataSourceContainerStub.OpcUaNamespaceInfos.Add(shouldRemainUnchanged);
            var result = m_Synchronizer.Synchronize();

            Assert.IsTrue(m_DataSourceContainerStub.OpcUaNamespaceInfos.Contains(shouldRemainUnchanged));
            Assert.IsFalse(result.RemovedNamespaces.Contains(shouldRemainUnchanged));
            Assert.IsFalse(result.UnResolvedNamespaces.Contains(shouldRemainUnchanged));
            Assert.IsFalse(result.AddedNamespaces.Any(nsInfo => nsInfo.Uri == shouldRemainUnchanged.Uri));
        }

        [Test]
        public void NewNamespacesAreAddedOnNextFreeSlot()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>()));
            m_DataSourceContainerStub.OpcUaNamespaceInfos.Clear();
            OpcUaNamespaceInfo shouldRemainUnchanged = new OpcUaNamespaceInfo("NS" + 0, "J0");
            m_DataSourceContainerStub.OpcUaNamespaceInfos.Add(shouldRemainUnchanged);
            m_DataSourceContainerStub.OpcUaNamespaceInfos.Add(new OpcUaNamespaceInfo("NS" + 1, string.Empty));

            var nextFreeSlotNamespaceInfo = m_DataSourceContainerStub.OpcUaNamespaceInfos.First(nsInfo => string.IsNullOrEmpty(nsInfo.Uri));
            string nextFreeSlotNamespace = nextFreeSlotNamespaceInfo.Name;
            m_Synchronizer.Synchronize();
            var firstAdded = m_DataSourceContainerStub.OpcUaNamespaceInfos.Single(nsInfo => nsInfo.Uri == "J1");
            var secondAdded = m_DataSourceContainerStub.OpcUaNamespaceInfos.Single(nsInfo => nsInfo.Uri == "J2");
            
            Assert.AreEqual(nextFreeSlotNamespace, firstAdded.Name);
            Assert.AreEqual("NS2", secondAdded.Name);
        }


        [Test]
        public void AllNewAndExistingServerSideNamespacesHasAnIndexToPrefixMapping()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>()));
            OpcUaNamespaceInfo shouldRemainUnchanged = new OpcUaNamespaceInfo("NS" + 40, "J0");
            m_DataSourceContainerStub.OpcUaNamespaceInfos.Add(shouldRemainUnchanged);
            NamespaceSynchResult result = m_Synchronizer.Synchronize();
            for (int i = 0; i < m_Namespaces.Length; i++)
            {
                string prefix = m_DataSourceContainerStub.OpcUaNamespaceInfos.Single(nsInfo => nsInfo.Uri == m_Namespaces[i]).Name;
                string mappedPrefix = result.NamespaceIndexToPrefixMap[i];
                Assert.AreEqual(prefix, mappedPrefix);
            }
        }

        [Test]
        public void ThereIsTheSameOrMoreOfEmptyNamespaceSlotsAsBeforeSynch()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>() { new DataItem("A", "NS20:J", BEDATATYPE.DT_BOOLEAN, 2, 2, 2) }));

            var numberOfEmptySlotsBeforeSynch = m_DataSourceContainerStub.OpcUaNamespaceInfos.Count(nsInfo => string.IsNullOrEmpty(nsInfo.Uri));
            m_Synchronizer.Synchronize();
            var numberOfEmptySlotsAfterSynch = m_DataSourceContainerStub.OpcUaNamespaceInfos.Count(nsInfo => string.IsNullOrEmpty(nsInfo.Uri));
           
            Assert.IsTrue(numberOfEmptySlotsBeforeSynch <= numberOfEmptySlotsAfterSynch); // there might be more because we have removed more than we have added
        }

        [Test]
        public void IndexToPrefixMappingLeadsToSameUri()
        {
            m_DataSourceContainerStub.Stub(m => m.DataItemBases).Return(new ReadOnlyObservableCollection<IDataItemBase>(new ObservableCollection<IDataItemBase>()));
            NamespaceSynchResult result = m_Synchronizer.Synchronize();
            for (int i = 0; i < m_Namespaces.Length; i++)
            {
                string prefix = m_DataSourceContainerStub.OpcUaNamespaceInfos.Single(nsInfo => nsInfo.Uri == m_Namespaces[i]).Name;
                string mappedPrefix = result.NamespaceIndexToPrefixMap[i];
                Assert.AreEqual(prefix, mappedPrefix);
            }
        }
    }
}
