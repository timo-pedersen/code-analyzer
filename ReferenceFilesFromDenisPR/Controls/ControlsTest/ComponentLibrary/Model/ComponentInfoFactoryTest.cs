#if!VNEXT_TARGET
using System;
using System.IO;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    /// <summary>
    /// Unit tests on <see cref="ComponentInfoFactory"/>
    /// </summary>
    [TestFixture]
    public class ComponentInfoFactoryTest
    {
        private ComponentInfoFactory m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_UnderTest = new ComponentInfoFactory();
        }

        /// <summary>
        /// Calling CreateFromFile should return a component of the correct type
        /// </summary>
        /// <param name="file">The file name</param>
        /// <param name="componentType">Type of component to create</param>
        [Test]
        [TestCase("TestComponent.txt", typeof(TextComponent))]
        [TestCase("TestComponent.jpg", typeof(ImageComponent))]
        [TestCase("TestComponent.lib", typeof(ScreenObjectComponent))]
        [TestCase("TestComponent.wmv", typeof(VideoComponent))]
        [TestCase("TestComponent.tmp", typeof(OtherComponent))]
        [TestCase("TestComponent.mp3", typeof(AudioComponent))]
        public void CreateFromFile_should_return_component(string file, Type componentType)
        {
            Assert.That(m_UnderTest.CreateFromFile(file), Is.TypeOf(componentType));
        }

        /// <summary>
        /// Calling CreateCopyFromFile should copy the file and create a new component
        /// </summary>
        [Test]
        public void CreateCopyFromFile_should_copy_file_and_create_component()
        {
            try
            {
                // Arrange
                var category = Substitute.For<IComponentCategory>();
                category.FileSystemPath.Returns(".");

                // Act
                var component = m_UnderTest.CreateCopyFromFile("TestComponent.txt", category);

                // Assert
                Assert.That(component, Is.TypeOf<TextComponent>());
                Assert.That(component.DisplayName, Is.EqualTo("TestComponent1"));
                Assert.That(File.Exists("TestComponent1.txt"));
            }
            finally
            {
                // Clean up
                if (File.Exists("TestComponent1.txt"))
                {
                    File.Delete("TestComponent1.txt");
                }
            }
        }

        /// <summary>
        /// Calling TryCreateVectorComponentFromXaml should return true when xaml is valid
        /// </summary>
        [Test]
        public void TryCreateVectorComponentFromXaml_should_succeed_with_valid_xaml()
        {
            string newSymbolPath;
            var result = m_UnderTest.TryCreateVectorComponentFromXaml("TestSymbol", "TestComponent.xaml", out newSymbolPath);

            Assert.That(result, Is.True);
            Assert.That(Path.GetFileName(newSymbolPath), Is.EqualTo("TestSymbol.lib"));            
        }

        /// <summary>
        /// Calling TryCreateVectorComponentFromXaml should return false when xaml is invalid
        /// </summary>
        [Test]
        public void TryCreateVectorComponentFromXaml_should_fail_with_invalid_xaml()
        {
            string newSymbolPath;
            var result = m_UnderTest.TryCreateVectorComponentFromXaml("TestSymbol", "TestComponent.tmp", out newSymbolPath);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Calling CreateFromText should create a text component and a file with the text
        /// </summary>
        [Test]
        public void CreateFromText_should_create_TextComponent()
        {
            // Arrange
            var category = Substitute.For<IComponentCategory>();
            category.FileSystemPath.Returns(".");

            // Act
            var component = m_UnderTest.CreateFromText("SomeText", category);

            // Assert
            Assert.That(component, Is.TypeOf<TextComponent>());
            Assert.That(File.Exists(component.FullFileName));
            Assert.That(File.ReadAllText(component.FullFileName), Is.EqualTo("SomeText"));
        }

        /// <summary>
        /// Calling CreateFromObjectAndSave should create a new ScreenObjectComponent and a new file
        /// </summary>
        [Test]
        public void CreateFromObjectAndSave_should_create_object_and_a_file()
        {
            try
            {
                // Arrange
                var component = new ScreenObjectComponent("TestComponent.lib");
                var dataObject = (IScreenDataObject)component.CreateDataObject();
                var category = Substitute.For<IComponentCategory>();
                category.FileSystemPath.Returns(".");

                // Act
                var result = m_UnderTest.CreateFromObjectAndSave(dataObject, category);

                // Assert
                Assert.That(result, Is.TypeOf<ScreenObjectComponent>());
                Assert.That(result.DisplayName, Is.EqualTo("TestComponent1"));
                Assert.That(File.Exists("TestComponent1.lib"));
            }
            finally
            {
                // Clean up
                if (File.Exists("TestComponent1.lib"))
                {
                    File.Delete("TestComponent1.lib");
                }
            }
        }
    }
}
#endif
