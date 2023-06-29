#if !VNEXT_TARGET
using System;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    public class OpcUaConvertionFakeData
    {

        static readonly IOpcUaNode m_BaseDataVariableType = OpcUaTestUtilities.CreateNode("BaseDataVariableType", "BaseDataVariableType", 63, typeof(Opc.Ua.Variant), NodeRepresentation.Type);
       
        public static IOpcUaNode GetFullyInheritedTestNode()
        {
            // Creating type representation of ServerStatus
            IOpcUaNode serverStatusBaseType = OpcUaTestUtilities.CreateNode("ServerStatusType", "ServerStatusType", 2138, typeof(Opc.Ua.ServerStatusDataType), NodeRepresentation.Type);
            IOpcUaNode startTimeType = OpcUaTestUtilities.CreateNode("StartTime", "StartTime", 2139, typeof(DateTime), NodeRepresentation.Object);
            
            startTimeType.BaseType = m_BaseDataVariableType;

            IOpcUaNode productUriType = OpcUaTestUtilities.CreateNode("ProductUri", "ProductUri", 3698, typeof(String), NodeRepresentation.Object);
            productUriType.BaseType = m_BaseDataVariableType;

            IOpcUaNode buildInfoType = OpcUaTestUtilities.CreateNode("BuildInfo", "BuildInfo", 2142, typeof(Opc.Ua.BuildInfo), NodeRepresentation.Object);
            IOpcUaNode buildInfoBaseType = OpcUaTestUtilities.CreateNode("BuildInfoType", "BuildInfoType", 3051, typeof(Opc.Ua.BuildInfo), NodeRepresentation.Type);
            IOpcUaNode productUriTypeInBuildInfoBaseType = OpcUaTestUtilities.CreateNode("ProductUri", "ProductUri", 3052, typeof(String), NodeRepresentation.Object);
            productUriTypeInBuildInfoBaseType.BaseType = m_BaseDataVariableType;
            buildInfoBaseType.Children.Add(productUriTypeInBuildInfoBaseType);
            buildInfoType.BaseType = buildInfoBaseType;
            buildInfoType.Children.Add(productUriType);

            serverStatusBaseType.Children.Add(startTimeType);
            serverStatusBaseType.Children.Add(buildInfoType);
            

            // Createing object representation of ServerStatus

            IOpcUaNode serverStatusObject = OpcUaTestUtilities.CreateNode("ServerStatus", "ServerStatus", 2256,typeof (Opc.Ua.ServerStatusDataType),NodeRepresentation.Object);
            serverStatusObject.BaseType = serverStatusBaseType;

            IOpcUaNode startTimeObject = OpcUaTestUtilities.CreateNode("StartTime", "StartTime", 2257, typeof(DateTime), NodeRepresentation.Object);
            startTimeObject.BaseType = m_BaseDataVariableType;

            IOpcUaNode productUriObject = OpcUaTestUtilities.CreateNode("ProductUri", "ProductUri", 2262,typeof (String), NodeRepresentation.Object);
            productUriObject.BaseType = m_BaseDataVariableType;

            IOpcUaNode buildInfoObject = OpcUaTestUtilities.CreateNode("BuildInfo", "BuildInfo", 2260,typeof (Opc.Ua.BuildInfo),NodeRepresentation.Object);
            buildInfoObject.BaseType = buildInfoBaseType;

            buildInfoObject.Children.Add(productUriObject);
            serverStatusObject.Children.Add(buildInfoObject);
            serverStatusObject.Children.Add(startTimeObject);


            return serverStatusObject;
        }

        public static IOpcUaNode GetNodeWithAdditionalPropertyInObject()
        {
            IOpcUaNode node = GetFullyInheritedTestNode();
            IOpcUaNode newObjectInBuildInfoObject = OpcUaTestUtilities.CreateNode("BuildNumber", "BuildNumber", 2265,typeof (String),NodeRepresentation.Object);
            newObjectInBuildInfoObject.BaseType = m_BaseDataVariableType;

            IOpcUaNode buildInfoObjectNode = node.Children.FirstOrDefault(n => n.Name == "BuildInfo");
            buildInfoObjectNode.Children.Add(newObjectInBuildInfoObject);

            return node;
        }

        public static IOpcUaNode GetNodeWithAdditionalPropertyInType()
        {
            IOpcUaNode node = GetNodeWithAdditionalPropertyInObject();
            IOpcUaNode newTypeInServerStatusTypesBuildInfo = OpcUaTestUtilities.CreateNode("BuildNumber", "BuildNumber", 3702, typeof(String), NodeRepresentation.Object);
            newTypeInServerStatusTypesBuildInfo.BaseType = m_BaseDataVariableType;

            IOpcUaNode buildInfoInServerStatusType = node.BaseType.Children.FirstOrDefault(n => n.Name == "BuildInfo");
            buildInfoInServerStatusType.Children.Add(newTypeInServerStatusTypesBuildInfo);

            return node;
        }

    }
}
#endif
