using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredType;
using Neo.ApplicationFramework.Interfaces.StructuredType.Services;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using Neo.ApplicationFramework.Tools.StructuredType.Model;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.StructuredTags.Common
{
    public class StructuredTagsTestBase
    {
        private GlobalController m_GlobalController;
        protected IStructuredTypeService StructuredTypeService { get; private set; }
        protected IOpcClientServiceIde OpcClientServiceIde { get; private set; }
        protected IErrorListService ErrorListService { get; private set; }

        const string Tag1Name = "tag1";
        const string Type1Name = "type1";

        /// <summary>
        /// Creates a stub for structured tag service.
        /// </summary>
        /// <returns>An instance of the stub.</returns>
        public static IStructuredTypeService CreateStructuredTagServiceStub()
        {
            var rootComponent = Substitute.For<IStructuredTypeRootComponent>();
            rootComponent.Members.Returns(new List<ITag>());
            var structuredTagService = TestHelper.AddServiceStub<IStructuredTypeService>();
            structuredTagService.GetType(Arg.Any<string>()).Returns(rootComponent);
            return structuredTagService;
        }

        protected virtual void SetUpBase()
        {
            TestHelper.ClearServices();

            TestHelper.AddService<IDataItemCountingService>(Substitute.For<IDataItemCountingService>());

            StructuredTypeService = Substitute.For<IStructuredTypeService>();
            TestHelper.AddService<IStructuredTypeService>(StructuredTypeService);

            OpcClientServiceIde = Substitute.For<IOpcClientServiceIde>();
            TestHelper.AddService<IOpcClientServiceCF>(OpcClientServiceIde);
            TestHelper.AddService<IOpcClientServiceIde>(OpcClientServiceIde);
            OpcClientServiceIde.Controllers.Returns(new ExtendedBindingList<IDataSourceContainer>());
            m_GlobalController = new GlobalController();
            OpcClientServiceIde.GlobalController.Returns(m_GlobalController);

            ErrorListService = Substitute.For<IErrorListService>();
            TestHelper.AddService<IErrorListService>(ErrorListService);

        }

        protected virtual void TearDownBase()
        {
            m_GlobalController.Dispose();
        }

        protected void PreprareTestWhenMembersAreTheSame(int[] dataItemMemberPostFixNr, int[] structuredMemberPostFixNr, bool appendExtraOnType = false, bool appendExtraOnTag = false)
        {

            if (appendExtraOnType || appendExtraOnTag)
                Assert.IsTrue(appendExtraOnType != appendExtraOnTag, "Error in test configuration");

            var dataItemMembers = dataItemMemberPostFixNr.Select(item => "member" + item).ToArray();
            var structuredMembers = structuredMemberPostFixNr.Select(item => "structmember" + item).ToArray();

            IStructuredTypeRootComponent rootType = Substitute.For<IStructuredTypeRootComponent>();
            rootType.BaseType.Returns(x => null);
            rootType.Name = Type1Name;

            List<ITag> allMembers = PrepareMembers(dataItemMemberPostFixNr, structuredMemberPostFixNr, appendExtraOnType, dataItemMembers, structuredMembers);

            rootType.Members.Returns(allMembers.ToArray());
            StructuredTypeService.GetType(Type1Name).Returns(rootType);
            StructuredTypeService.TypeNameExists(Type1Name).Returns(true);

            StructuredTagInstance tagInstance = new StructuredTagInstance();
            tagInstance.Name = Tag1Name;
            tagInstance.InstanceMapping = new StructuredTagInstanceMapping { Name = Tag1Name, TypeName = tagInstance.TypeName };
            tagInstance.TypeName = Type1Name;

            PrepareMappings(tagInstance, dataItemMemberPostFixNr, structuredMemberPostFixNr, appendExtraOnTag, dataItemMembers, structuredMembers);


            ((GlobalController)OpcClientServiceIde.GlobalController).GlobalStructuredTags.Add(tagInstance);

        }

        protected static void PrepareMappings(StructuredTagInstance tagInstance, int[] dataItemMemberPostFixNr, int[] structuredMemberPostFixNr, bool appendExtraOnTag, string[] dataItemMembers, string[] structuredMembers)
        {
            foreach (var member in dataItemMembers)
            {
                tagInstance.InstanceMapping.GlobalDataItemMappings.Add(new GlobalDataItemMappingCF(member));
            }

            if (appendExtraOnTag)
            {
                tagInstance.InstanceMapping.GlobalDataItemMappings.Add(new GlobalDataItemMappingCF("member" + (dataItemMemberPostFixNr.Max() + 1)));
            }

            foreach (var member in structuredMembers)
            {
                tagInstance.InstanceMapping.StructuredTagMappings.Add(new StructuredTagInstanceMapping{ Name = member, TypeName = tagInstance.TypeName});
            }

            if (appendExtraOnTag)
            {
                tagInstance.InstanceMapping.StructuredTagMappings.Add(new StructuredTagInstanceMapping{ Name = "member" + (structuredMemberPostFixNr.Max() + 1), TypeName = tagInstance.TypeName});
            }
        }

        protected List<ITag> PrepareMembers(int[] dataItemMemberPostFixNr, int[] structuredMemberPostFixNr, bool appendExtraOnType, string[] dataItemMembers, string[] structuredMembers)
        {
            List<ITag> allMembers = new List<ITag>();

            foreach (string member in dataItemMembers)
            {
                GlobalDataItem member1 = new GlobalDataItem() { Name = member };
                allMembers.Add(member1);
            }
            if (appendExtraOnType)
            {
                allMembers.Add(new GlobalDataItem() { Name = "member" + (dataItemMemberPostFixNr.Max() + 1) });
            }


            foreach (string member in structuredMembers)
            {
                var structMember = GenerateReferenceAndTypeFromMemberName(member);
                allMembers.Add(structMember);
            }
            if (appendExtraOnType)
            {
                string memberName = "member" + (structuredMemberPostFixNr.Max() + 1);
                var structMember = GenerateReferenceAndTypeFromMemberName(memberName);
                allMembers.Add(structMember);
            }
            return allMembers;
        }

        private StructuredTypeReference GenerateReferenceAndTypeFromMemberName(string member)
        {
            string typeName = member + "Type";
            var structMember = GenerateReferenceAndRegisterType(member, typeName);
            return structMember;
        }

        private StructuredTypeReference GenerateReferenceAndRegisterType(string memberName, string typeName)
        {
            var structMember = new StructuredTypeReference() { Name = memberName, TypeName = typeName };
            var structType = Substitute.For<IStructuredTypeRootComponent>();
            structType.Name = typeName;
            structType.Members.Returns(Enumerable.Empty<ITag>());
            structType.BaseType.Returns(x => null);
            StructuredTypeService.GetType(typeName).Returns(structType);
            StructuredTypeService.TypeNameExists(typeName).Returns(true);
            return structMember;
        }


    }

}
