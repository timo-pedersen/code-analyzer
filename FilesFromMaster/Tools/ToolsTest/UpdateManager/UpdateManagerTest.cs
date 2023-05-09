using System;
using System.IO;
using System.Threading.Tasks;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Brand;
using Neo.ApplicationFramework.Tools.UpdateManager.Dialogs;
using NUnit.Framework;
using Rhino.Mocks;
using StoreSoftwareManagerLibrary;

namespace Neo.ApplicationFramework.Tools.UpdateManager
{
    public class UpdateManagerTest
    {

        private const string Product = "iX Developer";
        private const string Version = "2.40";

        private UpdateManager m_UpdateManager;

        private IIDEOptionsService m_IdeOptionsService;
        private IBrandServiceIde m_BrandServiceIde;
        private IStoreSoftwareManager m_StoreSoftwareManager;

        [SetUp]
        public void Setup()
        {
            m_BrandServiceIde = MockRepository.GenerateStub<IBrandServiceIde>();
            var lazyBrandServiceIde = MockRepository.GenerateStub<ILazy<IBrandServiceIde>>();
            lazyBrandServiceIde.Stub(x => x.Value)
                .Return(m_BrandServiceIde);

            m_IdeOptionsService = MockRepository.GenerateStub<IIDEOptionsService>();
            var lazyIdeOptionsService = MockRepository.GenerateStub<ILazy<IIDEOptionsService>>();
            lazyIdeOptionsService.Stub(x => x.Value)
                .Return(m_IdeOptionsService);
            
            m_StoreSoftwareManager = MockRepository.GenerateMock<IStoreSoftwareManager>();

            m_UpdateManager = new UpdateManager(
                lazyBrandServiceIde,
                lazyIdeOptionsService,
                m_StoreSoftwareManager);
        }

        [Test]
        public async Task NewerVersionExistsAsync_BrandIsiXAndStoreHasNewerVersion_ReturnsTrue()
        {
            // Arrange
            BrandToolHelper.Instance = new BrandToolHelper(BrandToolHelper.PanelBrandId8);
            m_StoreSoftwareManager.Stub(x => x.IsLatestSoftwareVersionInstalledAsync(Arg<string>.Is.Anything, Arg<Version>.Is.Anything))
                .Return(Task.FromResult(false));

            // Act
            bool newerVersionExists = await m_UpdateManager.NewerVersionExistsAsync();

            // Assert
            Assert.True(newerVersionExists);
        }

        [Test]
        public async Task NewerVersionExistsAsync_BrandIsNotiX_ReturnsFalse()
        {
            // Arrange
            BrandToolHelper.Instance = new BrandToolHelper(BrandToolHelper.PanelBrandId4);

            // Act
            bool newerVersionExists = await m_UpdateManager.NewerVersionExistsAsync();

            // Assert
            Assert.False(newerVersionExists);
        }

        [Test]
        public async Task CheckStoreSoftwareManagerException_NewerVersionExistsAsync()
        {
            // arrange
            IStoreSoftwareManager storeSoftwareManager = new StoreSoftwareManager();
            FileLoadException caughtException = null;
            m_BrandServiceIde.Stub(x => x.ProductName).Return(Product);
            m_BrandServiceIde.Stub(x => x.Version).Return(Version);

            // act
            await storeSoftwareManager.IsLatestSoftwareVersionInstalledAsync(m_BrandServiceIde.ProductName, new Version(m_BrandServiceIde.Version)).ContinueWith(
                t =>
                {
                    if (t.Exception != null && t.Exception.InnerException!.GetType() == typeof(FileLoadException))
                        caughtException = (FileLoadException)t.Exception.InnerException;
                });

            // assert
            Assert.That(caughtException, Is.Null, "Please check the dependentAssembly in App.Config");
        }
    }
}
