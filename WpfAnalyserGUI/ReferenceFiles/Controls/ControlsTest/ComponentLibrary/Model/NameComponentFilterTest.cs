#if !VNEXT_TARGET
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class NameComponentFilterTest
    {
        private NameComponentFilter m_NameComponentFilter;
        private IComponentInfo m_ComponentInfo;
        
        [SetUp]
        public void SetUp()
        {
            m_NameComponentFilter = new NameComponentFilter();
            m_ComponentInfo = Substitute.For<IComponentInfo>();
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
        public void IsMatch(string componentName, string filterText, bool expectedIsMatch)
        {
            // ARRANGE
            m_ComponentInfo.DisplayName.Returns(componentName);
            
            m_NameComponentFilter.Parameter = filterText;

            // ACT
            bool isMatch = m_NameComponentFilter.IsMatch(m_ComponentInfo);

            // ASSERT
            Assert.That(isMatch, Is.EqualTo(expectedIsMatch));
        }
    }
}
#endif
