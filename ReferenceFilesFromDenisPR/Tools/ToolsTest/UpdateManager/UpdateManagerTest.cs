using System;
using System.Threading.Tasks;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Brand;
using Neo.ApplicationFramework.Tools.UpdateManager.Dialogs;
using NUnit.Framework;
using NSubstitute;
using StoreSoftwareManagerLibrary;

namespace Neo.ApplicationFramework.Tools.UpdateManager
{
    public class UpdateManagerTest
    {
        private UpdateManager m_UpdateManager;

        private IIDEOptionsService m_IdeOptionsService;
        private IBrandServiceIde m_BrandServiceIde;
        private IStoreSoftwareManager m_StoreSoftwareManager;

        [SetUp]
        public void Setup()
        {
            m_BrandServiceIde = Substitute.For<IBrandServiceIde>();
            var lazyBrandServiceIde = Substitute.For<ILazy<IBrandServiceIde>>();
            lazyBrandServiceIde.Value.Returns(m_BrandServiceIde);

            m_IdeOptionsService = Substitute.For<IIDEOptionsService>();
            var lazyIdeOptionsService = Substitute.For<ILazy<IIDEOptionsService>>();
            lazyIdeOptionsService.Value.Returns(m_IdeOptionsService);
            
            m_StoreSoftwareManager = Substitute.For<IStoreSoftwareManager>();

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
            m_StoreSoftwareManager.IsLatestSoftwareVersionInstalledAsync(Arg.Any<string>(), Arg.Any<Version>())
                .Returns(Task.FromResult(false));

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
    }
}
