#if !VNEXT_TARGET
using System;
using Core.Api.Feature;
using Neo.ApplicationFramework.Common.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Storage.Settings;
using Neo.ApplicationFramework.Tools.Datalogger.Features;
using Neo.ApplicationFramework.Tools.Storage;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.DataLogger
{
    [TestFixture]
    public class DataLoggerPropertyTest
    {
        private IFeatureSecurityService m_FeatureSecurityService;
        private IProjectManager m_ProjectManager;
        private ILocallyHostedStorageProviderSettings m_LocallyHostedStorageProviderSettings;
        private IProject m_Project;

        [Test]
        public void MinLogIntervallTest()
        {
            m_FeatureSecurityService = Substitute.For<IFeatureSecurityService>();
            m_FeatureSecurityService.IsActivated<FastLoggingFeatureCF>().Returns(true);
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_ProjectManager.IsProjectOpen.Returns(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = Substitute.For<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = Substitute.For<ITerminal>();
            m_Project.Terminal.Name.Returns("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.LessOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 0.05, "Fast logging should be allowed here");
        }

        [Test]
        public void MinLogIntervallPanelNameTest()
        {
            m_FeatureSecurityService = Substitute.For<IFeatureSecurityService>();
            m_FeatureSecurityService.IsActivated<FastLoggingFeatureCF>().Returns(true);
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_ProjectManager.IsProjectOpen.Returns(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = Substitute.For<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = Substitute.For<ITerminal>();
            m_Project.Terminal.Name.Returns("Panel PP886M"); // Wrong name!

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong panel");
        }

        [Test]
        public void MinLogIntervallFeatureTest()
        {
            m_FeatureSecurityService = Substitute.For<IFeatureSecurityService>();
            m_FeatureSecurityService.IsActivated<FastLoggingFeatureCF>().Returns(false); //Feature not activated
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_ProjectManager.IsProjectOpen.Returns(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = Substitute.For<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = Substitute.For<ITerminal>();
            m_Project.Terminal.Name.Returns("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong Features activated");
        }

        [Test]
        public void MinLogIntervallStorageLocationTest()
        {
            m_FeatureSecurityService = Substitute.For<IFeatureSecurityService>();
            m_FeatureSecurityService.IsActivated<FastLoggingFeatureCF>().Returns(true);
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_ProjectManager.IsProjectOpen.Returns(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = false }; //StorageLocation is Hdd
            m_Project = Substitute.For<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = Substitute.For<ITerminal>();
            m_Project.Terminal.Name.Returns("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong Storage Location");
        }
    }
}
#endif
