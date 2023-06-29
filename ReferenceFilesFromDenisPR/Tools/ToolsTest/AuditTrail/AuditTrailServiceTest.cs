using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Controls.AuditTrail;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.Storage.Legacy;
using Neo.ApplicationFramework.Storage.Query;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.AuditTrail
{
    [TestFixture]
    public class AuditTrailServiceTest
    {
        private IStorage m_Storage;
        private IStorageQuery m_Query;
        private IStorageScheme m_Scheme;
        private IDatabaseManagerLegacy m_DatabaseManagerLegacy;

        private IAuditTrailService m_IAuditTrailService;
        private AuditTrailServiceCF m_AuditTrailService;
        private IAuditTrail m_AuditTrail;

        private const string DataItem = "Controller1.D0";
        private const string TableName = "AuditLog";
        private const string ValueBefore = "1";
        private const string ValueAfter = "2";
        private const string UserName = "theuser";

        [SetUp]
        public void SetUp()
        {
            var securityService = Substitute.For<ISecurityServiceCF>();
            securityService.CurrentUser.Returns(UserName);
            TestHelper.AddService(securityService);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.LocalTime.Returns(DateTime.Now);

            m_Query = Substitute.For<IStorageQuery>();
            m_Scheme = Substitute.For<IStorageScheme>();
            m_Scheme.EnsureTable(Arg.Any<IStorableItem>()).Returns(true);
            m_Storage = Substitute.For<IStorage>();
            m_Storage.Query.Returns(m_Query);
            m_Storage.Scheme.Returns(m_Scheme);

            m_DatabaseManagerLegacy = Substitute.For<IDatabaseManagerLegacy>();

            var storageCacheService = Substitute.For<IStorageCacheService>();
            storageCacheService.GetStorage(Arg.Any<string>()).Returns(m_Storage);
            storageCacheService.GetDatabaseManagerLegacy(Arg.Any<string>()).Returns(m_DatabaseManagerLegacy);
            TestHelper.AddService(storageCacheService);
            TestHelper.AddService(Substitute.For<ISdCardCeService>());

            m_AuditTrailService = new AuditTrailServiceCF(false);
            m_IAuditTrailService = m_AuditTrailService;

            m_AuditTrail = Substitute.For<IAuditTrail>();
            m_AuditTrail.MaxSize.Returns(1);
            m_IAuditTrailService.AuditTrail = m_AuditTrail;
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void LogMessageLogsMessage()
        {
            const string msg = "a message";

            m_IAuditTrailService.LogMessage(msg);

            m_Storage.Query.Received(1)
                .Insert(Arg.Is<IStorableItem[]>(x => x.All(storableItem => ((AuditStorableItem)storableItem).Message == msg)));
        }

        [Test]
        public void LogMessageLogsUsername()
        {
            m_IAuditTrailService.LogMessage(string.Empty);

            m_Query.Received(1)
                .Insert(Arg.Is<IStorableItem[]>(x => x.All(storableItem => ((AuditStorableItem)storableItem).UserName == UserName)));
        }

        [Test]
        public void LogMessageDoesNotLogIfAuditTrailItemDoesntExist()
        {
            m_IAuditTrailService.AuditTrail = null;

            m_IAuditTrailService.LogMessage(string.Empty);

            m_Query.DidNotReceiveWithAnyArgs().Insert(Arg.Any<IStorableItem[]>());
        }

        [Test]
        public void LogDataItemChangedInsertsTableName()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => storableItem.TableName == TableName));
        }

        [Test]
        public void LogDataItemChangedInsertsDateTimeNow()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => IsAlmostNow(((AuditStorableItem)storableItem).TimeStamp)));
        }

        [Test]
        public void LogDataItemChangedInsertsValueBefore()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => ((AuditStorableItem)storableItem).ValueBefore == ValueBefore));
        }

        [Test]
        public void LogDataItemChangedInsertsValueAfter()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => ((AuditStorableItem)storableItem).ValueAfter == ValueAfter));
        }

        [Test]
        public void LogDataItemChangedInsertsUserName()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => ((AuditStorableItem)storableItem).UserName == UserName));
        }

        [Test]
        public void LogDataItemChangedInsertsMessage()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty(storableItems => 
                storableItems.All(storableItem => ((AuditStorableItem)storableItem).Message.Contains(DataItem)));
        }

        [Test]
        public void LogDataItemChangedLogsNothingIfAuditTrailIsNull()
        {
            m_IAuditTrailService.AuditTrail = null;

            m_IAuditTrailService.LogDataItemChanged(DataItem, new VariantValue(1), new VariantValue(2));

            m_Query.DidNotReceiveWithAnyArgs().Insert(Arg.Any<IStorableItem[]>());
        }

        [Test]
        public void GetLogItemsReturnsNullIfDatabaseNotExists()
        {
            m_Query.Select(Arg.Any<Func<System.Data.IDataReader, IEnumerable<AuditStorableItem>>>(), Arg.Any<string>())
                .Returns(x => throw new StorageReaderException(string.Empty));

            IList<IAuditStorableItem> list = m_IAuditTrailService.GetLogItems();

            Assert.IsTrue(list == null || !list.Any());
            m_Query.ReceivedWithAnyArgs(1).Select(Arg.Any<Func<System.Data.IDataReader, IEnumerable<AuditStorableItem>>>(), Arg.Any<string>());
        }

        [Test]
        public void GetLogItemsReturnsNullInDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            // Act
            IList<IAuditStorableItem> list = m_IAuditTrailService.GetLogItems(); //After executing this the m_ToolManager.Runtime stub will go back to default value true (set up in fixture set up)

            // Assert
            Assert.IsNull(list);

            NeoDesignerProperties.IsInDesignMode = false;

            m_Query.DidNotReceiveWithAnyArgs().Select(Arg.Any<Func<System.Data.IDataReader, IEnumerable<AuditStorableItem>>>(), Arg.Any<string>());
        }

        [Test]
        public void LogMessageDoesNotLogWhenDatabaseSizeIsVeryLarge()
        {
            string msg = "a message";

            m_Storage.Size.Returns(long.MaxValue);

            // Act
            m_IAuditTrailService.LogMessage(msg);

            m_Query.DidNotReceiveWithAnyArgs().Insert(Arg.Any<IStorableItem[]>());
        }

        [Test]
        public void IsLogActionEnabledReturnsFalseIfAuditTrailIsNull()
        {
            m_IAuditTrailService.AuditTrail = null;
            Assert.IsFalse(m_AuditTrailService.IsLogActionEnabled("SomeAction"));
        }

        [TestCase("SomeAction", false)]
        [TestCase("SomeOtherAction", true)]
        public void IsLogActionEnabledTest(string actionName, bool expected)
        {
            var actionList = new List<string> { "SomeAction" };

            m_AuditTrail.SuppressedLogActionNameList.Returns(actionList);

            Assert.AreEqual(expected, m_AuditTrailService.IsLogActionEnabled(actionName));
        }

        private void LogDataItemChanged_VerifyAuditStorableItemProperty(System.Linq.Expressions.Expression<Predicate<IStorableItem[]>> predicate)
        {
                m_Storage.Query.Insert(null);

                m_IAuditTrailService.LogDataItemChanged(DataItem, new VariantValue(1), new VariantValue(2));

            m_Storage.Query.Received(1).Insert(Arg.Is(predicate));
        }

        private static bool IsAlmostNow(DateTime dateTimeUnderTest)
        {
            TimeSpan timeSpan = DateTime.Now - dateTimeUnderTest;

            return timeSpan.Seconds < 10;
        }
    }
}
