using System;
using System.ComponentModel;
using Core.Api.DataSource;
using Core.Api.Utilities;
using Core.Component.Api.Design;
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
using NUnit.Framework;
using Rhino.Mocks;

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

            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_Item = MockRepository.GenerateStub<IDesignerProjectItem>();
            m_ProjectItemFinder = MockRepository.GenerateStub<IProjectItemFinder>();
            m_DesignerHost = MockRepository.GenerateStub<INeoDesignerHost>();

            m_ProjectManager.Stub(inv => inv.AddNewDesigner(Arg<Type>.Is.Anything, Arg<bool>.Is.Anything, Arg<bool>.Is.Anything, Arg<string>.Is.Anything)).
                WhenCalled(invocation =>
                {
                    m_RootComponent = new StructuredTypeRootComponent() { Name = invocation.Arguments[3].ToString() };
                    m_Item.Stub(inv => inv.DesignerType).Return(m_RootComponent.GetType());
                    m_Item.Stub(inv => inv.ContainedObject).Return(m_RootComponent);
                    invocation.ReturnValue = m_Item;
                    m_ProjectManager.GetEventRaiser(x => x.ProjectItemAdded += null).Raise(null, new ProjectItemFactoryEventArgs(m_Item));
                }).Return(default(DesignerProjectItem));
            m_DesignerHost.Stub(inv => inv.RootComponent).WhenCalled(inv => inv.ReturnValue = m_RootComponent).Return(m_RootComponent);
            m_Item.Stub(inv => inv.DesignerHost).Return(m_DesignerHost);


            m_DataSourceContainer = MockRepository.GenerateStub<IDataSourceContainer>();
            m_DataSourceContainer.Name = "Controller1";
            var dataSourceContainers = new ExtendedBindingList<IDataSourceContainer>();
            dataSourceContainers.Add(m_DataSourceContainer);

            var opcClientService = MockRepository.GenerateStub<IOpcClientServiceIde>();
            opcClientService.Stub(inv => inv.Controllers).Return(dataSourceContainers);
            TestHelper.AddService<IOpcClientServiceCF>(opcClientService);
            TestHelper.AddService<IOpcClientServiceIde>(opcClientService);
            m_OpcClientService = opcClientService.ToILazy();

            Container c = new Container();
            m_GlobalController = new GlobalController();
            c.Add(m_GlobalController, StringConstants.Tags);

            m_OpcClientService.Value.Stub(inv => inv.GlobalController).Return(m_GlobalController);

            m_NameCreationService = MockRepository.GenerateStub<INameCreationService>();
            m_NameCreationService.Stub(inv => inv.IsValidName(default(string))).IgnoreArguments().Return(true);
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
