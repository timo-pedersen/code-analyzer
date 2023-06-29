#if !VNEXT_TARGET
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;
using Arg = NSubstitute.Arg;

namespace Neo.ApplicationFramework.Tools.Recipe
{
    [TestFixture]
    public class RecipeExportTest
    {
        private const string RecipeName = "Recipe";
        private const string CsvFileName = "csvExportTest.csv";

        private string m_CsvExportFile;
        private RecipeExport m_RecipeExport;
        private ILazy<IProjectItemFinder> m_ProjectItemFinder;
        private ICsvHelperIde m_CsvHelper;
        private IProjectItem m_ProjectItem;
        private IRecipeItems m_RecipeItems;
        private IRecipe m_Recipe;

        [SetUp]
        public void SetUp()
        {
            m_CsvExportFile = Path.Combine(TestHelper.CurrentDirectory, CsvFileName);

            m_RecipeItems = Substitute.For<IRecipeItems>();
            m_Recipe = Substitute.For<IRecipe>();
            m_ProjectItem = Substitute.For<IProjectItem>();
            m_ProjectItemFinder = Substitute.For<ILazy<IProjectItemFinder>>();
            m_CsvHelper = Substitute.For<ICsvHelperIde>();
            m_RecipeExport = new RecipeExport(m_ProjectItemFinder, m_CsvHelper);
        }

        [Test]
        public void ExportAsCsvSetZeroAsValueForRecipeItemsWithNullValue()
        {
            // Arrange
            var runtimeColumnValues = new List<string>
            {
                null, null, null, null
            };

            var expectedResult = new List<List<string>>
            {
                new List<string>{ Recipe.FieldNameColumnName, "ColumnOne", "ColumnTwo", "ColumnThree", "ColumnFour"},
                new List<string>{ "RecipeTitleOne", "0", "0", "0", "0" },
                new List<string>{ "RecipeTitleTwo", "0", "0", "0", "0" },
                new List<string>{ "RecipeTitleThree", "0", "0", "0", "0" },
                new List<string>{ "RecipeTitleFour", "0", "0", "0", "0" }
            };
            m_RecipeItems.GetEnumerator().Returns(x => GetRecipeItems(runtimeColumnValues));

            var runtimeRecipeColumnNames = new List<string>
            {
                expectedResult[0][1],
                expectedResult[0][2],
                expectedResult[0][3],
                expectedResult[0][4]
            };
            m_RecipeItems.ColumnNames.Returns(runtimeRecipeColumnNames);

            var runtimeRecipeTitles = new List<string>
            {
                expectedResult[1][0],
                expectedResult[2][0],
                expectedResult[3][0],
                expectedResult[4][0]
            };
            m_Recipe.FieldNames.Returns(runtimeRecipeTitles);
            m_Recipe.RecipeItems.Returns(m_RecipeItems);
            m_ProjectItem.ContainedObject.Returns(m_Recipe);
            m_ProjectItemFinder.Value.GetProjectItem(RecipeName).Returns(m_ProjectItem);

            // Act
            m_RecipeExport.ExportAsCsv(RecipeName, m_CsvExportFile, ApplicationConstantsCF.CommaDelimiter, Encoding.UTF8);

            // Assert
            m_CsvHelper.Received().ExportDataToCsv(
                Arg.Is<List<List<string>>>(
                    argumentList => ListValuesAreEqual(argumentList, expectedResult)),
                Arg.Any<Encoding>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>());
        }


        [Test]
        public void ExportAsCsv()
        {
            // Arrange
            var runtimeColumnValues = new List<string>
            {
                "0", "100", "1000", "10000"
            };

            var expectedResult = new List<List<string>>
            {
                new List<string>{ Recipe.FieldNameColumnName, "ColumnOne", "ColumnTwo", "ColumnThree", "ColumnFour"},
                new List<string>{ "RecipeTitleOne", runtimeColumnValues[0], runtimeColumnValues[1], runtimeColumnValues[2], runtimeColumnValues[3] },
                new List<string>{ "RecipeTitleTwo", runtimeColumnValues[0], runtimeColumnValues[1], runtimeColumnValues[2], runtimeColumnValues[3] },
                new List<string>{ "RecipeTitleThree", runtimeColumnValues[0], runtimeColumnValues[1], runtimeColumnValues[2], runtimeColumnValues[3] },
                new List<string>{ "RecipeTitleFour", runtimeColumnValues[0], runtimeColumnValues[1], runtimeColumnValues[2], runtimeColumnValues[3] }
            };
            m_RecipeItems.GetEnumerator().Returns(x => GetRecipeItems(runtimeColumnValues));

            var runtimeRecipeColumnNames = new List<string>
            {
                expectedResult[0][1],
                expectedResult[0][2],
                expectedResult[0][3],
                expectedResult[0][4]
            };
            m_RecipeItems.ColumnNames.Returns(runtimeRecipeColumnNames);

            var runtimeRecipeTitles = new List<string>
            {
                expectedResult[1][0],
                expectedResult[2][0],
                expectedResult[3][0],
                expectedResult[4][0]
            };
            m_Recipe.FieldNames.Returns(runtimeRecipeTitles);
            m_Recipe.RecipeItems.Returns(m_RecipeItems);
            m_ProjectItem.ContainedObject.Returns(m_Recipe);
            m_ProjectItemFinder.Value.GetProjectItem(RecipeName).Returns(m_ProjectItem);

            // Act
            m_RecipeExport.ExportAsCsv(RecipeName, m_CsvExportFile, ApplicationConstantsCF.CommaDelimiter, Encoding.UTF8);

            // Assert
            m_CsvHelper.Received().ExportDataToCsv(
                Arg.Is<List<List<string>>>(
                    argumentList => ListValuesAreEqual(argumentList, expectedResult)),
                Arg.Any<Encoding>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>());
        }

        private static bool ListValuesAreEqual(IReadOnlyList<List<string>> listOne, IReadOnlyList<List<string>> listTwo)
        {
            for (var i = 0; i < listTwo.Count; i++)
            {
                for (var j = 0; j < listTwo[i].Count; j++)
                {
                    if (listTwo[i][j] != listOne[i][j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static IEnumerator<IRecipeItem> GetRecipeItems(IEnumerable<string> columnValues)
        {
            foreach (var columnValue in columnValues)
            {
                var recipeItem = Substitute.For<IRecipeItem>();
                recipeItem.GetRuntimeData(Arg.Any<string>()).Returns(columnValue);
                yield return recipeItem;
            }
        }
    }
}
#endif
