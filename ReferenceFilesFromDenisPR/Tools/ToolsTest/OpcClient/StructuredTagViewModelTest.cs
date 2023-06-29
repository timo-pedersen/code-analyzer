#if !VNEXT_TARGET
using System.Collections.ObjectModel;
using System.Linq;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTagsViewer;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class StructuredTagViewModelTest
    {
        [Test]
        public void Name()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();

            structuredTag.Name.Returns("Some name");

            var viewModel = new StructuredTagViewModel(structuredTag);

            // ACT
            string name = viewModel.Name;

            // ASSERT
            Assert.That(name, Is.EqualTo("Some name"));
        }

        [Test]
        public void Children()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();
            var structuredChildTag1 = Substitute.For<ITagEntity>();
            var structuredChildTag11 = Substitute.For<ITagEntity>();
            var structuredChildTag12 = Substitute.For<ITagEntity>();

            // Setup names
            structuredTag.Name = "Root";
            structuredChildTag1.Name = "Child 1";
            structuredChildTag11.Name = "Child 1.1";
            structuredChildTag12.Name = "Child 1.2";

            // Setup relationships
            structuredTag.Children.Returns(new[] { structuredChildTag1 });

            structuredChildTag1.Children.Returns(new[] { structuredChildTag11, structuredChildTag12 });

            structuredChildTag11.Children.Returns(Enumerable.Empty<ITagEntity>());

            structuredChildTag12.Children.Returns(Enumerable.Empty<ITagEntity>());

            var viewModel = new StructuredTagViewModel(structuredTag);

            // ACT
            ObservableCollection<StructuredTagViewModel> children = viewModel.Children;

            // ASSERT
            Assert.That(children.Count, Is.EqualTo(1));
            
            // Child 1
            var child1 = children.ElementAt(0);
            Assert.That(child1.Name, Is.EqualTo("Child 1"));
            Assert.That(child1.Children.Count, Is.EqualTo(2));

            // Child 1.1
            var child11 = child1.Children.ElementAt(0);
            Assert.That(child11.Name, Is.EqualTo("Child 1.1"));
            Assert.That(child11.Children.Count, Is.EqualTo(0));

            // Child 1.2
            var child12 = child1.Children.ElementAt(1);
            Assert.That(child12.Name, Is.EqualTo("Child 1.2"));
            Assert.That(child12.Children.Count, Is.EqualTo(0));
        }
    }
}
#endif
