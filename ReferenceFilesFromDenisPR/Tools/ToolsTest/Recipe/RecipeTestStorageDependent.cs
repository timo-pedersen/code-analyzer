using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.Storage.Common;
#if VNEXT_TARGET
using Neo.ApplicationFramework.Storage.Providers.SQLiteDatabase;
#else
using Neo.ApplicationFramework.Storage.Providers.SqlCeDatabase;
#endif
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.Storage;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Recipe
{
    [TestFixture]
    public class RecipeTestStorageDependent : RecipeTest
    {
        private IStorageCacheService m_StorageCacheService;
        private IGlobalReferenceService m_GlobalReferenceService;
        private IOPCClientStatusService m_OPCClientStatusService;
        private readonly Func<IDataReader, IEnumerable<RecipeField>> m_RecipeFieldFactoryFunc = result => RecipeField.Create(result, TableName);

        private IStorage Storage { get; set; }


        [SetUp]
        public void SetUp()
        {
            var scs = new StorageClientToolCF() as IStorageClientService;
            TestHelper.AddService(scs);
            var storageSettings = new LocallyHostedProjectStorageProviderSettings();

            string path = Path.Combine(Path.GetTempPath(), "StorageTest");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);

            TestHelper.UseTestWindowThreadHelper = true;

            storageSettings.RootDirectory = path;
            const string sqlCe = "SQL CE Database";
            storageSettings.DisplayName = sqlCe;
            IStorageService storageService = new StorageServiceCF();
#if VNEXT_TARGET
            var provider = new SQLiteDatabaseProviderCF();
#else
            var provider = new SqlCeDatabaseProviderCF();
#endif
            storageService.RegisterStorageProvider(provider);

            TestHelper.AddService(storageService);

            m_StorageCacheService = new StorageCacheService(storageSettings);
            TestHelper.AddService(m_StorageCacheService);
            Storage = m_StorageCacheService.GetStorage("Database");

            TestHelper.CreateAndAddServiceStub<INativeAPI>();
            TestHelper.CreateAndAddServiceStub<ISplashService>();

            IToolManager toolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(false);

            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();

            IBatchService batchService = TestHelper.AddServiceStub<IBatchService>();
            batchService.GetControllersInUse(Arg.Any<IEnumerable<IConnectableTag>>()).Returns(Enumerable.Empty<IDataSourceContainer>());

            if (Storage.Exists())
            {
                Storage.Close();
                Storage.Delete();
                Storage.Open();
            }

            var systemTagServiceCF = TestHelper.CreateAndAddServiceStub<ISystemTagServiceCF>();
            m_OPCClientStatusService = TestHelper.CreateAndAddServiceStub<IOPCClientStatusService>();

            m_Recipe = new Recipe(m_StorageCacheService.ToILazy(), systemTagServiceCF.ToILazy(), m_OPCClientStatusService.ToILazy())
            {
                TableName = TableName
            };
        }

        [TearDown]
        public void TearDown()
        {
            Storage.Close();

            m_StorageCacheService.DisposeDisposable();
            m_StorageCacheService = null;
            ServiceContainerCF.Instance.RemoveService(typeof(IStorageCacheService));

            if (m_Recipe != null)
            {
                ((IDisposable)m_Recipe).Dispose();
                m_Recipe = null;
            }
        }

        private static string RecipeFieldName => Recipe.FieldNameColumnName;

        [Test]
        public void SaveRecipeWithOneItem()
        {
            RecipeWithOneItem();

            IBasicTag globalDataItem = new GlobalDataItem
            {
                DataType = BEDATATYPE.DT_INTEGER2
            };
            m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Any<string>()).Returns(globalDataItem);

            m_Recipe.RecipeItems[0].DataConnection = "Tags.Tag1";

            globalDataItem = m_Recipe.RecipeItems[0].DataItem;
            globalDataItem.Value = 100;

            Assert.AreEqual(100, m_Recipe.RecipeItems[0].Value);

            m_Recipe.SaveRecipe(RecipeFieldName);

            IEnumerable<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName).ToArray();

            Assert.AreEqual(1, fields.Count(), "Only one field should exist in the db");

            RecipeField fieldInList = fields.First();

            Assert.AreEqual(RecipeFieldName, fieldInList.FieldName);
            Assert.AreEqual(1, fieldInList.FieldValues.Count);
            Assert.AreEqual(100, fieldInList.FieldValues[0].Value);

        }

        [Test]
        public void RecipeNotSavedIfCommErrorExists()
        {
            // ARRANGE
            RecipeWithOneItem();
            IBasicTag globalDataItem = new GlobalDataItem
            {
                DataType = BEDATATYPE.DT_INTEGER2
            };
            m_GlobalReferenceService.GetObject<IBasicTag>("Tags.Tag1").Returns(globalDataItem);

            m_Recipe.RecipeItems[0].DataConnection = "Tags.Tag1";

            globalDataItem = m_Recipe.RecipeItems[0].DataItem;
            globalDataItem.Value = 100;
            m_OPCClientStatusService.NbrOfActiveCommErrors.Returns(1);

            // ACT
            m_Recipe.SaveRecipe(RecipeFieldName);

            // ASSERT
            Assert.Throws<StorageReaderException>(() => Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName));
            
        }

        [Test]
        public void LoadRecipeWithOneItem()
        {
            SaveRecipeWithOneItem();

            m_Recipe.LoadRecipe(RecipeFieldName);

            Assert.AreEqual(100, m_Recipe.RecipeItems[0].Value);
            IGlobalDataItem globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[0].DataItem;
            globalDataItem.Value = 0;
            Assert.AreEqual(0, m_Recipe.RecipeItems[0].Value);

            m_Recipe.LoadRecipe(RecipeFieldName);
            Assert.AreEqual(100, m_Recipe.RecipeItems[0].Value);
        }

        [Test]
        public void RecipeNotLoadedIfCommErrorExists()
        {
            // ARRANGE
            SaveRecipeWithOneItem();
            IGlobalDataItem globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[0].DataItem;
            globalDataItem.Value = 0;
            m_OPCClientStatusService.NbrOfActiveCommErrors.Returns(1);

            // ACT
            m_Recipe.LoadRecipe(RecipeFieldName);

            // ASSERT
            Assert.AreNotEqual(100, m_Recipe.RecipeItems[0].Value);
        }

        [Test]
        public void SaveRecipeWithManyItems()
        {
            RecipeWithManyItems();

            for (int index = 0; index < NoOfManyItems; index++)
            {
                IGlobalDataItem globalDataItem = new GlobalDataItem();
                globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;

                string dataItemFullName = "Tags.Tag" + index;
                m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Any<string>()).Returns(globalDataItem);

                m_Recipe.RecipeItems[index].DataConnection = dataItemFullName;
                globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[index].DataItem;

                globalDataItem.Value = index;
            }

            m_Recipe.SaveRecipe(RecipeFieldName);

            IEnumerable<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName);

            Assert.AreEqual(1, fields.Count(), "Only one field should exist in the db");

            RecipeField fieldInList = fields.First();

            Assert.AreEqual(RecipeFieldName, fieldInList.FieldName);
            Assert.AreEqual(NoOfManyItems, fieldInList.FieldValues.Count);
            for (int index = 0; index < NoOfManyItems; index++)
            {
                Assert.AreEqual(index, fieldInList.FieldValues[index].Value);
            }
        }

        [Test]
        public void LoadRecipeWithManyItems()
        {
            SaveRecipeWithManyItems();

            m_Recipe.LoadRecipe(RecipeFieldName);

            for (int index = 0; index < NoOfManyItems; index++)
            {
                Assert.AreEqual(index, m_Recipe.RecipeItems[index].Value);
            }

            for (int index = 0; index < NoOfManyItems; index++)
            {
                var globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[index].DataItem;
                globalDataItem.Value = 0;
                Assert.AreEqual(0, m_Recipe.RecipeItems[index].Value);
            }

            m_Recipe.LoadRecipe(RecipeFieldName);

            for (int index = 0; index < NoOfManyItems; index++)
            {
                Assert.AreEqual(index, m_Recipe.RecipeItems[index].Value);
            }
        }

        [Test]
        public void OverwriteRecipeWithOneItem()
        {
            SaveRecipeWithOneItem();

            var globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[0].DataItem;
            globalDataItem.Value = 923;

            m_Recipe.SaveRecipe(RecipeFieldName);

            IEnumerable<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName);

            Assert.AreEqual(1, fields.Count(), "Only one field should exist in the db");

            RecipeField fieldInList = fields.First();

            Assert.AreEqual(RecipeFieldName, fieldInList.FieldName);
            Assert.AreEqual(1, fieldInList.FieldValues.Count);
            Assert.AreEqual(923, fieldInList.FieldValues[0].Value);
        }

        [Test]
        public void OverwriteRecipeWithManyItems()
        {
            SaveRecipeWithManyItems();

            for (int index = 0; index < NoOfManyItems; index++)
            {
                var globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[index].DataItem;
                globalDataItem.Value = index + NoOfManyItems;
            }

            m_Recipe.SaveRecipe(RecipeFieldName);

            IEnumerable<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName);

            Assert.AreEqual(1, fields.Count(), "Only one field should exist in the db");

            RecipeField fieldInList = fields.First();

            Assert.AreEqual(RecipeFieldName, fieldInList.FieldName);
            Assert.AreEqual(NoOfManyItems, fieldInList.FieldValues.Count);
            for (int index = 0; index < NoOfManyItems; index++)
            {
                Assert.AreEqual(NoOfManyItems + index, fieldInList.FieldValues[index].Value);
            }
        }

        [Test]
        public void SaveManyFieldsOneItem()
        {
            RecipeWithOneItem();

            m_Recipe.RecipeItems[0].DataConnection = "Tags.Tag1";

            m_GlobalReferenceService
                .GetObject<IBasicTag>("Tags.Tag1")
                .Returns(new GlobalDataItem
                {
                    DataType = BEDATATYPE.DT_INTEGER2
                });

            // Saving NoOfManyItems recipe fields
            for (int index = 0; index < NoOfManyItems; index++)
            {
                IGlobalDataItem globalDataItem = (IGlobalDataItem)m_Recipe.RecipeItems[0].DataItem;
                globalDataItem.Value = index;
                m_Recipe.SaveRecipe(RecipeFieldName + index);
            }

            List<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName).ToList();

            Assert.AreEqual(NoOfManyItems, fields.Count, "{0} fields should exist in the db", NoOfManyItems);

            bool[] allIncludedVector = new bool[NoOfManyItems];

            for (int index = 0; index < NoOfManyItems; index++)
            {
                int id = int.Parse(fields[index].FieldName.Substring(RecipeFieldName.Length));

                Assert.IsFalse(allIncludedVector[id]);
                allIncludedVector[id] = true;

                Assert.AreEqual(id, fields[index].FieldValues[0].Value);
            }
        }

        [Test]
        public void SaveManyFieldsManyItems()
        {
            RecipeWithManyItems();

            // Saving 5 recipe fields
            for (int index = 0; index < 5; index++)
            {
                for (int index2 = 0; index2 < NoOfManyItems; index2++)
                {
                    GlobalDataItem globalDataItem = new GlobalDataItem();
                    globalDataItem.DataType = BEDATATYPE.DT_INTEGER2;
                    globalDataItem.Value = index2;

                    string dataItemFullName = string.Format("Tags.Tag{0}_{1}", index, index2);
                    m_GlobalReferenceService.GetObject<IBasicTag>(dataItemFullName).Returns(globalDataItem);

                    m_Recipe.RecipeItems[index2].DataConnection = dataItemFullName;
                }

                m_Recipe.SaveRecipe(RecipeFieldName + index);
            }

            List<RecipeField> fields = Storage.Query.Select(m_RecipeFieldFactoryFunc, TableName).ToList();

            Assert.AreEqual(5, fields.Count, "5 fields should exist in the db");

            bool[] allIncludedVector = new bool[5];

            for (int index = 0; index < 5; index++)
            {
                int id = int.Parse(fields[index].FieldName.Substring(RecipeFieldName.Length));

                Assert.IsFalse(allIncludedVector[id]);
                allIncludedVector[id] = true;

                for (int index2 = 0; index2 < NoOfManyItems; index2++)
                {
                    Assert.AreEqual(index2, fields[index].FieldValues[index2].Value);
                }
            }
        }

        [Test]
        public void DeleteRecipeWithOneItem()
        {
            SaveRecipeWithOneItem();

            m_Recipe.LoadRecipe(RecipeFieldName);
            Assert.AreEqual(1, Storage.Query.Count(m_Recipe.TableName));

            m_Recipe.DeleteRecipe(RecipeFieldName);

            Assert.AreEqual(0, Storage.Query.Count(m_Recipe.TableName));
        }

    }
}
