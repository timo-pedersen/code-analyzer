using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Api.Feature;
using Core.Api.Service;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Features;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    /// <summary>
    /// Unit test on <see cref="ComponentRepository"/>
    /// </summary>
    [TestFixture]
    public class ComponentRepositoryTest
    {
        private IFileSettingsServiceIde m_FileSettings;
        private ComponentRepository m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(@"ComponentLibrary\SubCategory");

            m_FileSettings = MockRepository.GenerateMock<IFileSettingsServiceIde>();
            m_FileSettings.Expect(i => i.CommonApplicationDataFolder).Return(".");

            m_UnderTest = new ComponentRepository(m_FileSettings, new ComponentInfoFactory());
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Directory.Delete("ComponentLibrary", true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Calling FindRootCategories should return three nodes,
        /// one StandardCategory, one SymbolLibraryCategory and one ProjectFilesCategory 
        /// </summary>
        [Test]
        public void FindRootCategories_should_return_three_nodes()
        {
            var actual = m_UnderTest.FindRootCategories().ToArray();

            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actual[0], Is.TypeOf<StandardCategory>());
            Assert.That(actual[1], Is.TypeOf<SymbolLibraryCategory>());
            Assert.That(actual[2], Is.TypeOf<ProjectFilesCategory>());
        }

        /// <summary>
        /// Calling FindComponentsByCategory should find all components in the category and all sub categories
        /// </summary>
        [Test]
        public void FindComponentsByCategory_should_find_all_components()
        {
            // Arrange
            File.Copy("TestComponent.txt", @"ComponentLibrary\RootComponent.txt");
            File.Copy("TestComponent.txt", @"ComponentLibrary\SubCategory\SubComponent.txt");
            var category = m_UnderTest.FindRootCategories().First();

            // Act
            var actual = m_UnderTest.FindComponentsByCategory(category).ToArray();

            // Assert
            Assert.That(actual.Length, Is.EqualTo(2));
            Assert.That(actual[0].DisplayName, Is.EqualTo("RootComponent"));
            Assert.That(actual[1].DisplayName, Is.EqualTo("SubComponent"));
        }

        /// <summary>
        /// Calling AddComponent should create file and component
        /// </summary>
        [Test]
        public void AddComponent_should_create_file_and_component()
        {
            // Arrange
            var category = m_UnderTest.FindRootCategories().First();

            // Act
            var component = m_UnderTest.AddComponent(category, "TestComponent.xaml");

            // Assert
            Assert.That(component, Is.TypeOf<ScreenObjectComponent>());
            Assert.That(File.Exists(@"ComponentLibrary\TestComponent.lib"));
        }

        /// <summary>
        /// Calling AddTextComponent should create text file and component
        /// </summary>
        [Test]
        public void AddTextComponent_should_create_file_and_component()
        {
            // Arrange
            var category = m_UnderTest.FindRootCategories().First();

            // Act
            var component = m_UnderTest.AddTextComponent(category, "SomeText");

            // ASSERT
            Assert.That(component, Is.TypeOf<TextComponent>());
            Assert.That(File.Exists(@"ComponentLibrary\Text.txt"));
        }

        /// <summary>
        /// Calling AddScreenObjectComponent should create file and component
        /// </summary>
        [Test]
        public void AddScreenObjectComponent_should_create_file_and_component()
        {
            // Arrange
            var screenObject = new ScreenObjectComponent("TestComponent.lib");
            var dataObject = (IScreenDataObject)screenObject.CreateDataObject();
            var category = m_UnderTest.FindRootCategories().First();

            // Act
            var component = m_UnderTest.AddScreenObjectComponent(category, dataObject);

            // Assert
            Assert.That(component, Is.TypeOf<ScreenObjectComponent>());
            Assert.That(File.Exists(@"ComponentLibrary\TestComponent.lib"));
        }

        /// <summary>
        /// Calling ImportCategory should unpack the provided zip file and create a category from it.
        /// </summary>
        [Test]
        public void ImportCategory_should_unpack_zip_and_return_new_category()
        {
            // Arrange
            var parentCategory = m_UnderTest.FindRootCategories().First();

            // Act
            var category = m_UnderTest.ImportCategory(parentCategory, "TestCategory.zip");

            // Assert
            Assert.That(category, Is.TypeOf<StandardCategory>());
            Assert.That(Directory.Exists(@"ComponentLibrary\TestCategory"));
            Assert.That(File.Exists(@"ComponentLibrary\TestCategory\TestComponent.txt"));
        }

        /// <summary>
        /// Calling ExportCategory should zip the category directory
        /// </summary>
        [Test]
        public void ExportCategory_should_zip_directory()
        {
            // Arrange
            File.Copy("TestComponent.txt", @"ComponentLibrary\SubCategory\SubComponent.txt");
            var category = m_UnderTest.FindRootCategories().First().SubCategories.First();

            // Act
            m_UnderTest.ExportCategory(category, @"ComponentLibrary\ExportCategory.zip");

            // ASSERT
            Assert.That(File.Exists(@"ComponentLibrary\ExportCategory.zip"));
        }

        /// <summary>
        /// Calling AddCategory should create a new directory and a new StandardCategory
        /// </summary>
        [Test]
        public void AddCategory_should_create_directory_and_category()
        {
            // Arrange
            var parentCategory = m_UnderTest.FindRootCategories().First();

            // Act
            var category = m_UnderTest.AddCategory(parentCategory);

            // Assert
            Assert.That(category, Is.TypeOf<StandardCategory>());
            Assert.That(Directory.Exists(@"ComponentLibrary\NewCategory"));
        }

        /// <summary>
        /// Calling DeleteCategory should delete directory
        /// </summary>
        [Test]
        public void DeleteCategory_should_delete_directory()
        {
            // Arrange
            var parentCategory = m_UnderTest.FindRootCategories().First();
            var category = m_UnderTest.AddCategory(parentCategory);

            // Act
            m_UnderTest.DeleteCategory(category);

            // Assert
            Assert.That(Directory.Exists(@"ComponentLibrary\NewCategory"), Is.False);
        }

        /// <summary>
        /// Calling rename category should rename directory and create new StandardCategory
        /// </summary>
        [Test]
        public void RenameCategory_should_rename_directory_and_create_category()
        {
            // Arrange
            var originalCategory = m_UnderTest.FindRootCategories().First().SubCategories.First();

            // Act
            var renamedCategory = m_UnderTest.RenameCategory(originalCategory, "Renamed");

            // ASSERT
            Assert.That(renamedCategory.Name, Is.EqualTo("Renamed"));
            Assert.That(Directory.Exists(@"ComponentLibrary\SubCategory"), Is.False);
            Assert.That(Directory.Exists(@"ComponentLibrary\Renamed"));
        }

        /// <summary>
        /// Calling DeleteAllComponents should delete all files in the directory of the category
        /// </summary>
        [Test]
        public void DeleteAllComponents_should_delete_all_files()
        {
            // Arrange
            TestHelper.UseTestWindowThreadHelper = true;

            File.Copy("TestComponent.txt", @"ComponentLibrary\ComponentA.txt");
            File.Copy("TestComponent.txt", @"ComponentLibrary\ComponentB.txt");
            var category = m_UnderTest.FindRootCategories().First();

            var featureService = MockRepository.GenerateMock<IFeatureSecurityServiceIde>();
            featureService.Expect(i => i.IsActivated<HidePrefixedComponentFeature>()).Return(false);
            ServiceContainerCF.Instance.AddService(typeof(IFeatureSecurityServiceIde), featureService);

            // Act
            m_UnderTest.DeleteAllComponents(category);

            // Assert
            Assert.That(Directory.EnumerateFiles(@"ComponentLibrary"), Is.Empty);
        }

        /// <summary>
        /// Calling DeleteComponent should delete the component file
        /// </summary>
        [Test]
        public void DeleteComponent_should_delete_file()
        {
            // Arrange
            File.Copy("TestComponent.txt", @"ComponentLibrary\TestComponent.txt");
            var category = m_UnderTest.FindRootCategories().First();
            var component = m_UnderTest.FindComponentsByCategory(category).First();

            // Act
            m_UnderTest.DeleteComponent(component);

            // Assert
            Assert.That(File.Exists(@"ComponentLibrary\TestComponent.txt"), Is.False);
        }

        /// <summary>
        /// Calling RenameComponent should rename the component file and create a new component with the new name
        /// </summary>
        [Test]
        public void RenameComponent_should_rename_file_and_create_component()
        {
            // Arrange
            File.Copy("TestComponent.txt", @"ComponentLibrary\TestComponent.txt");
            var category = m_UnderTest.FindRootCategories().First();
            var originalComponent = m_UnderTest.FindComponentsByCategory(category).First();

            // Act
            var newComponent = m_UnderTest.RenameComponent(category, originalComponent, "RenamedComponent");

            // Assert
            Assert.That(newComponent.DisplayName, Is.EqualTo("RenamedComponent"));
            Assert.That(File.Exists(@"ComponentLibrary\TestComponent.txt"), Is.False);
            Assert.That(File.Exists(@"ComponentLibrary\RenamedComponent.txt"));
        }
    }
}
