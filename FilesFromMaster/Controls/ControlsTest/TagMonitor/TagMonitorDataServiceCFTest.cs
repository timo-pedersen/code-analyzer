using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces.TagMonitor;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.TagMonitor
{
    [TestFixture]
    public class TagMonitorDataServiceCFTest
    {
        private ITagMonitorDataServiceCF m_TagMonitorDataService;

        const string TagMonitor1 = "TagMonitor1";
        const string TagMonitor2 = "TagMonitor2";

        const string Tag1 = "tag1";
        const string Tag2 = "tag2";

        [SetUp]
        public void SetUp()
        {
            var fileHelper = MockRepository.GenerateMock<ITagMonitorDataFileHelper>();
            fileHelper.Stub(x => x.ReadTagMonitorTagsFromFile()).Return(new Dictionary<string, IList<string>>());
            m_TagMonitorDataService = new TagMonitorDataServiceCF(fileHelper);
            m_TagMonitorDataService.AddTag(TagMonitor1, Tag1);
            m_TagMonitorDataService.AddTag(TagMonitor1, Tag2);
            m_TagMonitorDataService.AddTag(TagMonitor2, Tag1);
        }

        [TearDown]
        public void TearDown()
        {
            m_TagMonitorDataService.RemoveTags(TagMonitor1);
            m_TagMonitorDataService.RemoveTags(TagMonitor2);
        }

        [Test]
        public void TestAddingToCache()
        {
            Assert.AreEqual(2, m_TagMonitorDataService.GetTags(TagMonitor1).Count);
            Assert.AreEqual(1, m_TagMonitorDataService.GetTags(TagMonitor2).Count);

            Assert.AreEqual(Tag1, m_TagMonitorDataService.GetTags(TagMonitor2)[0]);
        }

        [Test]
        public void TestRemovingFromCache()
        {
            m_TagMonitorDataService.RemoveTag(TagMonitor1, Tag1);
            Assert.AreEqual(Tag2, m_TagMonitorDataService.GetTags(TagMonitor1)[0]);

            m_TagMonitorDataService.RemoveTags(TagMonitor2);
            Assert.IsFalse(m_TagMonitorDataService.GetTags(TagMonitor2).Any());
        }
    }
}
