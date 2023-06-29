using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.InformationDesignerImport
{
    [TestFixture]
    public class InformationDesignerImportSettingsServiceTest
    {
        [Test]
        public void DataLoaded()
        {
            // ARRANGE
            ILazy<IFileSettingsService> fileSettingsService = Substitute.For<ILazy<IFileSettingsService>>();
            fileSettingsService.Value.LoadUserSettings<InformationDesignerImportSettingsService.InformationDesignerImportSettingsData>().Returns(
                new InformationDesignerImportSettingsService.InformationDesignerImportSettingsData { ColorConversion = true });
            InformationDesignerImportSettingsService informationDesignerImportSettingsService = new InformationDesignerImportSettingsService(fileSettingsService);

            // ASSERT
            Assert.IsTrue(informationDesignerImportSettingsService.ColorConversion);
        }
    }
}
