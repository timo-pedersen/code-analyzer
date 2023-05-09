#if !VNEXT_TARGET
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.TrendViewer.Validation;
using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace Neo.ApplicationFramework.Tools.TrendViewer
{
    [TestFixture]
    class TrendViewerValidatorTest
    {
        private readonly string m_ErrorMessage = TextsIde.TrendViewerValidationMessage;
        private IErrorListService m_ErrorListService;
        private ICrossReferenceService m_CrossReferenceService;
        private TrendViewerValidator m_TrendViewerValidator;
        private List<string> m_ValidateWarnings;

        [SetUp]
        public void Setup()
        {
            AddServices();
            m_TrendViewerValidator = new TrendViewerValidator(m_ErrorListService.ToILazy(), m_CrossReferenceService.ToILazy());
            m_ValidateWarnings = new List<string>();
        }

        [Test]
        public void NoTrendViewersYieldsNoBuildWarning()
        {
            //ARRANGE
            m_CrossReferenceService.GetReferences<ITrendViewerCrossReferenceItem>().Returns(new ITrendViewerCrossReferenceItem[0]);

            //ACT
            m_TrendViewerValidator.Validate();

            //ASSERT
            Assert.That(m_ValidateWarnings.IsNullOrEmpty);
        }

        [Test]
        public void OneTrendViewerWithNoCacheYieldsNoBuildWarning()
        {
            //ARRANGE
            var trendViewerCrossReferenceItem = new ITrendViewerCrossReferenceItem[] { GetTrendViewerWithNoCache() };
            m_CrossReferenceService.GetReferences<ITrendViewerCrossReferenceItem>().Returns(trendViewerCrossReferenceItem);

            //ACT
            m_TrendViewerValidator.Validate();

            //ASSERT
            Assert.That(m_ValidateWarnings.IsNullOrEmpty);
        }

        [Test]
        public void OneTrendViewerWithCacheYieldsOneBuildWarning()
        {
            //ARRANGE
            var trendViewerCrossReferenceItem = new ITrendViewerCrossReferenceItem[] { GetTrendViewerWithCache() };
            m_CrossReferenceService.GetReferences<ITrendViewerCrossReferenceItem>().Returns(trendViewerCrossReferenceItem);

            //ACT
            m_TrendViewerValidator.Validate();

            //ASSERT
            Assert.That(m_ValidateWarnings.Contains(m_ErrorMessage));
            Assert.AreEqual(m_ValidateWarnings.Count, 1);
        }

        [Test]
        public void TwoTrendViewersWithCacheYieldsOneBuildWarning()
        {
            //ARRANGE
            var trendViewerCrossReferenceItem = new ITrendViewerCrossReferenceItem[] { GetTrendViewerWithCache(), GetTrendViewerWithCache() };
            m_CrossReferenceService.GetReferences<ITrendViewerCrossReferenceItem>().Returns(trendViewerCrossReferenceItem);

            //ACT
            m_TrendViewerValidator.Validate();

            //ASSERT
            Assert.That(m_ValidateWarnings.Contains(m_ErrorMessage));
            Assert.AreEqual(m_ValidateWarnings.Count, 1);
        }

        private ITrendViewerCrossReferenceItem GetTrendViewerWithNoCache()
        {
            var trendViewer = GetTrendViewerStub();
            trendViewer.InitialBufferSize = TimeSpan.Zero;
            return trendViewer;
        }

        private ITrendViewerCrossReferenceItem GetTrendViewerWithCache()
        {
            var trendViewer = GetTrendViewerStub();
            trendViewer.InitialBufferSize = new TimeSpan(0, 0, 0, 10);
            return trendViewer;
        }

        private ITrendViewerCrossReferenceItem GetTrendViewerStub()
        {
            return Substitute.For<ITrendViewerCrossReferenceItem>();
        }

        private void AddServices()
        {
            m_CrossReferenceService = TestHelper.CreateAndAddServiceStub<ICrossReferenceService>();

            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();
            m_ErrorListService.When(x => x.AddNewCompilerError(m_ErrorMessage, true)).Do(y => m_ValidateWarnings.Add(m_ErrorMessage));
        }
    }
}
#endif
