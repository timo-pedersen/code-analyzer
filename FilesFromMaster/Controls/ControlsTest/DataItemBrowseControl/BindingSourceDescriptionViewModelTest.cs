using Core.Controls.Api.Bindings.DataSources;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class BindingSourceDescriptionViewModelTest
    {
        [Test]
        public void Name()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription("Binding source name", null, null, false);
            
            var viewModel = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel.Name, Is.EqualTo("Binding source name"));
        }

        [Test]
        public void IsMatch()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription(null, "Binding source name", null, false);

            var viewModel = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ACT
            bool isMatch1 = viewModel.IsMatch("Binding source name");
            bool isMatch2 = viewModel.IsMatch("Binding");
            bool isMatch3 = viewModel.IsMatch("BINDING");
            bool isMatch4 = viewModel.IsMatch("source");
            bool isMatch5 = viewModel.IsMatch("SOURCE");
            bool isMatch6 = viewModel.IsMatch("name");
            bool isMatch7 = viewModel.IsMatch("NAME");
            bool isMatch8 = viewModel.IsMatch("ourc");

            // ASSERT
            Assert.That(isMatch1, Is.True);
            Assert.That(isMatch2, Is.True);
            Assert.That(isMatch3, Is.True);
            Assert.That(isMatch4, Is.True);
            Assert.That(isMatch5, Is.True);
            Assert.That(isMatch6, Is.True);
            Assert.That(isMatch7, Is.True);
            Assert.That(isMatch8, Is.True);
        }

        [Test]
        public void IsNotMatch()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription(null, "Binding source name", null, false);

            var viewModel = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ACT
            bool isMatch = viewModel.IsMatch("Not matching name");

            // ASSERT
            Assert.That(isMatch, Is.False);
        }

        [Test]
        public void Equals_SameInstance()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();
            
            var viewModel1 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);
            var viewModel2 = viewModel1;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_FirstInstanceIsNull()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();

            BindingSourceDescriptionViewModel viewModel1 = null;
            var viewModel2 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_SecondInstanceIsNull()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription();

             var viewModel1 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);
             BindingSourceDescriptionViewModel viewModel2 = null;

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
            Assert.That(viewModel1 == viewModel2, Is.False);
            Assert.That(viewModel1 != viewModel2, Is.True);
        }

        [Test]
        public void Equals_BothInstancesAreNull()
        {
            // ARRANGE
            BindingSourceDescriptionViewModel viewModel1 = null;
            BindingSourceDescriptionViewModel viewModel2 = null;

            // ASSERT
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_SameFullName()
        {
            // ARRANGE
            var bindingSourceDescription = new BindingSourceDescription(null, null, "Binding source full name", false);

            var viewModel1 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);
            var viewModel2 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.True);
            Assert.That(viewModel1 == viewModel2, Is.True);
            Assert.That(viewModel1 != viewModel2, Is.False);
        }

        [Test]
        public void Equals_WrongType()
        {
            var bindingSourceDescription = new BindingSourceDescription();

            var viewModel1 = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);
            var viewModel2 = new object();

            // ASSERT
            Assert.That(viewModel1.Equals(viewModel2), Is.False);
        }
    }
}