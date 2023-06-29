#if !VNEXT_TARGET
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class StructuredTypesViewModelsTests
    {
        private IStructuredTypesViewerFacade m_StructuredTypesViewerFacade;

        [SetUp]
        public void SetUp()
        {
            m_StructuredTypesViewerFacade = Substitute.For<IStructuredTypesViewerFacade>();
        }

        [Test]
        public void TestStructuredTypeMemberViewModel()
        {
            var entity = Substitute.For<ITypeMember>();
            entity.Name = "NAME";
            entity.iXType.Returns("IXTYPE");
            entity.UniqueIdentifier.Returns("ID");
            entity.ControllerName.Returns("CTRLNAME");
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
            var ref1 = Substitute.For<ITypeReference>();
            ref1.Name = "T1";
            ref1.TypeName.Returns("T1");
            references.Add(ref1);

            var ref2 = Substitute.For<ITypeReference>();
            ref2.Name = "T2";
            ref2.TypeName.Returns("T2");
            references.Add(ref2);

            var member = Substitute.For<ITypeMember>();
            member.Name = "Member";
            member.ControllerName.Returns("CTRL");
            member.UniqueIdentifier.Returns("memberID");
            member.iXType.Returns("BEDATATYPE.REAL");

            var entity = Substitute.For<ITypeEntity>();
            entity.Name = "NAME";
            entity.iXType.Returns("IXTYPE");
            entity.UniqueIdentifier.Returns("ID");
            entity.ControllerName.Returns("CTRLNAME");
            entity.StructuredChildren.Returns(references);
            entity.Children.Returns(new []{member});
            
            // create

            m_StructuredTypesViewerFacade.GetType(Arg.Any<ITypeReference>())
                .Returns(
                    inv =>
                    {
                        var reference = (ITypeReference)inv[0];
                        var result = Substitute.For<ITypeEntity>();
                        result.StructuredChildren.Returns(Enumerable.Empty<ITypeReference>());
                        result.Children.Returns(Enumerable.Empty<ITypeMember>());
                        result.Name = reference.TypeName;
                        return result;
                    });

            
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

            var typeEntity = Substitute.For<ITypeEntity>();
            typeEntity.Name = typeName;
            typeEntity.StructuredChildren.Returns(Enumerable.Empty<ITypeReference>());
            typeEntity.UniqueIdentifier.Returns("id");
            typeEntity.iXType.Returns(typeName);

            var r = Substitute.For<ITypeReference>();
            r.Name = typeName;
            r.TypeName.Returns(typeName);
            m_StructuredTypesViewerFacade.GetType(Arg.Any<ITypeReference>()).Returns(typeEntity);
            var viewModel = new BaseTypeViewModel(m_StructuredTypesViewerFacade, new[] {r});
            
            Assert.That(viewModel.Name, Is.EqualTo(BaseTypeViewModel.LevelName));
            Assert.That(viewModel.Children.Count, Is.EqualTo(1));
            Assert.That(viewModel.Children[0].Name, Is.EqualTo(typeName));
        }
    }
}
#endif
