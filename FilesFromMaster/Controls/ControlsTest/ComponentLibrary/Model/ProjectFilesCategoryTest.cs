using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class ProjectFilesCategoryTest
    {
        private ProjectFilesCategory m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_UnderTest = new ProjectFilesCategory();
        }

        [Test]
        public void PropertiesTest()
        {
            Assert.That(m_UnderTest.Name, Is.EqualTo("Project Files"));
            Assert.That(m_UnderTest.IsReadOnly, Is.False);
            Assert.That(m_UnderTest.SubCategories, Is.Empty);
        }
    }
}