using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CachedDataItem;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_CachedDataItemServiceMock = MockRepository.GenerateMock<ICachedDataItemService>();

            IDataItemProxy tag1Stub = MockRepository.GenerateMock<IDataItemProxy>();
            IDataItemProxy tag2Stub = MockRepository.GenerateMock<IDataItemProxy>();

            var tag1GlobalDataItem = MockRepository.GenerateMock<IMultiMockDataItemInterface>();
            var tag2GlobalDataItem = MockRepository.GenerateMock<IMultiMockDataItemInterface>();

            tag1GlobalDataItem.Stub(m => m.GetConnectedDataItems(Arg<AccessRights>.Is.Anything)).Return(new IDataItem[0]);
            tag2GlobalDataItem.Stub(m => m.GetConnectedDataItems(Arg<AccessRights>.Is.Anything)).Return(new IDataItem[0]);

            tag1GlobalDataItem.Stub(m => m.DataTypeFriendlyName).Return("int16");
            tag2GlobalDataItem.Stub(m => m.DataTypeFriendlyName).Return("INT16");

            tag1Stub.Stub(m => m.DataItem).Return(tag1GlobalDataItem);
            tag2Stub.Stub(m => m.DataItem).Return(tag2GlobalDataItem);

            tag1Stub.Stub(m => m.Value).Return(new VariantValue(1111));
            tag2Stub.Stub(m => m.Value).Return(new VariantValue(2222));

            m_CachedDataItemServiceMock
                .Stub(m => m.GetTags(Arg.Is(new[] {"tag1"}), Arg<ILifetimeContext>.Is.Anything))
                .Return(new Dictionary<string, IDataItemProxy> { { "tag1", tag1Stub } });

            m_CachedDataItemServiceMock
                .Stub(m => m.GetTags(Arg.Is(new[] { "tag2" }), Arg<ILifetimeContext>.Is.Anything))
                .Return(new Dictionary<string, IDataItemProxy> { { "tag2", tag2Stub } });
            
            m_CachedDataItemServiceMock
                .Stub(m => m.GetTags(Arg.Is(new[] { "tag1", "tag2" }), Arg<ILifetimeContext>.Is.Anything))
                .Return(new Dictionary<string, IDataItemProxy> { { "tag1", tag1Stub }, { "tag2", tag2Stub } });

            Session fakeSession = new Session(Guid.NewGuid().ToString(), DateTime.UtcNow + TimeSpan.FromMinutes(1));
            m_BatchTagsHandler = new BatchTagsHandler(fakeSession, m_CachedDataItemServiceMock);
        }

        [Test]
        public void Should_get_tags_from_tag_service()
        {
            BatchTagOperationDto batchTagOperationDto = new BatchTagOperationDto();
            batchTagOperationDto.getTags = new List<string> { "tag1", "tag2" };
            
            m_BatchTagsHandler.Post(batchTagOperationDto);

            m_CachedDataItemServiceMock.AssertWasCalled(m => m.GetTags(Arg<IEnumerable<string>>.Is.Anything, Arg<ILifetimeContext>.Is.Anything));
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

            m_CachedDataItemServiceMock.AssertWasCalled(m => m.SetTagValues(Arg<IDictionary<string, VariantValue>>.Is.Anything, Arg<ILifetimeContext>.Is.Anything));
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
