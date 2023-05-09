﻿#if !VNEXT_TARGET
using System.ComponentModel;
using Core.Controls.Api.Bindings.DataSources;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class BindingSourceDescriptionViewModelBaseComparerTest
    {
        private BindingSourceDescriptionViewModelBase m_FirstBindingSourceDescription;
        private BindingSourceDescriptionViewModelBase m_SecondBindingSourceDescription;

        [SetUp]
        public void SetUp()
        {
            m_FirstBindingSourceDescription = Substitute.For<BindingSourceDescriptionViewModelBase>(
                new BindingSourceDescription());
            m_SecondBindingSourceDescription = Substitute.For<BindingSourceDescriptionViewModelBase>(
                new BindingSourceDescription());
        }

        [Test]
        public void FirstIsNull()
        {
            // ARRANGE
            var comparer = new BindingSourceDescriptionViewModelBaseComparer();
            comparer.PropertyName = "Name";

            // ACT
            var result = comparer.Compare(null, m_SecondBindingSourceDescription);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void SecondIsNull()
        {
            // ARRANGE
            var comparer = new BindingSourceDescriptionViewModelBaseComparer();
            comparer.PropertyName = "Name";

            // ACT
            var result = comparer.Compare(m_FirstBindingSourceDescription, null);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void BothNull()
        {
            // ARRANGE
            var comparer = new BindingSourceDescriptionViewModelBaseComparer();
            comparer.PropertyName = "Name";

            // ACT
            var result = comparer.Compare(null, null);

            // ASSERT
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void Ascending()
        {
            // ARRANGE
            m_FirstBindingSourceDescription.Name.Returns("Tag10");

            m_SecondBindingSourceDescription.Name.Returns("Tag8");

            var comparer = new BindingSourceDescriptionViewModelBaseComparer();
            comparer.PropertyName = "Name";
            comparer.SortDirection = ListSortDirection.Ascending;

            // ACT
            var result = comparer.Compare(m_FirstBindingSourceDescription, m_SecondBindingSourceDescription);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Descending()
        {
            // ARRANGE
            m_FirstBindingSourceDescription.Name.Returns("Tag10");

            m_SecondBindingSourceDescription.Name.Returns("Tag8");

            var comparer = new BindingSourceDescriptionViewModelBaseComparer();
            comparer.PropertyName = "Name";
            comparer.SortDirection = ListSortDirection.Descending;

            // ACT
            var result = comparer.Compare(m_FirstBindingSourceDescription, m_SecondBindingSourceDescription);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }
    }
}
#endif
