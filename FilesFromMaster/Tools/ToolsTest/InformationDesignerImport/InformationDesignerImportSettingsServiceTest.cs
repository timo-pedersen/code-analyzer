using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.InformationDesignerImport
{
    [TestFixture]
    public class InformationDesignerImportSettingsServiceTest
    {
        [Test]
        public void DataLoaded()
        {
            // ARRANGE
            ILazy<IFileSettingsService> fileSettingsService = MockRepository.GenerateStub<ILazy<IFileSettingsService>>();
            fileSettingsService.Stub(x => x.Value.LoadUserSettings<InformationDesignerImportSettingsService.InformationDesignerImportSettingsData>()).Return(
                new InformationDesignerImportSettingsService.InformationDesignerImportSettingsData { ColorConversion = true });
            InformationDesignerImportSettingsService informationDesignerImportSettingsService = new InformationDesignerImportSettingsService(fileSettingsService);

            // ASSERT
            Assert.IsTrue(informationDesignerImportSettingsService.ColorConversion);
        }
    }
}
