#if !VNEXT_TARGET
using System.Collections.ObjectModel;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer;
using NSubstitute;
using NUnit.Framework;

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
            m_StructuredTypesViewerFacade = Substitute.For<IStructuredTypesViewerFacade>();
        }

        [Test]
        public void TestHasNoTypes()
        {
            m_StructuredTypesViewerFacade.GetTypes().Returns(new ITypeEntity[0]);
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            Assert.IsFalse(viewModel.HasTypes);
            m_StructuredTypesViewerFacade.Received(1).GetTypes();
        }

        [Test]
        public void TestHasTypes()
        {
            ITypeEntity entity = Substitute.For<ITypeEntity>();
            entity.Name = TypeName;
            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] { entity });
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            Assert.IsTrue(viewModel.HasTypes);
            m_StructuredTypesViewerFacade.Received(1).GetTypes();
        }

        [Test]
        public void TestStructuredTags()
        {
            ITypeEntity entity = Substitute.For<ITypeEntity>();
            entity.Name = TypeName;
            entity.UniqueIdentifier.Returns("uniqueid");
            entity.iXType.Returns(string.Concat(StringConstants.NeoApplicationFrameworkGenerated, ".", TypeName));
            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] { entity });
            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);

            Assert.IsTrue(viewModel.StructuredTypes.Count == 1);
            Assert.IsTrue(viewModel.StructuredTypes[0].Name == entity.Name);
            Assert.IsTrue(viewModel.StructuredTypes[0].UniqueIdentifier == entity.UniqueIdentifier);
            Assert.IsTrue(viewModel.StructuredTypes[0].iXType == entity.iXType);

            m_StructuredTypesViewerFacade.Received(2).GetTypes();
        }


        [Test]
        public void StructuredTypesUpdated()
        {
            // ARRANGE
            var originalStructuredType = Substitute.For<ITypeEntity>();
            originalStructuredType.Name = "OriginalType";

            var updatedStructuredType = Substitute.For<ITypeEntity>();
            updatedStructuredType.Name = "UpdatedType";

            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] { originalStructuredType }, new[] { updatedStructuredType });

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);

            var firstResponseRelevantOnlyForVerifyingExpectations = viewModel.StructuredTypes;
            Assert.IsTrue(1 ==firstResponseRelevantOnlyForVerifyingExpectations.Count); // only do this in order to remove warning from row above

            // ACT
            Raise.Event();
            ObservableCollection<StructuredTypeViewModel> structuredTags = viewModel.StructuredTypes;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("UpdatedType"));
            m_StructuredTypesViewerFacade.Received(2).GetTypes();
        }

        [Test]
        public void CanNotRemove()
        {
            // ARRANGE
            var structuredType = Substitute.For<ITypeEntity>();

            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] {structuredType});

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
            m_StructuredTypesViewerFacade.IsTypeDeletable(Arg.Any<string>()).Returns(isOkToDeleteType);

            // ARRANGE
            var structuredType = Substitute.For<ITypeEntity>();

            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] {structuredType});

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
            var structuredTag = Substitute.For<ITypeEntity>();
            structuredTag.Name = TypeName;

            m_StructuredTypesViewerFacade.GetTypes().Returns(new[] {structuredTag});

            var viewModel = new StructuredTypesControlViewModel(m_StructuredTypesViewerFacade);
            viewModel.SelectedStructuredType = viewModel.StructuredTypes.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_StructuredTypesViewerFacade.Received(1).DeleteType(TypeName);
        }
    }
}
#endif
