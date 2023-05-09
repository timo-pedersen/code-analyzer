using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.ViewModel
{
    [TestFixture]
    public class ComponentCategoryViewModelTest
    {
        private ComponentCategoryViewModel m_Parent;
        private ComponentCategoryViewModel m_UnderTest;
        private IComponentCategoryCommandHandler m_CommandHandler;
        
        [SetUp]
        public void SetUp()
        {
            m_CommandHandler = MockRepository.GenerateMock<IComponentCategoryCommandHandler>();

            var subCategory = MockRepository.GenerateMock<IComponentCategory>();
            subCategory.Expect(i => i.SubCategories).Return(Enumerable.Empty<IComponentCategory>());
            var category = MockRepository.GenerateMock<IComponentCategory>();
            category.Expect(i => i.SubCategories).Return(new[] { subCategory });

            m_Parent = new ComponentCategoryViewModel("Parent");
            m_UnderTest = new ComponentCategoryViewModel(category, m_CommandHandler, m_Parent);
        }

        [Test]
        public void Should_validate_and_fire_PropertyChanged_when_name_changed()
        {
            var category = MockRepository.GenerateMock<IComponentCategory>();
            category.Expect(i => i.Name).Return("ModifiedName");
            category.Expect(i => i.SubCategories).Return(Enumerable.Empty<IComponentCategory>());
            m_CommandHandler.Expect(i => i.RenameCategory(m_UnderTest, "SomeOtherName")).Return(category);
            m_CommandHandler.Replay();

            Assert.That(m_UnderTest.NotifiesOn(x => x.Name).When(x => x.Name = "SomeOtherName"));
            Assert.That(m_UnderTest.Name, Is.EqualTo("ModifiedName"));
            m_CommandHandler.VerifyAllExpectations();
        }

        [Test]
        public void Should_fire_PropertyChanged_when_IsSelected_changed()
        {
            Assert.That(m_UnderTest.NotifiesOn(x => x.IsSelected).When(x => x.IsSelected = true));
        }

        [Test]
        public void Should_fire_PropertyChanged_when_IsExpanded_changed()
        {
            Assert.That(m_UnderTest.NotifiesOn(x => x.IsExpanded).When(x => x.IsExpanded = true));
        }

        [Test]
        public void Should_set_CommandHandler_selection_when_selected()
        {
            m_CommandHandler.Expect(i => i.SelectedCategory).PropertyBehavior();

            m_UnderTest.IsSelected = true;

            Assert.That(m_CommandHandler.SelectedCategory, Is.SameAs(m_UnderTest));
            m_CommandHandler.VerifyAllExpectations();
        }

        [Test]
        public void Should_expand_parent_when_expanded()
        {
            m_UnderTest.IsExpanded = true;
            Assert.That(m_Parent.IsExpanded);
        }

        [Test]
        public void ExitEditModeCommand_should_clear_IsEditMode()
        {
            m_UnderTest.IsEditMode = true;

            m_UnderTest.ExitEditModeCommand.Execute(null);

            Assert.That(m_UnderTest.IsEditMode, Is.False);
        }

        [Test]
        public void Commands_should_be_relayed_from_CommandHandler()
        {
            // Arrange
            var command = MockRepository.GenerateMock<ICommand>();
            m_CommandHandler.Expect(i => i.AddComponentsCommand).Return(command);
            m_CommandHandler.Expect(i => i.ImportComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.ExportComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.AddCategoryCommand).Return(command);
            m_CommandHandler.Expect(i => i.DeleteCategoryCommand).Return(command);
            m_CommandHandler.Expect(i => i.EnterEditModeCommand).Return(command);
            m_CommandHandler.Expect(i => i.AddPicturesCommand).Return(command);
            m_CommandHandler.Expect(i => i.RemoveUnusedPicturesCommand).Return(command);
            m_CommandHandler.Expect(i => i.AddProjectFileCommand).Return(command);
            m_CommandHandler.Expect(i => i.DeleteAllProjectFilesCommand).Return(command);
            m_CommandHandler.Replay();

            // Act & Assert
            Assert.That(m_UnderTest.AddComponentsCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.ImportComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.ExportComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.AddCategoryCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.DeleteCategoryCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.EnterEditModeCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.AddPicturesCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.RemoveUnusedPicturesCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.AddProjectFileCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.DeleteAllProjectFilesCommand, Is.SameAs(command));
            m_CommandHandler.VerifyAllExpectations();
        }
    }
}
