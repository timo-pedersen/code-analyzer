using System;
using Core.Api.DI.PlatformFactory;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Printer
{
	[TestFixture]
	public class PrinterTest
	{
		private IPrinterServiceCF m_PrinterServiceCF;
		private ITool m_PrinterToolCF;

	    private IGdiPrinterCF m_GdiPrinterCFMock;

	    private IMessageBoxServiceCF m_MessageBoxServiceCFMock;

	    private IDeviceManagerServiceCF m_DeviceManagerServiceCFMock;

	    [SetUp]
		public void Setup()
		{
            if (!ServiceContainerCF.Instance.IsServicePresent<IPrinterServiceCF>())
            {
                m_PrinterToolCF = new PrinterToolCF(); 
                m_PrinterToolCF.Owner = ServiceContainerCF.Instance;
                m_PrinterToolCF.RegisterServices();
            }
            
            IPrinterDevice printerDeviceMock = MockRepository.GenerateMock<IPrinterDevice>();
            
            m_DeviceManagerServiceCFMock = MockRepository.GenerateMock<IDeviceManagerServiceCF>();
            m_DeviceManagerServiceCFMock.Stub(m => m.GetOutputDevice<IPrinterDevice>()).Return(printerDeviceMock);

		    m_GdiPrinterCFMock = MockRepository.GenerateMock<IGdiPrinterCF>();

            IPlatformFactoryService platformFactoryServiceMock = MockRepository.GenerateMock<IPlatformFactoryService>();
            platformFactoryServiceMock.Stub(m => m.Create<IGdiPrinterCF>()).Return(m_GdiPrinterCFMock);

            m_MessageBoxServiceCFMock = MockRepository.GenerateMock<IMessageBoxServiceCF>();

            ServiceContainerCF.Instance.AddService(typeof(IDeviceManagerServiceCF), m_DeviceManagerServiceCFMock);
            ServiceContainerCF.Instance.AddService(typeof(IPlatformFactoryService), platformFactoryServiceMock);
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), m_MessageBoxServiceCFMock);

            m_PrinterServiceCF = ServiceContainerCF.GetService<IPrinterServiceCF>();
		}

		[Test]
		public void Should_fail_print_when_print_all_pages_fail()
        {
            
            m_GdiPrinterCFMock
                .Stub(m => m.PrintAllPages(null))
                .IgnoreArguments()
                .Throw(new ArgumentException());

            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());
            
            Assert.Throws<ArgumentException>(() => printTask.Wait());
        }
        
        [Test]
		public void Should_succeed_when_print_completes_successfully()
        {
            m_GdiPrinterCFMock.Stub(m => m.PrintAllPages(null)).IgnoreArguments();

            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());
            
            Assert.DoesNotThrow(() => printTask.Wait());
        }

        [Test]
        public void Should_show_error_message_when_output_device_is_not_configured()
        {
            m_DeviceManagerServiceCFMock
                .Stub(m => m.GetOutputDevice<IPrinterDevice>()).Return(null)
                .Repeat.Any(); // To reset mock

            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());

            m_MessageBoxServiceCFMock.AssertWasCalled(m => m.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<System.Action>.Is.Null));
        }
    }
}
