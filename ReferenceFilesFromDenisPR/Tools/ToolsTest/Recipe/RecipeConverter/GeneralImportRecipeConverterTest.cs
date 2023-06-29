#if !VNEXT_TARGET
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.InformationDesignerImport.ConverterManager;
using Neo.ApplicationFramework.Tools.Recipe.RecipeConverters.Models;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Recipe.RecipeConverters
{
    [TestFixture]
    public class GeneralImportRecipeConverterTest
    {
        private GeneralImportRecipeConverter m_GeneralImportRecipeConverter;
        private List<IImportDataItemLight> m_RepositoryDataItems;

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<IConverterApiService>();
            TestHelper.CreateAndAddServiceStub<IProjectManager>();

            SetupRepositoryDataItems();

            IConverterManager converterManager = Substitute.For<IConverterManager>();
            TestHelper.AddService(converterManager);

            var controllerConverterManager = new ControllerConverterManager();
            converterManager.ControllerConverterManager.Returns(controllerConverterManager);
            converterManager.ControllerConverterManager.DataItems = m_RepositoryDataItems;

            IConversionHelperService conversionHelper = Substitute.For<IConversionHelperService>();
            IConverterApiService converterApiService = Substitute.For<IConverterApiService>();

            m_GeneralImportRecipeConverter = new GeneralImportRecipeConverter(
                new RecipeConverterResult(),
                converterManager,
                conversionHelper,
                converterApiService);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        // Depending on what ID/ED control was exported in recipe, recipeFileData have different content, esp regarding data type
        // which defaults to DT_INTEGER2 where it should have been DT_BIT.
        // This test makes sure both kinds can be fetched from the repository.
        [TestCase(true, "A")]
        [TestCase(false, "A")]
        [TestCase(true, "B")]
        [TestCase(false, "B")]
        public void GetExistingTagsFromRepositoryForDigitalTagsReturnsCorrectRepositoryTag(bool useDefaultDataType, string address)
        {
            // Arrange
            RecipeFileData recipeFileData = CreateRecipeFileData(useDefaultDataType, address);

            // Act
            var dataItems = m_GeneralImportRecipeConverter.GetExistingTagsFromRepository(recipeFileData);

            // Assert
            Assert.AreEqual(1, dataItems.Length);
            Assert.AreEqual(address, dataItems[0].Address);
        }

        [TestCase(true, "NotInRepository")]
        public void GetExistingTagsFromRepositoryForDigitalWrongAddressReturnsEmptyList(bool useDefaultDataType, string address)
        {
            // Arrange
            RecipeFileData recipeFileData = CreateRecipeFileData(useDefaultDataType, address);

            // Act
            var dataItems = m_GeneralImportRecipeConverter.GetExistingTagsFromRepository(recipeFileData);

            // Assert
            Assert.AreEqual(0, dataItems.Length);
        }

        [TestCase("A")]
        [TestCase("B")]
        public void GetExistingTagsFromRepositoryWrongRecipeDataReturnsEmptyList(string address)
        {
            // Arrange
            RecipeFileData recipeFileData = CreateRecipeFileDataWithWrongData(BEDATATYPE.DT_REAL8, address, 1);

            // Act
            var dataItems = m_GeneralImportRecipeConverter.GetExistingTagsFromRepository(recipeFileData);

            // Assert
            Assert.AreEqual(0, dataItems.Length);
        }

        [TestCase("M")]
        [TestCase("N")]
        [TestCase("O")]
        [TestCase("P")]
        public void GetExistingTagsWithNonDefaultDataReturnsEmptyList(string address)
        {
            // Arrange
            RecipeFileData recipeFileData = CreateRecipeFileData(true, address);

            // Act
            var dataItems = m_GeneralImportRecipeConverter.GetExistingTagsFromRepository(recipeFileData);

            // Assert
            Assert.AreEqual(0, dataItems.Length);
        }

        #region helpers

        RecipeFileData CreateRecipeFileData(bool defaultDataType, string address)
        {
            return new RecipeFileData
            {
                BeDataType = defaultDataType ? BEDATATYPE.DT_INTEGER2 : BEDATATYPE.DT_BIT,
                OriginalDataType = defaultDataType ? "" : "BI",
                OriginalAddress = address,
                TagAddress = address,
                ControllerNumber = 1,
            };
        }

        RecipeFileData CreateRecipeFileDataWithWrongData(BEDATATYPE dataType, string address, int controllerNumber)
        {
            return new RecipeFileData
            {
                BeDataType = dataType,
                OriginalDataType = "",
                OriginalAddress = address,
                TagAddress = address,
                ControllerNumber = controllerNumber,
            };
        }

        void SetupRepositoryDataItems()
        {
            m_RepositoryDataItems = new List<IImportDataItemLight>
            {
                CreateMockDataItem("A"),
                CreateMockDataItem("B"),
                CreateMockDataItem("D"),
                CreateMockDataItemWithGainOffsetExpression("M", true, false, false, false),
                CreateMockDataItemWithGainOffsetExpression("N", false, true, false, false),
                CreateMockDataItemWithGainOffsetExpression("O", false, false, true, false),
                CreateMockDataItemWithGainOffsetExpression("P", false, false, false, true),
            };
        }

        IImportDataItemLight CreateMockDataItem(string address)
        {
            IImportDataItemLight dataItem = Substitute.For<IImportDataItemLight>();
            dataItem.Address = address;
            dataItem.ControllerNumber = 1;
            dataItem.DataType = BEDATATYPE.DT_INTEGER2;
            dataItem.Offset = 0;
            dataItem.Gain = 1;
            dataItem.ReadExpression = null;
            dataItem.WriteExpression = null;

            return dataItem;
        }

        IImportDataItemLight CreateMockDataItemWithGainOffsetExpression(string address, bool gain, bool offset, bool readExpression, bool writeExpression)
        {
            IImportDataItemLight dataItem = Substitute.For<IImportDataItemLight>();
            dataItem.Address = address;
            dataItem.ControllerNumber = 1;
            dataItem.DataType = BEDATATYPE.DT_INTEGER2;
            dataItem.Offset = offset ? 0.1 : 0;
            dataItem.Gain = gain ? 1.1 : 1;
            dataItem.ReadExpression = readExpression ? "Something" : null;
            dataItem.WriteExpression = writeExpression ? "Something" : null;

            return dataItem;
        }

        #endregion
    }
}
#endif
