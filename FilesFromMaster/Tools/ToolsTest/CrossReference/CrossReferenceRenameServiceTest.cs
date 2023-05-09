using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Core.Api.CrossReference;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using Neo.ApplicationFramework.Tools.CrossReference.Shell;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ProjectItemFinderMock = MockRepository.GenerateMock<IProjectItemFinder>();
            m_ProjectManagerMock = MockRepository.GenerateStub<IProjectManager>();
            m_GlobalReferenceServiceMock = MockRepository.GenerateMock<IGlobalReferenceService>();
            m_CrossReferenceRebinderServiceMock = MockRepository.GenerateMock<ICrossReferenceRebinderService>();
            m_CrossReferenceServiceMock = MockRepository.GenerateMock<ICrossReferenceService>();
            m_InformationProgressServiceMock = MockRepository.GenerateMock<IInformationProgressService>();
            m_MessageBoxService = MockRepository.GenerateStub<IMessageBoxServiceIde>();

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
            m_ProjectManagerMock.VerifyAllExpectations();
            m_MessageBoxService.VerifyAllExpectations();
        }

        [Test]
        public void UpdateNameByCrossReferencesVerifyProjectDirty()
        {
            // Arrange
            string oldGroupName = "OldGroup";
            string newGroupName = "NewGroup";
            var textLibraryCrossReferenceItems = new List<ITextLibraryCrossReferenceItem>();
            textLibraryCrossReferenceItems.Add(new TextLibraryCrossReferenceItem("Screen1.Button", "TargetPropertyName", oldGroupName));            

            m_CrossReferenceServiceMock.Stub(serviceMock => serviceMock.GetReferences<ITextLibraryCrossReferenceItem>(Arg<string>.Is.Anything)).Return(textLibraryCrossReferenceItems);

            var targetObject = new object();
            m_GlobalReferenceServiceMock.Stub<IGlobalReferenceService>(serviceMock => serviceMock.GetObject<object>(oldGroupName)).Return(targetObject);

            var rebinder = MockRepository.GenerateMock<ICrossReferenceRebinder>();
            rebinder.Stub<ICrossReferenceRebinder>(rebinderMock => rebinderMock.Rebind(Arg<object>.Is.Anything, Arg<ICrossReferenceItem>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(true);

            m_CrossReferenceRebinderServiceMock.Stub<ICrossReferenceRebinderService>(serviceMock => serviceMock.GetRebinder(Arg<ICrossReferenceItem>.Is.Anything, Arg<string>.Is.Anything)).Return(rebinder);
            var designerProjectItem = MockRepository.GenerateMock<IDesignerProjectItem>();
            m_ProjectItemFinderMock.Stub<IProjectItemFinder>(finderMock => finderMock.GetProjectItem(Arg<object>.Is.Anything)).Return(designerProjectItem);

            // Act
            m_RenamerService.UpdateNameByCrossReferences<ITextLibraryCrossReferenceItem>(oldGroupName, newGroupName, null, CrossReferenceTypes.TextLibrary.ToString());

            // Assert
            designerProjectItem.Expect(projectItemMock => projectItemMock.FireItemChanged()).Repeat.Once();
        }

        [Test]
        public void UpdateTextLibraryGroupNameNoCrossReferencesVerifyUpdateCancelled()
        {
            // Arrange
            const string oldGroupName = "OldGroup";
            const string newGroupName = "NewGroup";
            var textLibraryCrossReferenceItems = new List<ITextLibraryCrossReferenceItem>();
            m_CrossReferenceServiceMock.Stub(serviceMock => serviceMock.GetReferences<ITextLibraryCrossReferenceItem>(Arg<string>.Is.Anything)).Return(textLibraryCrossReferenceItems);

            // Act
            m_RenamerService.UpdateNameByCrossReferences<ITextLibraryCrossReferenceItem>(oldGroupName, newGroupName, null, CrossReferenceTypes.TextLibrary.ToString());

            // Assert
            m_GlobalReferenceServiceMock.Expect(serviceMock => serviceMock.GetObject<object>(Arg<string>.Is.Anything)).Repeat.Never();
        }

        [Test]
        public void NameShouldBeUpdatedNoReferencesVerifyShouldBeUpdated()
        {
            // Arrange
            string categoryName = CrossReferenceTypes.GlobalDataItem.ToString();

            m_ProjectManagerMock.Stub(x => x.EnsureProjectSaved()).Return(true);
            m_CrossReferenceServiceMock.Stub(x => x.GetReferences<IActionCrossReferenceItem>(Arg<string>.Is.Anything)).Return(new List<IActionCrossReferenceItem>());
            m_MessageBoxService.Expect(x => x.Show(Arg<string>.Is.Anything)).Repeat.Never();

            // Act
            bool shouldBeUpdated = m_RenamerService.NameShouldBeUpdated<IActionCrossReferenceItem>(OldName, null, categoryName);

            // Assert
            Assert.IsTrue(shouldBeUpdated);
        }

        [Test]
        public void NameShouldBeUpdatedSaveProjectAndUserClicksYesVerifyShouldBeUpdated()
        {
            // Arrange
            string categoryName = CrossReferenceTypes.GlobalDataItem.ToString();
            var crossReferenceItems = new List<IActionCrossReferenceItem> { new ActionCrossReferenceItem { SourceFullName = OldName } };

            m_ProjectManagerMock.Stub(x => x.IsProjectDirty).Return(true);
            m_ProjectManagerMock.Stub(x => x.SaveProject()).Return(true);
            m_ProjectManagerMock.Stub(x => x.EnsureProjectSaved()).Return(true);
            m_MessageBoxService.Stub(x => x.Show(Arg<string>.Is.Anything)).Return(DialogResult.Yes);
            m_CrossReferenceServiceMock.Stub(x => x.GetReferences<IActionCrossReferenceItem>(Arg<string>.Is.Anything)).Return(crossReferenceItems);
            m_GlobalReferenceServiceMock.Stub(x => x.GetObject<object>(Arg<string>.Is.Anything)).Return(new object());

            var crossReferenceBinderMock = MockRepository.GenerateMock<ICrossReferenceRebinder>();
            crossReferenceBinderMock.Stub(x => x.Rebind(Arg<object>.Is.Anything, Arg<ICrossReferenceItem>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            m_CrossReferenceRebinderServiceMock.Stub(x => x.GetRebinder(Arg<ICrossReferenceItem>.Is.Anything, Arg<string>.Is.Anything)).Return(crossReferenceBinderMock);

            m_MessageBoxService.Expect(x => x.Show(Arg<string>.Is.Anything)).Repeat.Twice();
            m_ProjectManagerMock.Expect(x => x.SaveProject());

            // Act
            bool shouldBeUpdated = m_RenamerService.NameShouldBeUpdated<IActionCrossReferenceItem>(OldName, null, categoryName);

            // Assert
            Assert.IsTrue(shouldBeUpdated);        
        }

        [Test]
        public void UpdateNameByCrossReferencesWhenFirstTargetIsNullVerifySecondRenamed()
        {
            // Arrange
            const string targetFullName = "TargetFullName";
            var crossReferenceRebinderMock = MockRepository.GenerateMock<ICrossReferenceRebinder>();
            crossReferenceRebinderMock.Expect(x => x.Rebind(null, null, null, null)).IgnoreArguments().Repeat.Once().Return(false);

            // Setup two crossreferences where the first one has an empty target.
            var crossReferenceItems = new List<ITrendViewerCrossReferenceItem>
            {
                new TrendViewerCrossReferenceItem { SourceFullName = OldName, TargetFullName = string.Empty },
                new TrendViewerCrossReferenceItem { SourceFullName = OldName, TargetFullName = targetFullName }
            };
            m_CrossReferenceServiceMock.Stub(x => x.GetReferences<ITrendViewerCrossReferenceItem>(Arg<string>.Is.Anything)).Return(crossReferenceItems);
            m_GlobalReferenceServiceMock.Stub(x => x.GetObject<object>(targetFullName)).Return(new object());
            m_GlobalReferenceServiceMock.Stub(x => x.GetObject<object>(string.Empty)).Return(null);
            m_CrossReferenceRebinderServiceMock.Stub(x => x.GetRebinder(Arg<ITrendViewerCrossReferenceItem>.Is.Anything, Arg<string>.Is.Anything)).Return(crossReferenceRebinderMock);
            
                // Act
            bool wasUpdated = m_RenamerService.UpdateNameByCrossReferences<ITrendViewerCrossReferenceItem>(OldName, NewName, null, CrossReferenceTypes.TrendViewer.ToString());

            // Assert
            Assert.IsTrue(wasUpdated);            
            crossReferenceRebinderMock.VerifyAllExpectations();
        }
    }
}
