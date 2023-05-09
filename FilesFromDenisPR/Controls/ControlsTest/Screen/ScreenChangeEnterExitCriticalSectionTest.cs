using System;
using System.Collections.Generic;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Threading;
using Neo.ApplicationFramework.Utilities.Lazy;
using NSubstitute;
using NUnit.Framework;
using Form = Neo.ApplicationFramework.Controls.Controls.Form;

namespace Neo.ApplicationFramework.Controls.Screen
{
    /// <summary>
    /// Testing that the time critical scope used when changing screens is both entered and exited in all different kinds of screen change situations.
    /// </summary>
    [TestFixture]
    public class ScreenChangeEnterExitCriticalSectionTest
    {
        private IScopeService m_ScopeService;
        private TestScreenManager m_ScreenManager;
        private ISecurityServiceCF m_SecurityServiceCF;
        private IGlobalReferenceService m_GlobalReferenceService;
        private ScopeStub[] m_Scopes;
        private bool[] m_Shown;
        private int m_CurrentScopeIndex;
        private int m_ScopeIndex;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_CurrentScopeIndex = 0;
            m_ScopeIndex = 0;
            m_Scopes = new ScopeStub[20];
            m_Shown = new bool[20];
            for (int i = 0; i < m_Shown.Length; i++) { m_Shown[i] = false; }

            m_ScreenManager = new TestScreenManager();

            m_ScopeService = Substitute.For<IScopeService>();
            m_ScopeService.RequestScope(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => 
                {
                    var scope = new ScopeStub();
                    m_Scopes[m_CurrentScopeIndex] = scope;
                    m_CurrentScopeIndex++;
                    return scope;
                });

            IsAccessGranted = true;
            m_SecurityServiceCF = Substitute.For<ISecurityServiceCF>();
            m_SecurityServiceCF.IsAccessGranted(Arg.Any<object>())
                .Returns(x =>
                {
                    return IsAccessGranted;
                });

            m_GlobalReferenceService = Substitute.For<IGlobalReferenceService>();

            ServiceContainerCF.Instance.AddService(typeof(IScreenManager), m_ScreenManager);
            ServiceContainerCF.Instance.AddService(typeof(IScopeService), m_ScopeService);
            ServiceContainerCF.Instance.AddService(typeof(ISecurityServiceCF), m_SecurityServiceCF);
            ServiceContainerCF.Instance.AddService(typeof(IGlobalReferenceService), m_GlobalReferenceService);
        }

        private bool IsAccessGranted { get; set; }

        #region Assert methods

        private void AssertHasNotEntered() { AssertHasNotEntered(m_ScopeIndex); }
        private void AssertHasNotEntered(int scopeIndex)
        {
            Assert.IsNull(m_Scopes[scopeIndex]);
        }

        private void AssertHasEntered() { AssertHasEntered(m_ScopeIndex); }
        private void AssertHasEntered(int scopeIndex)
        {
            Assert.IsNotNull(m_Scopes[scopeIndex]);
        }

        private void AssertHasExited() { AssertHasExited(m_ScopeIndex); }
        private void AssertHasExited(int scopeIndex)
        {
            Assert.IsTrue(m_Scopes[scopeIndex].HasExited);
        }

        private void AssertHasEnteredAndExited()
        {
            AssertHasEntered();
            AssertHasExited();
        }

        private void AssertHasBeenShown(bool shown)
        {
            Assert.AreEqual(shown, m_Shown[m_ScopeIndex]);
        }

        #endregion

        #region Do methods

        private enum ScreenChangeType
        {
            Show,
            Back,
            Forward
        }

        private IScreen DoCreateScreen(bool cacheable, bool popup, bool isStartScreen = false)
        {
            if (cacheable && popup)
                Assert.Fail();

            AssertHasNotEntered();
            IScreen screen = isStartScreen ?
                new StartupScreen(new LazyCF<IScopeService>(() => m_ScopeService), m_GlobalReferenceService.ToILazy()) :
                new Form(new LazyCF<IScopeService>(() => m_ScopeService), m_GlobalReferenceService.ToILazy(), cacheable);
            AssertHasEnteredAndExited();
            m_ScopeIndex++;

            screen.PopupScreen = popup;
            screen.Shown += OnShown;

            return screen;
        }

        private void OnShown(object sender, EventArgs e)
        {
            m_Shown[m_ScopeIndex] = true;
        }

        private IScreen DoShowScreen(IScreen screen, bool shouldBeShown, ScreenChangeType screenChangeType = ScreenChangeType.Show, bool shouldEnterAndExit = true)
        {
            AssertHasNotEntered();
            switch (screenChangeType)
            {
                case ScreenChangeType.Back:
                    screen.BackScreen();
                    screen = m_ScreenManager.BackScreen;
                    break;
                case ScreenChangeType.Forward:
                    screen.ForwardScreen();
                    screen = m_ScreenManager.ForwardScreen;
                    break;
                case ScreenChangeType.Show:
                default:
                    screen.Show();
                    break;
            }
            if (screen != null)
                ((Form)screen).Refresh(); // to fire paint event (which will only be done if screen is actually visible)

            AssertHasBeenShown(shouldBeShown);
            if (shouldEnterAndExit)
                AssertHasEnteredAndExited();
            else
                AssertHasNotEntered();
            m_ScopeIndex++;
            return screen;
        }

        private IScreen DoShowPreviousScreen(IScreen currentScreen, IScreen previousScreen, bool shouldBeShown, bool shouldEnterAndExit = true)
        {
            m_ScreenManager.BackScreen = previousScreen;
            return DoShowScreen(currentScreen, shouldBeShown, ScreenChangeType.Back, shouldEnterAndExit);
        }

