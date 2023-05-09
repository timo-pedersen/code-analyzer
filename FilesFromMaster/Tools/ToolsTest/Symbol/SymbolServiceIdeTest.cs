using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Common.Graphics;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using Neo.ApplicationFramework.Controls.Dialogs;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Build.BuildManager;
using Neo.ApplicationFramework.Tools.CrossReference;
using Neo.ApplicationFramework.Tools.Symbol.Service;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Symbol
{
    [TestFixture]
    public class SymbolServiceIdeTest
    {
        private TestableSymbolServiceIde m_SymbolServiceIde;
        private ISymbolServiceIde m_ISymbolServiceIde;
        private FileHelper m_FileHelperMock;
        private DirectoryHelper m_DirectoryHelperStub;
        private BitmapHelper m_BitmapHelperStub;
        private OpenFileDialogEx m_OpenFileDialogStub;
        private IProjectManager m_ProjectManagerStub;
        private IProject m_ProjectStub;
        private ITargetService m_TargetService;

        private const string ProjectFolderPath = @"x:\MyProject\";
        private const string SymbolSourceDirectory = @"x:\MySymbols\";
        private const string SymbolName = "MySymbol";
        private const string SymbolFileExtension = ".png";

        [SetUp]
        public void SetUp()
        {
            m_OpenFileDialogStub = MockRepository.GenerateStub<OpenFileDialogEx>();

            m_FileHelperMock = MockRepository.GenerateStub<FileHelper>();
            m_FileHelperMock.Stub(x => x.Exists(null))
                .IgnoreArguments()
                .Return(true);

            m_DirectoryHelperStub = MockRepository.GenerateStub<DirectoryHelper>();
            m_DirectoryHelperStub.Stub(x => x.Exists(null))
                                 .IgnoreArguments()
                                 .Return(true);

            m_BitmapHelperStub = MockRepository.GenerateStub<BitmapHelper>();

            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectStub = MockRepository.GenerateStub<IProject>();

            m_ProjectManagerStub.Project = m_ProjectStub;
            m_ProjectStub.FolderPath = ProjectFolderPath;

            var componentInfoFactory = MockRepository.GenerateStub<IComponentInfoFactory>();

            m_TargetService = MockRepository.GenerateStub<ITargetService>();

            m_SymbolServiceIde = new TestableSymbolServiceIde(
                m_ProjectManagerStub,
                m_FileHelperMock,
                m_DirectoryHelperStub,
                m_BitmapHelperStub,
                m_OpenFileDialogStub,
                componentInfoFactory,
                m_TargetService);

            m_ISymbolServiceIde = m_SymbolServiceIde;

            var symbolInfo = MockRepository.GenerateStub<ISymbolInfo>();
            var symbolInfoFactory = MockRepository.GenerateStub<SymbolInfoFactoryIde>();
            symbolInfoFactory.Stub(x => x.CreateSymbolInfo(string.Empty, string.Empty)).IgnoreArguments().Return(symbolInfo);
            m_SymbolServiceIde.SymbolInfoFactory = symbolInfoFactory;



        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        #region AddSymbol

        [Test]
        public void AfterAddingASymbolTheDictionaryShouldContainIt()
        {
            string symbolPath = CreateSymbolPathFromSymbolName(SymbolName);
            string expectedSymbolName = SymbolName.ToLower();

            m_ISymbolServiceIde.AddSymbol(symbolPath);


            Assert.IsTrue(m_ISymbolServiceIde.GetSymbolNameList().Contains(expectedSymbolName));

        }

        [Test]
        public void AddSymbolThrowsFileNotFoundExceptionWhenFileDoesNotExist()
        {
            var fileHelper = MockRepository.GenerateStub<FileHelper>();
            fileHelper.Stub(x => x.Exists(null))
                            .IgnoreArguments()
                            .Return(false);
            m_SymbolServiceIde.FileHelper = fileHelper;

            Assert.Throws<FileNotFoundException>(() => m_ISymbolServiceIde.AddSymbol("somesymbolname"));
        }

        [Test]
        public void AddingDifferentSymbolsWithTheSameNameWillCreateThreeSymbolsWithUniqueNames()
        {
            string symbolPath = CreateSymbolPathFromSymbolName(SymbolName);

            var symbolInfo = MockRepository.GenerateStub<ISymbolInfo>();
            symbolInfo.Stub(x => x.SymbolName).Return(SymbolName);
            symbolInfo.Stub(x => x.FileName).Return(symbolPath);

            var symbolInfoFactory = MockRepository.GenerateStub<SymbolInfoFactoryIde>();
            symbolInfoFactory.Stub(x => x.CreateSymbolInfo(string.Empty, string.Empty)).IgnoreArguments().Return(symbolInfo);
            m_SymbolServiceIde.SymbolInfoFactory = symbolInfoFactory;

            string expectedSymbolName1 = SymbolName.ToLower();
            string expectedSymbolName2 = expectedSymbolName1 + " (1)";
            string expectedSymbolName3 = expectedSymbolName1 + " (2)";

            m_ISymbolServiceIde.AddSymbol(symbolPath);
            m_ISymbolServiceIde.AddSymbol(symbolPath);
            m_ISymbolServiceIde.AddSymbol(symbolPath);

            ICollection<string> symbols = m_ISymbolServiceIde.GetSymbolNameList();

            Assert.AreEqual(3, symbols.Count);
            Assert.AreEqual(true, symbols.Contains(expectedSymbolName1));
            Assert.AreEqual(true, symbols.Contains(expectedSymbolName2));
            Assert.AreEqual(true, symbols.Contains(expectedSymbolName3));
        }

        [Test]
        public void AddingTheSameIdenticalSymbolThriceOnlyAddsOneSymbol()
        {
            string symbolPath = CreateSymbolPathFromSymbolName(SymbolName);

            var symbolInfo = MockRepository.GenerateStub<ISymbolInfo>();
            symbolInfo.Stub(x => x.SymbolName).Return(SymbolName);
            symbolInfo.Stub(x => x.FileName).Return(symbolPath);

            var symbolInfoFactory = MockRepository.GenerateStub<SymbolInfoFactoryIde>();
            symbolInfoFactory.Stub(x => x.CreateSymbolInfo(string.Empty, string.Empty)).IgnoreArguments().Return(symbolInfo);
            m_SymbolServiceIde.SymbolInfoFactory = symbolInfoFactory;

            string expectedSymbolName = SymbolName.ToLower();

            var bitmapHelper = MockRepository.GenerateStub<BitmapHelperStub>();
            bitmapHelper.Stub(x => x.IsSameBinaryFile(null, null))
                        .IgnoreArguments()
                        .Return(true);
            m_SymbolServiceIde.BitmapHelper = bitmapHelper;

            m_ISymbolServiceIde.AddSymbol(symbolPath);
            m_ISymbolServiceIde.AddSymbol(symbolPath);
            m_ISymbolServiceIde.AddSymbol(symbolPath);

            ICollection<string> symbols = m_ISymbolServiceIde.GetSymbolNameList();

            Assert.AreEqual(1, symbols.Count);
            Assert.AreEqual(true, symbols.Contains(expectedSymbolName));
        }

        #endregion

        [Test]
        public void SelectNewSymbolShowsDialogAndReturnsSymbolName()
        {
            string symbolPath = CreateSymbolPathFromSymbolName(SymbolName);
            string expectedSymbolName = SymbolName.ToLower();

            m_OpenFileDialogStub.Stub(x => x.FileNames).Return(new [] { symbolPath });
            m_OpenFileDialogStub.Stub(x => x.ShowDialog()).Return(DialogResult.OK);

            string symbolName = m_ISymbolServiceIde.SelectNewSymbolFromFile();

            Assert.AreEqual(expectedSymbolName, symbolName);
        }

        #region FindUsedSymbols

        [Test]
        public void FindUsedSymbolsReturnsButtonSymbol()
        {
            string symbolName = "buttonsymbol";
            var button = MockRepository.GenerateStub<ApplicationFramework.Controls.Button>();
            button.Stub(x => x.SymbolIntervals).Return(new SymbolIntervalList());

            var symbolDictionary = (Dictionary<string, ISymbolInfo>)TestHelper.GetInstanceField(typeof(SymbolServiceIde), m_SymbolServiceIde, "m_SymbolDictionary");
            symbolDictionary.Add("buttonsymbol", new SymbolInfo(true));

            Dictionary<string, Size> usedSymbols = new Dictionary<string, Size>();

            button.SymbolName = symbolName;

            IList<ISymbolSizeInfo> symbols = m_ISymbolServiceIde.FindSymbolsUsed(button);
            foreach (ISymbolSizeInfo symbol in symbols)
            {
                usedSymbols.Add(symbol.Name, symbol.Size);
            }

            Assert.AreEqual(1, usedSymbols.Count);
            Assert.IsTrue(usedSymbols.ContainsKey(symbolName));
        }

        #endregion

        [Test]
        public void RemoveUnusedSymbolsTest()
        {
            var projectManager = TestHelper.AddServiceStub<IProjectManager>();
            projectManager.Stub(x => x.IsProjectDirty).Return(false);

            var symbolDictionary = (Dictionary<string, ISymbolInfo>)TestHelper.GetInstanceField(typeof(SymbolServiceIde), m_SymbolServiceIde, "m_SymbolDictionary");
            m_SymbolServiceIde.AddSymbol("Used", new SymbolInfo(true));
            m_SymbolServiceIde.AddSymbol("Unused", new SymbolInfo(true));

            var crossReferenceItems = new List<ISymbolCrossReferenceItem>();
            crossReferenceItems.Add(new SymbolCrossReferenceItem(string.Empty, string.Empty, "Used", 10, 10, false));

            var crossReferenceServiceStub = TestHelper.AddServiceStub<ICrossReferenceQueryService>();
            crossReferenceServiceStub.Expect(crossRefSvc => crossRefSvc.GetSymbols()).Return(crossReferenceItems);

            m_ISymbolServiceIde.FindAndRemoveUnusedSymbols();

            Assert.AreEqual(1, m_ISymbolServiceIde.GetSymbolList().Count);
        }

        #region Test Helpers

        private string CreateSymbolPathFromSymbolName(string symbolName)
        {
            return SymbolSourceDirectory + symbolName + SymbolFileExtension;
        }

        class TestableSymbolServiceIde : SymbolServiceIde
        {
            public TestableSymbolServiceIde(
                IProjectManager projectManager,
                FileHelper fileHelper,
                DirectoryHelper directoryHelper,
                BitmapHelper bitmapHelper,
                OpenFileDialogEx openFileDialogEx,
                IComponentInfoFactory componentInfoFactory,
                ITargetService targetService
                )
                : base(
                    projectManager,
                    fileHelper,
                    directoryHelper,
                    bitmapHelper,
                    openFileDialogEx,
                    componentInfoFactory,
                    targetService.ToILazy()
                )
            {
            }

            public void AddSymbol(string name, SymbolInfo symbolInfo)
            {
                m_SymbolDictionary.Add(name, symbolInfo);
            }
        }

        #endregion
    }
}
