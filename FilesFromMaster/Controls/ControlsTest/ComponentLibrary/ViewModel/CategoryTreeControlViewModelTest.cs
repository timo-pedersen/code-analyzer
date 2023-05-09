using System;
using System.ComponentModel;
using System.Linq;
using Neo.ApplicationFramework.Common.FrameworkDialogs;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.ViewModel
{
    [TestFixture]
    public class CategoryTreeControlViewModelTest
    {
        private CategoryTreeControlViewModel m_UnderTest;
        private IComponentFacade m_ComponentFacade;
        private IDialogService m_DialogService;

        [SetUp]
        public void SetUp()
        {
            m_ComponentFacade = MockRepository.GenerateMock<IComponentFacade>();
            m_DialogService = MockRepository.GenerateMock<IDialogService>();
            var projectManager = MockRepository.GenerateMock<IProjectManager>();
            m_UnderTest = new CategoryTreeControlViewModel(m_ComponentFacade, m_DialogService, projectManager.ToILazy());
        }

        [Test]
        public void Should_fire_PropertyChanged()
        {
            Assert.That(m_UnderTest.NotifiesOn(x => x.SelectedCategory).When(x => x.SelectedCategory = new ComponentCategoryViewModel("Test")));
        }

        [Test]
        public void RenameCategory_should_call_ComponentFacade()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            var newCategory = CreateTestCategory();
            newCategory.Expect(i => i.Name).Return("AnotherName");
            m_ComponentFacade.Expect(i => i.RenameCategory(category.Model, "NewName")).Return(newCategory);
            m_ComponentFacade.Replay();

            // Act
            var actual = m_UnderTest.RenameCategory(category, "NewName");

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.SameAs(newCategory));
        }

        [Test]
        public void Load_should_fetch_categories_and_select_the_first_one()
        {
            // Arrange
            var categoryA = CreateTestCategory();
            var categoryB = CreateTestCategory();
            m_ComponentFacade.Expect(i => i.FindRootCategories()).Return(new[] { categoryA, categoryB });
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.Load();

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(m_UnderTest.RootCategories.Count, Is.EqualTo(2));
            Assert.That(m_UnderTest.SelectedCategory.Model, Is.SameAs(categoryA));
        }

        [Test]
        public void AddComponentsCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.Expect(i => i.ShowOpenFileDialog(null)).IgnoreArguments().Return(false);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.AddComponentsCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void AddComponentsCommand_should_call_ComponentFacade_when_file_selected()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();

            var fileNames = new[]
            {
                "TestFile1",
                "TestFile2"
            };

            m_DialogService
                .Expect(i => i.ShowOpenFileDialog(null)).IgnoreArguments()
                .WhenCalled(
                    invocation =>
                    {
                        var settings = (OpenFileDialogSettings)invocation.Arguments[0];
                        settings.FileNames = fileNames;
                    })
                .Return(true);
            m_ComponentFacade.Expect(i => i.AddComponents(category.Model, fileNames));
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.AddComponentsCommand.Execute(category);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void ImportComponentCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.Expect(i => i.ShowOpenFileDialog(null)).IgnoreArguments().Return(false);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.ImportComponentCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void ImportComponentCommand_should_do_nothing_when_component_not_added()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            m_DialogService
                .Expect(i => i.ShowOpenFileDialog(null)).IgnoreArguments()
                .WhenCalled(
                    invocation =>
                    {
                        var settings = (OpenFileDialogSettings)invocation.Arguments[0];
                        settings.FileName = "TestFile";
                    })
                .Return(true);
            m_ComponentFacade.Expect(i => i.ImportComponents(category.Model, "TestFile")).Return(null);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.ImportComponentCommand.Execute(category);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void ImportComponentCommand_should_show_added_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            var category = CreateTestCategory();
            m_DialogService
                .Expect(i => i.ShowOpenFileDialog(null)).IgnoreArguments()
                .WhenCalled(
                    invocation =>
                    {
                        var settings = (OpenFileDialogSettings)invocation.Arguments[0];
                        settings.FileName = "TestFile";
                    })
                .Return(true);
            m_ComponentFacade.Expect(i => i.ImportComponents(rootCategory.Model, "TestFile")).Return(category);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.ImportComponentCommand.Execute(rootCategory);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
            Assert.That(rootCategory.IsExpanded);
            Assert.That(rootCategory.SubCategories[0].IsSelected);
            Assert.That(rootCategory.SubCategories[0].Model, Is.SameAs(category));
        }

        [Test]
        public void ExportComponentCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.Expect(i => i.ShowSaveFileDialog(null)).IgnoreArguments().Return(false);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.ExportComponentCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void ExportComponentCommand_should_call_ExportComponent()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            m_DialogService
                .Expect(i => i.ShowSaveFileDialog(null)).IgnoreArguments()
                .WhenCalled(
                    invocation =>
                    {
                        var settings = (SaveFileDialogSettings)invocation.Arguments[0];
                        settings.FileName = "TestFile";
                    })
                .Return(true);
            m_ComponentFacade.Expect(i => i.ExportComponents(category.Model, "TestFile"));
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.ExportComponentCommand.Execute(category);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            m_DialogService.VerifyAllExpectations();
        }

        [Test]
        public void AddCategoryCommand_should_show_added_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            
            var category = CreateTestCategory();
            category.Expect(i => i.SubCategories).Return(Enumerable.Empty<IComponentCategory>());

            m_ComponentFacade.Expect(i => i.AddCategory(rootCategory.Model)).Return(category);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.AddCategoryCommand.Execute(rootCategory);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(rootCategory.IsSelected, Is.False);
            Assert.That(rootCategory.IsExpanded);
            Assert.That(rootCategory.SubCategories[0].IsSelected);
            Assert.That(rootCategory.SubCategories[0].Model, Is.SameAs(category));
        }

        [Test]
        public void DeleteCategoryCommand_should_be_disabled_if_category_is_readonly()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            category.Model.Expect(i => i.IsReadOnly).Return(true);

            // Act
            var actual = m_UnderTest.DeleteCategoryCommand.CanExecute(category);

            // Assert
            Assert.That(actual, Is.False);
        }

        [Test]
        public void DeleteCategoryCommand_should_be_enabled_if_category_is_not_readonly()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            category.Model.Expect(i => i.IsReadOnly).Return(false);

            // Act
            var actual = m_UnderTest.DeleteCategoryCommand.CanExecute(category);

            // Assert
            Assert.That(actual, Is.True);
        }

        [Test]
        public void EnterEditModeCommand_should_set_IsEditMode()
        {
            var category = CreateTestCategoryViewModel();

            m_UnderTest.EnterEditModeCommand.Execute(category);

            Assert.That(category.IsEditMode);
        }

        [Test]
        public void DeleteCategoryCommand_should_do_nothing_when_user_does_not_confirm_delete()
        {
            // Arrange
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(false);
            
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.DeleteCategoryCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void DeleteCategoryCommand_should_delete_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            var subCategory = CreateTestCategoryViewModel(rootCategory);
            rootCategory.SubCategories.Add(subCategory);
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(true);
            m_ComponentFacade.Expect(i => i.DeleteCategory(subCategory.Model)).Return(true);
            m_ComponentFacade.Replay();
            m_DialogService.Replay();

            // Act
            m_UnderTest.DeleteCategoryCommand.Execute(subCategory);

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(rootCategory.SubCategories, Is.Empty);
            Assert.That(rootCategory.IsSelected);
        }

        [Test]
        public void AddPicturesCommand_should_call_SelectNewSymbolsFromFile()
        {
            // Arrange
            m_ComponentFacade.Expect(i => i.SelectNewSymbolsFromFile());

            // Act
            m_UnderTest.AddPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void RemoveUnusedPicturesCommand_should_do_nothing_when_user_cancels()
        {
            // Arrange
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(false);
            m_DialogService.Replay();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.RemoveUnusedPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void RemoveUnusedPicturesCommand_should_call_RemoveUnusedSymbols_when_user_confirms()
        {
            // Arrange
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(true);
            m_ComponentFacade.Expect(i => i.RemoveUnusedSymbols());
            m_DialogService.Replay();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.RemoveUnusedPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void DeleteAllProjectFilesCommand_should_do_nothing_when_user_cancels()
        {
            // Arrange
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(false);
            m_DialogService.Replay();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.DeleteAllProjectFilesCommand.Execute(CreateTestCategoryViewModelWithProjectFilesCategory());

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void DeleteAllProjectFilesCommand_should_call_RemoveAllFilesFromLibrary_when_user_confirms()
        {
            // Arrange
            var category = CreateTestCategoryViewModelWithProjectFilesCategory();
            m_DialogService
                .Expect(i => i.ShowQuestion(null, null)).IgnoreArguments()
                .Return(true);
            m_ComponentFacade.Expect(i => i.RemoveAllComponents(category.Model));
            m_DialogService.Replay();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.DeleteAllProjectFilesCommand.Execute(category);

            // Assert
            m_DialogService.VerifyAllExpectations();
            m_ComponentFacade.VerifyAllExpectations();
        }

        private ComponentCategoryViewModel CreateTestCategoryViewModelWithSymbolLibraryCategory() 
        {
            return new ComponentCategoryViewModel(MockRepository.GenerateMock<SymbolLibraryCategory>(), m_UnderTest, null);
        }

        private ComponentCategoryViewModel CreateTestCategoryViewModelWithProjectFilesCategory()
        {
            return new ComponentCategoryViewModel(MockRepository.GenerateMock<ProjectFilesCategory>(), m_UnderTest, null);
        }


        private ComponentCategoryViewModel CreateTestCategoryViewModel(ComponentCategoryViewModel parent = null)
        {
            return new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest, parent);
        }

        private IComponentCategory CreateTestCategory() 
        {
            var category = MockRepository.GenerateMock<IComponentCategory>();
            category.Expect(i => i.SubCategories).Return(Enumerable.Empty<IComponentCategory>());
            return category;
        }
    }
}
