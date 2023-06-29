#if!VNEXT_TARGET
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Common.Behaviors;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class HierarchicalBindingSourceDescriptionViewModelTest
    {
        [Test]
        public void Name_1()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription("Binding source name", null, null, false);

            var viewModel = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel.Name, Is.EqualTo("Binding source name"));
        }

        [Test]
        public void Name_2()
        {
            // ARRANGE
            var parent = Substitute.For<ITreeViewItem>();
            var viewModel = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);

            // ASSERT
            Assert.That(viewModel.Name, Is.EqualTo("Binding source name"));
        }

        [Test]
        public void IsMatch()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);
            
            firstGeneration.Children.Add(child);
            
            // ACT
            bool isMatch1 = firstGeneration.IsMatch("First generation");
            bool isMatch2 = firstGeneration.IsMatch("First");
            bool isMatch3 = firstGeneration.IsMatch("FIRST");
            bool isMatch4 = firstGeneration.IsMatch("generation");
            bool isMatch5 = firstGeneration.IsMatch("GENERATION");
            bool isMatch6 = firstGeneration.IsMatch("eneratio");

            // ASSERT
            Assert.That(isMatch1, Is.True);
            Assert.That(isMatch2, Is.True);
            Assert.That(isMatch3, Is.True);
            Assert.That(isMatch4, Is.True);
            Assert.That(isMatch5, Is.True);
            Assert.That(isMatch6, Is.True);
        }

        [Test]
        public void IsMatchDueToChild()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);

            firstGeneration.Children.Add(child);

            // ACT
            bool isMatch1 = firstGeneration.IsMatch("Child");
            bool isMatch2 = firstGeneration.IsMatch("CHILD");
            bool isMatch3 = firstGeneration.IsMatch("il");
            
            // ASSERT
            Assert.That(isMatch1, Is.True);
            Assert.That(isMatch2, Is.True);
            Assert.That(isMatch3, Is.True);
        }

        [Test]
        public void IsNotMatch()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);

            firstGeneration.Children.Add(child);

            // ACT
            bool isMatch = firstGeneration.IsMatch("Not matching name");

            // ASSERT
            Assert.That(isMatch, Is.False);
        }

        [Test]
        public void FirstGenerationIsExpanded()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);
            var grandChild = new HierarchicalBindingSourceDescriptionViewModel("Grandchild", child);

            firstGeneration.Children.Add(child);
            child.Children.Add(grandChild);

            // ACT
            firstGeneration.IsExpanded = true;

            // ASSERT
            Assert.That(firstGeneration.IsExpanded, Is.True);
            Assert.That(child.IsExpanded, Is.False);
            Assert.That(grandChild.IsExpanded, Is.False);
        }

        [Test]
        public void ChildIsExpanded()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);
            var grandChild = new HierarchicalBindingSourceDescriptionViewModel("Grandchild", child);

            firstGeneration.Children.Add(child);
            child.Children.Add(grandChild);

            // ACT
            child.IsExpanded = true;

            // ASSERT
            Assert.That(firstGeneration.IsExpanded, Is.True);
            Assert.That(child.IsExpanded, Is.True);
            Assert.That(grandChild.IsExpanded, Is.False);
        }

        [Test]
        public void GrandChildIsExpanded()
        {
            // ARRANGE
            var firstGeneration = new HierarchicalBindingSourceDescriptionViewModel("First generation", null);
            var child = new HierarchicalBindingSourceDescriptionViewModel("Child", firstGeneration);
            var grandChild = new HierarchicalBindingSourceDescriptionViewModel("Grandchild", child);

            firstGeneration.Children.Add(child);
            child.Children.Add(grandChild);

            // ACT
            grandChild.IsExpanded = true;

            // ASSERT
            Assert.That(firstGeneration.IsExpanded, Is.True);
            Assert.That(child.IsExpanded, Is.True);
            Assert.That(grandChild.IsExpanded, Is.True);
        }

        [Test]
        public void Equals_SameInstance_1()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();

            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);
            var viewModel2 = viewModel1;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_SameInstance_2()
        {
            // ARRANGE
            var parent = Substitute.For<ITreeViewItem>();
            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);
            var viewModel2 = viewModel1;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_FirstInstanceIsNull_1()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();

            HierarchicalBindingSourceDescriptionViewModel viewModel1 = null;
            var viewModel2 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_FirstInstanceIsNull_2()
        {
            // ARRANGE
            var parent = Substitute.For<ITreeViewItem>();
            HierarchicalBindingSourceDescriptionViewModel viewModel1 = null;
            var viewModel2 = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);

            // ASSERT
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_SecondInstanceIsNull_1()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();

            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);
            HierarchicalBindingSourceDescriptionViewModel viewModel2 = null;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_SecondInstanceIsNull_2()
        {
            // ARRANGE
            var parent = Substitute.For<ITreeViewItem>();
            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel("Binding source description", parent);
            HierarchicalBindingSourceDescriptionViewModel viewModel2 = null;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_BothInstancesAreNull()
        {
            // ARRANGE
            HierarchicalBindingSourceDescriptionViewModel viewModel1 = null;
            HierarchicalBindingSourceDescriptionViewModel viewModel2 = null;

            // ASSERT
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_SameFullName_1()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription(null, null, "Binding source full name", false);

            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);
            var viewModel2 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_SameFullName_2()
        {
            // ARRANGE
            var parent = Substitute.For<ITreeViewItem>();
            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);
            var viewModel2 = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_WrongType_1()
        {
            var bindingSourceDescription = new BindingSourceDescription();

            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel(bindingSourceDescription);
            var viewModel2 = new object();

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
        }

        [Test]
        public void Equals_WrongType_2()
        {
            var parent = Substitute.For<ITreeViewItem>();
            var viewModel1 = new HierarchicalBindingSourceDescriptionViewModel("Binding source name", parent);
            var viewModel2 = new object();

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
        }
    }
}
#endif
