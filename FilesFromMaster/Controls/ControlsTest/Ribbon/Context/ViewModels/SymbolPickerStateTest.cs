using System.ComponentModel.Design;
using Neo.ApplicationFramework.Controls.Symbol;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class SymbolPickerStateTest
    {
        private ISelectionService m_SelectionService;
        private SymbolPickerState m_SymbolPickerState;

        [SetUp]
        public void SetUp()
        {
            m_SelectionService = MockRepository.GenerateMock<ISelectionService>();
            m_SymbolPickerState = new SymbolPickerState(m_SelectionService);
        }

        [Test]
        public void NoPrimarySelection()
        {
            // ARRANGE
            m_SelectionService
                .Stub(service => service.GetSelectedComponents())
                .Return(null);

            // ACT
            m_SymbolPickerState.Update();

            // ASSERT
            Assert.That(m_SymbolPickerState.IsSupportingComponentLibrary, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingBrowseFile, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingClearSymbol, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingProjectPictures, Is.False);
        }

        [Test]
        public void IsSupportingComponentLibrary()
        {
            // ARRANGE
            m_SelectionService
                .Stub(service => service.GetSelectedComponents())
                .Return(new [] {new IsSupportingComponentLibraryMock()});

            // ACT
            m_SymbolPickerState.Update();

            // ASSERT
            Assert.That(m_SymbolPickerState.IsSupportingComponentLibrary, Is.True);
            Assert.That(m_SymbolPickerState.IsSupportingBrowseFile, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingClearSymbol, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingProjectPictures, Is.False);
        }

        [Test]
        public void IsSupportingBrowseFile()
        {
            // ARRANGE
            m_SelectionService
                .Stub(service => service.GetSelectedComponents())
                .Return(new [] {new IsSupportingBrowseFileMock()});

            // ACT
            m_SymbolPickerState.Update();

            // ASSERT
            Assert.That(m_SymbolPickerState.IsSupportingComponentLibrary, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingBrowseFile, Is.True);
            Assert.That(m_SymbolPickerState.IsSupportingClearSymbol, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingProjectPictures, Is.False);
        }

        [Test]
        public void IsSupportingClearSymbol()
        {
            // ARRANGE
            m_SelectionService
                .Stub(service => service.GetSelectedComponents())
                .Return(new [] {new IsSupportingClearSymbolMock()});

            // ACT
            m_SymbolPickerState.Update();

            // ASSERT
            Assert.That(m_SymbolPickerState.IsSupportingComponentLibrary, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingBrowseFile, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingClearSymbol, Is.True);
            Assert.That(m_SymbolPickerState.IsSupportingProjectPictures, Is.False);
        }

        [Test]
        public void IsSupportingProjectPictures()
        {
            // ARRANGE
            m_SelectionService
                .Stub(service => service.GetSelectedComponents())
                .Return(new [] {new IsSupportingProjectPicturesMock()});

            // ACT
            m_SymbolPickerState.Update();

            // ASSERT
            Assert.That(m_SymbolPickerState.IsSupportingComponentLibrary, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingBrowseFile, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingClearSymbol, Is.False);
            Assert.That(m_SymbolPickerState.IsSupportingProjectPictures, Is.True);
        }

        #region Helper classes

        [SymbolPickerState(IsSupportingComponentLibrary = true)]
        private class IsSupportingComponentLibraryMock
        {
        }

        [SymbolPickerState(IsSupportingBrowseFile = true)]
        private class IsSupportingBrowseFileMock
        {
        }

        [SymbolPickerState(IsSupportingClearSymbol = true)]
        private class IsSupportingClearSymbolMock
        {
        }

        [SymbolPickerState(IsSupportingProjectPictures = true)]
        private class IsSupportingProjectPicturesMock
        {
        }

        #endregion
    }
}