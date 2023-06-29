using System;
using System.Drawing;
using System.IO;
using Core.Api.DI.PlatformFactory;
using FlexCel.XlsAdapter;
using log4net;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Common.Utilities.Threading;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportHandlingCFTest
    {
        private const string ReportStubName = "Report1";
        private static readonly string PathToXlsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Reporting\Generating\ExampleReports\2sheet5x5.xls");
        private ReportHandlingCFDouble m_ReportHandlingCF;
        private IFilePathLogic m_FilePathLogic;
        private NotificationLogic m_NotificationLogic;
        private IMessageBoxServiceCF m_MessageBoxService;
        private IReportGenerator m_ReportGeneration;
        private FileHelperCF m_FileHelper;
        private IPrinterServiceCF m_PrinterService;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();

            m_FilePathLogic = Substitute.For<IFilePathLogic>();
            m_NotificationLogic = Substitute.For<NotificationLogic>(new Bitmap(1, 1));
            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            m_PrinterService = TestHelper.CreateAndAddServiceStub<IPrinterServiceCF>();
            m_MessageBoxService = TestHelper.CreateAndAddServiceStub<IMessageBoxServiceCF>();
            m_ReportGeneration = Substitute.For<IReportGenerator>();
            m_FileHelper = Substitute.For<FileHelperCF>();
            platFormFactoryService.Create<IFilePathLogic>().Returns(m_FilePathLogic);
            platFormFactoryService.Create<IReportGenerator>().Returns(m_ReportGeneration);

            m_PrinterService.SendToPrinterAsync(Arg.Any<Interfaces.Printer.IFlowDocument>()).Returns(ITaskBuilder.FromCompleted());

            m_ReportHandlingCF = new ReportHandlingCFDouble(m_FilePathLogic, m_NotificationLogic, m_FileHelper, m_PrinterService);
        }

        [Test]
        public void SavingThatSuceedsGeneratesAndSavesTheReport()
        {
            string path = TempPath;
            IReport report = ReportStub;

            var outcome = new ReportGenerationOutcome(report.Name, "SavedReport1.xls");
            StubTargetPathForReport(path);
            StubEnoughDiskSpace();
            m_ReportGeneration.GenerateAndSave(report, path).Returns(outcome);


            m_ReportHandlingCF.Save(report, path).Wait();

            Assert.That(m_ReportHandlingCF.WasSaveEventFired, Is.True);
        }

        [Test]
        public void WhenSavingAndTheresNotEnoughDiskSpaceItWarnsAboutItAndDoesntSaveTheReport()
        {
            StubTargetPathForReport(String.Empty);
            StubNotEnoughDiskSpace();

            m_ReportHandlingCF.Save(ReportStub, TempPath).Wait();

            Assert.That(m_ReportHandlingCF.WasSaveEventFired, Is.False);

            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.True);
        }

        [Test]
        public void SavingShowsANotificationMessageAndThenHidesIt()
        {
            IReport report = ReportStub;
            string expectedNotificationMessage = String.Format(TextsCF.ReportGenerationSavingMessage, report.Name);

            StubTargetPathForReport("");
            StubEnoughDiskSpace();
            StubTemporaryCreatedReportFile(report, TempPath);

            m_ReportHandlingCF.Save(report, TempPath).Wait();

            m_NotificationLogic.Received().ShowNotification(expectedNotificationMessage);
            m_NotificationLogic.Received().HideNotification();
        }

        [Test]
        public void SavingThatFailesCausesGenerationFailedEvent()
        {
            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.False);
            m_ReportGeneration.GenerateAndSave(Arg.Any<IReport>(), Arg.Any<string>()).Returns(x => throw new NullReferenceException());

            m_ReportHandlingCF.Save(ReportStub, String.Empty).Wait();

            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.True);
        }

        [Test]
        public void SavingThatSuceedsCausesSavedEvent()
        {
            Assert.That(m_ReportHandlingCF.WasSaveEventFired, Is.False);

            IReport reportStub = ReportStub;
            StubTargetPathForReport("");
            StubEnoughDiskSpace();
            StubTemporaryCreatedReportFile(reportStub, TempPath);

            m_ReportHandlingCF.Save(reportStub, TempPath).Wait();

            Assert.That(m_ReportHandlingCF.WasSaveEventFired, Is.True);
        }

        [Test]
        public void PrintingShowsANotificationMessageAndThenHidesIt()
        {
            IReport report = ReportStub;
            string expectedNotificationMessage = string.Format(TextsCF.ReportGenerationPrintingMessage, report.Name);

            StubTargetPathForReport("");
            StubEnoughDiskSpace();
            StubTemporaryCreatedReportFile();

            m_ReportHandlingCF.Print(report).Wait();

            m_NotificationLogic.Received().ShowNotification(expectedNotificationMessage);
            m_NotificationLogic.Received().HideNotification();
        }

        [Test]
        public void PrintingThatFailesCausesGenerationFailedEvent()
        {
            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.False);

            m_ReportHandlingCF.Print(ReportStub).Wait();

            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.True);
        }

        [Test]
        public void PrintingThatSuceedsCausesPrintedEvent()
        {
            Assert.That(m_ReportHandlingCF.WasPrintedEventFired, Is.False);

            IReport report = ReportStub;
            StubTargetPathForReport("");
            StubReportFileForPrinting(report);

            m_ReportHandlingCF.Print(report).Wait();

            Assert.That(m_ReportHandlingCF.WasPrintedEventFired, Is.True);
        }

        #region StubHelpers
        private void StubTemporaryCreatedReportFile(IReport report = null, string tempPath = null, string fileNameOfTemporaryReport = null)
        {
            if (report == null)
                report = ReportStub;

            if (fileNameOfTemporaryReport == null)
                fileNameOfTemporaryReport = string.Empty;

            if (!File.Exists(PathToXlsFile))
            {
                Assert.Fail(string.Format("This test requires a report xls file located at {0}", PathToXlsFile));
            }
            var outcome = new ReportGenerationOutcome(ReportStubName, fileNameOfTemporaryReport, PathToXlsFile);
            m_ReportGeneration.GenerateAndSave(report, tempPath).Returns(outcome);
        }

        private void StubReportFileForPrinting(IReport report = null)
        {
            var xlsFile = new XlsFile(PathToXlsFile);
            m_ReportGeneration.Generate(report).Returns(xlsFile);
        }

        private void StubTargetPathForReport(string path)
        {
            m_FilePathLogic.GetTargetPathForFile(FileDirectory.ProjectFiles).Returns(path);

        }

        private void StubNotEnoughDiskSpace()
        {
            m_ReportHandlingCF.DiskCheckerStub.ThereIsNotEnoughDiskSpaceForReport(Arg.Any<IReport>(), Arg.Any<string>())
                .Returns(true);
        }

        private void StubEnoughDiskSpace()
        {
            m_ReportHandlingCF.DiskCheckerStub.ThereIsNotEnoughDiskSpaceForReport(Arg.Any<IReport>(), Arg.Any<string>())
                .Returns(false);
        }

        private void AssertNotEnoughtDiskSpaceMessageWasShowed(string caption)
        {
            m_MessageBoxService.Received().Show(TextsCF.ReportGenerationFailedDiskSpace, caption, Arg.Any<bool>());
        }

        private string TempPath
        {
            get { return Path.GetTempPath(); }
        }

        private IReport ReportStub
        {
            get
            {
                IReport report = Substitute.For<IReport>();
                report.Name.Returns(ReportStubName);
                report.FileName.Returns($"{ReportStubName}.xls");
                return report;
            }
        }
        #endregion
    }

    internal class ReportHandlingCFDouble : ReportHandlingBase
    {
        public bool WasSaveEventFired { get; private set; }
        public bool WasGenerationFailedEventFired { get; private set; }
        public bool WasPrintedEventFired { get; private set; }

        protected override ILog Log => LogManager.GetLogger(typeof(ReportHandlingCFDouble));

        public ReportDiskSpaceChecker DiskCheckerStub
        {
            get { return DiskChecker; }
        }

        public ReportHandlingCFDouble(IFilePathLogic filePathLogic, NotificationLogic notificationLogic, FileHelperCF fileHelper, IPrinterServiceCF printerService)
            : base(Substitute.For<ReportDiskSpaceChecker>(filePathLogic),
            new SynchronousExtendedThreadPool(), notificationLogic, filePathLogic, fileHelper, printerService)
        {
        }

        protected override void FireReportSaved(string reportName, string generatedReportFileName)
        {
            base.FireReportSaved(reportName, generatedReportFileName);
            WasSaveEventFired = true;
        }

        protected override void FireReportPrinted(string reportName)
        {
            base.FireReportPrinted(reportName);
            WasPrintedEventFired = true;
        }

        protected override void FireReportGenerationFailed(GenerateReportAction action, string reportName, string generatedFileName, string message, Exception exception)
        {
            base.FireReportGenerationFailed(action, reportName, generatedFileName, message, exception);
            WasGenerationFailedEventFired = true;
        }
    }

    internal class SynchronousExtendedThreadPool : ExtendedThreadPool
    {
        public override bool QueueUserWorkItem<T>(T state, Action<T> callback)
        {
            callback(state);
            return true;
        }
    }
}
