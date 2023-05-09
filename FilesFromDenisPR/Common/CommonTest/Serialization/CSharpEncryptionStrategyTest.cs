using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Serialization.Encryption;
using Neo.ApplicationFramework.Common.Utilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Common.Serialization
{
    [TestFixture]
    public class CSharpEncryptionStrategyTest
    {
        private class UnclosableMemoryStream : MemoryStream
        {
            public override void Close() { }
        }

        private TestCSharpEncryptionStrategy m_Encryptor;
        private FileHelper File { get; set; }
        private DirectoryHelper Directory { get; set; }
        private Dictionary<string, string> m_FileData;
        private Dictionary<string, UnclosableMemoryStream> m_SavedFiles;
        private const string SamplePath = "";
        private const string SampleFilename = "Sample.script.cs";
        private const string SampleFilename2 = "Sample2.script.cs";
        private const string SampleContents = "A quick brown fox jumped over the lazy dog";

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            m_FileData = new Dictionary<string, string>();
            m_SavedFiles = new Dictionary<string, UnclosableMemoryStream>();
        }

        [SetUp]
        public void Setup() 
        {
            File = Substitute.For<FileHelper>();
            Directory = Substitute.For<DirectoryHelper>();
            m_Encryptor = new TestCSharpEncryptionStrategy(File, Directory, FileStreamProvider);

            File.When(x => x.WriteAllText(Arg.Any<string>(), Arg.Any<string>()))
                .Do(x => { m_FileData.Add((string)x[0], (string)x[1]); });

            File.ReadAllText(Arg.Any<string>()).Returns(x =>
            {
                try 
                { 
                    return m_FileData[(string)x[0]];
                } 
                catch 
                { 
                    throw new FileNotFoundException();
                }
            });
            
            File.Delete(Arg.Do<string>(DeleteSavedFile));

            File.Exists(Arg.Any<string>()).Returns(x =>
            {
                var file = (string)x[0];
                return m_SavedFiles.ContainsKey(file) || m_FileData.ContainsKey(file);
            });

            Directory.GetFiles(Arg.Any<string>(), Arg.Any<string>()).Returns(x => { return GetFiles((string)x[0], (string)x[1]); });
        }

        [TearDown]
        public void Teardown()
        {
            m_FileData.Clear();
            foreach (var kvp in m_SavedFiles) { kvp.Value.Dispose(); }
            m_SavedFiles.Clear();
        }

        [Test]
        public void TestSaveScriptFile()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents);

            Assert.AreEqual(m_SavedFiles.Count, 1);
            Assert.IsTrue(m_SavedFiles.ContainsKey(ToEncryptedFilename(SampleFilename)));
        }

        [Test]
        public void TestSaveScriptFileWithEncoding()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents, Encoding.Unicode);

            Assert.AreEqual(m_SavedFiles.Count, 1);
            Assert.IsTrue(m_SavedFiles.ContainsKey(ToEncryptedFilename(SampleFilename)));
        }
        
        [Test]
        public void TestDecryptFileData()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents);
            Assert.AreEqual(m_Encryptor.DoDecryptFileData(ToEncryptedFilename(SampleFilename)), SampleContents);
        }

        [Test]
        public void TestDecryptFile()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents);
            m_Encryptor.DoDecryptFile(ToEncryptedFilename(SampleFilename));
            File.Received().WriteAllText(Arg.Is(SampleFilename), Arg.Is(SampleContents));
        }

        [Test]
        public void TestLoadScriptFile()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents, Encoding.Unicode);
            string loadedFile = m_Encryptor.LoadScriptFile(SampleFilename);
            Assert.AreEqual(loadedFile, SampleContents);
        }

        [Test]
        public void TestLoadScriptFileWithUnencryptedFallback()
        {
            File.WriteAllText(SampleFilename, SampleContents);
            string resultText = m_Encryptor.LoadScriptFile(SampleFilename);
            Assert.AreEqual(resultText, SampleContents);
        }

        [Test]
        public void TestLoadScriptFileFailsToLoadReturnsEmpty()
        {
            string resultText = m_Encryptor.LoadScriptFile(SampleFilename);
            Assert.AreEqual(string.Empty, resultText);
        }

        [Test]
        public void TestDoesMissingScriptExist()
        {
            Assert.IsFalse(m_Encryptor.DoesScriptExist(SampleFilename));
        }

        [Test]
        public void TestDoesExistingScriptExist()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents, Encoding.Unicode);
            Assert.IsTrue(m_Encryptor.DoesScriptExist(SampleFilename));
        }

        [Test]
        public void TestDoesScriptExistWithUnencryptedFallback()
        {
            Assert.IsFalse(m_Encryptor.DoesScriptExist(SampleFilename));
            File.WriteAllText(SampleFilename, SampleContents);
            Assert.IsTrue(m_Encryptor.DoesScriptExist(SampleFilename));
        }
        
        [Test]
        public void TestDecryptFilesForBuild()
        {
            m_Encryptor.SaveScriptFile(SampleFilename, SampleContents);
            m_Encryptor.SaveScriptFile(SampleFilename2, SampleContents);
            Assert.AreEqual(m_SavedFiles.Count, 2);
            Assert.AreEqual(m_FileData.Count, 0);

            m_Encryptor.DecryptFilesForBuild(SamplePath);
            Assert.AreEqual(m_SavedFiles.Count, 2);
            Assert.AreEqual(m_FileData.Count, 2);
            Assert.IsTrue(m_FileData.ContainsKey(SampleFilename));
            Assert.IsTrue(m_FileData.ContainsKey(SampleFilename2));            
        }

        [Test]
        public void TestDeleteUnencryptedBuildFiles()
        {
            TestDecryptFilesForBuild();
            m_Encryptor.DeleteUnencryptedBuildFiles();
            Assert.AreEqual(m_SavedFiles.Count, 2);
            Assert.AreEqual(m_FileData.Count, 0);
        }

        private string ToEncryptedFilename(string filename)
        {
            return filename + ApplicationConstantsCF.EncryptedScriptFileExtension;
        }

        private Stream FileStreamProvider(string fileName, FileMode mode, FileAccess access)
        {
            UnclosableMemoryStream returnStream = null;
            if (!m_SavedFiles.TryGetValue(fileName, out returnStream))
            {
                returnStream = new UnclosableMemoryStream();
                m_SavedFiles.Add(fileName, returnStream);
            }
            if (mode == FileMode.Create) { returnStream.SetLength(0); }
            if (mode == FileMode.Append)
            {
                returnStream.Position = returnStream.Length;
            }
            else
            {
                returnStream.Position = 0;
            }
            return returnStream;
        }

        private void DeleteSavedFile(string filename)
        {
            if (m_FileData.ContainsKey(filename)) { m_FileData.Remove(filename); }
            if (m_SavedFiles.ContainsKey(filename)) { m_SavedFiles.Remove(filename); }
        }

        private string[] GetFiles(string path, string pattern)
        {
            if (path != SamplePath) { return new string[0]; }
            if (!pattern.EndsWith(ApplicationConstantsCF.EncryptedScriptFileExtension)) { return new string[0]; }

            return m_SavedFiles.Keys.ToArray();
        }

        private class TestCSharpEncryptionStrategy : CSharpEncryptionStrategy
        {
            public TestCSharpEncryptionStrategy(FileHelper file, DirectoryHelper directory, Func<string, FileMode, FileAccess, Stream> fileStreamProvider)
                : base(file, directory, fileStreamProvider)
            {
            }

            public string DoDecryptFileData(string encryptedFilename)
            {
                return DecryptFileData(encryptedFilename);
            }

            public void DoDecryptFile(string encryptedFilename)
            {
                DecryptFile(encryptedFilename);
            }

        }
    }
}
