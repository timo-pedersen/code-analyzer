using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Core.Api.Feature;
using Core.Api.Platform;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Build;
using Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Storage;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectFactoryTest
    {
        private const string TestFileName = "test.xml";
        private const string XmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml></xml>";
        private readonly string m_TestFile = Path.Combine(TestHelper.CurrentDirectory, TestFileName);

        private XmlDocument m_XmlContentDoc;

        private IXmlConverterService m_XmlConverterService;
        private IBrandService m_BrandService;
        private IBuildService m_BuildService;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            m_XmlContentDoc = new XmlDocument();
            m_XmlContentDoc.LoadXml(XmlContent);
            TestHelper.ClearServices();

            File.WriteAllText(m_TestFile, XmlContent);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            File.Delete(m_TestFile);
        }

        [SetUp]
        public void SetUp()
        {
            m_XmlConverterService = TestHelper.AddServiceStub<IXmlConverterService>();
            TestHelper.AddServiceStub<IMessageBoxServiceIde>();
            m_BrandService = TestHelper.AddServiceStub<IBrandService>();
            var storageService = TestHelper.AddServiceStub<IStorageService>();

            m_BrandService.Stub(x => x.BrandName).Return("iX");
            storageService.Stub(x => x.CreateProviderSettings(null, (int)TargetPlatform.Windows, (int)TargetPlatformVersion.NotApplicable))
                .IgnoreArguments()
                .Return(new LocallyHostedProjectStorageProviderSettings());

            TestHelperExtensions.AddServiceToolManager(false);

            TestHelper.AddService<IProjectDefaultSettingsService>(new ProjectDefaultSettingsService());

            var featureSecurityServiceIde = TestHelper.AddServiceStub<IFeatureSecurityServiceIde>();
            featureSecurityServiceIde.Stub(inv => inv.GetAllFeatures().OfType<ISecuredFeature>()).Return(Enumerable.Empty<ISecuredFeature>());
            featureSecurityServiceIde.Stub(x => x.GetAllActiveFeatures()).Return(Enumerable.Empty<ISecuredFeature>());

            TestHelper.AddServiceStub<IIDEOptionsService>();

            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());

            m_BuildService = TestHelper.AddServiceStub<IBuildService>();
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
        public void OpenProjectWithEqualVersionAsDesignerOpensProject()
        {
            // ARRANGE
            Version openingProjectConverterVersion = new(1, 0, 0);
            bool isProjectConverted = false;
            bool isGenerationConverted = false;

            m_XmlConverterService.Stub(x => x.GetDocumentProductVersion(m_TestFile)).Return(openingProjectConverterVersion);
            m_XmlConverterService.Stub(x => x.ConvertProjectIfNeeded(m_TestFile, ref isProjectConverted, ref isGenerationConverted)).Return(true);
            IObjectSerializer objectSerializer = MockRepository.GenerateMock<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = MockRepository.GenerateMock<IObjectSerializerFactoryIde>();
            IProject project = MockRepository.GenerateMock<IProject>();

            objectSerializerFactory.Expect(x => x.GetSerializer()).Return(objectSerializer);
            objectSerializer.Expect(x => x.DeseralizeFile(m_TestFile)).Return(project);
            project.Expect(x => x.Load());
            project.Expect(x => x.Filename = TestFileName);

            project.Expect(x => x.FeatureDependencies.Where(feature => feature.IsEssential)).Return(Enumerable.Empty<IFeatureDependency>());
            project.Expect(x => x.ProjectItems).Return(new IProjectItem[] { new ProjectItem("Functions") });

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            // ACT
            IProject loadedProject = projectFactory.OpenProject(m_TestFile, out bool _);

            // ASSERT
            Assert.IsNotNull(project, "Project is null after Load");
            Assert.AreSame(loadedProject, project, "OpenProject does not return the correct project");

            objectSerializerFactory.VerifyAllExpectations();
            objectSerializer.VerifyAllExpectations();
            project.VerifyAllExpectations();
        }

        [Test]
        public void OpenProjectWithLowerVersionThanDesignerOpensProject()
        {
            // ARRANGE
            Version openingProjectConverterVersion = new(1, 0, 0);
            bool isProjectConverted = false;
            bool isGenerationConverted = false;

            m_XmlConverterService.Stub(x => x.GetDocumentProductVersion(m_TestFile)).Return(openingProjectConverterVersion);
            m_XmlConverterService.Stub(x => x.ConvertProjectIfNeeded(m_TestFile, ref isProjectConverted, ref isGenerationConverted)).Return(true);

            IObjectSerializer objectSerializer = MockRepository.GenerateMock<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = MockRepository.GenerateMock<IObjectSerializerFactoryIde>();
            IProject project = MockRepository.GenerateMock<IProject>();

            objectSerializerFactory.Expect(x => x.GetSerializer()).Return(objectSerializer);
            objectSerializer.Expect(x => x.DeseralizeFile(m_TestFile)).Return(project);
            project.Expect(x => x.Load());
            project.Expect(x => x.Filename = TestFileName);

            project.Stub(x => x.FeatureDependencies.Where(feature => feature.IsEssential)).Return(Enumerable.Empty<IFeatureDependency>());
            project.Stub(x => x.ProjectItems).Return(new IProjectItem[] { new ProjectItem("Functions") });

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            // ACT
            IProject loadedProject = projectFactory.OpenProject(m_TestFile, out bool _);

            // ASSERT
            Assert.IsNotNull(project, "Project is null after Load");
            Assert.AreSame(loadedProject, project, "OpenProject does not return the correct project");

            objectSerializerFactory.VerifyAllExpectations();
            objectSerializer.VerifyAllExpectations();
            project.VerifyAllExpectations();
        }

        [Test]
        public void OpenProjectWithHigherVersionThanDesignerDoesNotOpenProject()
        {
            // ARRANGE
            IObjectSerializer objectSerializer = MockRepository.GenerateMock<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = MockRepository.GenerateMock<IObjectSerializerFactoryIde>();
            IProject project = MockRepository.GenerateMock<IProject>();

            objectSerializerFactory.Stub(x => x.GetSerializer()).Return(objectSerializer);
            objectSerializer.Stub(x => x.DeseralizeFile(m_TestFile)).Return(project);

            project.Stub(x => x.FeatureDependencies.Where(feature => feature.IsEssential)).Return(Enumerable.Empty<IFeatureDependency>());
            project.Stub(x => x.ProjectItems).Return(new IProjectItem[] { new ProjectItem("Functions") });

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            // ACT
            IProject loadedProject = projectFactory.OpenProject(m_TestFile, out bool _);

            // ASSERT
            Assert.IsNull(loadedProject, "Loaded project is not null");
        }

        [Test]
        public void SaveProject()
        {
            const string saveFilename = "test.xml";

            m_XmlConverterService.Stub(x => x.LastConverterVersion).Return(new Version());
            IObjectSerializer objectSerializer = MockRepository.GenerateMock<IObjectSerializer>();
            IObjectSerializerFactoryIde objectSerializerFactory = MockRepository.GenerateMock<IObjectSerializerFactoryIde>();
            IProject projectMock = MockRepository.GenerateMock<IProject>();

            projectMock.Expect(x => x.GetDesignerProjectItems()).Return(new List<IDesignerProjectItem>().ToArray());
            objectSerializerFactory.Expect(x => x.GetSerializer()).Return(objectSerializer);
            objectSerializer.Expect(x => x.SerializeToFile(projectMock, saveFilename));

            IProjectFactory projectFactory = new ProjectFactory(objectSerializerFactory.ToILazy());

            projectFactory.SaveProject(projectMock, saveFilename);

            projectMock.VerifyAllExpectations();
            objectSerializerFactory.VerifyAllExpectations();
            objectSerializer.VerifyAllExpectations();
        }
    }
}
