using System;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredType.LightweightRepresentation;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    [TestFixture]
    public class OpcUaNodeToStructuredTypeConverterTest
    {
        private const char Separator = '|';
        private IOpcUaNodeToStructuredTypeConverter m_Converter;

        [SetUp]
        public void SetUp()
        {
            m_Converter = new OpcUaNodeToStructuredTypeConverter();
        }
        
        [Test]
        public void AddressIsFetchedFromBaseType()
        {
            IOpcUaNode fullyInheritedTestNode = OpcUaConvertionFakeData.GetFullyInheritedTestNode();
            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(fullyInheritedTestNode);

            IStructuredReference buildInfoObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            ITagImportReference startTimeObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "StartTime");
            ITagImportReference productUriObjectNode = buildInfoObjectNode.ReferenceType.Members.FirstOrDefault(node => node.Name == "ProductUri");

            Assert.IsTrue(buildInfoObjectNode.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("2142"));
            Assert.IsTrue(startTimeObjectNode.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("2139"));
            Assert.IsTrue(productUriObjectNode.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("3052"));
        }

        [Test]
        public void TypeAddressForPropertyOnlyExistingInObjectExistsInTypeDefinitionAndHasTheSameAddress()
        {
            IOpcUaNode nodeWithAdditionalPropertyInObject = OpcUaConvertionFakeData.GetNodeWithAdditionalPropertyInObject();
            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(nodeWithAdditionalPropertyInObject);

            IStructuredReference buildInfoObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            ITagImportReference buildNumberObject = buildInfoObjectNode.ReferenceType.Members.FirstOrDefault(node => node.Name == "BuildNumber");

            Assert.IsTrue(buildNumberObject.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("2265"));
        }

        [Test]
        public void TypeAddressForPropertyOnlyExistingInTypeIsExistsAndAddressUnchanged()
        {
            IOpcUaNode nodeWithAdditionalPropertyInType = OpcUaConvertionFakeData.GetNodeWithAdditionalPropertyInType();
            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(nodeWithAdditionalPropertyInType);

            IStructuredReference buildInfoObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            ITagImportReference buildNumberObject = buildInfoObjectNode.ReferenceType.Members.FirstOrDefault(node => node.Name == "BuildNumber");

            Assert.IsTrue(buildNumberObject.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("3702"));
        }


        [Test]
        public void ArtificialBaseTypeIsCreatedIfThereIsNotATypeThatFullyMatchesTheObject()
        {
            IOpcUaNode fullyInheritedTestNode = OpcUaConvertionFakeData.GetNodeWithAdditionalPropertyInObject();
            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(fullyInheritedTestNode);

            IStructuredReference buildInfoObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            IStructuredTypeInfo buildInfoObjectNodeBase = buildInfoObjectNode.ReferenceType.BaseType;
            IStructuredTypeInfo buildInfoObjectNodeType = buildInfoObjectNode.ReferenceType;

            Assert.IsTrue(buildInfoObjectNodeBase.AddressDescriptor.GetIxControlAddress(Separator).Contains("3051"));
            Assert.IsTrue(buildInfoObjectNodeType.AddressDescriptor.GetIxControlAddress(Separator).Contains("2260"));
        }

        [Test]
        public void NodeWithoutBaseCreatesTypeNameFromNodeIdAndDisplayName()
        {
            IOpcUaNode fullyInheritedTestNode = OpcUaConvertionFakeData.GetFullyInheritedTestNode();
            fullyInheritedTestNode.BaseType = null;

            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(fullyInheritedTestNode);

            Assert.IsTrue(structuredTypeInfo.TypeName == "ServerStatus");
        }

        [Test]
        public void TypeNodeWithInheritanceIsDecoratedWithItAsBaseType()
        {
            IOpcUaNode fullyInheritedTestNode = OpcUaConvertionFakeData.GetNodeWithAdditionalPropertyInObject();
            IOpcUaNode additionalInheritanceTypeToMapTheObjectProperty = OpcUaTestUtilities.CreateNode("BuildNumberType", "BuildNumberType", 22265, typeof(object), NodeRepresentation.Type);
            IOpcUaNode additionalInheritanceObjectToMapTheObjectProperty = OpcUaTestUtilities.CreateNode("BuildNumber", "BuildNumber", 12265, typeof(String), NodeRepresentation.Object);
            additionalInheritanceTypeToMapTheObjectProperty.Children.Add(additionalInheritanceObjectToMapTheObjectProperty);
            fullyInheritedTestNode.Children.FirstOrDefault(n => n.Name == "BuildInfo").BaseType.BaseType = additionalInheritanceTypeToMapTheObjectProperty;

            IStructuredTypeInfo structuredTypeInfo = m_Converter.Convert(fullyInheritedTestNode);
            IStructuredReference buildInfoObjectNode = structuredTypeInfo.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            IStructuredTypeInfo buildInfoObjectNodeBase = buildInfoObjectNode.ReferenceType.BaseType;

            Assert.IsTrue(buildInfoObjectNodeBase.AddressDescriptor.GetIxControlAddress(Separator).Contains("22265"));
            Assert.IsTrue(buildInfoObjectNodeBase.Members.Count() == 1);

        }
    }
}
