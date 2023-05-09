using System;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.InformationDesignerImport.UI.ViewModels
{
    [TestFixture]
    public class ImportOptionsViewModelTest
    {
        private ImportOptionsViewModel m_ViewModel;
        private IInformationDesignerImportSettingsService m_InformationDesignerImportSettingsService;
        private IInformationDesignerImportService m_InformationDesignerImportService;

        [SetUp]
        public void SetUp()
        {
            m_InformationDesignerImportSettingsService = MockRepository.GenerateStub<IInformationDesignerImportSettingsService>();
            m_InformationDesignerImportService = MockRepository.GenerateStub<IInformationDesignerImportService>();

            m_InformationDesignerImportService.Stub(x => x.TargetType.IsPC).Return(false);

            m_ViewModel = new ImportOptionsViewModel(
                m_InformationDesignerImportSettingsService.ToILazy(),
                m_InformationDesignerImportService.ToILazy(),
                MockRepository.GenerateStub<IBrandServiceIde>().ToILazy());
        }

        [Test]
        public void OkButtonTextTest()
        {
            Assert.AreEqual(m_ViewModel.OKText, "Import");
        }

        [Test]
        public void CancelButtonTextTest()
        {
            Assert.AreEqual(m_ViewModel.CancelText, "Back");
        }

        [Test]
        public void TitleTest()
        {
            Assert.IsTrue(m_ViewModel.MainTitle.Contains("Import options for"));
        }

        [Test]
        public void DefaultSettingsTest()
        {
            Assert.IsFalse(m_ViewModel.ColorConversion);
            Assert.IsFalse(m_ViewModel.ResizeAllImages);
        }

        [Test]
        public void PartOfTotalTest()
        {
            Assert.AreEqual(m_ViewModel.PartOfTotal, "0/5");
        }

        [Test]
        public void OneOfTwoTest()
        {
            // ARRANGE
            m_ViewModel.ResizeAllImages = true;

            // ASSERT
            Assert.AreEqual(m_ViewModel.PartOfTotal, "1/5");
        }

        [Test]
        public void TwoOfTwoTest()
        {
            // ARRANGE
            m_ViewModel.ColorConversion = true;
            m_ViewModel.ResizeAllImages = true;

            // ASSERT
            Assert.AreEqual(m_ViewModel.PartOfTotal, "2/5");
        }

        [Test]
        public void SaveExecutedTest()
        {
            // ACT
            m_ViewModel.Save();

            // ASSERT
            m_InformationDesignerImportSettingsService.AssertWasCalled(x => x.Save());

        }
    }
}
