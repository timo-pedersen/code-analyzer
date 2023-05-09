using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Api.CrossReference;
using Core.Component.Api.Design;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.CrossReference.Validation;
using NUnit.Framework;
using Rhino.Mocks;
using Neo.ApplicationFramework.Common.CrossReference;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    public class CrossReferencesValidatorTest
    {
        private CrossReferencesProjectValidator m_CrossReferencesProjectValidator;
        private ICrossReferenceQueryService m_CrossReferenceQueryServiceMock;
        private ICrossReferenceService m_CrossReferenceServiceMock;
        private IProjectItemFinder m_ProjectItemFinderMock;
        private IExpressionsService m_ExpressionsServiceMock;
        private IOutputWindowService m_OutputWindowServiceMock;
        private IGlobalReferenceService m_GlobalReferenceServiceMock;
        private IErrorListService m_ErrorListServiceMock;
        private ISubItemsServiceIde m_SubItemsServiceMock;
        private IFontService m_FontServiceMock;

        [SetUp]
        public void SetUp()
        {
            m_CrossReferenceQueryServiceMock = MockRepository.GenerateMock<ICrossReferenceQueryService>();
            m_CrossReferenceServiceMock = MockRepository.GenerateMock<ICrossReferenceService>();
            m_ProjectItemFinderMock = MockRepository.GenerateMock<IProjectItemFinder>();
            m_ExpressionsServiceMock = MockRepository.GenerateMock<IExpressionsService>();
            m_GlobalReferenceServiceMock = MockRepository.GenerateMock<IGlobalReferenceService>();
            m_ErrorListServiceMock = MockRepository.GenerateMock<IErrorListService>();
            m_OutputWindowServiceMock = MockRepository.GenerateStub<IOutputWindowService>();
            m_SubItemsServiceMock = MockRepository.GenerateMock<ISubItemsServiceIde>();
            m_FontServiceMock = MockRepository.GenerateMock<IFontService>();
            m_CrossReferencesProjectValidator = new CrossReferencesProjectValidator(
                m_CrossReferenceQueryServiceMock.ToILazy(),
                m_CrossReferenceServiceMock.ToILazy(),
                m_ProjectItemFinderMock.ToILazy(),
                m_ExpressionsServiceMock.ToILazy(),
                m_GlobalReferenceServiceMock.ToILazy(),
                m_ErrorListServiceMock.ToILazy(),
                m_OutputWindowServiceMock.ToILazy(),
                m_SubItemsServiceMock.ToILazy(),
                m_FontServiceMock.ToILazy()
                );
        }

        [Test]
        public void ValidateWhenNoCrossReferencesTest()
        {
            // Arrange
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetReferences<ICrossReferenceItem>(Arg<string[]>.Is.Anything)).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceServiceMock.Stub(x => x.GetReferences<ICrossReferenceItem>(Arg<string[]>.Is.Anything)).Return(new List<ICrossReferenceItem>());
            m_ProjectItemFinderMock.Stub(x => x.GetProjectItems(Arg<Func<IProjectItem, bool>>.Is.Anything)).Return(new IProjectItem[0]);
            m_ExpressionsServiceMock.Stub(x => x.Expressions).Return(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetExpressions()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetFonts()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetTextLibraries()).Return(new List<ITextLibraryCrossReferenceItem>());

            // Act
            var isValid = m_CrossReferencesProjectValidator.Validate();

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void ValidateWhenMissingDesignerForCrossReferenceVerifyInvalidTest()
        {
            // Arrange
            const string targetFullName = "targetFullName";
            const string targetPropertyName = "targetPropertyName";
            string sourceFullName = StringConstants.TagsRoot + "Tag1";
            ICrossReferenceItem item = new CrossReferenceItem(targetFullName, targetPropertyName, sourceFullName);
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetReferences<ICrossReferenceItem>(Arg<string[]>.Is.Anything)).Return(new List<ICrossReferenceItem> { item });

            m_ProjectItemFinderMock.Stub(x => x.GetProjectItems(Arg<Func<IProjectItem, bool>>.Is.Anything)).Return(new IProjectItem[0]);
            m_ExpressionsServiceMock.Stub(x => x.Expressions).Return(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetExpressions()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetFonts()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetTextLibraries()).Return(new List<ITextLibraryCrossReferenceItem>());

            // Act
            var isValid = m_CrossReferencesProjectValidator.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateWhenMissingTagForCrossReferenceVerifyInvalidTest()
        {
            // Arrange
            const string targetFullName = "targetFullName";
            const string targetPropertyName = "targetPropertyName";
            const string designerName = StringConstants.Tags;
            const string sourceFullName = designerName + StringConstants.ObjectNameSeparator + ".Tag1";
            ICrossReferenceItem item = new CrossReferenceItem(targetFullName, targetPropertyName, sourceFullName);
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetReferences<ICrossReferenceItem>(Arg<string[]>.Is.Anything)).Return(new List<ICrossReferenceItem> { item });

            // Setup an empty tag designer
            var tagDesignerProjectItem = MockRepository.GenerateMock<IDesignerProjectItem>();
            tagDesignerProjectItem.Stub(x => x.Name).Return(designerName);
            var designerHost = MockRepository.GenerateMock<INeoDesignerHost>();
            designerHost.Stub(x => x.RootComponent).Return(new Component());
            tagDesignerProjectItem.Stub(x => x.DesignerHost).Return(designerHost);
            m_ProjectItemFinderMock.Stub(x => x.GetProjectItems(Arg<Func<IProjectItem, bool>>.Is.Anything)).Return(new IProjectItem[] { tagDesignerProjectItem });

            m_ExpressionsServiceMock.Stub(x => x.Expressions).Return(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetExpressions()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetFonts()).Return(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.Stub(x => x.GetTextLibraries()).Return(new List<ITextLibraryCrossReferenceItem>());
            m_SubItemsServiceMock.Stub(x => x.GetSubItems(Arg<IComponent>.Is.Anything, Arg<Func<IComponent, bool>>.Is.Anything)).Return(new ComponentCollection(new IComponent[0]));

            // Act
            var isValid = m_CrossReferencesProjectValidator.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}