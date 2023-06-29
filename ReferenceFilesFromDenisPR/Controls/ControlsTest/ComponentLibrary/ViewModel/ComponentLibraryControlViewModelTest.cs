#if!VNEXT_TARGET
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
using NSubstitute;
using NUnit.Framework;

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
            m_ComponentFacade = Substitute.For<IComponentFacade>();
            m_DialogService = Substitute.For<IDialogService>();
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
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 3 });

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;

            // Assert
            Assert.That(m_UnderTest.PagingControlViewModel.PageNumber, Is.EqualTo(1));
            Assert.That(m_UnderTest.PagingControlViewModel.TotalPages, Is.EqualTo(3));
            Assert.That(m_UnderTest.Components.Count, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
        }

        [Test]
        public void Component_list_should_be_updated_when_filter_changed()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 1 });

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.Filter = "FilterString";

            // Assert
            Assert.That(m_UnderTest.Filter, Is.EqualTo("FilterString"));
            Assert.That(m_UnderTest.PagingControlViewModel.PageNumber, Is.EqualTo(1));
            Assert.That(m_UnderTest.PagingControlViewModel.TotalPages, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components.Count, Is.EqualTo(1));
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
            m_ComponentFacade.Received(2).FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
        }

        [Test]
        public void DataChanged_event_should_refresh_data_by_navigating_to_first_page()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component }, PageNumber = 1, TotalPages = 1 });

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            Raise.Event();

            // Assert
            m_ComponentFacade.Received(2).FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
        }

        [Test]
        public void CanAddComponent_should_call_ComponentFacade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = Substitute.For<IDataObject>();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.CanAddComponent(category.Model, dataObject).Returns(true);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.CanAddComponent(dataObject);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void AddComponent_should_do_nothing_if_component_not_added()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = Substitute.For<IDataObject>();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.AddComponents(category.Model, dataObject).Returns(Enumerable.Empty<IComponentInfo>());

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.AddComponent(dataObject);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
        }


        [Test]
        public void AddComponent_should_show_and_selected_added_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var dataObject = Substitute.For<IDataObject>();
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.AddComponents(category.Model, dataObject).Returns(new[] { component });
            m_ComponentFacade.FindPageNumber(category.Model, m_UnderTest.ComponentInfoFilter, component, 100).Returns(1);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.AddComponent(dataObject);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
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
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.CanDeleteComponent(category.Model, component).Returns(false);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.CutComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void CutCommand_should_remove_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.CanDeleteComponent(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfo>()).Returns(true);
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.CutComponentCommand.Execute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.ReceivedWithAnyArgs().CanDeleteComponent(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfo>());
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.Received().DeleteComponent(component);
            Assert.That(m_UnderTest.Components, Is.Empty);
        }

        [Test]
        public void CopyCommand_should_call_CopyToClipboard()
        {
            // Arrange
            var component = CreateTestComponent();
            var componentVm = new ComponentInfoViewModel(component, m_UnderTest);
            m_UnderTest.Components.Add(componentVm);

            // Act
            m_UnderTest.CopyComponentCommand.Execute(componentVm);

            // Assert
            component.Received().CopyToClipboard();
        }

        [Test]
        public void PasteCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = Enumerable.Empty<IComponentInfo>() });
            m_ComponentFacade.CanAddComponent(category.Model, NeoClipboard.GetDataObject()).ReturnsForAnyArgs(false);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.PasteComponentCommand.CanExecute(null);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.ReceivedWithAnyArgs().CanAddComponent(category.Model, NeoClipboard.GetDataObject());
            Assert.That(actual, Is.False);
        }

        [Test]
        public void PasteComponent_should_show_and_selected_added_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.AddComponents(category.Model, NeoClipboard.GetDataObject()).Returns(new[] { component });
            m_ComponentFacade.FindPageNumber(category.Model, m_UnderTest.ComponentInfoFilter, component, 100).Returns(1);
            m_ComponentFacade.CanAddComponent(Arg.Any<IComponentCategory>(), Arg.Any<IDataObject>()).Returns(true);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.PasteComponentCommand.Execute(null);

            // Assert
            m_ComponentFacade.Received(2).FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.Received().AddComponents(category.Model, NeoClipboard.GetDataObject());
            m_ComponentFacade.Received().FindPageNumber(category.Model, m_UnderTest.ComponentInfoFilter, component, 100);
            m_ComponentFacade.Received().CanAddComponent(Arg.Any<IComponentCategory>(), Arg.Any<IDataObject>());
            Assert.That(m_UnderTest.Components[0].IsSelected);
            Assert.That(m_UnderTest.Components[0].Model, Is.SameAs(component));
        }

        [Test]
        public void DeleteCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.CanDeleteComponent(category.Model, component).Returns(false);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.DeleteComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.Received().CanDeleteComponent(category.Model, component);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void DeleteCommand_should_remove_component()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.CanDeleteComponent(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfo>()).Returns(true);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            m_UnderTest.DeleteComponentCommand.Execute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.ReceivedWithAnyArgs().CanDeleteComponent(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfo>());
            m_ComponentFacade.Received().DeleteComponent(component);
            Assert.That(m_UnderTest.Components, Is.Empty);
        }

        [Test]
        public void RenameCommand_should_be_disabled_by_facade()
        {
            // Arrange
            var category = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);
            var component = CreateTestComponent();
            m_ComponentFacade.FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100)
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { component } });
            m_ComponentFacade.CanRenameComponent(category.Model, component).Returns(false);

            // Act
            m_UnderTest.CategoryTree.SelectedCategory = category;
            var actual = m_UnderTest.RenameComponentCommand.CanExecute(m_UnderTest.Components[0]);

            // Assert
            m_ComponentFacade.Received().FindComponentsByCategory(category.Model, m_UnderTest.ComponentInfoFilter, 1, 100);
            m_ComponentFacade.Received().CanRenameComponent(category.Model, component);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void RenameCommand_should_set_IsEditMode()
        {
            // Arrange
            var componentInfo = CreateTestComponent();
            var componentViewModel = new ComponentInfoViewModel(componentInfo, m_UnderTest);
            m_ComponentFacade.CanRenameComponent(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfo>()).Returns(true);
            m_ComponentFacade.FindComponentsByCategory(Arg.Any<IComponentCategory>(), Arg.Any<IComponentInfoFilter>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(new PagedResult<IComponentInfo> { Data = new[] { componentInfo }, PageNumber = 1, TotalPages = 1 });
            
            m_UnderTest.CategoryTree.SelectedCategory = new ComponentCategoryViewModel(CreateTestCategory(), m_UnderTest.CategoryTree, null);

            // Act
            m_UnderTest.RenameComponentCommand.Execute(componentViewModel);

            // Assert
            Assert.That(componentViewModel.IsEditMode);
        }

        private IComponentCategory CreateTestCategory()
        {
            var category = Substitute.For<IComponentCategory>();
            
            category.SubCategories.Returns(Enumerable.Empty<IComponentCategory>());
            return category;
        }

        private IComponentInfo CreateTestComponent()
        {
            var tcs = new TaskCompletionSource<ImageSource>();
            tcs.SetResult(null);
            var component = Substitute.For<IComponentInfo>();
            component.LoadThumbnailAsync().Returns(tcs.Task);
            return component;
        }
    }
}
#endif
