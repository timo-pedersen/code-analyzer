using System;
using Core.Api.Feature;
using Neo.ApplicationFramework.Common.Features;
using Neo.ApplicationFramework.Interfaces;
using Storage.Settings;
using Neo.ApplicationFramework.Tools.Datalogger.Features;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_FeatureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            m_FeatureSecurityService.Stub(x => x.IsActivated<FastLoggingFeatureCF>()).Return(true);
            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManager.Stub(x => x.IsProjectOpen).Return(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = MockRepository.GenerateStub<ITerminal>();
            m_Project.Terminal.Stub(x => x.Name).Return("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.LessOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 0.05, "Fast logging should be allowed here");
        }

        [Test]
        public void MinLogIntervallPanelNameTest()
        {
            m_FeatureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            m_FeatureSecurityService.Stub(x => x.IsActivated<FastLoggingFeatureCF>()).Return(true);
            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManager.Stub(x => x.IsProjectOpen).Return(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = MockRepository.GenerateStub<ITerminal>();
            m_Project.Terminal.Stub(x => x.Name).Return("Panel PP886M"); // Wrong name!

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong panel");
        }

        [Test]
        public void MinLogIntervallFeatureTest()
        {
            m_FeatureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            m_FeatureSecurityService.Stub(x => x.IsActivated<FastLoggingFeatureCF>()).Return(false); //Feature not activated
            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManager.Stub(x => x.IsProjectOpen).Return(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = true };
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = MockRepository.GenerateStub<ITerminal>();
            m_Project.Terminal.Stub(x => x.Name).Return("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong Features activated");
        }

        [Test]
        public void MinLogIntervallStorageLocationTest()
        {
            m_FeatureSecurityService = MockRepository.GenerateStub<IFeatureSecurityService>();
            m_FeatureSecurityService.Stub(x => x.IsActivated<FastLoggingFeatureCF>()).Return(true);
            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManager.Stub(x => x.IsProjectOpen).Return(true);

            m_LocallyHostedStorageProviderSettings = new LocallyHostedProjectStorageProviderSettings { StorageLocationIsSdCard = false }; //StorageLocation is Hdd
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.StorageProviderSettings = m_LocallyHostedStorageProviderSettings;

            m_Project.Terminal = MockRepository.GenerateStub<ITerminal>();
            m_Project.Terminal.Stub(x => x.Name).Return("Panel PP886H");

            m_ProjectManager.Project = m_Project;

            var fastLoggingFeatureLogicService = new FastLoggingFeatureLogicServiceIde(m_FeatureSecurityService.ToILazy(), m_ProjectManager.ToILazy()) as IFastLoggingFeatureLogicService;
            Assert.GreaterOrEqual(fastLoggingFeatureLogicService.GetMinLogInterval(), 1.0, "Fast logging should not be allowed here due to wrong Storage Location");
        }
    }
}