        private IScreen DoShowNextScreen(IScreen currentScreen, IScreen nextScreen, bool shouldBeShown, bool shouldEnterAndExit = true)
        {
            m_ScreenManager.ForwardScreen = nextScreen;
            return DoShowScreen(currentScreen, shouldBeShown, ScreenChangeType.Forward, shouldEnterAndExit);
        }

        private IScreen DoCreateNonCachedScreen()
        {
            return DoCreateScreen(false, false);
        }

        private IScreen DoCreateCachedScreen()
        {
            return DoCreateScreen(true, false);
        }

        private IScreen DoCreatePopup()
        {
            return DoCreateScreen(false, true);
        }

        public IScreen DoCreateAndShowNonCachedScreen(bool shouldBeShown = true)
        {
            var screen = DoCreateNonCachedScreen();
            DoShowScreen(screen, shouldBeShown);
            return screen;
        }

        public IScreen DoCreateAndShowCachedScreen(bool shouldBeShown = true)
        {
            var screen = DoCreateCachedScreen();
            DoShowScreen(screen, shouldBeShown);
            return screen;
        }

        public IScreen DoCreateAndShowPopup(bool shouldBeShown = true)
        {
            var screen = DoCreatePopup();
            DoShowScreen(screen, shouldBeShown);
            return screen;
        }

        #endregion

        #region Test cases

        [Test]
        public void CreateNonCachedScreen()
        {
            DoCreateNonCachedScreen();
        }

        [Test]
        public void CreateCachedScreen()
        {
            DoCreateCachedScreen();
        }

        [Test]
        public void CreatePopup()
        {
            DoCreatePopup();
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void ShowNonCachedScreen()
        {
            DoCreateAndShowNonCachedScreen();
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void ShowCachedScreen()
        {
            DoCreateAndShowCachedScreen();
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void ShowPopup()
        {
            DoCreateAndShowPopup();
        }

        // Can't get this to work, because for some reason the Paint event in the Form won't be fired
        // when you show the screen the second time. But it is fired when running the application.
        //[Test]
        //[Category("RunOnlyOnLocalMachine")]
        //public void ShowCachedScreenTwice()
        //{
        //    var screen = DoCreateAndShowCachedScreen();
        //    DoCreateAndShowCachedScreen();

        //    // Show first cached screen again
        //    DoShowScreen(screen, true);
        //}

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void ShowPreviousScreen()
        {
            var previousScreen = DoCreateCachedScreen();
            var currentScreen = DoCreateAndShowCachedScreen();

            DoShowPreviousScreen(currentScreen, previousScreen, true);

            Assert.AreSame(previousScreen, m_ScreenManager.ActiveScreen);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void ShowNextScreen()
        {
            var nextScreen = DoCreateCachedScreen();
            var currentScreen = DoCreateAndShowCachedScreen();

            DoShowNextScreen(currentScreen, nextScreen, true);
            Assert.AreSame(nextScreen, m_ScreenManager.ActiveScreen);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowSameScreen()
        {
            var screen = DoCreateAndShowCachedScreen();
            DoShowScreen(screen, false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowScreenWithNoAccessGranted()
        {
            IsAccessGranted = false;
            DoCreateAndShowCachedScreen(false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowScreenWhichFailsToBeRegistered()
        {
            m_ScreenManager.RegisterScreenReturnValue = false;
            DoCreateAndShowCachedScreen(false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowNonExistentPreviousScreen()
        {
            var screen = DoCreateCachedScreen();
            DoShowPreviousScreen(screen, null, false, false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowNonExistentNextScreen()
        {
            var screen = DoCreateCachedScreen();
            DoShowNextScreen(screen, null, false, false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowAlreadyVisiblePopup()
        {
            DoCreateAndShowNonCachedScreen();
            var popup = DoCreateAndShowPopup();
            DoShowScreen(popup, false);
        }

        /// <summary>
        /// When you open the parent screen from a popup,
        /// you just close the popup, not actually show the parent screen again.
        /// </summary>
        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowParentScreenFromPopup()
        {
            var screen = DoCreateAndShowNonCachedScreen();
            DoCreateAndShowPopup();
            DoShowScreen(screen, false);
        }

        [Test]
        [Category("RunOnlyOnLocalMachine")]
        public void TryShowScreensBeforeStartScreenAtStartup()
        {
            m_ScreenManager.StartupScreenHasBeenShown = false;
            m_ScreenManager.StartUpScreenType = typeof(StartupScreen);
            var startScreen = DoCreateScreen(false, false, true);

            // Try to show screen and popup before the start screen, and they should not be shown
            var screen = DoCreateAndShowNonCachedScreen(false);
            var popup = DoCreateAndShowPopup(false);

            // Try to show start screen and it should be shown
            DoShowScreen(startScreen, true);

            // The list of screens to show after start screen should be correctly populated
            var expectedList = new List<IScreen>() { screen, popup };
            CollectionAssert.AreEqual(expectedList, m_ScreenManager.ScreensToShowAfterStartupScreen);
        }

        #endregion
    }

    internal class ScopeStub : IDisposable
    {
        public bool HasExited { get; private set; }

        public void Dispose()
        {
            HasExited = true;
        }
    }

    /// <summary>
    /// A class to be able to differentiate the startup screen from other screens using IScreenManager.StartupScreenType.
    /// </summary>
    internal class StartupScreen : Form
    {
        public StartupScreen(LazyCF<IScopeService> scopeService, ILazy<IGlobalReferenceService> globalReferenceService)
            : base(scopeService, globalReferenceService)
        {
        }
    }
}
