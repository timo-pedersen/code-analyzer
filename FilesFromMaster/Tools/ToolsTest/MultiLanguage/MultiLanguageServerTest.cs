using System;
using Core.Api.Application;
using Core.Api.Platform;
using Core.Api.ProjectOutput;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageServerTest
    {

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceMock<ICoreApplication>();
            TestHelperExtensions.AddServiceToolManager(false);

            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());

            IExtendedBindingList<ILanguageInfo> bindingList = new ExtendedBindingList<ILanguageInfo>();
            IMultiLanguageServiceCF multiLanguageServiceCF = TestHelper.AddServiceStub<IMultiLanguageServiceCF>();
            multiLanguageServiceCF.Stub(x => x.CreateLanguageList()).Return(bindingList);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetBuildFilesReturnsOnlySelectedLanguages()
        {
            string dir = @"c:\";
            string lang1 = "lang1", lang2 = "lang2";
            string designerExtension = ".Designer.lng";
            string systemTextExtension = ".lng";

            MultiLanguageServer multiLanguageServer = new MultiLanguageServer();
            IAdditionalStorage additionalStorage = multiLanguageServer;

            multiLanguageServer.Languages.Add(new LanguageInfo { Name = lang1, UseInRuntime = false });
            multiLanguageServer.Languages.Add(new LanguageInfo { Name = lang2, UseInRuntime = true });

            string[] files = additionalStorage.GetBuildFiles(dir, string.Empty, TargetPlatform.Windows);

            Assert.AreEqual(2, files.Length); //Both designer, systemtexts
            Assert.AreEqual(string.Concat(dir, lang2, systemTextExtension), files[0]);
            Assert.AreEqual(string.Concat(dir, lang2, designerExtension), files[1]);
        }

        [Test]
        public void GetBuildFilesReturnsOnlySelectedLanguagesWhenTextIDEnabled()
        {
            string dir = @"c:\";
            string lang1 = "lang1", lang2 = "lang2";
            string designerExtension = ".Designer.lng";
            string systemTextExtension = ".lng";
            string textIDExtension = ".TextIDs.lng";

            MultiLanguageServer multiLanguageServer = new MultiLanguageServer();
            multiLanguageServer.MultiLanguageService.Stub(x => x.IsTextIDEnabled()).Return(true);
            IAdditionalStorage additionalStorage = multiLanguageServer;

            multiLanguageServer.Languages.Add(new LanguageInfo { Name = lang1, UseInRuntime = false });
            multiLanguageServer.Languages.Add(new LanguageInfo { Name = lang2, UseInRuntime = true });

            string[] files = additionalStorage.GetBuildFiles(dir, string.Empty, TargetPlatform.Windows);

            Assert.AreEqual(4, files.Length); //Both designer, systemtexts, TextIDs + Default.TextID.lng
            Assert.AreEqual(string.Concat(dir, lang2, systemTextExtension), files[0]);
            Assert.AreEqual(string.Concat(dir, lang2, designerExtension), files[1]);
            Assert.AreEqual(string.Concat(dir, lang2, textIDExtension), files[2]);
            Assert.AreEqual(string.Concat(dir, "Default", textIDExtension), files[3]);
        }


        [Test]
        public void LanguagesListChangedThrowsOnEmptyLanguageName()
        {
            MultiLanguageServer multiLanguageServer = new MultiLanguageServer();

            LanguageInfo languageInfo = new LanguageInfo() { Name = "Nisse" };
            multiLanguageServer.Languages.Add(languageInfo);

            Assert.Throws<ArgumentException>(() => languageInfo.Name = "");
        }

        [Test]
        public void LanguagesListChangedThrowsWhenLanguageNameIsNull()
        {
            MultiLanguageServer multiLanguageServer = new MultiLanguageServer();

            LanguageInfo languageInfo = new LanguageInfo() { Name = "Nisse" };
            multiLanguageServer.Languages.Add(languageInfo);

            Assert.Throws<ArgumentException>(() => languageInfo.Name = null);
        }
    }
}
