using System.Linq;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Printer.SerialPrinter
{
    [TestFixture]
    public class SerialPrinterCFTest
    {
        private FlowDocument m_ThreeLinesText;
        private FlowDocument m_5x5Table;
        private IPrinterDevice m_PrinterDeviceStub;
        private IPrinterCF m_SerialPrinter;
        private SerialPortFactory m_SerialPortFactoryMock;
        private SerialPortRecorder m_SerialPortRecorder;

        [SetUp] 
        public void Setup()
        {
            m_PrinterDeviceStub = Substitute.For<IPrinterDevice>();
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.LF;
            m_SerialPortRecorder = new SerialPortRecorder();

            m_SerialPortFactoryMock = Substitute.For<SerialPortFactory>();
            m_SerialPortFactoryMock.Create(null).ReturnsForAnyArgs(m_SerialPortRecorder);

            m_SerialPrinter = new SerialPrinterCF(m_SerialPortFactoryMock);
            m_SerialPrinter.Device = m_PrinterDeviceStub;

            m_ThreeLinesText = new FlowDocument();
            m_ThreeLinesText.Blocks.Add(new Paragraph("one\ntwo\nthree"));

            m_5x5Table = CreateTable(5,5);
        }

        private FlowDocument CreateTable(int cols, int rows)
        {
            Table table = new Table();

            for (int colIndex = 0; colIndex < cols; colIndex++)
            {
                table.Columns.Add(new TableColumn());   
            }

            for (int rowIndex = 1; rowIndex <= rows; rowIndex++)
            {
                TableRow tableRow = new TableRow();

                for (int colIndex = 1; colIndex <= cols; colIndex++)
                {
                    tableRow.Cells.Add(new TableCell(new Paragraph(rowIndex + "." + colIndex)));    
                }
                
                table.TableRowGroup.Add(tableRow);
            }

            FlowDocument flowDocument = new FlowDocument();
            flowDocument.Blocks.Add(table);
            return flowDocument;
        }

        [Test]
        public void Should_replace_newline_with_carrage_return()
        {
            // Arrange
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.CR;
            
            // Act
            m_SerialPrinter.PrintAllPages(m_ThreeLinesText);
            var output = m_SerialPortRecorder.GetRecordedWrites();

            // Assert
            Assert.That(output, Does.StartWith("one\rtwo\rthree"));
        }
        
        [Test]
        public void Should_replace_newline_with_linefeed()
        {
            // Arrange
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.LF;
            
            // Act
            m_SerialPrinter.PrintAllPages(m_ThreeLinesText);
            var output = m_SerialPortRecorder.GetRecordedWrites();

            // Assert
            Assert.That(output, Does.StartWith("one\ntwo\nthree"));
        }
        
        [Test]
        public void Should_replace_newline_with_carrage_return_and_linefeed()
        {
            // Arrange
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.CRLF;
            
            // Act
            m_SerialPrinter.PrintAllPages(m_ThreeLinesText);
            var output = m_SerialPortRecorder.GetRecordedWrites();

            // Assert
            Assert.That(output, Does.StartWith("one\r\ntwo\r\nthree"));
        }
        
        [Test]
        public void Should_end_each_paragraph_with_two_configured_newlines()
        {
            // Arrange
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.LF;
            
            // Act
            m_SerialPrinter.PrintAllPages(m_ThreeLinesText);
            var output = m_SerialPortRecorder.GetRecordedWrites();

            // Assert
            Assert.That(output, Does.EndWith("\n\n"));
        }

        [Test]
        public void Should_print_tables()
        {
            m_SerialPrinter.PrintAllPages(m_5x5Table);

            Assert.That(m_SerialPortRecorder.GetRecordedWrites(), Is.Not.Empty);
        }

        [Test]
        public void Should_print_tables_formatted()
        {
            m_SerialPrinter.PrintAllPages(m_5x5Table);

            // Min col width is 4 + 8 in cell padding => 9 spaces
            string expectedFormat =
                "1.1     1.2     1.3     1.4     1.5 \n" +
                "2.1     2.2     2.3     2.4     2.5 \n" +
                "3.1     3.2     3.3     3.4     3.5 \n" +
                "4.1     4.2     4.3     4.4     4.5 \n" +
                "5.1     5.2     5.3     5.4     5.5 \n";
                
            Assert.That(m_SerialPortRecorder.GetRecordedWrites(), Does.StartWith(expectedFormat));
        }
        
        [Test]
        public void Should_span_colspan_cells_over_columns()
        {
            var spanningCell = new TableCell(new Paragraph("000000000000"));
            spanningCell.ColSpan = 1;
            var table = m_5x5Table.Blocks.First() as Table;
            table.TableRowGroup[1].Cells[0] = spanningCell;
            
            m_SerialPrinter.PrintAllPages(m_5x5Table);

            string expectedFormat =
                "1.1     1.2     1.3     1.4     1.5 \n" +
                "000000000000    2.3     2.4     2.5 \n" +
                "3.1     3.2     3.3     3.4     3.5 \n" +
                "4.1     4.2     4.3     4.4     4.5 \n" +
                "5.1     5.2     5.3     5.4     5.5 \n";
                
            Assert.That(m_SerialPortRecorder.GetRecordedWrites(), Does.StartWith(expectedFormat));
        }        
        
        [Test]
        public void Should_adapt_cell_widths_to_longest_cell_when_no_col_span_is_specified()
        {
            var spanningCell = new TableCell(new Paragraph("00000000000000000000"));
            var table = m_5x5Table.Blocks.First() as Table;
            table.TableRowGroup[1].Cells[0] = spanningCell;
            
            m_SerialPrinter.PrintAllPages(m_5x5Table);

            string expectedFormat =
                "1.1                     1.2     1.3     1.4     1.5 \n" +
                "00000000000000000000    2.2     2.3     2.4     2.5 \n" +
                "3.1                     3.2     3.3     3.4     3.5 \n" +
                "4.1                     4.2     4.3     4.4     4.5 \n" +
                "5.1                     5.2     5.3     5.4     5.5 \n";
                
            Assert.That(m_SerialPortRecorder.GetRecordedWrites(), Does.StartWith(expectedFormat));
        }
    }
}