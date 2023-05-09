using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Neo.ApplicationFramework.Common.FrameworkDialogs;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.ViewModel
{
    [TestFixture]
    public class ComponentLibraryControlViewModelTest
    {
        private IComponentFacade m_ComponentFacade;
        private IDialogService m_DialogService;
        private ComponentLibraryControlViewModel m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_ComponentFacade = MockRepository.GenerateMock<IComponentFacade>();
            m_DialogService = MockRepository.GenerateMock<IDialogService>();
            m_UnderTest = new ComponentLibraryControlViewModel(m_ComponentFacade, m_DialogService, new NameComponentFilter());
            
        }

        [Test]
        public void Should_fire_PropertyChanged()
        {
            Assert.That(m_UnderTest.NotifiesOn(x => x.Components).When(x => x.Components = new ObservableCollection<ComponentInfoViewModel>()));
            Assert.That(m_UnderTest.NotifiesOn(x => x.ComponentInfoSize).When(x => x.ComponentInfoSize = 50));
            Assert.That(m_UnderTest.NotifiesOn(x => x.Filter).When(x => x.Filter = "SomeFilter"));
        }

        [Test]
        public void Size_of_all_components_should_be_updated_when_ComponentInfoSize_changed()
        {
            // Arrange
            var component = new ComponentInfoViewModel(CreateTestComponent(), m_UnderTest);
            m_UnderTest.Components.Add(component);

            // Act
            m_UnderTest.ComponentInfoSize = 100;

            // Assert
            Assert.That(component.Size, Is.EqualTo(100));
            Assert.That(m_UnderTest.ComponentInfoSize, Is.EqualTo(100));
        }

        [Test]
        public void First_page_of_components_should_be_shown_when_category_selected()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 3 });
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;

            // Assert
            Assert.That(m_UnderTest.PagingControlViewModel.PageNumber, Is.EqualTo(1));
            Assert.That(m_UnderTest.PagingControlViewModel.TotalPages, Is.EqualTo(3));
            Assert.That(m_UnderTest.Components.Count, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void Component_list_should_be_updated_when_filter_changed()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 1 })
                .Repeat.Twice();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.Filter = "FilterString";

            // Assert
            Assert.That(m_UnderTest.Filter, Is.EqualTo("FilterString"));
            Assert.That(m_UnderTest.PagingControlViewModel.PageNumber, Is.EqualTo(1));
            Assert.That(m_UnderTest.PagingControlViewModel.TotalPages, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components.Count, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void DataChanged_event_should_refresh_data_by_navigating_to_first_page()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 1 })
                .Repeat.Twice();
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_ComponentFacade.Raise(i => i.DataChanged += null, m_ComponentFacade, EventArgs.Empty);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void CanAddComponent_should_call_ComponentFacade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = MockRepository.GenerateMock<IDataObject>();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)).Return(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.Expect(i => i.CanAddComponent(category.Model, dataObject)).Return(true);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.CanAddComponent(dataObject);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.True);
        }

        [Test]
        public void AddComponent_should_do_nothing_if_component_not_added()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = MockRepository.GenerateMock<IDataObject>();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)).Return(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.Expect(i => i.AddComponents(category.Model, dataObject)).Return(Enumerable.Empty<IComponentInfo>());

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.AddComponent(dataObject);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
        }


        [Test]
        public void AddComponent_should_show_and_selected_added_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = MockRepository.GenerateMock<IDataObject>();
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component } }).Repeat.Any();
            m_ComponentFacade.Expect(i => i.AddComponents(category.Model, dataObject)).Return(new[] { component });
            m_ComponentFacade.Expect(i => i.FindPageNumber(category.Model, m_UnderTest.ComponentInfoFilter, component, 100)).Return(1);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.AddComponent(dataObject);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(m_UnderTest.Components[0].IsSelected);
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
        }

        [Test]
        public void Should_disable_commands_when_nothing_selected()
        {
            Assert.That(m_UnderTest.CutComponentCommand.CanExecute(null), Is.False);
            Assert.That(m_UnderTest.CopyComponentCommand.CanExecute(null), Is.False);
            Assert.That(m_UnderTest.DeleteComponentCommand.CanExecute(null), Is.False);
            Assert.That(m_UnderTest.RenameComponentCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void CutCommand_should_be_disabled_by_the_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.Expect(i => i.CanDeleteComponent(category.Model, component)).Return(false);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.CutComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.False);
        }

        [Test]
        public void CutCommand_should_remove_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.CanDeleteComponent(Arg<IComponentCategory>.Is.Anything, (Arg<IComponentInfo>.Is.Anything))).Return(true);
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.Expect(i => i.DeleteComponent(component));
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.CutComponentCommand.Execute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(m_UnderTest.Components, Is.Empty);
        }

        [Test]
        public void CopyCommand_should_call_CopyToClipboard()
        {
            // Arrange
            var component = CreateTestComponent();
            var componentVm = new ComponentInfoViewModel(component, m_UnderTest);
            m_UnderTest.Components.Add(componentVm);
            component.Expect(i => i.CopyToClipboard());
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CopyComponentCommand.Execute(componentVm);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
        }

        [Test]
        public void PasteCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.Expect(i => i.CanAddComponent(category.Model, NeoClipboard.GetDataObject())).IgnoreArguments().Return(false);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.PasteComponentCommand.CanExecute(null);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.False);
        }

        [Test]
        public void PasteComponent_should_show_and_selected_added_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100))
                .Return(new PagedResult<IComponentInfo> { Data = new[] { component } })
                .Repeat.Twice();
            m_ComponentFacade.Expect(i => i.AddComponents(category.Model, NeoClipboard.GetDataObject())).IgnoreArguments().Return(new[] { component });
            m_ComponentFacade.Expect(i => i.FindPageNumber(category.Model, m_UnderTest.ComponentInfoFilter, component, 100)).Return(1);
            m_ComponentFacade.Expect(i => i.CanAddComponent(Arg<IComponentCategory>.Is.Anything, Arg<IDataObject>.Is.Anything)).Return(true);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.PasteComponentCommand.Execute(null);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(m_UnderTest.Components[0].IsSelected);
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
        }

        [Test]
        public void DeleteCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)).Return(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.Expect(i => i.CanDeleteComponent(category.Model, component)).Return(false);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.DeleteComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.False);
        }

        [Test]
        public void DeleteCommand_should_remove_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)).Return(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.Expect(i => i.CanDeleteComponent(Arg<IComponentCategory>.Is.Anything, Arg<IComponentInfo>.Is.Anything)).Return(true);
            m_ComponentFacade.Expect(i => i.DeleteComponent(component));
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.DeleteComponentCommand.Execute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(m_UnderTest.Components, Is.Empty);
        }

        [Test]
        public void RenameCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)).Return(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.Expect(i => i.CanRenameComponent(category.Model, component)).Return(false);
            m_ComponentFacade.Replay();

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.RenameComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.VerifyAllExpectations();
            Assert.That(actual, Is.False);
        }

        [Test]
        public void RenameCommand_should_set_IsEditMode()
        {
            // Arrange
            var componentInfo = CreateTestComponent();
            var componentViewModel = new ComponentInfoViewModel(componentInfo, m_UnderTest);
            m_ComponentFacade.Expect(i => i.CanRenameComponent(Arg<IComponentCategory>.Is.Anything, Arg<IComponentInfo>.Is.Anything)).Return(true);
            m_ComponentFacade.Expect(i => i.FindComponentsByCategory(Arg<IComponentCategory>.Is.Anything, Arg<IComponentInfoFilter>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(new PagedResult<IComponentInfo> { Data = new[] { componentInfo }, PageNumber = 1, TotalPages = 1 });
            
            m_UnderTest.CategoryTree.SelectedCategory = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);

            // Act
            m_UnderTest.RenameComponentCommand.Execute(componentViewModel);

            // Assert
            Assert.That(componentViewModel.IsEditMode);
        }

        private IComponentCategory CreateTestCategory()
        {
            var category = MockRepository.GenerateMock<IComponentCategory>();
            
            category.Expect(i => i.SubCategories).Return(Enumerable.Empty<IComponentCategory>());
            return category;
        }

        private IComponentInfo CreateTestComponent()
        {
            var tcs = new TaskCompletionSource<ImageSource>();
            tcs.SetResult(null);
            var component = MockRepository.GenerateMock<IComponentInfo>();
            component.Expect(i => i.LoadThumbnailAsync()).Return(tcs.Task);
            return component;
        }
    }
}
