using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    /// <summary>
    /// Unit tests of <see cref="ComponentAllFilter"/>
    /// </summary>
    [TestFixture]
    public class ComponentAllFilterTest
    {
        private ComponentAllFilter m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_UnderTest = new ComponentAllFilter();
        }

        /// <summary>
        /// All components should match the filter
        /// </summary>
        [Test]
        public void All_components_should_match()
        {
            Assert.That(m_UnderTest.IsMatch(null), Is.True);
            Assert.That(m_UnderTest.IsMatch(MockRepository.GenerateMock<IComponentInfo>()), Is.True);
        }
    }
}