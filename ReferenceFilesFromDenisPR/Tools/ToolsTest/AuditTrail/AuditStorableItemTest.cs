using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Neo.ApplicationFramework.Controls.AuditTrail;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.AuditTrail
{
    [TestFixture]
    public class AuditStorableItemTest
    {
        private const string ExpectedUserName = "SomeUserName";
        private const string ExpectedMessage = "SomeMessage";
        private const string ExpectedDescription = "SomeDescription";
        private readonly DateTime m_ExpectedTimeStamp = DateTime.Now;
        private const string ExpectedValueBefore = "OldValue";
        private const string ExpectedValueAfter = "NewValue";
        private string[] m_Columns;
        private Type[] m_Types;

        [SetUp]
        public void SetUp()
        {
            m_Columns = new[]
            {
                "UserName",
                "Message",
                "Description",
                "TimeStamp",
                "ValueBefore",
                "ValueAfter"
            };

            m_Types = new[]
            {
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(object),
                typeof(object),
                typeof(object)
            };

            var dateTimeEditService = TestHelper.AddServiceStub<IDateTimeEditService>();
            dateTimeEditService.LocalTime.Returns(DateTime.Now);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void NormalMappingWorks()
        {
            var row = new object[]
                        {
                            ExpectedUserName,
                            ExpectedMessage,
                            ExpectedDescription,
                            m_ExpectedTimeStamp,
                            ExpectedValueBefore,
                            ExpectedValueAfter
                        };

            IEnumerable<AuditStorableItem> items;
            using (IDataReader dataReader = new DataTableReader(CreateTestDataTable(row)))
            {
                items = AuditStorableItem.Create(dataReader);
            }

            AuditStorableItem auditStorableItem = items.First();

            Assert.AreEqual(ExpectedUserName, auditStorableItem.UserName);
            Assert.AreEqual(ExpectedMessage, auditStorableItem.Message);
            Assert.AreEqual(m_ExpectedTimeStamp, auditStorableItem.TimeStamp);
            Assert.AreEqual(ExpectedValueBefore, auditStorableItem.ValueBefore);
            Assert.AreEqual(ExpectedValueAfter, auditStorableItem.ValueAfter);
        }

        [Test]
        public void StringColumnsThatAreNullOrDbNullWillBecomeStringEmpty()
        {
            var row = new object[]
                        {
                            ExpectedUserName,
                            null,
                            ExpectedDescription,
                            m_ExpectedTimeStamp,
                            DBNull.Value,
                            ExpectedValueAfter
                        };

            IEnumerable<AuditStorableItem> items;
            using (IDataReader dataReader = new DataTableReader(CreateTestDataTable(row)))
            {
                items = AuditStorableItem.Create(dataReader);
            }

            AuditStorableItem auditStorableItem = items.FirstOrDefault();

            Assert.AreEqual(ExpectedUserName, auditStorableItem.UserName);
            Assert.AreEqual(string.Empty, auditStorableItem.Message);
            Assert.AreEqual(m_ExpectedTimeStamp, auditStorableItem.TimeStamp);
            Assert.AreEqual(string.Empty, auditStorableItem.ValueBefore);
            Assert.AreEqual(ExpectedValueAfter, auditStorableItem.ValueAfter);
        }

        [Test]
        public void DateTimeColumnsThatAreNullOrDbNullWillBecomeDateTimeMinValue()
        {
            var row = new object[]
                        {
                            null,
                            null,
                            null,
                            null,
                            null,
                            null
                        };

            IEnumerable<AuditStorableItem> items;
            using (IDataReader dataReader = new DataTableReader(CreateTestDataTable(row)))
            {
                items = AuditStorableItem.Create(dataReader);
            }

            AuditStorableItem item = items.FirstOrDefault();

            Assert.AreEqual(DateTime.MinValue, item.TimeStamp);
        }

        private DataTable CreateTestDataTable(object[] row)
        {
            var dataTable = new DataTable();
            for (int i = 0; i <  m_Columns.Length; i++)
            {
                dataTable.Columns.Add(new DataColumn(m_Columns[i], m_Types[i]));
            }

            var dataRow = dataTable.NewRow();
            for (int i = 0; i < m_Columns.Length; i++)
            {
                dataRow[m_Columns[i]] = row[i];
            }

            dataTable.Rows.Add(dataRow);

            return dataTable;
        }
    }
}
