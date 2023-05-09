using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Interfaces.OpcUaServer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcUaServer
{
    public class OpcUaServerNativeWrapperDummy : IOpcUaServerNativeWrapper
    {
        IOpcUaServerRootComponent IOpcUaServerNativeWrapper.OpcUaServerRootComponent { set => throw new NotImplementedException(); }

        bool IOpcUaServerNativeWrapper.Init(string configFilename)
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.InitNodeManager(string[] tagNames, int[] updateRates, IntPtr defaultValueVariants, uint[] accessRights, string[] descriptions, string[] stringNodeIDs, uint count)
        {
            throw new NotImplementedException();
        }

        uint IOpcUaServerNativeWrapper.OpcServerGetMutex()
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.OpcServerReleaseMutex()
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.OpcServerSubscriptionWrite(int[] cookies, uint count, IntPtr writeVariants, short[] opcDaQualities, int samplingInterval, int[] writeResults)
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.SoftInit()
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.SoftStop(string shutdownReason)
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.Start()
        {
            throw new NotImplementedException();
        }

        bool IOpcUaServerNativeWrapper.Stop(string shutdownReason)
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class OpcUaServerDataManagerTest
    {
        private const int TagCount = 10;
        private const int MinInterval = 100;
        private const int BelowMinInterval = 10;
        private ICachedDataItemService m_CachedDataItemService;
        private IOpcUaServerNativeWrapper m_OpcUaServerNativeWrapper;
        private OpcUaDataManagerTestExtension m_OpcUaDataManager;
        private IDictionary<int, OpcUaServerTagInfo> m_TagInfoDictionary;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            IDataItemProxy dataItemProxyStub = MockRepository.GenerateStub<IDataItemProxy>();

            m_OpcUaServerNativeWrapper = MockRepository.GenerateMock<OpcUaServerNativeWrapperDummy>();

            m_CachedDataItemService = MockRepository.GenerateMock<ICachedDataItemService>();
            m_CachedDataItemService.Stub(x => x.GetTag(Arg<string>.Is.Anything, Arg<ILifetimeContext>.Is.Anything)).Return(dataItemProxyStub);

            IDictionary<string, IDataItemProxy> dataItemProxiesStub = MockRepository.GenerateStub<IDictionary<string, IDataItemProxy>>();
            dataItemProxiesStub.Stub(x => x.Values).Return(new List<IDataItemProxy>());

            m_CachedDataItemService.Stub(x => x.GetTags(Arg<IEnumerable<string>>.Is.Anything, Arg<ILifetimeContext>.Is.Anything)).Return(dataItemProxiesStub);

            IDictionary<string, VariantValue> dataItemValuesStub = MockRepository.GenerateStub<IDictionary<string, VariantValue>>();
            m_CachedDataItemService.Stub(x => x.GetTagValues(Arg<IEnumerable<string>>.Is.Anything, Arg<ILifetimeContext>.Is.Anything)).Return(dataItemValuesStub);
        }

        [SetUp]
        public void SetUp()
        {
            PopulateTagInfoDictionary();
            m_OpcUaDataManager = new OpcUaDataManagerTestExtension(m_TagInfoDictionary, m_OpcUaServerNativeWrapper, m_CachedDataItemService.ToILazy());
        }

        [Test]
        public void InitialStateIsCorrect()
        {
            VerifyDataStructuresCleared();
        }

        #region Subscribe tests
        [TestCase(1, new int[] { 1, 2, 3 }, new int[] { 100, 100, 100 },
                    null, null,
                    new int[] { 1, 2, 3 }, new int[] { 100, 100, 100 })]
        [TestCase(2, new int[] { 1, }, new int[] { 100 },
                    new int[] { 2, 3 }, new int[] { 100, 100 },
                    new int[] { 1, 2, 3 }, new int[] { 100, 100, 100 })]
        [TestCase(3, new int[] { 1, 1, 2, 3, 4, 1 }, new int[] { 100, 100, 500, 100, 200, 200 },
                    null, null,
                    new int[] { 1, 1, 2, 3, 4, 1 }, new int[] { 100, 100, 500, 100, 200, 200 })]
        [TestCase(4, new int[] { 1, 2 }, new int[] { 100, 100 },
                    new int[] { 1, 2 }, new int[] { BelowMinInterval, BelowMinInterval },
                    new int[] { 1, 1, 2, 2 }, new int[] { 100, MinInterval, 100, MinInterval })]
        public void SubscribeProducesCorrectDataStructures(int testCaseId,
            int[] cookies, int[] rates, int[] moreCookies, int[] moreRates, int[] expectedCookies, int[] expectedRates)
        {
            Subscribe(cookies, rates);
            if (moreCookies != null)
            {
                Subscribe(moreCookies, moreRates);
            }

            VerifyDataStructures(expectedCookies, expectedRates);
        }

        [Test]
        public void SubscribeResultsAreCorrect()
        {
            var initialCookies = new int[] { 3, 4 };
            var initialRates = new int[] { 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, initialRates);

            bool[] results = new bool[5];
            int[] revisedUpdateRates = new int[5];
            int[] newCookies = new int[] { -1, 1, 2, 3, 4 };
            int[] newRates = new int[] { 100, 200, BelowMinInterval, 500, BelowMinInterval };
            m_OpcUaDataManager.Subscribe(newCookies, newRates, out revisedUpdateRates, out results);
            m_OpcUaDataManager.SubscribeReady(newCookies); // Simulate subscribe ready from native code

            Assert.That(results[0], Is.False);
            Assert.That(revisedUpdateRates[0], Is.EqualTo(0));

            Assert.That(results[1], Is.True);
            Assert.That(revisedUpdateRates[1], Is.EqualTo(200));

            Assert.That(results[2], Is.True);
            Assert.That(revisedUpdateRates[2], Is.EqualTo(MinInterval)); // was below update limit

            Assert.That(results[3], Is.True);
            Assert.That(revisedUpdateRates[3], Is.EqualTo(500));

            Assert.That(results[4], Is.True);
            Assert.That(revisedUpdateRates[4], Is.EqualTo(MinInterval)); // was below update limit

            VerifyDataStructures(new int[] { 3, 4, 1, 2, 3, 4 }, new int[] { 300, 400, 200, MinInterval, 500, MinInterval });
            VerifyTimerIntervals(new int[] { MinInterval, 200, 300, 400, 500 });
        }
        #endregion

        #region Unsubscribe tests
        [Test]
        public void DataStructuresAndTimerIntervalsAreCleareAfterAllSubscriptionsRemoved()
        {
            int[] cookies = new int[] { 1, 2, 3, 1, 2, 3 };
            int[] rates = new int[] { 100, 100, 100, 200, 200, 200 };

            Subscribe(cookies, rates);
            VerifyDataStructures(cookies, rates);

            Unsubscribe(cookies, rates);
            VerifyDataStructures(new int[0], new int[0]);
            VerifyTimerIntervals(new int[0]);
        }

        [TestCase(1, new int[] { 2 }, new int[] { 200 },
                    null, null,
                    new int[] { 1, 3, 4 }, new int[] { MinInterval, 300, 400 })]
        [TestCase(2, new int[] { 2 }, new int[] { 200 },
                    new int[] { 3 }, new int[] { 300 },
                    new int[] { 1, 4 }, new int[] { MinInterval, 400 })]
        [TestCase(3, new int[] { 1 }, new int[] { MinInterval },
                    new int[] { 3, 4 }, new int[] { 300, 400 },
                    new int[] { 2 }, new int[] { 200 })]
        [TestCase(4, new int[] { 1 }, new int[] { MinInterval },
                    null, null,
                    new int[] { 2, 3, 4 }, new int[] { 200, 300, 400 })]
        public void UnsubscribeAffectsDataStructuresCorrectly(int testCaseId,
            int[] cookies, int[] rates,
            int[] moreCookies, int[] moreRates,
            int[] expectedCookies, int[] expectedRates)
        {
            var initialCookies = new int[] { 1, 2, 3, 4 };
            var initialRates = new int[] { BelowMinInterval, 200, 300, 400 };
            var expectedInitialRevisedRates = new int[] { MinInterval, 200, 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, expectedInitialRevisedRates);

            Unsubscribe(cookies, rates);
            if (moreCookies != null)
            {
                Unsubscribe(moreCookies, moreRates);
            }

            VerifyDataStructures(expectedCookies, expectedRates);
        }

        [Test]
        public void UnsubscribeOnNonExistingSubscriptionsDoesNotAffectDataStructures()
        {
            var initialCookies = new int[] { 1, 2, 3, 4 };
            var initialRates = new int[] { 100, 200, 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, initialRates);

            bool[] results = new bool[3];
            m_OpcUaDataManager.Unsubscribe(new int[] { 4, 5, -1 }, new int[] { 500, 500, 400 }, out results);

            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.False);
            Assert.That(results[2], Is.False);

            VerifyDataStructures(initialCookies, initialRates);
            VerifyTimerIntervals(new int[] { 100, 200, 300, 400 });
        }
        #endregion

        #region Modify subscription tests
        [TestCase(1, new int[] { 2 }, new int[] { 200 }, new int[] { 300 },
                   null, null, null,
                   new int[] { 1, 2, 3, 4 }, new int[] { MinInterval, 300, 300, 400 })]
        [TestCase(2, new int[] { 1, 2 }, new int[] { MinInterval, 200 }, new int[] { 200, 300 },
                   null, null, null,
                   new int[] { 1, 2, 3, 4 }, new int[] { 200, 300, 300, 400 })]
        [TestCase(3, new int[] { 2 }, new int[] { 200 }, new int[] { 300 },
                   new int[] { 2 }, new int[] { 300 }, new int[] { 400 },
                   new int[] { 1, 2, 3, 4 }, new int[] { MinInterval, 400, 300, 400 })]
        public void ModifySubscriptionAffectsDataStructuresCorrectly(int testCaseId,
            int[] cookies, int[] oldRates, int[] newRates,
            int[] moreCookies, int[] moreOldRates, int[] moreNewRates,
            int[] expectedCookies, int[] expectedRates)
        {
            var initialCookies = new int[] { 1, 2, 3, 4 };
            var initialRates = new int[] { 10, 200, 300, 400 };
            var expectedInitialRevisedRates = new int[] { MinInterval, 200, 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, expectedInitialRevisedRates);

            ModifySubscription(cookies, oldRates, newRates);
            if (moreCookies != null)
            {
                ModifySubscription(moreCookies, moreOldRates, moreNewRates);
            }

            VerifyDataStructures(expectedCookies, expectedRates);
        }

        [Test]
        public void ModifyOnNonExistingSubscriptionsDoesNotAffectDataStructures()
        {
            var initialCookies = new int[] { 1, 2, 3, 4 };
            var initialRates = new int[] { 10, 200, 300, 400 };
            var expectedInitialRevisedRates = new int[] { MinInterval, 200, 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, expectedInitialRevisedRates);

            bool[] results = new bool[3];
            int[] revisedUpdateRates = new int[3];
            m_OpcUaDataManager.ModifySubscription(new int[] { 4, 5, -1 }, new int[] { 500, 400, 0 }, new int[] { 500, 500, 500 }, out revisedUpdateRates, out results);

            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.False);
            Assert.That(results[2], Is.False);

            VerifyDataStructures(initialCookies, expectedInitialRevisedRates);
        }

        [Test]
        public void ModifySubscriptionResultsAreCorrect()
        {
            var initialCookies = new int[] { 3, 4 };
            var initialRates = new int[] { 300, 400 };

            Subscribe(initialCookies, initialRates);
            VerifyDataStructures(initialCookies, initialRates);

            bool[] results = new bool[2];
            int[] revisedUpdateRates = new int[2];
            // Attempt to modify non existing subscriptions
            m_OpcUaDataManager.ModifySubscription(new int[] { -1, 3 }, new int[] { 100, 5000 }, new int[] { BelowMinInterval, BelowMinInterval }, out revisedUpdateRates, out results);

            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.False);

            // Attempt to modify existing subscriptions
            m_OpcUaDataManager.ModifySubscription(new int[] { 3, 4 }, new int[] { 300, 400 }, new int[] { 200, BelowMinInterval }, out revisedUpdateRates, out results);
            Assert.That(results[0], Is.True);
            Assert.That(revisedUpdateRates[0], Is.EqualTo(200));

            Assert.That(results[1], Is.True);
            Assert.That(revisedUpdateRates[1], Is.EqualTo(MinInterval)); // was below update limit

            VerifyDataStructures(new int[] { 3, 4 }, new int[] { 200, MinInterval });
            VerifyTimerIntervals(new int[] { 100, 200 });
        }
        #endregion

        #region Timer interval tests
        [Test]
        public void TimerIntervalsAreUpdatedAfterSubscriptionsAdded()
        {
            var cookies = new int[] { 1 };
            var rates = new int[] { 200 };

            Subscribe(cookies, rates);
            VerifyTimerIntervals(rates);

            Subscribe(cookies, rates);
            VerifyTimerIntervals(rates);

            rates = new int[] { 150 };
            Subscribe(cookies, rates);
            VerifyTimerIntervals(new int[] { 150, 200 });

            rates = new int[] { BelowMinInterval };
            Subscribe(cookies, rates);
            VerifyTimerIntervals(new int[] { MinInterval, 150, 200 });

            cookies = new int[] { 2, 3 };
            rates = new int[] { 200, 300 };
            Subscribe(cookies, rates);
            VerifyTimerIntervals(new int[] { MinInterval, 150, 200, 300 });
        }

        [Test]
        public void TimerIntervalsAreUpdatedAfterSubscriptionsModified()
        {
            var initialCookies = new int[] { 1, 1, 1, 1, 2, 3 };
            var initialRates = new int[] { 200, 200, 150, BelowMinInterval, 200, 300 };
            Subscribe(initialCookies, initialRates);
            VerifyTimerIntervals(new int[] { MinInterval, 150, 200, 300 });

            var cookies = new int[] { 1, 2, 3 };
            var oldRates = new int[] { MinInterval, 200, 300 };
            var rates = new int[] { 500, 150, 400 };
            ModifySubscription(cookies, oldRates, rates);
            VerifyTimerIntervals(new int[] { 150, 200, 400, 500 });
        }

        [Test]
        public void TimerIntervalsAreUpdatedAfterSubscriptionsRemoved()
        {
            var initialCookies = new int[] { 1, 1, 1, 1, 2, 3 };
            var initialRates = new int[] { 150, 200, 200, 500, 150, 400 };
            Subscribe(initialCookies, initialRates);
            VerifyTimerIntervals(new int[] { 150, 200, 400, 500 });

            var cookies = new int[] { 1, 1, 1 };
            var rates = new int[] { 500, 200, 150 };
            Unsubscribe(cookies, rates);
            VerifyTimerIntervals(new int[] { 150, 200, 400 });

            cookies = new int[] { 1 };
            rates = new int[] { 200 };
            Unsubscribe(cookies, rates);
            VerifyTimerIntervals(new int[] { 150, 400 });

            cookies = new int[] { 2, 3 };
            rates = new int[] { 150, 400 };
            Unsubscribe(cookies, rates);
            VerifyTimerIntervals(new int[0]);
        }
        #endregion

        #region Test Helper Fucntions
        private void Subscribe(int[] cookies, int[] updateRates)
        {
            int[] revisedUpdateRates = new int[updateRates.Length];
            bool[] results = new bool[updateRates.Length];

            m_OpcUaDataManager.Subscribe(cookies, updateRates, out revisedUpdateRates, out results);
            m_OpcUaDataManager.SubscribeReady(cookies); // Simulate subscribe ready from native code
        }

        private void ModifySubscription(int[] cookies, int[] oldUpdateRates, int[] newUpdateRates)
        {
            int[] revisedUpdateRates = new int[newUpdateRates.Length];
            bool[] results = new bool[newUpdateRates.Length];

            m_OpcUaDataManager.ModifySubscription(cookies, oldUpdateRates, newUpdateRates, out revisedUpdateRates, out results);
        }

        private void Unsubscribe(int[] cookies, int[] updateRates)
        {
            bool[] results = new bool[updateRates.Length];

            m_OpcUaDataManager.Unsubscribe(cookies, updateRates, out results);
        }

        private void PopulateTagInfoDictionary()
        {
            m_TagInfoDictionary = new Dictionary<int, OpcUaServerTagInfo>();
            
            for (int cookie = 0; cookie < TagCount; cookie++)
            {
                IGlobalDataItem globalDataItemStub = MockRepository.GenerateStub<IGlobalDataItem>();
                globalDataItemStub.Name = string.Format("Tag{0}", cookie);
                globalDataItemStub.Stub(x => x.UpdateRate).Return(MinInterval);

                OpcUaServerTagInfo opcUaServerTagInfo = new OpcUaServerTagInfo(globalDataItemStub, cookie);
                m_TagInfoDictionary.Add(cookie, opcUaServerTagInfo);
            }
        }

        private void VerifyTimerIntervals(IEnumerable<int> expectedIntervals)
        {
            expectedIntervals.OrderBy(x => x);
            var actualIntervals = m_OpcUaDataManager.CurrentTimerIntervals.OrderBy(x => x);

            Assert.That(Enumerable.SequenceEqual(actualIntervals, expectedIntervals), Is.True, "Unexpected sequence of timer intervals.");
        }

        private void VerifyDataStructures(int[] expectedCookies, int[] expectedUpdateRates)
        {
            Assert.That(expectedCookies.Length == expectedUpdateRates.Length, Is.True);

            if (expectedCookies.Length == 0)
            {
                VerifyDataStructuresCleared();
                return;
            }

            var expectedTagInfos = GetExpectedTagInfos(expectedCookies, expectedUpdateRates);

            VerifyTagInfosByUpdateRate(expectedTagInfos);
            VerifyUpdateRatesForAllTagInfos(expectedTagInfos);
        }

        private void VerifyDataStructuresCleared()
        {
            Assert.That(m_OpcUaDataManager.SubscriptionTagInfosByUpdateRate, Is.Empty);
            foreach (OpcUaServerTagInfo tagInfo in m_OpcUaDataManager.TagInfoDictionary.Values)
            {
                Assert.That(tagInfo.HasAnySubscription, Is.False);
            }
        }

        private void VerifyTagInfosByUpdateRate(IEnumerable<ExpectedTagInfo> expectedTagInfos)
        {
            var expectedUpdateRates = expectedTagInfos.Select(x => x.UpdateRates).Aggregate((result, next) => result.Concat(next)).Distinct();
            var existingUpdateRates = m_OpcUaDataManager.SubscriptionTagInfosByUpdateRate.Keys;
            bool isEqualUpdateRates = Enumerable.SequenceEqual(expectedUpdateRates.OrderBy(x => x), existingUpdateRates.OrderBy(x => x));
            Assert.That(isEqualUpdateRates, Is.True, "Update rates did not match expectations.");

            // Assert that expected cookies are found for each update rate
            foreach (int expectedUpdateRate in expectedUpdateRates)
            {
                IList<OpcUaServerTagInfo> tagInfos = null;
                m_OpcUaDataManager.SubscriptionTagInfosByUpdateRate.TryGetValue(expectedUpdateRate, out tagInfos);
                Assert.That(tagInfos, Is.Not.Null, "Could not find tag infos for expected update rate.");

                var cookiesInTagInfos = tagInfos.Select(x => x.Cookie).OrderBy(x => x);
                var expectedTagInfosWithMatchingUpdateRate = expectedTagInfos.Where(x => x.UpdateRates.Contains(expectedUpdateRate));
                var cookiesInExpectedTagInfos = expectedTagInfosWithMatchingUpdateRate.Select(x => x.Cookie).OrderBy(x => x);

                bool isEqualCookies = Enumerable.SequenceEqual(cookiesInTagInfos, cookiesInExpectedTagInfos);
                Assert.That(isEqualCookies, Is.True, "Could not find all expected cookies in tag infos.");
            }
        }

        private void VerifyUpdateRatesForAllTagInfos(IEnumerable<ExpectedTagInfo> expectedTagInfos)
        {
            foreach (ExpectedTagInfo expectedTagInfo in expectedTagInfos)
            {
                OpcUaServerTagInfo tagInfo = null;
                m_OpcUaDataManager.TagInfoDictionary.TryGetValue(expectedTagInfo.Cookie, out tagInfo);
                Assert.That(tagInfo, Is.Not.Null);

                var distinctUpdateRates = expectedTagInfo.UpdateRates.Distinct();
                foreach (int updateRate in distinctUpdateRates)
                {
                    int expectedReferenceCount = expectedTagInfo.UpdateRates.Where(x => x == updateRate).Count();
                    Assert.That(tagInfo.GetSubscriptionCount(updateRate), Is.EqualTo(expectedReferenceCount));
                }
            }
        }

        private IEnumerable<ExpectedTagInfo> GetExpectedTagInfos(int[] cookies, int[] updateRates)
        {
            IList<Tuple<int, int>> cookieUpdateRatePairs = new List<Tuple<int, int>>();
            for (int i = 0; i < cookies.Length; i++)
            {
                cookieUpdateRatePairs.Add(Tuple.Create(cookies[i], updateRates[i]));
            }

            var ratesByCookie = from pair in cookieUpdateRatePairs
                                let cookie = pair.Item1
                                group new { pair.Item2 } by cookie into newGroup
                                select newGroup;

            var result = new List<ExpectedTagInfo>();

            foreach (var cookie in ratesByCookie)
            {
                IList<int> rates = new List<int>();
                foreach (var rate in cookie)
                {
                    rates.Add(rate.Item2);
                }

                result.Add(new ExpectedTagInfo(cookie.Key, rates));
            }

            return result;
        }

        #endregion
    }

    #region ExpectedTagInfo
    internal class ExpectedTagInfo
    {
        internal ExpectedTagInfo(int cookie, IEnumerable<int> updateRates)
        {
            Cookie = cookie;
            UpdateRates = updateRates;
        }

        public int Cookie { get; }
        public IEnumerable<int> UpdateRates { get; }
    }
    #endregion

    #region OpcUaDataManagerTestExtension
    internal class OpcUaDataManagerTestExtension : OpcUaServerDataManager
    {
        internal OpcUaDataManagerTestExtension(IDictionary<int, OpcUaServerTagInfo> tagInfoDictionary, IOpcUaServerNativeWrapper opcUaServerNativeWrapper, ILazy<ICachedDataItemService> cachedDataItemService)
            : base(tagInfoDictionary, opcUaServerNativeWrapper, cachedDataItemService)
        {
        }

        internal IDictionary<int, OpcUaServerTagInfo> TagInfoDictionary
        {
            get { return m_TagInfoDictionary; }
        }

        internal IDictionary<int, IList<OpcUaServerTagInfo>> SubscriptionTagInfosByUpdateRate
        {
            get { return m_TagInfosByUpdateRate; }
        }

        internal IEnumerable<int> CurrentTimerIntervals
        {
            get { return m_IntervalElapsedNotifier.CurrentIntervals.Select(x => x.Milliseconds); }
        }
    }
    #endregion
}
