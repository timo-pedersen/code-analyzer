using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.UpdateManager.Dialogs;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_IdeOptionsService = MockRepository.GenerateStub<IIDEOptionsService>();
            var lazyIdeOptionsService = MockRepository.GenerateStub<ILazy<IIDEOptionsService>>();
            lazyIdeOptionsService.Stub(x => x.Value)
                .Return(m_IdeOptionsService);

            m_UpdateService = MockRepository.GenerateStub<IUpdateService>();
            var lazyUpdateService = MockRepository.GenerateStub<ILazy<IUpdateService>>();
            lazyUpdateService.Stub(x => x.Value)
                .Return(m_UpdateService);

            m_UpdateSoftwareDialogViewModel = new UpdateSoftwareDialogViewModel(lazyUpdateService, lazyIdeOptionsService);
        }

        [Test]
        public void ShouldRemindAboutSoftware_SetsUpdateSoftwareOption()
        {
            // Arrange
            m_IdeOptionsService.Stub(x => x.GetOption<UpdateSoftwareOptions>()).Return(new UpdateSoftwareOptions());

            // Act
            m_UpdateSoftwareDialogViewModel.ShouldRemindAboutSoftware = true;
            
            // Assert
            m_IdeOptionsService.AssertWasCalled(x => x.GetOption<UpdateSoftwareOptions>().ShowUpdateInfoOnStartup);
        }
    }
}
