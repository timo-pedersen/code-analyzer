using System;
using Neo.ApplicationFramework.Interfaces.Reporting;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportItemsTest
    {
        [Test]
        public void FindingReportByNameIsNotCaseSensetive()
        {
            string reportName = "RePoRt!";
            IReport reportToFind = MockRepository.GenerateStub<IReport>().With(rep => rep.Name = reportName);

            var reportItems = new ReportOwnedList(new Reports())
            {
                reportToFind
            };

            Assert.That(reportItems.FindByReportName(reportName.ToLower()), Is.SameAs(reportToFind));
        }

        [Test]
        public void FindingReportByNameReturnsNullWhenReportIsntFound()
        {
            var reportItems = new ReportOwnedList(new Reports());

            Assert.That(reportItems.FindByReportName("NoReport!"),Is.Null);
        }
    }

}