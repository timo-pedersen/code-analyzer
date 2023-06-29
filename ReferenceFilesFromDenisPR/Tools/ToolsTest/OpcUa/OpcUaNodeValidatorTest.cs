#if !VNEXT_TARGET
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.OpcUa.Validators;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    [TestFixture]
    public class OpcUaNodeValidatorTest
    {
        private IValidator<IOpcUaNode> m_NodeValidator;
        
        [SetUp]
        public void SetUp()
        {
            m_NodeValidator = new OpcUaNodeValidator();
        }

        [Test]
        [TestCase("NotUnique", "NotUnique", false)]
        [TestCase("Unique", "AlsoUnique", true)]
        public void DuplicatedChildNamesCauseValidationToFail(string name1, string name2, bool expectedResult)
        {
            IOpcUaNode node = new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() {ValueRank = -1}};
            IOpcUaNode child1 = new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() { DisplayName = name1, BrowseName = "C1", ValueRank = -1} };
            IOpcUaNode child2 = new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() { DisplayName = name2, BrowseName = "C2", ValueRank = -1} };
            node.Children.AddRange(new[] { child1, child2 });
            Assert.AreEqual(m_NodeValidator.Validate(node), expectedResult);
        }

        public IOpcUaNode CreateNodeWithBrowseName(string browseName)
        {
            return new OpcUaNode() { NodeInfo = new OpcUaNodeInfo() { BrowseName = browseName } };
        }

    }
}
#endif
