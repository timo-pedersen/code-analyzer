using System.IO;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Security;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.ProjectValidation
{
    public class ServerProjectValidatorTest
    {
        private class TestServerProjectValidator : ServerProjectValidator
        {
            public void ValidatePassword(string password) { ValidatePassword("ComponentName", "PasswordLabel", password); }
        }

        private IErrorListService m_ErrorListService;

        [SetUp]
        public void SetUp()
        {
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string blacklistPath = Path.Combine(assemblyDirectory, "Configurations", PasswordValidator.BlacklistFileName);
            var fileSettingsServiceStub = TestHelper.AddServiceStub<IFileSettingsServiceIde>();
            fileSettingsServiceStub.Stub(x => x.FileNameInCommonApplicationDataFolder(Arg<string>.Is.Anything)).Return(blacklistPath);
            m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [TestCase("", false)]
        [TestCase("password", false)]
        [TestCase("goijsdfgojsdfogJ", true)]
        public void ValidatePassword(string password, bool expectedValid)
        {
            int addNewCompilerErrorCount = expectedValid ? 0 : 1;
            var testServerProjectValidator = new TestServerProjectValidator();

            testServerProjectValidator.ValidatePassword(password);

            m_ErrorListService.AssertWasCalled(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything), options => options.Repeat.Times(addNewCompilerErrorCount));
        }
    }
}
