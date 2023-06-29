using System;
using System.Reflection;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Services;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ProjectConfiguration.PropertyPages.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class MemoryManagementSettingsTest
    {
        private IProjectManager m_ProjectManager;
        private IScreenCacheSetupService m_ScreenCacheSetupService;
        private IMessageBoxServiceIde m_MessageBoxServiceIde;
        private IInformationProgressService m_InformationProgressService;
        private IBrandServiceIde m_BrandServiceIde;
        private IStructuredTagService m_StructuredTagService;
        private IFileLaunchService m_FileLaunchService;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceStub<IEventBrokerService>();

            m_ScreenCacheSetupService = TestHelper.CreateAndAddServiceStub<IScreenCacheSetupService>();
            m_MessageBoxServiceIde = TestHelper.CreateAndAddServiceStub<IMessageBoxServiceIde>();
            m_InformationProgressService = TestHelper.CreateAndAddServiceStub<IInformationProgressService>();
            m_BrandServiceIde = TestHelper.CreateAndAddServiceStub<IBrandServiceIde>();
            m_BrandServiceIde.BrandName.Returns("iX");
            m_StructuredTagService = TestHelper.CreateAndAddServiceStub<IStructuredTagService>();
            m_FileLaunchService = TestHelper.CreateAndAddServiceStub<IFileLaunchService>();

            var project = Substitute.For<IProject>();
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_ProjectManager.Project = project;
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void MemoryManagementVisibility(bool isPC)
        {
            var currentTerminal = Substitute.For<ITerminal>();
            currentTerminal.IsPC.Returns(isPC);
            m_ProjectManager.Project.Terminal = currentTerminal;

            var gapServiceStubLazy = Substitute.For<ILazy<IGapService>>();
            gapServiceStubLazy.Value.Returns(Substitute.For<IGapService>());
            gapServiceStubLazy.Value.IsSubjectConsideredGap(Arg.Any<MemberInfo>()).Returns(false);

            var advancedPropertyPageViewModel = new AdvancedPropertyPageViewModel(
                m_ProjectManager,
                m_ScreenCacheSetupService.ToILazy(),
                m_MessageBoxServiceIde.ToILazy(),
                m_InformationProgressService.ToILazy(),
                m_BrandServiceIde.ToILazy(),
                m_StructuredTagService.ToILazy(),
                m_FileLaunchService,
                gapServiceStubLazy);

            Assert.AreEqual(isPC, advancedPropertyPageViewModel.ShowMemoryManagement);
        }
    }
}
