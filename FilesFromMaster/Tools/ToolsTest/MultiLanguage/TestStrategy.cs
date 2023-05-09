using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    internal class TestStrategy : IMultiLanguageImportStrategy
    {
        public IList<string> LanguageNames;
        public IList<IResourceItem> ResourceItems;

        public void Import(IEnumerable<string> languageNames, IEnumerable<IResourceItem> itemsToImport, IEnumerable<IResourceItem> existingItems)
        {
            LanguageNames = languageNames.ToList();
            ResourceItems = itemsToImport.ToList();
        }

        #region IMultiLanguageImportStrategy Members

        void IMultiLanguageImportStrategy.Import(IEnumerable<string> languageNames, IEnumerable<IResourceItem> itemsToImport, IEnumerable<IResourceItem> existingItems)
        {
            Import(languageNames,  itemsToImport, existingItems);
        }

        ResourceItemCsvOptions IMultiLanguageImportStrategy.Options
        {
            get { return ResourceItemCsvOptions.All; }
        }

        #endregion
    }
}