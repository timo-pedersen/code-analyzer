using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using NUnit.Framework;
using NSubstitute;

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
            m_DataItemsStub = Substitute.For<BeDataItems>();

            m_DataSourceStub = Substitute.For<IDataItemDataSource>();
            
            m_DataSourceContainerStub = Substitute.For<IDataSourceContainer>();
            m_DataSourceContainerStub.DataSource.Returns(m_DataSourceStub);
        }

        [Test]
        public void SettingDataSourceContainerKeepsItemIDWhenConnectedToExternalOPCServerAndTagIsMissing()
        {
            m_DataSourceContainerStub.DataSourceType = DataSourceType.DataSourceOpcClassicExternal;

            m_DataSourceStub.ValidateIO(Arg.Any<string>(), out Arg.Any<string>())
                            .Returns(true);

            //Assert.Throws<ArgumentException>
            m_DataItemsStub.AddDataItem(Arg.Any<string>(), ref Arg.Any<string>(), Arg.Any<short>(), 
                Arg.Any<int>(), Arg.Any<short>(), Arg.Any<BEDATATYPE>(), Arg.Any<object>(), Arg.Any<object>())
                           .Returns(x => throw new ArgumentException());

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

            m_DataSourceStub.ValidateIO(Arg.Any<string>(), out Arg.Any<string>())
                            .Returns(true);

            m_DataItemsStub.AddDataItem(Arg.Any<string>(), ref Arg.Any<string>(), Arg.Any<short>(),
                Arg.Any<int>(), Arg.Any<short>(), Arg.Any<BEDATATYPE>(), Arg.Any<object>(), Arg.Any<object>())
                           .Returns(x => throw new ArgumentException());

            IDataItem dataItem = new DataItem();
            dataItem.Name = "ControllerD0";
            dataItem.ItemID = "Controller.D0";

            dataItem.DataSourceContainer = m_DataSourceContainerStub;

            Assert.AreEqual("Controller.D0", dataItem.ItemID);
        }
    }
}
