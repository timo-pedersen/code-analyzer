using System;
using System.ComponentModel;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportsTest
    {
        private Reports m_ReportsUnderTest;

        private IReport ReportStub
        {
            get { return Substitute.For<IReport>(); }
        }

        [SetUp]
        public virtual void SetUp()
        {
            m_ReportsUnderTest = new Reports();

            TestHelper.ClearServices();

            TestHelper.AddServiceStub<IMessageBoxServiceCF>();
        }

        [Test]
        public void CanPrintAnExistingReport()
        {
            string existingReportName = "I do exist!";
            IReport existingReport = ReportStub.With(rep => rep.Name = existingReportName);
            m_ReportsUnderTest.ReportItems.Add(existingReport);

            m_ReportsUnderTest.PrintReport(existingReportName);

            existingReport.Received().Print();
        }

        [Test]
        public void DoesNotTryToPrintAnReportThatDoesNotExist()
        {
            IReport existingReport = ReportStub.With(rep => rep.Name = "I do exist!");
            m_ReportsUnderTest.ReportItems.Add(existingReport);

            m_ReportsUnderTest.PrintReport("I doesnt exist!");

            existingReport.DidNotReceive().Print();
        }

        [Test]
        public void CanSaveAReportWhenItsFound()
        {
            string existingReportName = "I do exist!";
            IReport existingReport = ReportStub.With(rep => rep.Name = existingReportName);
            m_ReportsUnderTest.ReportItems.Add(existingReport);

            m_ReportsUnderTest.SaveReport(existingReportName, FileDirectory.USB);

            existingReport.Received().Save(FileDirectory.USB);
        }

        [Test]
        public void DoesNotTryToSaveAReportThatDoesNotExist()
        {
            IReport existingReport = ReportStub.With(rep => rep.Name = "I do exist!");
            m_ReportsUnderTest.ReportItems.Add(existingReport);

            m_ReportsUnderTest.SaveReport("I doesnt exist!", FileDirectory.USB);

            existingReport.DidNotReceive().Save(FileDirectory.USB);
        }

        [Test]
        public void NotifiesWhenReportItemsChanges()
        {
            IReport firstReport = ReportStub.With(rep => rep.Name = "FirstReport");
            m_ReportsUnderTest.ReportItems.Add(firstReport);

            Assert.That(m_ReportsUnderTest.NotifiesOn(x => x.ReportItems)
                .When(x => x.ReportItems.Add(ReportStub.With(rep => rep.Name = "SecondReport")))
                , Is.True);

            Assert.That(m_ReportsUnderTest.NotifiesOn(x => x.ReportItems)
              .When(x => x.ReportItems.Remove(firstReport))
              , Is.True);
        }
    }

}