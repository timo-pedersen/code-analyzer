using System.Linq;
using System.Xml.Linq;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Security;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    public class AddAndUpdateProjectGuidConverterTest
    {
        private FileHelper m_FileHelper;
        private const string TestDocument = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <?neo version='2.0.482.0'?> <NeoItem d1p1:Serializer=\"Neo.ApplicationFramework.Common.Serialization.ObjectSerializer\" xmlns:d1p1=\"urn:Neo.ApplicationFramework.Serializer\">   <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalController, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"" + StringConstants.Tags + "\">     <GlobalDataItems>       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"Tag1\">         <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\".ctor\" MemberType=\"Constructor\">           <Parameter Name=\"Name\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"Tag1\" />           </Parameter>           <Parameter Name=\"DataType\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interop.DataSource.BEDATATYPE, DataSourceInterop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" primitive.value=\"DEFAULT\" />           </Parameter>           <Parameter Name=\"Size\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>           <Parameter Name=\"Offset\">             <Object d1p1:type=\"System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />           </Parameter>           <Parameter Name=\"Gain\">             <Object d1p1:type=\"System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>           <Parameter Name=\"IndexRegisterNumber\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />           </Parameter>           <Parameter Name=\"LogToAuditTrail\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"TriggerName\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"Value Change\" />           </Parameter>           <Parameter Name=\"AccessRight\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interfaces.AccessRights, InterfacesCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\">               <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Interfaces.AccessRights, InterfacesCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\"ReadWrite\" MemberType=\"Field\" />             </Object>           </Parameter>           <Parameter Name=\"PollGroupName\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"PollGroup1\" />           </Parameter>           <Parameter Name=\"AlwaysActive\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"NonVolatile\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"GlobalDataType\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interop.DataSource.BEDATATYPE, DataSourceInterop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" primitive.value=\"DEFAULT\" />           </Parameter>           <Parameter Name=\"Description\" Value=\"\" />           <Parameter Name=\"ArraySize\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>         </InstanceDescriptor>         <GlobalDataSubItems>           <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataSubItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\">             <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataSubItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\".ctor\" MemberType=\"Constructor\">               <Parameter Name=\"InitialValue\" Value=\"\" />               <Parameter Name=\"ArrayIndex\">                 <Object d1p1:type=\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />               </Parameter>               <Parameter Name=\"Keys\">                 <Object d1p1:arrayLength=\"0\" d1p1:type=\"System.String[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />               </Parameter>               <Parameter Name=\"Values\">                 <Object d1p1:arrayLength=\"0\" d1p1:type=\"System.String[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />               </Parameter>               <Parameter Name=\"PreventDuplicateEvents\">                 <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"True\" />               </Parameter>             </InstanceDescriptor>           </Object>         </GlobalDataSubItems>       </Object>     </GlobalDataItems>     <PollGroups>       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup1\" d1p1:Site.Name=\"PollGroup1\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup2\" d1p1:Site.Name=\"PollGroup2\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup3\" d1p1:Site.Name=\"PollGroup3\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup4\" d1p1:Site.Name=\"PollGroup4\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup5\" d1p1:Site.Name=\"PollGroup5\" />     </PollGroups>   </Object> </NeoItem>";
        private const string ConvertedDoc = "<?neo version='2.0.482.0'?> <NeoItem d1p1:Serializer=\"Neo.ApplicationFramework.Common.Serialization.ObjectSerializer\" xmlns:d1p1=\"urn:Neo.ApplicationFramework.Serializer\">   <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalController, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"" + StringConstants.Tags + "\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\">     <GlobalDataItems>       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"Tag1\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\">         <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\".ctor\" MemberType=\"Constructor\">           <Parameter Name=\"Name\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"Tag1\" />           </Parameter>           <Parameter Name=\"DataType\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interop.DataSource.BEDATATYPE, DataSourceInterop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" primitive.value=\"DEFAULT\" />           </Parameter>           <Parameter Name=\"Size\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>           <Parameter Name=\"Offset\">             <Object d1p1:type=\"System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />           </Parameter>           <Parameter Name=\"Gain\">             <Object d1p1:type=\"System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>           <Parameter Name=\"IndexRegisterNumber\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />           </Parameter>           <Parameter Name=\"LogToAuditTrail\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"TriggerName\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"Value Change\" />           </Parameter>           <Parameter Name=\"AccessRight\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interfaces.AccessRights, InterfacesCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\">               <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Interfaces.AccessRights, InterfacesCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\"ReadWrite\" MemberType=\"Field\" />             </Object>           </Parameter>           <Parameter Name=\"PollGroupName\">             <Object d1p1:type=\"System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"PollGroup1\" />           </Parameter>           <Parameter Name=\"AlwaysActive\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"NonVolatile\">             <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"False\" />           </Parameter>           <Parameter Name=\"GlobalDataType\">             <Object d1p1:type=\"Neo.ApplicationFramework.Interop.DataSource.BEDATATYPE, DataSourceInterop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" primitive.value=\"DEFAULT\" />           </Parameter>           <Parameter Name=\"Description\" Value=\"\" />           <Parameter Name=\"ArraySize\">             <Object d1p1:type=\"System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"1\" />           </Parameter>         </InstanceDescriptor>         <GlobalDataSubItems>           <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataSubItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\">             <InstanceDescriptor DeclaringType=\"Neo.ApplicationFramework.Tools.OpcClient.GlobalDataSubItem, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Name=\".ctor\" MemberType=\"Constructor\">               <Parameter Name=\"InitialValue\" Value=\"\" />               <Parameter Name=\"ArrayIndex\">                 <Object d1p1:type=\"System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"0\" />               </Parameter>               <Parameter Name=\"Keys\">                 <Object d1p1:arrayLength=\"0\" d1p1:type=\"System.String[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />               </Parameter>               <Parameter Name=\"Values\">                 <Object d1p1:arrayLength=\"0\" d1p1:type=\"System.String[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />               </Parameter>               <Parameter Name=\"PreventDuplicateEvents\">                 <Object d1p1:type=\"System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" primitive.value=\"True\" />               </Parameter>             </InstanceDescriptor>           </Object>         </GlobalDataSubItems>       </Object>     </GlobalDataItems>     <PollGroups>       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup1\" d1p1:Site.Name=\"PollGroup1\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup2\" d1p1:Site.Name=\"PollGroup2\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup3\" d1p1:Site.Name=\"PollGroup3\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup4\" d1p1:Site.Name=\"PollGroup4\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Tools.OpcClient.PollGroup, ToolsCF, Version=2.0.482.0, Culture=neutral, PublicKeyToken=null\" Interval=\"500\" Name=\"PollGroup5\" d1p1:Site.Name=\"PollGroup5\" ProjectGuid=\"01c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />     </PollGroups>   </Object> </NeoItem>";
        private const string Guid = "51c1cc62-7d39-4093-9ab4-6da6c1f5f739";
        private const string SecurityNeoFile = "<?xml version=\"1.0\" encoding=\"utf-8\"?><?neo version='2.11.2020.0'?><NeoItem d1p1:Serializer=\"Neo.ApplicationFramework.Common.Serialization.ObjectSerializer\" xmlns:d1p1=\"urn:Neo.ApplicationFramework.Serializer\">  <Object d1p1:type=\"Neo.ApplicationFramework.Tools.Security.SecurityManager, ToolsCF, Version=2.11.2020.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"Security\" ProjectGuid=\"51c1cc62-7d39-4093-9ab4-6da6c1f5f739\" /></NeoItem>";
        private const string SecurityConfigFile = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Security SchemaVersion=\"2.0.463.0\">  <SecurityGroups>    <Group ID=\"Group_01\" Name=\"Administrators\" />    <Group ID=\"Group_02\" Name=\"Operators\" />  </SecurityGroups>  <SecurityUsers>    <User>      <Username>Administrator</Username>      <PasswordHash>OoMDty4A6yBaM9z7Qo7vCnMLZCU=</PasswordHash>      <Description></Description>      <Groups>        <Group>Group_01</Group>      </Groups>      <IsErasable>False</IsErasable>    </User>  </SecurityUsers></Security>";
        private const string ExpressionFile = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <?neo version='2.0.463.0'?> <NeoItem d1p1:Serializer=\"Neo.ApplicationFramework.Common.Serialization.ObjectSerializer\" xmlns:d1p1=\"urn:Neo.ApplicationFramework.Serializer\">   <Object d1p1:type=\"Neo.ApplicationFramework.Tools.Expressions.ExpressionManager, ToolsCF, Version=2.0.463.0, Culture=neutral, PublicKeyToken=null\" d1p1:Site.Name=\"Expressions\" ProjectGuid=\"51c1cc62-7d39-4093-9ab4-6da6c1f5f739\">     <Expressions>       <Object d1p1:type=\"Neo.ApplicationFramework.Controls.Expressions.Expression, ControlsIde, Version=2.0.463.0, Culture=neutral, PublicKeyToken=null\" Group=\"Math\" Script=\"System.Math.Cos(value)\" Name=\"NoGuidExp\" Description=\"Returns the cosine of the specified angle (rad).\" />       <Object d1p1:type=\"Neo.ApplicationFramework.Controls.Expressions.Expression, ControlsIde, Version=2.0.463.0, Culture=neutral, PublicKeyToken=null\" Group=\"Math\" Script=\"System.Math.Cos(value)\" Name=\"GuidExp\" Description=\"Returns the cosine of the specified angle (rad).\" ProjectGuid=\"51c1cc62-7d39-4093-9ab4-6da6c1f5f739\" />     </Expressions>   </Object> </NeoItem>";

        [SetUp]
        public void Setup()
        {
            m_FileHelper = Substitute.For<FileHelper>();
        }

        [Test]
        public void UpdatesAllDesignerItemBaseObjects()
        {
            m_FileHelper.Exists(Arg.Any<string>()).Returns(false);
            XDocument doc = XDocument.Parse(TestDocument);
            int numOfGuidBefore = CountGuids(doc);
            var converter = new TestConverter(m_FileHelper);
            bool success = converter.Convert(doc);
            var numOfGuidAfter = CountGuids(doc);
            Assert.AreEqual(numOfGuidBefore, 0);
            Assert.AreEqual(numOfGuidAfter, 7); // this might change if more types in the AAUPGC has been added
            Assert.IsTrue(success);
        }

        [Test]
        public void DoesNotUpdateIfPropertyExists()
        {
            m_FileHelper.Exists(Arg.Any<string>()).Returns(false);
            XDocument doc = XDocument.Parse(ConvertedDoc);
            var converter = new TestConverter(m_FileHelper);
            bool success = converter.Convert(doc);
            Assert.IsFalse(doc.ToString().Contains(Guid));
            Assert.IsTrue(success);
        }

        [Test]
        public void UpdatesExpressionsGuids()
        {
            m_FileHelper.Exists(Arg.Any<string>()).Returns(false);
            XDocument doc = XDocument.Parse(ExpressionFile);
            int numOfGuidBefore = CountGuids(doc);
            var converter = new TestConverter(m_FileHelper);
            bool success = converter.Convert(doc);
            var numOfGuidAfter = CountGuids(doc);
            Assert.AreEqual(numOfGuidBefore, 2);
            Assert.AreEqual(numOfGuidAfter, 3); // this might change if more types in the AAUPGC has been added
            Assert.IsTrue(success);
        }

        [Test]
        public void ConvertsSecurityXmlFile()
        {
            m_FileHelper.Exists(Arg.Any<string>()).Returns(true);
            XDocument doc = XDocument.Parse(SecurityNeoFile);
            var converter = new TestConverter(m_FileHelper);
            bool success = converter.Convert(doc);

            // Check that security config file was updated with guid's
            XElement groupElement = converter.SavedSecurityDoc.Descendants(SecurityUserSerializer.SecurityGroupsConstant).Elements().First();
            XElement userElement = converter.SavedSecurityDoc.Descendants(SecurityUserSerializer.SecurityUsers).Elements().First();

            Assert.IsTrue(success);
            Assert.IsTrue(groupElement.Attribute(SecurityUserSerializer.ProjectGuidAttribute).Value == Guid);
            Assert.IsTrue(userElement.Attribute(SecurityUserSerializer.ProjectGuidAttribute).Value == Guid);
        }

        private static int CountGuids(XDocument testDoc)
        {
            var v = testDoc.ToString();
            int numOfGuidBefore = v.Select((c, i) => v.Substring(i)).Count(sub => sub.StartsWith(Guid));
            return numOfGuidBefore;
        }

        class TestConverter : AddAndUpdateProjectGuidConverter
        {

            public XDocument SavedSecurityDoc { get; set; }

            public TestConverter(FileHelper fileHelper)
            {
                FileHelper = fileHelper;
            }

            protected override string ExtractGuid(string folderPath)
            {
                return Guid;
            }

            protected override XDocument LoadDocumentElements(string filepath)
            {
                return XDocument.Parse(SecurityConfigFile);
            }

            protected override void SaveDocumentElements(XDocument document, string filepath)
            {
                SavedSecurityDoc = document;
            }

            public bool Convert(XDocument doc)
            {
                return ConvertDesigner("", doc);
            }
        }
    }
}
