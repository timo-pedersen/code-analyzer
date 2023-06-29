using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Api.CrossReference;
using Core.Api.GlobalReference;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.CrossReference.Validation;
using NSubstitute;
using NUnit.Framework;

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

        [SetUp]
        public void SetUp()
        {
            m_CrossReferenceQueryServiceMock = Substitute.For<ICrossReferenceQueryService>();
            m_CrossReferenceServiceMock = Substitute.For<ICrossReferenceService>();
            m_ProjectItemFinderMock = Substitute.For<IProjectItemFinder>();
            m_ExpressionsServiceMock = Substitute.For<IExpressionsService>();
            m_GlobalReferenceServiceMock = Substitute.For<IGlobalReferenceService>();
            m_ErrorListServiceMock = Substitute.For<IErrorListService>();
            m_OutputWindowServiceMock = Substitute.For<IOutputWindowService>();
            m_SubItemsServiceMock = Substitute.For<ISubItemsServiceIde>();
            m_CrossReferencesProjectValidator = new CrossReferencesProjectValidator(
                m_CrossReferenceQueryServiceMock.ToILazy(),
                m_CrossReferenceServiceMock.ToILazy(),
                m_ProjectItemFinderMock.ToILazy(),
                m_ExpressionsServiceMock.ToILazy(),
                m_GlobalReferenceServiceMock.ToILazy(),
                m_ErrorListServiceMock.ToILazy(),
                m_OutputWindowServiceMock.ToILazy(),
                m_SubItemsServiceMock.ToILazy()
                );
        }

        [Test]
        public void ValidateWhenNoCrossReferencesTest()
        {
            // Arrange
            m_CrossReferenceQueryServiceMock.GetReferences<ICrossReferenceItem>(Arg.Any<string[]>()).Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceServiceMock.GetReferences<ICrossReferenceItem>(Arg.Any<string[]>()).Returns(new List<ICrossReferenceItem>());
            m_ProjectItemFinderMock.GetProjectItems(Arg.Any<Func<IProjectItem, bool>>()).Returns(new IProjectItem[0]);
            m_ExpressionsServiceMock.Expressions.Returns(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.GetExpressions().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetFonts().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetTextLibraries().Returns(new List<ITextLibraryCrossReferenceItem>());

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
            m_CrossReferenceQueryServiceMock.GetReferences<ICrossReferenceItem>(Arg.Any<string[]>()).Returns(new List<ICrossReferenceItem> { item });

            m_ProjectItemFinderMock.GetProjectItems(Arg.Any<Func<IProjectItem, bool>>()).Returns(new IProjectItem[0]);
            m_ExpressionsServiceMock.Expressions.Returns(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.GetExpressions().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetFonts().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetTextLibraries().Returns(new List<ITextLibraryCrossReferenceItem>());

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
            m_CrossReferenceQueryServiceMock.GetReferences<ICrossReferenceItem>(Arg.Any<string[]>()).Returns(new List<ICrossReferenceItem> { item });

            // Setup an empty tag designer
            var tagDesignerProjectItem = Substitute.For<IDesignerProjectItem>();
            tagDesignerProjectItem.Name.Returns(designerName);
            var designerHost = Substitute.For<INeoDesignerHost>();
            designerHost.RootComponent.Returns(new Component());
            tagDesignerProjectItem.DesignerHost.Returns(designerHost);
            m_ProjectItemFinderMock.GetProjectItems(Arg.Any<Func<IProjectItem, bool>>()).Returns(new IProjectItem[] { tagDesignerProjectItem });

            m_ExpressionsServiceMock.Expressions.Returns(new ExtendedBindingList<IExpression>());
            m_CrossReferenceQueryServiceMock.GetExpressions().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetFonts().Returns(new List<ICrossReferenceItem>());
            m_CrossReferenceQueryServiceMock.GetTextLibraries().Returns(new List<ITextLibraryCrossReferenceItem>());
            m_SubItemsServiceMock.GetSubItems(Arg.Any<IComponent>(), Arg.Any<Func<IComponent, bool>>())
                .Returns(new ComponentCollection(new IComponent[0]));

            // Act
            var isValid = m_CrossReferencesProjectValidator.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}