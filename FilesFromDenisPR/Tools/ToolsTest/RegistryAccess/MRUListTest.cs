using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.Win32;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.RegistryAccess
{
    [TestFixture]
    public class MRUListTest
    {
        private const string MRU_REGISTRY_PATH = @"SOFTWARE\Beijer\TEST\MRUTest";

        private IRecentProjectsListService m_MruListService;

        [SetUp]
        public void SetUp()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(MRU_REGISTRY_PATH);
            }
            catch (Exception) { }

            TestHelper.CreateAndAddServiceStub<IBrandServiceIde>();
            m_MruListService = new RecentProjectsListService();
            m_MruListService.MaxNumberOfEntries = 10;
            ((RecentProjectsListService)m_MruListService).FileHelper = GetFileHelperStub(true);
            ((RecentProjectsListService)m_MruListService).RegistryPath = MRU_REGISTRY_PATH;
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(MRU_REGISTRY_PATH);
            }
            catch (Exception) { }
        }

        [Test]
        public void TestCreateMRUList()
        {
            Assert.IsNotNull(m_MruListService);

            Assert.IsNotNull(m_MruListService.FileNames, "File name list should be empty, not null");

            Assert.AreEqual(0, m_MruListService.FileNames.Count, "Count should be zero when not initialized");
        }

        [Test]
        public void TestSetRegistryPath()
        {
            Assert.AreEqual(MRU_REGISTRY_PATH, ((RecentProjectsListService)m_MruListService).RegistryPath);

            RegistryKey regKey = null;
            try
            {
                regKey = Registry.CurrentUser.OpenSubKey(MRU_REGISTRY_PATH);
            }
            catch (Exception e)
            {
                Assert.Fail("Could not open registry key.\r\n\r\nMessage:{0}", e.Message);
            }
            Assert.IsNotNull(regKey, "Registry key is empty");

            Assert.AreEqual(0, regKey.ValueCount, "Registry should have no entries");
        }

        [Test]
        public void TestAddItem()
        {

            Assert.IsTrue(m_MruListService.AddFile("TESTFile"), "Could not add file");
            Assert.IsNotNull(m_MruListService.FileNames, "File name list should not be null");

            Assert.AreEqual(1, m_MruListService.FileNames.Count, "Count should be 1");
            Assert.IsTrue(m_MruListService.FileNames.Contains("TESTFile"));

            Assert.IsTrue(m_MruListService.AddFile("TESTFile2"), "Could not add file");
            Assert.IsTrue(m_MruListService.AddFile("TESTFile3"), "Could not add file");
            Assert.IsTrue(m_MruListService.AddFile("TESTFile4"), "Could not add file");
            Assert.IsTrue(m_MruListService.AddFile("TESTFile5"), "Could not add file");

            Assert.AreEqual(5, m_MruListService.FileNames.Count, "Count should be 5");

            Assert.IsTrue(m_MruListService.FileNames.Contains("TESTFile3"));
        }

        [Test]
        public void TestRemoveItem()
        {
            m_MruListService.AddFile("File1");
            m_MruListService.AddFile("File2");
            m_MruListService.AddFile("File3");
            m_MruListService.AddFile("File4");
            m_MruListService.AddFile("File5");
            m_MruListService.AddFile("File6");

            Assert.AreEqual(6, m_MruListService.FileNames.Count, "Count should be 6");

            Assert.IsTrue(m_MruListService.FileNames.Contains("File1"));
            Assert.IsTrue(m_MruListService.RemoveFile("File1"));
            Assert.IsFalse(m_MruListService.FileNames.Contains("File1"));

            Assert.IsTrue(m_MruListService.FileNames.Contains("File5"));
            Assert.IsTrue(m_MruListService.RemoveFile("File5"));
            Assert.IsFalse(m_MruListService.FileNames.Contains("File5"));

        }

        [Test]
        public void TestNotificationOnAddNew()
        {
            Semaphore eventSemaphore = new Semaphore(0, 1);
            string propertyName = string.Empty;

            ((INotifyPropertyChanged)m_MruListService).PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                eventSemaphore.Release(1);
                propertyName = e.PropertyName;
            };

            m_MruListService.AddFile("TestFile");

            Assert.IsTrue(eventSemaphore.WaitOne(1000, false), "Event timed out");
            Assert.AreEqual("FileNames", propertyName);
        }

        [Test]
        public void NonExistingFilesAreNotReturned()
        {
            FileHelper stubHelper = Substitute.For<FileHelper>();
            stubHelper.Exists("ExistingFile1").Returns(true);
            stubHelper.Exists("ExistingFile2").Returns(true);

            //Remove stub filehelper that always says that the file exists
            ((RecentProjectsListService)m_MruListService).FileHelper = stubHelper;

            Assert.AreEqual(m_MruListService.FileNames.Count, 0, "Wrong start value");
            m_MruListService.AddFile("TestFile1");
            m_MruListService.AddFile("TestFile2");
            m_MruListService.AddFile("ExistingFile1");
            m_MruListService.AddFile("TestFile3");
            m_MruListService.AddFile("ExistingFile2");

            Assert.AreEqual(2, m_MruListService.FileNames.Count);
        }

        [Test]
        public void TestNotificationOnRemove()
        {
            Semaphore eventSemaphore = new Semaphore(0, 1);
            string propertyName = string.Empty;

            m_MruListService.AddFile("TestFile");

            ((INotifyPropertyChanged)m_MruListService).PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                eventSemaphore.Release(1);
                propertyName = e.PropertyName;
            };

            m_MruListService.RemoveFile("TestFile");

            Assert.IsTrue(eventSemaphore.WaitOne(1000, false), "Event timed out");
            Assert.AreEqual("FileNames", propertyName);
        }

        [Test]
        public void TestPersistMRU()
        {
            m_MruListService.AddFile("Test1");
            m_MruListService.SaveList();

            IRecentProjectsListService newListService = new RecentProjectsListService();
            ((RecentProjectsListService)newListService).FileHelper = GetFileHelperStub(true);
            ((RecentProjectsListService)newListService).RegistryPath = MRU_REGISTRY_PATH;

            Assert.IsNotNull(newListService.FileNames, "MRUList should not be null");

            Assert.AreEqual(1, newListService.FileNames.Count, "Count should be 1");
            Assert.IsTrue(newListService.FileNames.Contains("Test1"), "Test1 not found in MRU");


            m_MruListService.AddFile("Test2");
            m_MruListService.AddFile("Test3");
            m_MruListService.AddFile("Test4");
            m_MruListService.AddFile("Test5");
            m_MruListService.SaveList();

            newListService = new RecentProjectsListService();
            ((RecentProjectsListService)newListService).FileHelper = GetFileHelperStub(true);
            ((RecentProjectsListService)newListService).RegistryPath = MRU_REGISTRY_PATH;

            Assert.AreEqual(5, newListService.FileNames.Count, "Count should be 5");
            Assert.IsTrue(newListService.FileNames.Contains("Test4"), "Test4 not found in MRU");
        }

        [Test]
        public void AdvancedPersist()
        {
            m_MruListService.AddFile("Test1");
            m_MruListService.AddFile("Test2");
            m_MruListService.AddFile("Test3");
            m_MruListService.SaveList();

            IRecentProjectsListService newListService = new RecentProjectsListService();
            ((RecentProjectsListService)newListService).FileHelper = GetFileHelperStub(true);
            ((RecentProjectsListService)newListService).RegistryPath = MRU_REGISTRY_PATH;

            Assert.IsNotNull(newListService.FileNames, "MRUList should not be null");

            Assert.AreEqual(3, newListService.FileNames.Count, "Count should be 3");
            Assert.IsTrue(newListService.FileNames.Contains("Test2"), "Test2 not found in MRU");

            m_MruListService.RemoveFile("Test2");
            m_MruListService.SaveList();

            newListService = new RecentProjectsListService();
            ((RecentProjectsListService)newListService).FileHelper = GetFileHelperStub(true);
            ((RecentProjectsListService)newListService).RegistryPath = MRU_REGISTRY_PATH;

            Assert.AreEqual(2, newListService.FileNames.Count, "Count should be 2");
            Assert.IsFalse(newListService.FileNames.Contains("Test2"), "Test2 should not be found in MRU");


        }

        private FileHelper GetFileHelperStub(bool fileExistsResult)
        {
            FileHelper stubHelper = Substitute.For<FileHelper>();
            stubHelper.Exists(Arg.Any<string>()).Returns(fileExistsResult);

            return stubHelper;
        }

    }
}
