using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Opc.Ua;
using System;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    [TestFixture]
    public class OpcUaNodeTest
    {

        [Test]
        public void NodeWithPrimitiveTypeIsALeafNode()
        {
            IOpcUaNode primitiveNode = OpcUaTestUtilities.CreateNode("A", "A", 1, typeof(string), NodeRepresentation.Object);
            primitiveNode.Children.Add(OpcUaTestUtilities.CreateNode("C", "C", 2, typeof (string),NodeRepresentation.Object));
            Assert.IsTrue(primitiveNode.IsSupportedPrimitiveType);
        }
       
        [Test]
        public void NodeThatIsNotOfPrimitiveTypeIsNotALeaf()
        {
            IOpcUaNode primitiveNode = OpcUaTestUtilities.CreateNode("A", "A", 1, typeof(Opc.Ua.ServerStatusDataType), NodeRepresentation.Object);
            Assert.IsFalse(primitiveNode.IsSupportedPrimitiveType);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeIdWorksWithBothAbsoluteAndNonAbsoluteExpandedNodes(bool isAbsolute)
        {
            //ARRANGE
            ExpandedNodeId expandedNodeId;
            if (isAbsolute)
                expandedNodeId = new ExpandedNodeId("randomIdentifier", (ushort)new Random(1).Next(), "nonEmpty-namespace-uri", (uint)new Random(1).Next());
            else
                expandedNodeId = new ExpandedNodeId("randomIdentifier", (ushort)new Random(1).Next(), string.Empty, 0);

            //ACT
            //ASSERT
            Assert.DoesNotThrow(() => OpcUaNodeUtility.GetNodeId(expandedNodeId));
        }

        [TestCase("A")]
        [TestCase("Name")]
        [TestCase("abcdefghijklmnopqrstuvwxyz")]
        public void CreateOpcUaNodeCreatesNodeWithCorrectName(string name)
        {
            //ARRANGE
            OpcUaNodeInfo nodeInfo;

            //ACT
            IOpcUaNode node = OpcUaNodeUtility.CreateOpcUaNode(1, name, "B", out nodeInfo);

            //ASSERT
            Assert.AreEqual(name, node.Name);
            Assert.AreEqual(name, nodeInfo.DisplayName);
        }

        [TestCase((uint)1, IdType.Numeric)]
        [TestCase("id", IdType.String)]
        [TestCase(uint.MaxValue, IdType.Numeric)]
        [TestCase("åäö", IdType.String)]
        public void CreateOpcUaNodeCreatesNodeWithCorrectNodeId(object nodeIdValue, IdType type)
        {
            //ARRANGE
            OpcUaNodeInfo nodeInfo;
            NodeId nodeId = new NodeId(nodeIdValue, (ushort)new Random().Next(ushort.MinValue, ushort.MaxValue));

            //ACT
            IOpcUaNode node = OpcUaNodeUtility.CreateOpcUaNode(nodeId, "A", "B", out nodeInfo);

            //ASSERT
            Assert.AreEqual(nodeId.Identifier, node.NodeInfo.Identifier);
            Assert.AreEqual(nodeId.Identifier, nodeInfo.Identifier);
            Assert.AreEqual(type.ToString(), node.NodeInfo.IdType);
        }

        [Test]
        public void CreateOpcUaNodeWithNoParentDescriptorReturnsNullParentDescriptor()
        {
            //ARRANGE
            OpcUaNodeInfo nodeInfo;

            //ACT
            IOpcUaNode node = OpcUaNodeUtility.CreateOpcUaNode(1, "A", "B", out nodeInfo);

            //ASSERT
            Assert.AreEqual(null, node.ParentDescriptor);
        }
    }
}
