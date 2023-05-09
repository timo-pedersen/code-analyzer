using System;
using Core.Api.Feature;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Datalogger.Features;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class PollGroupTest
    {
        [Test]
        [TestCase(40)]
        [TestCase(int.MaxValue)]
        public void TestIntervalLimits(int value)
        {
            Assert.Throws<ArgumentException>(() => new PollGroup(GetFastLoggingFeatureLogicServiceWithNormalMinInterval()) { Interval = value });
        }

        [Test]
        public void SettingIntervalToAValueBetweenMinAndMaxLimitIsAccepted()
        {
            IPollGroup pollGroup = new PollGroup(GetFastLoggingFeatureLogicServiceWithNormalMinInterval()) { Interval = 1000 };
            Assert.AreEqual(1000, pollGroup.Interval);
        }

        [Test]
        public void SettingIntervalToAFastLogValueIsAcceptedForRightPanelType()
        {
            IPollGroup pollGroup = new PollGroup(GetFastLoggingFeatureLogicService("Panel PP886H")) { Interval = 50 };
            Assert.AreEqual(50, pollGroup.Interval);
        }

        [Test]
        public void SettingIntervalToAFastLogValueIsNotAcceptedForWrongPanelType()
        {
            Assert.Throws<ArgumentException>(() => new PollGroup(GetFastLoggingFeatureLogicService("Panel PP886M")) { Interval = 50 });
        }

        private ILazy<IFastLoggingFeatureLogicService> GetFastLoggingFeatureLogicServiceWithNormalMinInterval()
        {
            var fastLoggingFeatureLogicServiceStub = MockRepository.GenerateStub<IFastLoggingFeatureLogicService>();
            fastLoggingFeatureLogicServiceStub.Stub(x => x.GetPollGroupMinInterval()).Return(PollGroup.NormalMinInterval);
            return fastLoggingFeatureLogicServiceStub.ToILazy();
        }

        private ILazy<IFastLoggingFeatureLogicService> GetFastLoggingFeatureLogicService(string panelName)
        {
            var featureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            featureSecurityService.Stub(x => x.IsActivated<FastLoggingFeatureCF>()).Return(true);

            var project = MockRepository.GenerateStub<IProject>();
            project.StorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true }; ;
            project.Terminal = MockRepository.GenerateStub<ITerminal>();
            project.Terminal.Stub(x => x.Name).Return(panelName);

            var projectManager = MockRepository.GenerateStub<IProjectManager>();
            projectManager.Stub(x => x.IsProjectOpen).Return(true);
            projectManager.Project = project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(featureSecurityService.ToILazy(), projectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            return fastLoggingFeatureLogicService.ToILazy();
        }
    }
}
