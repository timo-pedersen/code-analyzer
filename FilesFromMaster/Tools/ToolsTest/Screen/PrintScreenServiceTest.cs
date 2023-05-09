using System;
using System.Windows;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Neo.ApplicationFramework.Tools.Screen
{
    [TestFixture]
    public class PrintScreenServiceTest
    {
        private IPrintScreenService m_PrintScreenService;
        private IPrintScreenService m_PrintScreenServiceCF;
        private IScreenManager m_ScreenManager;
        private IMainScreen m_MainScreen;

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_PrintScreenService = new PrintScreenService();
            m_PrintScreenServiceCF = new PrintScreenServiceCF();
            m_ScreenManager = MockRepository.GenerateStub<IScreenManager>();
            m_MainScreen = MockRepository.GenerateStub<IMainScreen>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void PrintCurrentScreenDoesntThrowExceptionWhenMainScreenIsNotSetCF()
        {
            m_ScreenManager.Stub(x => x.ActiveScreen).Return(null);
            m_ScreenManager.Stub(x => x.MainScreen).Return(null);
            TestHelper.AddService(m_ScreenManager);

            Assert.DoesNotThrow(() => m_PrintScreenServiceCF.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }

        [Test]
        public void PrintCurrentScreenDoesntThrowExceptionWhenMainScreenIsNotSet()
        {
            m_ScreenManager.Stub(x => x.ActiveScreen).Return(null);
            m_ScreenManager.Stub(x => x.MainScreen).Return(null);
            TestHelper.AddService(m_ScreenManager);

            Assert.DoesNotThrow(() => m_PrintScreenService.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }

        [Test]
        public void PrintCurrentScreenThrowsExceptionWhenMainScreenIsSetAndActiveScreenIsNotCF()
        {
            
            m_ScreenManager.Stub(x => x.ActiveScreen).Return(null);
            m_ScreenManager.Stub(x => x.MainScreen).Return(m_MainScreen);
            TestHelper.AddService(m_ScreenManager);

            Assert.Throws<NullReferenceException>(() => m_PrintScreenServiceCF.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }
    }
}
