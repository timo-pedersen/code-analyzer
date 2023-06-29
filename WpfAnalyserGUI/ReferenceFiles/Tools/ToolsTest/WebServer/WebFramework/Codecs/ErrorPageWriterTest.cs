using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs
{
    [TestFixture]
    public class ErrorPageWriterTest
    {
        private IResponse m_ResponseMock;

        [SetUp]
        public void Setup()
        {
            m_ResponseMock = Substitute.For<IResponse>();
        }

        [Test]
        public void Should_set_response_error()
        {
            ErrorPageWriter errorPageWriter = new ErrorPageWriter();

            errorPageWriter.WriteTo(new ErrorPage(400, "Not Found"), m_ResponseMock);

            m_ResponseMock.Received().SetErrorResponse(Arg.Is(400), Arg.Is("Not Found"), Arg.Any<string>());
        }
    }
}