#if !VNEXT_TARGET
using NSubstitute;
using NUnit.Framework;

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
            Assert.That(m_UnderTest.IsMatch(Substitute.For<IComponentInfo>()), Is.True);
        }
    }
}
#endif
