using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Registration
{
    [TestFixture]
    public class KeyValidatorTest
    {
        private const string RegistrationCodeSeed = "beijer";
        private const string RegistrationCodeSeed2 = "beijer2";
        private const string InvalidLicenseKey = "testar";
        private const string ValidLicenseKey = "2C41-99B7-E746-7B71";
        private const string ValidDemoLicenseKey = "E42C-AE80-3AB5-8FD6";
        private const string BlackListedLicenseKey = "192F-D65E-48DE-1AF9";
        private const string BlackListedLicenseKeyCaseSensitive = "192f-d65e-48de-1af9";
        private const string BlackListedLicenseKeyModified = "192F-D65E-48DE1AF9";

        private IBrandServiceIde m_BrandService;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            m_BrandService = TestHelper.CreateAndAddServiceStub<IBrandServiceIde>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void InvalidLicenseKeyReturnsFalse()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator();
            bool valid = validator.IsValidKey(InvalidLicenseKey);

            Assert.IsFalse(valid);
        }

        [Test]
        public void ValidLicenseKeyReturnsTrue()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator();
            bool valid = validator.IsValidKey(ValidLicenseKey);

            Assert.IsTrue(valid);
        }

        [Test]
        public void EmptyLicenseKeyReturnsFalse()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator();
            bool valid = validator.IsValidKey(string.Empty);

            Assert.IsFalse(valid);
        }

        [Test]
        public void EmptyBrandNameReturnsFalse()
        {
            m_BrandService.RegistrationCodeSeed.Returns(string.Empty);
            KeyValidator validator = new KeyValidator();
            bool valid = validator.IsValidKey(ValidLicenseKey);

            Assert.IsFalse(valid);
        }

        [Test]
        public void IsDemoEqualsFalseIsSameAsEmptyConstructor()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator(false);
            bool valid = validator.IsValidKey(ValidLicenseKey);
            Assert.IsTrue(valid);
        }

        [Test]
        public void IsDemoEqualsTrueIsNotValid()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator(true);
            bool valid = validator.IsValidKey(ValidLicenseKey);
            Assert.IsFalse(valid);
        }

        [Test]
        public void InvalidDemoLicenseKeyReturnsFalse()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator(true);
            bool valid = validator.IsValidKey(InvalidLicenseKey);

            Assert.IsFalse(valid);
        }

        [Test]
        public void ValidDemoLicenseKeyReturnsTrue()
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed);
            KeyValidator validator = new KeyValidator(true);
            bool valid = validator.IsValidKey(ValidDemoLicenseKey);

            Assert.IsTrue(valid);
        }

        [TestCase(BlackListedLicenseKey)]
        [TestCase(BlackListedLicenseKeyModified)]
        [TestCase(BlackListedLicenseKeyCaseSensitive)]
        public void BlackListedLicenseKeyReturnsFalse(string blackListedLicenseKey)
        {
            m_BrandService.RegistrationCodeSeed.Returns(RegistrationCodeSeed2);
            KeyValidator validator = new KeyValidator();
            bool valid = validator.IsValidKey(blackListedLicenseKey);

            Assert.IsFalse(valid);
        }
    }
}
