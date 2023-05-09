using System.Drawing;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

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

            m_GdiWrapperMock = MockRepository.GenerateMock<IGdiWrapper>();
            m_GdiWrapperMock.Stub(m => m.GetPageDimensions()).Return(new Rectangle(0, 0, 800, 800));

            m_GdiPrinter = new GdiPrinterCF(m_GdiWrapperMock);
            m_TwoPageReport = CreateTwoPageReport();
        }

        [Test]
        public void Should_start_document()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.AssertWasCalled(x => x.StartDocument(
                Arg<IPrinterDevice>.Is.Anything,
                Arg<OrientationType>.Is.Anything));
        }

        [Test]
        public void Should_end_document()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.AssertWasCalled(x => x.EndDocument());
        }      
        
        [Test]
        public void Should_end_document_when_errors_occur()
        {
            StubFailedGetTextExtentExPoint();

            Assert.Throws<PrintException>(() => m_GdiPrinter.PrintAllPages(m_TwoPageReport));
            
            m_GdiWrapperMock.AssertWasCalled(x => x.EndDocument());
        }

        [Test]
        public void Should_start_page_for_each_page()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.AssertWasCalled(x => x.StartPage(), options => options.Repeat.Twice());
        }

        [Test]
        public void Should_end_page_for_each_page()
        {
            StubSucessfulGetTextExtentExPoint();

            m_GdiPrinter.PrintAllPages(m_TwoPageReport);

            m_GdiWrapperMock.AssertWasCalled(x => x.EndPage(), options => options.Repeat.Twice());
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
                .Stub(x => x.GetTextExtentExPoint(
                    Arg<string>.Is.Anything,
                    Arg<int>.Is.Anything,
                    Arg<int>.Is.Anything,
                    out Arg<int>.Out(0).Dummy,
                    Arg<int[]>.Is.Anything,
                    out Arg<Size>.Out(extentSize).Dummy))
                .Return(false);
        }

        private void StubSucessfulGetTextExtentExPoint()
        {
            Size extentSize = new Size(200, 50);
            m_GdiWrapperMock
                .Stub(x => x.GetTextExtentExPoint(
                    Arg<string>.Is.Anything,
                    Arg<int>.Is.Anything,
                    Arg<int>.Is.Anything,
                    out Arg<int>.Out(10).Dummy,
                    Arg<int[]>.Is.Anything,
                    out Arg<Size>.Out(extentSize).Dummy))
                .Return(true);
        }
    }
}
