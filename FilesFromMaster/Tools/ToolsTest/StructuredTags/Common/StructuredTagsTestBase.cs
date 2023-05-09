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
using Rhino.Mocks;

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
            var rootComponent = MockRepository.GenerateStub<IStructuredTypeRootComponent>();
            rootComponent.Stub(x => x.Members).Return(new List<ITag>());
            var structuredTagService = TestHelper.AddServiceStub<IStructuredTypeService>();
            structuredTagService.Stub(x => x.GetType(Arg<string>.Is.Anything)).Return(rootComponent);
            return structuredTagService;
        }

        protected virtual void SetUpBase()
        {
            TestHelper.ClearServices();

            TestHelper.AddService<IDataItemCountingService>(MockRepository.GenerateStub<IDataItemCountingService>());

            StructuredTypeService = MockRepository.GenerateStub<IStructuredTypeService>();
            TestHelper.AddService<IStructuredTypeService>(StructuredTypeService);

            OpcClientServiceIde = MockRepository.GenerateStub<IOpcClientServiceIde>();
            TestHelper.AddService<IOpcClientServiceCF>(OpcClientServiceIde);
            TestHelper.AddService<IOpcClientServiceIde>(OpcClientServiceIde);
            OpcClientServiceIde.Stub(inv => inv.Controllers).Return(new ExtendedBindingList<IDataSourceContainer>());
            m_GlobalController = new GlobalController();
            OpcClientServiceIde.Stub(inv => inv.GlobalController).Return(m_GlobalController);

            ErrorListService = MockRepository.GenerateStub<IErrorListService>();
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

            IStructuredTypeRootComponent rootType = MockRepository.GenerateStub<IStructuredTypeRootComponent>();
            rootType.Stub(inv => inv.BaseType).Return(null);
            rootType.Name = Type1Name;

            List<ITag> allMembers = PrepareMembers(dataItemMemberPostFixNr, structuredMemberPostFixNr, appendExtraOnType, dataItemMembers, structuredMembers);

            rootType.Stub(inv => inv.Members).Return(allMembers.ToArray());
            StructuredTypeService.Stub(inv => inv.GetType(Arg<string>.Is.Equal(Type1Name))).Return(rootType);
            StructuredTypeService.Stub(inv => inv.TypeNameExists(Arg<string>.Is.Equal(Type1Name))).Return(true);

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
            var structType = MockRepository.GenerateStub<IStructuredTypeRootComponent>();
            structType.Name = typeName;
            structType.Stub(inv => inv.Members).Return(Enumerable.Empty<ITag>());
            structType.Stub(inv => inv.BaseType).Return(null);
            StructuredTypeService.Stub(inv => inv.GetType(Arg<string>.Is.Equal(typeName))).Return(structType);
            StructuredTypeService.Stub(inv => inv.TypeNameExists(Arg<string>.Is.Equal(typeName))).Return(true);
            return structMember;
        }


    }

}
