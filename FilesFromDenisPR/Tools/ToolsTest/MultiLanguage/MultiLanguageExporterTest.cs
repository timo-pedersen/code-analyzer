#if !VNEXT_TARGET
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ImportExport;
using Neo.ApplicationFramework.Tools.MultiLanguage.TextID;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageExporterTest
    {
        private const string FieldSeparator = ",";
        private const string ResourceItemKey = "Screen1.Button1.Text";

        private const string Culture1 = "en-GB";
        private const string Culture1Text = "Click";
        private const string Culture2 = "sv-SE";
        private const string Culture2Text = "Klick";

        private IMultiLanguageExporter m_MultiLanguageExporter;
        private IMultiLanguageExporter m_TextIDExporter;
        private IExportModule m_ExportModule;
        private IImportModule m_ImportModule;
        private IExportModule m_MockedExportModule;
        private string m_TempFileName;

      
        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            File.Delete(m_TempFileName);
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            TestHelper.AddServiceStub<IMultiLanguageServiceCF>();

            m_TempFileName = Path.GetTempFileName();
            File.SetAttributes(m_TempFileName, File.GetAttributes(m_TempFileName) | FileAttributes.Temporary);

            m_ExportModule = new TextExportModule() as IExportModule;
            m_ExportModule.Separator = ",";
            m_ExportModule.FileName = m_TempFileName;

            m_ImportModule = new TextImportModule() as IImportModule;
            m_ImportModule.Separator = ",";
            m_ImportModule.FileName = m_TempFileName;

            m_MockedExportModule = Substitute.For<IExportModule>();
            m_MultiLanguageExporter = new MultiLanguageExporter<DesignerResourceItem>() as IMultiLanguageExporter;
            m_TextIDExporter = new MultiLanguageExporter<TextIDResourceItem>() as IMultiLanguageExporter;

        }

        [Test]
        public void Exporting_a_resource_item_where_one_language_is_missing_exports_with_empty_string()
        {
            const string emptyCulture = "en-US";

            IDesignerResourceItem resourceItem = new DesignerResourceItem(ResourceItemKey);
            resourceItem.ReferenceValue = Culture1Text;
            resourceItem.LanguageValues[Culture2] = Culture2Text;

            var resourceItems = new List<IDesignerResourceItem> { resourceItem };
            var languages = new List<string> { emptyCulture, Culture2 };

            m_MultiLanguageExporter.Export(m_ExportModule, resourceItems.Cast<IResourceItem>(), languages, ',');
            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);

            string expectedString = string.Format("{1}{0}{2}{0}{3}{0}{4}",
                FieldSeparator, ResourceItemKey, Culture1Text, string.Empty, Culture2Text);

            VerifyLastLine(expectedString, importedString);
        }

        private static void VerifyLastLine(string expectedString, string multiLineString)
        {
            Assert.AreEqual(expectedString, GetLastLine(multiLineString));
        }

        private static void VerifyLine(string expectedString, string multiLineString, int lineNumber)
        {
            Assert.AreEqual(expectedString, GetLine(multiLineString, lineNumber));
        }

        private static string GetLine(string multiLineString, int lineNumber)
        {
            string lineToReturn = null;
            using (StringReader stringReader = new StringReader(multiLineString))
            {
                int lineCounter = 1;
                string line = stringReader.ReadLine();
                while (line != null)
                {
                    lineToReturn = line;
                    if (lineCounter++ == lineNumber)
                        break;

                    line = stringReader.ReadLine();
                }
            }

            return lineToReturn;
        }

        private static string GetFirstLine(string multiLineString)
        {
            string lineToReturn = null;
            using (StringReader stringReader = new StringReader(multiLineString))
            {
                lineToReturn = stringReader.ReadLine();
            }

            return lineToReturn;
        }

        private static void VerifyFirstLine(string expectedString, string multiLineString)
        {
            Assert.AreEqual(expectedString, GetFirstLine(multiLineString));
        }

        private static string GetLastLine(string multiLineString)
        {
            string lineToReturn = null;
            using (StringReader stringReader = new StringReader(multiLineString))
            {
                string line = stringReader.ReadLine();
                while (line != null)
                {
                    lineToReturn = line;
                    line = stringReader.ReadLine();
                }
            }

            return lineToReturn;
        }

        [Test]
        public void ExportWritesBasicLine()
        {
            IList<IResourceItem> resourceItems = CreateResourceItem();

            m_MultiLanguageExporter.Export(m_ExportModule, resourceItems, new List<string> { Culture1 }, ',');
            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyLastLine("Screen1.Button1.Text,ClickRef," + Culture1Text, importedString);
        }

        [Test]
        public void ExportWritesHeaderLine()
        {
            IList<IResourceItem> resourceItems = CreateResourceItem();

            m_MultiLanguageExporter.Export(m_ExportModule, resourceItems, new List<string> { Culture1 }, ',');
            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyFirstLine("Key,ReferenceText," + Culture1, importedString);
        }

        [Test]
        public void ExportWritesSeveralLanguages()
        {
            IList<IResourceItem> resourceItems = CreateResourceItem();
            resourceItems[0].LanguageValues.Add(Culture2, Culture2Text);

            //The order of the culture list is the order of the output'd languages
            m_MultiLanguageExporter.Export(m_ExportModule, resourceItems.Cast<IResourceItem>(), new List<string> { Culture1, Culture2 }, ',');
            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyFirstLine("Key,ReferenceText," + Culture1 + FieldSeparator + Culture2, importedString);
            VerifyLastLine("Screen1.Button1.Text,ClickRef," + Culture1Text + FieldSeparator + Culture2Text, importedString);
        }

        [Test]
        public void ExportWritesSeveralLines()
        {
            IList<IResourceItem> items = CreateResourceItem();
            IResourceItem item = new DesignerResourceItem
                                    {
                                        DesignerName = "Screen1",
                                        ObjectName = "Button1",
                                        PropertyName = "Text",
                                        ReferenceValue = "AAA"
                                    };
            item.LanguageValues.Add(Culture1, "BBB");
            items.Add(item);

            m_MultiLanguageExporter.Export(m_ExportModule, items.Cast<IResourceItem>(), new List<string> { Culture1 }, ',');

            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyLine("Screen1.Button1.Text,ClickRef," + Culture1Text, importedString, 2);
            VerifyLine("Screen1.Button1.Text,AAA,BBB", importedString, 3);
        }

        [Test]
        public void ExportQuotesTextWithSpaces()
        {
            TestFormatting("en knapp", "\"en knapp\"");
        }

        [Test]
        public void ExportQuotesTextWithComma()
        {
            TestFormatting("ett,två", "\"ett,två\"");
        }

        [Test]
        public void ExportQuotesTextWithNewline()
        {
            TestFormatting("ett\ntvå", "\"ett\ntvå\"");
        }

        [Test]
        public void ExportQuotesTextWithCarriageReturn()
        {
            TestFormatting("en\rknapp", "\"en\rknapp\"");
        }

        private void TestFormatting(string value, string expectedFormattedValue)
        {
            m_MultiLanguageExporter.Export(m_ExportModule, CreateResourceItem(value).Cast<IResourceItem>(), new List<string> { Culture1 }, ',');

            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            IList<Dictionary<string, string>> importedData = m_ImportModule.Import(ImportExportHeaderTypes.ContainsPropertyNames);
            Dictionary<string, string> dictionary = importedData.Last();

            Assert.AreEqual(value, dictionary[Culture1]);
        }

        private IList<IResourceItem> CreateResourceItem()
        {
            return CreateResourceItem(Culture1Text);
        }

        private IList<IResourceItem> CreateResourceItem(string cultureText)
        {
            IResourceItem resourceItem = new DesignerResourceItem
            {
                DesignerName = "Screen1",
                ObjectName = "Button1",
                PropertyName = "Text",
                ReferenceValue = "ClickRef"
            };
            resourceItem.LanguageValues.Add(Culture1, cultureText);
            return new List<IResourceItem>
                       {
                           resourceItem
                       };
        }

        private IList<IResourceItem> CreateTextIDResourceItem(string cultureText)
        {
            IResourceItem resourceItem = new TextIDResourceItem(27,"ClickRef");

            resourceItem.LanguageValues.Add(Culture1, cultureText);
            return new List<IResourceItem>
                       {
                           resourceItem
                       };
        }


        [Test]
        public void TextIDExportWritesSeveralLanguages()
        {
            IList<IResourceItem> resourceItems = CreateTextIDResourceItem(Culture1Text);
            resourceItems[0].LanguageValues.Add(Culture2, Culture2Text);

            //The order of the culture list is the order of the output'd languages
            m_TextIDExporter.Export(m_ExportModule, resourceItems.Cast<IResourceItem>(), new List<string> { Culture1, Culture2 }, ',');
            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyFirstLine("Key,ReferenceText," + Culture1 + FieldSeparator + Culture2, importedString);
            VerifyLastLine("27,ClickRef," + Culture1Text + FieldSeparator + Culture2Text, importedString);
        }


    }

}
#endif
