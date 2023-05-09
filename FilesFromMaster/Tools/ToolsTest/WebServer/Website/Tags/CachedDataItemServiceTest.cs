using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.CachedDataItem;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_OpcClientServiceMock = MockRepository.GenerateMock<IOpcClientServiceCF>();
            m_BatchServiceMock = MockRepository.GenerateMock<IBatchService>();
            m_ActivateDataItemCacheMock = MockRepository.GenerateMock<IActivatedDataItemCache>();
            m_LifetimeContextMock = MockRepository.GenerateMock<ILifetimeContext>();
            m_CachedDataItemService = new CachedDataItemService(m_OpcClientServiceMock.ToILazy(), m_BatchServiceMock.ToILazy(), m_ActivateDataItemCacheMock);
        }

        [Test]
        public void TagIsAddedToCacheIfTagExistsButNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.AssertWasCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.List.ContainsAll(new IGlobalDataItem[] { dataItemMock })));
        }

        [Test]
        public void TagIsNotAddedToCacheIfTagNotExists()
        {
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.AssertWasNotCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.Is.Anything));
        }

        [Test]
        public void TagIsNotAddedToCacheWhenWhenTagIsInvalid()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            dataItemMock.Stub(m => m.IsArrayTag).Return(true);
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.AssertWasNotCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.Is.Anything));
        }

        [Test]
        public void GetTagValueWillForceBatchReadOnGlobalDataItemWhenTagNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.GetTagValue("fooTag", m_LifetimeContextMock);
            dataItemMock.AssertWasCalled(m => m.BatchRead());
        }

        [Test]
        public void GetTagValuesWillAddNonExistingTagsToCache()
        {
            IGlobalDataItem fooMock = MockGlobalDataItem("fooTag", "foo");
            IGlobalDataItem barMock = MockGlobalDataItem("barTag", "bar");
            m_ActivateDataItemCacheMock.Stub(m => m.GetDataItem("fooTag")).Return(new DataItemProxy());
            List<string> tagNames = new List<string>() { "fooTag", "barTag" };
            m_CachedDataItemService.GetTagValues(tagNames, m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.AssertWasNotCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.List.ContainsAll(new IGlobalDataItem[] { fooMock })));
            m_ActivateDataItemCacheMock.AssertWasCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.List.ContainsAll(new IGlobalDataItem[] { barMock })));
        }

        [Test]
        public void SetTagValueWillAddTagToCacheIfTagExistsButNotInCache()
        {
            IGlobalDataItem dataItemMock = MockGlobalDataItem("fooTag", "foo");
            m_CachedDataItemService.SetTagValue("fooTag", "foo", m_LifetimeContextMock);
            m_ActivateDataItemCacheMock.AssertWasCalled(m => m.AddDataItems(Arg<IEnumerable<IGlobalDataItem>>.List.ContainsAll(new IGlobalDataItem[] { dataItemMock })));
        }

        [Test]
        public void LifeTimeContextIsNotifiedWhenItemAccessed()
        {
            string tagName = "fooTag";
            string[] tagNames = new string[] { tagName };
            string tagValue = "foo";

            IGlobalDataItem fooMock = MockGlobalDataItem(tagName, tagValue);
            m_ActivateDataItemCacheMock.Stub(m => m.GetDataItem(tagName)).Return(new DataItemProxy());

            m_CachedDataItemService.GetTag(tagName, m_LifetimeContextMock);
            m_CachedDataItemService.GetTags(tagNames, m_LifetimeContextMock);

            m_CachedDataItemService.GetTagValue(tagName, m_LifetimeContextMock);
            m_CachedDataItemService.GetTagValues(new string[] { tagName }, m_LifetimeContextMock);

            m_CachedDataItemService.SetTagValue(tagName, tagValue, m_LifetimeContextMock);
            var tagNamesAndValues = new Dictionary<string, VariantValue>();
            tagNamesAndValues.Add(tagName, tagValue);
            m_CachedDataItemService.SetTagValues(tagNamesAndValues, m_LifetimeContextMock);

            m_LifetimeContextMock.AssertWasCalled(m => m.ItemsAccessed(Arg<IEnumerable<string>>.List.ContainsAll(tagNames)), options => options.Repeat.Times(6));
        }

        private IGlobalDataItem MockGlobalDataItem(string tagName, VariantValue tagValue)
        {
            IGlobalDataItem globalDataItemMock = MockRepository.GenerateMock<IGlobalDataItem>();
            globalDataItemMock.Stub(m => m.Name).Return(tagName);
            globalDataItemMock.Stub(m => m.Value).Return(tagValue);

            m_OpcClientServiceMock.Stub(m => m.FindTag(tagName)).Return(globalDataItemMock);

            return globalDataItemMock;
        }
    }
}