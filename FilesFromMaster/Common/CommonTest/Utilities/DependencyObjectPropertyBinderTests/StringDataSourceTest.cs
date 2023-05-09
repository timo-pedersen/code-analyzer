using System;
using Neo.ApplicationFramework.Common.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Utilities.DependencyObjectPropertyBinderTests
{
    [TestFixture]
    public class StringDataSourceTest : DependencyObjectPropertyBinderTestBase<string>
    {
        private const string TestValue = "12345";
        private const string TestValueWithDecimals = "12345,6789";

        [Test]
        public void ToShortSubscriber()
        {
            SetValueOnDataItem<short>(TestValue);
        }

        [Test]
        public void FromShortSubscriber()
        {
            SetValueOnDependencyProperty<short>(short.MaxValue);
        }

        [Test]
        public void ToIntSubscriber()
        {
            SetValueOnDataItem<int>(TestValue);
        }

        [Test]
        public void ToIntSubscriberWithFormatError()
        {
            SetValueOnDataItem<int>(TestValueWithDecimals, true);

            Assert.That((int)((VariantValue)m_DataItemProxy.Value), Is.EqualTo(0));
            Assert.That(Convert.ChangeType(((FrameworkElementMock<int>)m_DependencyObject).Value, typeof(string)), Is.EqualTo("0"));
        }

        [Test]
        public void FromIntSubscriber()
        {
            SetValueOnDependencyProperty<int>(int.MaxValue);
        }

        [Test]
        public void ToSingleSubscriber()
        {
            SetValueOnDataItem<Single>(TestValue);
        }

        [Test]
        public void FromSingleSubscriber()
        {
            SetValueOnDependencyProperty<Single>(Single.Parse(TestValue));
        }

        [Test]
        public void ToDoubleSubscriber()
        {
            using (var swedishCulture = new SelectSwedishTestingCulture())
            {
                SetValueOnDataItem<double>(TestValueWithDecimals);
            }
        }

        [Test]
        public void FromDoubleSubscriber()
        {
            SetValueOnDependencyProperty<double>(123456789.123456789);
        }

        [Test]
        public void FromMaxDoubleSubscriber()
        {
            SetValueOnDependencyProperty<double>(double.MaxValue, true);

            Assert.That((double)((VariantValue)m_DataItemProxy.Value), Is.EqualTo(0));
            Assert.That(Convert.ChangeType(((FrameworkElementMock<double>)m_DependencyObject).Value, typeof(string)), Is.EqualTo("0"));
        }

        [Test]
        public void ToStringSubscriber()
        {
            SetValueOnDataItem<string>(TestValue);
        }

        [Test]
        public void FromStringSubscriber()
        {
            SetValueOnDependencyProperty<string>(TestValue);
        }

        [Test]
        public void FromStringSubscriberWithDecimals()
        {
            SetValueOnDependencyProperty<string>(TestValueWithDecimals);
        }

        [Test]
        public void ToUInt16Subscriber()
        {
            SetValueOnDataItem<UInt16>(TestValue);
        }

        [Test]
        public void FromUInt16Subscriber()
        {
            SetValueOnDependencyProperty<UInt16>(UInt16.MaxValue);
        }

        [Test]
        public void ToUInt32Subscriber()
        {
            SetValueOnDataItem<UInt32>(TestValue);
        }

        [Test]
        public void FromUInt32Subscriber()
        {
            SetValueOnDependencyProperty<UInt32>(UInt32.MinValue);
        }

        [Test]
        public void ToDateTimeSubscriber()
        {
            SetValueOnDataItem<DateTime>(DateTime.Now.ToString());
        }

        [Test]
        public void ToDateTimeSubscriberWithFormatError()
        {
            using (var swedishCulture = new SelectSwedishTestingCulture())
            {
                SetValueOnDataItem<DateTime>("Invalid format", true);

                Assert.That((DateTime)((VariantValue)m_DataItemProxy.Value), Is.EqualTo(DateTime.MinValue));
                Assert.That(Convert.ChangeType(((FrameworkElementMock<DateTime>)m_DependencyObject).Value, typeof(string)), Is.EqualTo("0001-01-01 00:00:00"));
            }
        }

        [Test]
        public void FromDateTimeSubscriber()
        {
            SetValueOnDependencyProperty<DateTime>(DateTime.MaxValue);
        }
    }
}
