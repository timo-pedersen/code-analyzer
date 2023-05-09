using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ImportExport
{
    [TestFixture]
    public class ListManagerTest
    {
        private ListManager<DataItemImportInfo> m_ListManager;
        private IOpcClientServiceIde m_OpcClientService;
        private ExtendedBindingList<IDataSourceContainer> m_Controllers;

        [SetUp]
        public void SetUp()
        {
            m_ListManager = Substitute.For<ListManager<DataItemImportInfo>>(MergeAction.Merge, MergeAction.All);
            m_ListManager.m_GetFullNameFromProperties = GetName;
            m_ListManager.m_GetShortNameFromProperties = GetName;
            m_ListManager.m_SetShortNameInProperties = SetName;
            m_ListManager.m_CreateNewObject = CreateNewObject;
            m_ListManager.TypeToImport = typeof(DataItemImportInfo);
            m_ListManager.m_ValidateProperties = ValidateImportRulesOnAddress;

            m_Controllers = new ExtendedBindingList<IDataSourceContainer>();
            m_OpcClientService = TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_OpcClientService.Controllers.Returns(m_Controllers);
            AddControllers(2);
        }

        [TearDown]
        public void TearDown()
        {
            m_Controllers.Clear();
            TestHelper.ClearServices();
        }

        [Test]
        public void ImportItemsWithChangeNameAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            m_ListManager.GetMergeAction(Arg.Is(imports[0]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag4";
                    return MergeAction.ChangeName; 
                });
            m_ListManager.GetMergeAction(Arg.Is(imports[1]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag5";
                    return MergeAction.ChangeName;
                });
            m_ListManager.GetMergeAction(Arg.Is(imports[2]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag6";
                    return MergeAction.ChangeName;
                });

            m_ListManager.MergeLists(existingItems, imports);

            Assert.AreEqual(6, m_ListManager.MergedItems.Count);
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
            AssertMergeAction(MergeAction.ChangeName, "Tag5");
            AssertMergeAction(MergeAction.ChangeName, "Tag6");
        }

        [Test]
        public void ImportItemsWithOverWriteAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
                .Returns(MergeAction.OverWrite);

            m_ListManager.MergeLists(existingItems, imports);

            Assert.AreEqual(3, m_ListManager.MergedItems.Count);
            AssertMergeAction(MergeAction.OverWrite, "Tag1");
            AssertMergeAction(MergeAction.OverWrite, "Tag2");
            AssertMergeAction(MergeAction.OverWrite, "Tag3");
        }

        [Test]
        public void ImportItemsWithSkipAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
                .Returns(MergeAction.Skip);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
        }

        [Test]
        public void ImportItemsWithMergeAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3", "Tag4");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag3", "Tag4");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
                .Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Merge, "Tag3");
            AssertMergeAction(MergeAction.Merge, "Tag4");
        }

        [Test]
        public void ImportEmptyItemWithMergeActionAndEmptyExistingData()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1");

            AddPropertyToItemInImportedItemsList(imports, "Tag1", "AccessRight_1", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "AccessRight_2", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "Address_1", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "Address_2", "");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
               .Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");

            //Assert Tag1
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag1", "AccessRights", new List<AccessRights>());
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag1", "Addresses", new List<string>());
        }

        [Test]
        public void ImportDataItemWithMergeActionAndEmptyExistingData()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1");

            //Existing Tag setup
            SetPropertyOnExistingItem(existingItems, "Tag1", "AccessRights", new List<AccessRights>());
            SetPropertyOnExistingItem(existingItems, "Tag1", "Addresses", new List<string>());

            //Imported Tag setup
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "AccessRight_1", "Read");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "AccessRight_2", "Write");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "Address_1", "C1");
            AddPropertyToItemInImportedItemsList(imports, "Tag1", "Address_2", "D1");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
               .Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");

            //Assert Tag1
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag1", "AccessRights", new List<AccessRights>() { AccessRights.Read, AccessRights.Write });
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag1", "Addresses", new List<string>() { "C1", "D1" });
        }

        [Test]
        public void ImportEmptyItemWithMergeActionAndExistingData()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag3");

            //Existing Tags setup
            SetPropertyOnExistingItem(existingItems, "Tag3", "AccessRights", new List<AccessRights>() { AccessRights.Write, AccessRights.Read });
            SetPropertyOnExistingItem(existingItems, "Tag3", "Addresses", new List<string>() { "C2", "D2" });

            //Imported Tags setup
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "AccessRight_1", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "AccessRight_2", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "Address_1", "");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "Address_2", "");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
               .Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Merge, "Tag3");

            //Assert Tag3
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag3", "AccessRights", new List<AccessRights>() { AccessRights.Write, AccessRights.Read });
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag3", "Addresses", new List<string>() { "C2", "D2" });
        }

        [Test]
        public void ImportDataItemWithMergeActionAndExistingData()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag3");

            //Existing Tags setup
            SetPropertyOnExistingItem(existingItems, "Tag3", "AccessRights", new List<AccessRights>() { AccessRights.Read, AccessRights.Write });
            SetPropertyOnExistingItem(existingItems, "Tag3", "Addresses", new List<string>() { "C2", "D2" });

            //Imported Tags setup
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "AccessRight_1", "Write");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "AccessRight_2", "Read");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "Address_1", "C3");
            AddPropertyToItemInImportedItemsList(imports, "Tag3", "Address_2", "D3");

            m_ListManager.GetMergeAction(Arg.Any<Dictionary<string, string>>(), ref Arg.Any<string>())
               .Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Merge, "Tag3");

            //Assert Tag3
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag3", "AccessRights", new List<AccessRights>() { AccessRights.Write, AccessRights.Read });
            AssertPropertyOfItemInList(m_ListManager.ImportedItems, "Tag3", "Addresses", new List<string>() { "C3", "D3" });
        }

        [Test]
        public void ImportItemsWithChangeNameAndOverWriteAndSkipAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            string newName = "";
            m_ListManager.GetMergeAction(Arg.Is(imports[0]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag4";
                    return MergeAction.ChangeName;
                });
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.OverWrite);
            m_ListManager.GetMergeAction(imports[2], ref newName).Returns(MergeAction.Skip);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.OverWrite, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
        }

        [Test]
        public void ImportItemsWithOverWriteAndSkipAndChangeNameAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            string newName = "";
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.OverWrite);
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(Arg.Is(imports[2]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag4";
                    return MergeAction.ChangeName;
                });

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.OverWrite, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
        }

        [Test]
        public void ImportItemsWithSkipAndChangeNameAndOverWriteAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag1", "Tag2", "Tag3");

            string newName = "";
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(Arg.Is(imports[1]), ref Arg.Any<string>())
                .Returns(x => {
                    x[1] = "Tag4";
                    return MergeAction.ChangeName;
                }); 
            m_ListManager.GetMergeAction(imports[2], ref newName).Returns(MergeAction.OverWrite);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.OverWrite, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
        }

        [Test]
        public void ImportLessItemsThanExistingWithChangeNameAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(Arg.Is(imports[0]), ref Arg.Any<string>())
               .Returns(x => {
                   x[1] = "Tag4";
                   return MergeAction.ChangeName;
               });

            m_ListManager.MergeLists(existingItems, imports);

            Assert.AreEqual(4, m_ListManager.MergedItems.Count);
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
        }

        [Test]
        public void ImportLessItemsThanExistingWithOverWriteAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.OverWrite);

            m_ListManager.MergeLists(existingItems, imports);

            Assert.AreEqual(3, m_ListManager.MergedItems.Count);
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.OverWrite, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
        }

        [Test]
        public void ImportLessItemsThanExistingWithSkipAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
        }

        [Test]
        public void ImportNewItemsTogetherWithChangeNameAndSkipAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2", "Tag3");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(Arg.Is(imports[0]), ref Arg.Any<string>())
               .Returns(x => {
                   x[1] = "Tag3";
                   return MergeAction.ChangeName;
               });

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.ChangeName, "Tag3");
            AssertSkipAction("Tag4");
        }

        [Test]
        public void ImportNewItemsTogetherWithChangeNameAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2", "Tag3", "Tag5");

            m_ListManager.GetMergeAction(Arg.Is(imports[1]), ref Arg.Any<string>())
               .Returns(x => {
                   x[1] = "Tag4";
                   return MergeAction.ChangeName;
               });
            m_ListManager.GetMergeAction(Arg.Is(imports[0]), ref Arg.Any<string>())
               .Returns(x => {
                   x[1] = "Tag3";
                   return MergeAction.ChangeName;
               });

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.ChangeName, "Tag3");
            AssertMergeAction(MergeAction.ChangeName, "Tag4");
            AssertMergeAction(MergeAction.Add, "Tag5");
        }

        [Test]
        public void ImportNewItemsTogetherWithOverWriteAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2", "Tag3", "Tag4");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.OverWrite);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.OverWrite, "Tag2");
            AssertMergeAction(MergeAction.Add, "Tag3");
            AssertMergeAction(MergeAction.Add, "Tag4");
        }

        [Test]
        public void ImportNewItemsTogetherWithSkipAction()
        {
            IList<DataItemImportInfo> existingItems = InitExistingItems("Tag1", "Tag2", "Tag3");
            IList<Dictionary<string, string>> imports = InitImportItems("Tag2", "Tag3", "Tag4");

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Skip);

            m_ListManager.MergeLists(existingItems, imports);

            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.None, "Tag3");
            AssertMergeAction(MergeAction.Add, "Tag4");
        }

        [Test]
        public void ImportNewItemsWithSameNameAsExistingItemsWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1_new", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(1, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address is wrong");
        }

        [Test]
        public void ImportNewItemsWithSameNameAsExistingItemsWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1_new", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(1, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address is wrong");
        }

        [Test]
        public void AutoImportNewItemsWithSameNameAsExistingItemsWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1_new", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.ImportedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            Assert.IsTrue(CompareAddressForController(0, "Tag1", "Address1_new", existingItems), "Tag1 address is wrong");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address is wrong");
        }

        [Test]
        public void AutoImportNewItemsWithSameNameAsExistingItemsWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1_new", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.ImportedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            Assert.IsTrue(CompareAddressForController(0, "Tag1", "Address1_new", existingItems), "Tag1 address is wrong");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address is wrong");
        }

        [Test]
        public void ImportNewItemsWithSameAddressAsExistingItemsAndDifferentNamesWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1_new", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2_new", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.SkippedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Skip, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void ImportNewItemsWithSameAddressAsExistingItemsAndDifferentNamesWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1_new", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2_new", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", unmodifiedItems), "Tag2 missing");
            Assert.IsTrue(FindItemNameInList("Tag1_new", m_ListManager.ImportedItems), "Tag1_new missing");
            Assert.IsTrue(FindItemNameInList("Tag2_new", m_ListManager.ImportedItems), "Tag2_new missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Add, "Tag1_new");
            AssertMergeAction(MergeAction.Add, "Tag2_new");
        }

        [Test]
        public void AutoImportNewItemsWithSameAddressAsExistingItemsAndDifferentNamesWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1_new", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2_new", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.SkippedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Skip, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void AutoImportNewItemsWithSameAddressAsExistingItemsAndDifferentNamesWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1_new", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2_new", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", unmodifiedItems), "Tag2 missing");
            Assert.IsTrue(FindItemNameInList("Tag1_new", m_ListManager.ImportedItems), "Tag1_new missing");
            Assert.IsTrue(FindItemNameInList("Tag2_new", m_ListManager.ImportedItems), "Tag2_new missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Add, "Tag1_new");
            AssertMergeAction(MergeAction.Add, "Tag2_new");
        }

        [Test]
        public void ImportNewItemsWithSameNameAndSameAddressAsExistingItemsWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.SkippedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Skip, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void ImportNewItemsWithSameNameAndSameAddressAsExistingItemsWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Skip);
            m_ListManager.GetMergeAction(imports[1], ref newName).Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(1, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
        }

        [Test]
        public void AutoImportNewItemsWithSameNameAndSameAddressAsExistingItemsWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.SkippedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Skip, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void AutoImportNewItemsWithSameNameAndSameAddressAsExistingItemsWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 0);
            AddImportedItemWithAddress(imports, "Tag2", "Address2", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, false, false);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.ImportedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.Merge, "Tag1");
            AssertMergeAction(MergeAction.Merge, "Tag2");
        }

        [Test]
        public void DeleteUnusedItemsDuringAutoImportWithCompareAddressActivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddExistingItemWithAddress(existingItems, "Tag3", "Address3");
            AddExistingItemWithAddress(existingItems, "Tag4", "Address4");
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);
            AddImportedItemWithAddress(imports, "Tag3_new", "Address3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, true, false);
            m_ListManager.ShowMergeConflictDialog = false;

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag4", unmodifiedItems), "Tag4 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            Assert.IsTrue(FindItemNameInList("Tag3", m_ListManager.SkippedItems), "Tag3 missing");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            AssertMergeAction(MergeAction.Skip, "Tag3");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address for controller 1 is wrong");
        }

        [Test]
        public void DeleteUnusedItemsDuringAutoImportWithCompareAddressDeactivated()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1");
            AddExistingItemWithAddress(existingItems, "Tag2", "Address2");
            AddExistingItemWithAddress(existingItems, "Tag3", "Address3");
            AddImportedItemWithAddress(imports, "Tag2", "Address2_new", 0);
            AddImportedItemWithAddress(imports, "Tag3_new", "Address3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, false, false);
            m_ListManager.ShowMergeConflictDialog = false;

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(2, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag3", unmodifiedItems), "Tag3 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.ImportedItems), "Tag2 missing");
            Assert.IsTrue(FindItemNameInList("Tag3_new", m_ListManager.ImportedItems), "Tag3_new missing");
            AssertMergeAction(MergeAction.Merge, "Tag2");
            Assert.IsTrue(CompareAddressForController(0, "Tag2", "Address2_new", existingItems), "Tag2 address for controller 1 is wrong");
        }

        [Test]
        public void ImportSameItemsASecondTimeForController2WithoutGettingAddressOnController1AfterMerge()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1", 1);
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 1);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(1, false, false, false);

            string newName = string.Empty;
            m_ListManager.GetMergeAction(imports[0], ref newName).Returns(MergeAction.Merge);

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.ImportedItems), "Tag1 missing");
            AssertMergeAction(MergeAction.Merge, "Tag1");
            Assert.IsTrue(CheckAddressEmptyForController(0, "Tag1", existingItems), "Tag1 address for controller 0 is wrong");
            Assert.IsTrue(CompareAddressForController(1, "Tag1", "Address1", existingItems), "Tag1 address for controller 1 is wrong");
        }

        [Test]
        public void AutoImportSameItemsASecondTimeForController2WithoutGettingAddressOnController1AfterMerge()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();

            AddExistingItemWithAddress(existingItems, "Tag1", "Address1", 1);
            AddImportedItemWithAddress(imports, "Tag1", "Address1", 1);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(1, false, false, false);            

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(0, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", m_ListManager.ImportedItems), "Tag1 missing");
            AssertMergeAction(MergeAction.Merge, "Tag1");
            Assert.IsTrue(CheckAddressEmptyForController(0, "Tag1", existingItems), "Tag1 address for controller 0 is wrong");
            Assert.IsTrue(CompareAddressForController(1, "Tag1", "Address1", existingItems), "Tag1 address for controller 1 is wrong");
        }

        [Test]
        public void ImportItemsWithRules()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);
            AddImportedItemWithAddress(imports, "Item4", "TagD4", 0);
            AddImportedItemWithAddress(imports, "Item5", "TagE5", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB* | *C* | *4";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            Assert.AreEqual(3, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
            Assert.IsTrue(FindItemNameInList("Item3", m_ListManager.ImportedItems), "Item3 missing");
            Assert.IsTrue(FindItemNameInList("Item4", m_ListManager.ImportedItems), "Item4 missing");
        }

        [Test]
        public void AutoImportItemsWithRules()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);
            AddImportedItemWithAddress(imports, "Item4", "TagD4", 0);
            AddImportedItemWithAddress(imports, "Item5", "TagE5", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB* | *C* | *4";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            Assert.AreEqual(3, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
            Assert.IsTrue(FindItemNameInList("Item3", m_ListManager.ImportedItems), "Item3 missing");
            Assert.IsTrue(FindItemNameInList("Item4", m_ListManager.ImportedItems), "Item4 missing");
        }

        [Test]
        public void ImportItemsWithOneRuleOnly()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
        }

        [Test]
        public void AutoImportItemsWithOneRuleOnly()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
        }

        [Test]
        public void ImportItemsWithRuleToController2()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();            
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 1);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 1);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 1);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(1, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
        }

        [Test]
        public void AutoImportItemsWithRuleToController2()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 1);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 1);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 1);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(1, true, false, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Item2", m_ListManager.ImportedItems), "Item2 missing");
        }

        [Test]
        public void ImportItemsWithAddressCompareAndRule()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddExistingItemWithAddress(existingItems, "Tag1", "TagA1", 0);
            AddExistingItemWithAddress(existingItems, "Tag2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Default, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(1, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void AutoImportItemsWithAddressCompareAndRule()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddExistingItemWithAddress(existingItems, "Tag1", "TagA1", 0);
            AddExistingItemWithAddress(existingItems, "Tag2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item1", "TagA1", 0);
            AddImportedItemWithAddress(imports, "Item2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Item3", "TagC3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, false, true, false);
            importTagsSettings.AutomaticImportRules = "TagB*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(1, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", m_ListManager.SkippedItems), "Tag2 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.Skip, "Tag2");
        }

        [Test]
        public void FlagExistingItemsForDeleteWithNewRule()
        {
            IList<DataItemImportInfo> existingItems = new List<DataItemImportInfo>();
            IList<Dictionary<string, string>> imports = new List<Dictionary<string, string>>();
            AddExistingItemWithAddress(existingItems, "Tag1", "TagA1", 0);
            AddExistingItemWithAddress(existingItems, "Tag2", "TagB2", 0);
            AddImportedItemWithAddress(imports, "Tag3", "TagC3", 0);

            IImportTagsSettings importTagsSettings = SetImportTagSettings(0, true, true, false);
            importTagsSettings.AutomaticImportRules = "TagC*";

            m_ListManager.MergeLists(existingItems, imports, ImportMode.Silent, importTagsSettings);

            IList<DataItemImportInfo> unmodifiedItems = existingItems.Except(m_ListManager.ImportedItems).Except(m_ListManager.DeletedItems).Except(m_ListManager.SkippedItems).ToList();

            Assert.AreEqual(2, unmodifiedItems.Count, "unmodifiedItems.Count invalid");
            Assert.AreEqual(0, m_ListManager.SkippedItems.Count, "m_ListManager.SkippedItems.Count invalid");
            Assert.AreEqual(1, m_ListManager.ImportedItems.Count, "m_ListManager.ImportedItems.Count invalid");
            Assert.IsTrue(FindItemNameInList("Tag1", unmodifiedItems), "Tag1 missing");
            Assert.IsTrue(FindItemNameInList("Tag2", unmodifiedItems), "Tag2 missing");
            Assert.IsTrue(FindItemNameInList("Tag3", m_ListManager.ImportedItems), "Tag3 missing");
            AssertMergeAction(MergeAction.None, "Tag1");
            AssertMergeAction(MergeAction.None, "Tag2");
            AssertMergeAction(MergeAction.Add, "Tag3");
        }

        private void AddControllers(int numberOfControllers)
        {
            IDataSourceContainer controller = null;
            for (int i = 1; i <= numberOfControllers; i++)
            {
                ControllerHelper.CreateStubController(out controller, string.Format("Controller{0}", i));
                m_Controllers.Add(controller);
            }
        }

        private void AddExistingItemWithAddress(IList<DataItemImportInfo> existingItems, string itemName, string itemAddress)
        {
            AddExistingItemWithAddress(existingItems, itemName, itemAddress, 0);
        }

        private void AddImportedItemWithAddress(IList<Dictionary<string, string>> importItems, string itemName, string itemAddress, int controllerIndex)
        {
            Dictionary<string, string> importData = new Dictionary<string, string>();
            importData.Add("Name", itemName);
            importData.Add(string.Format("Address_{0}", controllerIndex + 1), itemAddress);
            importItems.Add(importData);
        }

        private void AddExistingItemWithAddress(IList<DataItemImportInfo> existingItems, string itemName, string itemAddress, int controllerIndex)
        {
            DataItemImportInfo newItem = new DataItemImportInfo();
            newItem.Name = itemName;
            int currentControllerIndex = 0;

            while (currentControllerIndex < controllerIndex)
            {
                newItem.Addresses.Add(string.Empty);
                currentControllerIndex++;
            }

            newItem.Addresses.Add(itemAddress);
            existingItems.Add(newItem);
        }

        private string GetName(ref Dictionary<string, string> propertiesDictionary)
        {
            return propertiesDictionary["Name"];
        }

        private void SetName(ref Dictionary<string, string> propertiesDictionary, string newName)
        {
            propertiesDictionary["Name"] = newName;
        }

        private object CreateNewObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private IList<DataItemImportInfo> InitExistingItems(params string[] itemNames)
        {
            IList<DataItemImportInfo> items = new List<DataItemImportInfo>();

            foreach (string itemName in itemNames)
            {
                items.Add(new DataItemImportInfo() { Name = itemName });
            }

            return items;
        }

        private IList<Dictionary<string, string>> InitImportItems(params string[] itemNames)
        {
            IList<Dictionary<string, string>> importItems = new List<Dictionary<string, string>>();

            foreach (string itemName in itemNames)
            {
                Dictionary<string, string> importData = new Dictionary<string, string>();
                importData.Add("Name", itemName);
                importItems.Add(importData);
            }

            return importItems;
        }

        private void AssertMergeAction(MergeAction expectedAction, string itemName)
        {
            IImportMergeInfo importMergeInfo = FindMergedItem(itemName);
            Assert.IsNotNull(importMergeInfo);
            Assert.AreEqual(expectedAction, importMergeInfo.MergeAction);
        }

        private void AssertSkipAction(string itemName)
        {
            IImportMergeInfo importMergeInfo = FindMergedItem(itemName);
            Assert.IsNull(importMergeInfo);
        }

        private IImportMergeInfo FindMergedItem(string itemName)
        {
            return m_ListManager.MergedItems.Where(item => item.Name == itemName).FirstOrDefault() as IImportMergeInfo;
        }

        private bool FindItemNameInList(string itemName, IList<DataItemImportInfo> itemList)
        {
            foreach (DataItemImportInfo currentItem in itemList)
            {
                if (currentItem.Name == itemName)
                    return true;
            }
            return false;
        }

        private IImportTagsSettings SetImportTagSettings(int controllerIndex, bool deleteUnused, bool compareAddresses, bool useVerificationDialog)
        {
            IImportTagsSettings importTagsSettings = new ImportTagsSettings();
            ((IImportTagsSettings)importTagsSettings).ControllerIndex = controllerIndex;
            ((IImportTagsSettings)importTagsSettings).DeleteUnused = deleteUnused;
            ((IImportTagsSettings)importTagsSettings).CompareAddresses = compareAddresses;
            ((IImportTagsSettings)importTagsSettings).UseVerificationDialog = useVerificationDialog;

            return importTagsSettings;
        }

        private bool CheckAddressEmptyForController(int controllerIndex, string itemName, IList<DataItemImportInfo> itemList)
        {
            foreach (DataItemImportInfo currentItem in itemList.Where(item => (item.Name == itemName)))
            {
                if (currentItem.Name == itemName)
                {
                    if (currentItem.Addresses.Count < (controllerIndex + 1))
                        return false;

                    return string.IsNullOrEmpty(currentItem.Addresses[controllerIndex]);
                }
            }
            return false;
        }

        private bool CompareAddressForController(int controllerIndex, string itemName, string controllerAddress, IList<DataItemImportInfo> itemList)
        {
            foreach (DataItemImportInfo currentItem in itemList.Where(item => (item.Name == itemName)))
            {
                if (currentItem.Name == itemName)
                {
                    if (currentItem.Addresses.Count < (controllerIndex + 1))
                        return false;

                    return string.Equals(controllerAddress, currentItem.Addresses[controllerIndex]);
                }
            }
            return false;
        }

        private bool ValidateImportRulesOnAddress(ref Dictionary<string, string> propertyToValue, ref string errorMessage, string importRules)
        {
            int controllerCount = m_Controllers.Count();
            string dataItem;

            for (int index = 0; index < controllerCount; index++)
            {
                if (propertyToValue.TryGetValue(string.Format("Address_{0}", index + 1), out dataItem))
                {
                    if (!ValidateImportRulesOnAddress(dataItem, importRules))
                        return false;
                }
            }
            return true;
        }

        private bool ValidateImportRulesOnAddress(string dataItem, string importRules)
        {
            return ImportRulesTranslator.IsValidAccordingToRule(importRules, dataItem);
        }

        /// <summary>
        /// Adds [propertyToAdd]: [valueForProperty] key-value pairs to the dictionary which contains an key-value pair [Name]: [tagName] 
        /// </summary>
        /// <param name="importedList">A list of dictionaries that resembles an array of string-only JSONs in memory</param>
        /// <param name="tagName">The tag name that will be used to find the dictionary onto which [propertyToAdd]: [valueForProperty] will be added</param>
        /// <param name="propertyToAdd">The key that should be added to the selected dictionary</param>
        /// <param name="valueForProperty">The value that should be set for the key "propertyToAdd" in the selected dictionary</param>
        private void AddPropertyToItemInImportedItemsList(IList<Dictionary<string, string>> importedList, string tagName, string propertyToAdd, string valueForProperty)
        {
            string tempValue;
            IDictionary<string, string> singleImportedItem = importedList.FirstOrDefault(
                x =>
                {
                    x.TryGetValue("Name", out tempValue);
                    return tempValue == tagName;
                });

            singleImportedItem.Add(propertyToAdd, valueForProperty);
        }

        /// <summary>
        /// Sets a value for a property in an object within dataItemList whose tag name matches tagName
        /// </summary>
        /// <param name="dataItemList">List of DataItemImportInfo objects into which you want to operate. NOTE: objects within this list will be modified</param>
        /// <param name="tagName">The tag name of the object you wish to modify</param>
        /// <param name="propertyToSet">The property for which you would like to set a value</param>
        /// <param name="valueForProperty">The value you would like to set for "propertyToSet"</param>
        private void SetPropertyOnExistingItem(IList<DataItemImportInfo> dataItemList, string tagName, string propertyToSet, object valueForProperty)
        {
            DataItemImportInfo dataItemImportInfo = dataItemList.FirstOrDefault(x => x.Name == tagName);

            dataItemImportInfo.GetType().GetProperty(propertyToSet).SetValue(dataItemImportInfo, valueForProperty, null);
        }

        /// <summary>
        /// Asserts equality for the expectedValueForProperty compared to the value of 
        /// property "propertyName" found in an object with tag name tagName within dataItemList 
        /// </summary>
        /// <param name="dataItemList">A list of DataItemImportInfo objects in which you want to check a specific property in a tag</param>
        /// <param name="tagName">The name of the tag for which the assertion will be made</param>
        /// <param name="propertyName">The name of the property, the value of which will be checked agaisnt expectedValueForProperty</param>
        /// <param name="expectedValueForProperty">The object you expect to find in propertyName of tagName within dataItemList</param>
        private void AssertPropertyOfItemInList(IList<DataItemImportInfo> dataItemList, string tagName, string propertyName, object expectedValueForProperty)
        {
            DataItemImportInfo dataItemImportInfo = dataItemList.FirstOrDefault(x => x.Name == tagName);

            object currentValueOfProperty = dataItemImportInfo.GetType().GetProperty(propertyName).GetValue(dataItemImportInfo, null);
            Assert.AreEqual(currentValueOfProperty, expectedValueForProperty);
        }
    }
}
