#if !VNEXT_TARGET
using System;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredType.LightweightRepresentation;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    [TestFixture]
    public class OpcUaNodeToStructuredReferenceBaseConverterTest
    {
        const char Separator = '|';
        private IOpcUaNodeToStructuredReferenceBaseConverter m_Converter;
        private const string basePropertyName = "BaseProperty";
        private const string subTypeNodeName = "SubTypeNode";

        [SetUp]
        public void SetUp()
        {
            m_Converter = new OpcUaNodeToStructuredReferenceBaseConverter();
        }

        [Test]
        public void LeafNodesAreMappedToPrimitiveReference()
        {
            IOpcUaNode node = CreatePrimitiveTestSubTypeNode();
            var structuredReference = (IStructuredReference)m_Converter.Convert(node.BaseType);
            Type mappedType = structuredReference.ReferenceType.Members[0].GetType();
            Assert.True(mappedType.GetInterfaces().Contains(typeof(IPrimitiveReference)));
            Assert.True(structuredReference.ReferenceType.Members[0] is IPrimitiveReference);
        }

        [Test]
        public void LeafNodeCanBePassedAsArgument()
        {
            IOpcUaNode baseProperty = new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() { DisplayName = basePropertyName, Identifier = 1, Type = typeof(int), IdType = "Numeric"} };
            ITagImportReference tagImportReference = m_Converter.Convert(baseProperty);
            Assert.True(tagImportReference is IPrimitiveReference);
        }

        [Test]
        public void NonLeafNodesAreMappedToStructuredReference()
        {
            IOpcUaNode node = CreatePrimitiveTestSubTypeNode(false);
            ITagImportReference tagImportReference = m_Converter.Convert(node);
            Assert.True(tagImportReference is IStructuredReference);
        }

        [Test]
        public void AddressIsFetchedFromObjectNode()
        {
            IStructuredReference tagImportReference = m_Converter.Convert(OpcUaConvertionFakeData.GetFullyInheritedTestNode()) as IStructuredReference;
            IStructuredReference buildInfoObjectNode = tagImportReference.ReferenceType.Members.FirstOrDefault(node => node.Name == "BuildInfo") as IStructuredReference;
            ITagImportReference productUri = buildInfoObjectNode.ReferenceType.Members.FirstOrDefault(node => node.Name == "ProductUri");

            Assert.IsTrue(buildInfoObjectNode.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("2260"));
            Assert.IsTrue(productUri.AddressDescriptor.GetIxControlAddress(Separator).EndsWith("2262"));
        }

       
        private static IOpcUaNode CreatePrimitiveTestSubTypeNode(bool includeSuperType = true)
        {
            IOpcUaNode baseProperty = new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() { DisplayName = basePropertyName, Identifier = 1, Type = typeof(int), IdType = "Numeric" } };
            IOpcUaNode baseNode = new OpcUaNode() {NodeInfo = new OpcUaNodeInfo() { DisplayName = "BaseNode", Identifier = 2, IdType = "Numeric"} };
            baseNode.Children.Add(baseProperty);
            var node = new OpcUaNode {NodeInfo = new OpcUaNodeInfo { DisplayName = subTypeNodeName, Identifier = 3, IdType = "Numeric" }};
            node.Children.Add(baseProperty);
            if (includeSuperType)
                node.BaseType = baseNode;

            return node;
        }
    }
}
#endif
