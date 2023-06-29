using System;
using System.ComponentModel;
using Core.Api.DataSource;
using Core.Api.Utilities;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.Interfaces.StructuredType.LightweightRepresentation;
using Neo.ApplicationFramework.Interfaces.StructuredType.Services;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.OpcUa;
using Neo.ApplicationFramework.Tools.ProjectManager;
using Neo.ApplicationFramework.Tools.StructuredType.LightweightRepresentation.Implementation;
using Neo.ApplicationFramework.Tools.StructuredType.Model;
using Neo.ApplicationFramework.Tools.StructuredType.Services;
using Neo.ApplicationFramework.Tools.Utilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class StructuredTypeServiceTests
    {
        private StructuredTypeRootComponent m_RootComponent = null;
        private IProjectManager m_ProjectManager = null;
        private IDesignerProjectItem m_Item = null;
        private INeoDesignerHost m_DesignerHost = null;
        private INameCreationService m_NameCreationService = null;
        private ILazy<IOpcClientServiceIde> m_OpcClientService = null;
        private IDataSourceContainer m_DataSourceContainer = null;
        private IProjectItemFinder m_ProjectItemFinder;
        private GlobalController m_GlobalController;

        [SetUp]
		public void Setup()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.ClearServices();

            m_ProjectManager = Substitute.For<IProjectManager>();
            m_Item = Substitute.For<IDesignerProjectItem>();
            m_ProjectItemFinder = Substitute.For<IProjectItemFinder>();
            m_DesignerHost = Substitute.For<INeoDesignerHost>();
            m_DesignerHost.RootComponent.Returns(m_RootComponent);
            m_Item.DesignerHost.Returns(m_DesignerHost);

            m_ProjectManager.AddNewDesigner(Arg.Any<Type>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>()).
                Returns(invocation =>
                {
                    m_RootComponent = new StructuredTypeRootComponent() { Name = invocation[3].ToString() };
                    m_Item.DesignerType.Returns(m_RootComponent.GetType());
                    m_Item.ContainedObject.Returns(m_RootComponent);
                    Raise.EventWith(null, new ProjectItemFactoryEventArgs(m_Item));
                    return m_Item;
                });

            m_DataSourceContainer = Substitute.For<IDataSourceContainer>();
            m_DataSourceContainer.Name = "Controller1";
            var dataSourceContainers = new ExtendedBindingList<IDataSourceContainer>();
            dataSourceContainers.Add(m_DataSourceContainer);

            var opcClientService = Substitute.For<IOpcClientServiceIde>();
            opcClientService.Controllers.Returns(dataSourceContainers);
            TestHelper.AddService<IOpcClientServiceCF>(opcClientService);
            TestHelper.AddService<IOpcClientServiceIde>(opcClientService);
            m_OpcClientService = opcClientService.ToILazy();

            Container c = new Container();
            m_GlobalController = new GlobalController();
            c.Add(m_GlobalController, StringConstants.Tags);

            m_OpcClientService.Value.GlobalController.Returns(m_GlobalController);

            m_NameCreationService = Substitute.For<INameCreationService>();
            m_NameCreationService.IsValidName(Arg.Any<string>()).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
            m_GlobalController.Dispose();
        }

        [Test]
		public void VerifyIsValidNewTypeNameWhenTypeIsNotCreated()
        {
            const string typeName = "TestType";
            IStructuredTypeService structuredTypeService = new StructuredTypeService(m_ProjectManager, m_NameCreationService, m_OpcClientService, m_ProjectItemFinder);

            bool isValidNewTypeName = structuredTypeService.IsValidNewName(typeName, null);
            Assert.IsTrue(isValidNewTypeName);
        }


        [Test]
        public void VerifyNamingContextWorksOnIsValidNewName()
        {
            using (INamingContext namingContext = NamingContextFactory.CreateNamingContext())
            {
                const string typeName = "TestType";

                IStructuredTypeService structuredTypeService = new StructuredTypeService(m_ProjectManager, m_NameCreationService, m_OpcClientService, m_ProjectItemFinder);

                bool isValidNewTypeName = structuredTypeService.IsValidNewName(typeName, namingContext);
                Assert.True(isValidNewTypeName);
                namingContext.AllocName(typeName);
                isValidNewTypeName = structuredTypeService.IsValidNewName(typeName, namingContext);
                Assert.IsFalse(isValidNewTypeName);
            }
        }

        [Test]
        public void VerifyIsValidNewTypeNameWhenTypeIsCreated()
        {
            const string typeName = "TestType";
            IStructuredTypeService structuredTypeService = new StructuredTypeService(m_ProjectManager, m_NameCreationService, m_OpcClientService, m_ProjectItemFinder);
            CreateStructuredType(typeName, structuredTypeService);

            bool isValidNewTypeName = structuredTypeService.IsValidNewName(typeName, null);
            Assert.IsFalse(isValidNewTypeName);
        }


        private void CreateStructuredType(string typeName, IStructuredTypeService structuredTypeService)
        {
            StructuredTypeInfo structuredType = new StructuredTypeInfo();
            structuredType.TypeName = typeName;
            structuredType.AddMemberRange(new ITagImportReference[]{
                    new PrimitiveReference("member", BEDATATYPE.DT_UINTEGER4,new OpcUaNumericNodeID("NS2", new Numeric(1), "DisplayAddress"), StructuredSourceType.OpcUa, AccessRights.ReadWrite),
                });
            structuredTypeService.CreateType(m_DataSourceContainer, structuredType);
        }
    }
}
