using System.Collections.Generic;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using NSubstitute;
using NUnit.Framework;

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
            m_TagServiceMock = Substitute.For<ICachedDataItemService>();
            m_TagsHandler = new TagsHandler(m_TagServiceMock);
        }

        [Test]
        public void Should_get_tag_value_from_tag_service()
        {
            MockTagValue("tag1", 123);

            m_TagsHandler.Get("tag1");

            m_TagServiceMock.Received().GetTag(Arg.Is("tag1"), Arg.Any<ILifetimeContext>());
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

            m_TagServiceMock.Received().SetTagValue(Arg.Is("foo"), Arg.Is(new VariantValue("value")), Arg.Any<ILifetimeContext>());
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
            m_TagServiceMock.GetAllTagNames().Returns(new[] { "foo", "bar" });

            var names = m_TagsHandler.GetAllTagNames();

            Assert.That(names, Is.EquivalentTo(new[] { "foo", "bar" }));
        }

        private void MockTagValue(string tagName, object tagValue)
        {
            var dataItemProxy = Substitute.For<IDataItemProxy>();
            var globalDataItem = Substitute.For<IMultiMockDataItemInterface>();

            dataItemProxy.DataItem.Returns(globalDataItem);
            dataItemProxy.Value.Returns(new VariantValue(tagValue));

            globalDataItem.GetConnectedDataItems(Arg.Any<AccessRights>()).Returns(new List<IDataItem>());
            globalDataItem.DataTypeFriendlyName.Returns("int16");
            ((IDataItemProxySource)globalDataItem).Name.Returns(tagName);
            ((IGlobalDataItem)globalDataItem).Name.Returns(tagName);

            m_TagServiceMock.Exists(tagName).Returns(true);
            m_TagServiceMock.GetTag(Arg.Is(tagName), Arg.Any<ILifetimeContext>()).Returns(dataItemProxy);
        }
    }
}
