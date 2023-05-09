using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageImportKeyStrategyTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            TestHelper.AddServiceStub<IMultiLanguageServiceCF>();
        }

        #region ImportTranslations Tests

        [Test]
        public void ImportTranslations_does_not_import_a_record_if_a_ResourceItem_with_the_same_key_cannot_be_found()
        {
            List<IDesignerResourceItem> resourceItemsToImport = new List<IDesignerResourceItem>() { new DesignerResourceItem("Designer.Object.Property") };

            var resourceItems = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();

            var multiLanguageService = MockRepository.GenerateStub<IMultiLanguageServiceIde>();

            var strategy = new MultiLanguageImportKeyStrategy(multiLanguageService);


            strategy.ImportTranslations(resourceItemsToImport.Cast<IResourceItem>(), resourceItems.Cast<IResourceItem>());

            Assert.AreEqual(0, resourceItems.Count);
        }

        [Test]
        public void ImportTranslations_updates_the_ResourceItem_with_the_same_key()
        {
            const string key = "Designer.Object.Property";
            const string language = "Language";

            IDesignerResourceItem resourceItemToImport = new DesignerResourceItem(key);
            resourceItemToImport.ReferenceValue = "Updated";
            resourceItemToImport.LanguageValues[language] = language;
            List<IDesignerResourceItem> resourceItemsToImport = new List<IDesignerResourceItem>() { resourceItemToImport };

            var resourceItem = new DesignerResourceItem(key);
            resourceItem.ReferenceValue = "Original";
            resourceItemToImport.LanguageValues[language] = string.Empty;

            var resourceItems = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>() { resourceItem };
            
            var multiLanguageService = MockRepository.GenerateStub<IMultiLanguageServiceIde>();
            
            var strategy = new MultiLanguageImportKeyStrategy(multiLanguageService);


            strategy.ImportTranslations(resourceItemsToImport.Cast<IResourceItem>(), resourceItems.Cast<IResourceItem>());

            Assert.AreEqual(1, resourceItems.Count);

            Assert.AreEqual(resourceItemsToImport[0].ReferenceValue, resourceItems[0].ReferenceValue);
            Assert.AreEqual(resourceItemsToImport[0].LanguageValues[language], resourceItems[0].LanguageValues[language]);
        }

        #endregion

    }
}
