using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.CachedDataItem;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.Website.Tags
{
    [TestFixture]
    public class CachedDataItemServiceTest
    {
        private IOpcClientServiceCF m_OpcClientServiceMock;
        private IBatchService m_BatchServiceMock;
        private IActivatedDataItemCache m_ActivateDataItemCacheMock;
        private ILifetimeContext m_LifetimeContextMock;
        private ICachedDataItemService m_CachedDataItemService;

        [SetUp]
        public void Setup()
        {
            m_OpcClientServiceMock = Substitute.For<IOpcClientServiceCF>();
            m_BatchServiceMock = Substitute.For<IBatchService>();
            m_ActivateDataItemCacheMock = Substitute.For<IActivatedDataItemCache>();
            m_LifetimeContextMock = Substitute.For<ILifetimeContext>();
            m_CachedDataItemService = 
                new CachedDataItemService(m_OpcClientServiceMock.ToILazy(), m_BatchServiceMock.ToILazy(), m_ActivateDataItemCacheMock);
        }

        [Test]
        public void TagIsAddedToCacheIfTagExistsButNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.Received()
                .AddDataItems(Arg.Is<IEnumerable<IGlobalDataItem>>(x => x.ContainsSameElements(new IGlobalDataItem[] { dataItemMock })));
        }

        [Test]
        public void TagIsNotAddedToCacheIfTagNotExists()
        {
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.DidNotReceiveWithAnyArgs().AddDataItems(Arg.Any<IEnumerable<IGlobalDataItem>>());
        }

        [Test]
        public void TagIsNotAddedToCacheWhenWhenTagIsInvalid()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            dataItemMock.IsArrayTag.Returns(true);
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.DidNotReceiveWithAnyArgs().AddDataItems(Arg.Any<IEnumerable<IGlobalDataItem>>());
        }

        [Test]
        public void GetTagValueWillForceBatchReadOnGlobalDataItemWhenTagNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            dataItemMock.Received().BatchRead();
        }

        [Test]
        public void GetTagValuesWillAddNonExistingTagsToCache()
        {
            IGlobalDataItem fooMock = MockGlobalDataItem("fooTag", "foo");
            IGlobalDataItem barMock = MockGlobalDataItem("barTag", "bar");
            m_ActivateDataItemCacheMock.GetDataItem("fooTag").Returns(new DataItemProxy());
            List<string> tagNames = new List<string>() { "fooTag", "barTag" };
            m_CachedDataItemService.GetTagValues(tagNames, m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.DidNotReceiveWithAnyArgs()
                .AddDataItems(Arg.Is<IEnumerable<IGlobalDataItem>>(x => x.ContainsSameElements(new IGlobalDataItem[] { fooMock })));
            m_ActivateDataItemCacheMock.Received()
                .AddDataItems(Arg.Is<IEnumerable<IGlobalDataItem>>(x => x.ContainsSameElements(new IGlobalDataItem[] { barMock })));
        }

        [Test]
        public void SetTagValueWillAddTagToCacheIfTagExistsButNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.SetTagValue("fooTag", "foo", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.Received()
                .AddDataItems(Arg.Is<IEnumerable<IGlobalDataItem>>(x => x.ContainsSameElements(new IGlobalDataItem[] { dataItemMock })));
        }

        [Test]
        public void LifeTimeContextIsNotifiedWhenItemAccessed()
        {
            string tagName = "fooTag";
            string[] tagNames = new string[] { tagName };
            string tagValue = "foo";

            IGlobalDataItem fooMock = MockGlobalDataItem(tagName, tagValue);
            m_ActivateDataItemCacheMock.GetDataItem(tagName).Returns(new DataItemProxy());

            m_CachedDataItemService.GetTag(tagName, m_LifetimeContextMock);
            m_CachedDataItemService.GetTags(tagNames, m_LifetimeContextMock);

            m_CachedDataItemService.GetTagValue(tagName, m_LifetimeContextMock);
            m_CachedDataItemService.GetTagValues(new string[] { tagName }, m_LifetimeContextMock);

            m_CachedDataItemService.SetTagValue(tagName, tagValue, m_LifetimeContextMock);
            var tagNamesAndValues = new Dictionary<string, VariantValue>();
            tagNamesAndValues.Add(tagName, tagValue);
            m_CachedDataItemService.SetTagValues(tagNamesAndValues, m_LifetimeContextMock);

            m_LifetimeContextMock.Received(6)
                .ItemsAccessed(Arg.Is<IEnumerable<string>>(x => x.ContainsSameElements(tagNames)));
        }

        private IGlobalDataItem MockGlobalDataItem(string tagName, VariantValue tagValue)
        {
            IGlobalDataItem globalDataItemMock = Substitute.For<IGlobalDataItem>();
            globalDataItemMock.Name.Returns(tagName);
            globalDataItemMock.Value.Returns(tagValue);

            m_OpcClientServiceMock.FindTag(tagName).Returns(globalDataItemMock);

            return globalDataItemMock;
        }
    }
}