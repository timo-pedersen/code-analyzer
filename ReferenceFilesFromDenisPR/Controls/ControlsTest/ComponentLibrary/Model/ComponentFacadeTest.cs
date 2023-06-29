#if!VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    /// <summary>
    /// Unit test of <see cref="ComponentFacade"/>
    /// </summary>
    [TestFixture]
    public class ComponentFacadeTest
    {
        private IComponentRepository m_Repository;
        private IInformationProgressService m_ProgressService;
        private ISymbolServiceIde m_SymbolService;
        private IProjectManager m_ProjectManager;
        private ComponentFacade m_UnderTest;
        private bool m_DataChangeEventFired;
            
        [SetUp]
        public void SetUp()
        {
            m_Repository = Substitute.For<IComponentRepository>();
            m_ProgressService = Substitute.For<IInformationProgressService>();
            m_SymbolService = Substitute.For<ISymbolServiceIde>();
            m_ProjectManager = Substitute.For<IProjectManager>();
            m_DataChangeEventFired = false;

            m_UnderTest = new ComponentFacade(
                m_Repository.ToILazy(),
                m_ProgressService.ToILazy(),
                m_SymbolService.ToILazy(),
                m_ProjectManager.ToILazy());
            m_UnderTest.DataChanged += (sender, args) => m_DataChangeEventFired = true;

        }

        /// <summary>
        /// Calling FindCategories should retrieve root categories from repository
        /// </summary>
        [Test]
        public void FindCategories_should_return_RootCategories()
        {
            // Arrange
            var rootCategories = Enumerable.Range(0, 2).Select(_ => Substitute.For<IComponentCategory>());
            m_Repository.FindRootCategories().Returns(rootCategories);

            // Act
            var actual = m_UnderTest.FindRootCategories();

            // Assert
            Assert.That(actual, Is.SameAs(rootCategories));
            m_Repository.Received().FindRootCategories();
        }

        /// <summary>
        /// Calling FindComponentsByCategory should retreive all categories from repository then divide it to pages
        /// </summary>
        [Test]
        public void FindComponentsByCategory_should_return_PagedResult()
        {
            // Arrange
            var components = Enumerable.Range(0, 10).Select(_ => Substitute.For<IComponentInfo>()).ToArray();
            var category = Substitute.For<IComponentCategory>();
            m_Repository.FindComponentsByCategory(category).Returns(components);

            // Act
            var actual = m_UnderTest.FindComponentsByCategory(category, new ComponentAllFilter(), 1, 3);

            // Assert
            Assert.That(actual.PageNumber, Is.EqualTo(1));
            Assert.That(actual.TotalPages, Is.EqualTo(4));
            Assert.That(actual.Data, Is.EquivalentTo(components.Take(3)));
            m_Repository.Received().FindComponentsByCategory(category);
        }

        /// <summary>
        /// Calling FindPageNumber should return the page number of the component
        /// </summary>
        /// <param name="componentIndex">Component index</param>
        /// <param name="pageSize">Size of page</param>
        /// <param name="expected">Expected result</param>
        [Test]
        [TestCase(1, 5, 1)]
        [TestCase(4, 2, 3)]
        [TestCase(9, 3, 4)]
        [TestCase(0, 5, 1)]
        public void FindPageNumber_should_return_page_number_of_component(int componentIndex, int pageSize, int expected)
        {
            // Arrange
            var components = Enumerable.Range(0, 10).Select(_ => Substitute.For<IComponentInfo>()).ToArray();
            var category = Substitute.For<IComponentCategory>();
            m_Repository.FindComponentsByCategory(category).Returns(components);

            // Act
            var actual = m_UnderTest.FindPageNumber(category, new ComponentAllFilter(), components[componentIndex], pageSize);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
            m_Repository.Received().FindComponentsByCategory(category);
        }

        /// <summary>
        /// Calling CanAddComponent should return true if the category is not read-only and
        /// the dataformat is supported
        /// </summary>
        /// <param name="categoryReadOnly">Category read-only flag</param>
        /// <param name="format">Data fromat</param>
        /// <param name="expected">Expected result</param>
        [Test]
        [TestCase(false, "FileDrop", true)]
        [TestCase(true, "FileDrop", false)]
        [TestCase(false, "Text", true)]
        [TestCase(false, "Neo.ApplicationFramework.Common.ScreenDataObject", true)]
        [TestCase(false, "SomeFormat", false)]
        public void CanAddComponent_should_return_true_for_some_data_formats(bool categoryReadOnly, string format, bool expected)
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            category.IsReadOnly.Returns(categoryReadOnly);

            var dataObject = Substitute.For<IDataObject>();
            dataObject.GetDataPresent(format).Returns(true);
            dataObject.GetDataPresent(Arg.Any<string>()).Returns(false);

            // Act
            var actual = m_UnderTest.CanAddComponent(category, dataObject);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
            dataObject.Received().GetDataPresent(format);
            dataObject.ReceivedWithAnyArgs().GetDataPresent(Arg.Any<string>());
        }

        /// <summary>
        /// Calling AddComponents with FileDrop format should return created components and fire the DataChanged event
        /// </summary>
        [Test]
        public void AddComponents_with_FileDrop_format_should_return_created_component()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var files = new[] { "FileA", "FileB" };
            var dataObject = Substitute.For<IDataObject>();
            dataObject.GetDataPresent(DataFormats.FileDrop).Returns(true);
            dataObject.GetData(DataFormats.FileDrop).Returns(files);

            var componentA = Substitute.For<IComponentInfo>();
            var componentB = Substitute.For<IComponentInfo>();
            m_Repository.AddComponent(category, files[0]).Returns(componentA);
            m_Repository.AddComponent(category, files[1]).Returns(componentB);
            var expected = new[] { componentA, componentB };

            // Act
            var actual = m_UnderTest.AddComponents(category, dataObject);

            // Assert
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// Calling AddComponents with Text format should return created component and fire the DataChanged event
        /// </summary>
        [Test]
        public void AddComponents_with_Text_format_should_return_created_component()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();

            var dataObject = Substitute.For<IDataObject>();
            dataObject.GetDataPresent(DataFormats.Text).Returns(true);
            dataObject.GetDataPresent(Arg.Any<string>()).Returns(false);
            dataObject.GetData(DataFormats.Text, false).Returns("TextData");

            var component = Substitute.For<IComponentInfo>();
            m_Repository.AddTextComponent(category, "TextData").Returns(component);
            var expected = new [] { component };

            // Act
            var actual = m_UnderTest.AddComponents(category, dataObject);

            // Assert
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.That(m_DataChangeEventFired, Is.True);
            dataObject.Received().GetDataPresent(DataFormats.Text);
            dataObject.ReceivedWithAnyArgs().GetDataPresent(Arg.Any<string>());
        }

        /// <summary>
        /// Calling AddComponents with ScreenDataObject from should return created component and fire the DataChanged event
        /// </summary>
        [Test]
        public void AddComponents_with_ScreenDataObject_format_should_return_created_component()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();

            var dataObject = Substitute.For<IDataObject>();
            var data = new ScreenDataObject();
            dataObject.GetDataPresent(ScreenDataObject.ClipboardFormat).Returns(true);
            dataObject.GetDataPresent(Arg.Any<string>()).Returns(false);
            dataObject.GetData(ScreenDataObject.ClipboardFormat).Returns(data);

            var component = Substitute.For<IComponentInfo>();
            m_Repository.AddScreenObjectComponent(category, data).Returns(component);
            var expected = new [] { component };

            // Act
            var actual = m_UnderTest.AddComponents(category, dataObject);

            // Assert
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.That(m_DataChangeEventFired, Is.True);
            dataObject.Received().GetDataPresent(ScreenDataObject.ClipboardFormat);
            dataObject.ReceivedWithAnyArgs().GetDataPresent(Arg.Any<string>());
        }

        /// <summary>
        /// Calling AddComponents with unknown data format should return empty collection and not fire DataChanged event
        /// </summary>
        [Test]
        public void AddComponents_with_unknown_format_should_return_null()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var dataObject = Substitute.For<IDataObject>();
            dataObject.GetDataPresent(Arg.Any<string>()).Returns(false);

            // Act
            var actual = m_UnderTest.AddComponents(category, dataObject);

            // Assert
            Assert.That(actual, Is.Empty);
            Assert.That(m_DataChangeEventFired, Is.False);
            dataObject.ReceivedWithAnyArgs().GetDataPresent(Arg.Any<string>());
        }

        /// <summary>
        /// Calling AddComponents should return created component and fire the DataChanged event
        /// </summary>
        [Test]
        public void AddComponents_should_return_created_component()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var component = Substitute.For<IComponentInfo>();
            m_Repository.AddComponent(category, "TestFile").Returns(component);

            // Act
            IEnumerable<IComponentInfo> actual = m_UnderTest.AddComponents(category, new[] { "TestFile" });

            // Assert
            Assert.That(actual.First(), Is.SameAs(component));
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// Calling ImportComponents should return category created from the file
        /// </summary>
        [Test]
        public void ImportComponents_should_return_created_category()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var newCategory = Substitute.For<IComponentCategory>();
            m_Repository.ImportCategory(category, "TestFile").Returns(newCategory);

            // Act
            var actual = m_UnderTest.ImportComponents(category, "TestFile");

            // Assert
            Assert.That(actual, Is.SameAs(newCategory));
        }

        /// <summary>
        /// Calling ExportComponent should forward the call to the repository
        /// </summary>
        [Test]
        public void ExportComponent_should_call_Repository()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            m_Repository.ExportCategory(category, "TestFile");

            // Act
            m_UnderTest.ExportComponents(category, "TestFile");

            // Assert
            m_Repository.Received().ExportCategory(category, "TestFile");
        }

        /// <summary>
        /// Calling AddCategory should return created category
        /// </summary>
        [Test]
        public void AddCategory_should_return_created_category()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var newCategory = Substitute.For<IComponentCategory>();
            m_Repository.AddCategory(category).Returns(newCategory);

            // Act
            var actual = m_UnderTest.AddCategory(category);

            // Assert
            Assert.That(actual, Is.SameAs(newCategory));
        }

        /// <summary>
        /// Calling DeleteCategory should forward the call to the repository
        /// </summary>
        [Test]
        public void DeleteCategory_should_call_Repository()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            m_Repository.DeleteCategory(category);

            // Act
            var actual = m_UnderTest.DeleteCategory(category);

            // Assert
            Assert.That(actual, Is.True);
            m_Repository.Received().DeleteCategory(category);
        }

        /// <summary>
        /// Calling RenamedCategory should return a new category with the new name
        /// </summary>
        [Test]
        public void RenameCategory_should_return_renamed_category()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var newCategory = Substitute.For<IComponentCategory>();
            m_Repository.RenameCategory(category, "NewName").Returns(newCategory);

            // Act
            var actual = m_UnderTest.RenameCategory(category, "NewName");

            // Assert
            Assert.That(actual, Is.SameAs(newCategory));
        }

        /// <summary>
        /// Calling SelectNewSymbolsFromFile should forward the call to the symbol service
        /// </summary>
        [Test]
        public void SelectNewSymbolsFromFile_should_call_SymbolService()
        {
            // Arrange
            m_SymbolService.SelectNewSymbolsFromFile().Returns(Enumerable.Empty<string>());

            // Act
            m_UnderTest.SelectNewSymbolsFromFile();

            // Assert
            m_SymbolService.Received().SelectNewSymbolsFromFile();
        }

        /// <summary>
        /// Calling RemoveUnusedSymbols should forward the call to the symbol service and fire the DataChanged event
        /// </summary>
        [Test]
        public void RemoveUnusedSymbols_should_call_SymbolService()
        {
            // Arrange
            m_SymbolService.RemoveUnusedSymbols();

            // Act
            m_UnderTest.RemoveUnusedSymbols();

            // Assert
            m_SymbolService.Received().RemoveUnusedSymbols();
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// Calling RemoveAllComponents should forward the call to the repository and fire the DataChanged event
        /// </summary>
        [Test]
        public void RemoveAllComponents_should_call_Repository()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            m_Repository.DeleteAllComponents(category);

            // Act
            m_UnderTest.RemoveAllComponents(category);

            // Assert
            m_Repository.Received().DeleteAllComponents(category);
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// CanDeleteComponent should return false if category is read-only.
        /// </summary>
        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void CanDeleteComponent_if_category_is_not_readonly(bool isReadOnly, bool expected)
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            category.IsReadOnly.Returns(isReadOnly);

            var component = Substitute.For<IComponentInfo>();

            // Act
            var actual = m_UnderTest.CanDeleteComponent(category, component);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// Calling DeleteComponent should forward the call to the repository and fire the DataChanged event
        /// </summary>
        [Test]
        public void DeleteComponent_should_call_LibraryManager()
        {
            // Arrange
            var component = Substitute.For<IComponentInfo>();
            m_Repository.DeleteComponent(component);

            // Act
            m_UnderTest.DeleteComponent(component);

            // Assert
            m_Repository.DeleteComponent(component);
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// CanRenameComponent should return false if category is read-only
        /// </summary>
        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void CanRenameComponent_if_category_is_not_readonly(bool isReadOnly, bool expected)
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            category.IsReadOnly.Returns(isReadOnly);

            var component = Substitute.For<IComponentInfo>();

            // Act
            var actual = m_UnderTest.CanRenameComponent(category, component);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// RenameComponent should return new component with new name
        /// </summary>
        [Test]
        public void RenameComponent_should_return_renamed_component()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            var component = Substitute.For<IComponentInfo>();
            var newComponent = Substitute.For<IComponentInfo>();
            m_Repository.RenameComponent(category, component, "NewName").Returns(newComponent);

            // Act
            var actual = m_UnderTest.RenameComponent(category, component, "NewName");

            // Assert
            Assert.That(actual, Is.SameAs(newComponent));
        }

        /// <summary>
        /// The ItemsChanged event from SymbolService should fire the DataChanged event
        /// </summary>
        [Test]
        public void ItemsChanged_from_SymbolService_should_fire_DataChanged()
        {
            Raise.Event();
            Assert.That(m_DataChangeEventFired, Is.True);
        }

        /// <summary>
        /// The ItemsChanged event from SymbolService should not fire the DataChanged event
        /// if the project is closing down
        /// </summary>
        [Test]
        public void ItemsChanged_from_SymbolService_should_not_fire_DataChanged_when_closing_project()
        {
            // Arrange
            m_ProjectManager.IsProjectClosing.Returns(true);

            // Act
            Raise.Event();

            // Assert
            Assert.That(m_DataChangeEventFired, Is.False);
        }
    }
}
#endif
