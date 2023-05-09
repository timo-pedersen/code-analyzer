using System.ComponentModel.Design;
using System.IO.Packaging;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Button
{
    [TestFixture]
    public class ButtonPictureViewModelTest
    {
        private IDataCommandFacade m_DataCommandFacade;

        [SetUp]
        public void SetUp()
        {
            var s = PackUriHelper.UriSchemePack;
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
            TestHelper.CreateAndAddServiceMock<ISelectionService>();

            m_DataCommandFacade = MockRepository.GenerateMock<IDataCommandFacade>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void ChangePictureCommand()
        {
            // Act
            var viewModel = new ButtonPictureViewModel(m_DataCommandFacade);
            viewModel.ChangePictureCommand.Execute(null);

            // Assert
            Assert.That(viewModel.IsButtonChecked, Is.True);
            Assert.That(viewModel.IsSymbolDropDownOpen, Is.True);
        }

        [Test]
        public void IsSymbolDropDownOpenVerifyIsButtonChecked()
        {
            // Act
            var viewModel = new ButtonPictureViewModel(m_DataCommandFacade);
            viewModel.IsSymbolDropDownOpen = true;

            // Assert
            Assert.That(viewModel.IsButtonChecked, Is.True);
        }
    }
}