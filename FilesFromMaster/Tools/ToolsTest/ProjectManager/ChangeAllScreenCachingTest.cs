using System;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.ScreenCacheSetup;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ChangeAllScreenCachingTest
    {
        private const string StartupScreen = "HomeScreen";

        private IScreenGroupServiceIde m_ScreenGroupServiceIde;
        private IProjectManager m_ProjectManager;
        private IScreenCacheSetupService m_ScreenCacheSetupService;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            m_ProjectManager = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            m_ProjectManager.Expect(projectManager => projectManager.SaveProject()).Return(true);
            m_ProjectManager.Project = MockRepository.GenerateStub<IProject>();
            m_ProjectManager.Project.StartupScreen = StartupScreen;

            m_ScreenGroupServiceIde = TestHelper.CreateAndAddServiceStub<IScreenGroupServiceIde>();

            m_ScreenCacheSetupService = new ScreenCacheSetupService(m_ScreenGroupServiceIde.ToILazy(), m_ProjectManager.ToILazy());
        }

        [TestCase(0, 0)]
        [TestCase(0, 4)]
        [TestCase(4, 0)]
        [TestCase(4, 4)]
        [Test]
        public void EnableCachingOnAllAllowedScreens(int numberOfNormalScreens, int numberOfPopupScreens)
        {
            //ARRANGE
            var screens = ArrangeScreens(numberOfNormalScreens, numberOfPopupScreens);

            //ACT
            m_ScreenCacheSetupService.ChangeCacheOnAllScreens(true);

            //ASSERT
            Assert.That(IsCacheEnabledOnAllScreens(screens), Is.True, "Cache is not enabled on all allowed screens.");
            Assert.That(IsStartScreenCacheable(screens), Is.True, "Startscreen should be always cache enabled");
            Assert.That(AreAllPopupScreenNonCacheable(screens), Is.True, "Popup screens should be always cache disabled");
        }

        [TestCase(0, 0)]
        [TestCase(0, 4)]
        [TestCase(4, 0)]
        [TestCase(4, 4)]
        [Test]
        public void DisableCachingOnAllAllowedScreens(int numberOfNormalScreens, int numberOfPopupScreens)
        {
            //ARRANGE
            var screens = ArrangeScreens(numberOfNormalScreens, numberOfPopupScreens);

            //ACT
            m_ScreenCacheSetupService.ChangeCacheOnAllScreens(false);

            //ASSERT
            Assert.That(IsCacheDisabledOnAllScreens(screens), Is.True, "Cache is not disabled on all allowed screens.");
            Assert.That(IsStartScreenCacheable(screens), Is.True, "Startscreen should be always cache enabled");
            Assert.That(AreAllPopupScreenNonCacheable(screens), Is.True, "Popup screens should be always cache disabled");
        }

        private IScreenDesignerProjectItem[] ArrangeScreens(int numberOfNormalScreens, int numberOfPopupScreens)
        {
            var screenRootChildren = new List<IScreenDesignerProjectItem>();

            screenRootChildren.Add(CreateDesignerProjectItem(StartupScreen, true, false));
            if (numberOfNormalScreens > 0)
                screenRootChildren.AddRange(CreateNormalScreenStubs(numberOfNormalScreens));
            if (numberOfPopupScreens > 0)
                screenRootChildren.AddRange(CreatePopupScreenStubs(numberOfPopupScreens));

            m_ScreenGroupServiceIde.Expect(groupService => groupService.ScreenRoot.GetProjectItems()).Return(screenRootChildren.ToArray());
            return screenRootChildren.ToArray();
        }

        private IScreenDesignerProjectItem CreateDesignerProjectItem(string screenName, bool isCacheable, bool isPopup)
        {
            IScreenWindow normalScreen = MockRepository.GenerateStub<IScreenWindow>();

            normalScreen.Stub(screen => screen.Name).Return(screenName);
            normalScreen.IsCacheable = isCacheable;
            normalScreen.PopupScreen = isPopup;

            var screenDesignerProjectItem = MockRepository.GenerateStub<IScreenDesignerProjectItem>();
            screenDesignerProjectItem.Stub(designerProjectItem => designerProjectItem.DesignerHostInternal.RootDesigner)
                    .Return(MockRepository.GenerateStub<IScreenRootDesigner>());
            ((IScreenRootDesigner)screenDesignerProjectItem.DesignerHostInternal.RootDesigner).Stub(x => x.ScreenWindow).Return(normalScreen);

            return screenDesignerProjectItem;
        }

        private IEnumerable<IScreenDesignerProjectItem> CreateNormalScreenStubs(int numberOfScreens)
        {
            var normalScreenDesignerProjectItems = new List<IScreenDesignerProjectItem>();

            while (numberOfScreens > 0)
            {
                normalScreenDesignerProjectItems.Add(CreateDesignerProjectItem("Screen" + numberOfScreens, numberOfScreens % 2 == 0, false));
                numberOfScreens--;
            }

            return normalScreenDesignerProjectItems;
        }

        private IEnumerable<IScreenDesignerProjectItem> CreatePopupScreenStubs(int numberOfPopupScreens)
        {
            var popupScreenDesignerProjectItems = new List<IScreenDesignerProjectItem>();

            while (numberOfPopupScreens > 0)
            {
                popupScreenDesignerProjectItems.Add(CreateDesignerProjectItem("PopupScreen" + numberOfPopupScreens, false, true));
                numberOfPopupScreens--;
            }

            return popupScreenDesignerProjectItems;
        }

        private bool IsStartScreenCacheable(IEnumerable<IScreenDesignerProjectItem> screenRootChildren)
        {
            IDesignerProjectItem startScreen =
                screenRootChildren.FirstOrDefault(screen => ((IScreenRootDesigner)screen.DesignerHostInternal.RootDesigner).ScreenWindow.Name.Equals(StartupScreen));

            if (startScreen == null)
                return false;

            return (((IScreenRootDesigner)(startScreen).DesignerHostInternal.RootDesigner).ScreenWindow).IsCacheable;
        }

        private bool AreAllPopupScreenNonCacheable(IEnumerable<IScreenDesignerProjectItem> screenRootChildren)
        {
            IEnumerable<IScreenWindow> screens =
                screenRootChildren.Select(screen => ((IScreenRootDesigner)screen.DesignerHostInternal.RootDesigner).ScreenWindow);

            return !screens.Any(screen => screen.PopupScreen && screen.IsCacheable);
        }

        private bool IsCacheEnabledOnAllScreens(IEnumerable<IScreenDesignerProjectItem> screenRootChildren)
        {
            return CheckAllScreensForCaching(screenRootChildren, true);
        }

        private bool IsCacheDisabledOnAllScreens(IEnumerable<IDesignerProjectItem> screenRootChildren)
        {
            return CheckAllScreensForCaching(screenRootChildren, false);
        }

        private bool CheckAllScreensForCaching(IEnumerable<IDesignerProjectItem> screenRootChildren, bool isEnabled)
        {
            IEnumerable<IScreenWindow> screens =
                screenRootChildren.Select(screen => ((IScreenRootDesigner)screen.DesignerHostInternal.RootDesigner).ScreenWindow);
            screens = screens.Where(screen => !(screen.Name.Equals(StartupScreen) || screen.PopupScreen));

            if (!screens.Any())
                return true;

            return !screens.Any(screen => !screen.IsCacheable.Equals(isEnabled));
        }
    }
}
