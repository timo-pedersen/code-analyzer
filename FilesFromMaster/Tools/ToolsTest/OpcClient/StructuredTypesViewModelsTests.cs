using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class StructuredTypesViewModelsTests
    {
        private IStructuredTypesViewerFacade m_StructuredTypesViewerFacade;

        [SetUp]
        public void SetUp()
        {
            m_StructuredTypesViewerFacade = MockRepository.GenerateMock<IStructuredTypesViewerFacade>();
        }

        [Test]
        public void TestStructuredTypeMemberViewModel()
        {
            var entity = MockRepository.GenerateStub<ITypeMember>();
            entity.Name = "NAME";
            entity.Stub(inv => inv.iXType).Return("IXTYPE");
            entity.Stub(inv => inv.UniqueIdentifier).Return("ID");
            entity.Stub(inv => inv.ControllerName).Return("CTRLNAME");
            var viewModel = new StructuredTypeMemberViewModel(entity);

            Assert.That(viewModel.Name, Is.EqualTo("NAME"));
            Assert.That(viewModel.iXType, Is.EqualTo("IXTYPE"));
            Assert.That(viewModel.UniqueIdentifier, Is.EqualTo("ID"));
            Assert.That(viewModel.ControllerName, Is.EqualTo("CTRLNAME"));
            Assert.That(viewModel.Children.Count, Is.EqualTo(0));
        }


        [Test]
        public void TestStructuredTypeViewModel()
        {
            var references = new List<ITypeReference>();
            var ref1 = MockRepository.GenerateStub<ITypeReference>();
            ref1.Name = "T1";
            ref1.Stub(inv=>inv.TypeName).Return("T1");
            references.Add(ref1);

            var ref2 = MockRepository.GenerateStub<ITypeReference>();
            ref2.Name = "T2";
            ref2.Stub(inv => inv.TypeName).Return("T2");
            references.Add(ref2);

            var member = MockRepository.GenerateStub<ITypeMember>();
            member.Name = "Member";
            member.Stub(inv=>inv.ControllerName).Return("CTRL");
            member.Stub(inv=>inv.UniqueIdentifier).Return("memberID");
            member.Stub(inv=>inv.iXType).Return("BEDATATYPE.REAL");
                


            var entity = MockRepository.GenerateStub<ITypeEntity>();
            entity.Name = "NAME";
            entity.Stub(inv => inv.iXType).Return("IXTYPE");
            entity.Stub(inv => inv.UniqueIdentifier).Return("ID");
            entity.Stub(inv => inv.ControllerName).Return("CTRLNAME");
            entity.Stub(inv => inv.StructuredChildren).Return(references);
            entity.Stub(inv => inv.Children).Return(new []{member});
            
            // create

            m_StructuredTypesViewerFacade.Stub(
                inv =>
                    inv.GetType(Arg<ITypeReference>.Is.Anything)
                ).WhenCalled(
                    inv =>
                    {
                        var reference = (ITypeReference)inv.Arguments[0];
                        var result = MockRepository.GenerateStub<ITypeEntity>();
                        result.Stub(i => i.StructuredChildren).Return(Enumerable.Empty<ITypeReference>());
                        result.Stub(i => i.Children).Return(Enumerable.Empty<ITypeMember>());
                        result.Name = reference.TypeName;
                        inv.ReturnValue = result;
                    }).Return(default(ITypeEntity));

            
            var viewModel = new StructuredTypeViewModel(entity, m_StructuredTypesViewerFacade);

            Assert.That(viewModel.Name, Is.EqualTo("NAME"));
            Assert.That(viewModel.iXType, Is.EqualTo("IXTYPE"));
            Assert.That(viewModel.UniqueIdentifier, Is.EqualTo("ID"));
            Assert.That(viewModel.ControllerName, Is.EqualTo("CTRLNAME"));
            Assert.That(viewModel.Children.Count, Is.EqualTo(3));
            Assert.That(viewModel.Children[0].Name, Is.EqualTo("T1"));
            Assert.That(viewModel.Children[1].Name, Is.EqualTo("T2"));
            Assert.That(viewModel.Children[2].Name, Is.EqualTo("Member"));
        }

        
        [Test]
        public void TestBaseTypeViewModel()
        {
            const string typeName = "Type";

            var typeEntity = MockRepository.GenerateStub<ITypeEntity>();
            typeEntity.Name = typeName;
            typeEntity.Stub(inv => inv.StructuredChildren).Return(Enumerable.Empty<ITypeReference>());
            typeEntity.Stub(inv => inv.UniqueIdentifier).Return("id");
            typeEntity.Stub(inv => inv.iXType).Return(typeName);

            var r = MockRepository.GenerateStub<ITypeReference>();
            r.Name = typeName;
            r.Stub(inv=>inv.TypeName).Return(typeName);
            m_StructuredTypesViewerFacade.Stub(inv => inv.GetType(Arg<ITypeReference>.Is.Anything)).Return(typeEntity);
            var viewModel = new BaseTypeViewModel(m_StructuredTypesViewerFacade, new[] {r});
            
            Assert.That(viewModel.Name, Is.EqualTo(BaseTypeViewModel.LevelName));
            Assert.That(viewModel.Children.Count, Is.EqualTo(1));
            Assert.That(viewModel.Children[0].Name, Is.EqualTo(typeName));
        }

    }
}
