using System.Collections.Generic;
using System.IO;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MessageLibrary
{
    [TestFixture]
    public class MessageImportExportTest
    {
        private const string FieldSeparator = ",";

        private IExportModule m_ExportModule;
        private IImportModule m_ImportModule;
        private IExportModule m_MockedExportModule;
        private string m_TempFileName;
        private IMultiLanguageServiceCF m_MultiLanguageServiceCF;

     
        [TearDown]
        public void TearDown()
        {
            File.Delete(m_TempFileName);
        }

        [SetUp]
        public void SetUp()
        {

            TestHelper.ClearServices();
            TestHelper.AddServiceStub<IExportService>();
            m_MultiLanguageServiceCF = TestHelper.CreateAndAddServiceStub<IMultiLanguageServiceCF>();

            m_TempFileName = Path.GetTempFileName();
            File.SetAttributes(m_TempFileName, File.GetAttributes(m_TempFileName) | FileAttributes.Temporary);

            m_ExportModule = new TextExportModule() as IExportModule;
            m_ExportModule.Separator = ",";
            m_ExportModule.FileName = m_TempFileName;

            m_ImportModule = new TextImportModule() as IImportModule;
            m_ImportModule.Separator = ",";
            m_ImportModule.FileName = m_TempFileName;

            m_MockedExportModule = MockRepository.GenerateStub<IExportModule>();
        }

        [Test]
        public void ExportImportMessageItem()
        {
            List<MessageItemImportExportInfo> existingMessageItems = new List<MessageItemImportExportInfo>();

            // Message Item 1 to mergedMessageItems
            MessageItem messageItem = new MessageItem();
            messageItem.Message = "Test message 1";
            messageItem.StartValue = 123;
            messageItem.EndValue = 456;
            messageItem.GroupName = "Group1";
            messageItem.DisplayName = "MessageItem1";
            MessageItemImportExportInfo messageItemImportInfo1 = new MessageItemImportExportInfo(messageItem);
            existingMessageItems.Add(messageItemImportInfo1);

            m_ExportModule.Properties = MessageLibraryImportExportManager.ExportPropertyNames;
            m_ExportModule.Export(existingMessageItems);

            string importedString = m_ImportModule.ImportAsString(ImportExportHeaderTypes.ContainsPropertyNames);
            VerifyLastLine("Group1_MessageItem1,\"Test message 1\",123,456", importedString);
        }

        private static void VerifyLastLine(string expectedString, string multiLineString)
        {
            Assert.AreEqual(expectedString, GetLastLine(multiLineString));
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
    }
}
