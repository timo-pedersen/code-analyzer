using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient.Controls;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTagsViewer;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_MessageBoxService = MockRepository.GenerateMock<IMessageBoxServiceIde>();
            m_StructuredTagsViewerFacade = MockRepository.GenerateMock<IStructuredTagsViewerFacade>();
            m_NameService = MockRepository.GenerateMock<INameService>().ToILazy();
        }

        [Test]
        public void StructuredTags()
        {
            // ARRANGE
            var structuredTag = MockRepository.GenerateStub<ITagEntity>();
            structuredTag.Name = "Some name";

            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { structuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // ACT
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Some name"));
        }

        [Test]
        public void StructuredTagsUpdated()
        {
            // ARRANGE
            var originalStructuredTag = MockRepository.GenerateStub<ITagEntity>();
            originalStructuredTag.Name = "Original tag";

            var updatedStructuredTag = MockRepository.GenerateStub<ITagEntity>();
            updatedStructuredTag.Name = "Updated tag";

            // Return original the first time method is called
            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { originalStructuredTag })
                .Repeat.Once();

            // Return updated the second time method is called
            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { updatedStructuredTag })
                .Repeat.Once(); 

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // ACT
            m_StructuredTagsViewerFacade.Raise(ss => ss.TagsChanged += null, m_StructuredTagsViewerFacade, EventArgs.Empty);
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Updated tag"));
            m_StructuredTagsViewerFacade.VerifyAllExpectations();
        }

        [Test]
        public void CanNotRemove()
        {
            // ARRANGE
            var structuredTag = MockRepository.GenerateStub<ITagEntity>();

            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { structuredTag });

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
            var structuredTag = MockRepository.GenerateStub<ITagEntity>();

            m_StructuredTagsViewerFacade
                .Stub(stvf => stvf.GetTags())
                .Return(new[] { structuredTag });

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
            var structuredTag = MockRepository.GenerateStub<ITagEntity>();
            structuredTag.Name = "Tag";

            m_StructuredTagsViewerFacade
                .Stub(stvf => stvf.GetTags())
                .Return(new[] { structuredTag });

            m_MessageBoxService
                .Expect(mbs => mbs.Show(
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<MessageBoxButtons>.Is.Equal(MessageBoxButtons.YesNo),
                    Arg<MessageBoxIcon>.Is.Equal(MessageBoxIcon.Warning),
                    Arg<DialogResult>.Is.Anything))
                .Return(DialogResult.Yes);

            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.DeleteTag("Tag"))
                .Repeat.Once();

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = viewModel.StructuredTags.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_MessageBoxService.VerifyAllExpectations();
            m_StructuredTagsViewerFacade.VerifyAllExpectations();
        }

        [Test]
        public void Remove_UserDeclined()
        {
            // ARRANGE
            var structuredTag = MockRepository.GenerateStub<ITagEntity>();
            structuredTag.Name = "Tag";

            m_StructuredTagsViewerFacade
                .Stub(stvf => stvf.GetTags())
                .Return(new[] { structuredTag });

            m_MessageBoxService
                .Expect(mbs => mbs.Show(
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<MessageBoxButtons>.Is.Equal(MessageBoxButtons.YesNo),
                    Arg<MessageBoxIcon>.Is.Anything,
                    Arg<DialogResult>.Is.Anything))
                .Return(DialogResult.No);

            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.DeleteTag("Tag"))
                .Repeat.Never();

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);
            viewModel.SelectedStructuredTag = viewModel.StructuredTags.First();

            // ACT
            viewModel.RemoveCommand.Execute(null);

            // ASSERT
            m_MessageBoxService.VerifyAllExpectations();
            m_StructuredTagsViewerFacade.VerifyAllExpectations();
        }

        [Test]
        public void Dispose()
        {
            // ARRANGE
            var originalStructuredTag = MockRepository.GenerateStub<ITagEntity>();
            originalStructuredTag.Name = "Original tag";

            var updatedStructuredTag = MockRepository.GenerateStub<ITagEntity>();
            updatedStructuredTag.Name = "Updated tag";

            // Return original the first time method is called
            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { originalStructuredTag })
                .Repeat.Once();

            // Return updated the second time method is called
            m_StructuredTagsViewerFacade
                .Expect(stvf => stvf.GetTags())
                .Return(new[] { updatedStructuredTag });

            var viewModel = new StructuredTagsControlViewModel(m_MessageBoxService, m_StructuredTagsViewerFacade, m_NameService);

            // Dispose view model, preventing it from getting any more events
            viewModel.Dispose();

            // ACT
            m_StructuredTagsViewerFacade.Raise(ss => ss.TagsChanged += null, m_StructuredTagsViewerFacade, EventArgs.Empty);
            ObservableCollection<StructuredTagViewModel> structuredTags = viewModel.StructuredTags;

            // ASSERT
            Assert.That(structuredTags.Count, Is.EqualTo(1));
            Assert.That(structuredTags.ElementAt(0).Name, Is.EqualTo("Original tag"));
        }
    }
}