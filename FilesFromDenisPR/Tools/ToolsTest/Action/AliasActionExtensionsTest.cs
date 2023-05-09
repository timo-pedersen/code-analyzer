using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.Tools.Actions;
using Neo.ApplicationFramework.Tools.OpcClient;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Action
{
    [TestFixture]
    public class AliasActionExtensionsTest
    {
        private IGlobalDataItem m_GlobalDataItem;
        private IInstantiatable m_ScreenStub;
        private readonly int m_Int = 0;
        private readonly double m_Double = 0.0;
        private readonly float m_Float = 0f;
        private readonly short m_Short = 0;
        private readonly ushort m_UShort = 0;
        private readonly uint m_UInt = 0;
        private readonly long m_Long = 0;
        private readonly byte m_Byte = 0;
        private readonly decimal m_Decimal = 0;
        private readonly bool m_Bool = false;
        private readonly DateTime m_DateTime = new DateTime(2000, 1, 1);
        private readonly string m_String = string.Empty;

        [SetUp]
        public void SetUp()
        {
            m_GlobalDataItem = new GlobalDataItem();

            m_ScreenStub = Substitute.For<IInstantiatable>();
            m_ScreenStub.GetBoundDataItem(Arg.Any<string>()).Returns(m_GlobalDataItem);
        }

        private void InvokeActionExtension(System.Action action, VariantValue initialValue, VariantValue expectedResult, BEDATATYPE dataType = BEDATATYPE.DT_DEFAULT)
        {
            m_GlobalDataItem.DataType = dataType;
            m_GlobalDataItem.Value = initialValue;
            object actualInitialValue = m_GlobalDataItem.Value;

            action.Invoke();

            Assert.That(actualInitialValue, Is.EqualTo(initialValue));

            if (dataType == BEDATATYPE.DT_REAL8)
                Assert.That(Math.Abs(((VariantValue)m_GlobalDataItem.Value - expectedResult)), Is.LessThanOrEqualTo(0.0000000001)); // due to floating point arithmetic, e.g., 9.2 - 5.2 = 3.999999999999991
            else
                Assert.That(m_GlobalDataItem.Value, Is.EqualTo(expectedResult));
        }

        [Test]
        public void TestToggleTag()
        {
            InvokeActionExtension(() => m_Int.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Double.ToggleTag(m_ScreenStub, "Alias"), 0.0, 1.0);
            InvokeActionExtension(() => m_Float.ToggleTag(m_ScreenStub, "Alias"), 0f, 1f);
            InvokeActionExtension(() => m_Short.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_UShort.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_UInt.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Long.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Byte.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Decimal.ToggleTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Bool.ToggleTag(m_ScreenStub, "Alias"), false, true, BEDATATYPE.DT_BOOLEAN);
        }

        [Test]
        public void TestSetTag()
        {
            InvokeActionExtension(() => m_Int.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Double.SetTag(m_ScreenStub, "Alias"), 0.0, 1.0);
            InvokeActionExtension(() => m_Float.SetTag(m_ScreenStub, "Alias"), 0f, 1f);
            InvokeActionExtension(() => m_Short.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_UShort.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_UInt.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Long.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Byte.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Decimal.SetTag(m_ScreenStub, "Alias"), 0, 1);
            InvokeActionExtension(() => m_Bool.SetTag(m_ScreenStub, "Alias"), false, true, BEDATATYPE.DT_BOOLEAN);
        }


        [Test]
        public void TestResetTag()
        {
            InvokeActionExtension(() => m_Int.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_Double.ResetTag(m_ScreenStub, "Alias"), 1.0, 0.0);
            InvokeActionExtension(() => m_Float.ResetTag(m_ScreenStub, "Alias"), 1f, 0f);
            InvokeActionExtension(() => m_Short.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_UShort.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_UInt.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_Long.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_Byte.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_Decimal.ResetTag(m_ScreenStub, "Alias"), 1, 0);
            InvokeActionExtension(() => m_Bool.ResetTag(m_ScreenStub, "Alias"), true, false, BEDATATYPE.DT_BOOLEAN);
        }

        [Test]
        public void TestIncrementAnalog()
        {
            InvokeActionExtension(() => m_Double.IncrementAnalog(5.2, m_ScreenStub, "Alias"), 4.0, 9.2, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Float.IncrementAnalog(5.2f, m_ScreenStub, "Alias"), 4f, 9.2f, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Decimal.IncrementAnalog(5.2, m_ScreenStub, "Alias"), 4, 9.2, BEDATATYPE.DT_REAL8);

            InvokeActionExtension(() => m_Int.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Short.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_UShort.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_UInt.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Long.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Byte.IncrementAnalog(5, m_ScreenStub, "Alias"), 4, 9);
        }

        [Test]
        public void TestDecrementAnalog()
        {
            InvokeActionExtension(() => m_Double.DecrementAnalog(5.2, m_ScreenStub, "Alias"), 9.2, 4.0, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Float.DecrementAnalog(5.2f, m_ScreenStub, "Alias"), 9.2f, 4f, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Decimal.DecrementAnalog(5.2, m_ScreenStub, "Alias"), 9.2, 4, BEDATATYPE.DT_REAL8);

            InvokeActionExtension(() => m_Int.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
            InvokeActionExtension(() => m_Short.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
            InvokeActionExtension(() => m_UShort.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
            InvokeActionExtension(() => m_UInt.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
            InvokeActionExtension(() => m_Long.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
            InvokeActionExtension(() => m_Byte.DecrementAnalog(5, m_ScreenStub, "Alias"), 9, 4);
        }

        [Test]
        public void TestSetAnalog()
        {
            InvokeActionExtension(() => m_Double.SetAnalog(9.2, m_ScreenStub, "Alias"), 4.0, 9.2, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Float.SetAnalog(9.2f, m_ScreenStub, "Alias"), 4f, 9.2f, BEDATATYPE.DT_REAL8);
            InvokeActionExtension(() => m_Decimal.SetAnalog(9.2, m_ScreenStub, "Alias"), 4, 9.2, BEDATATYPE.DT_REAL8);

            InvokeActionExtension(() => m_Int.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Short.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_UShort.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_UInt.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Long.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Byte.SetAnalog(9, m_ScreenStub, "Alias"), 4, 9);
            InvokeActionExtension(() => m_Bool.SetAnalog(true, m_ScreenStub, "Alias"), false, true, BEDATATYPE.DT_BOOLEAN);
        }

        [Test]
        public void TestSetStringOnString()
        {
            string initialValue = "initial string value";
            string stringToSet = "new string value";

            var globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_STRING;
            globalDataItem.Value = initialValue;
            string actualInitialValue = globalDataItem.Value;

            var screenStub = Substitute.For<IInstantiatable>();
            screenStub.GetBoundDataItem(Arg.Any<string>()).Returns(globalDataItem);

            m_String.SetString(stringToSet, screenStub, "Alias");

            Assert.That(actualInitialValue, Is.EqualTo(initialValue));
            Assert.That(globalDataItem.Value.String, Is.EqualTo(stringToSet));
        }

        [Test]
        public void TestSetStringOnDateTime()
        {
            DateTime initialValue = m_DateTime;
            DateTime dateTimeToSet = new DateTime(2001, 1, 1);

            var globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_DATETIME;
            globalDataItem.Value = initialValue;
            DateTime actualInitialValue = globalDataItem.Value;

            var screenStub = Substitute.For<IInstantiatable>();
            screenStub.GetBoundDataItem(Arg.Any<string>()).Returns(globalDataItem);

            m_DateTime.SetString(dateTimeToSet.ToString(), screenStub, "Alias");

            Assert.That(actualInitialValue, Is.EqualTo(initialValue));
            Assert.That(globalDataItem.Value.DateTime, Is.EqualTo(dateTimeToSet));
        }

        [Test]
        public void AliasWithUnboundDataItemDoesNotThrow()
        {
            // ARRANGE
            var screenStub = Substitute.For<IInstantiatable>();
            screenStub.GetBoundDataItem(Arg.Any<string>()).Returns(x => null);

            // ACT

            // ASSERT
            Assert.DoesNotThrow(() => m_Int.ToggleTag(screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_Int.SetTag(screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_Int.ResetTag(screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_Int.IncrementAnalog(1, screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_Int.DecrementAnalog(1, screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_Int.SetAnalog(1, screenStub, "Alias"));
            Assert.DoesNotThrow(() => m_String.SetString("1", screenStub, "Alias"));
        }
    }
}
