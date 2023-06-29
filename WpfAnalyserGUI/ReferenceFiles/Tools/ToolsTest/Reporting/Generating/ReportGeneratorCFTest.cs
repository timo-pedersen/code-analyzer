using System;
using Core.Api.Application;
using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Common.FileLogic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Reporting.Generating
{
    [TestFixture]
    public class ReportGeneratorCFTest
    {
#if VNEXT_TARGET
        private ReportGeneratorBase m_ReportGeneratorCF;
#else
        private ReportGeneratorCF m_ReportGeneratorCF;
#endif

        private ISettings m_Settings;

        private ISystemSettings m_SystemSettingsMock;

        private IReport m_FirstReport;
        private IReportExcelProvider m_ExcelProvider;
        private FileHelperCF m_FileHelper;

        [SetUp]
        public void Setup()
        {
            TestHelper.CreateAndAddServiceStub<IStorageCacheService>();

            m_Settings = Substitute.For<ISettings>();
            m_SystemSettingsMock = Substitute.For<ISystemSettings>();

            IDateTimeEditService dateTimeEditService = TestHelper.CreateAndAddServiceStub<IDateTimeEditService>();
            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            ICoreApplication coreApp = TestHelper.CreateAndAddServiceStub<ICoreApplication>();

#if VNEXT_TARGET
            IFilePathLogic filePathLogic = new FilePathLogic();
#else
            IFilePathLogic filePathLogic = new FilePathLogicCF();
#endif

            coreApp.StartupPath.Returns("C:\\SomeFolder\\iX");
            dateTimeEditService.CreateTimeString(Arg.Any<DateTime>(), Arg.Any<DateTimeDisplayFormat>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns("");

            m_ExcelProvider = Substitute.For<IReportExcelProvider>();
            //m_ExcelProvider.GenerateAndSave(Arg.Any<string>(), Arg.Any<string>());

            platFormFactoryService.Create<IFilePathLogic>().Returns(filePathLogic);

            m_FirstReport = Substitute.For<IReport>().With(rep => rep.FileName = "File name with spaces.and.dots.extension");

            m_FileHelper = Substitute.For<FileHelperCF>();
            //m_FileHelper.Move("", "")).IgnoreArguments();
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
            m_Settings.SystemSettings.Returns(m_SystemSettingsMock);

            m_SystemSettingsMock.FtpServerFriendlyNamesEnabled.Returns(isFtpFriendly);
            m_SystemSettingsMock.FtpServerEnabled.Returns(isFtpFriendly);

#if VNEXT_TARGET
            m_ReportGeneratorCF = new ReportGeneratorBase(m_Settings, m_ExcelProvider, m_FileHelper);
#else
            m_ReportGeneratorCF = new ReportGeneratorCF(m_Settings, m_ExcelProvider, m_FileHelper);
#endif
        }
    }
}