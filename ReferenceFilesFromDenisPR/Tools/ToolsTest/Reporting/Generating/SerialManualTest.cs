using System.IO;
using System.IO.Ports;
using System.Reflection;
using FlexCel.XlsAdapter;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Common.Printer.SerialPrinter;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Reporting.Generating
{
    /// <summary>
    /// These tests are considered manual no test fixture attribute here. 
    /// </summary>
    public class SerialManualTest
    {
        private readonly string m_ReportForSerialDevice =
            "Neo.ApplicationFramework.Tools.Reporting.Generating.ExampleReports.ReportForSerialDevice.xls";        
        
        private readonly string m_ReportExample1 =
            "Neo.ApplicationFramework.Tools.Reporting.Generating.ExampleReports.ReportExample1.xls";

        private IPrinterCF m_SerialPrinterCf;
        private IPrinterDevice m_PrinterDeviceStub;
        private ExcelToFlowDocumentConverter m_ExcelToFlowDocumentConverter;

        [SetUp]
        public void Setup()
        {
            // To run the tests you probably need to update the printer settings.
            // COM port for example is very likely change between computers.
            m_PrinterDeviceStub = Substitute.For<IPrinterDevice>();
            m_PrinterDeviceStub.COMPort = "COM4";

            // These settings are for the PayPrint EDUE-LPE58 printer, change if 
            // you are running something else.
            m_PrinterDeviceStub.BaudRate = 9600;
            m_PrinterDeviceStub.DataBit = 8;
            m_PrinterDeviceStub.StopBit = StopBits.One;
            m_PrinterDeviceStub.Handshake = HandshakeType.CTSRTS;
            m_PrinterDeviceStub.NewLineChar = NewLineCharType.CRLF;

            m_SerialPrinterCf = new SerialPrinterCF();
            m_SerialPrinterCf.Device = m_PrinterDeviceStub;

            m_ExcelToFlowDocumentConverter = new ExcelToFlowDocumentConverter();
        }

        public void PrintSimpleReport()
        {
            FlowDocument document = ConvertResource(m_ReportForSerialDevice);

            m_SerialPrinterCf.PrintAllPages(document);
        }      
        
        public void PrintReportExample1()
        {
            FlowDocument document = ConvertResource(m_ReportExample1);

            m_SerialPrinterCf.PrintAllPages(document);
        }

        private FlowDocument ConvertResource(string xlsResourceName)
        {
            return m_ExcelToFlowDocumentConverter.Convert(CreateExcelFile(xlsResourceName));
        }

        private XlsFile CreateExcelFile(string xlsResourceName)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xlsResourceName))
            {
                XlsFile xlsFile = new XlsFile();
                xlsFile.Open(resourceStream);
                return xlsFile;
            }
        }
    }
}
