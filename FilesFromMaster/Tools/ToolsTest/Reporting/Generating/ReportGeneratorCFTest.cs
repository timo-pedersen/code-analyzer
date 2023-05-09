using System;
using Core.Api.Application;
using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Common.FileLogic;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Neo.ApplicationFramework.Interfaces.Storage;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Reporting.Generating
{
    [TestFixture]
    public class ReportGeneratorCFTest
    {
        private ReportGeneratorCF m_ReportGeneratorCF;

        private ISettings m_Settings;

        private ISystemSettings m_SystemSettingsMock;

        private IReport m_FirstReport;
        private IReportExcelProvider m_ExcelProvider;
        private FileHelperCF m_FileHelper;

        [SetUp]
        public void Setup()
        {
            TestHelper.CreateAndAddServiceMock<IStorageCacheService>();

            m_Settings = MockRepository.GenerateMock<ISettings>();
            m_SystemSettingsMock = MockRepository.GenerateMock<ISystemSettings>();

            IDateTimeEditService dateTimeEditService = TestHelper.CreateAndAddServiceMock<IDateTimeEditService>();
            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceMock<IPlatformFactoryService>();
            ICoreApplication coreApp = TestHelper.CreateAndAddServiceMock<ICoreApplication>();
            IFilePathLogic filePathLogic = new FilePathLogicCF();

            coreApp.Stub(x => x.StartupPath).Return("C:\\SomeFolder\\iX");
            dateTimeEditService.Stub(x => x.CreateTimeString(new DateTime(), new DateTimeDisplayFormat(), false, false)).IgnoreArguments().Return("");

            m_ExcelProvider = MockRepository.GenerateStub<IReportExcelProvider>();
            m_ExcelProvider.Stub(x => x.GenerateAndSave("", "")).IgnoreArguments();

            platFormFactoryService.Stub(x => x.Create<IFilePathLogic>()).Return(filePathLogic);

            m_FirstReport = MockRepository.GenerateStub<IReport>().With(rep => rep.FileName = "File name with spaces.and.dots.extension");

            m_FileHelper = MockRepository.GenerateStub<FileHelperCF>();
            m_FileHelper.Stub(x => x.Move("", "")).IgnoreArguments();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void TestThatFileNameIsFtpFriendly()
        {
            // A space + date is added in the real implementation. The extra space appears because we do not provide a mock date;
            // the space is then converted to an underline for FTP friendliness
            const string expected = "/File_name_with_spaces-and-dots_.extension";

            PrepareGenerator(true);

            ReportGenerationOutcome outcome = m_ReportGeneratorCF.GenerateAndSave(m_FirstReport, "/");

            StringAssert.DoesNotContain(" ", outcome.FileName);

            Assert.AreEqual(expected, outcome.FileName);
        }

        [Test]
        public void TestThatFileNameIsNotFtpFriendly()
        {
            // A space + date is added in the real implementation. The extra space appears because we do not provide a mock date.
            const string expected = "/File name with spaces.and.dots .extension";

            PrepareGenerator(false);

            ReportGenerationOutcome outcome = m_ReportGeneratorCF.GenerateAndSave(m_FirstReport, "/");

            StringAssert.Contains(" ", outcome.FileName);

            Assert.AreEqual(expected, outcome.FileName);
        }

        private void PrepareGenerator(bool isFtpFriendly)
        {
            m_Settings.Stub(x => x.SystemSettings).Return(m_SystemSettingsMock);

            m_SystemSettingsMock.Stub(x => x.FtpServerFriendlyNamesEnabled).Return(isFtpFriendly);
            m_SystemSettingsMock.Stub(x => x.FtpServerEnabled).Return(isFtpFriendly);

            m_ReportGeneratorCF = new ReportGeneratorCF(m_Settings, m_ExcelProvider, m_FileHelper);
        }
    }
}
