using System.Drawing;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Common.Printer.GdiPrinter
{
    /// <summary>
    /// Test that you can use to manully test GDI printer code. Instead of using a printer
    /// the result will be drawn on a bitmap and show in a window. Note that this is not
    /// a regression test - merly an easy way to see what gets printed.
    /// </summary>
    [TestFixture]
    public class GdiPrinterCFTest
    {
        private IGdiWrapper m_GdiWrapperMock;

        private IGdiPrinterCF m_GdiPrinter;

        private FlowDocument m_TwoPageReport;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();

            m_GdiWrapperMock = Substitute.For<IGdiWrapper>();
            m_GdiWrapperMock.GetPageDimensions().Returns(new Rectangle(0, 0, 800, 800));

            m_GdiPrinter = new GdiPrinterCF(m_GdiWrapperMock);
            m_TwoPageReport = CreateTwoPageReport();
        }

        [Test]
        public void Should_start_document()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.Received().StartDocument(Arg.Any<IPrinterDevice>(), Arg.Any<OrientationType>());
        }

        [Test]
        public void Should_end_document()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.Received().EndDocument();
        }

        [Test]
        public void Should_end_document_when_errors_occur()
        {
            StubFailedGetTextExtentExPoint();

            Assert.Throws<PrintException>(() => m_GdiPrinter.PrintAllPages(m_TwoPageReport));
            
            m_GdiWrapperMock.Received().EndDocument();
        }

        [Test]
        public void Should_start_page_for_each_page()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.Received(2).StartPage();
        }

        [Test]
        public void Should_end_page_for_each_page()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.Received(2).EndPage();
        }

        private FlowDocument CreateTwoPageReport()
        {
            FlowDocument flowDocument = new FlowDocument();


            flowDocument.Blocks.Add(new Paragraph("Page1"));
            flowDocument.Blocks.Add(new Paragraph("Page2") { PageBreakBefore = true });

            return flowDocument;
        }

        private void StubFailedGetTextExtentExPoint()
        {
            Size extentSize = new Size();
            m_GdiWrapperMock
                .GetTextExtentExPoint(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), out Arg.Any<int>(), Arg.Any<int[]>(), out Arg.Any<Size>())
                .Returns(x => {
                    x[3] = 0;
                    x[5] = extentSize;
                    return false; 
                });
        }

        private void StubSucessfulGetTextExtentExPoint()
        {
            Size extentSize = new Size(200, 50);
            m_GdiWrapperMock
                .GetTextExtentExPoint(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), out Arg.Any<int>(), Arg.Any<int[]>(), out Arg.Any<Size>())
                .Returns(x => {
                    x[3] = 10;
                    x[5] = extentSize;
                    return true;
                });
        }
    }
}
