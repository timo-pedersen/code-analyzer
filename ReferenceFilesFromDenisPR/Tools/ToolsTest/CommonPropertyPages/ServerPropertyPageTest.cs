using System.IO;
using System.Windows.Forms;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Security;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CommonPropertyPages
{
    [TestFixture]
    public class ServerPropertyPageTest
    {
        private class TestServerPropertyPage : ServerPropertyPage
        {
            public bool ValidatePassword(string password) { return ValidatePassword("PasswordLabel", password); }
        }

        private IMessageBoxServiceIde m_MessageBoxService;

        [SetUp]
        public void SetUp()
        {
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string blacklistPath = Path.Combine(assemblyDirectory, "Configurations", PasswordValidator.BlacklistFileName);
            var fileSettingsServiceStub = TestHelper.AddServiceStub<IFileSettingsServiceIde>();
            fileSettingsServiceStub.FileNameInCommonApplicationDataFolder(Arg.Any<string>()).Returns(blacklistPath);
            m_MessageBoxService = TestHelper.CreateAndAddServiceStub<IMessageBoxServiceIde>();
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
            int messageBoxShowCount = expectedValid ? 0 : 1;
            var testServerPropertyPage = new TestServerPropertyPage();

            Assert.AreEqual(expectedValid, testServerPropertyPage.ValidatePassword(password));
            m_MessageBoxService.Received(messageBoxShowCount)
                .Show(Arg.Any<string>(),
                    Arg.Any<string>(),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2,
                    DialogResult.No);
        }
    }
}
