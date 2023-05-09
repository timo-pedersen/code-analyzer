using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.UpdateManager.Dialogs;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.UpdateManager
{
    public class UpdateSoftwareDialogViewModelTest
    {
        private UpdateSoftwareDialogViewModel m_UpdateSoftwareDialogViewModel;

        private IIDEOptionsService m_IdeOptionsService;
        private IUpdateService m_UpdateService;

        [SetUp]
        public void SetUp()
        {
            m_IdeOptionsService = Substitute.For<IIDEOptionsService>();
            var lazyIdeOptionsService = Substitute.For<ILazy<IIDEOptionsService>>();
            lazyIdeOptionsService.Value.Returns(m_IdeOptionsService);

            m_UpdateService = Substitute.For<IUpdateService>();
            var lazyUpdateService = Substitute.For<ILazy<IUpdateService>>();
            lazyUpdateService.Value.Returns(m_UpdateService);

            m_UpdateSoftwareDialogViewModel = new UpdateSoftwareDialogViewModel(lazyUpdateService, lazyIdeOptionsService);
        }

        [Test]
        public void ShouldRemindAboutSoftware_SetsUpdateSoftwareOption()
        {
            // Arrange
            m_IdeOptionsService.GetOption<UpdateSoftwareOptions>().Returns(new UpdateSoftwareOptions());

            // Act
            m_UpdateSoftwareDialogViewModel.ShouldRemindAboutSoftware = true;
            
            // Assert
            m_IdeOptionsService.Received().GetOption<UpdateSoftwareOptions>().ShowUpdateInfoOnStartup = true;
        }
    }
}
