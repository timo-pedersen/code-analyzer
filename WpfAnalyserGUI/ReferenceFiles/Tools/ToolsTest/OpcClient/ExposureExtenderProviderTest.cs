#if !VNEXT_TARGET
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Api.Utilities;
using Core.Component.Api.CodeGeneration;
using Core.Component.Api.Design;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.OpcUaServer;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Tools.MultiLanguage;
using Neo.ApplicationFramework.Tools.OpcUaServer;
using Neo.ApplicationFramework.Tools.Selection;
using NSubstitute;
using NUnit.Framework;
using INameCreationService = Neo.ApplicationFramework.Interfaces.INameCreationService;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class ExposureExtenderProviderTest
    {
        private IOpcClientServiceIde m_OpcClientService;
        private IDesignerHost m_DesignerHost;
        private IOpcUaServerRootComponent m_OpcUaServerRootComponentStub;
        private IOpcClientServiceIde m_OpcClientServiceIde;
        private IOpcUaServerServiceIde m_OpcUaServerServiceIde;
        private IGlobalController m_GlobalController;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.AddServiceStub<IFastLoggingFeatureLogicService>();
            TestHelper.AddServiceStub<IProjectManager>();

            m_GlobalController = Substitute.For<IGlobalController>();
            m_GlobalController.DataItemBases.Returns(new ReadOnlyCollection<IDataItemBase>(new IDataItemBase[] { }));

            TestHelper.AddService<INameCreationService>(new NameCreationService());
            TestHelper.AddServiceStub<IOpcClientServiceCF>();
            var protectableServiceIde = TestHelper.AddServiceStub<IProtectableItemServiceIde>();
            m_OpcClientServiceIde = new TestOpcClientServiceIde(new LazyWrapper<IProtectableItemServiceIde>(() => protectableServiceIde), m_GlobalController);

            var testSite = new TestSite();
            IDesignerDocument designerDocument = new DesignerDocument(
                testSite,
                Substitute.For<IDesignerPersistenceService>(),
                Substitute.For<System.ComponentModel.Design.Serialization.INameCreationService>().ToILazy(),
                () => new SelectionService(),
                new LazyWrapper<IReferenceProvider>(
                    () => new GlobalReferenceToReferenceAdapter(ServiceContainerCF.GetService<IGlobalReferenceService>())),
                new IDesignerSerializationProvider[] { new CodeDomMultiLanguageProvider(CodeDomLocalizationModel.PropertyReflection) }
            );
            m_DesignerHost = designerDocument.DesignerHost;
            ((IExtenderProviderService)m_DesignerHost).AddExtenderProvider((IExtenderProvider)Activator.CreateInstance(typeof(ExposureExtenderProvider)));

            m_OpcUaServerServiceIde = TestHelper.AddServiceStub<IOpcUaServerServiceIde>();
            m_OpcUaServerServiceIde.IsOpcUaServerEnabledInProject().Returns(true);
            TestHelper.AddServiceStub<IGlobalSelectionService>();

            TestHelper.AddServiceStub<ITagChangedNotificationServiceCF>();
            m_OpcUaServerRootComponentStub = Substitute.For<IOpcUaServerRootComponent>();
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.AllTagsVisible;
            var projectItem = Substitute.For<IDesignerProjectItem>();
            projectItem.ContainedObject.Returns(m_OpcUaServerRootComponentStub);
            var projectItemFinder = TestHelper.AddServiceStub<IProjectItemFinder>();
            projectItemFinder.GetProjectItems(typeof(IOpcUaServerRootComponent)).Returns(new[] { projectItem });

            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_OpcClientService.GlobalController.Returns(m_GlobalController);
            m_OpcClientService.AddNewDataItem(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IControllerBase>())
                .Returns(x => (IGlobalDataItem)m_DesignerHost.CreateComponent(typeof(CreateSeries.TestGlobalDataItem)));
        }

        [TearDown]
        public void TearDown() => TestHelper.ClearServices();

        [Test]
        public void CanReadExposedPropertyForGlobalDataItem()
        {
            // Arrange
            IGlobalDataItem dataItem = CreateGlobalDataItem("Tag1");
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.AllTagsVisible;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            PropertyDescriptor setPropertyDescriptor = TypeDescriptor.GetProperties(dataItem)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptor.SetValue(dataItem, true);

            // Act
            PropertyDescriptor getPropertyDescriptor = TypeDescriptor.GetProperties(dataItem)[ExposureExtenderProvider.IsExposedPropertyName];
            object tagExposedValue = getPropertyDescriptor.GetValue(dataItem);

            // Assert
            Assert.That(tagExposedValue, Is.Not.Null);
            Assert.That(tagExposedValue, Is.True);
        }

        [Test]
        public void TestGetExposedGlobalDataItems()
        {
            // Arrange
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.Customized;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            IGlobalDataItem dataItem1 = CreateGlobalDataItem("Tag1");
            IGlobalDataItem dataItem2 = CreateGlobalDataItem("Tag2");
            IGlobalDataItem dataItem3 = CreateGlobalDataItem("Tag3");

            PropertyDescriptor setPropertyDescriptorTag1 = TypeDescriptor.GetProperties(dataItem1)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptorTag1.SetValue(dataItem1, true);

            PropertyDescriptor setPropertyDescriptorTag2 = TypeDescriptor.GetProperties(dataItem2)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptorTag2.SetValue(dataItem2, false);

            PropertyDescriptor setPropertyDescriptorTag3 = TypeDescriptor.GetProperties(dataItem3)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptorTag3.SetValue(dataItem3, true);

            m_GlobalController.GetAllTags<IGlobalDataItem>(Arg.Any<TagsPredicate>()).Returns(new[] { dataItem1, dataItem2, dataItem3 });

            // Act
            IEnumerable<IGlobalDataItem> exposedTags = m_OpcClientServiceIde.GetExposedGlobalDataItems();

            // Assert
            Assert.That(exposedTags.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TestSetGlobalDataItemExposure()
        {
            // Arrange
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.Customized;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            IGlobalDataItem dataItem1 = CreateGlobalDataItem("Tag1");
            IGlobalDataItem dataItem2 = CreateGlobalDataItem("Tag2");
            IGlobalDataItem dataItem3 = CreateGlobalDataItem("Tag3");
            List<IGlobalDataItem> dataItemsExposed = new[] { dataItem1, dataItem3 }.ToList();
            List<IGlobalDataItem> dataItemsNotExposed = new[] { dataItem2 }.ToList();

            // Act
            m_OpcClientServiceIde.SetGlobalDataItemExposure(dataItemsExposed, true);
            m_OpcClientServiceIde.SetGlobalDataItemExposure(dataItemsNotExposed, false);

            // Assert
            PropertyDescriptor getPropertyDescriptorTag1 = TypeDescriptor.GetProperties(dataItem1)[ExposureExtenderProvider.IsExposedPropertyName];
            object tag1Exposure = getPropertyDescriptorTag1.GetValue(dataItem1);

            PropertyDescriptor getPropertyDescriptorTag2 = TypeDescriptor.GetProperties(dataItem2)[ExposureExtenderProvider.IsExposedPropertyName];
            object tag2Exposure = getPropertyDescriptorTag2.GetValue(dataItem2);

            PropertyDescriptor getPropertyDescriptorTag3 = TypeDescriptor.GetProperties(dataItem3)[ExposureExtenderProvider.IsExposedPropertyName];
            object tag3Exposure = getPropertyDescriptorTag3.GetValue(dataItem3);

            Assert.That(tag1Exposure, Is.True);
            Assert.That(tag2Exposure, Is.False);
            Assert.That(tag3Exposure, Is.True);
        }

        [Test]
        public void GeneratedCodeForCustomizedTagExposure()
        {
            // Arrange
            IGlobalDataItem dataItem = CreateGlobalDataItem("Tag1");
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.Customized;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            PropertyDescriptor setPropertyDescriptor = TypeDescriptor.GetProperties(dataItem)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptor.SetValue(dataItem, true);

            var serviceProvider = Substitute.For<IDesignerHost>();
            serviceProvider.RootComponentClassName.Returns(StringConstants.Tags);

            // Act
            var codeTypeDeclaration = new CodeTypeDeclaration("CodeGenerationHelperTest");
            ICompileUnitGenerator compileUnitGenerator = new CompileUnitGenerator(StringConstants.NeoApplicationFrameworkGenerated, codeTypeDeclaration, new object());

            ICodeGeneration extenderProvider = new ExposureExtenderProvider();
            extenderProvider.AddCode(compileUnitGenerator, serviceProvider);

            // Assert
            Assert.That(compileUnitGenerator.Namespace, Is.Not.Null);
            Assert.That(compileUnitGenerator.Namespace.Types, Is.Not.Null);
          
            CodeTypeDeclaration targetClass = compileUnitGenerator.Namespace.Types.Cast<CodeTypeDeclaration>().SingleOrDefault(x => x.Name == StringConstants.GlobalDataItemExposureClassName);
            Assert.That(targetClass, Is.Not.Null);

            CodeTypeMember targetProperty = targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == "DifferentlyExposedTags");
            Assert.That(targetProperty, Is.Not.Null);

            CodeTypeMember targetMethod = targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == nameof(ITagExposure.GetTagExposure));
            Assert.That(targetMethod, Is.Not.Null);
        }

        [Test]
        public void GeneratedCodeForAllTagsVisible()
        {
            // Arrange
            IGlobalDataItem dataItem = CreateGlobalDataItem("Tag1");
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.AllTagsVisible;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            PropertyDescriptor setPropertyDescriptor = TypeDescriptor.GetProperties(dataItem)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptor.SetValue(dataItem, true);

            var serviceProvider = Substitute.For<IDesignerHost>();
            serviceProvider.RootComponentClassName.Returns(StringConstants.Tags);

            // Act
            var codeTypeDeclaration = new CodeTypeDeclaration("CodeGenerationHelperTest");
            ICompileUnitGenerator compileUnitGenerator = new CompileUnitGenerator(StringConstants.NeoApplicationFrameworkGenerated, codeTypeDeclaration, new object());

            ICodeGeneration extenderProvider = new ExposureExtenderProvider();
            extenderProvider.AddCode(compileUnitGenerator, serviceProvider);

            // Assert
            Assert.That(compileUnitGenerator.Namespace, Is.Not.Null);
            Assert.That(compileUnitGenerator.Namespace.Types, Is.Not.Null);
          
            CodeTypeDeclaration targetClass = compileUnitGenerator.Namespace.Types.Cast<CodeTypeDeclaration>().SingleOrDefault(x => x.Name == StringConstants.GlobalDataItemExposureClassName);
            Assert.That(targetClass, Is.Not.Null);

            CodeTypeMember targetProperty = targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == "m_TagsExposed");
            Assert.That(targetProperty, Is.Null);

            var targetMethod = (CodeMemberMethod)targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == nameof(ITagExposure.GetTagExposure));
            Assert.That(targetMethod, Is.Not.Null);
            
            // ReSharper disable once PossibleNullReferenceException
            Assert.That(targetMethod.Statements, Is.Not.Null);
            Assert.That(targetMethod.Statements.Count, Is.EqualTo(1));
            Assert.That(targetMethod.Statements[0].GetType(), Is.EqualTo(typeof(CodeMethodReturnStatement)));

            CodeExpression expression = ((CodeMethodReturnStatement)targetMethod.Statements[0]).Expression;
            Assert.That(((CodePrimitiveExpression)expression).Value, Is.EqualTo(true));
        }

        [Test]
        public void GeneratedCodeForNoTagsVisible()
        {
            // Arrange
            IGlobalDataItem dataItem = CreateGlobalDataItem("Tag1");
            m_OpcUaServerRootComponentStub.ExposureOption = OpcUaServerTagExposureOption.NoTagsVisible;
            m_OpcUaServerRootComponentStub.ExposureDefaultValue = OpcUaServerTagExposureDefaultValue.On;

            PropertyDescriptor setPropertyDescriptor = TypeDescriptor.GetProperties(dataItem)[ExposureExtenderProvider.IsExposedPropertyName];
            setPropertyDescriptor.SetValue(dataItem, true);

            var serviceProvider = Substitute.For<IDesignerHost>();
            serviceProvider.RootComponentClassName.Returns(StringConstants.Tags);

            // Act
            var codeTypeDeclaration = new CodeTypeDeclaration("CodeGenerationHelperTest");
            ICompileUnitGenerator compileUnitGenerator = new CompileUnitGenerator(StringConstants.NeoApplicationFrameworkGenerated, codeTypeDeclaration, new object());

            ICodeGeneration extenderProvider = new ExposureExtenderProvider();
            extenderProvider.AddCode(compileUnitGenerator, serviceProvider);

            // Assert
            Assert.That(compileUnitGenerator.Namespace, Is.Not.Null);
            Assert.That(compileUnitGenerator.Namespace.Types, Is.Not.Null);

            CodeTypeDeclaration targetClass = compileUnitGenerator.Namespace.Types.Cast<CodeTypeDeclaration>().SingleOrDefault(x => x.Name == StringConstants.GlobalDataItemExposureClassName);
            Assert.That(targetClass, Is.Not.Null);

            CodeTypeMember targetProperty = targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == "m_TagsExposed");
            Assert.That(targetProperty, Is.Null);

            var targetMethod = (CodeMemberMethod)targetClass?.Members.Cast<CodeTypeMember>().SingleOrDefault(x => x.Name == nameof(ITagExposure.GetTagExposure));
            Assert.That(targetMethod, Is.Not.Null);

            // ReSharper disable once PossibleNullReferenceException
            Assert.That(targetMethod.Statements, Is.Not.Null);
            Assert.That(targetMethod.Statements.Count, Is.EqualTo(1));
            Assert.That(targetMethod.Statements[0].GetType(), Is.EqualTo(typeof(CodeMethodReturnStatement)));

            CodeExpression expression = ((CodeMethodReturnStatement)targetMethod.Statements[0]).Expression;
            Assert.That(((CodePrimitiveExpression)expression).Value, Is.EqualTo(false));
        }

        // Helpers
        private IGlobalDataItem CreateGlobalDataItem(string name)
        {
            var globalDataItem = (IGlobalDataItem)m_DesignerHost.CreateComponent(typeof(TestGlobalDataItem));
            globalDataItem.Name = name;
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            globalDataItem.Size = 1;
            globalDataItem.Offset = 5;
            globalDataItem.Gain = 10;
            globalDataItem.IndexRegisterNumber = 2;
            globalDataItem.LogToAuditTrail = false;
            globalDataItem.AccessRight = AccessRights.Read;
            globalDataItem.PollGroup.Name = string.Empty;
            globalDataItem.AlwaysActive = true;
            globalDataItem.NonVolatile = false;
            globalDataItem.GlobalDataSubItems.Add(new GlobalDataSubItem());
            globalDataItem.Description = "Some description";
            globalDataItem.PollGroup = new PollGroup { Name = "DefaultPollGroup" };
            globalDataItem.Trigger = new DataTrigger { Name = "DefaultTrigger" };
            globalDataItem.IsPublic = false;
            globalDataItem.ReadExpression = "ReadExpressionName";
            globalDataItem.ReadExpression = "WriteExpressionName";
            return globalDataItem;
        }
    }

    sealed class TestGlobalDataItem : GlobalDataItem
    {
        public TestGlobalDataItem()
        {
            PollGroup = new PollGroup();
        }

        protected override string ControllerName => "";
        public override IPollGroup PollGroup { get; set; }
    }

    sealed class TestOpcClientServiceIde : OpcClientServiceIde
    {
        public TestOpcClientServiceIde(ILazy<IProtectableItemServiceIde> protectableItemProtector, IGlobalController globalController)
            : base(protectableItemProtector)
        {
            GlobalController = globalController;
        }
    }
}
#endif
