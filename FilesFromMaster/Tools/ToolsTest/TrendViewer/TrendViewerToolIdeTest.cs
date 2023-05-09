using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.CrossReference;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.TrendViewer
{
    [TestFixture]
    public class TrendViewerToolIdeTest
    {
        private List<ITrendViewerCrossReferenceItem> m_TrendViewerCrossReferences;
        private TrendViewerToolIde m_TrendViewerToolIde;

        [SetUp]
        public void SetUp()
        {
            m_TrendViewerToolIde = new TrendViewerToolIde();
        }

        [Test]
        public void XDocumentStructureTest()
        {
            XDocument trendXDocument = SetupTestWithThreeTrendsInCrossReferenceAndCreateXDocument();
            IList<XElement> trendViewerElements = GetMainElementsInXDocument(trendXDocument);

            Assert.That(trendViewerElements.Count, Is.EqualTo(3));
            Assert.That(trendViewerElements[0].Name.LocalName, Is.EqualTo("TrendViewer"));
        }

        [Test]
        [TestCase("Screen2.TrendA", 3)]
        [TestCase("Screen1.TrendB", 2)]
        [TestCase("Screen1.TrendC", 2)]
        public void XDocumentContentTest(string trendViewerName, int dataSourceCount)
        {
            XDocument trendXDocument = SetupTestWithThreeTrendsInCrossReferenceAndCreateXDocument();
            IList<XElement> trendViewerElements = GetMainElementsInXDocument(trendXDocument);

            var trendViewerElement = trendViewerElements.Where(x => x.Attribute("Name").Value == trendViewerName).FirstOrDefault();
            Assert.That(trendViewerElement, Is.Not.Null);

            var globalDataItemElement = trendViewerElement.Elements();
            Assert.That(globalDataItemElement.Count(), Is.EqualTo(1));

            var globalDataItemElements = globalDataItemElement.Elements();
            Assert.That(globalDataItemElements.Count(), Is.EqualTo(dataSourceCount));
        }

        private XDocument SetupTestWithThreeTrendsInCrossReferenceAndCreateXDocument()
        {
            m_TrendViewerCrossReferences = GetTrendViewerCrossReferencesForThreeTrends();
            return m_TrendViewerToolIde.CreateXDocumentForTrendViewers(m_TrendViewerCrossReferences);
        }

        private IList<XElement> GetMainElementsInXDocument(XDocument trendXDocument)
        {
            return trendXDocument.Root.Elements().ToList();
        }

        private List<ITrendViewerCrossReferenceItem> GetTrendViewerCrossReferencesForThreeTrends()
        {
            return new List<ITrendViewerCrossReferenceItem>() { TrendA, TrendB, TrendC };
        }

        private ITrendViewerCrossReferenceItem TrendA
        {
            get
            {
                var dataSourceNames = new List<string>(){StringConstants.TagsRoot + "Tag3", StringConstants.TagsRoot +"Tag4", StringConstants.TagsRoot +"Tag2"};

                return new TrendViewerCrossReferenceItem("Screen2.TrendA", 600, new TimeSpan(1, 0, 0), string.Empty, dataSourceNames);
            }
        }

        private ITrendViewerCrossReferenceItem TrendB
        {
            get
            {
                var dataSourceNames = new List<string>() { StringConstants.TagsRoot + "Tag2", StringConstants.TagsRoot + "Tag1" };

                return new TrendViewerCrossReferenceItem("Screen1.TrendB", 400, new TimeSpan(0, 30, 0), string.Empty, dataSourceNames);
            }
        }

        private ITrendViewerCrossReferenceItem TrendC
        {
            get
            {
                var dataSourceNames = new List<string>(){StringConstants.TagsRoot + "Tag3", StringConstants.TagsRoot + "Tag1"};

                return new TrendViewerCrossReferenceItem("Screen1.TrendC", 1200, new TimeSpan(0, 2, 0), string.Empty, dataSourceNames);
            }
        }
    }
}
