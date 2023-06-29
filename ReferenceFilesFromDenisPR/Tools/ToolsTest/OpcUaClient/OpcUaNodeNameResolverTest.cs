#if !VNEXT_TARGET
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUaClient
{
    [TestFixture]
    internal class OpcUaNodeNameResolverTest
    {
        private OpcUaNodeNameResolver m_OpcUaNodeNameResolver;

        [SetUp]
        public void SetUp()
        {
            IDataSourceContainer dataSourceContainerStub = Substitute.For<IDataSourceContainer>();
            dataSourceContainerStub.OpcUaDefaultNamespaceName = "NS1";
            dataSourceContainerStub.OpcUaNamespaceNameBrowseNameSeparator = ':';
            IOpcUaNamespaceInfos namespaceInfos = new OpcUaNamespaceInfos();
            namespaceInfos.Add(new OpcUaNamespaceInfo() { Name = "NS1", Uri = "URI1" });
            namespaceInfos.Add(new OpcUaNamespaceInfo() { Name = "NS2", Uri = "URI2" });
            dataSourceContainerStub.OpcUaNamespaceInfos = namespaceInfos;

            m_OpcUaNodeNameResolver = new OpcUaNodeNameResolver(dataSourceContainerStub);
        }

        [Test]
        [TestCase("NS1:D0", "D0")]
        [TestCase("NS2:D0", "D0")]
        [TestCase("NS1:Folder1:D0", "Folder1:D0")]
        [TestCase("NS2:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        [TestCase("NS2:string:Folder1:Folder2:D0", "string:Folder1:Folder2:D0")] 

        [TestCase("*string:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        [TestCase("*String:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        [TestCase("*NS2:String:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        [TestCase("*NS2:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        [TestCase("*NS1:Folder1:Folder2:D0", "Folder1:Folder2:D0")]
        public void CorrectAddressStringsReturned(string dataItemId, string expectedBrowseName)
        {
            string addressString = m_OpcUaNodeNameResolver.GetAddressValueString(dataItemId);
            Assert.That(addressString, Is.EqualTo(expectedBrowseName));
        }

        [Test]
        [TestCase("NS1:D0", "NS1:D0", true)]
        [TestCase("NS2:D0", "NS2:D0", true)]
        [TestCase("NS1:D0", "D0", false)]
        [TestCase("NS2:D0", "NS2:D0", false)]
        [TestCase("NS1:Folder1:D0", "NS1:Folder1:D0", true)]
        [TestCase("NS1:Folder1:D0", "Folder1:D0", false)]
        [TestCase("NS2:Folder1:Folder2:D0", "NS2:Folder1:Folder2:D0", true)]
        [TestCase("NS2:Folder1:Folder2:D0", "NS2:Folder1:Folder2:D0", false)]
        [TestCase("UnavailableNamespaceNameInterpretedAsBrowseName:D0", "UnavailableNamespaceNameInterpretedAsBrowseName:D0", false)]
        [TestCase("UnavailableNamespaceNameInterpretedAsBrowseName:D0", "NS1:UnavailableNamespaceNameInterpretedAsBrowseName:D0", true)]

        [TestCase("*NS1:D0", "*NS1:String:D0", true)]
        [TestCase("*NS2:D0", "*NS2:String:D0", true)]
        [TestCase("*NS1:D0", "*D0", false)]
        [TestCase("*NS2:D0", "*NS2:D0", false)]
        [TestCase("*NS1:Folder1:D0", "*NS1:String:Folder1:D0", true)]
        [TestCase("*NS1:Folder1:D0", "*Folder1:D0", false)]
        [TestCase("*NS2:Folder1:Folder2:D0", "*NS2:String:Folder1:Folder2:D0", true)]
        [TestCase("*NS2:Folder1:Folder2:D0", "*NS2:Folder1:Folder2:D0", false)]
        [TestCase("*NS2:string:Folder1:Folder2:D0", "*NS2:String:Folder1:Folder2:D0", true)]
        [TestCase("*NS2:String:Folder1:Folder2:D0", "*NS2:String:Folder1:Folder2:D0", true)]
        [TestCase("*NS2:string:Folder1:Folder2:D0", "*NS2:Folder1:Folder2:D0", false)]
        [TestCase("*NS1:string:Folder1:Folder2:D0", "*NS1:String:Folder1:Folder2:D0", true)]
        [TestCase("*NS1:string:Folder1:Folder2:D0", "*Folder1:Folder2:D0", false)]

        [TestCase("*NS2:numeric:13676766", "*NS2:Numeric:13676766", true)]
        [TestCase("*NS2:Numeric:123456", "*NS2:Numeric:123456", false)]
        [TestCase("*NS1:Numeric:98765", "*NS1:Numeric:98765", true)]
        [TestCase("*NS1:numeric:0", "*Numeric:0", false)]

        [TestCase("*UnavailableNamespaceNameInterpretedAsNodeIdValue:D0", "*UnavailableNamespaceNameInterpretedAsNodeIdValue:D0", false)]
        [TestCase("*UnavailableNamespaceNameInterpretedAsNodeIdValue:D0", "*NS1:String:UnavailableNamespaceNameInterpretedAsNodeIdValue:D0", true)]
        public void CorrectDisplayTexts(string dataItemId, string expectedDisplayText, bool showDefaultNamespaceAndType)
        {
            string displayText = m_OpcUaNodeNameResolver.GetDisplayText(dataItemId, showDefaultNamespaceAndType);
            Assert.That(displayText, Is.EqualTo(expectedDisplayText));
        }

        [Test]
        [TestCase("NS1:D0", "NS1:D0")]
        [TestCase("NS2:D0", "NS2:D0")]
        [TestCase("D0", "NS1:D0")] // No valid namespace, append default namespace
        [TestCase("NS1:Folder1:D0", "NS1:Folder1:D0")]
        [TestCase("NS2:Folder1:D0", "NS2:Folder1:D0")]
        [TestCase("ns1:Folder1:D0", "NS1:Folder1:D0")] // Wrong case
        [TestCase("ns2:Folder1:D0", "NS2:Folder1:D0")] // Wrong case
        public void CorrectDataItemIds(string inputValue, string expectedDataItemId)
        {
            string dataItemId = m_OpcUaNodeNameResolver.GetValidDataItemId(inputValue);
            Assert.That(dataItemId, Is.EqualTo(expectedDataItemId));
        }
    }
}
#endif
