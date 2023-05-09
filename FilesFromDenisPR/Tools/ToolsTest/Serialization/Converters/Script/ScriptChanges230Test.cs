using System;
using System.Collections.Generic;
using Core.Api.ProjectTarget;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Serialization.Encryption;
using Neo.ApplicationFramework.Controls.Dialogs.ScriptChanges;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Script.ScriptSamples;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters.Script
{
    [TestFixture]
    public class ScriptChanges230Test
    {
        private const string DummyTestFolder = ".";
        private IXmlConverter m_Converter;

        private Dictionary<string, string> m_SavedFiles;

        private class TestableScriptConverter230 : ScriptConverter230
        {
            public TestableScriptConverter230(
                ILazy<IMessageBoxServiceIde> messageBoxService,
                ILazy<IEncryptionStrategyFactory> encryptionStrategy)
                : base(messageBoxService, encryptionStrategy)
            {
            }

            protected override bool ShowDialogWithScriptChanges(IEnumerable<ScriptConversionInfo> readableListOfScriptChanges)
            {
                return true;
            }
        }

        [SetUp]
        public void SetUp()
        {
            m_SavedFiles = new Dictionary<string, string>();
        }

        [Test]
        public void ConverterReturnsTrueIfExpressionIsFound()
        {
            // ARRANGE
            LoadTestFiles(new Dictionary<string, string>
            { { "Changes230WithChanges_script.cs", ScriptFiles.Changes230WithChanges_script },
                                                             { "Changes230NoChanges_script.cs", ScriptFiles.Changes230NoChanges_script },
                                                             { "NoChangesScreen.script.cs", ScriptFiles.NoChangesScreen_script },
                                                             { "Screen1.script.cs", ScriptFiles.Screen1_script },
                                                             { "Screen2.script.cs", ScriptFiles.Screen2_script },
                                                             { "SimpleScreen1.script.cs", ScriptFiles.SimpleScreen1InnerUsing_script } });
            // ACT
            bool result = m_Converter.ConvertTerminal(DummyTestFolder);

            // ASSERT
            Assert.IsTrue(result);
        }

        [Test]
        public void ConverterReturnsFalseIfExpressionNotFound()
        {
            // ARRANGE
            LoadTestFiles(new Dictionary<string, string> { { "Changes230NoChanges_script.cs", ScriptFiles.Changes230NoChanges_script },
                                                             { "NoChangesScreen.script.cs", ScriptFiles.NoChangesScreen_script },
                                                             { "Screen1.script.cs", ScriptFiles.Screen1_script },
                                                             { "Screen2.script.cs", ScriptFiles.Screen2_script },
                                                             { "SimpleScreen1.script.cs", ScriptFiles.SimpleScreen1InnerUsing_script } });
            // ACT
            bool result = m_Converter.ConvertTerminal(DummyTestFolder);

            // ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void ConverterUpdatesTheFileWithTheExpression()
        {
            // ARRANGE
            LoadTestFiles(new Dictionary<string, string> { { "Changes230WithChanges_script.cs", ScriptFiles.Changes230WithChanges_script },
                                                             { "Changes230NoChanges_script.cs", ScriptFiles.Changes230NoChanges_script },
                                                             { "NoChangesScreen.script.cs", ScriptFiles.NoChangesScreen_script },
                                                             { "Screen1.script.cs", ScriptFiles.Screen1_script },
                                                             { "Screen2.script.cs", ScriptFiles.Screen2_script },
                                                             { "SimpleScreen1.script.cs", ScriptFiles.SimpleScreen1InnerUsing_script } });
            // ACT
            m_Converter.ConvertTerminal(DummyTestFolder);

            // ASSERT
            Assert.AreEqual(1, m_SavedFiles.Count);
            Assert.AreEqual(ScriptFiles.Changes230WithChangesValidate_script, m_SavedFiles["Changes230WithChanges_script.cs"]);
        }

        [Test]
        public void ConverterDoesNotUpdateFilesWithoutTheExpression()
        {
            // ARRANGE
            LoadTestFiles(new Dictionary<string, string> { { "Changes230NoChanges_script.cs", ScriptFiles.Changes230NoChanges_script },
                                                             { "NoChangesScreen.script.cs", ScriptFiles.NoChangesScreen_script },
                                                             { "Screen1.script.cs", ScriptFiles.Screen1_script },
                                                             { "Screen2.script.cs", ScriptFiles.Screen2_script },
                                                             { "SimpleScreen1.script.cs", ScriptFiles.SimpleScreen1InnerUsing_script } });

            // ACT
            m_Converter.ConvertTerminal(DummyTestFolder);

            // ASSERT
            Assert.AreEqual(0, m_SavedFiles.Count);
        }

        private void LoadTestFiles(Dictionary<string, string> files)
        {
            var targetService = Substitute.For<ITargetService>().ToILazy();
            targetService.Value.CurrentTargetInfo.ProjectPath.Returns(DummyTestFolder);

            var messageBoxService = Substitute.For<IMessageBoxServiceIde>().ToILazy();

            var encryptionStrategyFactory = Substitute.For<IEncryptionStrategyFactory>().ToILazy();
            var encryptionStrategy = Substitute.For<ICSharpSerializationEncryptionStrategy>();
            encryptionStrategy.GetScriptsFromDirectory(DummyTestFolder).Returns(files.Keys);
            foreach (KeyValuePair<string, string> file in files)
            {
                string filename = file.Key;
                encryptionStrategy.LoadScriptFile(filename).Returns(file.Value);
            }
            encryptionStrategy
                .WhenForAnyArgs(x => x.SaveScriptFile(Arg.Any<string>(), Arg.Any<string>()))
                .Do(a => m_SavedFiles.Add((string)a[0], (string)a[1]));
            encryptionStrategyFactory.Value.GetEncryptionStrategy<ICSharpSerializationEncryptionStrategy>(true).Returns(encryptionStrategy);

            m_Converter = new TestableScriptConverter230(messageBoxService, encryptionStrategyFactory);
        }
    }
}