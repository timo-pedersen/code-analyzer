using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NUnit.Framework;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Neo.ApplicationFramework.Common.Graphics
{
    [TestFixture]
    public class BitmapHelperTest
    {
        private readonly Size m_DesiredThumbnailSize = new Size(100, 100);

        private BitmapHelper m_BitmapHelper;
        private Canvas m_ElementCanvas;
        private List<string> m_TempFileNames;

        [SetUp]
        public void TestSetup()
        {
            m_BitmapHelper = new BitmapHelper();

            m_ElementCanvas = new Canvas
            {
                Width = 800,
                Height = 600
            };

            m_TempFileNames = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string tempFileName in m_TempFileNames)
            {
                if (File.Exists(tempFileName))
                {
                    Trace.WriteLine("Deleting temp file: " + tempFileName);
                    File.Delete(tempFileName);
                }
            }
        }

        [Test]
        public void GetNewSizeWithSmallerAspectRatioReturnsBoundingWidthAndScaledBoundingHeight()
        {
            // ARRANGE
            var oldSize = new Size(100, 80);
            var bounds = new Size(50, 50);
            var expectedSize = new Size(50, 40);

            // ACT
            Size newSize = BitmapHelper.GetNewSize(
                bounds.Width,
                bounds.Height,
                oldSize.Width,
                oldSize.Height);

            // ASSERT
            Assert.AreEqual(expectedSize.Width, (int)newSize.Width);
            Assert.AreEqual(expectedSize.Height, (int)newSize.Height);
        }

        [Test]
        public void GetNewSizeWithLargerAspectRatioReturnsBoundingHeightAndScaledBoundingWidth()
        {
            // ARRANGE
            var oldSize = new Size(80, 100);
            var bounds = new Size(50, 50);
            var expectedSize = new Size(40, 50);

            // ACT
            Size newSize = BitmapHelper.GetNewSize(
                bounds.Width,
                bounds.Height,
                oldSize.Width,
                oldSize.Height);

            // ASSERT
            Assert.AreEqual(expectedSize.Width, (int)newSize.Width);
            Assert.AreEqual(expectedSize.Height, (int)newSize.Height);
        }

        [Test]
        public void CanGenerateBitmapSourceUsingASolidBrush()
        {
            // ARRANGE
            Brush solidBrush = new SolidColorBrush(Colors.Fuchsia);

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingBrush(
                solidBrush,
                new Size(800, 600),
                new Size(100, 100));

            // ASSERT
            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);

            // ASSERT
            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(0, 0).ToArgb());
            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(99, 74).ToArgb());
        }

        [Test]
        public void CanGenerateBitmapSourceWithSingleElement()
        {
            // ARRANGE
            Button button = AddButtonToCanvas(400, 300, 400, 300);
            button.Background = Brushes.Fuchsia;

            m_ElementCanvas.ForceLayout();

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrush(button, 100, 100);

            // ASSERT
            m_BitmapHelper.SaveBitmapSourceToFile(
                bitmap,
                new PngBitmapEncoder(),
                TempFilePath);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);

            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(1, 1).ToArgb());
            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(98, 73).ToArgb());
        }

        [Test]
        public void CanGenerateBitmapSourceWithSingleElementWhichIsOffset()
        {
            // ARRANGE
            Button button = AddButtonToCanvas(400, 300, 400, 300);
            button.Background = Brushes.Fuchsia;

            m_ElementCanvas.ForceLayout();

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrush(button, 100, 100);

            // ASSERT
            m_BitmapHelper.SaveBitmapSourceToFile(
                bitmap,
                new PngBitmapEncoder(),
                TempFilePath);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);

            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(1, 1).ToArgb());
            Assert.AreEqual(Color.Fuchsia.ToArgb(), convertedBitmap.GetPixel(98, 73).ToArgb());
        }

        [Test]
        public void CanGenerateBitmapSourceWithOneElementInList()
        {
            // ARRANGE
            AddButtonToCanvas(100, 100, 400, 200);

            m_ElementCanvas.ForceLayout();

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrush(
                m_ElementCanvas.Children.Cast<FrameworkElement>(),
                new Size(m_ElementCanvas.Width, m_ElementCanvas.Height),
                new Size(100, 100));

            // ASSERT
            m_BitmapHelper.SaveBitmapSourceToFile(
                bitmap,
                new PngBitmapEncoder(),
                TempFilePath);

            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(13, 13).A);
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(62, 37).A);

            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(12, 13).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(13, 12).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(62, 38).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(63, 37).A);
        }

        [Test]
        public void CanGenerateBitmapSourceWithTwoElements()
        {
            // ARRANGE
            AddButtonToCanvas(0, 0, 200, 200);
            AddButtonToCanvas(600, 400, 200, 200);

            m_ElementCanvas.ForceLayout();

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrush(
                m_ElementCanvas.Children.Cast<FrameworkElement>(),
                new Size(m_ElementCanvas.Width, m_ElementCanvas.Height),
                m_DesiredThumbnailSize);

            // ASSERT
            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            m_BitmapHelper.SaveBitmapSourceToFile(
                bitmap,
                new PngBitmapEncoder(),
                TempFilePath);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);
            //Verify edges of first button
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(0, 0).A);
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(24, 24).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(25, 0).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(25, 25).A);

            //Verify edges of second button
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(74, 50).A);
            Assert.AreEqual(Color.Transparent.A, convertedBitmap.GetPixel(75, 49).A);
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(75, 50).A);
            Assert.AreNotEqual(Color.Transparent.A, convertedBitmap.GetPixel(99, 74).A);
        }

        [Test]
        public void CanGenerateBitmapSourceForAutosizePolylineWithoutOffset()
        {
            const int maxWidth = 20;
            const int maxHeight = 30;

            var polyline = new Polyline
            {
                Stretch = Stretch.None,
                Points = new PointCollection(new[]
                {
                    new Point(10, 20),
                    new Point(maxWidth, maxHeight),
                })
            };

            Canvas.SetLeft(polyline, 100);
            Canvas.SetTop(polyline, 30);

            m_ElementCanvas.Children.Add(polyline);
            m_ElementCanvas.ForceLayout();

            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrushAdjustSize(polyline, polyline.Width, polyline.Height, new Size(maxWidth, maxHeight), false);

            // ACT
            Assert.IsNotNull(bitmap);

            Assert.AreEqual(maxWidth, bitmap.PixelWidth);
            Assert.AreEqual(maxHeight, bitmap.PixelHeight);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);
            Assert.IsNotNull(convertedBitmap);
        }

        [Test]
        public void BitmapOfElementsIsGeneratedWithCorrectZOrder()
        {
            // ARRANGE
            Button button = AddButtonToCanvas(0, 0, 400, 300);
            button.Background = Brushes.Blue;
            Panel.SetZIndex(button, 10);

            button = AddButtonToCanvas(200, 150, 400, 300);
            button.Background = Brushes.Red;
            Panel.SetZIndex(button, 1);

            m_ElementCanvas.ForceLayout();

            // ACT
            BitmapSource bitmap = m_BitmapHelper.CreateBitmapSourceUsingVisualBrush(
                m_ElementCanvas.Children.Cast<FrameworkElement>(),
                new Size(m_ElementCanvas.Width, m_ElementCanvas.Height),
                m_DesiredThumbnailSize);

            // ASSERT
            Assert.IsNotNull(bitmap);
            Assert.AreEqual(100, bitmap.PixelWidth);
            Assert.AreEqual(75, bitmap.PixelHeight);

            Bitmap convertedBitmap = m_BitmapHelper.ConvertBitmapSourceToBitmap(bitmap);

            //Upper left corner should be blue
            Assert.AreEqual(Color.Blue.ToArgb(), convertedBitmap.GetPixel(10, 10).ToArgb());
            //Non-overlapping area where only red testelement visible
            Assert.AreEqual(Color.Red.ToArgb(), convertedBitmap.GetPixel(60, 35).ToArgb());
            //Overlapping area should be blue since first testelement has higher z-index
            Assert.AreEqual(Color.Blue.ToArgb(), convertedBitmap.GetPixel(30, 30).ToArgb());
        }

        #region Helper methods

        private Button AddButtonToCanvas(double left, double top, double width, double height)
        {
            var button = new Button
            {
                Content = "Button",
                Width = width,
                Height = height
            };

            Canvas.SetLeft(button, left);
            Canvas.SetTop(button, top);

            m_ElementCanvas.Children.Add(button);

            return button;
        }

        private string TempFilePath
        {
            get
            {
                string fileName = Path.GetTempFileName();
                m_TempFileNames.Add(fileName);

                Trace.WriteLine("Creating temp file: " + fileName);
                return fileName;
            }
        }

        #endregion
    }
}