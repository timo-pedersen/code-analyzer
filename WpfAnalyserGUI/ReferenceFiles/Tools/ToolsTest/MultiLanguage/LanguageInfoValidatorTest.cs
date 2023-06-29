#if !VNEXT_TARGET
using System.Collections.Generic;
using System.ComponentModel;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class LanguageInfoValidatorTest
    {
        [Test]
        public void UniquePropertyExists()
        {
            ILanguageInfo languageInfo = new LanguageInfo();
            Assert.IsNotNull(TypeDescriptor.GetProperties(languageInfo).Find(LanguageInfoValidator.UniquePropertyName, false));
        }

        [Test]
        public void IsValidReturnsFalseWhenUsingSameIndex()
        {
            IPropertyValidator<ILanguageInfo> languageInfoValidator = new LanguageInfoValidator();

            ILanguageInfo languageInfoFirst = new LanguageInfo();
            languageInfoFirst.Index = 1;
            ILanguageInfo languageInfoSecond = new LanguageInfo();
            languageInfoSecond.Index = 2;
            ILanguageInfo languageInfoThird = new LanguageInfo();
            languageInfoSecond.Index = 1;

            IList<ILanguageInfo> languageInfos = new List<ILanguageInfo>(){languageInfoFirst,languageInfoSecond};

            bool isValid = languageInfoValidator.IsValid(languageInfoThird, LanguageInfoValidator.UniquePropertyName, languageInfoThird.Index, languageInfos);
            Assert.IsFalse(isValid);
        }
    }
}
#endif
