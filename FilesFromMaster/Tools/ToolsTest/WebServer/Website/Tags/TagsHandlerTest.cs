using System.Collections.Generic;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.Website.Tags
{
    [TestFixture]
    public class TagsHandlerTest
    {
        private TagsHandler m_TagsHandler;
        private ICachedDataItemService m_TagServiceMock;

        public interface IMultiMockDataItemInterface : IDataItemProxySource, IGlobalDataItem { }

        [SetUp]
        public void Setup()
        {
            m_TagServiceMock = MockRepository.GenerateMock<ICachedDataItemService>();
            m_TagsHandler = new TagsHandler(m_TagServiceMock);
        }

        [Test]
        public void Should_get_tag_value_from_tag_service()
        {
            MockTagValue("tag1", 123);

            m_TagsHandler.Get("tag1");

            m_TagServiceMock.AssertWasCalled(m => m.GetTag(Arg.Is("tag1"), Arg<ILifetimeContext>.Is.Anything));
        }

        [Test]
        public void Should_return_tag_result_when_tag_is_valid()
        {
            MockTagValue("tag1", 123);
            
            TagDto result = m_TagsHandler.Get("tag1") as TagDto;

            Assert.That(result.name, Is.EqualTo("tag1"));
            Assert.That(result.value, Is.EqualTo(123));
        }

        [Test]
        public void Should_set_status_code_to_404_when_tag_do_not_exist()
        {
            OperationResult operationResult = m_TagsHandler.Get("tag1") as OperationResult;

            Assert.That(operationResult.HttpStatusCode, Is.EqualTo(404));
        }
        [Test]
        public void Should_set_a_tag_value_using_tag_service()
        {
            m_TagsHandler.Put(new TagValueDto { name = "foo", value = "value" });

            m_TagServiceMock.AssertWasCalled(m => m.SetTagValue(Arg.Is("foo"), Arg.Is(new VariantValue("value")), Arg<ILifetimeContext>.Is.Anything));
        }
        
        [Test]
        public void Should_return_bad_request_when_trying_to_set_tag_without_name()
        {
            OperationResult result = m_TagsHandler.Put(new TagValueDto { name = null, value = "value" }) as OperationResult;

            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        }
        
        [Test]
        public void Should_get_all_tag_names_from_tag_service()
        {
            m_TagServiceMock.Stub(m => m.GetAllTagNames()).Return(new[] { "foo", "bar" });

            var names = m_TagsHandler.GetAllTagNames();

            Assert.That(names, Is.EquivalentTo(new[] { "foo", "bar" }));    
        }

        private void MockTagValue(string tagName, object tagValue)
        {
            var dataItemProxy = MockRepository.GenerateMock<IDataItemProxy>();
            var globalDataItem = MockRepository.GenerateMock<IMultiMockDataItemInterface>();

            dataItemProxy.Stub(m => m.DataItem).Return(globalDataItem);
            dataItemProxy.Stub(m => m.Value).Return(new VariantValue(tagValue));

            globalDataItem.Stub(m => m.GetConnectedDataItems(Arg<AccessRights>.Is.Anything)).Return(new List<IDataItem>());
            globalDataItem.Stub(m => m.DataTypeFriendlyName).Return("int16");
            globalDataItem.Stub(m => ((IDataItemProxySource)m).Name).Return(tagName);
            globalDataItem.Stub(m => ((IGlobalDataItem)m).Name).Return(tagName);

            m_TagServiceMock.Stub(m => m.Exists(tagName)).Return(true);            
            m_TagServiceMock.Stub(m => m.GetTag(Arg.Is(tagName), Arg<ILifetimeContext>.Is.Anything)).Return(dataItemProxy);
        }
    }
}
