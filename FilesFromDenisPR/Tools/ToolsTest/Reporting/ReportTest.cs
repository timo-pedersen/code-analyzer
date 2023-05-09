using System;
using System.IO;
using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Common.Utilities.Threading;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportTest
    {
        private IReportHandling m_ReportingHandling;
        private IPlatformFactoryService m_PlatformFactoryService;
        private IFilePathLogic m_FilePathLogic;

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.ClearServices();
            m_ReportingHandling = Substitute.For<IReportHandling>();
            m_FilePathLogic = Substitute.For<IFilePathLogic>();
            m_PlatformFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            TestHelper.AddServiceStub<IMessageBoxServiceCF>();

            m_PlatformFactoryService.Create<IReportHandling>().Returns(m_ReportingHandling);
            m_PlatformFactoryService.Create<IFilePathLogic>().Returns(m_FilePathLogic);
        }

        [Test]
        public void FiresEventWhenPrintedWithInformationAboutTheReportName()
        {
            string expectedReportName = "Report1";
            string actualReportName = string.Empty;

            IReport report = new Report("") { Name = expectedReportName };

            m_ReportingHandling.PrintAsync(Arg.Is(report))
                .Returns(x =>
                {
                    Raise.EventWith(m_ReportingHandling, new ReportPrintedEventArgs(report.Name));
                    return ITaskBuilder.FromCompleted();
                });

            report.ReportPrinted += (sender, e) => { actualReportName = e.ReportName;};

            report.Print();

            Assert.That(expectedReportName, Is.EqualTo(actualReportName));
        }

        [Test]
        public void FiresEventWhenSavedWithInformationAboutTheReportName()
        {
            string expectedReportName = "Report1";
            string expectedGeneratedReportFileName = "Report1.xls";
            IReport actualReport = new Report();

            IReport report = new Report(string.Empty) { Name = expectedReportName };

            m_ReportingHandling.Save(Arg.Is(report), Arg.Any<string>())
                .Returns(x =>
                 {
                     Raise.EventWith(m_ReportingHandling, new ReportSavedEventArgs(report.Name, expectedGeneratedReportFileName));
                     return ITaskBuilder.FromCompleted();
                 });

            m_FilePathLogic.CreateDirectoryIfItDoesNotExist(Arg.Any<string>()).Returns(true);

            report.ReportSaved += (sender, e) =>
                                      {
                                          actualReport.Name = e.ReportName;
                                          actualReport.FileName = e.GeneratedReportFileName;
                                      };

            report.Save(Path.GetTempPath());

            Assert.That(expectedReportName, Is.EqualTo(actualReport.Name));
            Assert.That(expectedGeneratedReportFileName, Is.EqualTo(actualReport.FileName));
        }

        [Test]
        public void FiresEventWhenReportGenerationFails()
        {
            IReport report = new Report("Report1");
            ReportGenerationFailedEventArgs actualArgs = null;
            ReportGenerationFailedEventArgs expectedArgs = new ReportGenerationFailedEventArgs(GenerateReportAction.Print,
                report.Name, "", "Failed!",new ArgumentException("Fail!"));
            

            m_ReportingHandling.PrintAsync(Arg.Is(report))
                .Returns(x =>
                    {
                        Raise.EventWith(m_ReportingHandling, expectedArgs);
                        return ITaskBuilder.FromCompleted();
                    });

            report.ReportGenerationFailed += (sender, eventArgs) => {
                                                     actualArgs = eventArgs;
                                                 };

            report.Print();

            Assert.That(actualArgs, Is.EqualTo(expectedArgs));
        }

        [Test]
        public void DoesNotSaveReportWhenDestinationDirectoryInvalid()
        {
            IReport report = new Report("Report1.xls") { Name = "Report" };
            m_FilePathLogic.GetTargetPathForFile(FileDirectory.NotApplicable).Returns(String.Empty);

            report.Save(FileDirectory.NotApplicable);

            m_ReportingHandling.DidNotReceiveWithAnyArgs().Save(Arg.Any<IReport>(), Arg.Any<string>());
        }

        [Test]
        public void CheckAndCreateReportDestinationDirectory()
        {
            IReport report = new Report("Report1.xls") { Name = "Report" };

            report.Save(Path.GetTempPath());

            m_FilePathLogic.Received().CreateDirectoryIfItDoesNotExist(Arg.Is<string>(x=> x != null));
        }
    }
}