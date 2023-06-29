#if !VNEXT_TARGET
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class ScreenObjectFilterTest
    {
        private ScreenObjectFilter m_ScreenObjectFilter;

        [SetUp]
        public void SetUp()
        {
            m_ScreenObjectFilter = new ScreenObjectFilter();
        }

        [TestCase("Component Name", "", false)]
        [TestCase("Component Name", "Component", false)]
        [TestCase("Component Name", "component", false)]
        [TestCase("Component Name", "Name", false)]
        [TestCase("Component Name", "name", false)]
        [TestCase("Component Name", "ponen", false)]
        [TestCase("Component Name", "nt Na", false)]
        [TestCase("Component Name", "Invalid", false)]
        [TestCase("Component Name", "Component  Name", false)]
        public void IsMatch_TextComponent(string componentName, string filterText, bool expectedIsMatch)
        {
            // ARRANGE
            var textComponent = new TextComponent(componentName);

            m_ScreenObjectFilter.Parameter = filterText;

            // ACT
            bool isMatch = m_ScreenObjectFilter.IsMatch(textComponent);

            // ASSERT
            Assert.That(isMatch, Is.EqualTo(expectedIsMatch));
        }

        [TestCase("Component Name", "", true)]
        [TestCase("Component Name", "Component", true)]
        [TestCase("Component Name", "component", true)]
        [TestCase("Component Name", "Name", true)]
        [TestCase("Component Name", "name", true)]
        [TestCase("Component Name", "ponen", true)]
        [TestCase("Component Name", "nt Na", true)]
        [TestCase("Component Name", "Invalid", false)]
        [TestCase("Component Name", "Component  Name", false)]
        public void IsMatch_ImageComponent(string componentName, string filterText, bool expectedIsMatch)
        {
            // ARRANGE
            var textComponent = new ImageComponent(componentName);

            m_ScreenObjectFilter.Parameter = filterText;

            // ACT
            bool isMatch = m_ScreenObjectFilter.IsMatch(textComponent);

            // ASSERT
            Assert.That(isMatch, Is.EqualTo(expectedIsMatch));
        }

        [TestCase("Component Name", "", true)]
        [TestCase("Component Name", "Component", true)]
        [TestCase("Component Name", "component", true)]
        [TestCase("Component Name", "Name", true)]
        [TestCase("Component Name", "name", true)]
        [TestCase("Component Name", "ponen", true)]
        [TestCase("Component Name", "nt Na", true)]
        [TestCase("Component Name", "Invalid", false)]
        [TestCase("Component Name", "Component  Name", false)]
        public void IsMatch_ScreenObjectComponent(string componentName, string filterText, bool expectedIsMatch)
        {
            // ARRANGE
            var textComponent = new ScreenObjectComponent(componentName);

            m_ScreenObjectFilter.Parameter = filterText;

            // ACT
            bool isMatch = m_ScreenObjectFilter.IsMatch(textComponent);

            // ASSERT
            Assert.That(isMatch, Is.EqualTo(expectedIsMatch));
        }
    }
}
#endif
