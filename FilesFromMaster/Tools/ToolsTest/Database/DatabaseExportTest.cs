using System.Collections.Generic;
using Core.Api.DI.PlatformFactory;
using Core.Api.GlobalReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Storage.Common;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Common.Utilities.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Storage.Threading;

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
            m_ActionConsumer = MockRepository.GenerateMock<IActionConsumer>();

            m_SystemSettings = MockRepository.GenerateMock<ISystemSettings>();

            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceMock<IPlatformFactoryService>();
            m_FilePathLogic = MockRepository.GenerateMock<IFilePathLogic>();

            platFormFactoryService.Stub(x => x.Create<IFilePathLogic>()).Return(m_FilePathLogic);

            IStorageCacheService storageCacheService = TestHelper.CreateAndAddServiceMock<IStorageCacheService>();
            IStorage storage = TestHelper.CreateAndAddServiceMock<IStorage>();
            IStorageScheme scheme = MockRepository.GenerateMock<IStorageScheme>();

            scheme.Stub(x => x.TableExists("tableName")).IgnoreArguments().Return(true);
            storage.Stub(x => x.Scheme).IgnoreArguments().Return(scheme);
            storageCacheService.Stub(x => x.GetStorage("string")).IgnoreArguments().Return(storage);

            var target = MockRepository.GenerateStub<ITarget>();
            target.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);

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
            m_SystemSettings.Stub(x => x.FtpServerFriendlyNamesEnabled).Return(true);
            m_SystemSettings.Stub(x => x.FtpServerEnabled).Return(true);

            m_DatabaseImportExportService.Export("databaseName", "tableName", FileDirectory.ProjectFiles, "groupName", "fileName", "csvPath", false, true, false, false, false);

            m_FilePathLogic.Stub(x => x.GetTargetPathForFile(FileDirectory.ProjectFiles, "fileName", "csv", false, true, "DatabaseExport\\groupName", true)).Return("something");

            IList<object[]> argList = m_ActionConsumer.GetArgumentsForCallsMadeOn(x => x.Enqueue(() => { }));
            ((System.Action)argList[0][0])();

            m_FilePathLogic.AssertWasCalled(x => x.GetTargetPathForFile(FileDirectory.ProjectFiles, "fileName", "csv", false, true, "DatabaseExport\\groupName", true));

            m_FilePathLogic.VerifyAllExpectations();
        }
    }
}
