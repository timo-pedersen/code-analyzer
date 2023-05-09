using System;
using System.ComponentModel;
using System.Globalization;
using Core.Api.GlobalReference;
using Core.Controls.Api.Bindings.PropertyBinders;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Dynamics;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Dynamics
{
    [TestFixture]
    public class DynamicStringTest
    {
        private const string PeopleNamesTextLibraryGroupName = "People";
        private const string GeneralTextLibraryGroupName = "General";
        private IDynamicString m_DynamicString;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IGlobalReferenceService>();
            m_DynamicString = new DynamicString();
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void IsInitializedIsAlwaysTrueWhenISupportInitializeIsNotUtilizedWhichIsTheCaseInRunTime()
        {
            DynamicString dynamicString = new DynamicString();

            Assert.IsTrue(dynamicString.IsInitialized);
        }

        [Test]
        public void IsInitializedIsFalseAfterBeginInitWhenISupportInitializeIsUtilizedWhichIsTheCaseInDesignTime()
        {
            DynamicString dynamicString = new DynamicString();

            ((ISupportInitialize)dynamicString).BeginInit();

            Assert.IsFalse(dynamicString.IsInitialized);
        }

        [Test]
        public void IsInitializedIsTrueAfterEndInitWhenISupportInitializeIsUtilizedWhichIsTheCaseInDesignTime()
        {
            DynamicString dynamicString = new DynamicString();
            ((ISupportInitialize)dynamicString).BeginInit();

            ((ISupportInitialize)dynamicString).EndInit();

            Assert.IsTrue(dynamicString.IsInitialized);
        }

        [Test]
        public void SetTextWhenEmpty()
        {
            m_DynamicString.Text = "My name is {0}.";

            Assert.AreEqual(1, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 11, null, dynamicItem);
        }

        [Test]
        public void SetTextWhenNotEmpty()
        {
            m_DynamicString.Text = "My name is {0}.";
            m_DynamicString.DynamicItems[0].Value = "Neo";

            m_DynamicString.Text = "My name is {0}, not {1}!";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 11, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 20, null, dynamicItem);
        }

        [Test]
        public void InsertTextBeforeDynamicItems()
        {
            m_DynamicString.Text = "My name is {0}, not {1}!";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";

            m_DynamicString.Text = "Hello! My name is {0}, not {1}!";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 18, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 27, "Morpheus", dynamicItem);
        }

        [Test]
        public void InsertTextBetweenDynamicItems()
        {
            m_DynamicString.Text = "My name is {0}, not {1}!";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";

            m_DynamicString.Text = "My name is {0}, and really not {1}!";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 11, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 31, "Morpheus", dynamicItem);
        }

        [Test]
        public void InsertTextAfterDynamicItems()
        {
            m_DynamicString.Text = "My name is {0}, not {1}!";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";

            m_DynamicString.Text = "My name is {0}, not {1}! Get it?!";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 11, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 20, "Morpheus", dynamicItem);
        }

        [Test]
        public void InsertDynamicItemBetweenDynamicItems()
        {
            m_DynamicString.Text = "My name really is {0}, and not {1}!";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";

            m_DynamicString.Text = "My name really is {0}, and I live in the {2}. I do know {1} though...";
            m_DynamicString.DynamicItems[2].Value = "Matrix";

            Assert.AreEqual(3, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 18, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 56, "Morpheus", dynamicItem);

            dynamicItem = FindDynamicItem("{2}");
            AssertDynamicStringItemIsEqualTo(2, 41, "Matrix", dynamicItem);
        }

        [Test]
        public void RemoveDuplicatedInsertedDynamicItem()
        {
            m_DynamicString.Text = "My name is {0}, not {1}!";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";

            m_DynamicString.Text = "Hello {0}! My name is {0}, not {1}!";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 6, "Neo", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 28, "Morpheus", dynamicItem);
        }

        [Test]
        public void RemoveFirstDynamicItem()
        {
            m_DynamicString.Text = "My name really is {0}, and I live in the {2}. I do know {1} though...";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";
            m_DynamicString.DynamicItems[2].Value = "Matrix";

            m_DynamicString.Text = "My name really is Trinity, and I live in the {2}. I do know {1} though...";

            Assert.AreEqual(2, m_DynamicString.DynamicItems.Count);

            IDynamicStringItem dynamicItem = FindDynamicItem("{0}");
            AssertDynamicStringItemIsEqualTo(0, 60, "Morpheus", dynamicItem);

            dynamicItem = FindDynamicItem("{1}");
            AssertDynamicStringItemIsEqualTo(1, 45, "Matrix", dynamicItem);

            Assert.AreEqual(0, string.Compare(m_DynamicString.Text, "My name really is Trinity, and I live in the {1}. I do know {0} though...", StringComparison.CurrentCulture));
        }

        [Test]
        public void RemoveAnyDynamicItemInTextWithOverTenItems()
        {
            m_DynamicString.Text = "Hi {3}! I'm {0} and I need {2}, {1}, {4}, {6}, {7}, {5}, {8}, {9}, {10} and {11}!";
            Assert.AreEqual(12, m_DynamicString.DynamicItems.Count);

            m_DynamicString.Text = "Hi {3}! I'm {0} and I need 2}, {1}, {4}, {6}, {7}, {5}, {8}, {9}, {10} and {11}!";
            Assert.AreEqual(11, m_DynamicString.DynamicItems.Count);

            Assert.AreEqual(0, string.Compare(m_DynamicString.Text, "Hi {2}! I'm {0} and I need 2}, {1}, {3}, {5}, {6}, {4}, {7}, {8}, {9} and {10}!", StringComparison.CurrentCulture));
        }

        [Test]
        public void AddDynamicItemWithSameNumberRemovesFirstDuplicate()
        {
            m_DynamicString.Text = "Hi {2}! I'm {0} and I need 2}, {1}, {3}, {5}, {6}, {4}, {7}, {8}, {9} and {10}!";
            Assert.AreEqual(11, m_DynamicString.DynamicItems.Count);

            m_DynamicString.Text = "Hi {2}! I'm {0} and I need {2}, {1}, {3}, {5}, {6}, {4}, {7}, {8}, {9} and {10}!";
            Assert.AreEqual(11, m_DynamicString.DynamicItems.Count);

            Assert.AreEqual(0, string.Compare(m_DynamicString.Text, "Hi {2}! I'm {0} and I need , {1}, {3}, {5}, {6}, {4}, {7}, {8}, {9} and {10}!", StringComparison.CurrentCulture));
        }

        [Test]
        public void DynamicStringItemsAddedInNumericalOrder()
        {
            m_DynamicString.Text = "Hi {3}! I'm {0} and I need {2}, {1}, {4}, {6}, {7}, {5}, {8}, {9}, {10} and {11}!";

            Assert.AreEqual(12, m_DynamicString.DynamicItems.Count);

            for (int item = 0; item < m_DynamicString.DynamicItems.Count; item++)
            {
                Assert.AreEqual(item, m_DynamicString.DynamicItems[item].DynamicIndex);
            }
        }

        [Test]
        public void Clone()
        {
            m_DynamicString.Text = "My name really is {0}, and I live in the {2}. I do know {1} though...";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";
            m_DynamicString.DynamicItems[2].Value = "Matrix";

            IDynamicString dynamicStringCopy = null;
            if (m_DynamicString is ICloneable cloneable)
            {
                dynamicStringCopy = (IDynamicString)cloneable.Clone();
            }

            // Change original in order to make sure that the copy isn't changed, deep cloning.
            m_DynamicString.DynamicItems[0].Value = "Neo2";
            m_DynamicString.DynamicItems[1].Value = "Morpheus2";
            m_DynamicString.DynamicItems[2].Value = "Matrix2";

            Assert.AreEqual(m_DynamicString.Text, dynamicStringCopy.Text);
            Assert.AreEqual(m_DynamicString.DynamicItems.Count, dynamicStringCopy.DynamicItems.Count);

            Assert.AreEqual(m_DynamicString.DynamicItems[0].DynamicIndex, dynamicStringCopy.DynamicItems[0].DynamicIndex);
            Assert.AreEqual(m_DynamicString.DynamicItems[0].TextIndex, dynamicStringCopy.DynamicItems[0].TextIndex);
            Assert.AreEqual("Neo", dynamicStringCopy.DynamicItems[0].Value);

            Assert.AreEqual(m_DynamicString.DynamicItems[1].DynamicIndex, dynamicStringCopy.DynamicItems[1].DynamicIndex);
            Assert.AreEqual(m_DynamicString.DynamicItems[1].TextIndex, dynamicStringCopy.DynamicItems[1].TextIndex);
            Assert.AreEqual("Morpheus", dynamicStringCopy.DynamicItems[1].Value);

            Assert.AreEqual(m_DynamicString.DynamicItems[2].DynamicIndex, dynamicStringCopy.DynamicItems[2].DynamicIndex);
            Assert.AreEqual(m_DynamicString.DynamicItems[2].TextIndex, dynamicStringCopy.DynamicItems[2].TextIndex);
            Assert.AreEqual("Matrix", dynamicStringCopy.DynamicItems[2].Value);
        }

        [Test]
        public void FormatToString()
        {
            m_DynamicString.Text = "My name really is {0}, and I live in the {2}. I do know {1} though...";
            m_DynamicString.DynamicItems[0].Value = "Neo";
            m_DynamicString.DynamicItems[1].Value = "Morpheus";
            m_DynamicString.DynamicItems[2].Value = "Matrix";

            Assert.AreEqual(0, string.Compare(m_DynamicString.ToString(), "My name really is Neo, and I live in the Matrix. I do know Morpheus though...", StringComparison.CurrentCulture));
        }

        [Test]
        public void FormatToStringUsingTextLibraries()
        {
            m_DynamicString = new DynamicString(CreateMessageLibraryDynamicsConverterCFProvider())
            {
                Text = "My name really is {0}, and I live in the {2}. I do know {1} though..."
            };

            m_DynamicString.DynamicItems[0].Value = 0;
            m_DynamicString.DynamicItems[1].Value = 1;
            m_DynamicString.DynamicItems[2].Value = 2;

            m_DynamicString.DynamicItems[0].TextLibraryGroupName = PeopleNamesTextLibraryGroupName;
            m_DynamicString.DynamicItems[1].TextLibraryGroupName = PeopleNamesTextLibraryGroupName;
            m_DynamicString.DynamicItems[2].TextLibraryGroupName = GeneralTextLibraryGroupName;

            Assert.AreEqual(0, string.Compare(m_DynamicString.ToString(), "My name really is Neo, and I live in the Matrix. I do know Morpheus though...", StringComparison.CurrentCulture));
        }

        [Test]
        public void FormatToStringUsingTextLibrariesInDesignMode()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_DynamicString = new DynamicString(CreateMessageLibraryDynamicsConverterCFProvider())
            {
                Text = "My name really is {0}, and I live in the {2}. I do know {1} though..."
            };

            m_DynamicString.DynamicItems[0].Value = 0;
            m_DynamicString.DynamicItems[1].Value = 1;
            m_DynamicString.DynamicItems[2].Value = 2;

            m_DynamicString.DynamicItems[0].TextLibraryGroupName = PeopleNamesTextLibraryGroupName;
            m_DynamicString.DynamicItems[1].TextLibraryGroupName = PeopleNamesTextLibraryGroupName;
            m_DynamicString.DynamicItems[2].TextLibraryGroupName = GeneralTextLibraryGroupName;

            Assert.AreEqual(0, string.Compare(m_DynamicString.ToString(), "My name really is 0, and I live in the 2. I do know 1 though...", StringComparison.CurrentCulture));
        }

        [TestCase(true, "Neo", "Morpheus")]
        [TestCase(false, "Neo", "Morpheus", "Matrix", "Cypher")]
        [TestCase(true, null)]
        public void GetDynamicTextWithWrongNumberOfValues(bool expectEmptyString, params object[] values)
        {
            string formatText = "My name really is {0}, and I live in the {2}. I do know {1} though...";
            m_DynamicString.Text = formatText; 

            Assert.AreEqual(expectEmptyString ? string.Empty : string.Format(formatText, values),
                m_DynamicString.GetDynamicText(formatText, values));
        }

        #region Test Helper Methods

        private void AssertDynamicStringItemIsEqualTo(int expectedDynamicIndex, int expectedTextIndex, object expectedValue, IDynamicStringItem actualDynamicItem)
        {
            Assert.IsNotNull(actualDynamicItem);
            Assert.AreEqual(expectedDynamicIndex, actualDynamicItem.DynamicIndex);
            Assert.AreEqual(expectedTextIndex, actualDynamicItem.TextIndex);
            Assert.AreEqual(expectedValue, actualDynamicItem.Value);
        }

        private IDynamicStringItem FindDynamicItem(string dynamicText)
        {
            foreach (IDynamicStringItem dynamicItem in m_DynamicString.DynamicItems)
            {
                if (string.Compare(dynamicItem.Text, dynamicText, StringComparison.CurrentCulture) == 0)
                {
                    return dynamicItem;
                }
            }

            return null;
        }

        private IMessageLibraryDynamicsConverterCFProvider CreateMessageLibraryDynamicsConverterCFProvider()
        {
            var provider = Substitute.For<IMessageLibraryDynamicsConverterCFProvider>();

            var valueConverter0 = Substitute.For<IValueConverterCF>();
            valueConverter0.Convert((double)0, typeof(string), null, CultureInfo.InvariantCulture).Returns("Neo");
            valueConverter0.Convert((double)1, typeof(string), null, CultureInfo.InvariantCulture).Returns("Morpheus");
            provider.CreateMessageLibraryDynamicsConverterCF(PeopleNamesTextLibraryGroupName).Returns(valueConverter0);

            var valueConverter1 = Substitute.For<IValueConverterCF>();
            valueConverter1.Convert((double)2, typeof(string), null, CultureInfo.InvariantCulture).Returns("Matrix");
            provider.CreateMessageLibraryDynamicsConverterCF(GeneralTextLibraryGroupName).Returns(valueConverter1);

            return provider;
        }

        #endregion
    }
}
