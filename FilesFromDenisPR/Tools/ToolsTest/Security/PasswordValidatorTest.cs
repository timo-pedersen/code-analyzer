#if !VNEXT_TARGET
using System.IO;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class PasswordValidatorTest
    {
        [SetUp]
        public void SetUp()
        {
            string assemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string blacklistPath = Path.Combine(assemblyDirectory, "Configurations", PasswordValidator.BlacklistFileName);
            var fileSettingsServiceStub = TestHelper.AddServiceStub<IFileSettingsServiceIde>();
            fileSettingsServiceStub.FileNameInCommonApplicationDataFolder(Arg.Any<string>()).Returns(blacklistPath);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [TestCase("", PasswordStrength.None)]
        [TestCase("xJ1", PasswordStrength.VeryWeak)]
        [TestCase("qwertyui", PasswordStrength.VeryWeak)]
        [TestCase("password", PasswordStrength.VeryWeak)]
        [TestCase("superman", PasswordStrength.VeryWeak)]
        [TestCase("goijsdfgojsdfogJ", PasswordStrength.Medium)]
        [TestCase("kjdsAd%o49i", PasswordStrength.Strong)]
        [TestCase("kjdsAd%o49ikgt", PasswordStrength.VeryStrong)]
        public void Strength(string password, PasswordStrength expectedStrength)
        {
            Assert.AreEqual(expectedStrength, PasswordValidator.Instance.GetStrength(password));
        }
    }
}
#endif
