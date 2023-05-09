using System;
using System.ComponentModel;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.Tools.Report;
using Neo.ApplicationFramework.Resources.Texts;


namespace Neo.ApplicationFramework.Tools.Reporting
{
    [TestFixture]
    public class ReportViewModelTest
    {
        [Test]
        public void ReportFormatDisplayNameTest()
        {
            IReport model = new TestReport();
            ReportViewModel reportViewModel = new ReportViewModel(model);

            model.ReportFormat = ReportFormats.Excel;
            Assert.AreEqual(reportViewModel.ReportFormatDisplayName, TextsIde.ReportFormatExcel);

            model.ReportFormat = ReportFormats.Pdf;
            Assert.AreEqual(reportViewModel.ReportFormatDisplayName, TextsIde.ReportFormatPdf);

            model.ReportFormat = ReportFormats.Both;
            Assert.AreEqual(reportViewModel.ReportFormatDisplayName, TextsIde.ReportFormatBoth);
        }

        [Test]
        public void ReportFormatTest()
        {
            IReport model = new TestReport();
            ReportViewModel reportViewModel = new ReportViewModel(model);

            reportViewModel.ReportFormatDisplayName = TextsIde.ReportFormatExcel;
            Assert.AreEqual(model.ReportFormat, ReportFormats.Excel);

            reportViewModel.ReportFormatDisplayName = TextsIde.ReportFormatPdf;
            Assert.AreEqual(model.ReportFormat, ReportFormats.Pdf);

            reportViewModel.ReportFormatDisplayName = TextsIde.ReportFormatBoth;
            Assert.AreEqual(model.ReportFormat, ReportFormats.Both);

            Assert.Throws<ArgumentException>(() => reportViewModel.ReportFormatDisplayName = "Another format");
        }

        private sealed class TestReport : IReport
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public ISite Site { get; set; }
            public string FileName { get; set; }
            public void Print()
            {
                throw new NotImplementedException();
            }

            public void Save(string destinationPath)
            {
                throw new NotImplementedException();
            }

            public void Save(FileDirectory fileDirectory)
            {
                throw new NotImplementedException();
            }

            public string Name { get; set; }
            public ReportFormats ReportFormat { get; set; }
            public event EventHandler<ReportSavedEventArgs> ReportSaved;
            public event EventHandler<ReportPrintedEventArgs> ReportPrinted;
            public event EventHandler<ReportGenerationFailedEventArgs> ReportGenerationFailed;

            public event EventHandler Disposed;

            private void OnReportGenerationFailed(ReportGenerationFailedEventArgs e)
            {
                var handler = ReportGenerationFailed;
                if (handler != null)
                    handler(this, e);
            }

            private void OnReportSaved(ReportSavedEventArgs e)
            {
                var handler = ReportSaved;
                if (handler != null)
                    handler(this, e);
            }

            private  void OnReportPrinted(ReportPrintedEventArgs e)
            {
                var handler = ReportPrinted;
                if (handler != null)
                    handler(this, e);
            }

            private  void OnDisposed()
            {
                var handler = Disposed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            #region Implementation of IPublic

            public bool IsPublic { get; set; }

            #endregion

            #region Implementation of IProjectGuid

            public Guid ProjectGuid { get; set; }

            #endregion
        }
    }
}