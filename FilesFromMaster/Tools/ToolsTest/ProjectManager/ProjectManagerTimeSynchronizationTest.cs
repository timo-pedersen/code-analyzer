using Core.Api.Feature;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Build;
using Neo.ApplicationFramework.Interfaces.SNTP;
using Neo.ApplicationFramework.Tools.DateTimeEdit;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
#region vNext
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.Tools.Build.DotNetRunner;
#endregion

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
	[TestFixture]
    public class ProjectManagerTimeSynchronizationTest
    {
        private IProjectManager m_ProjectManager;
        private IProjectManager m_ProjectManagerMock;
        private IProjectFactory m_ProjectFactory;
        private IProjectManagerUI m_ProjectManagerUI;
        private IIDEOptionsService m_IdeOptionsService;
        private IProject m_Project;
        private List<IDesignerProjectItem> m_DesignerProjectItems;
        private ISntpClientRootComponent m_SntpClientRootComponent;

		[SetUp]
        public void SetupUp()
        {
            m_ProjectManagerMock = MockRepository.GenerateMock<IProjectManager>();
            m_ProjectFactory = MockRepository.GenerateMock<IProjectFactory>();
            m_ProjectManagerUI = MockRepository.GenerateMock<IProjectManagerUI>();
            m_IdeOptionsService = MockRepository.GenerateStub<IIDEOptionsService>();
            m_Project = MockRepository.GenerateMock<IProject>();
            IFeatureSecurityServiceIde featureSecurityServiceIde = MockRepository.GenerateStub<IFeatureSecurityServiceIde>();
            INameCreationService nameCreationService = MockRepository.GenerateMock<INameCreationService>();
            ITargetService targetService = MockRepository.GenerateStub<ITargetService>();
            var opcClientServiceIdeLazy = new LazyWrapper<IOpcClientServiceIde>(() => MockRepository.GenerateStub<IOpcClientServiceIde>());

            Func<ITerminalTargetChangeService> terminalTargetChangeServiceInitializer = () =>
            {
                var terminalTargetChangeService = MockRepository.GenerateStub<ITerminalTargetChangeService>();
                return terminalTargetChangeService;
            };
            var terminalTargetChangeServiceLazy = new LazyWrapper<ITerminalTargetChangeService>(terminalTargetChangeServiceInitializer);
            var messageBoxService = new LazyWrapper<IMessageBoxServiceIde>(() => MockRepository.GenerateStub<IMessageBoxServiceIde>());

            ITerminal terminal = MockRepository.GenerateStub<ITerminal>();
	        terminal.Stub(x => x.SupportCloud).Return(true);
	        targetService.Stub(x => x.CurrentTargetInfo.TerminalDescription).Return(terminal);

            var brandService = MockRepository.GenerateMock<IBrandService>();
            var buildService = MockRepository.GenerateMock<IBuildService>();
            var errorListService = MockRepository.GenerateMock<IErrorListService>();
            var gapService = MockRepository.GenerateMock<IGapService>();
            var dotnetRunnerService = MockRepository.GenerateMock<IDotNetRunnerService>();

            m_ProjectManager = new ProjectManager(
                m_ProjectFactory,
                m_ProjectManagerUI,
                m_ProjectManagerMock,
                featureSecurityServiceIde.ToILazy(),
                nameCreationService,
                targetService.ToILazy(),
                opcClientServiceIdeLazy,
                terminalTargetChangeServiceLazy,
                m_IdeOptionsService.ToILazy(),
                messageBoxService,
                brandService.ToILazy(),
                buildService.ToILazy(),
                errorListService.ToILazy(),
                gapService.ToILazy(),
                dotnetRunnerService.ToILazy());

            m_DesignerProjectItems = new List<IDesignerProjectItem>();
            m_SntpClientRootComponent = MockRepository.GenerateStub<ISntpClientRootComponent>();
            IDesignerProjectItem projectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            m_DesignerProjectItems.Add(projectItem);
            projectItem.Stub(x => x.ContainedObject).Return(m_SntpClientRootComponent);
            ((IProjectItem)m_Project).Stub(mock => mock.GetDesignerProjectItems(Arg<Type>.Is.Anything)).Return(m_DesignerProjectItems.ToArray());
		}

		[Test]
        public void DefaultTimeSynchSettingsTimeSynchDisabledTest()
        {
            m_IdeOptionsService.AddOption<TimeSynchronizationOption>();
            m_IdeOptionsService.Stub(x => x.GetOption<TimeSynchronizationOption>().EnableTimeSync = false);

            m_ProjectManager.ConfigureDefaultTimeSyncSettings();

            Assert.AreEqual(false, m_SntpClientRootComponent.IsEnabled);
            Assert.AreEqual(null, m_SntpClientRootComponent.ServerName);
        }

        [Test]
        public void DefaultTimeSynchSettingsTimeSynchEnabledTest()
        {
            m_IdeOptionsService.AddOption<TimeSynchronizationOption>();
            m_IdeOptionsService.Stub(x => x.GetOption<TimeSynchronizationOption>().EnableTimeSync = true);
            m_ProjectManager.Project = m_Project;

            m_ProjectManager.ConfigureDefaultTimeSyncSettings();

            Assert.AreEqual(true, m_SntpClientRootComponent.IsEnabled);
            Assert.AreEqual(ApplicationConstantsCF.DefaultSntpServer, m_SntpClientRootComponent.ServerName);
        }
    }
}
