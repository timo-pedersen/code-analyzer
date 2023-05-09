using System;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Build;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Build;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectTest
    {
        private DirectoryHelper m_DirectoryHelperStub;

        private IProject m_Project;
        private IProjectItem m_ProjectProjectItem;
        private IProjectFactory m_ProjectFactory;

        private IProjectItemFactory m_ProjectItemFactoryMock;
        private IProjectItem m_ProjectItemMock;
        private IProjectManager m_ProjectManagerMock;
        private IIDEOptionsService m_IDEOptionsServiceMockup;

        //Rhino mocks
        private readonly MockRepository m_Mocks = new MockRepository();
        private IEventSubscriber m_Subscriber;
        private IBuildService m_BuildServiceStub;
        private IGapService m_GapServiceStub;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.ClearServices();
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            TestHelperExtensions.AddServiceToolManager(false);

            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());

            TestHelper.AddService<IProjectDefaultSettingsService>(new ProjectDefaultSettingsService());
            m_IDEOptionsServiceMockup = TestHelper.AddServiceStub<IIDEOptionsService>();

            var storageService = TestHelper.AddServiceStub<IStorageService>();
            storageService.Stub(x => x.CreateProviderSettings(null, (int)TargetPlatform.Windows, (int)TargetPlatformVersion.NotApplicable))
                .IgnoreArguments()
                .Return(new LocallyHostedProjectStorageProviderSettings());

            m_ProjectFactory = new ProjectFactory();

            m_ProjectItemFactoryMock = MockRepository.GenerateMock<IProjectItemFactory>();
            m_ProjectItemMock = MockRepository.GenerateMock<IProjectItem>();

            m_DirectoryHelperStub = MockRepository.GenerateStub<DirectoryHelper>();
            m_BuildServiceStub = TestHelper.AddServiceStub<IBuildService>();
            m_GapServiceStub = TestHelper.AddServiceStub<IGapService>();

            m_ProjectManagerMock = TestHelper.CreateAndAddServiceMock<IProjectManager>();
            m_ProjectManagerMock.Expect(x => x.ProjectActivity).Return(ProjectActivities.Inactive);

            m_Project = m_ProjectFactory.CreateProject();
            ((Project)m_Project).DirectoryHelper = m_DirectoryHelperStub;

            m_ProjectProjectItem = m_Project;
            ((ProjectItem)m_ProjectProjectItem).ProjectManager = m_ProjectManagerMock;

            //Rhino Mocks:
            m_Subscriber = m_Mocks.StrictMock<IEventSubscriber>();
        }

        [TearDown]
        public void VerifyMocks()
        {
            m_ProjectItemFactoryMock.VerifyAllExpectations();
            m_ProjectItemMock.VerifyAllExpectations();
            TestHelper.ClearServices();
        }

        private IProjectItem AddFormToEmptyProjectHelper()
        {
            //Check that the project creates a group and calls Add on that group
            m_ProjectItemMock.Stub(x => x.Name).Return("Form1");
            m_ProjectItemMock.Stub(x => x.Group).Return("Forms");
            m_ProjectItemMock.Stub(x => x.ProjectItems).Return(new IProjectItem[] { });

            IProjectItem groupProjectItemMock = MockRepository.GenerateMock<IProjectItem>();
            groupProjectItemMock.Stub(x => x.ProjectItems).Return(new IProjectItem[] { });
            groupProjectItemMock.Stub(x => x.Name).Return("Forms");

            m_ProjectItemFactoryMock.Expect(x => x.CreateGroup<GroupProjectItem>("Forms")).Return(groupProjectItemMock);

            m_ProjectProjectItem.Factory = m_ProjectItemFactoryMock;

            m_ProjectProjectItem.Add(m_ProjectItemMock);
            return groupProjectItemMock;
        }

        //Checks that a group is created when a projectitem is added
        [Test]
        public void AddFormToEmptyProject()
        {
            IProjectItem groupProjectItemMock = AddFormToEmptyProjectHelper();

            Assert.AreEqual(groupProjectItemMock, m_ProjectProjectItem.ProjectItems[0]);
            groupProjectItemMock.VerifyAllExpectations();
        }

        //Checks that no group is created if it already exists
        [Test]
        public void AddFormToProjectWithGroup()
        {
            IProjectItem groupProjectItemMock = AddFormToEmptyProjectHelper();

            m_ProjectItemMock.Stub(x => x.Name).Return("Form2");

            m_ProjectProjectItem.Add(m_ProjectItemMock);

            //Check that we havew only one subitem
            Assert.AreEqual(1, m_ProjectProjectItem.ProjectItems.GetLength(0));
            groupProjectItemMock.VerifyAllExpectations();
        }

        [Test]
        public void AddControllerToProjectWithFormsGroup()
        {
            AddFormToEmptyProjectHelper();


            IProjectItem controllerProjectItemMock = MockRepository.GenerateMock<IProjectItem>();
            controllerProjectItemMock.Stub(x => x.Name).Return("Controller");
            controllerProjectItemMock.Stub(x => x.Group).Return("Controllers");
            controllerProjectItemMock.Stub(x => x.ProjectItems).Return(new IProjectItem[] { });

            IProjectItem controllerGroupProjectItemMock = CreateProjectItemMock();
            controllerGroupProjectItemMock.Stub(x => x.Name).Return("Controllers");
            controllerGroupProjectItemMock.Expect(x => x.Add(controllerProjectItemMock));

            m_ProjectItemFactoryMock.Expect(x => x.CreateGroup<GroupProjectItem>("Controllers")).Return(controllerGroupProjectItemMock);

            //Number of groups is 1
            Assert.AreEqual(1, m_ProjectProjectItem.ProjectItems.GetLength(0));

            m_ProjectProjectItem.Add(controllerProjectItemMock);

            Assert.AreEqual(true, m_ProjectProjectItem.HasChild("Controllers"), "Controller group was not crerated");
            Assert.AreEqual(2, m_ProjectProjectItem.ProjectItems.GetLength(0));

            controllerProjectItemMock.VerifyAllExpectations();
            controllerGroupProjectItemMock.VerifyAllExpectations();
        }

        [Test]
        public void DeleteProjectGroupItem()
        {
            IProjectItem projectItemMock = AddFormToEmptyProjectHelper();

            m_ProjectProjectItem.DeleteChildItem(projectItemMock);

            Assert.AreEqual(true, m_ProjectProjectItem.IsEmpty);

            projectItemMock.VerifyAllExpectations();
        }

        private IProjectItem CreateProjectItemMock()
        {
            IProjectItem projectItemMock = MockRepository.GenerateMock<IProjectItem>();
            return projectItemMock;
        }

        [Test]
        public void AlphaNumericKeyboardLayoutSetterFiresItemChanged()
        {
            using (m_Mocks.Record())
            {
                ((IProjectTreeItem)m_Project).ItemChanged += m_Subscriber.Handler;

                m_Subscriber.Handler(m_Project, EventArgs.Empty);
            }

            using (m_Mocks.Playback())
            {
                m_Project.KeyboardLayout = null;
            }

            m_Mocks.VerifyAll();
        }

        [Test]
        public void BuildFilesAreNotCleanedWhenProjectVersionIsSameAsDesignerVersion()
        {
            AddTargetServiceStub();

            Project project = (Project)m_Project;
            project.Version = TestHelper.CurrentDesignerVersion.ToString();

            bool isBuildFilesCleaned = project.CleanBuildFilesForOldProject();
            Assert.IsFalse(isBuildFilesCleaned);
        }

        [Test]
        public void BuildFilesAreNotCleanedWhenTargetServiceIsMissingEvenIfVersionsDiffer()
        {
            Project project = (Project)m_Project;
            Version designerVersion = TestHelper.CurrentDesignerVersion;

            Version previousVersion = TestHelper.GetPreviousVersion(designerVersion);
            project.Version = previousVersion.ToString();

            bool isBuildFilesCleaned = project.CleanBuildFilesForOldProject();

            Assert.IsFalse(isBuildFilesCleaned);
        }

        [Test]
        public void BuildFilesAreNotCleanedWhenCommandLineParameterIsSetEvenIfVersionsDiffer()
        {
            AddTargetServiceStub();

            ICommandLineService commandLineServiceStub = TestHelper.CreateAndAddServiceStub<ICommandLineService>();
            commandLineServiceStub.Stub(x => x.CheckSwitch(ApplicationConstants.KeepBuildFilesSwitch)).Return(true);

            Project project = (Project)m_Project;
            Version designerVersion = TestHelper.CurrentDesignerVersion;

            Version previousVersion = TestHelper.GetPreviousVersion(designerVersion);
            project.Version = previousVersion.ToString();

            bool isBuildFilesCleaned = project.CleanBuildFilesForOldProject();

            Assert.IsFalse(isBuildFilesCleaned);
        }

        [Test]
        public void BuildFilesAreCleanedWhenVersionsDiffer()
        {
            AddTargetServiceStub();

            m_DirectoryHelperStub.Stub(x => x.Exists(null)).IgnoreArguments().Return(true);

            Project project = (Project)m_Project;
            Version designerVersion = TestHelper.CurrentDesignerVersion;

            Version previousVersion = TestHelper.GetPreviousVersion(designerVersion);
            project.Version = previousVersion.ToString();

            bool isBuildFilesCleaned = project.CleanBuildFilesForOldProject();

            Assert.IsTrue(isBuildFilesCleaned);
        }

        [Test]
        public void InitNewProjectSetScriptWarningsBehaviorToIdeDefaultValue()
        {
            BuildOptions buildOptions = new() { ScriptWarningsBehavior = WarningsBehavior.TreatAsErrors };
            m_IDEOptionsServiceMockup
                .Stub(x => x.GetOption<BuildOptions>())
                .Return(buildOptions);

            m_Project.InitNewProject();

            Assert.AreEqual(WarningsBehavior.TreatAsErrors, m_Project.ScriptWarningsBehavior);
        }

        private void AddTargetServiceStub()
        {
            ITargetService targetServiceStub = TestHelper.CreateAndAddServiceStub<ITargetService>();
            ITargetInfo targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetServiceStub.Stub(x => x.CurrentTargetInfo).Return(new TargetInfo());
        }
    }
}
