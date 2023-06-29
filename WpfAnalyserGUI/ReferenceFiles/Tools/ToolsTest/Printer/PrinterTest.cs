using System;
using Core.Api.DI.PlatformFactory;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Printer;
using NSubstitute;
using NUnit.Framework;

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
            
            IPrinterDevice printerDeviceMock = Substitute.For<IPrinterDevice>();
            
            m_DeviceManagerServiceCFMock = Substitute.For<IDeviceManagerServiceCF>();
            m_DeviceManagerServiceCFMock.GetOutputDevice<IPrinterDevice>().Returns(printerDeviceMock);

		    m_GdiPrinterCFMock = Substitute.For<IGdiPrinterCF>();

            IPlatformFactoryService platformFactoryServiceMock = Substitute.For<IPlatformFactoryService>();
            platformFactoryServiceMock.Create<IGdiPrinterCF>().Returns(m_GdiPrinterCFMock);

            m_MessageBoxServiceCFMock = Substitute.For<IMessageBoxServiceCF>();

            ServiceContainerCF.Instance.AddService(typeof(IDeviceManagerServiceCF), m_DeviceManagerServiceCFMock);
            ServiceContainerCF.Instance.AddService(typeof(IPlatformFactoryService), platformFactoryServiceMock);
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), m_MessageBoxServiceCFMock);

            m_PrinterServiceCF = ServiceContainerCF.GetService<IPrinterServiceCF>();
		}

		[Test]
		public void Should_fail_print_when_print_all_pages_fail()
        {
            
            m_GdiPrinterCFMock.WhenForAnyArgs(x => x.PrintAllPages(Arg.Any<IFlowDocument>()))
                .Do(y => throw new ArgumentException());

            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());
            
            Assert.Throws<ArgumentException>(() => printTask.Wait());
        }
        
        [Test]
		public void Should_succeed_when_print_completes_successfully()
        {
            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());
            
            Assert.DoesNotThrow(() => printTask.Wait());
            m_GdiPrinterCFMock.ReceivedWithAnyArgs().PrintAllPages(Arg.Any<IFlowDocument>());
        }

        [Test]
        public void Should_show_error_message_when_output_device_is_not_configured()
        {
            m_DeviceManagerServiceCFMock.GetOutputDevice<IPrinterDevice>().Returns(x => null);

            IPrinterServiceCF printerToolCF = new PrinterToolCF();
            ITask printTask = printerToolCF.SendToPrinterAsync(new FlowDocument());

            m_MessageBoxServiceCFMock.Received().Show(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), null);
        }
    }
}
