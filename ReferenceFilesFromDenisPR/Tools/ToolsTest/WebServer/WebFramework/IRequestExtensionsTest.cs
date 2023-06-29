using System;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework
{
    [TestFixture]
    public class IRequestExtensionsTest
    {
        private IRequest m_RequestMock;

        [SetUp]
        public void Setup()
        {
            m_RequestMock = Substitute.For<IRequest>();
        }

        [Test]
        public void Should_be_modified_when_last_write_is_more_recent_than_last_modified()
        {
            DateTime lastWrite = new DateTime(2000, 06, 02, 0, 0, 0, DateTimeKind.Utc);
            StubLastModified(new DateTime(2000, 06, 01, 0, 0, 0, DateTimeKind.Utc));

            bool isModified = m_RequestMock.IsModified(lastWrite);

            Assert.That(isModified, Is.True);
        }

        [Test]
        public void Should_not_be_modified_when_last_write_is_equal_to_last_modified()
        {
            using (var swedishCulture = new SelectSwedishTestingCulture())
            {
                DateTime lastWrite = new DateTime(2000, 06, 02, 0,0,0, DateTimeKind.Utc);
                StubLastModified(new DateTime(2000, 06, 02, 0,0,0, DateTimeKind.Utc));

                bool isModified = m_RequestMock.IsModified(lastWrite);

                Assert.That(isModified, Is.False);
            }
        }

        [Test]
        public void Should_not_be_modified_when__last_modified()
        {
            DateTime lastWrite = new DateTime(2000, 06, 02, 0, 0, 0, DateTimeKind.Utc);
            StubLastModified(new DateTime(2000, 06, 04, 0, 0, 0, DateTimeKind.Utc));

            bool isModified = m_RequestMock.IsModified(lastWrite);

            Assert.That(isModified, Is.False);
        }

        [Test]
        public void Should_be_modified_if_no_last_modified_header_was_supplied_in_request()
        {
            DateTime lastWrite = new DateTime(2000, 06, 02, 0, 0, 0, DateTimeKind.Utc);

            bool isModified = m_RequestMock.IsModified(lastWrite);

            Assert.That(isModified, Is.True);
        }

        private void StubLastModified(DateTime lastModifiedsd)
        {
            var lastModified = lastModifiedsd.ToString("R");            
            m_RequestMock.GetHeader("If-Modified-Since").Returns(lastModified);
        }
    }
}
