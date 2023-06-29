using System;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Storage;
using NSubstitute;
using NUnit.Framework;

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

        //Rhino mocks
        private IEventSubscriber m_Subscriber;

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

            var storageService = TestHelper.AddServiceStub<IStorageService>();
            storageService.CreateProviderSettings(Arg.Any<string>(), Arg.Any<TargetPlatform>(), Arg.Any<TargetPlatformVersion>())
                .Returns(new LocallyHostedProjectStorageProviderSettings());

            m_ProjectFactory = new ProjectFactory();

            m_ProjectItemFactoryMock = Substitute.For<IProjectItemFactory>();
            m_ProjectItemMock = Substitute.For<IProjectItem>();

            m_DirectoryHelperStub = Substitute.For<DirectoryHelper>();

            m_Project = m_ProjectFactory.CreateProject();
            ((Project)m_Project).DirectoryHelper = m_DirectoryHelperStub;

            m_ProjectProjectItem = m_Project;

            m_ProjectManagerMock = SetProjectManagerMock(m_ProjectProjectItem);

            //Rhino Mocks:
            m_Subscriber = Substitute.For<IEventSubscriber>();
        }

        [TearDown]
        public void VerifyMocks()
        {
            TestHelper.ClearServices();
        }

        private IProjectManager CreateProjectManagerMock()
        {
            IProjectManager projectManagerMock = Substitute.For<IProjectManager>();
            projectManagerMock.ProjectActivity.Returns(ProjectActivities.Inactive);
            return projectManagerMock;
        }

        private IProjectManager SetProjectManagerMock(IProjectItem projectItem)
        {
            IProjectManager projectManagerMock = CreateProjectManagerMock();
            ((ProjectItem)projectItem).ProjectManager = projectManagerMock;
            return projectManagerMock;
        }

        private IProjectItem AddFormToEmptyProjectHelper()
        {
            //Check that the project creates a group and calls Add on that group
            m_ProjectItemMock.Name.Returns("Form1");
            m_ProjectItemMock.Group.Returns("Forms");
            m_ProjectItemMock.ProjectItems.Returns(new IProjectItem[] { });

            IProjectItem groupProjectItemMock = Substitute.For<IProjectItem>();
            groupProjectItemMock.ProjectItems.Returns(new IProjectItem[] { });
            groupProjectItemMock.Name.Returns("Forms");

            m_ProjectItemFactoryMock.CreateGroup<GroupProjectItem>("Forms").Returns(groupProjectItemMock);

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
        }

        //Checks that no group is created if it already exists
        [Test]
        public void AddFormToProjectWithGroup()
        {
            IProjectItem groupProjectItemMock = AddFormToEmptyProjectHelper();

            m_ProjectItemMock.Name.Returns("Form2");

            m_ProjectProjectItem.Add(m_ProjectItemMock);

            //Check that we havew only one subitem
            Assert.AreEqual(1, m_ProjectProjectItem.ProjectItems.GetLength(0));
        }

        [Test]
        public void AddControllerToProjectWithFormsGroup()
        {
            AddFormToEmptyProjectHelper();


            IProjectItem controllerProjectItemMock = Substitute.For<IProjectItem>();
            controllerProjectItemMock.Name.Returns("Controller");
            controllerProjectItemMock.Group.Returns("Controllers");
            controllerProjectItemMock.ProjectItems.Returns(new IProjectItem[] { });

            IProjectItem controllerGroupProjectItemMock = CreateProjectItemMock();
            controllerGroupProjectItemMock.Name.Returns("Controllers");

            m_ProjectItemFactoryMock.CreateGroup<GroupProjectItem>("Controllers").Returns(controllerGroupProjectItemMock);

            //Number of groups is 1
            Assert.AreEqual(1, m_ProjectProjectItem.ProjectItems.GetLength(0));

            m_ProjectProjectItem.Add(controllerProjectItemMock);

            Assert.AreEqual(true, m_ProjectProjectItem.HasChild("Controllers"), "Controller group was not crerated");
            Assert.AreEqual(2, m_ProjectProjectItem.ProjectItems.GetLength(0));

            controllerGroupProjectItemMock.Received().Add(controllerProjectItemMock);
            m_ProjectItemFactoryMock.Received().CreateGroup<GroupProjectItem>("Controllers");
        }

        [Test]
        public void DeleteProjectGroupItem()
        {
            IProjectItem projectItemMock = AddFormToEmptyProjectHelper();

            m_ProjectProjectItem.DeleteChildItem(projectItemMock);

            Assert.AreEqual(true, m_ProjectProjectItem.IsEmpty);
        }

        private IProjectItem CreateProjectItemMock()
        {
            IProjectItem projectItemMock = Substitute.For<IProjectItem>();
            return projectItemMock;
        }

        [Test]
        public void AlphaNumericKeyboardLayoutSetterFiresItemChanged()
        {
            ((IProjectTreeItem)m_Project).ItemChanged += m_Subscriber.Handler;

            m_Subscriber.Handler(m_Project, EventArgs.Empty);
            
            m_Project.KeyboardLayout = null;

            // Assert ???
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
            commandLineServiceStub.CheckSwitch(ApplicationConstants.KeepBuildFilesSwitch).Returns(true);

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

            m_DirectoryHelperStub.Exists(Arg.Any<string>()).Returns(true);

            Project project = (Project)m_Project;
            Version designerVersion = TestHelper.CurrentDesignerVersion;

            Version previousVersion = TestHelper.GetPreviousVersion(designerVersion);
            project.Version = previousVersion.ToString();

            bool isBuildFilesCleaned = project.CleanBuildFilesForOldProject();

            Assert.IsTrue(isBuildFilesCleaned);
        }

        private void AddTargetServiceStub()
        {
            ITargetService targetServiceStub = TestHelper.CreateAndAddServiceStub<ITargetService>();
            ITargetInfo targetInfoStub = Substitute.For<ITargetInfo>();
            targetServiceStub.CurrentTargetInfo.Returns(new TargetInfo());
        }
    }
}
