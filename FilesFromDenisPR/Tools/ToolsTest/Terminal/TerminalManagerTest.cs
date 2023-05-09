#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Terminal
{
    [TestFixture]
    public class TerminalManagerTest
    {
        private TerminalManager m_Manager;
        //private ITerminalManagerService m_ManagerService;

        [SetUp]
        public void SetUp()
        {
            ICoreApplication coreApplication = Substitute.For<ICoreApplication>();

            const string startupPath = "./";
            Directory.CreateDirectory(startupPath);
            coreApplication.StartupPath.Returns(startupPath);
            TestHelper.AddService<ICoreApplication>(coreApplication);

            m_Manager = new TerminalManager();
            //m_ManagerService = new TerminalManagerService(m_Manager);
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
#endif
