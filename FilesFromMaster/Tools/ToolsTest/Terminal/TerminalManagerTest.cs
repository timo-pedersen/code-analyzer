using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Terminal
{
    [TestFixture]
    public class TerminalManagerTest
    {
        private TerminalManager m_Manager;

        [SetUp]
        public void SetUp()
        {
            ICoreApplication coreApplication = MockRepository.GenerateMock<ICoreApplication>();

            const string startupPath = "./";
            Directory.CreateDirectory(startupPath);
            coreApplication.Stub(inv => inv.StartupPath).Return(startupPath);
            TestHelper.AddService<ICoreApplication>(coreApplication);

            m_Manager = new TerminalManager();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void EmptyTerminalListTest()
        {
            Lazy<List<ITerminal>> terminalList = m_Manager.TerminalList;

            Assert.True(terminalList.Value.Count == 0);
        }
    }
}
