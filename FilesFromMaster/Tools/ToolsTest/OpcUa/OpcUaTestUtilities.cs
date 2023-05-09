using System;
using Neo.ApplicationFramework.Interfaces;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    public class OpcUaTestUtilities
    {

        public static IOpcUaNode CreateNode(string name, string browseName, object identifier, Type type, NodeRepresentation nodeRepresentation )
        {
            IOpcUaNodeInfo nodeInfo = new OpcUaNodeInfo()
                                          {
                                              DisplayName = name,
                                              BrowseName = browseName,
                                              Identifier = identifier,
                                              IdType = "Numeric",
                                              Type = type
                                          };
            return new OpcUaNode {NodeInfo = nodeInfo, NodeRepresentation = nodeRepresentation};
        }

    }
}
