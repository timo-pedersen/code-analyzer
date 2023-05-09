using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs
{
    [TestFixture]
    public class WwwUrlFormReaderTest
    {
        private IRequest m_Request;

        private WwwUrlFormReader m_WwwUrlFormReader;

        [SetUp]
        public void SetUp()
        {
            m_Request = Substitute.For<IRequest>();
            m_WwwUrlFormReader = new WwwUrlFormReader();
        }

        [Test]
        public void Should_parse_simple_values_to_object()
        {
            StubContent("param1.key=foo&param1.value=bar");
            FooKVP result = m_WwwUrlFormReader.ReadFrom(m_Request, typeof(FooKVP), "param1") as FooKVP;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Key, Is.EqualTo("foo"));
            Assert.That(result.Value, Is.EqualTo("bar"));
        }       
        
        [Test]
        public void Should_ignore_missing_parameters()
        {
            StubContent("param1.key=foo");
            FooKVP result = m_WwwUrlFormReader.ReadFrom(m_Request, typeof(FooKVP), "param1") as FooKVP;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Null);
        }

        [Test]
        public void Should_decode_parameter_values()
        {
            StubContent("param1.key=foo%20bar");
            FooKVP result = m_WwwUrlFormReader.ReadFrom(m_Request, typeof(FooKVP), "param1") as FooKVP;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Key, Is.EqualTo("foo bar"));
        }

        private void StubContent(string content)
        {
            m_Request.ContentLength.Returns(content.Length);
            m_Request.Body.Returns(content);
        }

        public class FooKVP
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}
