using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Codecs
{
    [TestFixture]
    public class ErrorPageWriterTest
    {
        private IResponse m_ResponseMock;

        [SetUp]
        public void Setup()
        {
            m_ResponseMock = MockRepository.GenerateMock<IResponse>();
        }

        [Test]
        public void Should_set_response_error()
        {
            ErrorPageWriter errorPageWriter = new ErrorPageWriter();

            errorPageWriter.WriteTo(new ErrorPage(400, "Not Found"), m_ResponseMock);

            m_ResponseMock.AssertWasCalled(m => m.SetErrorResponse(Arg.Is(400), Arg.Is("Not Found"), Arg<string>.Is.Anything));
        }
    }
}