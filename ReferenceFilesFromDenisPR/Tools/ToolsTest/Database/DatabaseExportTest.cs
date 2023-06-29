using System.Collections.Generic;
using Core.Api.DI.PlatformFactory;
using Core.Api.GlobalReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Threading;
using Neo.ApplicationFramework.Utilities.Lazy;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Database
{
    [TestFixture]
    public class DatabaseExportTest
    {
        private IDatabaseImportExportService m_DatabaseImportExportService;
        private ISystemSettings m_SystemSettings;
        private IFilePathLogic m_FilePathLogic;
        private IActionConsumer m_ActionConsumer;

        [SetUp]
        public void SetUp()
        {
            m_ActionConsumer = Substitute.For<IActionConsumer>();

            m_SystemSettings = Substitute.For<ISystemSettings>();

            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            m_FilePathLogic = Substitute.For<IFilePathLogic>();

            platFormFactoryService.Create<IFilePathLogic>().Returns(m_FilePathLogic);

            IStorageCacheService storageCacheService = TestHelper.CreateAndAddServiceStub<IStorageCacheService>();
            IStorage storage = TestHelper.CreateAndAddServiceStub<IStorage>();
            IStorageScheme scheme = Substitute.For<IStorageScheme>();

            scheme.TableExists(Arg.Any<string>()).Returns(true);
            storage.Scheme.Returns(scheme);
            storageCacheService.GetStorage(Arg.Any<string>()).Returns(storage);

            var target = Substitute.For<ITarget>();
            target.Id.Returns(TargetPlatform.WindowsCE);

            m_DatabaseImportExportService = new DatabaseImportExportServiceCF(ServiceContainerCF.GetServiceLazy<IGlobalReferenceService>(), new LazyCF<ISystemSettings>(() => m_SystemSettings), m_ActionConsumer, true);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetTargetPathForFileIsCalledWithCorrectParameters()
        {
            m_SystemSettings.FtpServerFriendlyNamesEnabled.Returns(true);
            m_SystemSettings.FtpServerEnabled.Returns(true);

            m_DatabaseImportExportService.Export("databaseName", "tableName", FileDirectory.ProjectFiles, "groupName", "fileName", "csvPath", false, true, false, false, false);

            m_FilePathLogic.GetTargetPathForFile(FileDirectory.ProjectFiles, "fileName", "csv", false, true, "DatabaseExport\\groupName", true).Returns("something");

            m_ActionConsumer.WhenForAnyArgs(x => x.Enqueue(Arg.Any<System.Action>())).Do(y => ((System.Action)y[0])());

            m_FilePathLogic.Received()
                .GetTargetPathForFile(FileDirectory.ProjectFiles, "fileName", "csv", false, true, "DatabaseExport\\groupName", true);
        }
    }
}