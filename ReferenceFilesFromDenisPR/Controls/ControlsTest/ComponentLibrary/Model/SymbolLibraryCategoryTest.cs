#if !VNEXT_TARGET
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class SymbolLibraryCategoryTest
    {
        private SymbolLibraryCategory m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_UnderTest = new SymbolLibraryCategory();
        }

        [Test]
        public void PropertiesTest()
        {
            Assert.That(m_UnderTest.Name, Is.EqualTo("Project Pictures"));
            Assert.That(m_UnderTest.IsReadOnly);
            Assert.That(m_UnderTest.SubCategories, Is.Empty);
        }
    }
}
#endif
