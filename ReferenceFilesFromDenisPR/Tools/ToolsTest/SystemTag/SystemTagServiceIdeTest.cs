#if !VNEXT_TARGET
using System;
using System.Linq;
using Core.Api.DataSource;
using Core.Api.Feature;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.SystemTag.Features;
using Neo.ApplicationFramework.Tools.TestHelpers.Fixtures;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.SystemTag
{
    [TestFixture]
    public class SystemTagServiceIdeTest
    {
        private IOpcClientServiceIde m_OpcClientServiceStub;
        private ISystemTagServiceIde m_SystemTagServiceIde;
        private IFeatureSecurityServiceIde m_FeatureSecurityServiceIdeStub;

        [SetUp]
        public void SetUp()
        {
            m_OpcClientServiceStub = TestHelper.CreateAndAddServiceStub<IOpcClientServiceIde>();
            m_FeatureSecurityServiceIdeStub = TestHelper.CreateAndAddServiceStub<IFeatureSecurityServiceIde>();
            m_OpcClientServiceStub.AddNewDataItem<SystemDataItem>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IControllerBase>())
                .Returns(new SystemDataItem());
            m_FeatureSecurityServiceIdeStub.IsActivated<SystemTagFeature>().Returns(true);
            TestHelper.CreateAndAddServiceStub<ITargetService>();
            TestHelper.CreateAndAddServiceStub<IProjectManager>();
            TestHelper.CreateAndAddServiceStub<IEventBrokerService>();

            m_SystemTagServiceIde = new SystemTagServiceIde();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CanRegisterSystemTagInfo()
        {
            SystemTagInfo systemTagInfo = SystemTagInfoFixtures.CurrentScreenId;
            m_SystemTagServiceIde.Register(systemTagInfo);

            Assert.AreEqual(1, m_SystemTagServiceIde.SystemTagInfos.Count());

            Assert.AreSame(systemTagInfo, m_SystemTagServiceIde.SystemTagInfos.First());
        }

        [Test]
        public void CanRegisterMultipleSystemTagInfos()
        {
            SystemTagInfo aaaSystemTagInfo = SystemTagInfoFixtures.CurrentScreenId;
            SystemTagInfo bbbSystemTagInfo = SystemTagInfoFixtures.NewScreenId;

            m_SystemTagServiceIde.Register(aaaSystemTagInfo);
            m_SystemTagServiceIde.Register(bbbSystemTagInfo);

            Assert.AreEqual(2, m_SystemTagServiceIde.SystemTagInfos.Count());

            Assert.Contains(aaaSystemTagInfo, m_SystemTagServiceIde.SystemTagInfos.ToList());
            Assert.Contains(bbbSystemTagInfo, m_SystemTagServiceIde.SystemTagInfos.ToList());
        }

        [Test]
        public void CanNotRegisterMultipleSystemTagInfosWithSameName()
        {
            SystemTagInfo aaaSystemTagInfo = SystemTagInfoFixtures.CurrentScreenId;
            SystemTagInfo bbbSystemTagInfo = SystemTagInfoFixtures.CurrentScreenId;

            m_SystemTagServiceIde.Register(aaaSystemTagInfo);
            Assert.Throws<ArgumentException>(() => m_SystemTagServiceIde.Register(bbbSystemTagInfo));
        }

        [Test]
        public void CanNotRegisterSameSystemTagInfoTwice()
        {
            SystemTagInfo systemTagInfo = SystemTagInfoFixtures.CurrentScreenId;

            m_SystemTagServiceIde.Register(systemTagInfo);
            Assert.Throws<ArgumentException>(() => m_SystemTagServiceIde.Register(systemTagInfo));
        }

        [Test]
        public void FailsToCreateDataItemFromNonRegisteredSystemTagInfo()
        {
            SystemTagInfo systemTagInfo = SystemTagInfoFixtures.CurrentScreenId;

            Assert.Throws<ArgumentException>(() => m_SystemTagServiceIde.CreateSystemDataItem(systemTagInfo));
        }

        [Test]
        public void CanCreateSystemDataItemFromRegisteredSystemTagInfo()
        {
            SystemTagInfo systemTagInfo = SystemTagInfoFixtures.CurrentScreenId;

            m_SystemTagServiceIde.Register(systemTagInfo);

            IGlobalDataItem createdDataItem = m_SystemTagServiceIde.CreateSystemDataItem(systemTagInfo);

            Assert.IsNotNull(createdDataItem);
            Assert.IsInstanceOf<SystemDataItem>(createdDataItem);
        }

        [Test]
        public void CreatesTheCorrectSystemDataItemFromRegistredSystemTagInfo()
        {
            SystemTagInfo systemTagInfo = SystemTagInfoFixtures.WithAllPropertiesSet;

            m_SystemTagServiceIde.Register(systemTagInfo);

            SystemDataItem createdDataItem = m_SystemTagServiceIde.CreateSystemDataItem(systemTagInfo) as SystemDataItem;

            Assert.That(createdDataItem.AlwaysActive, Is.EqualTo(systemTagInfo.AlwaysActive));
            Assert.That(createdDataItem.SystemTagInfoName, Is.EqualTo(systemTagInfo.Name));
            Assert.That(createdDataItem.InitialValue.Value, Is.EqualTo(systemTagInfo.InitialValue.Value));
            Assert.That(createdDataItem.DataType, Is.EqualTo(systemTagInfo.DefaultDataType));
            Assert.That(createdDataItem.Size, Is.EqualTo(systemTagInfo.Size));
            Assert.That(createdDataItem.AccessRight, Is.EqualTo(systemTagInfo.AccessRight));
            Assert.That(createdDataItem.Description, Is.EqualTo(systemTagInfo.Description));
            Assert.That(createdDataItem.DataType, Is.EqualTo(systemTagInfo.DefaultDataType));
        }

        [Test]
        public void CanCreateSystemDataItemFromRegisteredSystemTagInfoWithCorrectInfo()
        {
            SystemTagInfo systemTagInfo = new SystemTagInfo("aaa", "some description", "some group", BEDATATYPE.DT_DATETIME, AccessRights.Write, DateTime.Parse("1901-01-01 01:01:01"));

            m_SystemTagServiceIde.Register(systemTagInfo);

            SystemDataItem createdDataItem = m_SystemTagServiceIde.CreateSystemDataItem(systemTagInfo) as SystemDataItem;

            if (createdDataItem == null)
                return;

            Assert.AreEqual(createdDataItem.AccessRight, systemTagInfo.AccessRight);
            Assert.AreEqual(createdDataItem.DataType, systemTagInfo.DefaultDataType);
            Assert.AreEqual(createdDataItem.Value.Value, systemTagInfo.InitialValue.Value);
            Assert.AreEqual(createdDataItem.SystemTagInfoName, systemTagInfo.Name);
        }


        [Test]
        public void CanSetDisplayName()
        {
            string defaultName = "Some Display Name";
            string defaultValidName = "SystemTagSomeDisplayName";

            m_OpcClientServiceStub.AddNewDataItem<SystemDataItem>(Arg.Is(defaultValidName), Arg.Any<string>(), Arg.Any<IControllerBase>())
                .Returns(new SystemDataItem() { Name = defaultValidName });
            m_OpcClientServiceStub.GlobalController.Returns(x => null);

            SystemTagInfo systemTagInfo = new SystemTagInfo("Some Other Display Name", defaultName, "some description", "some group", BEDATATYPE.DT_DATETIME, 1, AccessRights.Write, DateTime.Parse("1901-01-01 01:01:01"));

            m_SystemTagServiceIde.Register(systemTagInfo);

            SystemDataItem createdDataItem = m_SystemTagServiceIde.CreateSystemDataItem(systemTagInfo) as SystemDataItem;

            Assert.IsNotNull(createdDataItem);
            Assert.AreEqual(systemTagInfo.AccessRight, createdDataItem.AccessRight);
            Assert.AreEqual(systemTagInfo.DefaultDataType, createdDataItem.DataType);
            Assert.AreEqual(systemTagInfo.InitialValue.Value, createdDataItem.Value.Value);
            Assert.AreEqual(systemTagInfo.Name, createdDataItem.SystemTagInfoName);
            Assert.AreEqual(defaultValidName, createdDataItem.Name);
        }
    }
}
#endif
