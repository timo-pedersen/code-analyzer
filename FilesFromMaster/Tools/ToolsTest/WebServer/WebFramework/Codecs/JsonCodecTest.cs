using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs
{
    [TestFixture]
    public class JsonCodecTest
    {
        private IResponse m_ResponseMock;

        private IRequest m_RequestMock;

        [SetUp] 
        public void Setup()
        {
            m_ResponseMock = MockRepository.GenerateMock<IResponse>();
            m_RequestMock = MockRepository.GenerateMock<IRequest>();
        }

        [Test]
        public void Should_write_object_represented_as_json_format_to_the_response()
        {
            Foo foo = new Foo { intValue = 22, stringValue = "foo" };
            string expectedJson = @"{""intValue"":22,""stringValue"":""foo""}";
            JsonCodec jsonCodec = new JsonCodec();

            jsonCodec.WriteTo(foo, m_ResponseMock);

            m_ResponseMock.AssertWasCalled(m => m.StringContent = expectedJson);
        }    
        
        [Test]
        public void Should_set_content_type_to_json()
        {
            Foo foo = new Foo();
            JsonCodec jsonCodec = new JsonCodec();

            jsonCodec.WriteTo(foo, m_ResponseMock);

            m_ResponseMock.AssertWasCalled(m => m.AddHeader("Content-Type", "application/json"));
        }
        
        [Test]
        public void Should_read_a_object_in_valid_json()
        {
            Foo foo = new Foo();
            JsonCodec jsonCodec = new JsonCodec();
            string json = @"{""intValue"":22,""stringValue"":""foo""}";
            m_RequestMock.Stub(m => m.Body).Return(json);

            object result = jsonCodec.ReadFrom(m_RequestMock, typeof(Foo), null);

            Assert.That(result, Is.InstanceOf<Foo>());
            Assert.That(((Foo)result).intValue, Is.EqualTo(22));
            Assert.That(((Foo)result).stringValue, Is.EqualTo("foo"));
        }

        [Test]
        public void Should_support_json_content_types()
        {
            Assert.That(new JsonCodec().SupportedMediaType, Is.EquivalentTo(new[] { new MediaType("application/json") }));
        }       
        
        [Test]
        public void Should_support_any_result_type()
        {
            Assert.That(new JsonCodec().SupportedTypes, Is.EquivalentTo(new[] { typeof(object) }));
        }


        private class Foo
        {
            public int intValue { get; set; }
            public string stringValue { get; set; }
        }
    }
}
