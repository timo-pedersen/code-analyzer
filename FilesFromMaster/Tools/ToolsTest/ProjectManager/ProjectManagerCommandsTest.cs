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
using Neo.ApplicationFramework.Interfaces.Build;
using Neo.ApplicationFramework.Interfaces.WindowManagement;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Wizards.CreateNewProjectWizard;
using NUnit.Framework;
using Rhino.Mocks;
#region vNext
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.Tools.Build.DotNetRunner;
using System.Threading.Tasks;
#endregion

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
        private FileHelper m_FileHelper;
        private DirectoryHelper m_DirectoryHelper;
        private ITypeListService m_TypeListService;
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
            m_TargetInfo = MockRepository.GenerateStub<ITargetInfo>();
            m_TargetInfo.ProjectFilesPath = "ProjectFiles";
            m_TargetInfo.ProjectPath = "ProjectPath";
            m_TargetInfo.TempPath = "TempPath";
        }

        [SetUp]
        public void SetupProjectManager()
        {
            IBrandService brandService = MockRepository.GenerateStub<IBrandService>();
            brandService.Stub(x => x.FileExtension).Return(m_DesignerExtension);
            brandService.Stub(x => x.ProjectFileExtension).Return(m_DesignerExtension);
            TestHelper.AddService(brandService);

            IDesignerMetadata designerMetadataStub = MockRepository.GenerateStub<IDesignerMetadata>();
            IDesignerInfo designerInfoStub = MockRepository.GenerateStub<IDesignerInfo>();
            designerInfoStub.Stub(x => x.Metadata).Return(designerMetadataStub);
            designerInfoStub.Stub(x => x.Type).Return(typeof(DataSourceContainer));

            List<IDesignerInfo> designersList = new List<IDesignerInfo>
            {
                designerInfoStub
            };

            m_TypeListService = MockRepository.GenerateStub<ITypeListService>();
            m_TypeListService.Stub(x => x.GetDesigners()).Return(designersList);
            TestHelper.AddService(m_TypeListService);

            var nameCreationServiceStub = MockRepository.GenerateStub<INameCreationService>();
            nameCreationServiceStub.Stub(x => x.IsValidName(Arg<string>.Is.Anything, ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), "").Dummy)).IgnoreArguments().Return(true);
            nameCreationServiceStub.Stub(x => x.IsValidFileName(Arg<string>.Is.Anything, ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), "").Dummy)).IgnoreArguments().Return(true);
            TestHelper.AddService(nameCreationServiceStub);

            TestHelper.CreateAndAddServiceStub<IWelcomeScreenGreetingService>();

            TestHelper.AddService<IInformationProgressService>(new InvisibleInformationProgressManager());
            TestHelper.AddService<IProjectManagerOpenService>(new ProjectManagerOpenService());
            TestHelper.UseTestWindowThreadHelper = true;

            m_ProjectFactory = MockRepository.GenerateMock<IProjectFactory>();
            m_Project = MockRepository.GenerateMock<IProject>();
            m_Project.Stub(inv => inv.FeatureDependencies).Return(Enumerable.Empty<IFeatureDependency>());
            m_ProjectManagerUI = MockRepository.GenerateMock<IProjectManagerUI>();
            m_ProjectManagerMock = MockRepository.GenerateMock<IProjectManager>();

            m_FileHelper = MockRepository.GenerateStub<FileHelper>();
            m_DirectoryHelper = MockRepository.GenerateStub<DirectoryHelper>();

            var namingConstraints = MockRepository.GenerateStub<INamingConstraints>();
            namingConstraints.Stub(inv => inv.IsNameLengthValid(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(true);
            namingConstraints.Stub(inv => inv.ReservedApplicationNames).Return(new HashSet<string>());
            namingConstraints.Stub(inv => inv.ReservedSystemNames).Return(new HashSet<string>());
            
            var messageBoxService = MockRepository.GenerateStub<IMessageBoxServiceIde>();

            m_ProjectManager = new ProjectManager(
                m_ProjectFactory,
                m_ProjectManagerUI,
                m_ProjectManagerMock,
                new LazyWrapper<IFeatureSecurityServiceIde>(FssFunc),
                new NameCreationService(namingConstraints),
                new LazyWrapper<ITargetService>(TargetServiceInitializer),
                new LazyWrapper<IOpcClientServiceIde>(() => MockRepository.GenerateStub<IOpcClientServiceIde>()),
                new LazyWrapper<ITerminalTargetChangeService>(() => MockRepository.GenerateStub<ITerminalTargetChangeService>()),
                MockRepository.GenerateStub<IIDEOptionsService>().ToILazy(),
                messageBoxService.ToILazy(),
                brandService.ToILazy(),
                MockRepository.GenerateStub<IBuildService>().ToILazy(),
                MockRepository.GenerateStub<IErrorListService>().ToILazy(),
                MockRepository.GenerateStub<IGapService>().ToILazy(),
                MockRepository.GenerateStub<IDotNetRunnerService>().ToILazy());

            ((ProjectManager)m_ProjectManager).LoadOnDemandService = MockRepository.GenerateStub<ILoadOnDemandService>();
            ((ProjectManager)m_ProjectManager).FileHelper = m_FileHelper;
            ((ProjectManager)m_ProjectManager).DirectoryHelper = m_DirectoryHelper;
            ((ProjectManager)m_ProjectManager).WindowService = MockRepository.GenerateStub<IWindowService>();
            ((ProjectManager)m_ProjectManager).WindowServiceIde = MockRepository.GenerateStub<IWindowServiceIde>();
            m_MessageBoxService = messageBoxService;
            m_ProjectManager.ProjectOpened += OnProjectOpened;
            m_ProjectManager.ProjectLoaded += OnProjectLoaded;
            m_ProjectManager.ProjectCreated += OnProjectCreated;
            m_ProjectManager.ProjectSaved += OnProjectSaved;
        }

        [TearDown]
        public void VerifyMocks()
        {
            m_ProjectFactory.VerifyAllExpectations();
            m_Project.VerifyAllExpectations();
            m_ProjectManagerUI.VerifyAllExpectations();
            m_ProjectManagerMock.VerifyAllExpectations();
            m_ProjectManager.ProjectOpened -= OnProjectOpened;
            m_ProjectManager.ProjectSaved -= OnProjectSaved;

            TestHelper.ClearServices();
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

        [Test]
        public void NewProject_NoProjectIsOpen_UserOKOnNewInfo()
        {
            m_Project.Stub(x => x.StartupScreen).Return(string.Empty);

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

            m_Project.Stub(x => x.StartupScreen).Return(string.Empty);

            Assert.IsTrue(CreateNewProject(true), "Create New Project should return true");

            Assert.IsTrue(m_ProjectCreatedEventCalled, "NewProject should fire ProjectCreated event!");
            Assert.IsTrue(m_ProjectOpenedEventCalled, "NewProject_EventsFired should fire ProjectOpened event!");
        }

        [Test]
        public void NewProject_CloseProjectReturnsFalse()
        {
            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(false).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.NewProjectUI(), "NewProject should return false");
            Assert.IsNull(m_ProjectManager.Project, "No project should have been created");
        }

        [Test]
        public void CloseProject_WithNoProject()
        {
            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithUndirtyProject()
        {
            SetupProjectInProjectManager(false);

            m_Project.Expect(x => x.Close()).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();
            
            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserOksSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.Expect(x => x.ShowSaveProjectQuestion()).Return(DialogResult.Yes).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.SaveProject()).Return(true).Repeat.Once();

            m_Project.Expect(x => x.Close()).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();
            
            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserAnswersNoToSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.Stub(x => x.ShowSaveProjectQuestion()).Return(DialogResult.No);
            m_Project.Expect(x => x.Close()).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();

            Assert.IsTrue(m_ProjectManager.CloseProject(false));
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserCancelsSaveQuestion()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.Stub(x => x.ShowSaveProjectQuestion()).Return(DialogResult.Cancel);

            Assert.IsFalse(m_ProjectManager.CloseProject(false), "CloseProject should return false if user cancels");
        }

        [Test]
        public void CloseProject_WithDirtyProject_UserOksSaveQuestion_SaveFails()
        {
            SetupProjectInProjectManager(true);

            m_ProjectManagerUI.Stub(x => x.ShowSaveProjectQuestion()).Return(DialogResult.Yes);

            m_ProjectManagerMock.Stub(x => x.SaveProject()).Return(false);

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

            m_Project.Stub(x => x.FolderPath).Return(folderName);
            m_Project.Stub(x => x.Filename).Return(fileName);
            m_ProjectFactory.Expect(x => x.SaveProject(m_Project, Path.Combine(folderName, fileName))).Return(true).Repeat.Once();

            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithDirtyProject_NotSavedBefore_SaveAsReturnsTrue()
        {
            SetupProjectInProjectManager(true);

            m_Project.Expect(x => x.Filename).Return(string.Empty).Repeat.Once();
            m_Project.Stub(x => x.FolderPath = Arg<string>.Is.Anything);
            m_ProjectManagerMock.Expect(x => x.SaveProjectAs()).Return(true).Repeat.Once();

            Assert.IsTrue(m_ProjectManager.SaveProject(), "SaveProject should return true");
        }

        [Test]
        public void SaveProject_WithDirtyProject_NotSavedBefore_SaveAsReturnsFalse()
        {
            SetupProjectInProjectManager(true);

            m_Project.Expect(x => x.Filename).Return(string.Empty).Repeat.Once();
            m_Project.Stub(x => x.FolderPath = Arg<string>.Is.Anything);
            m_ProjectManagerMock.Expect(x => x.SaveProjectAs()).Return(false).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.SaveProject(), "SaveProject should return false if SaveProjectAs does");
        }

        [Test]
        public void SaveProject_ProjectSavedEventFired()
        {
            m_ProjectSavedEventCalled = false;
            SetupProjectInProjectManager(true);

            string fileName = m_Project1neo;
            string folderName = @"C:\";

            m_Project.Stub(x => x.Filename).Return(fileName);
            m_Project.Stub(x => x.FolderPath).Return(folderName);
            m_ProjectFactory.Expect(x => x.SaveProject(m_Project, Path.Combine(folderName, fileName))).Return(true).Repeat.Once();

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

            m_DirectoryHelper.Stub(x => x.Exists(existingFolderPath)).Return(true);
            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.Exists(existingFolderPath)).Return(true);
            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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

            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(null);

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
            m_Project.Stub(x => x.IsDirty).Return(true);

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

            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(false);
            m_DirectoryHelper.Stub(x => x.GetReadOnlyFiles(existingFolderPath)).Return(fileInfos);

            ExpectSaveAsDialogForSaveAsTest(existingFileName, existingFolderPath, newFilePath);
            ExpectCheckParentDirectoryForSaveAsTest(newFolderPath);

            Assert.IsFalse(m_ProjectManager.SaveProjectAs());
        }
        [Test]
        public void OpenProjectUI_NoProject_UserCancelsOpenProjectDialog()
        {
            m_ProjectManagerUI.Expect(x => x.ShowOpenProjectDialog()).Return(DialogResult.Cancel).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.OpenProject(), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectUI_WithDirtyProject_UserCancelsOpenProjectDialog()
        {
            m_ProjectManagerUI.Expect(x => x.ShowOpenProjectDialog()).Return(DialogResult.Cancel).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.OpenProject(), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectUI_UserOKsOpenProjectDialog()
        {
            string filename = Path.Combine(m_ProjectFolderPath , m_Project2neo);

            m_ProjectManagerUI.Expect(x => x.ShowOpenProjectDialog()).Return(DialogResult.OK).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.OpenFilename).Return(filename).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.OpenProject(filename)).Return(true).Repeat.Once();

            Assert.IsTrue(m_ProjectManager.OpenProject(), "OpenProject should return true");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_CloseOldProjectReturnsFalse()
        {
            SetupProject();

            m_Project.Expect(x => x.Filename).Return(m_Project1neo).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(false).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.OpenProject(m_Project2neo), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectFilename_EventsFired()
        {
            m_ProjectLoadedEventCalled = false;
            m_ProjectOpenedEventCalled = false;
            SetupProject();

            m_Project.Expect(x => x.Filename).Return(m_Project1neo).Repeat.Once();

            string filename = Path.Combine(m_ProjectFolderPath, m_Project2neo);

            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(true).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.IsProjectSupported(filename)).Return(true).Repeat.Once();

            ExpectProjectLoad(filename);
            m_ProjectManager.OpenProject(filename);

            Assert.IsTrue(m_ProjectLoadedEventCalled, "OpenProject should fire ProjectLoaded event!");
            Assert.IsTrue(m_ProjectOpenedEventCalled, "OpenProject should fire ProjectOpened event!");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_CloseOldProjectOK()
        {
            SetupProject();
            m_Project.Expect(x => x.Filename).Return(m_Project1neo).Repeat.Once();

            string filename = Path.Combine(m_ProjectFolderPath, m_Project2neo);

            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(true).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.IsProjectSupported(filename)).Return(true).Repeat.Once();

            ExpectProjectLoad(filename);

            Assert.IsTrue(m_ProjectManager.OpenProject(filename), "OpenProject should return true");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_UserCancelsReload()
        {
            string filename = m_Project1neo;

            SetupProject();

            m_Project.Expect(x => x.Filename).Return(filename).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ShowReloadProjectQuestion()).Return(DialogResult.Cancel).Repeat.Once();


            Assert.IsFalse(m_ProjectManager.OpenProject(filename), "OpenProject should return false");
        }

        [Test]
        public void OpenProjectFilename_WithDirtyProject_UserOKsReload()
        {
            string filename = Path.Combine(m_ProjectFolderPath, m_Project1neo);

            SetupProject();

            m_Project.Expect(x => x.Filename).Return(filename).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ShowReloadProjectQuestion()).Return(DialogResult.OK).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(true).Repeat.Once();
            m_ProjectManagerMock.Expect(x => x.IsProjectSupported(filename)).Return(true).Repeat.Once();

            ExpectProjectLoad(filename);

            Assert.IsTrue(m_ProjectManager.OpenProject(filename), "OpenProject should return true");
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

            m_ProjectManagerUI.Expect(x => x.ShowImportFileDialog()).Return(DialogResult.Cancel).Repeat.Once();

            Assert.IsFalse(m_ProjectManager.ImportFile(), "ImportFile should return false");
        }

        [Test]
        public async Task RebuildProjectAsyncBuildsTheProject()
        {
            m_ProjectManagerMock.Expect(x => x.BuildProjectAsync(true)).Return(Task.FromResult(true)).Repeat.Once();

            await m_ProjectManager.RebuildProjectAsync();
        }

        [Test]
        public async Task RebuildProjectAsyncDeletesTempAndBuildFilesFolders()
        {
            m_ProjectManagerMock.Expect(x => x.BuildProjectAsync(true)).Return(Task.FromResult(true)).Repeat.Once();

            await m_ProjectManager.RebuildProjectAsync();

            m_DirectoryHelper.AssertWasCalled(x => x.DeleteIfExists(Path.Combine(m_TargetInfo.ProjectPath, DirectoryConstants.TempFolderName)));
            m_DirectoryHelper.AssertWasCalled(x => x.DeleteIfExists(Path.Combine(m_TargetInfo.ProjectPath, DirectoryConstants.BuildFilesFolderName)));
        }

        [Test]
        public void RebuildProjectAsyncThatFailesBecauseOfUnauthorizedAccessErrorsReportsBackToTheUser()
        {

            m_ProjectManagerMock.Stub(x => x.BuildProjectAsync(true)).Throw(new UnauthorizedAccessException("Fail!"));

            m_ProjectManager.RebuildProjectAsync();

            AssertErrorMessageBoxWasCalled("Fail!");
        }

        [Test]
        public void RebuildProjectAsyncThatFailesBecauseOfIOErrorsReportsBackToTheUser()
        {
            m_ProjectManagerMock.Stub(x => x.BuildProjectAsync(true)).Throw(new IOException("Fail!"));

            m_ProjectManager.RebuildProjectAsync();

            AssertErrorMessageBoxWasCalled("Fail!");
        }

        #region HelperMethods

        private void AssertErrorMessageBoxWasCalled(string exceptionMessage)
        {
            m_MessageBoxService.AssertWasCalled(x => x.Show(Arg<string>.Is.Equal(exceptionMessage), Arg<string>.Is.Anything,
               Arg<MessageBoxButtons>.Is.Equal(MessageBoxButtons.OK), Arg<MessageBoxIcon>.Is.Equal(MessageBoxIcon.Error), Arg<DialogResult>.Is.Anything));
        }

        private void ExpectSaveAsDialogForSaveAsTest(
            string existingFileName,
            string existingFolderPath,
            string newFilePath,
            DialogResult dialogResult = DialogResult.OK)
        {
            SetupProject();

            m_Project.Stub(x => x.Filename).Return(existingFileName);
            m_Project.Stub(x => x.FolderPath).Return(existingFolderPath);

            string parentFolderPath = Path.GetDirectoryName(existingFolderPath);
            m_ProjectManagerUI.Expect(x => x.ShowSaveAsDialog(existingFileName, parentFolderPath)).Return(dialogResult).Repeat.Once();

            if (dialogResult != DialogResult.OK)
                return;

            m_Project.Stub(x => x.FileExtension).Return(m_ProjectExtension);
            m_Project.Stub(x => x.ProjectItems).Return(null);
            m_Project.Stub(x => x.GetDesignerProjectItems()).Return(new List<IDesignerProjectItem>().ToArray());

            m_ProjectManagerUI.Stub(x => x.SaveAsFilename).Return(newFilePath);
        }

        private void ExpectCheckParentDirectoryForSaveAsTest(
            string newFolderPath,
            DialogResult createNestedSubfolderDialogResult = DialogResult.Ignore)
        {
            string parentFolderPath = Path.GetDirectoryName(newFolderPath);
            m_DirectoryHelper.Stub(x => x.Exists(parentFolderPath)).Return(true);

            bool projectFileExists = createNestedSubfolderDialogResult != DialogResult.Ignore;

            m_DirectoryHelper.Stub(x => x.GetFiles(parentFolderPath, $"*{m_ProjectExtension}"))
                .Return(projectFileExists ? new[] { $"abc{m_ProjectExtension}" } : new string[] {});

            if (!projectFileExists)
                return;

            ConfirmSaveAsWarning(createNestedSubfolderDialogResult);
        }

        private void ExpectPrepareTargetDirectoryForSaveAsTest(
            string newFolderPath,
            DialogResult overwriteProjectDialogResult = DialogResult.Ignore)
        {
            m_DirectoryHelper.Stub(x => x.Exists(newFolderPath)).Return(true);

            if (overwriteProjectDialogResult == DialogResult.Ignore)
            {
                m_DirectoryHelper.Stub(x => x.GetFiles(newFolderPath, $"*{m_ProjectExtension}"))
                    .Return(new string[] { });
                return;
            }

            string projectFileNameToBeOverwritten = $"abc{m_ProjectExtension}";
            m_DirectoryHelper.Stub(x => x.GetFiles(newFolderPath, $"*{m_ProjectExtension}"))
                .Return(new[] { $"abc{m_ProjectExtension}" });
            m_DirectoryHelper.Stub(x => x.GetFiles(newFolderPath))
                .Return(new[] { $"abc{m_ProjectExtension}" });

            ConfirmSaveAsWarning(overwriteProjectDialogResult);

            if (overwriteProjectDialogResult == DialogResult.Yes)
                m_FileHelper.Expect(x => x.Delete(projectFileNameToBeOverwritten));
        }

        private void ConfirmSaveAsWarning(DialogResult dialogResult)
        {
            m_MessageBoxService.Stub(
                    x => x.Show(
                        Arg<string>.Is.Anything,
                        Arg<string>.Is.Equal(TextsIde.ConfirmReplace),
                        Arg<MessageBoxButtons>.Is.Anything,
                        Arg<MessageBoxIcon>.Is.Anything,
                        Arg<MessageBoxDefaultButton>.Is.Anything,
                        Arg<DialogResult>.Is.Anything
                    ))
                .IgnoreArguments()
                .Return(dialogResult);
        }

        private void ExpectSaveForSaveAsTest(
            string existingFileName,
            string existingFolderPath)
        {
            //Called in DoSaveProject()
            m_ProjectFactory.Expect(x => x.SaveProject(m_Project, Path.Combine(existingFolderPath, existingFileName))).Return(true).Repeat.Once();
        }

        private void ExpectSaveAndReopenForSaveAsTest(
            string existingFileName,
            string existingFolderPath,
            string newFilePath)
        {
            ExpectSaveForSaveAsTest(existingFileName, existingFolderPath);

            //Called in DoCloseProject()
            m_Project.Expect(x => x.Close()).Repeat.Once();
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();

            IProject newProject = ExpectLoadProjectNew(newFilePath);

            //Called in DoSaveProject()
            m_ProjectFactory.Expect(x => x.SaveProject(newProject, newFilePath)).Return(true).Repeat.Once();
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

        private bool CreateNewProject(bool projectInfoDialogResult)
        {
            const string projectName = "Project1";
            IProjectSettings projectSettings = new ProjectSettings();
            projectSettings.Name = projectName;
            projectSettings.Location = Path.GetTempPath();
            projectSettings.Terminal = MockRepository.GenerateStub<ITerminal>();
            projectSettings.Terminal.Stub(x => x.PanelTypeGroup).Return(PanelTypeGroup.TxB);

            string folderPath = Path.Combine(projectSettings.Location, projectName);
            string filePath = Path.Combine(folderPath, Path.ChangeExtension(projectName, m_ProjectExtension.Replace(".", "")));

            m_ProjectManagerMock.Expect(x => x.CloseProject(false)).Return(true).Repeat.Once();

            if (projectInfoDialogResult)
            {
                m_ProjectManagerUI.Expect(x => x.ShowNewProjectWizardInfoDialog(Arg<bool>.Is.Anything)).Return(projectSettings).Repeat.Once();
            }
            else
            {
                m_ProjectManagerUI.Expect(x => x.ShowNewProjectWizardInfoDialog(Arg<bool>.Is.Anything)).Return(null).Repeat.Once();
            }

            if (projectInfoDialogResult)
            {
                m_ProjectFactory.Expect(x => x.CreateProject()).Return(m_Project).Repeat.Once();
                m_ProjectFactory.Expect(x => x.SaveProject(m_Project, filePath)).Return(true).Repeat.Once();
                m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();
                m_Project.Expect(x => x.FireItemChanged()).Repeat.Once();

                m_Project.Expect(x => x.Name = projectName).Repeat.Once();
                m_Project.Expect(x => x.FolderPath = folderPath).Repeat.AtLeastOnce();
                m_Project.Stub(x => x.FolderPath).Return(folderPath);
                m_Project.Stub(x => x.FileExtension).Return(m_ProjectExtension);
                m_Project.Expect(x => x.IsDirty).Return(true).Repeat.Once();
                m_Project.Stub(x => x.Filename).Return(Path.GetFileName(filePath));

            }

            return m_ProjectManager.NewProjectUI();
        }

        private void ExpectProjectLoad(string filename)
        {
            ExpectProjectLoad(filename, m_Project);
        }

        private IProject ExpectLoadProjectNew(string filename)
        {
            IProject project = MockRepository.GenerateMock<IProject>();
            project.Stub(x => x.SetBuildError(Arg<bool>.Is.Anything));
            project.Stub(inv => inv.FeatureDependencies).Return(Enumerable.Empty<IFeatureDependency>());
            ExpectProjectLoad(filename, project);
            return project;
        }

        private void ExpectProjectLoad(string filename, IProject project)
        {
            bool isProjectDirtyByConversion;
            m_ProjectFactory.Expect(x => x.OpenProject(filename, out isProjectDirtyByConversion)).Return(project).Repeat.Once();
            m_ProjectFactory.Expect(x => x.CheckProjectExists(filename)).Return(true).Repeat.Once();
            project.Expect(x => x.RemoveUnusedProjectReferences()).Repeat.Once();
            project.Stub(x => x.Filename).Return(Path.GetFileName(filename));
            project.Stub(x => x.FolderPath).Return(Path.GetDirectoryName(filename));
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();
            project.Expect(x => x.ResetDirty()).Repeat.Once();
            project.Stub(x => x.StartupScreen).Return(string.Empty);
        }

        private void SetupProjectInProjectManager(bool dirtyProject)
        {
            m_Project.Expect(x => x.IsDirty).Return(dirtyProject).Repeat.Once();

            SetupProject();
        }

        private void SetupProject()
        {
            m_ProjectManagerUI.Expect(x => x.ProjectRootTreeItem = Arg<IProjectTreeItem>.Is.Anything).Repeat.Once();

            m_ProjectManager.Project = m_Project;
        }

        private IFeatureSecurityServiceIde FssFunc()
        {
            var featureSecurityServiceIde = MockRepository.GenerateStub<IFeatureSecurityServiceIde>();
            featureSecurityServiceIde.Stub(inv => inv.GetAllActiveFeatures()).Return(Enumerable.Empty<ISecuredFeature>());
            featureSecurityServiceIde.Stub(inv => inv.GetAllFeatures()).Return(Enumerable.Empty<IFeature>());
            return featureSecurityServiceIde;
        }

        private ITargetService TargetServiceInitializer()
        {
            var targetService = MockRepository.GenerateStub<ITargetService>();
            targetService.Stub(ts => ts.CurrentTargetInfo).Return(m_TargetInfo);
            return targetService;
        }

        #endregion
    }
}
