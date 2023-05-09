using System;
using System.Reflection;
using Core.Api.Feature;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Controls.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

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
            
            m_FeatureSecurityServiceStub.BackToRecord();
            m_FeatureSecurityServiceStub.Stub(x => x.IsActivated<ImportDesignersFeature>()).Return(true);
            m_FeatureSecurityServiceStub.Replay();

            var sourceVersion = new Version(0, 0);

            m_XmlConverterServiceStub.BackToRecord();
            m_XmlConverterServiceStub.Stub(x => x.GetDocumentProductVersion(Arg<string>.Is.Anything)).Return(sourceVersion);
            m_XmlConverterServiceStub.Replay();

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
            m_FeatureSecurityServiceStub.BackToRecord();
            m_FeatureSecurityServiceStub.Stub(x => x.IsActivated<ImportDesignersFeature>()).Return(false);
            m_FeatureSecurityServiceStub.Stub(x => x.IsActivated<ProjectScreenTemplateFeature>()).Return(true);
            m_FeatureSecurityServiceStub.Replay();
            m_XmlConverterServiceStub.Stub(x => x.GetDocumentProductVersion(Arg<string>.Is.Anything)).Return(version);

            // Act
            m_ProjectItemFactory.ImportDesigner("filePath", "newName");

            // Assert
            serializedDesignerExtractorStub.AssertWasCalled(x => x.DeserializeType(Arg<string>.Is.Anything));
        }
    }
}