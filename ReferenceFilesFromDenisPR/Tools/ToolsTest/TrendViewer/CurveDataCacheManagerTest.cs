using System.Collections.Generic;
using System.Linq;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.TrendViewer
{
    [TestFixture]
    public class CurveDataCacheManagerTest
    {
        private ICurveDataCacheManager m_CurveDataCache;
        private IGlobalReferenceService m_GlobalReferenceService;

        private IValue m_DataSourceOne;
        private IValue m_DataSourceTwo;
        private IValue m_DataSourceThree;

        private const string TrendViewerNameOne = "Screen1.TrendViewer1";
        private const string TrendViewerNameTwo = "Screen1.TrendViewer2";
        private const string TrendViewerNameThree = "Screen2.TrendViewer1";
        private const string TrendViewerNameFour = "Screen2.TrendViewer2";
        private const string TrendViewerNameFive = "Screen3.TrendViewer1";

        private const string DataSourceNameOne = "Tags.Tag1";
        private static readonly string DataSourceNameTwo = StringConstants.TagsRoot + "Tag2";
        private static readonly string DataSourceNameThree = StringConstants.TagsRoot + "Tag3";

        [SetUp]
        public void SetUp()
        {
            m_DataSourceOne = Substitute.For<IValue>();
            m_DataSourceTwo = Substitute.For<IValue>();
            m_DataSourceThree = Substitute.For<IValue>();

            AddServices();

            m_CurveDataCache = new ExtendedCacheManager();

            InitCaches();
        }

        private ExtendedCacheManager ExtendedCacheManager
        {
            get { return m_CurveDataCache as ExtendedCacheManager; }
        }

        private void AddServices()
        {
            m_GlobalReferenceService = Substitute.For<IGlobalReferenceService>();
            //m_GlobalReferenceService.GetObject(Arg<string>()).Returns(valueStub);
            m_GlobalReferenceService.GetObject<IValue>(DataSourceNameOne).Returns(m_DataSourceOne);
            m_GlobalReferenceService.GetObject<IValue>(DataSourceNameTwo).Returns(m_DataSourceTwo);
            m_GlobalReferenceService.GetObject<IValue>(DataSourceNameThree).Returns(m_DataSourceThree);

            TestHelper.ClearServices();
            TestHelper.AddService(m_GlobalReferenceService);
        }

        private void InitCaches()
        {
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne, DataSourceNameTwo, DataSourceNameThree }, 1500, 200);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameTwo, new string[] { DataSourceNameOne, DataSourceNameTwo }, 1500, 200);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameThree, new string[] { DataSourceNameOne, DataSourceNameTwo }, 1000, 150);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameFour, new string[] { DataSourceNameOne, DataSourceNameTwo }, 3000, 150);
        }

        [Test]
        [TestCase(1500, 200, 3)]
        [TestCase(1000, 150, 2)]
        [TestCase(3000, 150, 2)]
        public void VerifyInitialCaches(int samplingInterval, int bufferSize, int expectedCacheCount)
        {
            var caches = GetCachesBySamplingIntervalAndAssertCount(samplingInterval, expectedCacheCount);
            VerifyPropertiesForCurveDataCache(caches, samplingInterval, bufferSize);
        }

        [Test]
        public void VerifyAddCachesNewSamplingIntervalNewBufferSize()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 200);

            var caches = GetCachesBySamplingIntervalAndAssertCount(1500, 1);

            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
        }

        [Test]
        public void VerifyAddCachesMatchingSamplingIntervalNewBufferSize()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 200);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameTwo, new string[] { DataSourceNameOne }, 1500, 300);

            var caches = GetCachesBySamplingIntervalAndAssertCount(1500, 2);

            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
            VerifyPropertiesForCurveDataCache(cachesWithCorrectReferences, 1500, 200);

            cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameTwo);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
            VerifyPropertiesForCurveDataCache(cachesWithCorrectReferences, 1500, 300);
        }

        [Test]
        public void VerifyAddCachesMatchingSamplingIntervalMatchingBufferSize()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 200);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameTwo, new string[] { DataSourceNameOne }, 1500, 200);

            var caches = GetCachesBySamplingIntervalAndAssertCount(1500, 1);
            VerifyPropertiesForCurveDataCache(caches, 1500, 200);

            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));

            cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameTwo);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
        }

        [Test]
        public void VerifyGetCurveDataCachesBySamplingInterval()
        {
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(3));

            var curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1500);
            Assert.That(curveDataCaches, Is.Not.Null);
            Assert.That(curveDataCaches.Count(), Is.EqualTo(3));

            curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1000);
            Assert.That(curveDataCaches, Is.Not.Null);
            Assert.That(curveDataCaches.Count(), Is.EqualTo(2));
        }

        [Test]
        public void VerifyTrendViewerReferencesOnCurveDataCaches()
        {
            var curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1500);

            ICurveDataCache curveDataCache = curveDataCaches.Where(x => string.Compare(DataSourceNameOne, x.DataSourceName) == 0).FirstOrDefault();
            Assert.That(curveDataCache.TrendViewerReferences.Count, Is.EqualTo(2));
            Assert.That(curveDataCache.TrendViewerReferences.Contains(TrendViewerNameOne), Is.True);
            Assert.That(curveDataCache.TrendViewerReferences.Contains(TrendViewerNameTwo), Is.True);

            curveDataCache = curveDataCaches.Where(x => string.Compare(DataSourceNameThree, x.DataSourceName) == 0).FirstOrDefault();
            Assert.That(curveDataCache.TrendViewerReferences.Count, Is.EqualTo(1));
            Assert.That(curveDataCache.TrendViewerReferences.Contains(TrendViewerNameOne), Is.True);
        }

        [Test]
        [TestCase(1500, 200, TrendViewerNameOne, DataSourceNameOne)]
        [TestCase(1000, 150, TrendViewerNameThree, DataSourceNameOne)]
        public void VerifyGetCurveDataCacheForTrendViewerDataSource(int samplingInterval, int bufferSize, string trendViewerName, string dataSourceName)
        {
            ICurveDataCache curveDataCache = m_CurveDataCache.GetCurveDataCache(trendViewerName, dataSourceName);

            Assert.That(curveDataCache, Is.Not.Null);
            Assert.That(curveDataCache.DataSourceName, Is.EqualTo(dataSourceName));
            Assert.That(curveDataCache.SamplingInterval, Is.EqualTo(samplingInterval));
            Assert.That(curveDataCache.Capacity, Is.EqualTo(bufferSize));
        }

        [Test]
        [TestCase(TrendViewerNameOne, 3)]
        [TestCase(TrendViewerNameTwo, 2)]
        [TestCase(TrendViewerNameThree, 2)]
        public void VerifyGetCurveDataCachesByTrendViewerName(string trendViewerName, int expectedCacheCount)
        {
            IEnumerable<ICurveDataCache> curveDataCaches = m_CurveDataCache.GetCurveDataCaches(trendViewerName);
            Assert.That(curveDataCaches, Is.Not.Null);
            Assert.That(curveDataCaches.Count(), Is.EqualTo(expectedCacheCount));
        }

        [Test]
        public void VerifyDistinctDataSources()
        {
            IEnumerable<IValue> dataSources = m_CurveDataCache.DataSourcesCached;

            Assert.That(dataSources, Is.Not.Null);
            Assert.That(dataSources.Count(), Is.EqualTo(3));

            Assert.That(dataSources.Contains(m_DataSourceOne), Is.True);
            Assert.That(dataSources.Contains(m_DataSourceTwo), Is.True);
            Assert.That(dataSources.Contains(m_DataSourceThree), Is.True);
        }

        #region Update Cache Parameters
        [Test]
        public void VerifyUpdateCacheParametersListRemovedForUnusedSamplingInterval()
        {
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameThree, 4000, 200);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.ContainsKey(1000), Is.False);
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1000), Is.Null);
        }

        [Test]
        [TestCase(1500, 200, 3000, 200, 3000, 200)] // new sampling interval
        [TestCase(1500, 200, 1500, 300, 1500, 300)] // new buffer size
        [TestCase(1500, 200, 1500, 200, 1500, 200)] // same parameters
        public void VerifyUpdateCacheParametersOneCacheOneDataSource(int oldInterval, int oldSize,
            int newInterval, int newSize, int expectedInterval, int expectedSize)
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, oldInterval, oldSize);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, newInterval, newSize);

            ICurveDataCache curveDataCache = m_CurveDataCache.GetCurveDataCache(TrendViewerNameOne, DataSourceNameOne);
            Assert.That(curveDataCache, Is.Not.Null);
            Assert.That(curveDataCache.DataSourceName, Is.EqualTo(DataSourceNameOne));
            Assert.That(curveDataCache.SamplingInterval, Is.EqualTo(expectedInterval));
            Assert.That(curveDataCache.Capacity, Is.EqualTo(expectedSize));
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval[expectedInterval].Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyUpdateCacheParametersSizeCanNotDecrease()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 600);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, 1000, 15);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.ContainsKey(1000), Is.True);
            var curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1000);
            VerifyPropertiesForCurveDataCache(curveDataCaches, 1000, 600);

            Assert.That(m_CurveDataCache.GetCurveDataCaches(1500), Is.Null);
        }

        [Test]
        public void VerifyUpdateCacheParametersSharedToNew()
        {
            // Change sampling interval from 1500 (shared) to 4000 (new)
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameTwo, 4000, 400);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));

            var caches = GetCachesBySamplingIntervalAndAssertCount(1500, 3);

            // Verify references removed from (previoulsy) shared cache
            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(3));

            // Verify new caches created
            caches = GetCachesBySamplingIntervalAndAssertCount(4000, 2);

            // Verify references added to new caches
            cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameTwo);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            // Verify caches for all data sources added
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            VerifyPropertiesForCurveDataCache(caches, 4000, 400);
        }

        [Test]
        public void VerifyUpdateCacheParametersSharedToExisting()
        {
            // Change sampling interval from 1500 (shared) to 2000 (existing)
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameFive, new string[] { DataSourceNameOne, DataSourceNameTwo }, 2000, 250);
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, 2000, 250);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));
            var caches = GetCachesBySamplingIntervalAndAssertCount(2000, 3);

            VerifyPropertiesForCurveDataCache(caches, 2000, 250);

            // Verify caches for all data sources exists
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameThree) && x.DataSource == m_DataSourceThree), Is.True);

            // Verify references for existing caches
            var cachesWithCorrectReferences = caches.Where(x => x.TrendViewerReferences.Contains(TrendViewerNameOne));
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(3));

            cachesWithCorrectReferences = caches.Where(x => x.TrendViewerReferences.Contains(TrendViewerNameFive));
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            Assert.That(caches.Where(x => x.DataSourceName.Equals(DataSourceNameOne)).FirstOrDefault().TrendViewerReferences.Count(), Is.EqualTo(2));
            Assert.That(caches.Where(x => x.DataSourceName.Equals(DataSourceNameTwo)).FirstOrDefault().TrendViewerReferences.Count(), Is.EqualTo(2));
            Assert.That(caches.Where(x => x.DataSourceName.Equals(DataSourceNameThree)).FirstOrDefault().TrendViewerReferences.Count(), Is.EqualTo(1));
        }

        [Test]
        public void VerifyUpdateCacheParametersNotSharedToNew()
        {
            // Change sampling interval from 1000 (not shared) to 4000 (new)
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1000), Is.Not.Null);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameThree, 4000, 200);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(3));

            // Verify old cache removed
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1000), Is.Null);

            // Verify new cache created
            var caches = GetCachesBySamplingIntervalAndAssertCount(4000, 2);

            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameThree);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            VerifyPropertiesForCurveDataCache(caches, 4000, 200);
        }

        [Test]
        public void VerifyUpdatetCacheParametersNotSharedToExisting()
        {
            // Change sampling interval from 3000 (not shared) to 1000 (existing)
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameFour, 1000, 150);
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(2));

            // Verify old caches removed
            Assert.That(m_CurveDataCache.GetCurveDataCaches(3000), Is.Null);

            // Verify existing caches
            var caches = GetCachesBySamplingIntervalAndAssertCount(1000, 2);

            // Verify reference was added to cache and old references
            var cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameThree);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameFour);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            VerifyPropertiesForCurveDataCache(caches, 1000, 150);
        }
        #endregion

        private IEnumerable<ICurveDataCache> GetCachesBySamplingIntervalAndAssertCount(int samplingInterval, int expectedCount)
        {
            IEnumerable<ICurveDataCache> caches = m_CurveDataCache.GetCurveDataCaches(samplingInterval);
            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count(), Is.EqualTo(expectedCount));

            return caches;
        }

        private IEnumerable<ICurveDataCache> GetCurveDataCaches(IEnumerable<ICurveDataCache> caches, int trendViewerReferenceCount, string trendViewerName)
        {
            return caches.Where(x => x.TrendViewerReferences.Count == trendViewerReferenceCount &&
                            x.TrendViewerReferences.Contains(trendViewerName));
        }

        private void VerifyPropertiesForCurveDataCache(IEnumerable<ICurveDataCache> curveDataCaches, int expectedSamplingInterval,
            int expectedBufferSize)
        {
            foreach (ICurveDataCache curveDataCache in curveDataCaches)
            {
                Assert.That(curveDataCache.SamplingInterval, Is.EqualTo(expectedSamplingInterval));
                Assert.That(curveDataCache.Capacity, Is.EqualTo(expectedBufferSize));
            }
        }
    }

    internal class ExtendedCacheManager : CurveDataCacheManager
    {
        public IDictionary<int, IList<ICurveDataCache>> CachesBySamplingInterval
        {
            get { return m_CurveDataCachesBySamplingInterval; }
        }
    }
}
