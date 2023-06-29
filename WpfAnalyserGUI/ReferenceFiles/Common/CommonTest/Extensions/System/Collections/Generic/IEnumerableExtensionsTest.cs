#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Utilities.Assertion;
using Neo.ApplicationFramework.Common.Utilities;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Neo.ApplicationFramework.Common.Extensions.System.Collections.Generic
{
    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [Test]
        public void ReturnsEmptyCollectionWhenCastFails()
        {
            string[] actual = new[] { "A" };

            IEnumerable<bool> result = actual.TryCast<bool>();

            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ReturnsEmptyCollectionWhenIEnumerableIsNull()
        {
            string[] actual = null;

            IEnumerable<bool> result = actual.TryCast<bool>();

            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CanCastIEnumerableToCollection()
        {
            object[] actual = new object[] { false, true, false };

            IEnumerable<bool> result = actual.TryCast<bool>();

            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public void CombinedExceptReturnsFromBothCollectionsWhenNoEqualItems()
        {
            int[] firstValues = new int[] { 1, 2, 3 };
            int[] secondValues = new int[] { 4, 5, 6 };

            int[] result = firstValues.SymmetricDifference(secondValues).ToArray();

            Assert.That(result, Has.Member(1));
            Assert.That(result, Has.Member(2));
            Assert.That(result, Has.Member(3));
            Assert.That(result, Has.Member(4));
            Assert.That(result, Has.Member(5));
            Assert.That(result, Has.Member(6));
        }

        [Test]
        public void CombinedExceptDoesNotReturnDuplicatesWhenEqualValueInBothCollections()
        {
            int[] firstValues = new int[] { 1, 2, 3 };
            int[] secondValues = new int[] { 3, 4 };

            int[] result = firstValues.SymmetricDifference(secondValues).ToArray();

            Assert.That(result, Has.Member(1));
            Assert.That(result, Has.Member(2));
            Assert.That(result, Has.Member(4));
            Assert.That(result.Length, Is.EqualTo(3));
        }

        [Test]
        public void CombinedExceptDoesNotReturnAnyValuesWhenCollectionsAreEqual()
        {
            int[] firstValues = new int[] { 1, 2, 3 };
            int[] secondValues = new int[] { 1, 2, 3 };

            int[] result = firstValues.SymmetricDifference(secondValues).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CanCheckIfAnIEnumerableIsNullOrEmpty()
        {
            int[] nulledList = null;
            int[] emptyList = new int[] { };
            int[] notEmptyList = new int[] { 1, 2, 3 };


            Assert.That(nulledList.IsNullOrEmpty(), Is.True);
            Assert.That(emptyList.IsNullOrEmpty(), Is.True);
            Assert.That(notEmptyList.IsNullOrEmpty(), Is.False);
        }

        [Test]
        public void CanCheckIfAnIEnumerableNotIsNullOrEmpty()
        {
            int[] nulledList = null;
            int[] emptyList = new int[] { };
            int[] notEmptyList = new int[] { 1, 2, 3 };

            Assert.That(nulledList.IsNotNullOrEmpty(), Is.False);
            Assert.That(emptyList.IsNotNullOrEmpty(), Is.False);
            Assert.That(notEmptyList.IsNotNullOrEmpty(), Is.True);
        }

        [Test]
        public void CanFindDuplicatesWhenThereAnItemWithMoreTwoOccurencesThatAreEqual()
        {
            IEnumerable<int> listWithDuplicates = new[] { 1, 1, 2, 2, 3, 4 };

            var foundDuplicates = listWithDuplicates.FindDistinctDuplicates();

            Assert.That(foundDuplicates, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void CanFindDuplicatesWhenThereAnItemWithMoreThenTwoOccurencesThatAreEqual()
        {
            IEnumerable<int> listWithDuplicates = new[] { 1, 1, 1 };

            var foundDuplicates = listWithDuplicates.FindDistinctDuplicates();

            Assert.That(foundDuplicates, Is.EquivalentTo(new[] { 1 }));

        }

        [Test]
        public void CantFindDuplicatesWhenTheresNoDuplicates()
        {
            IEnumerable<int> listWithDuplicates = new[] { 1, 2, 3 };

            var foundDuplicates = listWithDuplicates.FindDistinctDuplicates();

            Assert.That(foundDuplicates, Is.Empty);
        }

        [Test]
        public void ToStringWithSeparatorWorksWithMultipleItems()
        {
            string expectedString = "1, 2, 3";

            string actualString = new[] { 1, 2, 3 }.ToString(",");

            Assert.That(actualString, Is.EqualTo(expectedString));
        }

        [Test]
        public void ToStringWithSeparatorWorksWithSingleItem()
        {
            string expectedString = "1";

            string actualString = new[] { 1 }.ToString(",");

            Assert.That(actualString, Is.EqualTo(expectedString));
        }


        [Test]
        public void ToStringWithSeparatorAndLastSeparatorWorksWithMultipleItems()
        {
            string expectedString = "1, 2 and 3";

            string actualString = new[] { 1, 2, 3 }.ToString(",", "and");

            Assert.That(actualString, Is.EqualTo(expectedString));
        }

        [Test]
        public void ToStringWithSeparatorAndLastSeparatorWorksWithSingleItem()
        {
            string expectedString = "1";

            string actualString = new[] { 1 }.ToString(",", "and");

            Assert.That(actualString, Is.EqualTo(expectedString));
        }

        [Test]
        public void ToSet()
        {
            // ARRANGE
            var texts = new[]
            {
                "a",
                "bb",
                "dddd"
            };

            // ACT
            ISet<string> textsSet = texts.ToSet();

            // ASSERT
            CollectionAssert.AreEqual(new[] { "a", "bb", "dddd" }, textsSet);
        }

        [Test]
        public void ToSetWithValueSelector()
        {
            // ARRANGE
            var texts = new[]
            {
                "a",
                "bb",
                "dddd"
            };

            // ACT
            ISet<int> textLengths = texts.ToSet(text => text.Length);

            // ASSERT
            CollectionAssert.AreEqual(new[] { 1, 2, 4 }, textLengths);
        }

        [Test]
        public void OrderByDependencyDoesNothingIfNoDependencies()
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            list.Add(new Tuple<string, string>("kalle", "hilda"));
            list.Add(new Tuple<string, string>("oskar", "hulda"));
            list.Add(new Tuple<string, string>("hugo", "josefine"));

            IList<Tuple<string, string>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, StringComparer.CurrentCulture).ToList();

            Assert.That(orderedList, Is.EqualTo(list));
        }

        [Test]
        public void OrderByDependencyHandlesStringKeys()
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            list.Add(new Tuple<string, string>("hugo", "josefine"));
            list.Add(new Tuple<string, string>("oskar", "kalle"));
            list.Add(new Tuple<string, string>("fredrik", "oskar"));
            list.Add(new Tuple<string, string>("kalle", "hilda"));

            IList<Tuple<string, string>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, StringComparer.CurrentCulture).ToList();

            Assert.That(orderedList[0], Is.EqualTo(list[0]));
            Assert.That(orderedList[1], Is.EqualTo(list[3]));
            Assert.That(orderedList[2], Is.EqualTo(list[1]));
            Assert.That(orderedList[3], Is.EqualTo(list[2]));
        }

        [Test]
        public void OrderByDependencyHandlesStringKeysWithCorrectComparerWhenIgnoringCase()
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            list.Add(new Tuple<string, string>("Hugo", "josefinE"));
            list.Add(new Tuple<string, string>("Oskar", "kallE"));
            list.Add(new Tuple<string, string>("Fredrik", "oskaR"));
            list.Add(new Tuple<string, string>("Kalle", "hildA"));

            IList<Tuple<string, string>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, StringComparer.CurrentCultureIgnoreCase).ToList();

            Assert.That(orderedList[0], Is.EqualTo(list[0]));
            Assert.That(orderedList[1], Is.EqualTo(list[3]));
            Assert.That(orderedList[2], Is.EqualTo(list[1]));
            Assert.That(orderedList[3], Is.EqualTo(list[2]));
        }

        [Test]
        public void OrderByDependencyHandlesNumericKeys()
        {
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();
            list.Add(new Tuple<int, int>(2, 20));
            list.Add(new Tuple<int, int>(3, 5));
            list.Add(new Tuple<int, int>(4, 3));
            list.Add(new Tuple<int, int>(5, 50));

            IList<Tuple<int, int>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, EqualityComparer<int>.Default).ToList();

            Assert.That(orderedList[0], Is.EqualTo(list[0]));
            Assert.That(orderedList[1], Is.EqualTo(list[3]));
            Assert.That(orderedList[2], Is.EqualTo(list[1]));
            Assert.That(orderedList[3], Is.EqualTo(list[2]));
        }

        [Test]
        public void OrderByDescendingHandlesDependenciesThatIsMissingFromList()
        {
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();
            list.Add(new Tuple<int, int>(1, 10));
            list.Add(new Tuple<int, int>(2, 20));

            IList<Tuple<int, int>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, EqualityComparer<int>.Default).ToList();

            Assert.That(orderedList[0], Is.EqualTo(list[0]));
            Assert.That(orderedList[1], Is.EqualTo(list[1]));
        }

        [Test]
        public void OrderByDescendingHandlesCircularDependencies()
        {
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();
            list.Add(new Tuple<int, int>(1, 10));
            list.Add(new Tuple<int, int>(2, 3));
            list.Add(new Tuple<int, int>(3, 2));

            IList<Tuple<int, int>> orderedList = list.OrderByDependency(x => x.Item1, x => x.Item2, EqualityComparer<int>.Default).ToList();

            Assert.That(orderedList[0], Is.EqualTo(list[0]));
            Assert.That(orderedList[1], Is.EqualTo(list[2]));
            Assert.That(orderedList[2], Is.EqualTo(list[1]));
        }


        [TestCase(new[] { 1, 1, 2, 2, 3, 4 }, true)]
        [TestCase(new[] { 1, 1, 1 }, true)]
        [TestCase(new[] { 1 }, false)]
        [TestCase(new[] { 1, 2, 3 }, false)]
        public void ContainsDuplicatesTest(IEnumerable<int> testdata, bool containsDuplicates)
        {
            Assert.True(testdata.ContainsDuplicates(i => i.GetHashCode()) == containsDuplicates);
            Assert.True(testdata.ContainsDuplicates() == containsDuplicates);
        }


        [TestCase(new[] { "1", "1", "2", "2", "3", "4" }, true)]
        [TestCase(new[] { "a", "A", "b" }, true)]
        [TestCase(new[] { "a" }, false)]
        [TestCase(new[] { "A", "B", "c" }, false)]
        public void ContainsDuplicatesTestIgnoreCasingStringEqualityComparer(IEnumerable<string> testdata, bool containsDuplicates)
        {
            Assert.True(testdata.ContainsDuplicates(item => item, new IgnoreCasingStringEqualityComparer()) == containsDuplicates);
        }

        [TestCase(new[] { "1", "1", "2", "2", "3", "4" }, true)]
        [TestCase(new[] { "a", "A", "b" }, false)]
        [TestCase(new[] { "a" }, false)]
        [TestCase(new[] { "A", "B", "c" }, false)]
        public void ContainsDuplicatesTestDefaultEqualityComparer(IEnumerable<string> testdata, bool containsDuplicates)
        {
            Assert.True(testdata.ContainsDuplicates() == containsDuplicates);
        }

        [TestCase(new[] { 1 }, new[] { 1 })]
        [TestCase(new[] { 1 }, new[] { 1, 2 })]
        [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2, 3 })]
        [TestCase(new[] { 1, 2 }, new int[] { })]
        [TestCase(new int[] { }, new[] { 1, 2 })]
        [TestCase(new int[] { }, new int[] { })]
        public void IntersectExceptUnionTest(IEnumerable<int> testData1Arg, IEnumerable<int> testData2Arg)
        {
            var testData1 = testData1Arg.ToArray();
            var testData2 = testData2Arg.ToArray();
            IntersectTest(testData1, testData2);
            ExceptTest(testData1, testData2);
            UnionTest(testData1, testData2);
        }

        [TestCase(new int[] { })]
        [TestCase(new[] { 1 })]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase(new[] { 1, 1 })]
        [TestCase(new[] { 1, 2, 3, 1 })]
        public void DistinctTest(IEnumerable<int> testData)
        {
            var distinct = testData.Distinct((i, ii) => i == ii, i => i.GetHashCode());
            var expectedDistinct = testData.Distinct();

            var successDistinct = ContainsSameElements(distinct, expectedDistinct);

            Assert.True(successDistinct);
        }

        [TestCase(new[] {1,2,3,4}, 0, true)]
        [TestCase(new []{1,2,3,4}, 1, true)]
        [TestCase(new []{1,2,3,4}, 2, true)]
        [TestCase(new []{1,2,3,4}, 3, true)]
        [TestCase(new []{1,2,3,4}, 4, true)]
        [TestCase(new []{1,2,3,4}, 5, false)]
        [TestCase(new  int[]{}, 0, true)]
        [TestCase(new  int[]{}, 1, false)]
        public void AtLeastTest(IEnumerable<int> testData, int atLeast, bool expectedResult)
        {
            Assert.AreEqual(testData.AtLeast(atLeast), expectedResult);
        }

        [TestCase(new[] { 1, 2, 3, 4 }, -1, false)]
        [TestCase(null, 0, true)]
        [TestCase(null, 1, false)]
        public void AtLeastTestException(IEnumerable<int> testData, int atLeast, bool expectedResult)
        {
            Assert.Throws<AssertException>(() => testData.AtLeast(atLeast));
        }

        [TestCase(new[] { 1, 2, 3, 4 }, new int[]{})]
        [TestCase(new[] { 1, 2, 1, 4 }, new []{1, 1})]
        [TestCase(new[] { 1, 2, 1, 2, 2 }, new[]{1, 1}, new []{2, 2, 2})]
        public void FindGroupsWithDuplicatesTests(IEnumerable<int> testData, params int[][] expectedResults)
        {
            var result = testData.FindGroupsWithDuplicates(item => item);
            int[][] arrayResult = result.Select(item => item.ToArray()).ToArray();
            for (int i = 0; i < arrayResult.Length; i++)
            {
                Assert.IsTrue(arrayResult[i].SequenceEqual(expectedResults[i]));
            }
        }

        [TestCase(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [TestCase(new[] { 3, 9, 10, 12, 13, 14 })]
        [TestCase(new[] { 8 })]
        [TestCase(new[] { 3 })]
        [TestCase(new int[0])]
        public void MultiPredicateWhereTest(int[] testData)
        {
            var allowEvenNumbers = new Func<int, bool>(x => x % 2 == 0);
            var allowOver5 = new Func<int, bool>(x => x > 5);
            var allow3Or8Or12 = new Func<int, bool>(x => x == 3 || x == 8 || x == 12);

            var evenNumbers = testData.Where(allowEvenNumbers);
            var over5 = testData.Where(allowOver5);
            var is3_Or8_Or12 = testData.Where(allow3Or8Or12);
            var expectedResult = evenNumbers.Intersect(over5).Intersect(is3_Or8_Or12);
            var result = testData.WhereAll(new[] { allowEvenNumbers, allowOver5, allow3Or8Or12 });
            Assert.IsTrue(result.ContainsSameElements(expectedResult));
        }

        #region Helper methods

        private static void UnionTest(int[] testData1, int[] testData2)
        {
            IEnumerable<int> union = testData1.Union(testData2, (item1, item2) => item1 == item2, item => item.GetHashCode());
            IEnumerable<int> expectedUnion = testData1.Union(testData2);
            bool successUnion = ContainsSameElements(union, expectedUnion);
            Assert.True(successUnion);
        }

        private static void ExceptTest(int[] testData1, int[] testData2)
        {
            IEnumerable<int> except = testData1.Except(testData2, (item1, item2) => item1 == item2, item => item.GetHashCode());
            IEnumerable<int> expectedExcept = testData1.Except(testData2);
            bool successExcept = ContainsSameElements(except, expectedExcept);
            Assert.True(successExcept);
        }

        private static void IntersectTest(int[] testData1, int[] testData2)
        {
            IEnumerable<int> intersect = testData1.Intersect(testData2, (item1, item2) => item1 == item2, item => item.GetHashCode());
            IEnumerable<int> expectedIntersection = testData1.Intersect(testData2);
            bool successIntersection = ContainsSameElements(intersect, expectedIntersection);
            Assert.True(successIntersection);
        }

        private static bool ContainsSameElements(IEnumerable<int> result, IEnumerable<int> excpectedResult)
        {
            return !result.Except(excpectedResult).Any();
        }

        #endregion
    }
}
#endif
