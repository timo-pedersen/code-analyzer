#if!VNEXT_TARGET
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using NSubstitute;
using NUnit.Framework;

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
            m_CommandHandler = Substitute.For<IComponentCategoryCommandHandler>();

            var subCategory = Substitute.For<IComponentCategory>();
            subCategory.SubCategories.Returns(Enumerable.Empty<IComponentCategory>());
            var category = Substitute.For<IComponentCategory>();
            category.SubCategories.Returns(new[] { subCategory });

            m_Parent = new ComponentCategoryViewModel("Parent");
            m_UnderTest = new ComponentCategoryViewModel(category, m_CommandHandler, m_Parent);
        }

        [Test]
        public void Should_validate_and_fire_PropertyChanged_when_name_changed()
        {
            var category = Substitute.For<IComponentCategory>();
            category.Name.Returns("ModifiedName");
            category.SubCategories.Returns(Enumerable.Empty<IComponentCategory>());
            m_CommandHandler.RenameCategory(m_UnderTest, "SomeOtherName").Returns(category);

            Assert.That(m_UnderTest.NotifiesOn(x => x.Name).When(x => x.Name = "SomeOtherName"));
            Assert.That(m_UnderTest.Name, Is.EqualTo("ModifiedName"));
            m_CommandHandler.Received().RenameCategory(m_UnderTest, "SomeOtherName");
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
            m_UnderTest.IsSelected = true;

            Assert.That(m_CommandHandler.SelectedCategory, Is.SameAs(m_UnderTest));
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
            var command = Substitute.For<ICommand>();
            m_CommandHandler.AddComponentsCommand.Returns(command);
            m_CommandHandler.ImportComponentCommand.Returns(command);
            m_CommandHandler.ExportComponentCommand.Returns(command);
            m_CommandHandler.AddCategoryCommand.Returns(command);
            m_CommandHandler.DeleteCategoryCommand.Returns(command);
            m_CommandHandler.EnterEditModeCommand.Returns(command);
            m_CommandHandler.AddPicturesCommand.Returns(command);
            m_CommandHandler.RemoveUnusedPicturesCommand.Returns(command);
            m_CommandHandler.AddProjectFileCommand.Returns(command);
            m_CommandHandler.DeleteAllProjectFilesCommand.Returns(command);

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
        }
    }
}
#endif
