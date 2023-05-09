#if !VNEXT_TARGET
using System.Collections.Generic;
using System.IO;
using Neo.ApplicationFramework.Common.Collections;
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
    public class MultiLanguageImporterTest
    {
        private string m_FileName;
        private IMultiLanguageServiceIde m_MultiLanguageServiceIdeStub;
        private IMultiLanguageServiceCF m_MultiLanguageServiceCFStub;
        private IInformationProgressService m_InformationProgressServiceStub;
        private IMultiLanguageImporter m_TextIDMultiLanguageImporter;
        private TextIDImportStrategy m_TextIDimportStrategy;

        [SetUp]
        public void TestSetUp()
        {
            TestHelper.ClearServices();
            m_MultiLanguageServiceCFStub = TestHelper.AddServiceStub<IMultiLanguageServiceCF>();
            m_FileName = Path.GetTempFileName();
            File.SetAttributes(m_FileName, File.GetAttributes(m_FileName) | FileAttributes.Temporary);
            m_MultiLanguageServiceIdeStub = TestHelper.AddServiceStub<IMultiLanguageServiceIde>();
            m_InformationProgressServiceStub = TestHelper.AddServiceStub<IInformationProgressService>();
        }

        [TearDown]
        public void TestTearDown()
        {
            File.Delete(m_FileName);
        }

        [Test]
        public void ImportReadsKey()
        {
            var importStrategy = new TestStrategy();
            IMultiLanguageImporter multiLanguageImporter = new MultiLanguageImporter<DesignerResourceItem>(importStrategy);
            File.WriteAllLines(m_FileName, new string[] { "Key,ReferenceText,se-SE", "Screen1.Button1.Text,Button,Knapp" });
            IImportModule importModule = new TextImportModule();
            importModule.FileName = m_FileName;

            multiLanguageImporter.Import(importModule, new List<string>(), new ResourceItemList<IResourceItem, IResourceItem>(), ',');
            IDesignerResourceItem designerResourceItem = importStrategy.ResourceItems[0] as IDesignerResourceItem;
            Assert.AreEqual(1, importStrategy.ResourceItems.Count);
            Assert.AreEqual("Screen1", designerResourceItem.DesignerName);
            Assert.AreEqual("Button1", designerResourceItem.ObjectName);
            Assert.AreEqual("Text", designerResourceItem.PropertyName);
        }

        [Test]
        public void ImportReadsOnlySelectedLanguages()
        {
            var importStrategy = new TestStrategy();
            IMultiLanguageImporter multiLanguageImporter = new MultiLanguageImporter<DesignerResourceItem>(importStrategy);
            File.WriteAllLines(m_FileName, new string[] { "Key,ReferenceText,se-SE,en-GB", "Screen1.Button1.Text,Button,Knapp,GbButton" });
            IImportModule importModule = new TextImportModule();
            importModule.FileName = m_FileName;

            var languagesToImport = new List<string> { "se-SE" };
            multiLanguageImporter.Import(importModule, languagesToImport, new List<IResourceItem>(), ',');
            IDesignerResourceItem designerResourceItem = importStrategy.ResourceItems[0] as IDesignerResourceItem;
            Assert.AreEqual(1, importStrategy.ResourceItems.Count);
            Assert.AreEqual("Button", designerResourceItem.ReferenceValue);
            Assert.AreEqual(1, designerResourceItem.LanguageValues.Count);
            Assert.AreEqual("Knapp", designerResourceItem.LanguageValues["se-SE"]);
        }

        private void TextIDTestSetup()
        {
            IExtendedBindingList<ILanguageInfo> bindingList = new ExtendedBindingList<ILanguageInfo>();
            m_MultiLanguageServiceIdeStub.Languages.Returns(bindingList);
            m_MultiLanguageServiceCFStub.CreateLanguageList().Returns(bindingList);

            MultiLanguageServer multiLanguageServer = new MultiLanguageServer();
            m_MultiLanguageServiceIdeStub.MultiLanguageServer = multiLanguageServer;

            m_TextIDimportStrategy = new TextIDImportStrategy(m_MultiLanguageServiceIdeStub);

            m_TextIDMultiLanguageImporter = new MultiLanguageImporter<TextIDResourceItem>(m_TextIDimportStrategy);
        }

        [Test]
        public void TextIDImportUpdatesAndAddsAllValues()
        {
            TextIDTestSetup();
            m_MultiLanguageServiceIdeStub.MultiLanguageServer.Languages.Add(new LanguageInfo("German (Germany)", "US"));
            m_MultiLanguageServiceIdeStub.MultiLanguageServer.Languages.Add(new LanguageInfo("French (France)", "US"));

            //data before import 
            ITextIDResourceItem oldTextIDResourceItem = new TextIDResourceItem(27,"OldRef");
            m_MultiLanguageServiceIdeStub.MultiLanguageServer.TextIDResourceItems.Add(oldTextIDResourceItem);


            File.WriteAllLines(m_FileName, new string[] { "Key,ReferenceText,German (Germany),French (France)", 
                                                            "27,Button,Knopfe,Fr-Button",
                                                            "127,Stop,Halt,Fr-Stop"});
            IImportModule importModule = new TextImportModule();
            importModule.FileName = m_FileName;

            var languagesToImport = new List<string> { "German (Germany)", "French (France)" };
            m_TextIDMultiLanguageImporter.Import(importModule, languagesToImport, m_MultiLanguageServiceIdeStub.MultiLanguageServer.TextIDResourceItems, ',');
            IResourceItemList<ITextIDResourceItem> textIDList = m_MultiLanguageServiceIdeStub.MultiLanguageServer.TextIDResourceItems;
            IList<ITextIDResourceItem> textIDListCopy = new List<ITextIDResourceItem>(); ;
            foreach (ITextIDResourceItem textIDResourceItem in textIDList)
            {
                textIDListCopy.Add(textIDResourceItem);
            }
            Assert.AreEqual("Button", textIDListCopy[0].ReferenceValue);
            Assert.AreEqual("27", textIDListCopy[0].Key);
            Assert.AreEqual("Knopfe", textIDListCopy[0].LanguageValues["German (Germany)"]);
            Assert.AreEqual("Fr-Button", textIDListCopy[0].LanguageValues["French (France)"]);
            Assert.AreEqual("Stop", textIDListCopy[1].ReferenceValue);
            Assert.AreEqual("127", textIDListCopy[1].Key);
            Assert.AreEqual("Halt", textIDListCopy[1].LanguageValues["German (Germany)"]);
            Assert.AreEqual("Fr-Stop", textIDListCopy[1].LanguageValues["French (France)"]);
        }

        [Test]
        public void TextIDImportDoesNotCreateLanguageIfNotExisting()
        {
            TextIDTestSetup();
            m_MultiLanguageServiceIdeStub.MultiLanguageServer.Languages.Add(new LanguageInfo("German (Germany)", "US"));

            File.WriteAllLines(m_FileName, new string[] { "Key,ReferenceText,German (Germany),French (France)", "27,Button,Knopfe,Fr-Button" });
            IImportModule importModule = new TextImportModule();
            importModule.FileName = m_FileName;

            var languagesToImport = new List<string> { "German (Germany)", "French (France)" };
            m_TextIDMultiLanguageImporter.Import(importModule, languagesToImport, new List<IResourceItem>(), ',');

            m_MultiLanguageServiceIdeStub.DidNotReceive().AddLanguage(Arg.Any<string>());

            Assert.AreEqual(1, ((ICollection<ILanguageInfo>)m_MultiLanguageServiceIdeStub.MultiLanguageServer.Languages).Count);
        }
    }
}
#endif
