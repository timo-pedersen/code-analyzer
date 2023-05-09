

using System.Collections.Generic;
using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using Neo.ApplicationFramework.Tools.WebServer.Website.Shared;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication.Forms
{
    [TestFixture]
    public class LoginHandlerTest
    {
        private LoginHandler m_LoginHandler;

        private FileReader m_FileReader;

        private EmbeddedFileReader m_EmbeddedFileReader;

        private ICommunicationContext m_Context;

        private FormsAuthentication m_FormsAuthentication;

        private readonly string m_StartupPath = Path.Combine(Path.GetTempPath(), typeof(LoginHandlerTest).Name);

        [SetUp]
        public void Setup()
        {
            var coreApplication = TestHelper.CreateAndAddServiceMock<ICoreApplication>();
            coreApplication.Stub(inv => inv.StartupPath).Return(m_StartupPath);
            Directory.CreateDirectory(m_StartupPath);

            m_FormsAuthentication = MockRepository.GenerateStub<FormsAuthentication>(new Dictionary<string, string>());
            m_EmbeddedFileReader = MockRepository.GenerateStub<EmbeddedFileReader>(null, null, null, null);
            m_FileReader = MockRepository.GenerateStub<FileReader>(null, null, null);

            m_Context = CommunicationContextFixture.AfterOperationCandidateGeneration;
            m_LoginHandler = new LoginHandler(m_Context.Session, m_Context.Response, m_FormsAuthentication, m_EmbeddedFileReader, m_FileReader);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(m_StartupPath, true);
        }


        [Test]
        public void Should_get_embedded_login_page_when_no_page_exist_in_webroot()
        {
            object embeddedFileResult = new object();
            m_FileReader.Stub(s => s.HasFile(null)).IgnoreArguments().Return(false);
            m_EmbeddedFileReader.Stub(s => s.Get("login.html")).Return(embeddedFileResult);

            object handlerResult = m_LoginHandler.Get();

            Assert.That(handlerResult, Is.SameAs(embeddedFileResult));
        }

        [Test]
        public void Should_get_login_page_from_webroot_when_it_exists()
        {
            object fileReaderResult = new object();
            m_FileReader.Stub(s => s.HasFile("login.html")).Return(true);
            m_FileReader.Stub(s => s.Get("login.html")).Return(fileReaderResult);

            object handlerResult = m_LoginHandler.Get();

            Assert.That(handlerResult, Is.SameAs(fileReaderResult));
        }

        [Test]
        public void Should_return_200_ok_when_posting_valid_credentials()
        {
            m_FormsAuthentication.Stub(s => s.Login(null, null, null)).IgnoreArguments().Return(true);

            object post = m_LoginHandler.Post(new LoginDataDto());

            Assert.That(post, Is.TypeOf<OperationResult>());
            Assert.That(((OperationResult)post).HttpStatusCode, Is.EqualTo(200));
        }

        [Test]
        public void Should_return_401_Not_Authurized_when_posting_invalid_credentials()
        {
            m_FormsAuthentication.Stub(s => s.Login(null, null, null)).IgnoreArguments().Return(false);

            object post = m_LoginHandler.Post(new LoginDataDto());

            Assert.That(post, Is.TypeOf<OperationResult>());
            Assert.That(((OperationResult)post).HttpStatusCode, Is.EqualTo(401));
        }
    }
}
