#if !VNEXT_TARGET
using System.Collections.Generic;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageImportStrategyBaseTest
    {

        [Test]
        public void ImportLanguages_calls_AddLanguage_on_MultiLanguageService()
        {
            IMultiLanguageServiceIde multiLanguageServiceIde = Substitute.For<IMultiLanguageServiceIde>();
            multiLanguageServiceIde.Languages.Returns(new ExtendedBindingList<ILanguageInfo>());

            MultiLanguageImportKeyStrategy strategy = new MultiLanguageImportKeyStrategy(multiLanguageServiceIde);

            strategy.ImportLanguages(new List<string> { "lang" });

            multiLanguageServiceIde.Received().AddLanguage("lang");
        }

        [Test]
        public void ImportLanguages_does_not_call_AddLanguage_on_a_second_copy_of_a_language_if_it_already_exists()
        {
            List<string> languagesToImport = new List<string> { "sv-SE", };

            var languageInfos = new ExtendedBindingList<ILanguageInfo> { new LanguageInfo { Name = "sv-SE" } };

            var multiLanguageService = Substitute.For<IMultiLanguageServiceIde>();
            multiLanguageService.Languages.Returns(languageInfos);

            var strategy = new MultiLanguageImportKeyStrategy(multiLanguageService);

            strategy.ImportLanguages(languagesToImport);

            multiLanguageService.DidNotReceiveWithAnyArgs().AddLanguage(Arg.Any<string>());
        }

        [Test]
        public void ImportLanguages_does_not_add_a_language_with_an_empty_name()
        {
            List<string> languagesToImport = new List<string>() { string.Empty };

            var languageInfos = new ExtendedBindingList<ILanguageInfo>();

            var multiLanguageService = Substitute.For<IMultiLanguageServiceIde>();
            multiLanguageService.Languages.Returns(languageInfos);

            var strategy = new MultiLanguageImportKeyStrategy(multiLanguageService);

            strategy.ImportLanguages(languagesToImport);

            Assert.AreEqual(0, languageInfos.Count);
        }
    }

}
#endif
