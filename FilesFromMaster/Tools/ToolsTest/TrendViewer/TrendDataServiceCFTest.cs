using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Core.Api.Feature;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.TrendViewer
{
    [TestFixture]
    public class TrendDataServiceCFTest
    {
        private IGlobalReferenceService m_GlobalReferenceService;
        private TestableTrendDataServiceCF m_TrendDataServiceCF;
        private XDocument m_Document;
        private IFeatureSecurityService m_FeatureSecurityService;

        [SetUp]
        public void SetUp()
        {
            AddServices();
            m_TrendDataServiceCF = new TestableTrendDataServiceCF();
        }

        private void AddServices()
        {
            m_GlobalReferenceService = MockRepository.GenerateStub<IGlobalReferenceService>();
            m_GlobalReferenceService.Stub(x => x.GetObjects<IDataLogger>()).Return(new IDataLogger[0]);

            m_FeatureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();

            var dateTimeEditService = MockRepository.GenerateStub<IDateTimeEditService>();
            dateTimeEditService.Stub(x => x.LocalTime).Return(DateTime.Now);

            TestHelper.ClearServices();
            TestHelper.AddService(m_GlobalReferenceService);
            TestHelper.AddService(m_FeatureSecurityService);
            TestHelper.AddService(dateTimeEditService);
        }

        private void InitTrendDataServiceWithDataFromDocument()
        {
            m_Document = new XDocument();
            InitDocument(ref m_Document, "Neo.ApplicationFramework.Tools.TrendViewer.TestFiles.TrendDataConfigTest.xml");
            m_TrendDataServiceCF.SetupDictionariesFromDocumentContentOverride(m_Document);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        [TestCase("Screen1.TrendViewer1", "Tags.Tag1", 600, 1200)]
        [TestCase("Screen1.TrendViewer1", "Tags.Tag2", 600, 1200)]
        [TestCase("Screen2.TrendViewer2", "Tags.Tag1", 600, 200)]
        [TestCase("Screen2.TrendViewer2", "Tags.Tag2", 600, 200)]
        public void VerifyGetCacheByTrendAndDataSourceName(string trendViewerName, string dataSourceName, int expectedSize, int expectedSamplingInterval)
        {
            InitTrendDataServiceWithDataFromDocument();

            ICurveDataCache curveDataCache = m_TrendDataServiceCF.CurveDataCacheManager.GetCurveDataCache(trendViewerName, dataSourceName);
            Assert.That(curveDataCache, Is.Not.Null);

            Assert.That(curveDataCache.Capacity, Is.EqualTo(expectedSize));
            Assert.That(curveDataCache.SamplingInterval, Is.EqualTo(expectedSamplingInterval));
        }

        [Test]
        [TestCase(200)]
        [TestCase(1200)]
        public void VerifySamplingIntervalsAfterDocumentDeserialization(int samplingInterval)
        {
            InitTrendDataServiceWithDataFromDocument();

            Assert.That(m_TrendDataServiceCF.CurveDataCacheManager.SamplingIntervals.Contains(samplingInterval), Is.True);
        }

        [Test]
        [TestCase(200)]
        [TestCase(1200)]
        public void VerifyTimersAfterDocumentDeserializationAndCachingStarted(int samplingInterval)
        {
            InitTrendDataServiceWithDataFromDocument();
            m_TrendDataServiceCF.CreateAndStartDataCacheTimersOverride();

            Assert.That(m_TrendDataServiceCF.TimerBySamplingInterval.Keys.Count, Is.EqualTo(2));
            Assert.That(m_TrendDataServiceCF.TimerBySamplingInterval.ContainsKey(samplingInterval), Is.True);

            foreach (Timer timer in m_TrendDataServiceCF.TimerBySamplingInterval.Values)
            {
                timer.Dispose();
            }
        }

        private void InitDocument(ref XDocument document, string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (XmlReader xmlReader = XmlReader.Create(stream))
            {
                document = XDocument.Load(xmlReader);
            }
        }
    }

    internal class TestableTrendDataServiceCF : TrendDataServiceCF
    {
        public void SetupDictionariesFromDocumentContentOverride(XDocument document)
        {
            SetupCachesFromDocumentContent(document);
        }

        public new ICurveDataCacheManager CurveDataCacheManager
        {
            get { return base.CurveDataCacheManager; }
        }

        public new IDictionary<int, Timer> TimerBySamplingInterval
        {
            get { return base.TimerBySamplingInterval; }
        }

        public void CreateAndStartDataCacheTimersOverride()
        {
            CreateAndStartDataCacheTimers();
        }
    }
}