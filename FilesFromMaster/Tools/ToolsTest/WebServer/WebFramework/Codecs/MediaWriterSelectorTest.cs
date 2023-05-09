using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework
{
    [TestFixture]
    public class MediaWriterSelectorTest
    {
        private MediaWriterSelector m_MediaWriterSelector;

        [SetUp]
        public void Setup()
        {
            m_MediaWriterSelector = new MediaWriterSelector();
        }

        [Test]
        public void Should_match_application_octed_stream_media_writer_for_file_resource()
        {
            var writer = m_MediaWriterSelector.GetBestMatchingMediaWriter(typeof(FileResource));

            Assert.That(writer, Is.InstanceOf<ApplicationOctetStreamWriter>());
        }
        
        [Test]
        public void Should_match_error_page_writer_for_error_page_result()
        {
            IMediaTypeWriter writer = m_MediaWriterSelector.GetBestMatchingMediaWriter(typeof(ErrorPage));

            Assert.That(writer, Is.InstanceOf<ErrorPageWriter>());
        }

        [Test]
        public void Should_match_the_hight_quality_media_writer_available()
        {
            IMediaTypeWriter barWriter = MockRepository.GenerateStub<IMediaTypeWriter>();
            IMediaTypeWriter bazWriter = MockRepository.GenerateStub<IMediaTypeWriter>();
            barWriter.Stub(m => m.SupportedTypes).Return(new[] { typeof(FooType) });
            barWriter.Stub(m => m.SupportedMediaType).Return(new[] { new MediaType("foo/bar"),  });
            bazWriter.Stub(m => m.SupportedTypes).Return(new[] { typeof(FooType) });
            bazWriter.Stub(m => m.SupportedMediaType).Return(new[] { new MediaType("foo/baz"),  });

            string acceptType = "not/known;, foo/bar;q=0.5, foo/baz;q=1";
            
            IMediaTypeWriter writer = m_MediaWriterSelector.GetBestMatchingMediaWriter(
                acceptType, 
                typeof(FooType),
                new[] { barWriter, bazWriter });

            Assert.That(writer, Is.SameAs(bazWriter));
        }
        
        [Test]
        public void Should_pick_writer_when_type_is_assignable_from_supported_type()
        {
            IMediaTypeWriter barWriter = MockRepository.GenerateStub<IMediaTypeWriter>();
            barWriter.Stub(m => m.SupportedTypes).Return(new[] { typeof(object) });
            barWriter.Stub(m => m.SupportedMediaType).Return(new[] { new MediaType("foo/bar"),  });
            
            string acceptType = "foo/bar";
            
            IMediaTypeWriter writer = m_MediaWriterSelector.GetBestMatchingMediaWriter(
                acceptType, 
                typeof(FooType),
                new[] { barWriter });

            Assert.That(writer, Is.SameAs(barWriter));
        }

        [Test]
        public void Should_return_null_when_no_matching_writers_are_found()
        {
            IMediaTypeWriter writer = m_MediaWriterSelector.GetBestMatchingMediaWriter(
                "not-a-known-media/type",
                typeof(FooType),
                new IMediaTypeWriter[0]);

            Assert.That(writer, Is.Null);
        }

        private class FooType
        {
            
        }
    }
}