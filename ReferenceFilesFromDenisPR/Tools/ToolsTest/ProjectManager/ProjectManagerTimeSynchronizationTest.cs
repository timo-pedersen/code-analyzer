using System;
using System.Collections.Generic;
using Core.Api.Feature;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.SNTP;
using Neo.ApplicationFramework.Tools.DateTimeEdit;
using NSubstitute;
using NUnit.Framework;

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
            m_ProjectManagerMock = Substitute.For<IProjectManager>();
            m_ProjectFactory = Substitute.For<IProjectFactory>();
            m_ProjectManagerUI = Substitute.For<IProjectManagerUI>();
            m_IdeOptionsService = Substitute.For<IIDEOptionsService>();
            m_Project = Substitute.For<IProject>();
            IFeatureSecurityServiceIde featureSecurityServiceIde = Substitute.For<IFeatureSecurityServiceIde>();
            INameCreationService nameCreationService = Substitute.For<INameCreationService>();
            ITargetService targetService = Substitute.For<ITargetService>();
            var opcClientServiceIdeLazy = new LazyWrapper<IOpcClientServiceIde>(() => Substitute.For<IOpcClientServiceIde>());

            Func<ITerminalTargetChangeService> terminalTargetChangeServiceInitializer = () =>
            {
                var terminalTargetChangeService = Substitute.For<ITerminalTargetChangeService>();
                return terminalTargetChangeService;
            };
            var terminalTargetChangeServiceLazy = new LazyWrapper<ITerminalTargetChangeService>(terminalTargetChangeServiceInitializer);
            var messageBoxService = new LazyWrapper<IMessageBoxServiceIde>(() => Substitute.For<IMessageBoxServiceIde>());

            ITerminal terminal = Substitute.For<ITerminal>();
	        terminal.SupportCloud.Returns(true);
	        targetService.CurrentTargetInfo.TerminalDescription.Returns(terminal);

            IBrandService brandService = Substitute.For<IBrandService>();

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
                brandService.ToILazy());

            m_DesignerProjectItems = new List<IDesignerProjectItem>();
            m_SntpClientRootComponent = Substitute.For<ISntpClientRootComponent>();
            IDesignerProjectItem projectItem = Substitute.For<IDesignerProjectItem>();
            m_DesignerProjectItems.Add(projectItem);
            projectItem.ContainedObject.Returns(m_SntpClientRootComponent);
            m_Project.GetDesignerProjectItems(Arg.Any<Type>()).Returns(m_DesignerProjectItems.ToArray());
		}

		[Test]
        public void DefaultTimeSynchSettingsTimeSynchDisabledTest()
        {
            m_IdeOptionsService.GetOption<TimeSynchronizationOption>().Returns(new TimeSynchronizationOption { EnableTimeSync = false });

            m_ProjectManager.ConfigureDefaultTimeSyncSettings();

            Assert.AreEqual(false, m_SntpClientRootComponent.IsEnabled);
            Assert.AreEqual(string.Empty, m_SntpClientRootComponent.ServerName);
        }

        [Test]
        public void DefaultTimeSynchSettingsTimeSynchEnabledTest()
        {
            m_IdeOptionsService.GetOption<TimeSynchronizationOption>().Returns(new TimeSynchronizationOption { EnableTimeSync = true });
            m_ProjectManager.Project = m_Project;

            m_ProjectManager.ConfigureDefaultTimeSyncSettings();

            Assert.AreEqual(true, m_SntpClientRootComponent.IsEnabled);
            Assert.AreEqual(ApplicationConstantsCF.DefaultSntpServer, m_SntpClientRootComponent.ServerName);
        }
    }
}
