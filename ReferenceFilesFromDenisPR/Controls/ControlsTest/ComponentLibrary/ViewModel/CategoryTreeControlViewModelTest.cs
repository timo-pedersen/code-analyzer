#if!VNEXT_TARGET
using System;
using System.ComponentModel;
using System.Linq;
using Neo.ApplicationFramework.Common.FrameworkDialogs;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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
            m_ComponentFacade = Substitute.For<IComponentFacade>();
            m_DialogService = Substitute.For<IDialogService>();
            var projectManager = Substitute.For<IProjectManager>();
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
            newCategory.Name.Returns("AnotherName");
            m_ComponentFacade.RenameCategory(category.Model, "NewName").Returns(newCategory);

            // Act
            var actual = m_UnderTest.RenameCategory(category, "NewName");

            // Assert
            m_ComponentFacade.Received().RenameCategory(category.Model, "NewName");
            Assert.That(actual, Is.SameAs(newCategory));
        }

        [Test]
        public void Load_should_fetch_categories_and_select_the_first_one()
        {
            // Arrange
            var categoryA = CreateTestCategory();
            var categoryB = CreateTestCategory();
            m_ComponentFacade.FindRootCategories().Returns(new[] { categoryA, categoryB });

            // Act
            m_UnderTest.Load();

            // Assert
            m_ComponentFacade.Received().FindRootCategories();
            Assert.That(m_UnderTest.RootCategories.Count, Is.EqualTo(2));
            Assert.That(m_UnderTest.SelectedCategory.Model, Is.SameAs(categoryA));
        }

        [Test]
        public void AddComponentsCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.AddComponentsCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_DialogService.Received().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
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

            m_DialogService.ShowOpenFileDialog(Arg.Do<OpenFileDialogSettings>(
                    invocation =>
                    {
                        var settings = invocation;
                        settings.FileNames = fileNames;
                    }))
                .Returns(true);
            m_ComponentFacade.AddComponents(category.Model, fileNames);

            // Act
            m_UnderTest.AddComponentsCommand.Execute(category);

            // Assert
            m_ComponentFacade.Received().AddComponents(category.Model, fileNames);
            m_DialogService.Received().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
        }

        [Test]
        public void ImportComponentCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.ImportComponentCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
        }

        [Test]
        public void ImportComponentCommand_should_do_nothing_when_component_not_added()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            m_DialogService.ShowOpenFileDialog(Arg.Do<OpenFileDialogSettings>(
                    invocation =>
                    {
                        var settings = invocation;
                        settings.FileName = "TestFile";
                    }))
                .Returns(true);
            m_ComponentFacade.ImportComponents(category.Model, "TestFile").Returns(x => null);

            // Act
            m_UnderTest.ImportComponentCommand.Execute(category);

            // Assert
            m_ComponentFacade.Received().ImportComponents(category.Model, "TestFile");
            m_DialogService.ReceivedWithAnyArgs().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
        }

        [Test]
        public void ImportComponentCommand_should_show_added_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            var category = CreateTestCategory();
            m_DialogService.ShowOpenFileDialog(Arg.Do<OpenFileDialogSettings>(
                    invocation =>
                    {
                        var settings = invocation;
                        settings.FileName = "TestFile";
                    }))
                .Returns(true);
            m_ComponentFacade.ImportComponents(rootCategory.Model, "TestFile").Returns(category);

            // Act
            m_UnderTest.ImportComponentCommand.Execute(rootCategory);

            // Assert
            m_ComponentFacade.Received().ImportComponents(rootCategory.Model, "TestFile");
            m_DialogService.ReceivedWithAnyArgs().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
            Assert.That(rootCategory.IsExpanded);
            Assert.That(rootCategory.SubCategories[0].IsSelected);
            Assert.That(rootCategory.SubCategories[0].Model, Is.SameAs(category));
        }

        [Test]
        public void ExportComponentCommand_should_do_nothing_when_no_file_selected()
        {
            // Arrange
            m_DialogService.ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.ExportComponentCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowOpenFileDialog(Arg.Any<OpenFileDialogSettings>());
        }

        [Test]
        public void ExportComponentCommand_should_call_ExportComponent()
        {
            // Arrange
            var category = CreateTestCategoryViewModel();
            m_DialogService
                .ShowSaveFileDialog(Arg.Do<SaveFileDialogSettings>(
                    invocation =>
                    {
                        var settings = invocation;
                        settings.FileName = "TestFile";
                    }))
                .Returns(true);
            m_ComponentFacade.ExportComponents(category.Model, "TestFile");

            // Act
            m_UnderTest.ExportComponentCommand.Execute(category);

            // Assert
            m_ComponentFacade.ExportComponents(category.Model, "TestFile");
            m_DialogService.ReceivedWithAnyArgs().ShowSaveFileDialog(Arg.Any<SaveFileDialogSettings>());
        }

        [Test]
        public void AddCategoryCommand_should_show_added_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            
            var category = CreateTestCategory();
            category.SubCategories.Returns(Enumerable.Empty<IComponentCategory>());

            m_ComponentFacade.AddCategory(rootCategory.Model).Returns(category);

            // Act
            m_UnderTest.AddCategoryCommand.Execute(rootCategory);

            // Assert
            m_ComponentFacade.Received().AddCategory(rootCategory.Model);
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
            category.Model.IsReadOnly.Returns(true);

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
            category.Model.IsReadOnly.Returns(false);

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
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.DeleteCategoryCommand.Execute(CreateTestCategoryViewModel());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void DeleteCategoryCommand_should_delete_category()
        {
            // Arrange
            var rootCategory = CreateTestCategoryViewModel();
            var subCategory = CreateTestCategoryViewModel(rootCategory);
            rootCategory.SubCategories.Add(subCategory);
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);

            m_ComponentFacade.DeleteCategory(subCategory.Model).Returns(true);

            // Act
            m_UnderTest.DeleteCategoryCommand.Execute(subCategory);

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
            m_ComponentFacade.Received().DeleteCategory(subCategory.Model);
            Assert.That(rootCategory.SubCategories, Is.Empty);
            Assert.That(rootCategory.IsSelected);
        }

        [Test]
        public void AddPicturesCommand_should_call_SelectNewSymbolsFromFile()
        {
            // Arrange

            // Act
            m_UnderTest.AddPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_ComponentFacade.Received().SelectNewSymbolsFromFile();
        }

        [Test]
        public void RemoveUnusedPicturesCommand_should_do_nothing_when_user_cancels()
        {
            // Arrange
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.RemoveUnusedPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void RemoveUnusedPicturesCommand_should_call_RemoveUnusedSymbols_when_user_confirms()
        {
            // Arrange
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);

            // Act
            m_UnderTest.RemoveUnusedPicturesCommand.Execute(CreateTestCategoryViewModelWithSymbolLibraryCategory());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
            m_ComponentFacade.Received().RemoveUnusedSymbols();
        }

        [Test]
        public void DeleteAllProjectFilesCommand_should_do_nothing_when_user_cancels()
        {
            // Arrange
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.DeleteAllProjectFilesCommand.Execute(CreateTestCategoryViewModelWithProjectFilesCategory());

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void DeleteAllProjectFilesCommand_should_call_RemoveAllFilesFromLibrary_when_user_confirms()
        {
            // Arrange
            var category = CreateTestCategoryViewModelWithProjectFilesCategory();
            m_DialogService.ShowQuestion(Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);
            m_ComponentFacade.RemoveAllComponents(category.Model);

            // Act
            m_UnderTest.DeleteAllProjectFilesCommand.Execute(category);

            // Assert
            m_DialogService.ReceivedWithAnyArgs().ShowQuestion(Arg.Any<string>(), Arg.Any<string>());
            m_ComponentFacade.RemoveAllComponents(category.Model);
        }

        private ComponentCategoryViewModel CreateTestCategoryViewModelWithSymbolLibraryCategory() 
        {
            return new ComponentCategoryViewModel(Substitute.For<SymbolLibraryCategory>(), m_UnderTest, null);
        }

        private ComponentCategoryViewModel CreateTestCategoryViewModelWithProjectFilesCategory()
        {
            return new ComponentCategoryViewModel(Substitute.For<ProjectFilesCategory>(), m_UnderTest, null);
        }


        private ComponentCategoryViewModel CreateTestCategoryViewModel(ComponentCategoryViewModel parent = null)
        {
            return new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest, parent);
        }

        private IComponentCategory CreateTestCategory() 
        {
            var category = Substitute.For<IComponentCategory>();
            category.SubCategories.Returns(Enumerable.Empty<IComponentCategory>());
            return category;
        }
    }
}
#endif
