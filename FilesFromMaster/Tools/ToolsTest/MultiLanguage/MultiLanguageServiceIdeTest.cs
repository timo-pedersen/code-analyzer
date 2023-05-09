using System.Collections.Generic;
using System.ComponentModel.Design;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.MultiLanguage.TextID;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageServiceIdeTest
    {
        private MultiLanguageServiceIde m_MultiLanguageService;
        private IMultiLanguageServiceIde m_IMultiLanguageService;

        [SetUp]
        public void SetUp()
        {
            IProjectManager projectManager = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            projectManager.Stub(x => x.IsProjectLoading).Return(true);

            m_IMultiLanguageService = m_MultiLanguageService = new MultiLanguageServiceIde();
            TestHelper.AddService(typeof(IMultiLanguageServiceCF), m_MultiLanguageService);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void AddLanguageCreatesUniqueIndexWithOneAsIndex()
        {
            string language = "swedish";
            IExtendedBindingList<ILanguageInfo> languageInfos;

            m_IMultiLanguageService.MultiLanguageServer = StubMultiLanguageServer(out languageInfos);        
            ILanguageInfo newLanguageInfo = m_IMultiLanguageService.AddLanguage(language);
            
            Assert.AreEqual(1, newLanguageInfo.Index);
            Assert.AreEqual(1, languageInfos[0].Index);
        }

        [Test]
        public void AddLanguageCreatesNewLanguageInfoInMultiLanguageServer()
        {
            string language = "swedish";
            IExtendedBindingList<ILanguageInfo> languageInfos;

            m_IMultiLanguageService.MultiLanguageServer = StubMultiLanguageServer(out languageInfos);
            ILanguageInfo newLanguageInfo = m_IMultiLanguageService.AddLanguage(language);

            Assert.AreEqual(1, ((ICollection<ILanguageInfo>)languageInfos).Count);
            Assert.That(languageInfos[0].Name == language);
        }

        private IMultiLanguageServer StubMultiLanguageServer(out IExtendedBindingList<ILanguageInfo>  languageInfos)
        {
            IMultiLanguageServer multiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Stub(x => x.Languages).Return(languageInfos);
            IResourceItemList<IDesignerResourceItem> resourceItems = new ResourceItemList<DesignerResourceItem,IDesignerResourceItem>();
            multiLanguageServer.Stub(x => x.ResourceItems).Return(resourceItems);

            return multiLanguageServer;
        }

        #region Save Tests

        [Test]
        public void SaveCallsSerializer()
        {
            string filename = "testfile";
            IMultiLanguageServer multiLanguageServer;
            ResourceItemList<DesignerResourceItem, IDesignerResourceItem> resourceItems;
            resourceItems = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();

            ResourceItemList<TextIDResourceItem, ITextIDResourceItem> textIDResourceItems;
            textIDResourceItems = new ResourceItemList<TextIDResourceItem, ITextIDResourceItem>();

            multiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            multiLanguageServer.Stub(srv => srv.ResourceItems).Return(resourceItems);
            multiLanguageServer.Stub(srv => srv.TextIDResourceItems).Return(textIDResourceItems);

            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Stub(x => x.Languages).Return(languageInfos);

            var multiLanguageSerializer = MockRepository.GenerateMock<MultiLanguageSerializer>();
            m_MultiLanguageService.MultiLanguageSerializer = multiLanguageSerializer;

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;

            m_IMultiLanguageService.Save(filename);

            multiLanguageSerializer.AssertWasCalled(x => x.Save<DesignerResourceItem>(Arg.Is(filename), Arg<IEnumerable<IResourceItem>>.Is.NotNull));
        }

        #endregion

        #region Import Tests

        [Test]
        public void ImportCallsGetLanguagesAndImportOnTheMultiLanguageImporter()
        {
            const string filePath = "The file path";
            var importModuleStub = MockRepository.GenerateStub<IImportModule>();

            var languageNamesToImport = new List<string>() { "The language name" };

            var multiLanguageImporter = MockRepository.GenerateMock<MultiLanguageImporter<DesignerResourceItem>>((IMultiLanguageImportStrategy)null);
            multiLanguageImporter.Expect(importer => ((IMultiLanguageImporter)importer).Import(Arg.Is(importModuleStub), Arg.Is(languageNamesToImport), Arg<IEnumerable<IResourceItem>>.Is.NotNull, Arg.Is(','))).Repeat.Once();

            IResourceItemList<IDesignerResourceItem> resourceList = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            IMultiLanguageServer multiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            multiLanguageServer.Stub(server => server.ResourceItems).Return(resourceList);
            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Stub(x => x.Languages).Return(languageInfos);

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;
            m_MultiLanguageService.MultiLanguageKeyImporter = multiLanguageImporter;

            m_IMultiLanguageService.Import(filePath, languageNamesToImport, MultiLanguageResourceItemTypeEnum.Designer, ',', MultiLanguageImportStrategyEnum.Key, importModuleStub);

            multiLanguageImporter.VerifyAllExpectations();
        }

        #endregion

        #region Export Tests

        [Test]
        public void ExportCallsExportOnTheMultiLanguageExporter()
        {
            const string filePath = "The file path";
            var exportModuelStub = MockRepository.GenerateStub<IExportModule>();

            ResourceItemList<DesignerResourceItem, IDesignerResourceItem> resourceItemsToExport = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            List<string> languagesToExport = new List<string>() { "First language", "Second language" };

            var multiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            multiLanguageServer.Expect(srv => srv.ResourceItems).Return(resourceItemsToExport).Repeat.Any();
            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Stub(x => x.Languages).Return(languageInfos);

            var multiLanguageExporter = MockRepository.GenerateMock<IMultiLanguageExporter>();
            multiLanguageExporter.Expect(exporter => exporter.Export(Arg.Is(exportModuelStub), Arg<IEnumerable<IResourceItem>>.Is.NotNull, Arg.Is(languagesToExport), Arg.Is(','))).Repeat.Once();

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;
            m_MultiLanguageService.MultiLanguageExporter = multiLanguageExporter;

            m_IMultiLanguageService.Export(filePath, languagesToExport, MultiLanguageResourceItemTypeEnum.Designer, ',', exportModuelStub);

            multiLanguageExporter.VerifyAllExpectations();
        }

        #endregion

        #region ChangeCultureOfTranslations Tests

        [Test]
        public void ChangeCultureOfTranslationsChangesTheLanguageValuesKeyForAllResourceItemsFromTheOldLanguageNameToTheNew()
        {
            const string buttonOne = "Button1";
            const string buttonTwo = "Button2";
            const string firstLanguageOldName = "First language old name";
            const string firstLanguageNewName = "First language new name";
            const string secondLanguage = "Second Language";

            var resourceItems = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            var systemResourceItems = new ResourceItemList<ResourceItem, IResourceItem>();
            var languageInfos = new ExtendedBindingList<ILanguageInfo>();

            var multiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            multiLanguageServer.Expect(srv => srv.ResourceItems).Return(resourceItems).Repeat.Any();
            multiLanguageServer.Expect(srv => srv.SystemResourceItems).Return(systemResourceItems).Repeat.Any();
            multiLanguageServer.Stub(x => x.Languages).Return(languageInfos);

            IDesignerEventService designerEventServiceStub = MockRepository.GenerateStub<IDesignerEventService>();
            designerEventServiceStub.Stub(x => x.ActiveDesigner).Return(new MultiLanguageRootDesigner() as INeoDesignerHost);

            TestHelper.AddService(designerEventServiceStub);

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;

            IDesignerResourceItem cancelItem = new DesignerResourceItem { DesignerName = MultiLanguageServiceCFTest.DesignerName, ObjectName = buttonOne, PropertyName = "Content", ReferenceValue = buttonOne };
            cancelItem.LanguageValues.Add(firstLanguageOldName, "Avbryt");
            cancelItem.LanguageValues.Add(secondLanguage, "Cancel");
            resourceItems.Add(cancelItem);

            IDesignerResourceItem okItem = new DesignerResourceItem { DesignerName = MultiLanguageServiceCFTest.DesignerName, ObjectName = buttonTwo, PropertyName = "Content", ReferenceValue = buttonTwo };
            okItem.LanguageValues.Add(firstLanguageOldName, "OK");
            okItem.LanguageValues.Add(secondLanguage, "OK");
            resourceItems.Add(okItem);

            m_IMultiLanguageService.ChangeLanguageOfTranslations(firstLanguageOldName, firstLanguageNewName);

            Assert.IsTrue(cancelItem.LanguageValues.ContainsKey(firstLanguageNewName));
            Assert.IsFalse(cancelItem.LanguageValues.ContainsKey(firstLanguageOldName));
            Assert.IsTrue(cancelItem.LanguageValues.ContainsKey(secondLanguage));
            Assert.AreEqual("Avbryt", cancelItem.LanguageValues[firstLanguageNewName]);

            Assert.IsTrue(okItem.LanguageValues.ContainsKey(firstLanguageNewName));
            Assert.IsFalse(okItem.LanguageValues.ContainsKey(firstLanguageOldName));
            Assert.IsTrue(okItem.LanguageValues.ContainsKey(secondLanguage));
            Assert.AreEqual("OK", okItem.LanguageValues[firstLanguageNewName]);
        }

        #endregion
    }
}
