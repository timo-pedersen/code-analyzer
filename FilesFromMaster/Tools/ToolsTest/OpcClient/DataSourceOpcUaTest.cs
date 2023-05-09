using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.Interfaces.OpcUaClient;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    public class OpcUaAddressDescriptors : IDisposable
    {
        protected readonly OpcUaAddressDescriptor[] m_Descriptors;

        public OpcUaAddressDescriptors(OpcUaAddressDescriptor[] descriptors)
        {
            m_Descriptors = descriptors;
        }

        public OpcUaAddressDescriptor[] Descriptors => m_Descriptors;

        void IDisposable.Dispose()
        {
            foreach (IDisposable disposable in m_Descriptors)
            {
                disposable.Dispose();
            }
        }
    }

    public class DataSourceOpcUaDummy : DataSourceOpcUa
    {
        internal DataSourceOpcUaDummy(IDataSourceContainer dataSourceContainer, IDataSourceOpcUa currentDataSource, uint clientConnectionId, IOpcUaDaClientService opcUaDaClientService)
            : base(dataSourceContainer, currentDataSource, clientConnectionId, opcUaDaClientService)
        {
        }
        
        protected override bool IsControllerConnected => true;

        public void UpdateSubscriptions()
        {
            DataSourceBatchActivationCommitTransaction();
        }

        protected override bool TryGetCookiesAndDataTypes(OpcUaAddressDescriptors addresses, out int[] cookies, out BEDATATYPE[] dataTypes)
        {
            cookies = new int[1] { 1 };
            dataTypes = new BEDATATYPE[1] { BEDATATYPE.DT_DEFAULT };
            return true;
        }
    }

    public class OpcUaClientServiceDummy : IOpcUaDaClientService
    {
        event EventHandler<OpcUaDataChangeEventArgs> IOpcUaDaClientService.ValueChange
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ServerInfoArgs> IOpcUaDaClientService.ServerInfo
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        #region IOpcUaDaClientService Members

        public virtual uint Init(string controllerName, string url)
        {
            return 0;
        }

        public virtual bool Connect(uint clientConnectionId, string userName, string password, uint maxNumberOfSubscriptions, uint maxSubscriptionItems, double commonPublishingInterval, bool allowNonSecureConnections)
        {
            return true;
        }

        public virtual bool Disconnect(uint clientConnectionId)
        {
            return true;
        }

        public virtual bool IsServerConnected(uint clientConnectionId)
        {
            return false;
        }

        public virtual bool AddDataItems(uint clientConnectionId, OpcUaAddressDescriptor[] addresses, out int[] cookies, out int hResult)
        {
            cookies = new int[0];
            hResult = 0;
            return false;
        }

        public virtual bool RemoveAllDataItems(uint clientConnectionId)
        {
            return false;
        }

        public virtual bool GetBeDataTypes(uint clientConnectionId, int[] cookies, out BEDATATYPE[] dataTypes)
        {
            dataTypes = new BEDATATYPE[0];
            return false;
        }

        public virtual bool Read(uint clientConnectionId, int[] cookies, out object[] readValues, out bool[] readResults)
        {
            readValues = new VariantValue[0];
            readResults = new bool[0];
            return false;
        }

        public virtual bool Write(uint clientConnectionId, int[] cookies, object[] writeValues, out bool[] writeResults)
        {
            writeResults = new bool[0];
            return false;
        }

        public virtual bool Subscribe(uint clientConnectionId, int[] cookies, double[] publishingIntervals, double[] samplingInterval, uint[] queueSize, uint count, out int hResult, out bool[] subscribeItemResults)
        {
            hResult = 0;
            subscribeItemResults = new bool[0];
            return false;
        }

        public virtual bool UnSubscribe(uint clientConnectionId, int[] cookies, uint count)
        {
            return false;
        }

        public virtual bool UnSubscribeAll(uint clientConnectionId)
        {
            return false;
        }

        #endregion
    }

    [TestFixture]
    public class DataSourceOpcUaTest
    {
        private IDataSourceOpcUa m_DataSourceOpcUa;
        private DataSourceContainer m_DataSourceContainer;
        private IOpcUaDaClientService m_OpcUaDaClientService;

        [SetUp]
        public void SetUp()
        {
            m_DataSourceContainer = new DataSourceContainer();

            m_OpcUaDaClientService = MockRepository.GenerateStub<OpcUaClientServiceDummy>();
            m_OpcUaDaClientService.Stub(x => x.IsServerConnected(1)).Return(true);

            m_DataSourceOpcUa = new DataSourceOpcUaDummy(m_DataSourceContainer, m_DataSourceOpcUa, 1, m_OpcUaDaClientService);

        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CheckSubscribeOnSetActiveTrueTest()
        {
            IDataItem dataItem = new DataItem();
            dataItem.Name = "Controller1";
            dataItem.ItemID = "Controller1.D0";

            m_DataSourceContainer.DataItems.Insert(0, dataItem);

            m_DataSourceOpcUa.UpdateDataItemsConnection(false);

            ((IDataItemDataSource)m_DataSourceOpcUa).SetActive(1, BEACTIVETYPE.ACTIVE_TRUE);

            ((DataSourceOpcUaDummy)m_DataSourceOpcUa).UpdateSubscriptions();

            m_OpcUaDaClientService.AssertWasCalled(x => x.Subscribe(
                Arg<uint>.Is.Anything, Arg<int[]>.Matches(y => y[0] == 1),
                Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<uint[]>.Is.Anything, Arg<uint>.Is.Anything, out Arg<int>.Out(1).Dummy, out Arg<bool[]>.Out(new[] { false }).Dummy));
        }

        [Test]
        public void CheckUnsubscribeOnSetActiveFalseTest()
        {
            IDataItem dataItem = new DataItem();
            dataItem.Name = "Controller1";
            dataItem.ItemID = "Controller1.D0";

            m_DataSourceContainer.DataItems.Insert(0, dataItem);

            m_DataSourceOpcUa.UpdateDataItemsConnection(false);

            ((IDataItemDataSource)m_DataSourceOpcUa).SetActive(1, BEACTIVETYPE.ACTIVE_FALSE);

            ((DataSourceOpcUaDummy)m_DataSourceOpcUa).UpdateSubscriptions();

            m_OpcUaDaClientService.AssertWasCalled(x => x.UnSubscribe(
                Arg<uint>.Is.Anything, Arg<int[]>.Matches(y => y[0] == 1), Arg<uint>.Is.Anything));
        }
    }
}
