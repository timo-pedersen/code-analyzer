using System;
using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            m_ScreenManager = Substitute.For<IScreenManager>();
            m_MainScreen = Substitute.For<IMainScreen, Window>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void PrintCurrentScreenDoesntThrowExceptionWhenMainScreenIsNotSetCF()
        {
            m_ScreenManager.ActiveScreen.Returns(x => null);
            m_ScreenManager.MainScreen.Returns(x => null);
            TestHelper.AddService(m_ScreenManager);

            Assert.DoesNotThrow(() => m_PrintScreenServiceCF.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }

        [Test]
        public void PrintCurrentScreenDoesntThrowExceptionWhenMainScreenIsNotSet()
        {
            m_ScreenManager.ActiveScreen.Returns(x => null);
            m_ScreenManager.MainScreen.Returns(x => null);
            TestHelper.AddService(m_ScreenManager);

            Assert.DoesNotThrow(() => m_PrintScreenService.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }

        [Test]
        public void PrintCurrentScreenThrowsExceptionWhenMainScreenIsSetAndActiveScreenIsNotCF()
        {
            m_ScreenManager.ActiveScreen.Returns(x => null);
            m_ScreenManager.MainScreen.Returns(m_MainScreen);
            TestHelper.AddService(m_ScreenManager);

            Assert.Throws<NullReferenceException>(() =>
                m_PrintScreenService.PrintCurrentScreen(PrintScreenAction.Print, FileDirectory.ProjectFiles, ""));
        }
    }
}
