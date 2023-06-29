using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.Website.Tags;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.Website
{
    [TestFixture]
    public class JsonWithTagOptimizationCodecTest
    {
        private readonly string getTagsOnlyT1AndT2 = "{\"getTags\":[\"T1\",\"T2\"]}";
        private readonly string setTagsOnlyT3AndT4 = "{\"setTags\":[{\"name\":\"T3\",\"value\":\"foo\"},{\"name\":\"T4\",\"value\":1234}]}";
        private readonly string setGetAndMetadata = "{\"setTags\":[{\"name\":\"T3\",\"value\":\"foo\"},{\"name\":\"T4\",\"value\":1234}], \"getTags\":[\"T1\",\"T2\"], \"includeTagMetadata\": true}";
        private readonly string getSetAndMetadata = "{\"getTags\":[\"T1\",\"T2\"], \"setTags\":[{\"name\":\"T3\",\"value\":\"foo\"},{\"name\":\"T4\",\"value\":1234}], \"includeTagMetadata\": true}";
        private readonly string MetadataSetGet = "{\"includeTagMetadata\": true, \"setTags\":[{\"name\":\"T3\",\"value\":\"foo\"},{\"name\":\"T4\",\"value\":1234}], \"getTags\":[\"T1\",\"T2\"]}";
        private readonly string metadata = "{\"includeTagMetadata\": true }";
        
        [Test]
        public void Should_deserialize_batchtagoperation_get_tags_property()
        {
            BatchTagOperationDto result = DeserializeBatchTagOperationDto(getTagsOnlyT1AndT2) as BatchTagOperationDto;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.getTags, Is.Not.Null);
            Assert.That(result.getTags, Contains.Item("T1"));
            Assert.That(result.getTags, Contains.Item("T2"));
        }
        
        [Test]
        public void Should_deserialize_batchtagoperation_set_tags_property()
        {
            BatchTagOperationDto result = DeserializeBatchTagOperationDto(setTagsOnlyT3AndT4) as BatchTagOperationDto;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.setTags, Is.Not.Null);
            Assert.That(result.setTags, Has.Count.EqualTo(2));
            Assert.That(result.setTags[0].name, Is.EqualTo("T3"));
            Assert.That(result.setTags[0].value, Is.EqualTo("foo"));
            Assert.That(result.setTags[1].name, Is.EqualTo("T4"));
            Assert.That(result.setTags[1].value, Is.EqualTo(1234));
        }
        
        [Test]
        public void Should_deserialize_batchtagoperation_include_metadata_property()
        {
            BatchTagOperationDto result = DeserializeBatchTagOperationDto(metadata) as BatchTagOperationDto;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.includeTagMetadata, Is.True);
        }

        [Test]
        public void Should_deserialize_batchtagoperation_property_regardless_of_order()
        {
            try
            {
                DeserializeBatchTagOperationDto(setGetAndMetadata);
                DeserializeBatchTagOperationDto(getSetAndMetadata);
                DeserializeBatchTagOperationDto(MetadataSetGet);
            } 
            catch (Exception ex)
            {
                Assert.Fail("Unable to parse json.\n" + ex);
            }
        }

        [Test]
        public void Should_serialize_batch_tag_operation_result_with_tag_value_dto()
        {
            DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            BatchTagOperationResultDto dto = new BatchTagOperationResultDto();
            dto.tags = new[]
                {
                    new TagValueDto { name = "foo", value = 123 }, 
                    new TagValueDto { name = "bar", value = dateTime } 
                };
            
            AssertSerializedResult(dto, @"{""tags"":[{""name"":""foo"",""value"":123},{""name"":""bar"",""value"":""2000-01-01T00:00:00Z""}]}");
        }

        [Test]
        public void Should_serialize_batch_tag_operation_result_with_tag_dto()
        {
            DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            BatchTagOperationResultDto dto = new BatchTagOperationResultDto();
            dto.tags = new[]
                {
                    new TagDto
                        {
                            name = "T1", 
                            value = 58,
                            controllers = new List<string> { "Controller1" },
                            quality = "Good",
                            dataType = "uint16",
                            initialValue = 10,
                            @readonly = false,
                            description = "foo bar"
                        }, 
                    new TagDto
                        {
                            name = "T2", 
                            value = dateTime,
                            controllers = new List<string> { "Controller1" },
                            quality = "Good",
                            dataType = "uint16",
                            initialValue = dateTime.AddDays(1),
                            @readonly = true,
                            description = "foo bar"
                        }, 
                };

            string serializationResult = 
                "{\"tags\":[{\"quality\":\"Good\",\"description\":\"foo bar\",\"name\":\"T1\",\"value\":58,\"dataType\":\"uint16\",\"controllers\":[\"Controller1\"],\"initialValue\":10,\"readonly\":false}," +
                "{\"quality\":\"Good\",\"description\":\"foo bar\",\"name\":\"T2\",\"value\":\"2000-01-01T00:00:00Z\",\"dataType\":\"uint16\",\"controllers\":[\"Controller1\"],\"initialValue\":\"2000-01-02T00:00:00Z\",\"readonly\":true}]}";
            AssertSerializedResult(dto, serializationResult);
        }

        [Test]
        public void Should_add_application_json_content_type_header()
        {
            IResponse response = Substitute.For<IResponse>();
            BatchTagOperationResultDto dto = new BatchTagOperationResultDto();
            dto.tags = new[] { new TagDto() };

            JsonWithTagOptimizationCodec jsonWithTagOptimizationCodec = new JsonWithTagOptimizationCodec();
            jsonWithTagOptimizationCodec.WriteTo(dto, response);

            response.Received().AddHeader("Content-Type", "application/json");
        }

        private object DeserializeBatchTagOperationDto(string body)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            IRequest response = Substitute.For<IRequest>();
            response.BodyStream.Returns(memoryStream);

            JsonWithTagOptimizationCodec deserializer = new JsonWithTagOptimizationCodec();
            return deserializer.ReadFrom(response, typeof(BatchTagOperationDto), null);
        }

        private void AssertSerializedResult(BatchTagOperationResultDto dto, string tagsNameFooValueNameBarValueT00Z)
        {
            IResponse response = Substitute.For<IResponse>();
            
            JsonWithTagOptimizationCodec jsonWithTagOptimizationCodec = new JsonWithTagOptimizationCodec();
            jsonWithTagOptimizationCodec.WriteTo(dto, response);

            Stream streamContent = response.StreamContent;
            StreamReader reader = new StreamReader(streamContent);
            Assert.That(reader.ReadToEnd(), Is.EqualTo(tagsNameFooValueNameBarValueT00Z));
        }
    }
}
