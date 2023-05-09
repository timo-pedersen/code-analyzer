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
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportHandlingCFTest
    {
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

            m_FilePathLogic = MockRepository.GenerateStub<IFilePathLogic>();
            m_NotificationLogic = MockRepository.GenerateStub<NotificationLogic>(new Bitmap(1, 1));
            IPlatformFactoryService platFormFactoryService = TestHelper.CreateAndAddServiceMock<IPlatformFactoryService>();
            m_PrinterService = TestHelper.CreateAndAddServiceMock<IPrinterServiceCF>();
            m_MessageBoxService = TestHelper.CreateAndAddServiceMock<IMessageBoxServiceCF>();
            m_ReportGeneration = MockRepository.GenerateStub<IReportGenerator>();
            m_FileHelper = MockRepository.GenerateStub<FileHelperCF>();
            platFormFactoryService.Stub(x => x.Create<IFilePathLogic>()).Return(m_FilePathLogic);
            platFormFactoryService.Stub(x => x.Create<IReportGenerator>()).Return(m_ReportGeneration);

            m_PrinterService.Stub(x => x.SendToPrinterAsync(null)).IgnoreArguments().Return(ITaskBuilder.FromCompleted());

            m_ReportHandlingCF = new ReportHandlingCFDouble(m_FilePathLogic, m_NotificationLogic, m_FileHelper, m_PrinterService);
        }

        [Test]
        public void SavingThatSuceedsGeneratesAndSavesTheReport()
        {
            string path = TempPath;
            IReport report = ReportStub;

            StubTargetPathForReport(path);
            StubEnoughDiskSpace();
            m_ReportGeneration.Stub(x => x.GenerateAndSave(report, path)).Return(new ReportGenerationOutcome(report.Name, "SavedReport1.xls"));


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

            m_NotificationLogic.AssertWasCalled(x => x.ShowNotification(expectedNotificationMessage));
            m_NotificationLogic.AssertWasCalled(x => x.HideNotification());
        }

        [Test]
        public void SavingThatFailesCausesGenerationFailedEvent()
        {
            Assert.That(m_ReportHandlingCF.WasGenerationFailedEventFired, Is.False);
            m_ReportGeneration.Stub(s => s.GenerateAndSave(null, null)).IgnoreArguments().Throw(
                new NullReferenceException());

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
            string expectedNotificationMessage = String.Format(TextsCF.ReportGenerationPrintingMessage, report.Name);

            StubTargetPathForReport("");
            StubEnoughDiskSpace();
            StubTemporaryCreatedReportFile();

            m_ReportHandlingCF.Print(report).Wait();

            m_NotificationLogic.AssertWasCalled(x => x.ShowNotification(expectedNotificationMessage));
            m_NotificationLogic.AssertWasCalled(x => x.HideNotification());
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
                fileNameOfTemporaryReport = String.Empty;

            string pathToXlsFile = @"Reporting\Generating\ExampleReports\2sheet5x5.xls";
            if (!File.Exists(pathToXlsFile))
            {
                Assert.Fail(String.Format("This test requires a report xls file located at {0}", pathToXlsFile));
            }

            m_ReportGeneration.Stub(x => x.GenerateAndSave(report, tempPath)).Return(new ReportGenerationOutcome(report.Name, fileNameOfTemporaryReport, pathToXlsFile));
        }

        private void StubReportFileForPrinting(IReport report = null)
        {
            m_ReportGeneration.Stub(x => x.Generate(report)).Return(new XlsFile(@"Reporting\Generating\ExampleReports\2sheet5x5.xls"));
        }

        private void StubTargetPathForReport(string path)
        {
            m_FilePathLogic.Stub(x => x.GetTargetPathForFile(FileDirectory.ProjectFiles)).Return(path);

        }

        private void StubNotEnoughDiskSpace()
        {
            m_ReportHandlingCF.DiskCheckerStub.Stub(
                x => x.ThereIsNotEnoughDiskSpaceForReport(Arg<IReport>.Is.Anything, Arg<string>.Is.Anything)).Return(
                    true);
        }

        private void StubEnoughDiskSpace()
        {
            m_ReportHandlingCF.DiskCheckerStub.Stub(
                x => x.ThereIsNotEnoughDiskSpaceForReport(Arg<IReport>.Is.Anything, Arg<string>.Is.Anything)).Return(
                    false);
        }

        private void AssertNotEnoughtDiskSpaceMessageWasShowed(string caption)
        {
            m_MessageBoxService.AssertWasCalled(x => x.Show(Arg<string>.Is.Equal(TextsCF.ReportGenerationFailedDiskSpace),
                Arg<string>.Is.Equal(caption), Arg<bool>.Is.Anything));
        }

        private string TempPath
        {
            get { return Path.GetTempPath(); }
        }

        private IReport ReportStub
        {
            get
            {
                IReport report = MockRepository.GenerateStub<IReport>();
                report.Name = "Report1";
                report.FileName = "Report1.xls";
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
            : base(MockRepository.GenerateStub<ReportDiskSpaceChecker>(filePathLogic),
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
