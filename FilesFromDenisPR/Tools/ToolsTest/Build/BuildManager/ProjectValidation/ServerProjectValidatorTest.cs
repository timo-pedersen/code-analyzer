using System.IO;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Security;
using NSubstitute;
using NUnit.Framework;

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
            fileSettingsServiceStub.FileNameInCommonApplicationDataFolder(Arg.Any<string>()).Returns(blacklistPath);
            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();
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

            m_ErrorListService.Received(addNewCompilerErrorCount)
                .AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }
    }
}
