using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Core.Api.Feature;
using Core.Api.Platform;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectFactoryTest
    {
        private const string OpenFilename = "test.xml";
        private const string XmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml></xml>";
        private XmlDocument XmlContentDoc;

        private IXmlConverterService m_XmlConverterService;
        private IBrandService m_BrandService;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            XmlContentDoc = new XmlDocument();
            XmlContentDoc.LoadXml(XmlContent);
            TestHelper.ClearServices();
            File.WriteAllText(OpenFilename, XmlContent);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            File.Delete(OpenFilename);
        }

        [SetUp]
        public void SetUp()
        {
            m_XmlConverterService = TestHelper.AddServiceStub<IXmlConverterService>();
            TestHelper.AddServiceStub<IMessageBoxServiceIde>();
            m_BrandService = TestHelper.AddServiceStub<IBrandService>();
            var storageService = TestHelper.AddServiceStub<IStorageService>();

            m_XmlConverterService.LastConverterVersion.Returns(new Version());
            m_BrandService.BrandName.Returns("iX");
            storageService.CreateProviderSettings(Arg.Any<string>(), Arg.Any<TargetPlatform>(), Arg.Any<TargetPlatformVersion>())
                .Returns(new LocallyHostedProjectStorageProviderSettings());

            TestHelperExtensions.AddServiceToolManager(false);

            TestHelper.AddService<IProjectDefaultSettingsService>(new ProjectDefaultSettingsService());

            var featureSecurityServiceIde = TestHelper.AddServiceStub<IFeatureSecurityServiceIde>();
            featureSecurityServiceIde.GetAllFeatures().Returns(Enumerable.Empty<IFeature>());

            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());
        }

        [TearDown]
        public void VerifyMocks()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void Create()
        {
            IProjectFactory projectFactory = new ProjectFactory();

            Assert.IsNotNull(projectFactory);
        }

        [Test]
        public void CreateProject()
        {
            IProjectFactory projectFactory = new ProjectFactory();

            IProject project = projectFactory.CreateProject();
            Assert.IsNotNull(project, "Project was not created");
        }

        [Test]
        public void OpenProjectWithEqualVersionAsDesigner()
        {
            m_XmlConverterService.GetDocumentVersion(Arg.Any<string>()).Returns(Assembly.GetExecutingAssembly().GetName().Version);

            IObjectSerializer objectSerializer = Substitute.For<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = Substitute.For<IObjectSerializerFactoryIde>();
            IProject project = Substitute.For<IProject>();
            project.FeatureDependencies.Returns(Enumerable.Empty<IFeatureDependency>());

            objectSerializerFactory.GetSerializer().Returns(objectSerializer);
            objectSerializer.DeseralizeFile(OpenFilename).Returns(project);
            objectSerializer.Load(OpenFilename).Returns(XmlContentDoc);

            IProjectItem projectItem = Substitute.For<IProjectItem>();
            projectItem.Name.Returns("Functions");
            projectItem.ProjectItems.Returns(new IProjectItem[0]);
            project.ProjectItems.Returns(new IProjectItem[] { projectItem });

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            bool hasConverterRun;
            IProject loadedProject = projectFactory.OpenProject(OpenFilename, out hasConverterRun);

            Assert.IsNotNull(project, "Project is null after Load");
            Assert.AreSame(loadedProject, project, "OpenProject does not return the correct project");

            objectSerializerFactory.Received().GetSerializer();
            objectSerializer.Received().DeseralizeFile(OpenFilename);
            objectSerializer.Received().Load(OpenFilename);
            project.Received().Filename = OpenFilename;
        }

        [Test]
        public void OpenProjectWithLowerVersionThanDesigner()
        {
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Version lowerVersion = TestHelper.GetPreviousVersion(currentVersion);

            m_XmlConverterService.GetDocumentVersion(Arg.Any<string>()).Returns(lowerVersion);

            IObjectSerializer objectSerializer = Substitute.For<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = Substitute.For<IObjectSerializerFactoryIde>();
            IProject project = Substitute.For<IProject>();
            project.FeatureDependencies.Returns(Enumerable.Empty<IFeatureDependency>());

            objectSerializerFactory.GetSerializer().Returns(objectSerializer);
            objectSerializer.DeseralizeFile(OpenFilename).Returns(project);
            objectSerializer.Load(OpenFilename).Returns(XmlContentDoc);

            IProjectItem projectItem = Substitute.For<IProjectItem>();
            projectItem.Name.Returns("Functions");
            projectItem.ProjectItems.Returns(new IProjectItem[0]);
            project.ProjectItems.Returns(new IProjectItem[] { projectItem });

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            bool isProjectDirtyByConversion;
            IProject loadedProject = projectFactory.OpenProject(OpenFilename, out isProjectDirtyByConversion);

            Assert.IsNotNull(project, "Project is null after Load");
            Assert.AreSame(loadedProject, project, "OpenProject does not return the correct project");

            objectSerializerFactory.GetSerializer();
            objectSerializer.Received().DeseralizeFile(OpenFilename);
            objectSerializer.Received().Load(OpenFilename);
            project.Received().Filename = OpenFilename;
            
        }

        [Test]
        public void OpenProjectWithHigherVersionThanDesigner()
        {
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            int currentBuild = currentVersion.Build;
            Version higherVersion = new Version(currentVersion.Major, currentVersion.Minor, ++currentBuild);

            m_XmlConverterService.GetDocumentVersion(Arg.Any<string>()).Returns(higherVersion);

            IObjectSerializer objectSerializer = Substitute.For<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = Substitute.For<IObjectSerializerFactoryIde>();
            IProject project = Substitute.For<IProject>();

            objectSerializerFactory.GetSerializer().Returns(objectSerializer);
            objectSerializer.DeseralizeFile(OpenFilename).Returns(project);

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            bool isProjectDirtyByConversion;
            IProject loadedProject = projectFactory.OpenProject(OpenFilename, out isProjectDirtyByConversion);

            Assert.IsNull(loadedProject, "Loaded project is not null");
        }

        [Test]
        public void SaveProject()
        {
            const string saveFilename = "test.xml";

            IObjectSerializer objectSerializer = Substitute.For<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = Substitute.For<IObjectSerializerFactoryIde>();
            IProject projectMock = Substitute.For<IProject>();

            projectMock.GetDesignerProjectItems().Returns(new List<IDesignerProjectItem>().ToArray());
            objectSerializerFactory.GetSerializer().Returns(objectSerializer);

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            projectFactory.SaveProject(projectMock, saveFilename);

            projectMock.Received().GetDesignerProjectItems();
            objectSerializerFactory.Received().GetSerializer();
            objectSerializer.Received().SerializeToFile(projectMock, saveFilename);
        }
    }
}
