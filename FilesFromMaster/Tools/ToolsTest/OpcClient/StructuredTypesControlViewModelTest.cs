using System;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class StructuredTypesControlViewModelTest
    {
        private IStructuredTypesViewerFacade m_StructuredTypesViewerFacade;
        private const string TypeName = "Type1";

        [SetUp]
        public void SetUp()
        {
            m_StructuredTypesViewerFacade = MockRepository.GenerateMock<IStructuredTypesViewerFacade>();
        }

        [Test]
        public void TestHasNoTypes()
        {
            m_StructuredTypesViewerFacade.Expect(inv => inv.GetTypes()).Repeat.Once().Return(new ITypeEntity[0]);
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            Assert.IsFalse(viewModel.HasTypes);
            m_StructuredTypesViewerFacade.VerifyAllExpectations();
        }


        [Test]
        public void TestHasTypes()
        {
            ITypeEntity entity = MockRepository.GenerateStub<ITypeEntity>();
            entity.Name = TypeName;
            m_StructuredTypesViewerFacade.Expect(inv => inv.GetTypes()).Repeat.Once().Return(new[] {entity});
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            Assert.IsTrue(viewModel.HasTypes);
            m_StructuredTypesViewerFacade.VerifyAllExpectations();
        }

        [Test]
        public void TestStructuredTags()
        {
            ITypeEntity entity = MockRepository.GenerateMock<ITypeEntity>();
            entity.Name = TypeName;
            entity.Expect(inv => inv.UniqueIdentifier).Repeat.Twice().Return("uniqueid");
            entity.Expect(inv => inv.iXType)
                  .Repeat.Twice()
                  .Return(string.Concat(StringConstants.NeoApplicationFrameworkGenerated, ".", TypeName));
            m_StructuredTypesViewerFacade.Expect(inv => inv.GetTypes()).Repeat.Once().Return(new[] {entity});
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);


            Assert.IsTrue(viewModel.StructuredTypes.Count == 1);
            Assert.IsTrue(viewModel.StructuredTypes[0].Name == entity.Name);
            Assert.IsTrue(viewModel.StructuredTypes[0].UniqueIdentifier == entity.UniqueIdentifier);
            Assert.IsTrue(viewModel.StructuredTypes[0].iXType == entity.iXType);

            entity.VerifyAllExpectations();
            m_StructuredTypesViewerFacade.VerifyAllExpectations();
        }


        [Test]
        public void StructuredTypesUpdated()
        {
            // ARRANGE
            var originalStructuredType = MockRepository.GenerateStub<ITypeEntity>();
            originalStructuredType.Name = "OriginalType";

            var updatedStructuredType = MockRepository.GenerateStub<ITypeEntity>();
            updatedStructuredType.Name = "UpdatedType";

            // Return original the first time method is called
            m_StructuredTypesViewerFacade
                .Expect(stvf => stvf.GetTypes())
                .Return(new[] {originalStructuredType})
                .Repeat.Once();

            // Return updated the second time method is called
            m_StructuredTypesViewerFacade
                .Expect(stvf => stvf.GetTypes())
                .Return(new[] {updatedStructuredType})
                .Repeat.Once();

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);

            var firstResponseRelevantOnlyForVerifyingExpectations = viewModel.StructuredTypes;
            Assert.IsTrue(1 ==firstResponseRelevantOnlyForVerifyingExpectations.Count); // only do this in order to remove warning from row above

            // ACT
            m_StructuredTypesViewerFacade.Raise(
                ss => ss.TypesChanged += null, m_StructuredTypesViewerFacade, EventArgs.Empty
            );
            ObservableCollection<StructuredTypeViewModel> structuredTags = viewModel.StructuredTypes;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("UpdatedType"));
            m_StructuredTypesViewerFacade.VerifyAllExpectations();
        }

        [Test]
        public void CanNotRemove()
        {
            // ARRANGE
            var structuredType = MockRepository.GenerateStub<ITypeEntity>();

            m_StructuredTypesViewerFacade
                .Expect(stvf => stvf.GetTypes())
                .Return(new[] {structuredType});

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            viewModel.SelectedStructuredType = null;

            // ACT
            bool canRemove = viewModel.RemoveCommand.CanExecute(null);

            // ASSERT
            Assert.That(canRemove, Is.False);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CanRemove(bool isOkToDeleteType)
        {
            InternalCanRemove(isOkToDeleteType);
        }

        private void InternalCanRemove(bool isOkToDeleteType)
        {
            m_StructuredTypesViewerFacade.Stub(
                    inv => inv.IsTypeDeletable(Arg<string>.Is.Anything)
                )
                .Return(isOkToDeleteType);

            // ARRANGE
            var structuredType = MockRepository.GenerateStub<ITypeEntity>();

            m_StructuredTypesViewerFacade
                .Expect(stvf => stvf.GetTypes())
                .Return(new[] {structuredType});

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            viewModel.SelectedStructuredType = viewModel.StructuredTypes.First();

            // ACT
            bool canRemove = viewModel.RemoveCommand.CanExecute(null);

            // ASSERT
            Assert.IsTrue(canRemove == isOkToDeleteType);
        }


        [Test]
        public void RemoveType()
        {
            // ARRANGE
            var structuredTag = MockRepository.GenerateStub<ITypeEntity>();
            structuredTag.Name = TypeName;

            m_StructuredTypesViewerFacade
                .Stub(stvf => stvf.GetTypes())
                .Return(new[] {structuredTag});


            m_StructuredTypesViewerFacade
                .Expect(stvf => stvf.DeleteType(TypeName))
                .Repeat.Once();

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            viewModel.SelectedStructuredType = viewModel.StructuredTypes.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_StructuredTypesViewerFacade.VerifyAllExpectations();
        }

    }

}
