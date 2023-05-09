#if!VNEXT_TARGET
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TextIdBrowser
{
    [TestFixture]
    public class ColumnOrderComparerTest
    {
        private PropertyDescriptor m_FirstNameDescriptor;
        private PropertyDescriptor m_SurnameDescriptor;
        private PropertyDescriptor m_AgeDescriptor;


        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(typeof(TestClass));
            m_FirstNameDescriptor = propertyDescriptors[0];
            m_SurnameDescriptor = propertyDescriptors[1];
            m_AgeDescriptor = propertyDescriptors[2];
        }


        [Test]
        public void AscendingTest()
        {
            // ARRANGE
            var comparer = new ColumnOrderComparer(new List<string>
            {
                "Age",
                "FirstName",
                "Surname"
            });
            
            // ACT
            int shouldBeEquals1 = comparer.Compare(m_AgeDescriptor, m_AgeDescriptor);
            int shouldBeNegative1 = comparer.Compare(m_AgeDescriptor, m_FirstNameDescriptor);
            int shouldBeNegative2 = comparer.Compare(m_AgeDescriptor, m_SurnameDescriptor);

            int shouldBePositive1 = comparer.Compare(m_FirstNameDescriptor, m_AgeDescriptor);
            int shouldBeEquals2 = comparer.Compare(m_FirstNameDescriptor, m_FirstNameDescriptor);
            int shouldBeNegative3 = comparer.Compare(m_FirstNameDescriptor, m_SurnameDescriptor);

            int shouldBePositive2 = comparer.Compare(m_SurnameDescriptor, m_AgeDescriptor);
            int shouldBePositive3 = comparer.Compare(m_SurnameDescriptor, m_FirstNameDescriptor);
            int shouldBeEquals3 = comparer.Compare(m_SurnameDescriptor, m_SurnameDescriptor);

            // ASSERT
            Assert.That(shouldBeEquals1, Is.EqualTo(0));
            Assert.That(shouldBeNegative1, Is.LessThan(0));
            Assert.That(shouldBeNegative2, Is.LessThan(0));

            Assert.That(shouldBePositive1, Is.GreaterThan(0));
            Assert.That(shouldBeEquals2, Is.EqualTo(0));
            Assert.That(shouldBeNegative3, Is.LessThan(0));

            Assert.That(shouldBePositive2, Is.GreaterThan(0));
            Assert.That(shouldBePositive3, Is.GreaterThan(0));
            Assert.That(shouldBeEquals3, Is.EqualTo(0));
        }


        [Test]
        public void DescendingTest()
        {
            // ARRANGE
            var comparer = new ColumnOrderComparer(new List<string>
            {
                "Surname",
                "FirstName",
                "Age"
            });

            // ACT
            int shouldBeEquals1 = comparer.Compare(m_AgeDescriptor, m_AgeDescriptor);
            int shouldBePositive1 = comparer.Compare(m_AgeDescriptor, m_FirstNameDescriptor);
            int shouldBePositive2 = comparer.Compare(m_AgeDescriptor, m_SurnameDescriptor);

            int shouldBeNegative1 = comparer.Compare(m_FirstNameDescriptor, m_AgeDescriptor);
            int shouldBeEquals2 = comparer.Compare(m_FirstNameDescriptor, m_FirstNameDescriptor);
            int shouldBePositive3 = comparer.Compare(m_FirstNameDescriptor, m_SurnameDescriptor);

            int shouldBeNegative2 = comparer.Compare(m_SurnameDescriptor, m_AgeDescriptor);
            int shouldBeNegative3 = comparer.Compare(m_SurnameDescriptor, m_FirstNameDescriptor);
            int shouldBeEquals3 = comparer.Compare(m_SurnameDescriptor, m_SurnameDescriptor);

            // ASSERT
            Assert.That(shouldBeEquals1, Is.EqualTo(0));
            Assert.That(shouldBePositive1, Is.GreaterThan(0));
            Assert.That(shouldBePositive2, Is.GreaterThan(0));

            Assert.That(shouldBeNegative1, Is.LessThan(0));
            Assert.That(shouldBeEquals2, Is.EqualTo(0));
            Assert.That(shouldBePositive3, Is.GreaterThan(0));

            Assert.That(shouldBeNegative2, Is.LessThan(0));
            Assert.That(shouldBeNegative3, Is.LessThan(0));
            Assert.That(shouldBeEquals3, Is.EqualTo(0));
        }
        

        class TestClass
        {
            public TestClass(string firstName, string surname, int age)
            {
                FirstName = firstName;
                Surname = surname;
                Age = age;
            }

            public string FirstName { get; private set; }
            public string Surname { get; private set; }
            public int Age { get; private set; }
        }
    }
}
#endif
