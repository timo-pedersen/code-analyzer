using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.Website.Tags
{

    [TestFixture]
    public class BatchTagsHandlerTest
    {
        private BatchTagsHandler m_BatchTagsHandler;

        private ICachedDataItemService m_CachedDataItemServiceMock;

        public interface IMultiMockDataItemInterface : IDataItemProxySource, IGlobalDataItem {}

        [SetUp]
        public void Setup()
        {
            m_CachedDataItemServiceMock = Substitute.For<ICachedDataItemService>();

            IDataItemProxy tag1Stub = Substitute.For<IDataItemProxy>();
            IDataItemProxy tag2Stub = Substitute.For<IDataItemProxy>();

            var tag1GlobalDataItem = Substitute.For<IMultiMockDataItemInterface>();
            var tag2GlobalDataItem = Substitute.For<IMultiMockDataItemInterface>();

            tag1GlobalDataItem.GetConnectedDataItems(Arg.Any<AccessRights>()).Returns(new IDataItem[0]);
            tag2GlobalDataItem.GetConnectedDataItems(Arg.Any<AccessRights>()).Returns(new IDataItem[0]);

            tag1GlobalDataItem.DataTypeFriendlyName.Returns("int16");
            tag2GlobalDataItem.DataTypeFriendlyName.Returns("INT16");

            tag1Stub.DataItem.Returns(tag1GlobalDataItem);
            tag2Stub.DataItem.Returns(tag2GlobalDataItem);

            tag1Stub.Value.Returns(new VariantValue(1111));
            tag2Stub.Value.Returns(new VariantValue(2222));

            m_CachedDataItemServiceMock.GetTags(Arg.Is<IEnumerable<string>>(x => x.First() == "tag1"), Arg.Any<ILifetimeContext>())
                .Returns(new Dictionary<string, IDataItemProxy> { { "tag1", tag1Stub } });

            m_CachedDataItemServiceMock.GetTags(Arg.Is<IEnumerable<string>>(x => x.First() == "tag2"), Arg.Any<ILifetimeContext>())
                .Returns(new Dictionary<string, IDataItemProxy> { { "tag2", tag2Stub } });
            
            m_CachedDataItemServiceMock.GetTags(Arg.Is<IEnumerable<string>>(x => x.First() == "tag1" && x.Last() == "tag2"), Arg.Any<ILifetimeContext>())
                .Returns(new Dictionary<string, IDataItemProxy> { { "tag1", tag1Stub }, { "tag2", tag2Stub } });

            Session fakeSession = new Session(Guid.NewGuid().ToString(), DateTime.UtcNow + TimeSpan.FromMinutes(1));
            m_BatchTagsHandler = new BatchTagsHandler(fakeSession, m_CachedDataItemServiceMock);
        }

        [Test]
        public void Should_get_tags_from_tag_service()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.getTags = new List<string> { "tag1", "tag2" };
            
            m_BatchTagsHandler.Post(batchTagOperationDto);

            m_CachedDataItemServiceMock.Received().GetTags(Arg.Any<IEnumerable<string>>(), Arg.Any<ILifetimeContext>());
        }       
        
        [Test]
        public void Should_include_metadata_when_getting_tags()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.includeTagMetadata = true;
            batchTagOperationDto.getTags = new List<string> { "tag1" };
            
            BatchTagOperationResultDto result = m_BatchTagsHandler.Post(batchTagOperationDto) as BatchTagOperationResultDto;

            Assert.That(result.tags.First(), Is.InstanceOf<TagDto>());
        }     
        
        [Test]
        public void Should_return_tag_values_when_getting_tags()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.getTags = new List<string>();
            batchTagOperationDto.getTags.Add("tag1");

            BatchTagOperationResultDto result = m_BatchTagsHandler.Post(batchTagOperationDto) as BatchTagOperationResultDto;

            Assert.That(result.tags.First(), Is.InstanceOf<TagValueDto>());
        }

        [Test]
        public void Should_set_tag_values_using_tag_service()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.setTags = new List<TagValueDto>();
            batchTagOperationDto.setTags.Add(new TagValueDto() { name = "tag1", value = "foo" });
            batchTagOperationDto.setTags.Add(new TagValueDto() { name = "tag2", value = "bar" });

            m_BatchTagsHandler.Post(batchTagOperationDto);

            m_CachedDataItemServiceMock.Received().SetTagValues(Arg.Any<IDictionary<string, VariantValue>>(), Arg.Any<ILifetimeContext>());
        }       
        
        [Test]
        public void Should_respond_bad_request_when_setting_a_tag_without_name()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.setTags = new List<TagValueDto>();
            batchTagOperationDto.setTags.Add(new TagValueDto { name = "", value = "foo" });

            OperationResult result = m_BatchTagsHandler.Post(batchTagOperationDto) as OperationResult;

            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        }      
        
        [Test]
        public void Should_respond_bad_request_when_getting_a_tag_without_name()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.getTags = new List<string>();
            batchTagOperationDto.getTags.Add(null);

            OperationResult result = m_BatchTagsHandler.Post(batchTagOperationDto) as OperationResult;

            Assert.That(result.HttpStatusCode, Is.EqualTo(400));
        }
    }
}
