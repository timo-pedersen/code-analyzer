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
    public class LayoutObjectAdapterForViewboxTest
    {
        private const double MasterLeft = 50;
        private const double MasterTop = 80;
        private const double MasterWidth = 300;
        private const double MasterHeight = 100;
        private const double InnerRectangleWidth = 200;
        private const double InnerRectangleHeight = 200;
        private const int MasterZIndex = 0;

        private ScreenEditorTestWindow m_ScreenEditor;
        private Viewbox m_MasterViewbox;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            TestHelper.ClearServices();

            var toolManagerMock = MockRepository.GenerateStub<IToolManager>();
            toolManagerMock.Stub(x => x.Runtime).Return(false);
            TestHelper.AddService<IToolManager>(toolManagerMock);

            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();

            m_MasterViewbox = new Viewbox();

            Rectangle rectangle = new Rectangle();
            rectangle.Width = InnerRectangleWidth;
            rectangle.Height = InnerRectangleHeight;
            rectangle.Fill = Brushes.PeachPuff;
            m_MasterViewbox.Child = rectangle;

            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterViewbox);
            m_ScreenEditor.Canvas.Children.Add(m_MasterViewbox);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            m_ScreenEditor.Close();

            TestHelper.ClearServices();
        }

        [SetUp]
        public void SetUp()
        {
            Canvas.SetLeft(m_MasterViewbox, MasterLeft);
            Canvas.SetTop(m_MasterViewbox, MasterTop);
            Canvas.SetZIndex(m_MasterViewbox, MasterZIndex);
            m_MasterViewbox.Width = MasterWidth;
            m_MasterViewbox.Height = MasterHeight;
            m_MasterViewbox.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterViewbox.RenderTransform = Transform.Identity;
            m_MasterViewbox.UpdateLayout();
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
            RotateElement(angle, m_MasterViewbox);

            double width = (Math.Cos(angle / 180 * Math.PI) * MasterWidth) + (Math.Cos((90 - angle) / 180 * Math.PI) * MasterHeight);
            Assert.AreEqual(width, m_MasterLayoutAdapter.BoundingWidth);
        }

        [Test]
        public void BoundingHeightWhenRotated()
        {
            double angle = 30;
            RotateElement(angle, m_MasterViewbox);

            double height = (Math.Sin(angle / 180 * Math.PI) * MasterWidth) + (Math.Sin((90 - angle) / 180 * Math.PI) * MasterHeight);
            Assert.AreEqual(height, m_MasterLayoutAdapter.BoundingHeight);
        }

        [Test]
        public void AfterScalingTheLayoutObjectAdaptersOriginalWidthStillEqualsWidth()
        {
            ScaleElement(2, 4, m_MasterViewbox);
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.OriginalWidth);
        }

        [Test]
        public void AfterScalingTheLayoutObjectAdaptersOriginalHeightStillEqualsHeight()
        {
            ScaleElement(2, 4, m_MasterViewbox);
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.OriginalHeight);
        }

        [Test]
        public void LayoutObjectAdapterWidthEqualsWidth()
        {
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.Width);
        }

        [Test]
        public void LayoutObjectAdapterHeightEqualsHeight()
        {
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void LayoutObjectAdapterWidthIsUnchangedWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.Width);
        }

        [Test]
        public void LayoutObjectAdapterHeightIsUnchangedWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void LayoutObjectAdapterLeftEqualsLeft()
        {
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
        }

        [Test]
        public void ChangingLeftTwiceThenTopTwiceDoesActuallyChangeLeftTwiceAndTopTwice()
        {

            m_MasterLayoutAdapter.Left = 30;
            m_MasterViewbox.UpdateLayout();
            Assert.AreEqual(30, m_MasterLayoutAdapter.Left);

            m_MasterLayoutAdapter.Left = 20;
            m_MasterViewbox.UpdateLayout();
            Assert.AreEqual(20, m_MasterLayoutAdapter.Left);

            m_MasterLayoutAdapter.Top = 100;
            m_MasterViewbox.UpdateLayout();
            Assert.AreEqual(100, m_MasterLayoutAdapter.Top);

            m_MasterLayoutAdapter.Top = 50;
            m_MasterViewbox.UpdateLayout();
            Assert.AreEqual(50, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void LayoutObjectAdapterRightEqualsLeftPlusWidth()
        {
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
        }

        [Test]
        public void LayoutObjectAdapterTopEqualsTop()
        {
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void LayoutObjectAdapterBottomEqualsTopPlusHeight()
        {
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void LayoutObjectAdapterHorizontalCenterEqualsLeftPlusHalfTheWidth()
        {
            Assert.AreEqual(MasterLeft + MasterWidth / 2, m_MasterLayoutAdapter.HorizontalCenter);
        }

        [Test]
        public void LayoutObjectAdapterVerticalCenterEqualsTopPlusHalfTheHeight()
        {
            Assert.AreEqual(MasterTop + MasterHeight / 2, m_MasterLayoutAdapter.VerticalCenter);
        }

        [Test]
        public void LeftWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);

            GeneralTransform generalTransform = m_MasterViewbox.TransformToAncestor((Visual)m_MasterViewbox.Parent);
            Point rotatedTopLeft = generalTransform.Transform(new Point(m_MasterLayoutAdapter.Offset.X, m_MasterLayoutAdapter.Offset.Y));

            Assert.AreEqual(rotatedTopLeft.X, m_MasterLayoutAdapter.Left);
        }

        [Test]
        public void RightWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);

            GeneralTransform generalTransform = m_MasterViewbox.TransformToAncestor((Visual)m_MasterViewbox.Parent);
            Point rotatedBottomRight = generalTransform.Transform(new Point(MasterWidth + m_MasterLayoutAdapter.Offset.X, MasterHeight + m_MasterLayoutAdapter.Offset.Y));

            Assert.AreEqual(rotatedBottomRight.X, m_MasterLayoutAdapter.Right);
        }

        [Test]
        public void TopWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);

            GeneralTransform generalTransform = m_MasterViewbox.TransformToAncestor((Visual)m_MasterViewbox.Parent);
            Point rotatedTopRight = generalTransform.Transform(new Point(MasterWidth + m_MasterLayoutAdapter.Offset.X, m_MasterLayoutAdapter.Offset.Y));

            Assert.AreEqual(rotatedTopRight.Y, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void BottomWhenRotated()
        {
            RotateElement(-30, m_MasterViewbox);

            GeneralTransform generalTransform = m_MasterViewbox.TransformToAncestor((Visual)m_MasterViewbox.Parent);
            Point rotatedBottomLeft = generalTransform.Transform(new Point(m_MasterLayoutAdapter.Offset.X, MasterHeight + m_MasterLayoutAdapter.Offset.Y));

            Assert.AreEqual(rotatedBottomLeft.Y, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeBottomRight()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.TopLeft);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ResizeTopRight()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomLeft);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeTopLeft()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomRight);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(MasterTop + MasterHeight, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void ResizeBottomLeft()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.TopRight);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(400, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(200, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(MasterLeft + MasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void MultipleResizes()
        {
            m_MasterLayoutAdapter.Resize(400, 200, Corner.BottomLeft);
            m_MasterViewbox.UpdateLayout();

            m_MasterLayoutAdapter.Resize(600, 250, Corner.BottomLeft);
            m_MasterViewbox.UpdateLayout();

            m_MasterLayoutAdapter.Resize(250, 150, Corner.BottomLeft);
            m_MasterViewbox.UpdateLayout();

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
            ScaleElement(scaleX, scaleY, m_MasterViewbox);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterViewbox.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterViewbox.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.TopLeft);
            m_MasterViewbox.UpdateLayout();

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
            ScaleElement(scaleX, scaleY, m_MasterViewbox);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterViewbox.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterViewbox.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.BottomLeft);
            m_MasterViewbox.UpdateLayout();

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
            ScaleElement(scaleX, scaleY, m_MasterViewbox);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterViewbox.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterViewbox.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.BottomRight);
            m_MasterViewbox.UpdateLayout();

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
            ScaleElement(scaleX, scaleY, m_MasterViewbox);

            // Recalculate bounds after scale operation.
            double newMasterWidth = MasterWidth * scaleX;
            double newMasterHeight = MasterHeight * scaleY;
            double newMasterLeft = MasterLeft - (newMasterWidth - MasterWidth) * m_MasterViewbox.RenderTransformOrigin.X;
            double newMasterTop = MasterTop - (newMasterHeight - MasterHeight) * m_MasterViewbox.RenderTransformOrigin.Y;

            double newWidth = 400;
            double newHeight = 200;
            m_MasterLayoutAdapter.Resize(newWidth, newHeight, Corner.TopRight);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(newWidth, m_MasterLayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_MasterLayoutAdapter.Height);
            Assert.AreEqual(newMasterLeft + newMasterWidth, m_MasterLayoutAdapter.Right);
            Assert.AreEqual(newMasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ChangingTransformOriginWillNotChangeLeftAndTop()
        {
            m_MasterLayoutAdapter.TransformOrigin = new Point(0.1, 0.1);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void ChangingTransformOriginWhenRotatedWillNotChangeLeftAndTop()
        {
            RotateElement(-30, m_MasterViewbox);
            double expectedLeft = Math.Round(m_MasterLayoutAdapter.Left, 4);
            double expectedTop = Math.Round(m_MasterLayoutAdapter.Top, 4);

            m_MasterLayoutAdapter.TransformOrigin = new Point(0.1, 0.1);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(expectedLeft, Math.Round(m_MasterLayoutAdapter.Left, 4));
            Assert.AreEqual(expectedTop, Math.Round(m_MasterLayoutAdapter.Top, 4));
        }

        [Test]
        public void ChangeZIndex()
        {
            m_MasterLayoutAdapter.ZIndex = 1;

            Assert.AreEqual(MasterZIndex + 1, m_MasterLayoutAdapter.ZIndex);
        }

        [Test]
        public void LeftOffset()
        {
            Assert.AreEqual(-100, m_MasterLayoutAdapter.Offset.X);
            Assert.AreEqual(0, m_MasterLayoutAdapter.Offset.Y);
        }

        [Test]
        public void LeftScaledOffset()
        {
            m_MasterViewbox.RenderTransform = new ScaleTransform(2, 2);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(-200, m_MasterLayoutAdapter.ScaledOffset.X);
            Assert.AreEqual(0, m_MasterLayoutAdapter.ScaledOffset.Y);
        }

        [Test]
        public void TopOffset()
        {
            m_MasterLayoutAdapter.Width = 100;
            m_MasterLayoutAdapter.Height = 300;

            Assert.AreEqual(0, m_MasterLayoutAdapter.Offset.X);
            Assert.AreEqual(-100, m_MasterLayoutAdapter.Offset.Y);
        }

        [Test]
        public void TopScaledOffset()
        {
            m_MasterLayoutAdapter.Width = 100;
            m_MasterLayoutAdapter.Height = 300;

            m_MasterViewbox.RenderTransform = new ScaleTransform(2, 2);
            m_MasterViewbox.UpdateLayout();

            Assert.AreEqual(0, m_MasterLayoutAdapter.ScaledOffset.X);
            Assert.AreEqual(-200, m_MasterLayoutAdapter.ScaledOffset.Y);
        }

        private void RotateElement(double angle, UIElement element)
        {
            m_MasterLayoutAdapter.RotationAngle = angle;
            element.UpdateLayout();
        }

        private static void ScaleElement(double scaleX, double scaleY, UIElement element)
        {
            element.RenderTransform = new ScaleTransform(scaleX, scaleY);
            element.UpdateLayout();
        }
    }
}
