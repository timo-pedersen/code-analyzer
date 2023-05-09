using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class AddingDataItemToDataSourceTest
    {
        private IDataSourceContainer m_DataSourceContainerStub;
        private IDataItemDataSource m_DataSourceStub;
        private BeDataItems m_DataItemsStub;

        [SetUp]
        public void SetUp()
        {
            m_DataItemsStub = MockRepository.GenerateStub<BeDataItems>();

            m_DataSourceStub = MockRepository.GenerateStub<IDataItemDataSource>();
            
            m_DataSourceContainerStub = MockRepository.GenerateStub<IDataSourceContainer>();
            m_DataSourceContainerStub.Stub(x => x.DataSource).Return(m_DataSourceStub);
        }

        [Test]
        public void SettingDataSourceContainerKeepsItemIDWhenConnectedToExternalOPCServerAndTagIsMissing()
        {
            m_DataSourceContainerStub.DataSourceType = DataSourceType.DataSourceOpcClassicExternal;

            string notUsed;
            m_DataSourceStub.Stub(x => x.ValidateIO(null, out notUsed))
                            .IgnoreArguments()
                            .Return(true);

            string itemID = string.Empty;
            m_DataItemsStub.Stub(x => x.AddDataItem(null, ref itemID, 0, 0, 0, BEDATATYPE.DT_DEFAULT, null, null))
                           .IgnoreArguments()
                           .Throw(new ArgumentException());

            IDataItem dataItem = new DataItem();
            dataItem.Name = "ControllerD0";
            dataItem.ItemID = "Controller.D0";

            dataItem.DataSourceContainer = m_DataSourceContainerStub;

            Assert.AreEqual("Controller.D0", dataItem.ItemID);
        }

        [Test]
        public void SettingDataSourceContainerKeepsItemIDWhenConnectedToInternalOPCServerAndTagIsMissing()
        {
            m_DataSourceContainerStub.DataSourceType = DataSourceType.DataSourceOpcClassicInprocess;

            string notUsed;
            m_DataSourceStub.Stub(x => x.ValidateIO(null, out notUsed))
                            .IgnoreArguments()
                            .Return(true);

            string itemID = string.Empty;
            m_DataItemsStub.Stub(x => x.AddDataItem(null, ref itemID, 0, 0, 0, BEDATATYPE.DT_DEFAULT, null, null))
                           .IgnoreArguments()
                           .Throw(new ArgumentException());

            IDataItem dataItem = new DataItem();
            dataItem.Name = "ControllerD0";
            dataItem.ItemID = "Controller.D0";

            dataItem.DataSourceContainer = m_DataSourceContainerStub;

            Assert.AreEqual("Controller.D0", dataItem.ItemID);
        }
    }
}
