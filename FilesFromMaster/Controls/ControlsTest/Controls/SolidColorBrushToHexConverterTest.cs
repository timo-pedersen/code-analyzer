using System;
using System.Globalization;
using System.Windows.Data;
using NUnit.Framework;
using System.Windows.Media;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class SolidColorBrushToHexConverterTest
    {
        [Test]
        public void ConvertBackHappyPathTest()
        {
            // ARRANGE
            IValueConverter converter = new Common.Converters.SolidColorBrushToHexConverter();

            // ACT
            var res = converter.ConvertBack("FF00FF", typeof(SolidColorBrush), null, new CultureInfo("en-US"));

            // ASSERT
            Assert.IsInstanceOf(typeof(SolidColorBrush), res);

            var solidColorBrush = res as SolidColorBrush;

            Assert.AreEqual("FF", solidColorBrush.Color.R.ToString("X2"));
        }

        [Test]
        public void ConvertBackTooShortStringTest()
        {
            // ARRANGE
            IValueConverter converter = new Common.Converters.SolidColorBrushToHexConverter();

            // ACT
            var res = converter.ConvertBack("FF00F", typeof(SolidColorBrush), null, new CultureInfo("en-US"));

            // ASSERT
            Assert.AreEqual(Binding.DoNothing, res);
        }

        [Test]
        public void ConvertBackErroneusStringTest()
        {
            // ARRANGE
            IValueConverter converter = new Common.Converters.SolidColorBrushToHexConverter();

            // ACT
            var res = converter.ConvertBack("FF00FP", typeof(SolidColorBrush), null, new CultureInfo("en-US"));

            // ASSERT
            Assert.AreEqual(Binding.DoNothing, res);
        }

        [Test]
        public void ConvertHappyPathTest()
        {
            // ARRANGE
            IValueConverter converter = new Common.Converters.SolidColorBrushToHexConverter();
            var solidColorBrush = new SolidColorBrush(Colors.AliceBlue);

            // ACT
            var res = converter.Convert(solidColorBrush, null, null, null);

            // ASSERT
            Assert.IsInstanceOf(typeof(string), res);

            Assert.IsTrue(((string)res).StartsWith("F0"));
        }

        [Test]
        public void ConvertWrongInputTest()
        {
            // ARRANGE
            IValueConverter converter = new Common.Converters.SolidColorBrushToHexConverter();
            var invalidClass = new NotASolidColorBrush();

            // ACT
            var res = converter.Convert(invalidClass, null, null, null);

            // ASSERT
            Assert.IsInstanceOf(typeof(string), res);

            Assert.IsTrue(((string)res).IsNullOrEmpty());
        }

        private class NotASolidColorBrush
        {
            
        }
    }
}
