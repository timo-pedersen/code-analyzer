using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Core.Api.CrossReference;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using Neo.ApplicationFramework.Tools.CrossReference.Shell;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CrossReference
{
    [TestFixture]
    public class CrossReferenceRenameServiceTest
    {
        private const string OldName = "OldName";
        private const string NewName = "NewName";

        private IProjectItemFinder m_ProjectItemFinderMock;
        private IProjectManager m_ProjectManagerMock;
        private IGlobalReferenceService m_GlobalReferenceServiceMock;
        private ICrossReferenceRebinderService m_CrossReferenceRebinderServiceMock;
        private ICrossReferenceService m_CrossReferenceServiceMock;
        private IInformationProgressService m_InformationProgressServiceMock;
        private IMessageBoxServiceIde m_MessageBoxService;

        /// <summary>
        /// The service under test.
        /// </summary>
        private ICrossReferenceRenameService m_RenamerService;

        [SetUp]
        public void Setup()
        {
            m_ProjectItemFinderMock = Substitute.For<IProjectItemFinder>();
            m_ProjectManagerMock = Substitute.For<IProjectManager>();
            m_GlobalReferenceServiceMock = Substitute.For<IGlobalReferenceService>();
            m_CrossReferenceRebinderServiceMock = Substitute.For<ICrossReferenceRebinderService>();
            m_CrossReferenceServiceMock = Substitute.For<ICrossReferenceService>();
            m_InformationProgressServiceMock = Substitute.For<IInformationProgressService>();
            m_MessageBoxService = Substitute.For<IMessageBoxServiceIde>();

            var projectItemFinder = m_ProjectItemFinderMock.ToILazy();
            var projectManager =  m_ProjectManagerMock.ToILazy();
            var globalReferenceService = m_GlobalReferenceServiceMock.ToILazy();
            var crossReferenceRebinderService = m_CrossReferenceRebinderServiceMock.ToILazy();
            var crossReferenceService = m_CrossReferenceServiceMock.ToILazy();
            var informationProgressService = m_InformationProgressServiceMock.ToILazy();
            var messageBoxService = m_MessageBoxService.ToILazy();

            m_RenamerService = new CrossReferenceRenameService(
                projectItemFinder,
                globalReferenceService,
                crossReferenceRebinderService,
                crossReferenceService,
                projectManager,
                informationProgressService,
                messageBoxService);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void UpdateNameByCrossReferencesVerifyProjectDirty()
        {
            // Arrange
            string oldGroupName = "OldGroup";
            string newGroupName = "NewGroup";
            string targetFullName = "Screen1.Button";
            var textLibraryCrossReferenceItems = new List<ITextLibraryCrossReferenceItem>();
            textLibraryCrossReferenceItems.Add(new TextLibraryCrossReferenceItem(targetFullName, "TargetPropertyName", oldGroupName));

            m_CrossReferenceServiceMock.GetReferences<ITextLibraryCrossReferenceItem>(Arg.Any<string>()).Returns(textLibraryCrossReferenceItems);

            var targetObject = new object();
            m_GlobalReferenceServiceMock.GetObject<object>(targetFullName).Returns(targetObject);

            var rebinder = Substitute.For<ICrossReferenceRebinder>();
            rebinder.Rebind(Arg.Any<object>(), Arg.Any<ICrossReferenceItem>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            m_CrossReferenceRebinderServiceMock.GetRebinder(Arg.Any<ICrossReferenceItem>(), Arg.Any<string>()).Returns(rebinder);
            var designerProjectItem = Substitute.For<IDesignerProjectItem>();
            m_ProjectItemFinderMock.GetProjectItem(Arg.Any<string>()).Returns(designerProjectItem);

            // Act
            m_RenamerService.UpdateNameByCrossReferences<ITextLibraryCrossReferenceItem>(oldGroupName, newGroupName, null, CrossReferenceTypes.TextLibrary.ToString());

            // Assert
            designerProjectItem.Received(1).FireItemChanged();
        }

        [Test]
        public void UpdateTextLibraryGroupNameNoCrossReferencesVerifyUpdateCancelled()
        {
            // Arrange
            const string oldGroupName = "OldGroup";
            const string newGroupName = "NewGroup";
            var textLibraryCrossReferenceItems = new List<ITextLibraryCrossReferenceItem>();
            m_CrossReferenceServiceMock.GetReferences<ITextLibraryCrossReferenceItem>(Arg.Any<string>()).Returns(textLibraryCrossReferenceItems);

            // Act
            m_RenamerService.UpdateNameByCrossReferences<ITextLibraryCrossReferenceItem>(oldGroupName, newGroupName, null, CrossReferenceTypes.TextLibrary.ToString());

            // Assert
            m_GlobalReferenceServiceMock.DidNotReceiveWithAnyArgs().GetObject<object>(Arg.Any<string>());
        }

        [Test]
        public void NameShouldBeUpdatedNoReferencesVerifyShouldBeUpdated()
        {
            // Arrange
            string categoryName = CrossReferenceTypes.GlobalDataItem.ToString();

            m_ProjectManagerMock.EnsureProjectSaved().Returns(true);
            m_CrossReferenceServiceMock.GetReferences<IActionCrossReferenceItem>(Arg.Any<string>()).Returns(new List<IActionCrossReferenceItem>());

            // Act
            bool shouldBeUpdated = m_RenamerService.NameShouldBeUpdated<IActionCrossReferenceItem>(OldName, null, categoryName);

            // Assert
            Assert.IsTrue(shouldBeUpdated);
            m_MessageBoxService.DidNotReceiveWithAnyArgs().Show(Arg.Any<string>());
        }

        [Test]
        public void NameShouldBeUpdatedSaveProjectAndUserClicksYesVerifyShouldBeUpdated()
        {
            // Arrange
            string categoryName = CrossReferenceTypes.GlobalDataItem.ToString();
            var crossReferenceItems = new List<IActionCrossReferenceItem> { new ActionCrossReferenceItem { SourceFullName = OldName } };

            m_ProjectManagerMock.EnsureProjectSaved().Returns(true);
            m_MessageBoxService.Show(Arg.Any<string>(), Arg.Any<string>(), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, DialogResult.OK)
                .Returns(DialogResult.Yes);
            m_CrossReferenceServiceMock.GetReferences<IActionCrossReferenceItem>(Arg.Any<string>()).Returns(crossReferenceItems);
            m_GlobalReferenceServiceMock.GetObject<object>(Arg.Any<string>()).Returns(new object());

            // Act
            bool shouldBeUpdated = m_RenamerService.NameShouldBeUpdated<IActionCrossReferenceItem>(OldName, null, categoryName);

            // Assert
            Assert.IsTrue(shouldBeUpdated);
            m_MessageBoxService.ReceivedWithAnyArgs(1)
                .Show(Arg.Any<string>(), Arg.Any<string>(), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, DialogResult.OK);
        }

        [Test]
        public void UpdateNameByCrossReferencesWhenFirstTargetIsNullVerifySecondRenamed()
        {
            // Arrange
            const string targetFullName = "TargetFullName";
            var crossReferenceRebinderMock = Substitute.For<ICrossReferenceRebinder>();
            crossReferenceRebinderMock.Rebind(Arg.Any<object>(), Arg.Any<ICrossReferenceItem>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(false);

            // Setup two crossreferences where the first one has an empty target.
            var crossReferenceItems = new List<ITrendViewerCrossReferenceItem>
            {
                new TrendViewerCrossReferenceItem { SourceFullName = OldName, TargetFullName = string.Empty },
                new TrendViewerCrossReferenceItem { SourceFullName = OldName, TargetFullName = targetFullName }
            };
            m_CrossReferenceServiceMock.GetReferences<ITrendViewerCrossReferenceItem>(Arg.Any<string>()).Returns(crossReferenceItems);
            m_GlobalReferenceServiceMock.GetObject<object>(targetFullName).Returns(new object());
            m_GlobalReferenceServiceMock.GetObject<object>(string.Empty).Returns(null);
            m_CrossReferenceRebinderServiceMock.GetRebinder(Arg.Any<ITrendViewerCrossReferenceItem>(), Arg.Any<string>())
                .Returns(crossReferenceRebinderMock);

                // Act
            bool wasUpdated = m_RenamerService.UpdateNameByCrossReferences<ITrendViewerCrossReferenceItem>(OldName, NewName, null, CrossReferenceTypes.TrendViewer.ToString());

            // Assert
            Assert.IsTrue(wasUpdated);
            crossReferenceRebinderMock.Rebind(Arg.Any<object>(), Arg.Any<ICrossReferenceItem>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
