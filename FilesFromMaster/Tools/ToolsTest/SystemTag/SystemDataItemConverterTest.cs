using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using Core.Api.DataSource;
using Core.Controls.Api.AsmMeta;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.SystemTag
{
    [TestFixture]
    public class SystemDataItemConverterTest
    {
        private TypeDescriptionProvider m_MappingTypeDescriptorProvider;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            m_MappingTypeDescriptorProvider = new AsmMetaTypeDescriptionProviderBuilder(typeof(object))
                .Build();
            
            TypeDescriptor.AddProvider(m_MappingTypeDescriptorProvider, typeof(object));
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            TypeDescriptor.RemoveProvider(m_MappingTypeDescriptorProvider, typeof(object));
        }

        [Test]
        public void CanBeSerializedToInstanceDescriptor()
        {
            SystemDataItem systemDataItem = new SystemDataItem("aaa", null);
            TypeConverter typeConverter = TypeDescriptor.GetConverter(systemDataItem);

            Assert.IsTrue(typeConverter.CanConvertTo(typeof(InstanceDescriptor)));

            InstanceDescriptor instanceDescriptor = typeConverter.ConvertTo(systemDataItem, typeof(InstanceDescriptor)) as InstanceDescriptor;

            Assert.IsNotNull(instanceDescriptor);
        }

        [Test]
        public void CanCreateInstanceFromParameterArray()
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(SystemDataItem));

            Assert.IsTrue(typeConverter.GetCreateInstanceSupported());

            Dictionary<string, object> propertyValues = new Dictionary<string, object>();
            propertyValues["SystemTagInfoName"] = "systemtag";
            propertyValues["InitialValue"] = "123";
            propertyValues["Name"] = "aaa";
            propertyValues["DataType"] = BEDATATYPE.DT_DEFAULT.ToString();
            propertyValues["Size"] = "1";
            propertyValues["Offset"] = "0";
            propertyValues["Gain"] = "0";
            propertyValues["IndexRegisterNumber"] = "0";
            propertyValues["LogToAuditTrail"] = "false";
            propertyValues["TriggerName"] = string.Empty;
            propertyValues["ArraySize"] = "1";
            object newInstance = typeConverter.CreateInstance(propertyValues);

            Assert.IsNotNull(newInstance);
        }

        [Test]
        public void CanCreateInstanceFromParameterArrayWithOverloadedConstructor()
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(SystemDataItem));

            Assert.IsTrue(typeConverter.GetCreateInstanceSupported());

            Dictionary<string, object> propertyValues = new Dictionary<string, object>();
            propertyValues["SystemTagInfoName"] = "systemtag";
            propertyValues["InitialValue"] = "123";
            propertyValues["Name"] = "aaa";
            propertyValues["DataType"] = BEDATATYPE.DT_INTEGER2.ToString();
            propertyValues["Size"] = "1";
            propertyValues["Offset"] = "0";
            propertyValues["Gain"] = "0";
            propertyValues["IndexRegisterNumber"] = "0";
            propertyValues["LogToAuditTrail"] = "false";
            propertyValues["TriggerName"] = string.Empty;
            propertyValues["AccessRight"] = "Read";
            propertyValues["PollGroupName"] = "PollGroup1";
            propertyValues["AlwaysActive"] = bool.FalseString;
            propertyValues["NonVolatile"] = bool.FalseString;
            propertyValues["GlobalDataType"] = BEDATATYPE.DT_REAL4.ToString();
            propertyValues["ArraySize"] = "1";
            object newInstance = typeConverter.CreateInstance(propertyValues);

            Assert.IsNotNull(newInstance);
        }

        [Test]
        public void InstanceDescriptorCreatesEqualObjectAsProperties()
        {
            SystemDataItem systemDataItem = new SystemDataItem("systemtaginfoname", (Int16)123);
            systemDataItem.TriggerName = "triggername";
            systemDataItem.Size = 10;
            systemDataItem.Offset = -1;
            systemDataItem.Name = "name";
            systemDataItem.LogToAuditTrail = true;
            systemDataItem.IndexRegisterNumber = -2;
            systemDataItem.Gain = 3.14;
            systemDataItem.DataType = BEDATATYPE.DT_UINTEGER4;
            systemDataItem.AccessRight = AccessRights.Write;

            SerializeAndDeserializeWithInstanceDescriptorAndValidateProperties(systemDataItem);
        }

        [Test]
        public void InstanceDescriptorCreatesEqualObjectAsParameterizedConstructor()
        {
            SystemDataItem systemDataItem = new SystemDataItem("systemtaginfoname", "name", BEDATATYPE.DT_UINTEGER4, 10, -1, 3.14, -2, true, "triggername", AccessRights.Write, string.Empty, false, false, (Int16)123);

            SerializeAndDeserializeWithInstanceDescriptorAndValidateProperties(systemDataItem);
        }

        [Test]
        public void PropertiesCreatesObjectEqualToParameterizedConstructor()
        {
            SystemDataItem constructorDataItem = new SystemDataItem("systemtaginfoname", "name", BEDATATYPE.DT_UINTEGER4, 10, -1, 3.14, -2, true, "triggername", AccessRights.Write, string.Empty, false, false, (Int16)123,1);
            SystemDataItem propertiesDataItem = new SystemDataItem("systemtaginfoname", (Int16)123);
            propertiesDataItem.TriggerName = "triggername";
            propertiesDataItem.Size = 10;
            propertiesDataItem.Offset = -1;
            propertiesDataItem.Name = "name";
            propertiesDataItem.LogToAuditTrail = true;
            propertiesDataItem.IndexRegisterNumber = -2;
            propertiesDataItem.Gain = 3.14;
            propertiesDataItem.DataType = BEDATATYPE.DT_UINTEGER4;
            propertiesDataItem.AccessRight = AccessRights.Write;

            ComparePropertiesOnSystemDataItems(constructorDataItem, propertiesDataItem, true);
        }

        private static void SerializeAndDeserializeWithInstanceDescriptorAndValidateProperties(SystemDataItem systemDataItem)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(SystemDataItem));

            InstanceDescriptor instanceDescriptor = typeConverter.ConvertTo(systemDataItem, typeof(InstanceDescriptor)) as InstanceDescriptor;

            Assert.IsNotNull(instanceDescriptor);

            SystemDataItem createdItem = instanceDescriptor.Invoke() as SystemDataItem;

            ComparePropertiesOnSystemDataItems(systemDataItem, createdItem, false);
        }

        private static void ComparePropertiesOnSystemDataItems(SystemDataItem systemDataItem, SystemDataItem newSystemDataItem, bool compareValues)
        {
            Assert.AreEqual(systemDataItem.AccessRight, newSystemDataItem.AccessRight);
            Assert.AreEqual(systemDataItem.ActionName, newSystemDataItem.ActionName);
            Assert.AreEqual(systemDataItem.DataType, newSystemDataItem.DataType);
            Assert.AreEqual(systemDataItem.Gain, newSystemDataItem.Gain);
            Assert.AreEqual(systemDataItem.IndexRegisterNumber, newSystemDataItem.IndexRegisterNumber);
            Assert.AreEqual(systemDataItem.LogToAuditTrail, newSystemDataItem.LogToAuditTrail);
            Assert.AreEqual(systemDataItem.Name, newSystemDataItem.Name);
            Assert.AreEqual(systemDataItem.Offset, newSystemDataItem.Offset);
            Assert.AreEqual(systemDataItem.Size, newSystemDataItem.Size);
            Assert.AreEqual(systemDataItem.SystemTagInfoName, newSystemDataItem.SystemTagInfoName);
            Assert.AreEqual(systemDataItem.TriggerName, newSystemDataItem.TriggerName);
            if (compareValues)
            {
                //if deserialization, values can not be verifield due to no existing GlobalDataSubItem
                //"Real" deserialization creates GlobalDataSubItems from file (Tags.neo)
                Assert.AreEqual(systemDataItem.Value.Value, newSystemDataItem.Value.Value);
                Assert.AreEqual(systemDataItem.InitialValue, newSystemDataItem.InitialValue);
            }
        }
    }
}
