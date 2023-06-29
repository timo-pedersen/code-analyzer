#if !VNEXT_TARGET
using System;
using Core.Api.Feature;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Datalogger.Features;
using Neo.ApplicationFramework.Tools.Storage;
using NSubstitute;
using NUnit.Framework;

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
            var fastLoggingFeatureLogicServiceStub = Substitute.For<IFastLoggingFeatureLogicService>();
            fastLoggingFeatureLogicServiceStub.GetPollGroupMinInterval().Returns(PollGroup.NormalMinInterval);
            return fastLoggingFeatureLogicServiceStub.ToILazy();
        }

        private ILazy<IFastLoggingFeatureLogicService> GetFastLoggingFeatureLogicService(string panelName)
        {
            var featureSecurityService = Substitute.For<IFeatureSecurityService>();
            featureSecurityService.IsActivated<FastLoggingFeatureCF>().Returns(true);

            var project = Substitute.For<IProject>();
            project.StorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true }; ;
            project.Terminal = Substitute.For<ITerminal>();
            project.Terminal.Name.Returns(panelName);

            var projectManager = Substitute.For<IProjectManager>();
            projectManager.IsProjectOpen.Returns(true);
            projectManager.Project = project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(featureSecurityService.ToILazy(), projectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            return fastLoggingFeatureLogicService.ToILazy();
        }
    }
}
#endif
