#if !VNEXT_TARGET
using System;
using System.Reflection;
using Core.Api.Feature;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.Storage.Settings;
using Neo.ApplicationFramework.Tools.Storage.PropertyPages;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Storage
{
    [TestFixture]
    class StorageSettingsViewModelPropertyPageTest
    {
        private StorageSettingsViewModel m_storageViewModel;
        private bool m_FastLoggingFeatureActive;
        private bool m_PlatformVersionIsCE8;
        private bool m_PanelHasSdCard;

        [OneTimeSetUp]
        public void OneTimeSetUpSetUp()
        {
            var targetService = Substitute.For<ITargetService>();
            var storageService = Substitute.For<IStorageService>();
            var projectManager = Substitute.For<IProjectManager>();
            var featureService = Substitute.For<IFeatureSecurityServiceIde>();
            var fastLoggingService = Substitute.For<IFastLoggingFeatureLogicService>();
            var project = Substitute.For<IProject>();
            var terminal = Substitute.For<ITerminal>();
            var storageProviderSettings = Substitute.For<IStorageProviderSettings>();
            var target = Substitute.For<ITarget>();

            storageProviderSettings.DisplayName.Returns("blah");
            storageProviderSettings.Settings.Returns(new IStorageProviderSetting[0]);
            target.Id.Returns(TargetPlatform.WindowsCE);
            target.PlatformVersion.Returns(x => m_PlatformVersionIsCE8 ? TargetPlatformVersion.CE8 : TargetPlatformVersion.CE6);
            targetService.CurrentTarget.Returns(target);
            fastLoggingService.IsFeatureAvailable(Arg.Any<string>(), Arg.Any<bool>())
                .Returns(a => m_FastLoggingFeatureActive);
            project.Terminal.Returns(terminal);
            project.StorageProviderSettings.Returns(storageProviderSettings);
            projectManager.Project.Returns(project);
            storageService.CreateProviderSettings(Arg.Any<string>(), Arg.Any<TargetPlatform>(), Arg.Any<TargetPlatformVersion>())
                .Returns(storageProviderSettings);
            storageService.ProvidersSupportedByTarget(Arg.Any<TargetPlatform>(), Arg.Any<TargetPlatformVersion>()).Returns(new string[0]);
            terminal.ExternalMemCardSupport.Returns(x => m_PanelHasSdCard ? 1 : 0);
            storageService.ProviderNames.Returns(new[] {"blah"});

            var gapServiceStub = Substitute.For<IGapService>();
            gapServiceStub.IsSubjectConsideredGap(Arg.Any<MemberInfo>()).Returns(false);

            m_storageViewModel = new StorageSettingsViewModel(targetService.ToILazy(), storageService, projectManager.ToILazy(), 
                featureService, fastLoggingService.ToILazy(), gapServiceStub.ToILazy());
        }
        
        [Test]
        [TestCase(false, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, true, true, true)]
        [TestCase(false, false, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, true, false, true)]
        [TestCase(true, true, true, true)]
        public void ShowStorageOptionsTest(bool featureActive, bool hasSdCard, bool isCE8, bool shouldShow)
        {
            m_FastLoggingFeatureActive = featureActive;
            m_PanelHasSdCard = hasSdCard;
            m_PlatformVersionIsCE8 = isCE8;

            bool result = m_storageViewModel.ShowStorageOptions;

            Assert.IsTrue(result == shouldShow);
        }

    }
}
#endif
