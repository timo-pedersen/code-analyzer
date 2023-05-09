#if !VNEXT_TARGET
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTagsViewer;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class StructuredTagsControlViewModelTest
    {
        private IMessageBoxServiceIde m_MessageBoxService;
        private IStructuredTagsViewerFacade m_StructuredTagsViewerFacade;
        private ILazy<INameService> m_NameService;

        [SetUp]
        public void SetUp()
        {
            m_MessageBoxService = Substitute.For<IMessageBoxServiceIde>();
            m_StructuredTagsViewerFacade = Substitute.For<IStructuredTagsViewerFacade>();
            m_NameService = Substitute.For<INameService>().ToILazy();
        }

        [Test]
        public void StructuredTags()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();
            structuredTag.Name = "Some name";

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { structuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // ACT
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Some name"));
            m_StructuredTagsViewerFacade.Received().GetTags();
        }

        [Test]
        public void StructuredTagsUpdated()
        {
            // ARRANGE
            var originalStructuredTag = Substitute.For<ITagEntity>();
            originalStructuredTag.Name = "Original tag";

            var updatedStructuredTag = Substitute.For<ITagEntity>();
            updatedStructuredTag.Name = "Updated tag";

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { originalStructuredTag }, new[] { updatedStructuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // ACT
            Raise.Event();
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Updated tag"));
            m_StructuredTagsViewerFacade.Received(2).GetTags();
        }

        [Test]
        public void CanNotRemove()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { structuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = null;

            // ACT
            bool canRemove = viewModel.RemoveCommand.CanExecute(null);

            // ASSERT
            Assert.That(canRemove, Is.False);
        }

        [Test]
        public void CanRemove()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { structuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = viewModel.StructuredTags.First();

            // ACT
            bool canRemove = viewModel.RemoveCommand.CanExecute(null);

            // ASSERT
            Assert.That(canRemove, Is.True);
        }

        [Test]
        public void Remove_UserAccepted()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();
            structuredTag.Name = "Tag";

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { structuredTag });

            m_MessageBoxService.Show(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    Arg.Any<DialogResult>())
                .Returns(DialogResult.Yes);

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = viewModel.StructuredTags.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_StructuredTagsViewerFacade.Received(1).DeleteTag("Tag");
            m_MessageBoxService.Received().Show(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    Arg.Any<DialogResult>());
        }

        [Test]
        public void Remove_UserDeclined()
        {
            // ARRANGE
            var structuredTag = Substitute.For<ITagEntity>();
            structuredTag.Name = "Tag";

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { structuredTag });

            m_MessageBoxService.Show(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    MessageBoxButtons.YesNo,
                    Arg.Any<MessageBoxIcon>(),
                    Arg.Any<DialogResult>())
                .Returns(DialogResult.No);

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = viewModel.StructuredTags.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_MessageBoxService.Received().Show(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    MessageBoxButtons.YesNo,
                    Arg.Any<MessageBoxIcon>(),
                    Arg.Any<DialogResult>());
            m_StructuredTagsViewerFacade.DidNotReceive().DeleteTag("Tag");
        }

        [Test]
        public void Dispose()
        {
            // ARRANGE
            var originalStructuredTag = Substitute.For<ITagEntity>();
            originalStructuredTag.Name = "Original tag";

            var updatedStructuredTag = Substitute.For<ITagEntity>();
            updatedStructuredTag.Name = "Updated tag";

            m_StructuredTagsViewerFacade.GetTags().Returns(new[] { originalStructuredTag }, new[] { updatedStructuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // Dispose view model, preventing it from getting any more events
            viewModel.Dispose();

            // ACT
            Raise.Event();
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Original tag"));
            m_StructuredTagsViewerFacade.Received(2).GetTags();
        }
    }
}
#endif
