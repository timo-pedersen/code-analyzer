using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class LayoutObjectAdapterPolylineTest
    {
        private const double MasterLeft = 20.5;
        private const double MasterRight = 138.2;
        private const double MasterTop = 10.8;
        private const double MasterBottom = 102;

        private ScreenEditorTestWindow m_ScreenEditor;
        private Polyline m_MasterPolyline;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            var toolManagerMock = MockRepository.GenerateStub<IToolManager>();
            toolManagerMock.Stub(x => x.Runtime).Return(false);
            TestHelper.AddService<IToolManager>(toolManagerMock);

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();

            m_MasterPolyline = new Polyline();
            m_MasterPolyline.Stroke = Brushes.Black;
            m_MasterPolyline.Points.Add(new Point(MasterLeft, MasterTop));
            m_MasterPolyline.Points.Add(new Point(105, 50.3));
            m_MasterPolyline.Points.Add(new Point(MasterRight, MasterBottom));
            m_MasterPolyline.Points.Add(new Point(48, 95.6));
            m_MasterPolyline.Points.Add(new Point(35.1, 78));
            m_MasterPolyline.Points.Add(new Point(MasterLeft, MasterTop));
            m_ScreenEditor.Canvas.Children.Add(m_MasterPolyline);
            m_MasterPolyline.UpdateLayout();
            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterPolyline);
            ((LayoutObjectAdapterPolyline)m_MasterLayoutAdapter).Normalize(m_MasterPolyline);

            m_MasterPolyline.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterPolyline.RenderTransform = Transform.Identity;
            Canvas.SetLeft(m_MasterPolyline, MasterLeft);
            Canvas.SetTop(m_MasterPolyline, MasterTop);
            m_MasterPolyline.UpdateLayout();
        }

        [TearDown]
        public void TearDown()
        {
            m_ScreenEditor.Close();

            TestHelper.ClearServices();
        }

        [Test]
        public void BoundingWidth()
        {
            Assert.AreEqual(Math.Round(MasterRight - MasterLeft,4), Math.Round(m_MasterLayoutAdapter.BoundingWidth, 4));
        }

        [Test]
        public void BoundingHeight()
        {
            Assert.AreEqual(MasterBottom - MasterTop, Math.Round(m_MasterLayoutAdapter.BoundingHeight, 4));
        }

        [Test]
        public void BoundingWidthWhenRotated()
        {
            double angle = 30;
            RotateElement(angle, m_MasterPolyline);

            Rect bounds = new Rect(0, 0, MasterRight - MasterLeft, MasterBottom - MasterTop);
            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Rect transformedBounds = generalTransform.TransformBounds(bounds);

            Assert.AreEqual(Math.Round(transformedBounds.Width, 4), Math.Round(m_MasterLayoutAdapter.BoundingWidth, 4));
        }

        [Test]
        public void BoundingHeightWhenRotated()
        {
            double angle = 30;
            RotateElement(angle, m_MasterPolyline);

            Rect bounds = new Rect(0, 0, MasterRight - MasterLeft, MasterBottom - MasterTop);
            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Rect transformedBounds = generalTransform.TransformBounds(bounds);

            Assert.AreEqual(Math.Round(transformedBounds.Height, 4), Math.Round(m_MasterLayoutAdapter.BoundingHeight, 4));
        }

        [Test]
        public void OriginalWidth()
        {
            ScaleElement(2, 4, m_MasterPolyline);
            double width = MasterRight - MasterLeft;
            Assert.AreEqual(Math.Round(width,4), Math.Round(m_MasterLayoutAdapter.OriginalWidth, 4));
        }

        [Test]
        public void OriginalHeight()
        {
            ScaleElement(2, 4, m_MasterPolyline);
            double height = MasterBottom - MasterTop;
            Assert.AreEqual(height, Math.Round(m_MasterLayoutAdapter.OriginalHeight, 4));
        }

        [Test]
        public void Width()
        {
            double width = MasterRight - MasterLeft;
            Assert.AreEqual(Math.Round(width, 4), Math.Round(m_MasterLayoutAdapter.BoundingWidth, 4));
        }

        [Test]
        public void Height()
        {
            double height = MasterBottom - MasterTop;
            Assert.AreEqual(height, Math.Round(m_MasterLayoutAdapter.BoundingHeight, 4));
        }

        [Test]
        public void WidthWhenRotated()
        {
            double width = m_MasterLayoutAdapter.Width;
            RotateElement(-30, m_MasterPolyline);
            Assert.AreEqual(width, m_MasterLayoutAdapter.Width);
        }

        [Test]
        public void HeightWhenRotated()
        {
            double height = m_MasterLayoutAdapter.Height;
            RotateElement(-30, m_MasterPolyline);
            Assert.AreEqual(height, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void Left()
        {
            Assert.AreEqual(MasterLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
        }

        [Test]
        public void Right()
        {
            Assert.AreEqual(MasterRight, Math.Round(m_MasterLayoutAdapter.Right, 4));
        }

        [Test]
        public void Top()
        {
            Assert.AreEqual(MasterTop, Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void Bottom()
        {
            Assert.AreEqual(MasterBottom, Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        [Test]
        public void HorizontalCenter()
        {
            Assert.AreEqual(MasterLeft + (MasterRight - MasterLeft) / 2, Math.Round(m_MasterLayoutAdapter.HorizontalCenter, 4));
        }

        [Test]
        public void VerticalCenter()
        {
            Assert.AreEqual(Math.Round(MasterTop + (MasterBottom - MasterTop) / 2,4), Math.Round(m_MasterLayoutAdapter.VerticalCenter, 4));
        }

        [Test]
        public void LeftWhenRotated()
        {
            RotateElement(-30, m_MasterPolyline);

            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Point rotatedTopLeft = generalTransform.Transform(new Point(0, 0));

            Assert.AreEqual(Math.Round(rotatedTopLeft.X, 4), Math.Round(m_MasterLayoutAdapter.Left, 4));
        }

        [Test]
        public void RightWhenRotated()
        {
            RotateElement(-30, m_MasterPolyline);

            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Point rotatedBottomRight = generalTransform.Transform(new Point(MasterRight - MasterLeft, MasterBottom - MasterTop));

            Assert.AreEqual(Math.Round(rotatedBottomRight.X, 3), Math.Round(m_MasterLayoutAdapter.Right, 3));
        }

        [Test]
        public void TopWhenRotated()
        {
            RotateElement(-30, m_MasterPolyline);

            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Point rotatedTopRight = generalTransform.Transform(new Point(MasterRight - MasterLeft, 0));

            Assert.AreEqual(Math.Round(rotatedTopRight.Y, 4), Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void BottomWhenRotated()
        {
            RotateElement(-30, m_MasterPolyline);

            GeneralTransform generalTransform = m_MasterPolyline.TransformToAncestor((Visual)m_MasterPolyline.Parent);
            Point rotatedBottomLeft = generalTransform.Transform(new Point(0, MasterBottom - MasterTop));

            Assert.AreEqual(Math.Round(rotatedBottomLeft.Y, 4), Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        [Test]
        public void ResizeBottomRight()
        {
            m_MasterLayoutAdapter.Resize(200, 100, Corner.TopLeft);
            m_MasterPolyline.UpdateLayout();

            AssertAreWithinTolerance(200, m_MasterLayoutAdapter.Width);
            AssertAreWithinTolerance(100, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
            Assert.AreEqual(MasterTop, Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void ResizeTopRight()
        {
            m_MasterLayoutAdapter.Resize(200, 100, Corner.BottomLeft);
            m_MasterPolyline.UpdateLayout();

            AssertAreWithinTolerance(200, m_MasterLayoutAdapter.Width);
            AssertAreWithinTolerance(100, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
            Assert.AreEqual(MasterBottom, Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        [Test]
        public void ResizeTopLeft()
        {
            m_MasterLayoutAdapter.Resize(200, 100, Corner.BottomRight);
            m_MasterPolyline.UpdateLayout();

            AssertAreWithinTolerance(200, m_MasterLayoutAdapter.Width);
            AssertAreWithinTolerance(100, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterRight, Math.Round(m_MasterLayoutAdapter.Right, 4));
            Assert.AreEqual(MasterBottom, Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        [Test]
        public void ResizeBottomLeft()
        {
            m_MasterLayoutAdapter.Resize(200, 100, Corner.TopRight);
            m_MasterPolyline.UpdateLayout();

            AssertAreWithinTolerance(200, m_MasterLayoutAdapter.Width);
            AssertAreWithinTolerance(100, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterRight, Math.Round(m_MasterLayoutAdapter.Right, 4));
            Assert.AreEqual(MasterTop, Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void MultipleResizes()
        {
            m_MasterLayoutAdapter.Resize(200, 100, Corner.BottomLeft);
            m_MasterPolyline.UpdateLayout();

            m_MasterLayoutAdapter.Resize(300, 200, Corner.BottomLeft);
            m_MasterPolyline.UpdateLayout();

            m_MasterLayoutAdapter.Resize(250, 150, Corner.BottomLeft);
            m_MasterPolyline.UpdateLayout();

            AssertAreWithinTolerance(250, m_MasterLayoutAdapter.Width);
            AssertAreWithinTolerance(150, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
            Assert.AreEqual(MasterBottom, Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        private void RotateElement(double angle, FrameworkElement element)
        {
            m_MasterLayoutAdapter.RotationAngle = angle;
            element.UpdateLayout();
        }

        private void ScaleElement(double scaleX, double scaleY, FrameworkElement element)
        {
            element.RenderTransform = new ScaleTransform(scaleX, scaleY);
            element.UpdateLayout();
        }

        /// <summary>
        /// Method used to assert that actual value is within tolerance. The reason this method
        /// exists is because the resize implementation of <see cref="LayoutObjectAdapterPolyline"/>
        /// isn't as accurate as one might expect.
        /// </summary>
        private static void AssertAreWithinTolerance(double expected, double actual)
        {
            // 2% tolerance
            const double tolerance = 0.02;

            double min = expected * (1 - tolerance);
            double max = expected * (1 + tolerance);

            Assert.That(actual, Is.GreaterThanOrEqualTo(min));
            Assert.That(actual, Is.LessThanOrEqualTo(max));
        }
    }
}
