using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class LayoutObjectAdapterLineTest
    {
        private const double MasterLeft = 105.5;
        private const double MasterTop = 50.2;
        private const double MasterRight = 310.8;
        private const double MasterBottom = 250;

        private Line m_MasterLine;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;
        private ScreenEditorTestWindow m_ScreenEditor;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            var toolManagerMock = Substitute.For<IToolManager>();
            toolManagerMock.Runtime.Returns(false);
            TestHelper.AddService<IToolManager>(toolManagerMock);

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();

            m_MasterLine = new Line();
            m_MasterLine.Stroke = Brushes.Black;

            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterLine);
            m_ScreenEditor.Canvas.Children.Add(m_MasterLine);

            m_MasterLine.X1 = MasterLeft;
            m_MasterLine.Y1 = MasterTop;
            m_MasterLine.X2 = MasterRight;
            m_MasterLine.Y2 = MasterBottom;
            //m_MasterLine.RenderTransformOrigin = new Point(0.5, 0.5);
            //m_MasterLine.RenderTransform = Transform.Identity;
            m_MasterLine.UpdateLayout();
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
            Assert.AreEqual(MasterRight - MasterLeft, m_MasterLayoutAdapter.BoundingWidth);
        }

        [Test]
        public void BoundingHeight()
        {
            Assert.AreEqual(MasterBottom - MasterTop, m_MasterLayoutAdapter.BoundingHeight);
        }

        [Test]
        public void BoundingBox()
        {
            Rect bounds = m_MasterLayoutAdapter.BoundingBox;
            Assert.AreEqual(m_MasterLine.X1, bounds.Left);
            Assert.AreEqual(m_MasterLine.Y1, bounds.Top);
            Assert.AreEqual(m_MasterLine.X2 - m_MasterLine.X1, bounds.Width);
            Assert.AreEqual(m_MasterLine.Y2 - m_MasterLine.Y1, bounds.Height);
        }

        [Test]
        public void Width()
        {
            Assert.AreEqual(MasterRight - MasterLeft, m_MasterLayoutAdapter.Width);
        }

        [Test]
        public void Height()
        {
            Assert.AreEqual(MasterBottom - MasterTop, m_MasterLayoutAdapter.Height);
        }

        [Test]
        public void SetWidth()
        {
            m_MasterLayoutAdapter.Width = 500;
            m_MasterLine.UpdateLayout();

            Assert.AreEqual(500, Math.Round(m_MasterLayoutAdapter.Width, 4));
        }

        [Test]
        public void SetHeightHasNoEffect()
        {
            double thickness = m_MasterLine.StrokeThickness;
            m_MasterLayoutAdapter.Height = 10;

            Assert.AreEqual(thickness, m_MasterLine.StrokeThickness);
        }

        [Test]
        public void Left()
        {
            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left);
        }

        [Test]
        public void Right()
        {
            Assert.AreEqual(MasterRight, m_MasterLayoutAdapter.Right);
        }

        [Test]
        public void Top()
        {
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top);
        }

        [Test]
        public void Bottom()
        {
            Assert.AreEqual(MasterBottom, m_MasterLayoutAdapter.Bottom);
        }

        [Test]
        public void HorizontalCenter()
        {
            Assert.AreEqual(MasterLeft + (MasterRight - MasterLeft) / 2, m_MasterLayoutAdapter.HorizontalCenter);
        }

        [Test]
        public void VerticalCenter()
        {
            Assert.AreEqual(MasterTop + (MasterBottom - MasterTop) / 2, m_MasterLayoutAdapter.VerticalCenter);
        }

        [Test]
        public void HorizontalThinLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 100,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void HorizontalThickLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 4
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 100,
                StrokeThickness = 4
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void VerticalThinLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 200,
                Y2 = 400,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 100,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void VerticalThickLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 200,
                Y2 = 400,
                StrokeThickness = 4
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 100,
                Y2 = 200,
                StrokeThickness = 4
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void SlopeThinLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 400,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.Width, realLayoutObjectAdapter.Height);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void SlopeThickLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 400,
                StrokeThickness = 4
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 200,
                StrokeThickness = 4
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 0, 0, realLayoutObjectAdapter.Width, realLayoutObjectAdapter.Height);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void PositiveOffsetNoScaleLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 220,
                Y1 = 220,
                X2 = 420,
                Y2 = 220,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(1d, 1d, 20d, 20d, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void NegativeOffsetNoScaleLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 180,
                Y1 = 180,
                X2 = 380,
                Y2 = 180,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(1d, 1d, -20d, -20d, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void PositiveOffsetScaleLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 420,
                Y1 = 420,
                X2 = 820,
                Y2 = 420,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, 20d, 20d, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        [Test]
        public void NegativeOffsetScaleLineTranslate()
        {
            m_ScreenEditor.Canvas.Children.Clear();

            var expectedLine = new Line
            {
                X1 = 380,
                Y1 = 380,
                X2 = 780,
                Y2 = 380,
                StrokeThickness = 1
            };

            expectedLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(expectedLine);

            var realLine = new Line
            {
                X1 = 200,
                Y1 = 200,
                X2 = 400,
                Y2 = 200,
                StrokeThickness = 1
            };

            realLine.UpdateLayout();
            m_ScreenEditor.Canvas.Children.Add(realLine);

            ILayoutObjectAdapter expectedLayoutObjectAdapter =
                new LayoutObjectAdapterLine(expectedLine);

            ILayoutObjectAdapter realLayoutObjectAdapter =
                new LayoutObjectAdapterLine(realLine);

            realLayoutObjectAdapter.Translate(2d, 2d, -20d, -20d, realLayoutObjectAdapter.OriginalWidth, realLayoutObjectAdapter.OriginalHeight);

            Assert.IsTrue(AssertLineEquality(expectedLayoutObjectAdapter, realLayoutObjectAdapter));
        }

        private static bool AssertLineEquality(ILayoutObjectAdapter expected, ILayoutObjectAdapter real)
        {
            const double tolerance = 0.1d;

            return
                Math.Abs(expected.GetValue(Line.X1Property) - real.GetValue(Line.X1Property)) < tolerance &&
                Math.Abs(expected.GetValue(Line.Y1Property) - real.GetValue(Line.Y1Property)) < tolerance &&
                Math.Abs(expected.GetValue(Line.X2Property) - real.GetValue(Line.X2Property)) < tolerance &&
                Math.Abs(expected.GetValue(Line.Y2Property) - real.GetValue(Line.Y2Property)) < tolerance &&
                Math.Abs(expected.Width - real.Width) < tolerance &&
                Math.Abs(expected.Height - real.Height) < tolerance &&
                Math.Abs(expected.GetValue(Shape.StrokeThicknessProperty) - real.GetValue(Shape.StrokeThicknessProperty)) < tolerance;
        }
    }
}
