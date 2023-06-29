#if !VNEXT_TARGET
using System.Collections.Generic;
using System.ComponentModel.Design;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.MultiLanguage.TextID;
using NSubstitute;
using NUnit.Framework;

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
            projectManager.IsProjectLoading.Returns(true);

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
            IMultiLanguageServer multiLanguageServer = Substitute.For<IMultiLanguageServer>();
            languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Languages.Returns(languageInfos);
            IResourceItemList<IDesignerResourceItem> resourceItems = new ResourceItemList<DesignerResourceItem,IDesignerResourceItem>();
            multiLanguageServer.ResourceItems.Returns(resourceItems);

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

            multiLanguageServer = Substitute.For<IMultiLanguageServer>();
            multiLanguageServer.ResourceItems.Returns(resourceItems);
            multiLanguageServer.TextIDResourceItems.Returns(textIDResourceItems);

            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Languages.Returns(languageInfos);

            var multiLanguageSerializer = Substitute.For<MultiLanguageSerializer>();
            m_MultiLanguageService.MultiLanguageSerializer = multiLanguageSerializer;

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;

            m_IMultiLanguageService.Save(filename);

            multiLanguageSerializer.Received().Save<DesignerResourceItem>(Arg.Is(filename), Arg.Is<IEnumerable<IResourceItem>>(x => x != null));
        }

        #endregion

        #region Import Tests

        [Test]
        public void ImportCallsGetLanguagesAndImportOnTheMultiLanguageImporter()
        {
            const string filePath = "The file path";
            var importModuleStub = Substitute.For<IImportModule>();

            var languageNamesToImport = new List<string>() { "The language name" };

            var multiLanguageImporter = Substitute.For<MultiLanguageImporter<DesignerResourceItem>>((IMultiLanguageImportStrategy)null);
            ((IMultiLanguageImporter)multiLanguageImporter).Import(Arg.Is(importModuleStub), Arg.Is(languageNamesToImport), 
                Arg.Is<IEnumerable<IResourceItem>>(x => x != null), Arg.Is(','));

            IResourceItemList<IDesignerResourceItem> resourceList = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            IMultiLanguageServer multiLanguageServer = Substitute.For<IMultiLanguageServer>();
            multiLanguageServer.ResourceItems.Returns(resourceList);
            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Languages.Returns(languageInfos);

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;
            m_MultiLanguageService.MultiLanguageKeyImporter = multiLanguageImporter;

            m_IMultiLanguageService.Import(filePath, languageNamesToImport, MultiLanguageResourceItemTypeEnum.Designer, ',', MultiLanguageImportStrategyEnum.Key, importModuleStub);

            ((IMultiLanguageImporter)multiLanguageImporter).Received(1).Import(Arg.Is(importModuleStub), Arg.Is(languageNamesToImport),
                Arg.Is<IEnumerable<IResourceItem>>(x => x != null), Arg.Is(','));
        }

        #endregion

        #region Export Tests

        [Test]
        public void ExportCallsExportOnTheMultiLanguageExporter()
        {
            const string filePath = "The file path";
            var exportModuelStub = Substitute.For<IExportModule>();

            ResourceItemList<DesignerResourceItem, IDesignerResourceItem> resourceItemsToExport = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            List<string> languagesToExport = new List<string>() { "First language", "Second language" };

            var multiLanguageServer = Substitute.For<IMultiLanguageServer>();
            multiLanguageServer.ResourceItems.Returns(resourceItemsToExport);
            IExtendedBindingList<ILanguageInfo> languageInfos = new ExtendedBindingList<ILanguageInfo>();
            multiLanguageServer.Languages.Returns(languageInfos);

            var multiLanguageExporter = Substitute.For<IMultiLanguageExporter>();
            multiLanguageExporter.Export(Arg.Is(exportModuelStub), Arg.Is<IEnumerable<IResourceItem>>(x => x != null), 
                Arg.Is(languagesToExport), Arg.Is(','));

            m_IMultiLanguageService.MultiLanguageServer = multiLanguageServer;
            m_MultiLanguageService.MultiLanguageExporter = multiLanguageExporter;

            m_IMultiLanguageService.Export(filePath, languagesToExport, MultiLanguageResourceItemTypeEnum.Designer, ',', exportModuelStub);

            multiLanguageExporter.Received(1).Export(Arg.Is(exportModuelStub), Arg.Is<IEnumerable<IResourceItem>>(x => x != null),
                Arg.Is(languagesToExport), Arg.Is(','));
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

            var multiLanguageServer = Substitute.For<IMultiLanguageServer>();
            multiLanguageServer.ResourceItems.Returns(resourceItems);
            multiLanguageServer.SystemResourceItems.Returns(systemResourceItems);
            multiLanguageServer.Languages.Returns(languageInfos);

            IDesignerEventService designerEventServiceStub = Substitute.For<IDesignerEventService>();
            designerEventServiceStub.ActiveDesigner.Returns(new MultiLanguageRootDesigner() as INeoDesignerHost);

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
#endif
