using System;
using System.Linq;
using System.Xml.Linq;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Common.AlarmDistributorServer;
using Neo.ApplicationFramework.Tools.Printer;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    public class XmlPrinterDeviceSettingsSplitConverterTest : XamlConverterBaseTest
    {
        private XmlTypeSerializer m_XmlTypeSerializerMock;
        private XmlPrinterDeviceSettingsSplitConverter m_XmlPrinterDeviceSettingsSplitConverter;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_XmlPrinterDeviceSettingsSplitConverter = new XmlPrinterDeviceSettingsSplitConverter();
            m_XmlConverterManager.RegisterConverter(m_XmlPrinterDeviceSettingsSplitConverter);

            m_XmlTypeSerializerMock = Substitute.For<XmlTypeSerializer>();
            m_XmlPrinterDeviceSettingsSplitConverter.XmlTypeSerializer = m_XmlTypeSerializerMock;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void ConfirmThatPropertiesAreRemovedAndSavedIntoNewFile()
        {

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var xDoc = XDocument.Parse(FileResources.PrinterDeviceWithAlarmDistributorSettings);
            bool isConverted = m_XmlConverterManager.ConvertConfigurationFile(new Version(0, 0), path, xDoc);
            
            Assert.That(isConverted, Is.True);
            Assert.IsFalse(PropertyExists("Body", xDoc));
            Assert.IsFalse(PropertyExists("FontSize", xDoc));
            Assert.IsFalse(PropertyExists("IsBufferRequired", xDoc));
            Assert.IsFalse(PropertyExists("BufferWaitingTime", xDoc));
            Assert.IsFalse(PropertyExists("BufferMaxAlarms", xDoc));

            m_XmlTypeSerializerMock.Received().Save(Arg.Any<string>(), Arg.Any<PrinterDistributor>());
        }

        private bool PropertyExists(string propertyName, XDocument document)
        {
            return document.Descendants().Where(x => x.Name == propertyName).Count() > 0;
        }

        [Test]
        public void ConfirmThatSectionIsCreatedWhenNoPrinterExists()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var xDoc = XDocument.Parse(FileResources.ProjectWithoutDeviceSection);
            bool isConverted = m_XmlConverterManager.ConvertProject(path, xDoc);
            Assert.That(isConverted, Is.True);

            var devices = from item in xDoc.Descendants("Object")
                          let designerTypeNameAttribute = item.Attribute("Name")
                          where designerTypeNameAttribute != null && designerTypeNameAttribute.Value == "Devices"
                          select item;

            Assert.That(devices.Count() > 0);
            m_XmlTypeSerializerMock.Received().Save(Arg.Any<string>(), Arg.Any<PrinterDevice>());
        }

        private int GetNumberOfProperties(XDocument xDocument)
        {
            return xDocument.Root.Elements().Count();
        }
    }
}
