using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.OpcUaServer;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUaServer
{
    [TestFixture]
    public class OpcUaServerConfigurationManagerTest
    {
        private const string CompanyName = "Beijer Electronics AB";
        private const string Product = "iX Developer";
        private const string Version = "1.40";
        private const string Build = "1.40.498.0";

        private XDocument m_Document;
        private IOpcUaServerRootComponent m_OpcUaServerRootComponentStub;
        private IBrandService m_BrandServiceStub;

        [SetUp]
        public void SetUp()
        {
            m_Document = new XDocument();
            InitDocument(ref m_Document, "Neo.ApplicationFramework.Tools.OpcUaServer.Configuration.OpcUaServerConfig.xml");

            m_OpcUaServerRootComponentStub = Substitute.For<IOpcUaServerRootComponent>();
            m_OpcUaServerRootComponentStub.Port = 4842;
            m_OpcUaServerRootComponentStub.AllowAnonymousLogin = true;
            m_OpcUaServerRootComponentStub.IPAddresses.Returns(new string[] { "1.2.3.4", "5.6.7.8" });

            m_BrandServiceStub = Substitute.For<IBrandService>();
            m_BrandServiceStub.CompanyName.Returns(CompanyName);
            m_BrandServiceStub.ProductName.Returns(Product);
            m_BrandServiceStub.Version.Returns(Version);
            m_BrandServiceStub.Build.Returns(Build);
        }

        [Test]
        public void InitialConfigurationeHasExpectedElements()
        {
            IEnumerable<System.Xml.Linq.XElement> uaEndpointTemplateElements = UaEndpointTemplateElements;
            Assert.That(uaEndpointTemplateElements.Count(), Is.EqualTo(1));

            IEnumerable<System.Xml.Linq.XElement> uaEndpointElements = UaEndpointElements;
            Assert.That(uaEndpointElements.Count(), Is.EqualTo(0));

            var userIdentityTokenElements = UserIdentityTokenElements;

            var enableAnonymousElements = userIdentityTokenElements.Elements("EnableAnonymous");
            Assert.That(enableAnonymousElements.Count(), Is.EqualTo(1));
            Assert.That(enableAnonymousElements.First().Value, Is.EqualTo("true"));

            var enableUserPw = userIdentityTokenElements.Elements("EnableUserPw");
            Assert.That(enableUserPw.Count(), Is.EqualTo(1));
            Assert.That(enableUserPw.First().Value, Is.EqualTo("false"));

            VerifySingleChildElementOfUaServerConfig("ApplicationUri", string.Empty);
            VerifySingleChildElementOfUaServerConfig("ManufacturerName", string.Empty);
            VerifySingleChildElementOfUaServerConfig("ApplicationName", string.Empty);
            VerifySingleChildElementOfUaServerConfig("SoftwareVersion", string.Empty);
            VerifySingleChildElementOfUaServerConfig("BuildNumber", string.Empty);

            VerifySingleChildElementOfUaServerConfig("ServerUri", string.Empty);
            VerifySingleChildElementOfUaServerConfig("ServerName", string.Empty);
        }

        [TestCase(4841)]
        [TestCase(4842)]
        public void UaEndpointsSetupCorrectly(int port)
        {
            m_OpcUaServerRootComponentStub.Port = port;
            CreateConfigurationManagerAndModifyDocument();

            var uaEndpointElements = UaEndpointElements;
            Assert.That(uaEndpointElements.Count(), Is.EqualTo(2));

            var urlElements = uaEndpointElements.Elements("Url");
            Assert.That(urlElements.Count(), Is.EqualTo(2));
            Assert.That(urlElements.Where(x => x.Value.Equals("opc.tcp://[NodeName]:" + port)).Count(), Is.EqualTo(2));

            var stackUrlElements = uaEndpointElements.Elements("StackUrl");
            Assert.That(stackUrlElements.Count(), Is.EqualTo(2));
            Assert.That(stackUrlElements.Where(x => x.Value.Equals("opc.tcp://1.2.3.4:" + port)).Count(), Is.EqualTo(1));
            Assert.That(stackUrlElements.Where(x => x.Value.Equals("opc.tcp://5.6.7.8:" + port)).Count(), Is.EqualTo(1));

            // Certificate configuration
            foreach (XElement uaEndpointElement in uaEndpointElements)
            {
                var elements = uaEndpointElement.Elements("CertificateStore").Elements("CertificateSettings").Elements("Organization");
                Assert.That(elements.Count(), Is.EqualTo(1));
                Assert.That(elements.First().Value, Is.EqualTo(m_BrandServiceStub.CompanyName));
            }
        }

        [TestCase(true, "true", "false")]
        [TestCase(false, "false", "true")]
        public void UserIdentityTokensSetupCorrectly(bool allowAnonymousLogin, string expectedEnableAnonymous, string expectedEnableUserPw)
        {
            m_OpcUaServerRootComponentStub.AllowAnonymousLogin = allowAnonymousLogin;
            CreateConfigurationManagerAndModifyDocument();

            var userIdentityTokenElements = UserIdentityTokenElements;

            var enableAnonymousElements = userIdentityTokenElements.Elements("EnableAnonymous");
            Assert.That(enableAnonymousElements.Count(), Is.EqualTo(1));
            Assert.That(enableAnonymousElements.First().Value, Is.EqualTo(expectedEnableAnonymous));

            var enableUserPw = userIdentityTokenElements.Elements("EnableUserPw");
            Assert.That(enableUserPw.Count(), Is.EqualTo(1));
            Assert.That(enableUserPw.First().Value, Is.EqualTo(expectedEnableUserPw));
        }

        [Test]
        public void BuildInformationSetupCorrectly()
        {
            CreateConfigurationManagerAndModifyDocument();

            VerifySingleChildElementOfUaServerConfig("ApplicationUri", string.Format("urn:{0}:{1}", m_BrandServiceStub.CompanyName, m_BrandServiceStub.ProductName));
            VerifySingleChildElementOfUaServerConfig("ManufacturerName", m_BrandServiceStub.CompanyName);
            VerifySingleChildElementOfUaServerConfig("ApplicationName", m_BrandServiceStub.ProductName);
            VerifySingleChildElementOfUaServerConfig("SoftwareVersion", m_BrandServiceStub.Version);
            VerifySingleChildElementOfUaServerConfig("BuildNumber", m_BrandServiceStub.Build);
        }

        [Test]
        public void ServerInstanceSetupCorrectly()
        {
            CreateConfigurationManagerAndModifyDocument();

            VerifySingleChildElementOfUaServerConfig("ServerUri", string.Format("urn:[NodeName]:{0}:{1}", m_BrandServiceStub.CompanyName, m_BrandServiceStub.ProductName));
            VerifySingleChildElementOfUaServerConfig("ServerName", string.Format("{0}@[NodeName]", m_BrandServiceStub.ProductName));
        }

        private void InitDocument(ref XDocument document, string resourceName)
        {
            using (Stream stream = Assembly.LoadFrom("ToolsIde.DLL").GetManifestResourceStream(resourceName))
            using (XmlReader xmlReader = XmlReader.Create(stream))
            {
                document = XDocument.Load(xmlReader);
            }
        }

        private void VerifySingleChildElementOfUaServerConfig(string elementName, string expectedValue)
        {
            var elements = UaServerConfigElement.Elements(elementName);
            Assert.That(elements.Count(), Is.EqualTo(1));
            Assert.That(elements.First().Value, Is.EqualTo(expectedValue));
        }

        private void CreateConfigurationManagerAndModifyDocument()
        {
            OpcUaServerConfigurationManager opcUaServerConfigurationManager = new OpcUaServerConfigurationManager(m_OpcUaServerRootComponentStub, m_BrandServiceStub);
            opcUaServerConfigurationManager.ModifyConfigurationFile(m_Document);
        }

        private XElement UaServerConfigElement
        {
            get { return m_Document.Elements("OpcServerConfig").Elements("UaServerConfig").First(); }
        }

        private IEnumerable<XElement> UaEndpointTemplateElements
        {
            get { return UaServerConfigElement.Elements("UaEndpointTemplate"); }
        }

        private IEnumerable<XElement> UaEndpointElements
        {
            get { return UaServerConfigElement.Elements("UaEndpoint"); }
        }

        private IEnumerable<XElement> UserIdentityTokenElements
        {
            get { return UaServerConfigElement.Elements("UserIdentityTokens"); }
        }
    }
}
