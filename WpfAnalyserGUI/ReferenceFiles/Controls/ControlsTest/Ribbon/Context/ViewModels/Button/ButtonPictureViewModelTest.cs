#if !VNEXT_TARGET
using System.ComponentModel.Design;
using System.IO.Packaging;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
            TestHelper.CreateAndAddServiceStub<ISelectionService>();

            m_DataCommandFacade = Substitute.For<IDataCommandFacade>();
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
#endif
