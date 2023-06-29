using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.Api.DataSource;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.SystemTag
{
    [TestFixture]
    public class SystemTagServiceCFTest
    {
        private IList<IDataItemBase> m_DataItems;

        [SetUp]
        public void Setup()
        {
            m_DataItems = new List<IDataItemBase>();

            IToolManager toolManager = Substitute.For<IToolManager>();
            toolManager.Runtime.Returns(true);
            TestHelper.AddService(toolManager);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        private void SetupServiceStubs()
        {
            IGlobalController globalControllerStub = Substitute.For<IGlobalController>();

            globalControllerStub.DataItemBases.Returns(new ReadOnlyCollection<IDataItemBase>(m_DataItems));

            globalControllerStub.GetSystemTags(Arg.Any<ISystemTagInfo>())
                .Returns(x => 
                    globalControllerStub.DataItemBases.OfType<ISystemDataItem>()
                        .Where(item => item.SystemTagInfoName == ((ISystemTagInfo)x[0]).Name).ToArray());

            IOpcClientServiceCF opcClientServiceStub = Substitute.For<IOpcClientServiceCF>();
            opcClientServiceStub.GlobalController.Returns(globalControllerStub);

            TestHelper.AddService(opcClientServiceStub);
        }

    

        private ISystemDataItem CreateTestSystemDataItem(string stemTagInfoName, string name)
        {
            return CreateTestSystemDataItem(stemTagInfoName, name, BEDATATYPE.DT_INTEGER4);
        }

        private ISystemDataItem CreateTestSystemDataItem(string stemTagInfoName, string name, BEDATATYPE dataType)
        {
            return new SystemDataItem(stemTagInfoName, name, dataType, 1, 0, 1.0, 0, false, string.Empty, AccessRights.Read, string.Empty, false, null,1);
        }

        [Test]
        public void CanUpdateSingleSystemDataItem()
        {
            SetupServiceStubs();

            ISystemTagInfo systemTagInfo = new SystemTagInfo("dummy system info", "", "", BEDATATYPE.DT_INTEGER4, AccessRights.Read);

            ISystemDataItem systemDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem");
            m_DataItems.Add(systemDataItem);

            ISystemTagServiceCF systemTagServiceCF = new SystemTagServiceCF();

            systemTagServiceCF.UpdateSystemTags(systemTagInfo, (int)1);

            Assert.AreEqual((int)1, ((VariantValue)systemDataItem.Value).Value);
        }

        [Test]
        public void CanUpdateMultipleSystemDataItems()
        {
            SetupServiceStubs();

            ISystemTagInfo systemTagInfo = new SystemTagInfo("dummy system info", "", "", BEDATATYPE.DT_INTEGER4, AccessRights.Read);

            ISystemDataItem firstDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem1");
            m_DataItems.Add(firstDataItem);

            ISystemDataItem secondDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem2");
            m_DataItems.Add(secondDataItem);

            ISystemTagServiceCF systemTagServiceCF = new SystemTagServiceCF();

            systemTagServiceCF.UpdateSystemTags(systemTagInfo, (int)1);

            Assert.AreEqual((int)1, ((VariantValue)firstDataItem.Value).Value);
            Assert.AreEqual((int)1, ((VariantValue)secondDataItem.Value).Value);
        }

        [Test]
        public void OnlyUpdatesMatchingSystemDataItems()
        {
            SetupServiceStubs();

            ISystemTagInfo systemTagInfo = new SystemTagInfo("dummy system info", "", "", BEDATATYPE.DT_INTEGER4, AccessRights.Read);

            ISystemDataItem firstDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem1");
            m_DataItems.Add(firstDataItem);

            ISystemDataItem secondDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem2");
            m_DataItems.Add(secondDataItem);

            ISystemDataItem notSameDataItem = CreateTestSystemDataItem("another system info", "dummy dataitem3");
            m_DataItems.Add(notSameDataItem);

            ISystemTagServiceCF systemTagServiceCF = new SystemTagServiceCF();

            VariantValue notUpdatedValue = notSameDataItem.Value as VariantValue;

            systemTagServiceCF.UpdateSystemTags(systemTagInfo, (int)1);

            Assert.AreEqual((int)1, ((VariantValue)firstDataItem.Value).Value);
            Assert.AreEqual((int)1, ((VariantValue)secondDataItem.Value).Value);
            Assert.AreEqual(notUpdatedValue.Value, ((VariantValue)notSameDataItem.Value).Value);
        }

        [Test]
        public void CachesCallsToGlobalControllerWithSameSystemTagInfo()
        {
            SetupServiceStubs();

            ISystemTagInfo systemTagInfo = new SystemTagInfo("dummy system info", "", "", BEDATATYPE.DT_INTEGER4, AccessRights.Read);

            ISystemDataItem firstDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem1");
            m_DataItems.Add(firstDataItem);

            ISystemDataItem secondDataItem = CreateTestSystemDataItem(systemTagInfo.Name, "dummy dataitem2");
            m_DataItems.Add(secondDataItem);

            ISystemTagServiceCF systemTagServiceCF = new SystemTagServiceCF();

            systemTagServiceCF.UpdateSystemTags(systemTagInfo, (int)1);

            systemTagServiceCF.UpdateSystemTags(systemTagInfo, (int)2);
        }

        [Test]
        public void MakesMultipleCallsToGlobalControllerWithMultipleSameSystemTagInfoButCachesThem()
        {
            SetupServiceStubs();

            ISystemTagInfo firstSystemTagInfo = new SystemTagInfo("dummy system info1", "", "", BEDATATYPE.DT_INTEGER4, AccessRights.Read);
            ISystemTagInfo secondSystemTagInfo = new SystemTagInfo("dummy system info2", "", "", BEDATATYPE.DT_INTEGER2, AccessRights.Read);

            ISystemDataItem firstDataItem = CreateTestSystemDataItem(firstSystemTagInfo.Name, "dummy dataitem1", BEDATATYPE.DT_INTEGER4);
            m_DataItems.Add(firstDataItem);

            ISystemDataItem secondDataItem = CreateTestSystemDataItem(secondSystemTagInfo.Name, "dummy dataitem2", BEDATATYPE.DT_INTEGER2);
            m_DataItems.Add(secondDataItem);

            ISystemTagServiceCF systemTagServiceCF = new SystemTagServiceCF();

            systemTagServiceCF.UpdateSystemTags(firstSystemTagInfo, (int)10);
            systemTagServiceCF.UpdateSystemTags(firstSystemTagInfo, (int)11);

            systemTagServiceCF.UpdateSystemTags(secondSystemTagInfo, (int)20);
            systemTagServiceCF.UpdateSystemTags(secondSystemTagInfo, (int)21);
        }

    }
}
