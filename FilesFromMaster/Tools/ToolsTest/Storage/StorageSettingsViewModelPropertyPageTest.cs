using System;
using Neo.ApplicationFramework.Interfaces;
using Storage.Common;
using NUnit.Framework;
using Neo.ApplicationFramework.Tools.Storage.PropertyPages;
using Core.Api.ProjectTarget;
using Core.Api.Feature;
using Storage.Settings;
using Rhino.Mocks;
using Core.Api.Platform;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using System.Reflection;

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
            var targetService = MockRepository.GenerateMock<ITargetService>();
            var storageService = MockRepository.GenerateMock<IStorageService>();
            var projectManager = MockRepository.GenerateMock<IProjectManager>();
            var featureService = MockRepository.GenerateMock<IFeatureSecurityServiceIde>();
            var fastLoggingService = MockRepository.GenerateMock<IFastLoggingFeatureLogicService>();
            var project = MockRepository.GenerateMock<IProject>();
            var terminal = MockRepository.GenerateMock<ITerminal>();
            var storageProviderSettings = MockRepository.GenerateMock<IStorageProviderSettings>();
            var target = MockRepository.GenerateMock<ITarget>();

            storageProviderSettings.Stub(x => x.DisplayName).Return("blah");
            storageProviderSettings.Stub(x => x.Settings).Return(new IStorageProviderSetting[0]);
            target.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);
            target.Stub(x => x.PlatformVersion).WhenCalled(x => x.ReturnValue = m_PlatformVersionIsCE8 ? TargetPlatformVersion.CE8 : TargetPlatformVersion.CE6).Return(TargetPlatformVersion.CE6); // return value ignored
            targetService.Stub(x => x.CurrentTarget).Return(target);
            fastLoggingService.Stub(x => x.IsFeatureAvailable(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
                .WhenCalled(a => a.ReturnValue = m_FastLoggingFeatureActive)
                .Return(m_FastLoggingFeatureActive); // return value required but ignored
            project.Stub(x => x.Terminal).Return(terminal);
            project.Stub(x => x.StorageProviderSettings).Return(storageProviderSettings);
            projectManager.Stub(x => x.Project).Return(project);
            storageService.Stub(x => x.CreateProviderSettings(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(storageProviderSettings);
            storageService.Stub(x => x.ProvidersSupportedByTarget(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(new string[0]);
            terminal.Stub(x => x.ExternalMemCardSupport).WhenCalled(x => x.ReturnValue = m_PanelHasSdCard ? 1 : 0).Return(0); // return value ignored
            storageService.Stub(x=>x.ProviderNames).Return(new[] {"blah"});

            var gapServiceStub = MockRepository.GenerateStub<IGapService>();
            gapServiceStub.Stub(x => x.IsSubjectConsideredGap(Arg<MemberInfo>.Is.Anything)).Return(false);

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
