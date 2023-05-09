using System.IO;
using System.Windows.Forms;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Security;
using NUnit.Framework;
using Rhino.Mocks;

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
            fileSettingsServiceStub.Stub(x => x.FileNameInCommonApplicationDataFolder(Arg<string>.Is.Anything)).Return(blacklistPath);
            m_MessageBoxService = TestHelper.CreateAndAddServiceMock<IMessageBoxServiceIde>();
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
            m_MessageBoxService.AssertWasCalled(x => x.Show(Arg<string>.Is.Anything,
                                                            Arg<string>.Is.Anything,
                                                            Arg<MessageBoxButtons>.Is.Equal(MessageBoxButtons.YesNo),
                                                            Arg<MessageBoxIcon>.Is.Equal(MessageBoxIcon.Exclamation),
                                                            Arg<MessageBoxDefaultButton>.Is.Equal(MessageBoxDefaultButton.Button2),
                                                            Arg<DialogResult>.Is.Equal(DialogResult.No)),
                                                            options => options.Repeat.Times(messageBoxShowCount));
        }
    }
}
