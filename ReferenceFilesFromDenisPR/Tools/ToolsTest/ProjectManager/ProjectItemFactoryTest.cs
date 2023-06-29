using System;
using System.Reflection;
using Core.Api.Feature;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Controls.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class ProjectItemFactoryTest
    {
        private IProjectItemFactory m_ProjectItemFactory;
        private IXmlConverterService m_XmlConverterServiceStub;
        private IFeatureSecurityServiceIde m_FeatureSecurityServiceStub;
        
        [SetUp]
        public void Setup()
        {
            m_ProjectItemFactory = ProjectItemFactory.Instance;
            TestHelper.AddServiceStub<IProjectManager>();
            m_XmlConverterServiceStub = TestHelper.AddServiceStub<IXmlConverterService>();
            m_FeatureSecurityServiceStub = TestHelper.AddServiceStub<IFeatureSecurityServiceIde>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CreateGroupProjectItem()
        {
            IProjectItem projectItem = m_ProjectItemFactory.CreateGroup<GroupProjectItem>("Forms");
            Assert.IsTrue(projectItem is GroupProjectItem);
        }

        [Test]
        public void CreateDesignerProjectItem()
        {
            // Act
            IProjectItem projectItem = m_ProjectItemFactory.CreateDesigner(typeof(TestDesigner));

            // Assert
            Assert.IsTrue(projectItem is DesignerProjectItem);
            Assert.AreEqual(((DesignerProjectItem)projectItem).DesignerTypeName, typeof(TestDesigner).AssemblyQualifiedName);
            Assert.AreEqual("TestGroup", projectItem.Group);
        }

        [Test]
        public void ImportDesignerFileAndDesignerVersionDoesNotMatch()
        {
            TestHelper.AddServiceStub<IMessageBoxServiceIde>();
            TestHelper.AddServiceStub<IErrorReporterService>();
            
            m_FeatureSecurityServiceStub.IsActivated<ImportDesignersFeature>().Returns(true);

            var sourceVersion = new Version(0, 0);

            m_XmlConverterServiceStub.GetDocumentVersion(Arg.Any<string>()).Returns(sourceVersion);

            IProjectItem projectItem = m_ProjectItemFactory.ImportDesigner("filePath", "newName");
            Assert.IsNull(projectItem);
        }

        [Test]
        public void ImportDesignerFileWhenProjectScreenTemplateFeatureActivated()
        {
            // Arrange
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            TestHelper.AddServiceStub<IMessageBoxServiceIde>();
            var serializedDesignerExtractorStub = TestHelper.AddServiceStub<ISerializedDesignerExtractor>();
            m_FeatureSecurityServiceStub.IsActivated<ImportDesignersFeature>().Returns(false);
            m_FeatureSecurityServiceStub.IsActivated<ProjectScreenTemplateFeature>().Returns(true);
            m_XmlConverterServiceStub.GetDocumentVersion(Arg.Any<string>()).Returns(version);

            // Act
            m_ProjectItemFactory.ImportDesigner("filePath", "newName");

            // Assert
            serializedDesignerExtractorStub.ReceivedWithAnyArgs().DeserializeType(Arg.Any<string>());
        }
    }
}