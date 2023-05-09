using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Recipe.RecipeConverters.RecipeArrays;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Recipe.RecipeArrays
{
    [TestFixture]
    public class RecipeArrayConverterHelperBaseTest
    {
        private TestRecipeArrayConverterHelper m_RecipeArrayConverterHelper;
        private IDataSourceContainer m_DataSourceContainer;

        [SetUp]
        public void SetUp()
        {
            m_DataSourceContainer = MockRepository.GenerateMock<IDataSourceContainer>();

            m_RecipeArrayConverterHelper = new TestRecipeArrayConverterHelper();
        }

        [TestCase("POU1.CS_ARTI", "POU1.CS_ARTI", "POU1.CS_ARTI[1]", "POU1.CS_ARTI[0]")]
        [TestCase("V177777", "V177777", "", "V177777")]
        [TestCase("4FFFF", "4FFFF", "", "4FFFF")]
        [TestCase("4FFFE", "4FFFF", "not used", "4FFFE")]
        [TestCase("X1", "X2", "not used", "X1")]
        public void UpdateBaseTagAddressWithBaseArrayItem(string address, string nextItemId, string nextItemId2, string expectedAddress)
        {
            // ARRANGE
            ArrayItemRecipeFileData baseArrayItem = CreateArrayItemRecipeFileData(address, 0);
            DataItem dataItem = CreateDataItem(address);

            m_DataSourceContainer.Stub(x => x.GetNextItemID(dataItem, 1)).Return(nextItemId).Repeat.Once();
            m_DataSourceContainer.Stub(x => x.GetNextItemID(dataItem, 1)).Return(nextItemId2).Repeat.Once();

            // ACT
            (bool result, string _) = m_RecipeArrayConverterHelper.UpdateRecipeArrayTagAddress(baseArrayItem, dataItem, m_DataSourceContainer);

            // ASSERT
            Assert.IsTrue(result);
            Assert.AreEqual(expectedAddress, dataItem.ItemID);
        }

        [TestCase("POU1.CS_ARTI[0]", "POU1.CS_ARTI[1]", "POU1.CS_ARTI[0]", "POU1.CS_ARTI[1]", true)]
        [TestCase("V177777", "V177777", "V177777", "", false)]
        [TestCase("4FFFF", "4FFFF", "4FFFF", "", false)]
        [TestCase("4FFFE", "4FFFF", "4FFFE", "4FFFF", true)]
        [TestCase("40000", "40056", "40055", "40056", true)]
        [TestCase("40000", "40001", "40000", "40001", true)]
        public void UpdateTagAddressWithSecondArrayItem(string baseItemAddress, string currentItemId, string previousItemId, string expectedAddress, bool expectedResult)
        {
            // ARRANGE
            ArrayItemRecipeFileData arrayItem = CreateArrayItemRecipeFileData(baseItemAddress, 1);
            DataItem dataItem = CreateDataItem(baseItemAddress);

            m_DataSourceContainer.Stub(x => x.GetNextItemID(dataItem, arrayItem.ArrayIndex)).Return(currentItemId).Repeat.Once();
            m_DataSourceContainer.Stub(x => x.GetNextItemID(dataItem, arrayItem.ArrayIndex - 1)).Return(previousItemId).Repeat.Once();

            // ACT
            (bool result, string _) = m_RecipeArrayConverterHelper.UpdateRecipeArrayTagAddress(arrayItem, dataItem, m_DataSourceContainer);

            // ASSERT
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedAddress, dataItem.ItemID);
        }

        private ArrayItemRecipeFileData CreateArrayItemRecipeFileData(string address, int index)
        {
            return new ArrayItemRecipeFileData
            {
                ArrayIndex = index,
                OriginalAddress = address
            };
        }

        private static DataItem CreateDataItem(string address)
        {
            return new DataItem
            {
                ItemID = address
            };
        }

        private class TestRecipeArrayConverterHelper : RecipeArrayConverterHelperBase
        {
            public override void AddRecipeFileDataArray(IEnumerable<ArrayItemRecipeFileData> recipeFileData)
            {
            }

            public override bool IsArrayItem(object arrayItem)
            {
                return true;
            }

            public override (bool isSuccess, string error) UpdateRecipeArrayTagAddress(object arrayItem, IDataItem dataItem, IDataSourceContainer dataSourceContainer)
            {
                var arrayItemRecipeFileData = (ArrayItemRecipeFileData)arrayItem;

                return UpdateTagAddress(dataItem, dataItem, dataSourceContainer, arrayItemRecipeFileData);
            }
        }
    }
}
