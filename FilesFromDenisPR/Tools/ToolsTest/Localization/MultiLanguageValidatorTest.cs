using System;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.MultiLanguage;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Localization
{
    [TestFixture]
    public class MultiLanguageValidatorTest
    {
        private const string Swedish = "Swedish (Sweden)";
        private const string French = "French (France)";
        private const string Pirate = "Pirate (Yarr)";

        private IMultiLanguageService m_MultiLanguageService;
        private LanguageValidator m_Validator;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IErrorListService>();
            m_MultiLanguageService = Substitute.For<IMultiLanguageService>();
            m_Validator = new LanguageValidator(m_MultiLanguageService.ToILazy());
        }

        [Test]
        public void TestValidatesLanguages()
        {
            var languages = new ExtendedBindingList<ILanguageInfo>();
            languages.Add(new LanguageInfo { Name = Swedish });
            languages.Add(new LanguageInfo { Name = French });
            m_MultiLanguageService.Languages.Returns(languages);

            Assert.IsTrue(m_Validator.Validate());
        }

        [Test]
        public void TestSucceedsWithUnknownLanguages()
        {
            var languages = new ExtendedBindingList<ILanguageInfo>();
            languages.Add(new LanguageInfo { Name = Swedish });
            languages.Add(new LanguageInfo { Name = Pirate });
            m_MultiLanguageService.Languages.Returns(languages);

            Assert.IsTrue(m_Validator.Validate());
        }
    }
}
