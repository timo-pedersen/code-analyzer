#if !VNEXT_TARGET
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class StandardCategoryTest
    {
        private StandardCategory m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(@".\BaseCategory\SubCategory");
            m_UnderTest = new StandardCategory(@".\BaseCategory");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Directory.Delete(@".\BaseCategory", true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Failed to delete directory .\BaseCategory " + e.Message);
            }
        }

        [Test]
        public void PropertiesTest()
        {
            Assert.That(m_UnderTest.Name, Is.EqualTo("BaseCategory"));
            Assert.That(m_UnderTest.FileSystemPath, Is.EqualTo(@".\BaseCategory"));
            Assert.That(m_UnderTest.IsReadOnly, Is.False);
        }

        [Test]
        public void SubCategoriesTest()
        {
            Assert.That(m_UnderTest.SubCategories.Count(), Is.EqualTo(1));
            Assert.That(m_UnderTest.SubCategories.First().Name, Is.EqualTo("SubCategory"));
        }

        [Test]
        [TestCase("TestComponent.jpg", true)]
        [TestCase("TestComponent.lib", true)]
        [TestCase("TestComponent.tmp", false)]
        [TestCase("TestComponent.txt", true)]
        [TestCase("TestComponent.wmv", true)]
        [TestCase("TestComponent.jpg", true)]
        public void IsSupportedFileFormatTest(string filename, bool expected)
        {
            Assert.That(m_UnderTest.IsSupportedFileFormat(filename), Is.EqualTo(expected));
        }
    }
}
#endif
