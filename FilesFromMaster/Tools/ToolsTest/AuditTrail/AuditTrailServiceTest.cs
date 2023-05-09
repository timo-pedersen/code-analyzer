using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Controls.AuditTrail;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Storage.Common;
using Storage.Legacy;
using Storage.Query;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace Neo.ApplicationFramework.Tools.AuditTrail
{
    [TestFixture]
    public class AuditTrailServiceTest
    {
        private MockRepository m_Mocks;

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
            var securityService = MockRepository.GenerateStub<ISecurityServiceCF>();
            securityService.Stub(x => x.CurrentUser).Return(UserName).Repeat.Any();
            TestHelper.AddService(securityService);

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.Stub(x => x.LocalTime).Return(DateTime.Now);

            m_Mocks = new MockRepository();
            m_Query = m_Mocks.DynamicMock<IStorageQuery>();
            m_Scheme = MockRepository.GenerateStub<IStorageScheme>();
            m_Scheme.Stub(x => x.EnsureTable(null)).IgnoreArguments().Return(true).Repeat.Any();
            m_Storage = MockRepository.GenerateStub<IStorage>();
            m_Storage.Stub(x => x.Query).Return(m_Query).Repeat.Any();
            m_Storage.Stub(x => x.Scheme).Return(m_Scheme).Repeat.Any();

            m_DatabaseManagerLegacy = m_Mocks.DynamicMock<IDatabaseManagerLegacy>();

            var storageCacheService = MockRepository.GenerateStub<IStorageCacheService>();
            storageCacheService.Stub(x => x.GetStorage(null)).IgnoreArguments().Return(m_Storage).Repeat.Any();
            storageCacheService.Stub(x => x.GetDatabaseManagerLegacy(null)).IgnoreArguments().Return(m_DatabaseManagerLegacy).Repeat.Any();
            TestHelper.AddService(storageCacheService);
            TestHelper.AddService(MockRepository.GenerateStub<ISdCardCeService>());

            m_AuditTrailService = new AuditTrailServiceCF(false);
            m_IAuditTrailService = m_AuditTrailService;

            m_AuditTrail = m_Mocks.DynamicMock<IAuditTrail>();
            m_AuditTrail.Stub(x => x.MaxSize).Return(1);
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

            using (m_Mocks.Record())
            {
                m_Storage.Query.Insert(null);
                LastCall.Repeat.Once().Constraints(Is.Matching<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).Message == msg)));
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogMessage(msg);
            }
        }

        [Test]
        public void LogMessageLogsUsername()
        {
            using (m_Mocks.Record())
            {
                m_Query.Insert(null);
                LastCall.Repeat.Once().Constraints(Is.Matching<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).UserName == UserName)));
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogMessage(string.Empty);
            }
        }

        [Test]
        public void LogMessageDoesNotLogIfAuditTrailItemDoesntExist()
        {
            m_IAuditTrailService.AuditTrail = null;

            using (m_Mocks.Record())
            {
                m_Query.Insert(null);
                LastCall.Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogMessage(string.Empty);
            }
        }

        [Test]
        public void LogDataItemChangedInsertsTableName()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => storableItem.TableName == TableName));
        }

        [Test]
        public void LogDataItemChangedInsertsDateTimeNow()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => IsAlmostNow(((AuditStorableItem)storableItem).TimeStamp)));
        }

        [Test]
        public void LogDataItemChangedInsertsValueBefore()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).ValueBefore == ValueBefore));
        }

        [Test]
        public void LogDataItemChangedInsertsValueAfter()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).ValueAfter == ValueAfter));
        }

        [Test]
        public void LogDataItemChangedInsertsUserName()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).UserName == UserName));
        }

        [Test]
        public void LogDataItemChangedInsertsMessage()
        {
            LogDataItemChanged_VerifyAuditStorableItemProperty<IStorableItem[]>(storableItems => storableItems.All(storableItem => ((AuditStorableItem)storableItem).Message.Contains(DataItem)));
        }

        [Test]
        public void LogDataItemChangedLogsNothingIfAuditTrailIsNull()
        {
            m_IAuditTrailService.AuditTrail = null;

            using (m_Mocks.Record())
            {
                m_Query.Insert(null);
                LastCall.IgnoreArguments().Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogDataItemChanged(DataItem, new VariantValue(1), new VariantValue(2));
            }
        }

        [Test]
        public void GetLogItemsReturnsNullIfDatabaseNotExists()
        {
            using (m_Mocks.Record())
            {
                Expect.Call(m_Storage.Query.Select(_ => Enumerable.Empty<AuditStorableItem>(), null)).IgnoreArguments().Repeat.Once().Throw(new StorageReaderException(string.Empty));
            }

            using (m_Mocks.Playback())
            {
                IList<IAuditStorableItem> list = m_IAuditTrailService.GetLogItems();
                Assert.IsNull(list);
            }
        }

        [Test]
        public void GetLogItemsReturnsNullInDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            using (m_Mocks.Record())
            {
                Expect.Call(m_Query.Select(_ => Enumerable.Empty<AuditStorableItem>(), null)).IgnoreArguments().Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                IList<IAuditStorableItem> list = m_IAuditTrailService.GetLogItems(); //After executing this the m_ToolManager.Runtime stub will go back to default value true (set up in fixture set up)

                Assert.IsNull(list);
            }

            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void LogMessageDoesNotLogWhenDatabaseSizeIsVeryLarge()
        {
            string msg = "a message";

            m_Storage.Stub(x => x.Size).Return(long.MaxValue);

            using (m_Mocks.Record())
            {
                m_Query.Insert(null);
                LastCall.Repeat.Never().IgnoreArguments();
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogMessage(msg);
            }
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
            using (m_Mocks.Record())
            {
                Expect.Call(m_AuditTrail.SuppressedLogActionNameList).Return(actionList);
            }

            using (m_Mocks.Playback())
            {
                Assert.AreEqual(expected, m_AuditTrailService.IsLogActionEnabled(actionName));
            }
        }

        private void LogDataItemChanged_VerifyAuditStorableItemProperty<T>(Predicate<T> predicate)
        {
            using (m_Mocks.Record())
            {
                m_Storage.Query.Insert(null);
                LastCall.Repeat.Once().Constraints(Is.Matching<T>(predicate));
            }

            using (m_Mocks.Playback())
            {
                m_IAuditTrailService.LogDataItemChanged(DataItem, new VariantValue(1), new VariantValue(2));
            }
        }

        private static bool IsAlmostNow(DateTime dateTimeUnderTest)
        {
            TimeSpan timeSpan = DateTime.Now - dateTimeUnderTest;

            return timeSpan.Seconds < 10;
        }
    }
}
