using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl.Builders
{
    [TestFixture]
    public class HierarchyParserTest
    {
        [Test]
        public void GetRoot()
        {
            // ASSERT
            Assert.That(HierarchyParser.GetFirstGeneration("FirstGeneration"), Is.EqualTo("FirstGeneration"));
            Assert.That(HierarchyParser.GetFirstGeneration("FirstGeneration.Child"), Is.EqualTo("FirstGeneration"));
            Assert.That(HierarchyParser.GetFirstGeneration("FirstGeneration.Child.GrandChild"), Is.EqualTo("FirstGeneration"));
        }

        [Test]
        public void GetParentFullName()
        {
            // ASSERT
            Assert.That(HierarchyParser.GetParentFullName("FirstGeneration"), Is.EqualTo(string.Empty));
            Assert.That(HierarchyParser.GetParentFullName("FirstGeneration.Child"), Is.EqualTo("FirstGeneration"));
            Assert.That(HierarchyParser.GetParentFullName("FirstGeneration.Child.GrandChild"), Is.EqualTo("FirstGeneration.Child"));
        }

        [Test]
        public void GetName()
        {
            // ASSERT
            Assert.That(HierarchyParser.GetName("FirstGeneration"), Is.EqualTo("FirstGeneration"));
            Assert.That(HierarchyParser.GetName("FirstGeneration.Child"), Is.EqualTo("Child"));
            Assert.That(HierarchyParser.GetName("FirstGeneration.Child.GrandChild"), Is.EqualTo("GrandChild"));
        }

        [Test]
        public void IsDescendant()
        {
            // ASSERT
            Assert.That(HierarchyParser.IsDescendant("FirstGeneration"), Is.False);
            Assert.That(HierarchyParser.IsDescendant("FirstGeneration.Child"), Is.True);
            Assert.That(HierarchyParser.IsDescendant("FirstGeneration.Child.GrandChild"), Is.True);
        }

        [Test]
        public void GetGenerations()
        {
            // ARRANGE
            IEnumerable<string> bloodline1 = HierarchyParser.GetGenerations("FirstGeneration");
            IEnumerable<string> bloodline2 = HierarchyParser.GetGenerations("FirstGeneration.Child");
            IEnumerable<string> bloodline3 = HierarchyParser.GetGenerations("FirstGeneration.Child.GrandChild");

            // ASSERT
            Assert.That(bloodline1.Count(), Is.EqualTo(1));
            Assert.That(bloodline1.ElementAt(0), Is.EqualTo("FirstGeneration"));

            Assert.That(bloodline2.Count(), Is.EqualTo(2));
            Assert.That(bloodline2.ElementAt(0), Is.EqualTo("FirstGeneration"));
            Assert.That(bloodline2.ElementAt(1), Is.EqualTo("FirstGeneration.Child"));

            Assert.That(bloodline3.Count(), Is.EqualTo(3));
            Assert.That(bloodline3.ElementAt(0), Is.EqualTo("FirstGeneration"));
            Assert.That(bloodline3.ElementAt(1), Is.EqualTo("FirstGeneration.Child"));
            Assert.That(bloodline3.ElementAt(2), Is.EqualTo("FirstGeneration.Child.GrandChild"));
        }
    }
}