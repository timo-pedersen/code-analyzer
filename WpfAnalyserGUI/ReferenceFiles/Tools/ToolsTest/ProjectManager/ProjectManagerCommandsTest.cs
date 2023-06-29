using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Core.Api.Feature;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Brand;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Dialogs.InformationProgress;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.WindowManagement;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Wizards.CreateNewProjectWizard;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectManagerCommandsTest
    {
        private readonly string m_Project1neo;
        private readonly string m_Project2neo;
        private readonly string m_ProjectFolderPath;
        private readonly string m_Project2FolderPath;
        private readonly string m_ProjectExtension;
        private readonly string m_DesignerExtension;

        private IProjectManager m_ProjectManager;
        private IProjectManager m_ProjectManagerMock;
        private IProjectFactory m_ProjectFactory;
        private IProject m_Project;
        private IProjectManagerUI m_ProjectManagerUI;
        private ILoadOnDemandService m_LoadOnDemandService;
        private FileHelper m_FileHelper;
        private DirectoryHelper m_DirectoryHelper;
        private IWindowService m_WindowService;
        private IWindowServiceIde m_WindowServiceIde;
        private ITypeListService m_TypeListService;
        private INameCreationService m_NameCreationService;
        private IMessageBoxServiceIde m_MessageBoxService;

        private readonly ITargetInfo m_TargetInfo;
        private bool m_ProjectLoadedEventCalled;
        private bool m_ProjectCreatedEventCalled;
        private bool m_ProjectOpenedEventCalled;
        private bool m_ProjectSavedEventCalled;

        public ProjectManagerCommandsTest()
        {
            m_ProjectExtension = BrandConstants.ProjectFileExtension;
            m_DesignerExtension = BrandConstants.FileExtension;

            m_Project1neo = $"Project1{m_ProjectExtension}";
            m_Project2neo = $"Project2{m_ProjectExtension}";
            m_ProjectFolderPath = @"c:\neo\MyProject";
            m_Project2FolderPath = @"c:\neo\MyProject2";
            m_TargetInfo = Substitute.For<ITargetInfo>();
            m_TargetInfo.ProjectFilesPath = "ProjectFiles";
            m_TargetInfo.TempPath = "TempPath";
        }

        [TearDown]
        public void TearDown()
        {
            m_ProjectManagerMock = Substitute.For<IProjectManager>();
        }

        [SetUp]
        public void SetupProjectManager()
        {
            IBrandService brandService = Substitute.For<IBrandService>();

            IDesignerMetadata designerMetadataStub = Substitute.For<IDesignerMetadata>();
            IDesignerInfo designerInfoStub = Substitute.For<IDesignerInfo>();
            designerInfoStub.Metadata.Returns(designerMetadataStub);
            designerInfoStub.Type.Returns(typeof(DataSourceContainer));

            List<IDesignerInfo> designersList = new List<IDesignerInfo>
            {
                designerInfoStub
            };
            m_TypeListService = Substitute.For<ITypeListService>();
            m_NameCreationService = Substitute.For<INameCreationService>();

            TestHelper.CreateAndAddServiceStub<IWelcomeScreenGreetingService>();

            TestHelper.AddService(brandService);
                brandService.FileExtension.Returns(m_DesignerExtension);
                brandService.ProjectFileExtension.Returns(m_DesignerExtension);

            TestHelper.AddService(m_TypeListService);
                m_TypeListService.GetDesigners().Returns(designersList);

            TestHelper.AddService(m_NameCreationService);
            m_NameCreationService.IsValidName(Arg.Any<string>(), ref Arg.Any<string>()).Returns(true);
            m_NameCreationService.IsValidFileName(Arg.Any<string>(), ref Arg.Any<string>()).Returns(true);

            TestHelper.AddService<IInformationProgressService>(new InvisibleInformationProgressManager());
            TestHelper.AddService<IProjectManagerOpenService>(new ProjectManagerOpenService());
            TestHelper.UseTestWindowThreadHelper = true;

            m_ProjectFactory = Substitute.For<IProjectFactory>();
            m_Project = Substitute.For<IProject>();
            m_Project.FeatureDependencies.Returns(Enumerable.Empty<IFeatureDependency>());
            m_ProjectManagerUI = Substitute.For<IProjectManagerUI>();
            m_ProjectManagerMock = Substitute.For<IProjectManager>();
            m_LoadOnDemandService = Substitute.For<ILoadOnDemandService>();
            m_WindowService = Substitute.For<IWindowService>();
            m_WindowServiceIde = Substitute.For<IWindowServiceIde>();

            m_FileHelper = Substitute.For<FileHelper>();
            m_DirectoryHelper = Substitute.For<DirectoryHelper>();

            Func<IFeatureSecurityServiceIde> fssFunc = () =>
            {
                var featureSecurityServiceIde = Substitute.For<IFeatureSecurityServiceIde>();
                featureSecurityServiceIde.GetAllActiveFeatures().Returns(Enumerable.Empty<ISecuredFeature>());
                featureSecurityServiceIde.GetAllFeatures().Returns(Enumerable.Empty<IFeature>());
                return featureSecurityServiceIde;
            };

            var featureSecurityServiceIdeLazy = new LazyWrapper<IFeatureSecurityServiceIde>(fssFunc);

            var opcClientServiceIdeLazy = new LazyWrapper<IOpcClientServiceIde>(() => Substitute.For<IOpcClientServiceIde>());

            Func<ITargetService> targetServiceInitializer = () =>
            {
                var targetService = Substitute.For<ITargetService>();
                targetService.CurrentTargetInfo.Returns(m_TargetInfo);
                return targetService;
            };

            var targetServiceLazy = new LazyWrapper<ITargetService>(targetServiceInitializer);

            Func<ITerminalTargetChangeService> terminalTargetChangeServiceInitializer = () =>
            {
                var terminalTargetChangeService = Substitute.For<ITerminalTargetChangeService>();
                return terminalTargetChangeService;
            };

            var terminalTargetChangeServiceLazy = new LazyWrapper<ITerminalTargetChangeService>(terminalTargetChangeServiceInitializer);

            var namingConstraints = Substitute.For<INamingConstraints>();
            namingConstraints.IsNameLengthValid(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
            namingConstraints.ReservedApplicationNames.Returns(new HashSet<string>());
            namingConstraints.ReservedSystemNames.Returns(new HashSet<string>());

            INameCreationService nameCreationService = new NameCreationService(namingConstraints);

            var ideOptionsService = Substitute.For<IIDEOptionsService>();

            var messageBoxService = Substitute.For<IMessageBoxServiceIde>();

            m_ProjectManager = new ProjectManager(
                m_ProjectFactory,
                m_ProjectManagerUI,
                m_ProjectManagerMock,
                featureSecurityServiceIdeLazy,
                nameCreationService,
                targetServiceLazy,
                opcClientServiceIdeLazy,
                terminalTargetChangeServiceLazy,
                ideOptionsService.ToILazy(),
                messageBoxService.ToILazy(),
                brandService.ToILazy());

            ((ProjectManager)m_ProjectManager).LoadOnDemandService = m_LoadOnDemandService;
            ((ProjectManager)m_ProjectManager).FileHelper = m_FileHelper;
            ((ProjectManager)m_ProjectManager).DirectoryHelper = m_DirectoryHelper;
            ((ProjectManager)m_ProjectManager).WindowService = m_WindowService;
            ((ProjectManager)m_ProjectManager).WindowServiceIde = m_WindowServiceIde;
            m_MessageBoxService = messageBoxService;
            m_ProjectManager.ProjectOpened += OnProjectOpened;
            m_ProjectManager.ProjectLoaded += OnProjectLoaded;
            m_ProjectManager.ProjectCreated += OnProjectCreated;
            m_ProjectManager.ProjectSaved += OnProjectSaved;

            m_DirectoryHelper.Exists(Arg.Any<string>()).Returns(true);
        }


        private void OnProjectCreated(object sender, EventArgs e)
        {
            Assert.AreEqual(m_ProjectManager, sender);
            m_ProjectCreatedEventCalled = true;
        }

        private void OnProjectLoaded(object sender, EventArgs e)
        {
            Assert.AreEqual(m_ProjectManager, sender);
            m_ProjectLoadedEventCalled = true;
        }

        private void OnProjectSaved(object sender, EventArgs e)
        {
            Assert.AreEqual(m_ProjectManager, sender);
            m_ProjectSavedEventCalled = true;
        }

        private void OnProjectOpened(object sender, EventArgs e)
        {
            Assert.AreEqual(m_ProjectManager, sender);
            m_ProjectOpenedEventCalled = true;
        }

        [TearDown]
        public void VerifyMocks()
        {
            m_ProjectManager.ProjectOpened -= OnProjectOpened;
            m_ProjectManager.ProjectSaved -= OnProjectSaved;
        }

        [Test]
        public void IsProjectOpen_FalseAtCreation()
        {
            Assert.AreEqual(false, m_ProjectManager.IsProjectOpen);
        }

        [Test]
        public void IsProjectDirty_FalseAtCreation()
        {
            Assert.AreEqual(false, m_ProjectManager.IsProjectDirty);
        }

        [Test]
        public void IsProjectDirty_WithUndirtyProject()
        {
            SetupProjectInProjectManager(false);
            Assert.AreEqual(false, m_ProjectManager.IsProjectDirty);
        }

        [Test]
        public void IsProjectDirty_WithDirtyProject()
        {
            SetupProjectInProjectManager(true);
            Assert.AreEqual(true, m_ProjectManager.IsProjectDirty);
        }

        [Test]
        public void Project_NullAtCreation()
        {
            Assert.AreEqual(null, m_ProjectManager.Project, "Project should be null");
        }

        private bool CreateNewProject(bool projectInfoDialogResult)
        {
            const string projectName = "Project1";
            IProjectSettings projectSettings = new ProjectSettings();
            projectSettings.Name = projectName;
            projectSettings.Location = Path.GetTempPath();
            projectSettings.Terminal = Substitute.For<ITerminal>();
            projectSettings.Terminal.PanelTypeGroup.Returns(PanelTypeGroup.TxB);

            string folderPath = Path.Combine(projectSettings.Location, projectName);
            string filePath = Path.Combine(folderPath, Path.ChangeExtension(projectName, m_ProjectExtension.Replace(".", "")));

            m_ProjectManagerMock.CloseProject(false).Returns(true);

            if (projectInfoDialogResult)
            {
                m_ProjectManagerUI.ShowNewProjectWizardInfoDialog(Arg.Any<bool>()).Returns(projectSettings);
            }
            else
            {
                m_ProjectManagerUI.ShowNewProjectWizardInfoDialog(Arg.Any<bool>()).Returns(x => null);
            }

            if (projectInfoDialogResult)
            {
                m_ProjectFactory.CreateProject().Returns(m_Project);
                m_ProjectFactory.SaveProject(m_Project, filePath).Returns(true);
                m_Project.FireItemChanged();

                m_Project.Name = projectName;
                m_Project.FolderPath = folderPath;
                m_Project.FolderPath.Returns(folderPath);
                m_Project.FileExtension.Returns(m_ProjectExtension);
                m_Project.IsDirty.Returns(true);
                m_Project.Filename.Returns(Path.GetFileName(filePath));
            }

            return m_ProjectManager.NewProjectUI();
        }

        [Test]
        public void NewProject_NoProjectIsOpen_UserOKOnNewInfo()
        {
            m_Project.StartupScreen.Returns(string.Empty);

            Assert.IsTrue(CreateNewProject(true), "Create New Project should return true");
            Assert.IsNotNull(m_ProjectManager.Project, "No project after NewProject");
            Assert.AreEqual(true, m_ProjectManager.IsProjectOpen);
        }

        [Test]
        public void NewProject_NoProjectIsOpen_UserCancelOnNewInfo()
        {
            Assert.IsFalse(CreateNewProject(false), "Create New Project should return false");
            Assert.IsNull(m_ProjectManager.Project, "No project should have been created");
            Assert.AreEqual(false, m_ProjectManager.IsProjectOpen);
        }

        [Test]
        public void NewProject_EventsFired()
        {
            m_ProjectCreatedEventCalled = false;
            m_ProjectOpenedEventCalled = false;

            m_Project.StartupScreen.Returns(string.Empty);
            m_WindowServiceIde.EnableToolWindowsAndRibbon(Arg.Any<bool>());

            Assert.IsTrue(CreateNewProject(true), "Create New Project should return true");

            Assert.IsTrue(m_ProjectCreatedEventCalled, "NewProject should fire ProjectCreated event!");
            Assert.IsTrue(m_ProjectOpenedEventCalled, "NewProject_EventsFired should fire ProjectOpened event!");
        }

        [Test]
        public void NewProject_CloseProjectReturnsFalse()
        {
            m_ProjectManagerMock.CloseProject(false).Returns(false);

            Assert.IsFalse(m_ProjectManager.NewProjectUI(), "NewProject should return false");
            Assert.IsNull(m_ProjectManager.Project, "No project should have been created");
        }

        [Test]
        public void CloseProject_WithNoProject()
        {
            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        private void SetupProjectInProjectManager(bool dirtyProject)
        {
            m_Project.IsDirty.Returns(dirtyProject);

            SetupProject();
        }

        private void SetupProject()
        {
            m_ProjectManager.Project = m_Project;
        }

        [Test]
        public void CloseProject_WithUndirtyProject()
        {
            SetupProjectInProjectManager(false);

            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserOksSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.ShowSaveProjectQuestion().Returns(DialogResult.Yes);
            m_ProjectManagerMock.SaveProject().Returns(true);

            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserAnswersNoToSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.ShowSaveProjectQuestion().Returns(DialogResult.No);

            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserCancelsSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.ShowSaveProjectQuestion().Returns(DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.CloseProject(false), "CloseProject should return false if user cancels");
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserOksSaveQuestion_SaveFails()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.ShowSaveProjectQuestion().Returns(DialogResult.Yes);

            m_ProjectManagerMock.SaveProject().Returns(false);

            Assert.IsFalse(m_ProjectManager.CloseProject(false), "CloseProject should return false if Save fails");
        }

        [Test]
        public void SaveProject_WithNoProject()
        {
            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithUndirtyProject()
        {
            SetupProjectInProjectManager(false);

            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithDirtyProject_SavedBefore()
        {
            string folderName = @"C:\";
            string fileName = m_Project1neo;
            SetupProjectInProjectManager(true);

            m_Project.FolderPath.Returns(folderName);
            m_Project.Filename.Returns(fileName);
            m_ProjectFactory.SaveProject(m_Project, Path.Combine(folderName, fileName)).Returns(true);

            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithDirtyProject_NotSavedBefore_SaveAsReturnsTrue()
        {
            SetupProjectInProjectManager(true);

            m_Project.Filename.Returns(string.Empty);
            m_ProjectManagerMock.SaveProjectAs().Returns(true);

            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithDirtyProject_NotSavedBefore_SaveAsReturnsFalse()
        {
            SetupProjectInProjectManager(true);

            m_Project.Filename.Returns(string.Empty);
            m_ProjectManagerMock.SaveProjectAs().Returns(false);

            Assert.IsFalse(m_ProjectManager.SaveProject(), "SaveProject should return false if SaveProjectAs does");
        }

        [Test]
        public void SaveProject_ProjectSavedEventFired()
        {
            m_ProjectSavedEventCalled = false;
            SetupProjectInProjectManager(true);

            string fileName = m_Project1neo;
            string folderName = @"C:\";

            m_Project.Filename.Returns(fileName);
            m_Project.FolderPath.Returns(folderName);
            m_ProjectFactory.SaveProject(m_Project, Path.Combine(folderName, fileName)).Returns(true);

            m_ProjectManager.SaveProject();

            Assert.IsTrue(m_ProjectSavedEventCalled, "Save returning true should fire event");
        }

        [Test]
        public void SaveProjectAs_UserCancels()
        {
            ExpectSaveAsDialogForSaveAsTest(m_Project1neo, m_ProjectFolderPath, m_Project2FolderPath, DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_InvalidFileName_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string invalidNewFilePath = Path.Combine(m_ProjectFolderPath, $"Controller1{m_ProjectExtension}");

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, invalidNewFilePath);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_FolderPathTooLong_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string invalidNewFilePath = Path.Combine(m_Project2FolderPath, new string('a', 201), m_Project2neo);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, invalidNewFilePath);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_SubdirectoryOfExistingProjectDirectory_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = Path.Combine(existingFolderPath, "subdir");
            string newFilePath = Path.Combine(newFolderPath, m_Project2neo);

            m_DirectoryHelper.Exists(existingFolderPath).Returns(true);
            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath, DialogResult.Yes);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath);
            ExpectSaveAndReopenForSaveAsTest(existingFileName, existingFolderPath, newFilePath);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_SubdirectoryOfExistingProjectDirectoryUserCancels_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = Path.Combine(existingFolderPath, "subdir");
            string newFilePath = Path.Combine(newFolderPath, m_Project2neo);

            m_DirectoryHelper.Exists(existingFolderPath).Returns(true);
            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath, DialogResult.No);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToNewDirectory_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(m_Project2FolderPath, m_Project2neo);

            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath);
            ExpectSaveAndReopenForSaveAsTest(existingFileName, existingFolderPath, newFilePath);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToNewNestedDirectory_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(m_Project2FolderPath, m_Project2neo);

            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath, DialogResult.Yes);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath);
            ExpectSaveAndReopenForSaveAsTest(existingFileName, existingFolderPath, newFilePath);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToNewNestedDirectoryUserCancels_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(m_Project2FolderPath, m_Project2neo);

            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath, DialogResult.No);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToDifferentDirectoryWithoutExistingProject_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(newFolderPath, m_Project2neo);

            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath);
            ExpectSaveAndReopenForSaveAsTest(existingFileName, existingFolderPath, newFilePath);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToDifferentDirectoryWithExistingProject_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(newFolderPath, m_Project2neo);

            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath, DialogResult.Yes);
            ExpectSaveAndReopenForSaveAsTest(existingFileName, existingFolderPath, newFilePath);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToDifferentDirectoryWithExistingProjectUserCancels_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(newFolderPath, m_Project2neo);

            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(x => null);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);
            ExpectPrepareTargetDirectoryForSaveAsTest(newFolderPath, DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToCurrentDirectory_ShouldReturnTrue()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string existingFilePath = Path.Combine(existingFolderPath, existingFileName);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, existingFilePath);
            ConfirmSaveAsWarning(DialogResult.Yes);
            ExpectSaveForSaveAsTest(existingFileName, existingFolderPath);
            m_Project.IsDirty.Returns(true);

            Assert.IsTrue(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_ToCurrentDirectoryUserCancels_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, existingFolderPath, DialogResult.No);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }

        [Test]
        public void SaveProjectAs_WithReadOnlyFiles_ShouldReturnFalse()
        {
            string existingFolderPath = m_ProjectFolderPath;
            string existingFileName = m_Project1neo;
            string newFolderPath = m_Project2FolderPath;
            string newFilePath = Path.Combine(m_Project2FolderPath, m_Project2neo);
            IEnumerable<FileInfo> fileInfos = new[] { new FileInfo("mockFile") };

            m_DirectoryHelper.Exists(newFolderPath).Returns(false);
            m_DirectoryHelper.GetReadOnlyFiles(existingFolderPath).Returns(fileInfos);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }
        [Test]
        public void OpenProjectUI_NoProject_UserCancelsOpenProjectDialog()
        {
            m_ProjectManagerUI.ShowOpenProjectDialog().Returns(DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.OpenProject(), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectUI_WithDirtyProject_UserCancelsOpenProjectDialog()
        {
            m_ProjectManagerUI.ShowOpenProjectDialog().Returns(DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.OpenProject(), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectUI_UserOKsOpenProjectDialog()
        {
            string filename = Path.Combine(m_ProjectFolderPath , m_Project2neo);

            m_ProjectManagerUI.ShowOpenProjectDialog().Returns(DialogResult.OK);
            m_ProjectManagerUI.OpenFilename.Returns(filename);
            m_ProjectManagerMock.OpenProject(filename).Returns(true);

            Assert.IsTrue(m_ProjectManager.OpenProject(), "OpenProject should return true");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_CloseOldProjectReturnsFalse()
        {
            SetupProject();

            m_Project.Filename.Returns(m_Project1neo);
            m_ProjectManagerMock.CloseProject(false).Returns(false);

            Assert.IsFalse(m_ProjectManager.OpenProject(m_Project2neo), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectFilename_EventsFired()
        {
            m_ProjectLoadedEventCalled = false;
            m_ProjectOpenedEventCalled = false;
            SetupProject();

            m_Project.Filename.Returns(m_Project1neo);

            string filename = Path.Combine(m_ProjectFolderPath, m_Project2neo);

            m_ProjectManagerMock.CloseProject(false).Returns(true);
            m_ProjectManagerMock.IsProjectSupported(filename).Returns(true);

            ExpectProjectLoad(filename);
            m_ProjectManager.OpenProject(filename);

            Assert.IsTrue(m_ProjectLoadedEventCalled, "OpenProject should fire ProjectLoaded event!");
            Assert.IsTrue(m_ProjectOpenedEventCalled, "OpenProject should fire ProjectOpened event!");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_CloseOldProjectOK()
        {
            SetupProject();
            m_Project.Filename.Returns(m_Project1neo);

            string filename = Path.Combine(m_ProjectFolderPath, m_Project2neo);

            m_ProjectManagerMock.CloseProject(false).Returns(true);
            m_ProjectManagerMock.IsProjectSupported(filename).Returns(true);

            ExpectProjectLoad(filename);

            Assert.IsTrue(m_ProjectManager.OpenProject(filename), "OpenProject should return true");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_UserCancelsReload()
        {
            string filename = m_Project1neo;

            SetupProject();

            m_Project.Filename.Returns(filename);
            m_ProjectManagerUI.ShowReloadProjectQuestion().Returns(DialogResult.Cancel);


            Assert.IsFalse(m_ProjectManager.OpenProject(filename), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_UserOKsReload()
        {
            string filename = Path.Combine(m_ProjectFolderPath, m_Project1neo);

            SetupProject();

            m_Project.Filename.Returns(filename);
            m_ProjectManagerUI.ShowReloadProjectQuestion().Returns(DialogResult.OK);
            m_ProjectManagerMock.CloseProject(false).Returns(true);
            m_ProjectManagerMock.IsProjectSupported(filename).Returns(true);

            ExpectProjectLoad(filename);

            Assert.IsTrue(m_ProjectManager.OpenProject(filename), "OpenProject should return true");
        }

        private void ExpectProjectLoad(string filename)
        {
            ExpectProjectLoad(filename, m_Project);
        }

        private IProject ExpectLoadProjectNew(string filename)
        {
            IProject project = Substitute.For<IProject>();
            project.FeatureDependencies.Returns(Enumerable.Empty<IFeatureDependency>());
            ExpectProjectLoad(filename, project);
            return project;
        }

        private void ExpectProjectLoad(string filename, IProject project)
        {
            bool isProjectDirtyByConversion;
            m_ProjectFactory.OpenProject(filename, out isProjectDirtyByConversion).Returns(project);
            m_ProjectFactory.CheckProjectExists(filename).Returns(true);
            project.RemoveUnusedProjectReferences();
            project.Filename.Returns(Path.GetFileName(filename));
            project.FolderPath.Returns(Path.GetDirectoryName(filename));
            project.StartupScreen.Returns(string.Empty);
        }

        [Test]
        public void ImportFile_NoProjectOpen()
        {
            Assert.IsFalse(m_ProjectManager.ImportFile(), "OpenProject should return false");
        }

        [Test]
        public void ImportFile_UserCancelsImportFileDialog()
        {
            SetupProject();

            m_ProjectManagerUI.ShowImportFileDialog().Returns(DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.ImportFile(), "ImportFile should return false");
        }


        [Test]
        public void RebuildProjectBuildsTheProject()
        {
            m_ProjectManagerMock.BuildProject(true).Returns(true);

            m_ProjectManager.RebuildProject();
        }

        [Test]
        public void RebuildDeletesTempAndBuildFilesFolders()
        {
            m_ProjectManagerMock.BuildProject(true).Returns(true);

            m_ProjectManager.RebuildProject();

            m_DirectoryHelper.Received().DeleteIfExists(m_TargetInfo.TempPath);
            m_DirectoryHelper.Received().DeleteIfExists(m_TargetInfo.BuildFilesPath);
        }

        [Test]
        public void RebuildThatFailesBecauseOfUnauthorizedAccessErrorsReportsBackToTheUser()
        {
            m_ProjectManagerMock.BuildProject(true).Returns(x => throw new UnauthorizedAccessException("Fail!"));

            m_ProjectManager.RebuildProject();

            AssertErrorMessageBoxWasCalled("Fail!");
        }

        [Test]
        public void RebuildThatFailesBecauseOfIOErrorsReportsBackToTheUser()
        {
            m_ProjectManagerMock.BuildProject(true).Returns(x => throw new IOException("Fail!"));

            m_ProjectManager.RebuildProject();

            AssertErrorMessageBoxWasCalled("Fail!");
        }

        private void AssertErrorMessageBoxWasCalled(string exceptionMessage)
        {
            m_MessageBoxService.Received().Show(Arg.Is(exceptionMessage), Arg.Any<string>(),
               Arg.Is(MessageBoxButtons.OK), Arg.Is(MessageBoxIcon.Error), Arg.Any<DialogResult>());
        }

        private void ExpectSaveAsDialogForSaveAsTest(
            string existingFileName,
            string existingFolderPath,
            string newFilePath,
            DialogResult dialogResult = DialogResult.OK)
        {
            SetupProject();

            m_Project.Filename.Returns(existingFileName);
            m_Project.FolderPath.Returns(existingFolderPath);

            string parentFolderPath = Path.GetDirectoryName(existingFolderPath);
            m_ProjectManagerUI.ShowSaveAsDialog(existingFileName, parentFolderPath).Returns(dialogResult);

            if (dialogResult != DialogResult.OK)
                return;

            m_Project.FileExtension.Returns(m_ProjectExtension);
            m_Project.ProjectItems.Returns(x => null);
            m_Project.GetDesignerProjectItems().Returns(new List<IDesignerProjectItem>().ToArray());

            m_ProjectManagerUI.SaveAsFilename.Returns(newFilePath);
        }

        private void ExpectCheckParentDirectoryForSaveAsTest(
            string newFolderPath,
            DialogResult createNestedSubfolderDialogResult = DialogResult.Ignore)
        {
            string parentFolderPath = Path.GetDirectoryName(newFolderPath);
            m_DirectoryHelper.Exists(parentFolderPath).Returns(true);

            bool projectFileExists = createNestedSubfolderDialogResult != DialogResult.Ignore;

            m_DirectoryHelper.GetFiles(parentFolderPath, $"*{m_ProjectExtension}")
                .Returns(projectFileExists ? new[] { $"abc{m_ProjectExtension}" } : new string[] {});

            if (!projectFileExists)
                return;

            ConfirmSaveAsWarning(createNestedSubfolderDialogResult);
        }

        private void ExpectPrepareTargetDirectoryForSaveAsTest(
            string newFolderPath,
            DialogResult overwriteProjectDialogResult = DialogResult.Ignore)
        {
            m_DirectoryHelper.Exists(newFolderPath).Returns(true);

            if (overwriteProjectDialogResult == DialogResult.Ignore)
            {
                m_DirectoryHelper.GetFiles(newFolderPath, $"*{m_ProjectExtension}")
                    .Returns(new string[] { });
                return;
            }

            string projectFileNameToBeOverwritten = $"abc{m_ProjectExtension}";
            m_DirectoryHelper.GetFiles(newFolderPath, $"*{m_ProjectExtension}")
                .Returns(new[] { $"abc{m_ProjectExtension}" });
            m_DirectoryHelper.GetFiles(newFolderPath)
                .Returns(new[] { $"abc{m_ProjectExtension}" });

            ConfirmSaveAsWarning(overwriteProjectDialogResult);

            if (overwriteProjectDialogResult == DialogResult.Yes)
                m_FileHelper.Delete(projectFileNameToBeOverwritten);
        }

        private void ConfirmSaveAsWarning(DialogResult dialogResult)
        {
            m_MessageBoxService.Show(
                        Arg.Any<string>(),
                        Arg.Is<string>(TextsIde.ConfirmReplace),
                        Arg.Any<MessageBoxButtons>(),
                        Arg.Any<MessageBoxIcon>(),
                        Arg.Any<MessageBoxDefaultButton>(),
                        Arg.Any<DialogResult>())
                .Returns(dialogResult);
        }

        private void ExpectSaveForSaveAsTest(
            string existingFileName,
            string existingFolderPath)
        {
            //Called in DoSaveProject()
            m_ProjectFactory.SaveProject(m_Project, Path.Combine(existingFolderPath, existingFileName)).Returns(true);
        }

        private void ExpectSaveAndReopenForSaveAsTest(
            string existingFileName,
            string existingFolderPath,
            string newFilePath)
        {
            ExpectSaveForSaveAsTest(existingFileName, existingFolderPath);

            IProject newProject = ExpectLoadProjectNew(newFilePath);

            //Called in DoSaveProject()
            m_ProjectFactory.SaveProject(newProject, newFilePath).Returns(true);
        }
    }
}
