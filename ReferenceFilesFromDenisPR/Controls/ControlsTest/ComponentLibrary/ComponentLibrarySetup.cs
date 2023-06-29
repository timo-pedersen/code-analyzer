#if!VNEXT_TARGET
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Neo.ApplicationFramework.Common.Utilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary
{
    [SetUpFixture]
    public class ComponentLibrarySetup
    {
        private Window m_Window;

        /// <summary>
        /// This is run once before any tests in the ComponentLibrary namespace
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

            // Create a Window instance to force the uri scheme "pack://application,,," to be registered.
            m_Window = new Window();
            m_Window.Show();

            // Copy TestData resources to files
            CopyResourceToFile("TestComponent.txt");
            CopyResourceToFile("TestComponent.jpg");
            CopyResourceToFile("TestComponent.lib");
            CopyResourceToFile("TestComponent.wmv");
            CopyResourceToFile("TestComponent.tmp");
            CopyResourceToFile("TestComponent.xaml");
            CopyResourceToFile("TestCategory.zip");
            CopyResourceToFile("TestComponent.mp3");
        }

        /// <summary>
        /// This is run once after all tests in the ComponentLibrary namespace
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_Window.Close();
        }

        /// <summary>
        /// Copy test data from embedded resource to a file
        /// </summary>
        /// <param name="name">The resource name</param>
        private void CopyResourceToFile(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resource = "Neo.ApplicationFramework.Controls.ComponentLibrary.TestData." + name;
            using (var stream = asm.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var file = File.Create(name))
                    {
                        stream.CopyTo(file);
                    }
                }
            }
        }
    }
}
#endif
