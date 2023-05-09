using System;
using System.IO;
using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Common.Utilities.Threading;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ReportingHandling = MockRepository.GenerateMock<IReportHandling>();
            m_FilePathLogic = MockRepository.GenerateStub<IFilePathLogic>();
            m_PlatformFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            TestHelper.AddServiceStub<IMessageBoxServiceCF>();

            m_PlatformFactoryService.Stub(x => x.Create<IReportHandling>()).Return(m_ReportingHandling);
            m_PlatformFactoryService.Stub(x => x.Create<IFilePathLogic>()).Return(m_FilePathLogic);
        }

        [Test]
        public void FiresEventWhenPrintedWithInformationAboutTheReportName()
        {
            string expectedReportName = "Report1";
            string actualReportName = string.Empty;

            IReport report = new Report("") { Name = expectedReportName };

            m_ReportingHandling.Stub(p => p.PrintAsync((Arg<IReport>.Is.Equal(report))))
                .Return(ITaskBuilder.FromCompleted())
                .WhenCalled(_ => m_ReportingHandling.Raise(
                    reportingHandling => reportingHandling.ReportPrinted += null,
                    m_ReportingHandling,
                    new ReportPrintedEventArgs(report.Name)));

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

            m_ReportingHandling.Stub(p => p.Save(Arg<IReport>.Is.Equal(report),Arg<String>.Is.Anything))
                .Return(ITaskBuilder.FromCompleted())
                .WhenCalled(_ => m_ReportingHandling.Raise(
                    reportingHandling => reportingHandling.ReportSaved += null,
                    m_ReportingHandling,
                    new ReportSavedEventArgs(report.Name, expectedGeneratedReportFileName)));

            m_FilePathLogic.Stub(p => p.CreateDirectoryIfItDoesNotExist(Arg<string>.Is.Anything)).Return(true);

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
            

            m_ReportingHandling.Stub(p => p.PrintAsync(Arg<IReport>.Is.Equal(report)))
                .Return(ITaskBuilder.FromCompleted())
                .WhenCalled(_ => m_ReportingHandling.Raise(
                    reportingHandling => reportingHandling.ReportGenerationFailed += null,
                    m_ReportingHandling,
                    expectedArgs));

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
            m_FilePathLogic.Stub(x => x.GetTargetPathForFile(FileDirectory.NotApplicable)).Return(String.Empty);

            report.Save(FileDirectory.NotApplicable);

            m_ReportingHandling.AssertWasNotCalled(x => x.Save(Arg<IReport>.Is.Anything, Arg<string>.Is.Anything));
        }

        [Test]
        public void CheckAndCreateReportDestinationDirectory()
        {
            IReport report = new Report("Report1.xls") { Name = "Report" };                        

            report.Save(Path.GetTempPath());

            m_FilePathLogic.AssertWasCalled(p => p.CreateDirectoryIfItDoesNotExist(Arg<string>.Is.NotNull));            
        }
    }

}