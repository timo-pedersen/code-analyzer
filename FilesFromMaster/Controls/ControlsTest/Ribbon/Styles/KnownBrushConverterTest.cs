using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Ribbon.Styles
{
    [TestFixture]
    public class KnownBrushConverterTest
    {
        [Test]
        public void ConvertNull()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            
            // ACT
            object convertedValue = converter.Convert(null, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.EqualTo(Binding.DoNothing));
        }


        [Test]
        public void ConvertUnsupportedNumberOfValues()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            var values = new[] { Brushes.Red, KnownBrushes, new object() };

            // ACT
            object convertedValue = converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.EqualTo(Binding.DoNothing));
        }


        [Test]
        public void ConvertNullBrush()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            var values = new object[] { null, KnownBrushes };

            // ACT
            object convertedValue = converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.Null);
        }

        
        [Test]
        public void ConvertWithoutKnownBrushes()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            var values = new object[] { Brushes.Red, null };

            // ACT
            object convertedValue = converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.EqualTo(Binding.DoNothing));
        }


        [Test]
        public void ConvertKnownBrush()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            var values = new object[] { Brushes.Red, KnownBrushes };

            // ACT
            object convertedValue = converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.EqualTo(Brushes.Red));
        }


        [Test]
        public void ConvertUnknownBrush()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();
            var values = new object[] { Brushes.SteelBlue, KnownBrushes };

            // ACT
            object convertedValue = converter.Convert(values, typeof(object), null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.Null);
        }


        [Test]
        public void ConvertBackNull()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();

            // ACT
            object convertedValue = converter.ConvertBack(null, new Type[] { typeof(Brush), typeof(ObservableCollection<Brush>)},  null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.TypeOf<object[]>());

            object firstValue = ((object[])convertedValue)[0];
            object secondValue = ((object[])convertedValue)[1];

            Assert.That(firstValue, Is.Null);
            Assert.That(secondValue, Is.Null);
        }

        
        [Test]
        public void ConvertBackBrush()
        {
            // ARRANGE
            var converter = new KnownBrushConverter();

            // ACT
            object convertedValue = converter.ConvertBack(Brushes.Red, new Type[] { typeof(Brush), typeof(ObservableCollection<Brush>) }, null, CultureInfo.CurrentCulture);

            // ASSERT
            Assert.That(convertedValue, Is.TypeOf<object[]>());

            object firstValue = ((object[])convertedValue)[0];
            object secondValue = ((object[])convertedValue)[1];

            Assert.That(firstValue, Is.EqualTo(Brushes.Red));
            Assert.That(secondValue, Is.Null);
        }



        /// <summary>
        /// Gets a sequence of brushes, namely red, green and blue.
        /// </summary>
        private static IEnumerable<Brush> KnownBrushes
        {
            get
            {
                return new Brush[]
                {
                    Brushes.Red,
                    Brushes.Green,
                    Brushes.Blue 
                };
            }
        }
    }
}