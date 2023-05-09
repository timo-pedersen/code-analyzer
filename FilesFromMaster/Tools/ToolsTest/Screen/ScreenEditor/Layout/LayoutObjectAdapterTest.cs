using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Layout
{
    [TestFixture]
    public class LayoutObjectAdapterTest
    {
        private const double MasterLeft = 50;
        private const double MasterTop = 80;
        private const double MasterWidth = 300;
        private const double MasterHeight = 100;
        private const int MasterZIndex = 0;

        private ScreenEditorTestWindow m_ScreenEditor;
        private Rectangle m_MasterRectangle;
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

            m_MasterRectangle = new Rectangle();
            m_MasterRectangle.Stroke = Brushes.Black;
            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterRectangle);
            m_ScreenEditor.Canvas.Children.Add(m_MasterRectangle);

            Canvas.SetLeft(m_MasterRectangle, MasterLeft);
            Canvas.SetTop(m_MasterRectangle, MasterTop);
            Canvas.SetZIndex(m_MasterRectangle, MasterZIndex);
            m_MasterRectangle.MinWidth = 0;
            m_MasterRectangle.MinHeight = 0;
            m_MasterRectangle.MaxWidth = double.PositiveInfinity;
            m_MasterRectangle.MaxHeight = double.PositiveInfinity;

            m_MasterRectangle.Width = MasterWidth;
            m_MasterRectangle.Height = MasterHeight;
            m_MasterRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterRectangle.RenderTransform = Transform.Identity;
            m_MasterRectangle.UpdateLayout();
        }

        [TearDown]
        public void FixtureTearDown()
        {
            m_ScreenEditor.Close();

            TestHelper.ClearServices();
        }

        [Test]
        public void BoundingWidth()
        {
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.BoundingWidth);
        }

        [Test]
        public void BoundingHeight()
        {
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.BoundingHeight);
        }

        [Test]
        public void BoundingWidthWhenRotated()
        {
            double angle = 30;
            RotateElement(angle, m_MasterRectangle);

            double width = (Math.Cos(angle / 180 * Math.PI) * MasterWidth) + (Math.Cos((90 - angle) / 180 * Math.PI) * MasterHeight);
            Assert.AreEqual(width, m_MasterLayoutAdapter.BoundingWidth);
        }

        [Test]
        public void BoundingHeightWhenRotated()
        {
            double angle = 30;
            RotateElement(angle, m_MasterRectangle);

            double height = (Math.Sin(angle / 180 * Math.PI) * MasterWidth) + (Math.Sin((90 - angle) / 180 * Math.PI) * MasterHeight);
            Assert.AreEqual(height, m_MasterLayoutAdapter.BoundingHeight);
        }

        [Test]
        public void OriginalWidth()
        {
            ScaleElement(2, 4, m_MasterRectangle);
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.OriginalWidth);
        }

        [Test]
        public void OriginalHeight()
        {
            ScaleElement(2, 4, m_MasterRectangle);
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.OriginalHeight);
        }

        [Test]
        public void Width()
        {
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.Width);
        }
        
        [Test]
        public void WidthIsCoercedWithMinSize()
        {
            m_MasterRectangle.MinWidth = 10;

            m_MasterLayoutAdapter.Width = 5;

            Assert.That(m_MasterLayoutAdapter.Width, Is.EqualTo(10));
        }

        [Test]
        public void WidthIsCoercedWithDefaultMinimumSize()
        {
            m_MasterLayoutAdapter.Width = 3;

            Assert.That(m_MasterLayoutAdapter.Width, Is.EqualTo(4));
        }

        [Test]
        public void WidthIsCoercedWithMaxSize()
        {
            m_MasterRectangle.MaxWidth = 10;

            m_MasterLayoutAdapter.Width = 15;

            Assert.That(m_MasterLayoutAdapter.Width, Is.EqualTo(10));
        }

        [Test]
        public void Height()
        {
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void HeightIsCoercedWithMinSize()
        {
            m_MasterRectangle.MinHeight = 10;

            m_MasterLayoutAdapter.Height = 5;
            
            Assert.That(m_MasterLayoutAdapter.Height, Is.EqualTo(10));
        }

        [Test]
        public void HeightIsCoercedWithDefaultMinimumSize()
        {
            m_MasterLayoutAdapter.Height = 3;

            Assert.That(m_MasterLayoutAdapter.Height, Is.EqualTo(4));
        }
        
        [Test]
        public void HeightIsCoercedWithMaxSize()
        {
            m_MasterRectangle.MaxHeight = 10;

            m_MasterLayoutAdapter.Height = 15;

            Assert.That(m_MasterLayoutAdapter.Height, Is.EqualTo(10));
        }

        [Test]
        public void WidthWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.Width);
        }

        [Test]
        public void HeightWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void Left()
        {
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
        }

        [Test]
        public void MoveMultipleSteps()
        {

            m_MasterLayoutAdapter.Left = 30;
            m_MasterRectangle.UpdateLayout();
            Assert.AreEqual(30, m_MasterLayoutAdapter.Left);
            m_MasterLayoutAdapter.Left = 20;
            m_MasterRectangle.UpdateLayout();
            Assert.AreEqual(20, m_MasterLayoutAdapter.Left);
            m_MasterLayoutAdapter.Top = 100;
            m_MasterRectangle.UpdateLayout();
            Assert.AreEqual(100, m_MasterLayoutAdapter.Top);
            m_MasterLayoutAdapter.Top = 50;
            m_MasterRectangle.UpdateLayout();
            Assert.AreEqual(50, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void Right()
        {
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
        }

        [Test]
        public void Top()
        {
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void Bottom()
        {
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void HorizontalCenter()
        {
            Assert.AreEqual(MasterLeft + MasterWidth / 2, m_MasterLayoutAdapter.HorizontalCenter);
        }

        [Test]
        public void VerticalCenter()
        {
            Assert.AreEqual(MasterTop + MasterHeight / 2, m_MasterLayoutAdapter.VerticalCenter);
        }

        [Test]
        public void LeftWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);

            GeneralTransform generalTransform = m_MasterRectangle.TransformToAncestor((Visual)m_MasterRectangle.Parent);
            Point rotatedTopLeft = generalTransform.Transform(new Point(0, 0));

            Assert.AreEqual(rotatedTopLeft.X, m_MasterLayoutAdapter.Left);
        }

        [Test]
        public void RightWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);

            GeneralTransform generalTransform = m_MasterRectangle.TransformToAncestor((Visual)m_MasterRectangle.Parent);
            Point rotatedBottomRight = generalTransform.Transform(new Point(MasterWidth, MasterHeight));

            Assert.AreEqual(rotatedBottomRight.X, m_MasterLayoutAdapter.Right);
        }

        [Test]
        public void TopWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);

            GeneralTransform generalTransform = m_MasterRectangle.TransformToAncestor((Visual)m_MasterRectangle.Parent);
            Point rotatedTopRight = generalTransform.Transform(new Point(MasterWidth, 0));

            Assert.AreEqual(rotatedTopRight.Y, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void BottomWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);

            GeneralTransform generalTransform = m_MasterRectangle.TransformToAncestor((Visual)m_MasterRectangle.Parent);
            Point rotatedBottomLeft = generalTransform.Transform(new Point(0, MasterHeight));

            Assert.AreEqual(rotatedBottomLeft.Y, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeBottomRight()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ResizeTopRight()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeTopLeft()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomRight);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeBottomLeft()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.TopRight);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void MultipleResizes()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomLeft);
            m_MasterRectangle.UpdateLayout();

            m_MasterLayoutAdapter.Resize(600, 250, Corner.BottomLeft);
            m_MasterRectangle.UpdateLayout();

            m_MasterLayoutAdapter.Resize(250, 150, Corner.BottomLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(250, Math.Round(m_MasterLayoutAdapter.Width, 4));
            Assert.AreEqual(150, Math.Round(m_MasterLayoutAdapter.Height, 4));
            Assert.AreEqual(MasterLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
            Assert.AreEqual(MasterTop + MasterHeight, Math.Round(m_MasterLayoutAdapter.Bottom, 4));
        }

        [Test]
        public void ResizeBottomRightWhenScaled()
        {
            double scaleX = 2;
            double scaleY = 4;
            ScaleElement(scaleX, scaleY, m_MasterRectangle);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterRectangle.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterRectangle.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(newWidth, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(newMasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(newMasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ResizeTopRightWhenScaled()
        {
            double scaleX = 2;
            double scaleY = 4;
            ScaleElement(scaleX, scaleY, m_MasterRectangle);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterRectangle.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterRectangle.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.BottomLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(newWidth, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(newMasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(newMasterTop + newMasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeTopLeftWhenScaled()
        {
            double scaleX = 2;
            double scaleY = 4;
            ScaleElement(scaleX, scaleY, m_MasterRectangle);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterRectangle.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterRectangle.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.BottomRight);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(newWidth, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(newMasterLeft + newMasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(newMasterTop + newMasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeBottomLeftWhenScaled()
        {
            double scaleX = 2;
            double scaleY = 4;
            ScaleElement(scaleX, scaleY, m_MasterRectangle);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterRectangle.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterRectangle.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.TopRight);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(newWidth, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(newMasterLeft + newMasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(newMasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ChangeTransformOrigin()
        {
            m_MasterLayoutAdapter.TransformOrigin = new Point(0.1, 0.1);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ChangeTransformOriginWhenRotated()
        {
            RotateElement(-30, m_MasterRectangle);
            double left = m_MasterLayoutAdapter.Left;
            double top = m_MasterLayoutAdapter.Top;

            m_MasterLayoutAdapter.TransformOrigin = new Point(0.1, 0.1);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(left, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(Math.Round(top, 4), Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void ChangeZIndex()
        {
            m_MasterLayoutAdapter.ZIndex = 1;

            Assert.AreEqual(MasterZIndex + 1, m_MasterLayoutAdapter.ZIndex);
        }

        [Test]
        public void NoOffset()
        {
            Assert.AreEqual(0, m_MasterLayoutAdapter.Offset.X);
            Assert.AreEqual(0, m_MasterLayoutAdapter.Offset.Y);
        }

        [Test]
        public void NoScaledOffset()
        {
            Assert.AreEqual(0, m_MasterLayoutAdapter.ScaledOffset.X);
            Assert.AreEqual(0, m_MasterLayoutAdapter.ScaledOffset.Y);
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
    }
}
