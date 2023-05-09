using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Controls.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.DataLogger;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.TrendViewer
{
    [TestFixture]
    public class CurveDataCacheManagerTest
    {
        private ICurveDataCacheManager m_CurveDataCache;
        private IGlobalReferenceService m_GlobalReferenceService;
        private TrendViewerHelper m_TrendViewerHelper;

        private IValue m_DataSourceOne;
        private IValue m_DataSourceTwo;
        private IValue m_DataSourceThree;

        private ILogItem m_LogItemOne;
        private ILogItem m_LogItemTwo;
        private ILogItem m_LogItemThree;

        private const string TrendViewerNameOne = "Screen1.TrendViewer1";
        private const string TrendViewerNameTwo = "Screen1.TrendViewer2";
        private const string TrendViewerNameThree = "Screen2.TrendViewer1";
        private const string TrendViewerNameFour = "Screen2.TrendViewer2";
        private const string TrendViewerNameFive = "Screen3.TrendViewer1";

        private const string DataSourceNameOne = "Tags.Tag1";
        private static readonly string DataSourceNameTwo = StringConstants.TagsRoot + "Tag2";
        private static readonly string DataSourceNameThree = StringConstants.TagsRoot + "Tag3";
        
        private static readonly DateTime StopTime = new DateTime(2021,05,04,10,1,0);
        private static readonly DateTime StartTime = StopTime - new TimeSpan(0,1,0);
        private static readonly DateTime StartTimeLiveData = StopTime - new TimeSpan(0, 0, 30);

        [SetUp]
        public void SetUp()
        {
            m_DataSourceOne = MockRepository.GenerateStub<IValue>();
            m_DataSourceTwo = MockRepository.GenerateStub<IValue>();
            m_DataSourceThree = MockRepository.GenerateStub<IValue>();

            m_LogItemOne = MockRepository.GenerateStub<ILogItem>();
            m_LogItemTwo = MockRepository.GenerateStub<ILogItem>();
            m_LogItemThree = MockRepository.GenerateStub<ILogItem>();

            AddServices();

            m_CurveDataCache = new ExtendedCacheManager();
            m_TrendViewerHelper = new TrendViewerHelper();

            InitCaches();
        }

        private ExtendedCacheManager ExtendedCacheManager
        {
            get { return m_CurveDataCache as ExtendedCacheManager; }
        }

        private void AddServices()
        {
            m_GlobalReferenceService = MockRepository.GenerateStub<IGlobalReferenceService>();

            m_GlobalReferenceService.Stub(x => x.GetObject<IValue>(DataSourceNameOne)).Return(m_DataSourceOne);
            m_GlobalReferenceService.Stub(x => x.GetObject<IValue>(DataSourceNameTwo)).Return(m_DataSourceTwo);
            m_GlobalReferenceService.Stub(x => x.GetObject<IValue>(DataSourceNameThree)).Return(m_DataSourceThree);
            
            m_LogItemOne.Stub(x => x.FullName).Return(DataSourceNameOne);
            m_LogItemTwo.Stub(x => x.FullName).Return(DataSourceNameTwo);
            m_LogItemThree.Stub(x => x.FullName).Return(DataSourceNameThree);

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
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(samplingInterval, expectedCacheCount);
            VerifyPropertiesForCurveDataCache(caches, samplingInterval, bufferSize);
        }

        #region Add Caches for Trend Viewer

        [Test]
        public void VerifyAddCachesNewSamplingIntervalNewBufferSize()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 200);

            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(1500, 1);

            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
        }

        [Test]
        public void VerifyAddCachesMatchingSamplingIntervalNewBufferSize()
        {
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 200);
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameTwo, new string[] { DataSourceNameOne }, 1500, 300);

            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(1500, 2);

            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameOne);
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

            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(1500, 1);
            VerifyPropertiesForCurveDataCache(caches, 1500, 200);

            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameOne);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));

            cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameTwo);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(1));
        }

        #endregion

        #region Get Curve Data Cache

        [Test]
        public void VerifyGetCurveDataCachesBySamplingInterval()
        {
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(3));

            IEnumerable<ICurveDataCache> curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1500);
            Assert.That(curveDataCaches, Is.Not.Null);
            Assert.That(curveDataCaches.Count(), Is.EqualTo(3));

            curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1000);
            Assert.That(curveDataCaches, Is.Not.Null);
            Assert.That(curveDataCaches.Count(), Is.EqualTo(2));
        }

        [Test]
        public void VerifyTrendViewerReferencesOnCurveDataCaches()
        {
            IEnumerable<ICurveDataCache> curveDataCaches = m_CurveDataCache.GetCurveDataCaches(1500);

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

        #endregion

        #region Update Cache Parameters

        [Test]
        public void VerifyUpdateCacheParametersListRemovedForUnusedSamplingInterval()
        {
            // Act
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameThree, 4000, 200);

            // Assert
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
            // Act
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, oldInterval, oldSize);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, newInterval, newSize);

            // Assert
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
            // Arrange
            const int newInterval = 1000;

            // Act
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameOne, new string[] { DataSourceNameOne }, 1500, 600);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, newInterval, 15);
            
            // Assert
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.ContainsKey(newInterval), Is.True);
            IEnumerable<ICurveDataCache> curveDataCaches = m_CurveDataCache.GetCurveDataCaches(newInterval);
            VerifyPropertiesForCurveDataCache(curveDataCaches, newInterval, 600);
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1500), Is.Null);
        }

        [Test]
        public void VerifyUpdateCacheParametersSharedToNew()
        {
            // Arrange
            const int newInterval = 4000;

            // Act
            // Change sampling interval from 1500 (shared) to 4000 (new)
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameTwo, newInterval, 400);

            // Assert
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));

            IEnumerable<ICurveDataCache> cachesOldInterval = GetCachesBySamplingIntervalAndAssertCount(1500, 3);

            // Verify references removed from (previously) shared cache
            IEnumerable<ICurveDataCache> cachesOldIntervalWithCorrectReferences = GetCurveDataCaches(cachesOldInterval, 1, TrendViewerNameOne);
            Assert.That(cachesOldIntervalWithCorrectReferences.Count(), Is.EqualTo(3));

            // Verify new caches created
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 2);

            // Verify references added to new caches
            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameTwo);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            // Verify caches for all data sources added
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            VerifyPropertiesForCurveDataCache(caches, newInterval, 400);
        }

        [Test]
        public void VerifyUpdateCacheParametersSharedToExisting()
        {
            // Arrange
            const int newInterval = 2000;

            // Act & assert
            // Change sampling interval from 1500 (shared) to 2000 (existing)
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameFive, new string[] { DataSourceNameOne, DataSourceNameTwo }, newInterval, 250);
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, newInterval, 250);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(4));
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 3);

            VerifyPropertiesForCurveDataCache(caches, newInterval, 250);

            // Verify caches for all data sources exists
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameThree) && x.DataSource == m_DataSourceThree), Is.True);

            // Verify references for existing caches
            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = caches.Where(x => x.TrendViewerReferences.Contains(TrendViewerNameOne));
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
            // Arrange
            const int newInterval = 4000;

            // Act & assert
            // Change sampling interval from 1000 (not shared) to 4000 (new)
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1000), Is.Not.Null);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameThree, newInterval, 200);

            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(3));

            // Verify old cache removed
            Assert.That(m_CurveDataCache.GetCurveDataCaches(1000), Is.Null);

            // Verify new cache created
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 2);

            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 1, TrendViewerNameThree);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));
            
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);
            
            VerifyPropertiesForCurveDataCache(caches, newInterval, 200);
        }

        [Test]
        public void VerifyUpdatedCacheParametersNotSharedToExisting()
        {
            // Change sampling interval from 3000 (not shared) to 1000 (existing)
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameFour, 1000, 150);
            Assert.That(ExtendedCacheManager.CachesBySamplingInterval.Count, Is.EqualTo(2));

            // Verify old caches removed
            Assert.That(m_CurveDataCache.GetCurveDataCaches(3000), Is.Null);

            // Verify existing caches
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(1000, 2);

            // Verify reference was added to cache and old references
            IEnumerable<ICurveDataCache> cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameThree);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            cachesWithCorrectReferences = GetCurveDataCaches(caches, 2, TrendViewerNameFour);
            Assert.That(cachesWithCorrectReferences.Count(), Is.EqualTo(2));

            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);
            
            VerifyPropertiesForCurveDataCache(caches, 1000, 150);
        }

        #endregion

        #region Update Cache Values

        [Test]
        [TestCase(2000, 1000)] 
        [TestCase(2000, 50)]
        [TestCase(200, 1000)] 
        [TestCase(200, 50)]
        [TestCase(50000, 1000)]
        [TestCase(50000, 50)]
        public void VerifyUpdateCacheValuesNewInterval_TwoDataSources_BothDataLoggerData(int timeSpanValue, int trendWidth)
        {
            // Arrange
            TimeSpan timeRange = TrendDataHelper.GetTimeRangeFromDataSource(timeSpanValue);
            int newInterval = m_TrendViewerHelper.GetSamplingInterval(timeRange, trendWidth);
            int newBufferSize = m_TrendViewerHelper.GetBufferSize(timeRange, trendWidth);
            IList<ILogData> logData = GetDataLoggerDataList(newInterval, StartTime, StopTime);
            Dictionary<IValue, IList<ILogData>> logDataDictionary = GetDataLoggerDictionary(
                logData,
                new List<IValue>
                {
                    m_LogItemOne, m_LogItemTwo
                });
            var dataSourceNames = new List<string> { DataSourceNameOne, DataSourceNameTwo };
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameThree, dataSourceNames, newInterval, newBufferSize);

            // Act
            m_CurveDataCache.UpdateCacheValues(dataSourceNames, TrendViewerNameThree, logDataDictionary, newInterval);

            // Assert
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 2);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            IList<ILogData> logListOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetRange(StartTime, StopTime);
            IList<ILogData> logListTwo = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetRange(StartTime, StopTime);
            Assert.That(logListOne, Is.Not.Null);
            Assert.That(logListOne.Count, Is.EqualTo(logData.Count));
            Assert.That(logListTwo, Is.Not.Null);
            Assert.That(logListTwo.Count, Is.EqualTo(logData.Count));

            ILogData logOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetOldest();
            ILogData logTwo = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetOldest();
            Assert.That(logOne, Is.Not.Null);
            Assert.That(logOne.LogTime, Is.EqualTo(StartTime));
            Assert.That(logTwo, Is.Not.Null);
            Assert.That(logTwo.LogTime, Is.EqualTo(StartTime));
        }

        [Test]
        public void UpdatingOneTrendViewerDoesNotAffectAnotherOneWithSameDataSourceAndSamplingInterval()
        {
            // Arrange
            DateTime startTime = StopTime - new TimeSpan(0, 10, 0);
            int samplingInterval = 1000;
            var dataSourceNames = new List<string> { DataSourceNameOne };
            var dataSources = new List<IValue> { m_LogItemOne };
            IList<ILogData> logData = GetDataLoggerDataList(samplingInterval, startTime, StopTime);
            Dictionary<IValue, IList<ILogData>> logDataDictionary = GetDataLoggerDictionary(logData, dataSources);

            ExtendedCacheManager.CachesBySamplingInterval.Clear();

            //Setup Trend Viewer 2
            InitializeTrendViewer(TrendViewerNameTwo, 400);
            IList<ILogData> logListTrendViewer2Before = GetCachesForTrendViewer(TrendViewerNameTwo);

            //Setup Trend Viewer 1
            InitializeTrendViewer(TrendViewerNameOne, 300);
            IList<ILogData> logListTrendViewer1Before = GetCachesForTrendViewer(TrendViewerNameOne);

            // Act
            int newBufferSize = 100;
            IList<ILogData> newLogData = GetDataLoggerDataList(20000, startTime, StopTime);
            Dictionary<IValue, IList<ILogData>> newLogDataDictionary = GetDataLoggerDictionary(newLogData, dataSources);
            m_CurveDataCache.UpdateCacheParameters(TrendViewerNameOne, samplingInterval, newBufferSize);
            m_CurveDataCache.UpdateCacheValues(dataSourceNames, TrendViewerNameOne, newLogDataDictionary, samplingInterval);

            // Assert
            IList<ILogData> logListTrendViewer2After = GetCachesForTrendViewer(TrendViewerNameTwo);
            IList<ILogData> logListTrendViewer1After = GetCachesForTrendViewer(TrendViewerNameOne);

            CollectionAssert.AreEqual(newLogData, logListTrendViewer1After);
            CollectionAssert.AreNotEqual(logListTrendViewer1Before, logListTrendViewer1After);
            CollectionAssert.AreEqual(logListTrendViewer2Before, logListTrendViewer2After);

            IList<ILogData> GetCachesForTrendViewer(string trendViewerName)
            {
                IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervals(samplingInterval);
                return caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne
                                && x.TrendViewerReferences.Contains(trendViewerName))?.GetRange(startTime, StopTime);
            }

            void InitializeTrendViewer(string trendViewerName, int bufferSize)
            {
                m_CurveDataCache.AddCachesForTrendViewer(trendViewerName, dataSourceNames, samplingInterval, bufferSize);
                m_CurveDataCache.UpdateCacheValues(dataSourceNames, trendViewerName, logDataDictionary, samplingInterval);
            }
        }

        [Test]
        [TestCase(2000, 1000)]
        [TestCase(2000, 50)]
        [TestCase(200, 1000)]
        [TestCase(200, 50)]
        [TestCase(50000, 1000)]
        [TestCase(50000, 50)]
        public void VerifyUpdateCacheValuesNewInterval_TwoDataSources_OneLiveData(int timeSpanValue, int trendWidth)
        {
            // Arrange
            TimeSpan timeRange = TrendDataHelper.GetTimeRangeFromDataSource(timeSpanValue);
            int newInterval = m_TrendViewerHelper.GetSamplingInterval(timeRange, trendWidth);
            int newBufferSize = m_TrendViewerHelper.GetBufferSize(timeRange, trendWidth);
            IList<ILogData> logData = GetDataLoggerDataList(newInterval, StartTime, StopTime);
            Dictionary<IValue, IList<ILogData>> logDataDictionary = GetDataLoggerDictionary(
                logData,
                new List<IValue>
                {
                    m_LogItemOne
                });
            IList<ILogData> logLiveData = GetDataLoggerDataList(newInterval, StartTimeLiveData, StopTime);
            Dictionary<IValue, IList<ILogData>> logLiveDataDictionary = GetDataLoggerDictionary(
                logLiveData,
                new List<IValue>
                {
                    m_LogItemTwo
                });
            var dataSourceNames = new List<string> { DataSourceNameOne, DataSourceNameTwo };
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameThree, dataSourceNames, newInterval, newBufferSize);
            // Add live data values to cache
            m_CurveDataCache.AddValuesToCache(newInterval, new List<string> { DataSourceNameTwo }, logLiveDataDictionary);

            // Act
            m_CurveDataCache.UpdateCacheValues(dataSourceNames, TrendViewerNameThree, logDataDictionary, newInterval);

            // Assert
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 2);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            IList<ILogData> logListOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetRange(StartTime, StopTime);
            IList<ILogData> logListLiveData = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetRange(StartTime, StopTime);
            Assert.That(logListOne, Is.Not.Null);
            Assert.That(logListOne.Count, Is.EqualTo(logData.Count));
            Assert.That(logListLiveData, Is.Not.Null);
            Assert.That(logListLiveData.Count, Is.EqualTo(logLiveData.Count));

            ILogData logOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetOldest();
            ILogData logLiveDataResult = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetOldest();
            Assert.That(logOne, Is.Not.Null);
            Assert.That(logOne.LogTime, Is.EqualTo(StartTime));
            Assert.That(logLiveDataResult, Is.Not.Null);
            Assert.That(logLiveDataResult.LogTime, Is.EqualTo(StartTimeLiveData));
        }

        [Test]
        [TestCase(2000, 1000)]
        [TestCase(2000, 50)]
        [TestCase(200, 1000)]
        [TestCase(200, 50)]
        [TestCase(50000, 1000)]
        [TestCase(50000, 50)]
        public void VerifyUpdateCacheValuesNewInterval_TwoDataSources_BothLiveData(int timeSpanValue, int trendWidth)
        {
            // Arrange
            TimeSpan timeRange = TrendDataHelper.GetTimeRangeFromDataSource(timeSpanValue);
            int newInterval = m_TrendViewerHelper.GetSamplingInterval(timeRange, trendWidth);
            int newBufferSize = m_TrendViewerHelper.GetBufferSize(timeRange, trendWidth);
            IList<ILogData> logLiveData = GetDataLoggerDataList(newInterval, StartTimeLiveData, StopTime);
            Dictionary<IValue, IList<ILogData>> logLiveDataDictionary = GetDataLoggerDictionary(
                logLiveData,
                new List<IValue>
                {
                    m_LogItemOne, m_LogItemTwo
                });
            var dataSourceNames = new List<string> { DataSourceNameOne, DataSourceNameTwo };
            ExtendedCacheManager.CachesBySamplingInterval.Clear();
            m_CurveDataCache.AddCachesForTrendViewer(TrendViewerNameThree, dataSourceNames, newInterval, newBufferSize);
            // Add live data values to cache
            m_CurveDataCache.AddValuesToCache(newInterval, dataSourceNames, logLiveDataDictionary);

            // Act
            m_CurveDataCache.UpdateCacheValues(dataSourceNames, TrendViewerNameThree, new Dictionary<IValue, IList<ILogData>>(), newInterval);

            // Assert
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervalAndAssertCount(newInterval, 2);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameOne) && x.DataSource == m_DataSourceOne), Is.True);
            Assert.That(caches.Any(x => x.DataSourceName.Equals(DataSourceNameTwo) && x.DataSource == m_DataSourceTwo), Is.True);

            IList<ILogData> logListOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetRange(StartTime, StopTime);
            IList<ILogData> logListLiveData = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetRange(StartTime, StopTime);
            Assert.That(logListOne, Is.Not.Null);
            Assert.That(logListOne.Count, Is.EqualTo(logLiveData.Count));
            Assert.That(logListLiveData, Is.Not.Null);
            Assert.That(logListLiveData.Count, Is.EqualTo(logLiveData.Count));

            ILogData logLiveDataResultOne = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameOne)?.GetOldest();
            ILogData logLiveDataResultTwo = caches.FirstOrDefault(x => x.DataSourceName == DataSourceNameTwo)?.GetOldest();
            Assert.That(logLiveDataResultOne, Is.Not.Null);
            Assert.That(logLiveDataResultOne.LogTime, Is.EqualTo(StartTimeLiveData));
            Assert.That(logLiveDataResultTwo, Is.Not.Null);
            Assert.That(logLiveDataResultTwo.LogTime, Is.EqualTo(StartTimeLiveData));
        }

        #endregion

        #region Help methods

        private IEnumerable<ICurveDataCache> GetCachesBySamplingIntervalAndAssertCount(int samplingInterval, int expectedCount)
        {
            IEnumerable<ICurveDataCache> caches = GetCachesBySamplingIntervals(samplingInterval);

            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count(), Is.EqualTo(expectedCount));

            return caches;
        }

        private IEnumerable<ICurveDataCache> GetCachesBySamplingIntervals(int samplingInterval)
        {
            IEnumerable<ICurveDataCache> caches = m_CurveDataCache.GetCurveDataCaches(samplingInterval);

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

        private Dictionary<IValue, IList<ILogData>> GetDataLoggerDictionary(IList<ILogData> logDataList, List<IValue> dataSources)
        {
            var dataLoggerData = new Dictionary<IValue, IList<ILogData>>();
            foreach (IValue dataSource in dataSources)
            {
                dataLoggerData.Add(dataSource, logDataList);
            }

            return dataLoggerData;
        }

        private IList<ILogData> GetDataLoggerDataList(int interval, DateTime startTime, DateTime stopTime)
        {
            int numberOfDataPoints = (int)(stopTime - startTime).TotalMilliseconds / interval;

            var logDataList = new List<ILogData>
            {
                new LogData(1, startTime)
            };

            var logTime = startTime;
            for (int i = 2; i < numberOfDataPoints + 1; i++)
            {
                logTime = logTime.AddMilliseconds(interval);
                logDataList.Add(new LogData(i, logTime));
            }

            return logDataList;
        }

        #endregion
    }

    internal class ExtendedCacheManager : CurveDataCacheManager
    {
        public IDictionary<int, IList<ICurveDataCache>> CachesBySamplingInterval
        {
            get { return m_CurveDataCachesBySamplingInterval; }
        }
    }
}
