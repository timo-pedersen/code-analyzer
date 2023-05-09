#if!VNEXT_TARGET
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.TypeConverters;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Brush;
using Neo.ApplicationFramework.Tools.PropertyGrid;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class GroupTest
    {
        private ElementCanvas m_Canvas;

        private Rectangle m_InnerRectangle;
        private Group m_InnerGroup;

        private Ellipse m_OuterEllipse;
        private Group m_OuterGroup;

        private Window m_Window;

        private IToolManager m_ToolManager;

        private WPFToCFTypeDescriptionProvider m_WPFToCFTypeDescriptionProvider;

        [SetUp]
        public void TestFixtureSetUp()
        {
            m_WPFToCFTypeDescriptionProvider = new WPFToCFTypeDescriptionProvider(typeof(object));
            TypeDescriptor.AddProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            m_ToolManager = Substitute.For<IToolManager>();
            m_ToolManager.Runtime.Returns(true);

            TestHelper.ClearServices();
            TestHelper.AddService(m_ToolManager);
            
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            TestHelper.AddService<IObjectPropertyService>(new ObjectPropertyService());

            m_Canvas = new ElementCanvas();
            m_Canvas.Width = 1024;
            m_Canvas.Height = 768;

            m_Window = new Window();
            m_Window.Content = m_Canvas;
            m_Window.SizeToContent = SizeToContent.WidthAndHeight;

            m_InnerRectangle = new Rectangle();
            m_InnerRectangle.Fill = Brushes.Blue;
            m_InnerRectangle.Stroke = Brushes.Red;
            m_InnerRectangle.StrokeThickness = 10;
            m_InnerRectangle.Width = GroupCFTest.InnerRectangleWidth;
            m_InnerRectangle.Height = GroupCFTest.InnerRectangleHeight;

            m_InnerGroup = new Group();
            m_InnerGroup.Width = GroupCFTest.InnerGroupWidth;
            m_InnerGroup.Height = GroupCFTest.InnerGroupHeight;
            m_InnerGroup.Items.Add(m_InnerRectangle);

            m_OuterEllipse = new Ellipse();
            m_OuterEllipse.Fill = Brushes.Red;
            m_OuterEllipse.Stroke = Brushes.Blue;
            m_OuterEllipse.StrokeThickness = 5;
            m_OuterEllipse.Width = GroupCFTest.OuterEllipseWidth;
            m_OuterEllipse.Height = GroupCFTest.OuterEllipseHeight;

            m_OuterGroup = new Group();
            m_OuterGroup.Width = GroupCFTest.OuterGroupWidth;
            m_OuterGroup.Height = GroupCFTest.OuterGroupHeight;
            m_OuterGroup.Items.Add(m_OuterEllipse);
            m_OuterGroup.Items.Add(m_InnerGroup);

            m_Canvas.Children.Add(m_OuterGroup);

            Canvas.SetLeft(m_InnerRectangle, GroupCFTest.InnerRectangleLeft);
            Canvas.SetTop(m_InnerRectangle, GroupCFTest.InnerRectangleTop);
            Canvas.SetLeft(m_InnerGroup, GroupCFTest.InnerGroupLeft);
            Canvas.SetTop(m_InnerGroup, GroupCFTest.InnerGroupTop);
            Canvas.SetLeft(m_OuterEllipse, GroupCFTest.OuterEllipseLeft);
            Canvas.SetTop(m_OuterEllipse, GroupCFTest.OuterEllipseTop);
            Canvas.SetLeft(m_OuterGroup, GroupCFTest.OuterGroupLeft);
            Canvas.SetTop(m_OuterGroup, GroupCFTest.OuterGroupTop);
        }
         
        [TearDown]
        public void TestFixtureTearDown()
        {
            m_Window.Close();

            TypeDescriptor.RemoveProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            TestHelper.ClearServices();
        }

        [Test]
        public void OriginalGroupBounds()
        {
            InitGroups();

            Assert.AreEqual(GroupCFTest.InnerGroupLeft, m_InnerGroup.GroupLogic.OriginalGroupBounds.Left);
            Assert.AreEqual(GroupCFTest.InnerGroupTop, m_InnerGroup.GroupLogic.OriginalGroupBounds.Top);
            Assert.AreEqual(GroupCFTest.InnerGroupWidth, m_InnerGroup.GroupLogic.OriginalGroupBounds.Width);
            Assert.AreEqual(GroupCFTest.InnerGroupHeight, m_InnerGroup.GroupLogic.OriginalGroupBounds.Height);

            Assert.AreEqual(GroupCFTest.OuterGroupLeft, m_OuterGroup.GroupLogic.OriginalGroupBounds.Left);
            Assert.AreEqual(GroupCFTest.OuterGroupTop, m_OuterGroup.GroupLogic.OriginalGroupBounds.Top);
            Assert.AreEqual(GroupCFTest.OuterGroupWidth, m_OuterGroup.GroupLogic.OriginalGroupBounds.Width);
            Assert.AreEqual(GroupCFTest.OuterGroupHeight, m_OuterGroup.GroupLogic.OriginalGroupBounds.Height);
        }

        [Test]
        public void OriginalComponentBounds()
        {
            InitGroups();

            Assert.AreEqual(GroupCFTest.InnerGroupLeft, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Left);
            Assert.AreEqual(GroupCFTest.InnerGroupTop, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Top);
            Assert.AreEqual(GroupCFTest.InnerGroupWidth, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Width);
            Assert.AreEqual(GroupCFTest.InnerGroupHeight, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Height);

            Assert.AreEqual(GroupCFTest.OuterEllipseLeft, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Left);
            Assert.AreEqual(GroupCFTest.OuterEllipseTop, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Top);
            Assert.AreEqual(GroupCFTest.OuterEllipseWidth, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Width);
            Assert.AreEqual(GroupCFTest.OuterEllipseHeight, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Height);

            Assert.AreEqual(GroupCFTest.InnerRectangleLeft, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Left);
            Assert.AreEqual(GroupCFTest.InnerRectangleTop, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Top);
            Assert.AreEqual(GroupCFTest.InnerRectangleWidth, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Width);
            Assert.AreEqual(GroupCFTest.InnerRectangleHeight, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Height);
        }

        [Test]
        public void OriginalBoundsForScaledRectangle()
        {
            double scaleFactor = 0.5;
            m_OuterEllipse.RenderTransform = new ScaleTransform(scaleFactor, scaleFactor);

            InitGroups();

            Assert.AreEqual(GroupCFTest.OuterEllipseLeft * Math.Pow(scaleFactor, 2), m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Left);
            Assert.AreEqual(GroupCFTest.OuterEllipseTop * Math.Pow(scaleFactor, 2), m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Top);
            Assert.AreEqual(GroupCFTest.OuterEllipseWidth * scaleFactor, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Width);
            Assert.AreEqual(GroupCFTest.OuterEllipseHeight * scaleFactor, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Height);
        }

        [Test]
        public void OriginalBoundsForScaledLine()
        {
            int lineLeft = 10;
            int lineTop = 10;
            int lineRight = GroupCFTest.OuterGroupWidth - 10;
            int lineBottom = GroupCFTest.OuterGroupHeight - 10;

            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = lineLeft;
            line.Y1 = lineBottom;
            line.X2 = lineRight;
            line.Y2 = lineTop;
            m_OuterGroup.Items.Add(line);

            double scaleFactor = 0.5;
            line.RenderTransform = new ScaleTransform(scaleFactor, scaleFactor);

            InitGroups();

            ILayoutObjectAdapter layoutObjectAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(line);

            Assert.AreEqual(lineLeft * scaleFactor, layoutObjectAdapter.Left);
            Assert.AreEqual((lineRight - lineLeft) * scaleFactor, layoutObjectAdapter.Width);

            Assert.AreEqual(lineTop * scaleFactor, layoutObjectAdapter.Top);
            Assert.AreEqual((lineBottom - lineTop) * scaleFactor, layoutObjectAdapter.Height);
        }

        [Test]
        public void LocationsAreInLCS()
        {
            InitGroups();

            Assert.AreEqual(GroupCFTest.InnerGroupLeft, InnerGroup.Left);
            Assert.AreEqual(GroupCFTest.InnerGroupTop, InnerGroup.Top);

            Assert.AreEqual(GroupCFTest.InnerRectangleLeft, Canvas.GetLeft(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.InnerRectangleTop, Canvas.GetTop(m_InnerRectangle));

            Assert.AreEqual(GroupCFTest.OuterGroupLeft, OuterGroup.Left);
            Assert.AreEqual(GroupCFTest.OuterGroupTop, OuterGroup.Top);

            Assert.AreEqual(GroupCFTest.OuterEllipseLeft, Canvas.GetLeft(m_OuterEllipse));
            Assert.AreEqual(GroupCFTest.OuterEllipseTop, Canvas.GetTop(m_OuterEllipse));
        }

        [Test]
        public void ChangeOuterGroupWidth()
        {
            InitGroups();

            int widthFactor = 2;

            OuterGroup.Width *= widthFactor;

            Assert.AreEqual(GroupCFTest.InnerGroupLeft * widthFactor, InnerGroup.Left);
            Assert.AreEqual(GroupCFTest.InnerGroupWidth * widthFactor, InnerGroup.Width);

            Assert.AreEqual(GroupCFTest.InnerRectangleLeft * widthFactor, Canvas.GetLeft(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);

            Assert.AreEqual(GroupCFTest.OuterGroupLeft, OuterGroup.Left);
            Assert.AreEqual(GroupCFTest.OuterGroupWidth * widthFactor, OuterGroup.Width);

            Assert.AreEqual(GroupCFTest.OuterEllipseLeft * widthFactor, Canvas.GetLeft(m_OuterEllipse));
            Assert.AreEqual(GroupCFTest.OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void ChangeOuterGroupHeight()
        {
            InitGroups();

            int heightFactor = 2;

            OuterGroup.Height *= heightFactor;

            Assert.AreEqual(GroupCFTest.InnerGroupTop * heightFactor, InnerGroup.Top);
            Assert.AreEqual(GroupCFTest.InnerGroupHeight * heightFactor, InnerGroup.Height);

            Assert.AreEqual(GroupCFTest.InnerRectangleTop * heightFactor, Canvas.GetTop(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.InnerRectangleHeight * heightFactor, m_InnerRectangle.Height);

            Assert.AreEqual(GroupCFTest.OuterGroupTop, OuterGroup.Top);
            Assert.AreEqual(GroupCFTest.OuterGroupHeight * heightFactor, OuterGroup.Height);

            Assert.AreEqual(GroupCFTest.OuterEllipseTop * heightFactor, Canvas.GetTop(m_OuterEllipse));
            Assert.AreEqual(GroupCFTest.OuterEllipseHeight * heightFactor, m_OuterEllipse.Height);
        }

        [Test]
        public void ChangeOuterGroupLeft()
        {
            InitGroups();

            int leftDistance = 200;

            OuterGroup.Left += leftDistance;

            Assert.AreEqual(GroupCFTest.InnerGroupLeft, InnerGroup.Left);
            Assert.AreEqual(GroupCFTest.InnerRectangleLeft, Canvas.GetLeft(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.OuterGroupLeft + leftDistance, OuterGroup.Left);
            Assert.AreEqual(GroupCFTest.OuterEllipseLeft, Canvas.GetLeft(m_OuterEllipse));
        }

        [Test]
        public void ChangeOuterGroupTop()
        {
            InitGroups();

            int topDistance = 200;

            OuterGroup.Top += topDistance;

            Assert.AreEqual(GroupCFTest.InnerGroupTop, InnerGroup.Top);
            Assert.AreEqual(GroupCFTest.InnerRectangleTop, Canvas.GetTop(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.OuterGroupTop + topDistance, OuterGroup.Top);
            Assert.AreEqual(GroupCFTest.OuterEllipseTop, Canvas.GetTop(m_OuterEllipse));
        }

        [Test]
        public void ChangeOuterGroupLeftThenWidth()
        {
            InitGroups();

            int leftDistance = 200;
            int widthFactor = 2;

            OuterGroup.Left += leftDistance;
            OuterGroup.Width *= widthFactor;

            Assert.AreEqual(GroupCFTest.InnerGroupLeft * widthFactor, InnerGroup.Left);
            Assert.AreEqual(GroupCFTest.InnerRectangleLeft * widthFactor, Canvas.GetLeft(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.OuterGroupLeft + leftDistance, OuterGroup.Left);
            Assert.AreEqual(GroupCFTest.OuterEllipseLeft * widthFactor, Canvas.GetLeft(m_OuterEllipse));

            Assert.AreEqual(GroupCFTest.InnerGroupWidth * widthFactor, m_InnerGroup.Width);
            Assert.AreEqual(GroupCFTest.InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);
            Assert.AreEqual(GroupCFTest.OuterGroupWidth * widthFactor, m_OuterGroup.Width);
            Assert.AreEqual(GroupCFTest.OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void ChangeOuterGroupWidthThenLeft()
        {
            InitGroups();

            int leftDistance = 200;
            int widthFactor = 2;

            OuterGroup.Width *= widthFactor;
            OuterGroup.Left += leftDistance;

            Assert.AreEqual(GroupCFTest.InnerGroupLeft * widthFactor, InnerGroup.Left);
            Assert.AreEqual(GroupCFTest.InnerRectangleLeft * widthFactor, Canvas.GetLeft(m_InnerRectangle));
            Assert.AreEqual(GroupCFTest.OuterGroupLeft + leftDistance, OuterGroup.Left);
            Assert.AreEqual(GroupCFTest.OuterEllipseLeft * widthFactor, Canvas.GetLeft(m_OuterEllipse));

            Assert.AreEqual(GroupCFTest.InnerGroupWidth * widthFactor, m_InnerGroup.Width);
            Assert.AreEqual(GroupCFTest.InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);
            Assert.AreEqual(GroupCFTest.OuterGroupWidth * widthFactor, m_OuterGroup.Width);
            Assert.AreEqual(GroupCFTest.OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void MoveGroupWithLine()
        {
            int lineLeft = 10;
            int lineTop = 10;
            int lineRight = GroupCFTest.OuterGroupWidth - 10;
            int lineBottom = GroupCFTest.OuterGroupHeight - 10;

            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = lineLeft;
            line.Y1 = lineBottom;
            line.X2 = lineRight;
            line.Y2 = lineTop;
            m_OuterGroup.Items.Add(line);

            InitGroups();

            int leftDistance = 200;
            int topDistance = 100;

            OuterGroup.Left += leftDistance;
            OuterGroup.Top += topDistance;

            ILayoutObjectAdapter layoutObjectAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(line);
            Assert.AreEqual(lineLeft, layoutObjectAdapter.Left);
            Assert.AreEqual(lineTop, layoutObjectAdapter.Top);
        }

        [Test]
        public void ResizeGroupWithLine()
        {
            int lineLeft = 10;
            int lineTop = 10;
            int lineRight = GroupCFTest.OuterGroupWidth - 10;
            int lineBottom = GroupCFTest.OuterGroupHeight - 10;

            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = lineLeft;
            line.Y1 = lineBottom;
            line.X2 = lineRight;
            line.Y2 = lineTop;
            m_OuterGroup.Items.Add(line);

            InitGroups();

            int widthFactor = 2;
            int heightFactor = 3;

            OuterGroup.Width *= widthFactor;
            OuterGroup.Height *= heightFactor;

            ILayoutObjectAdapter layoutObjectAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(line);

            Assert.AreEqual(lineLeft * widthFactor, layoutObjectAdapter.Left);
            Assert.AreEqual((lineRight - lineLeft) * widthFactor, layoutObjectAdapter.Width);

            Assert.AreEqual(lineTop * heightFactor, layoutObjectAdapter.Top);
            Assert.AreEqual((lineBottom - lineTop) * heightFactor, layoutObjectAdapter.Height);
        }

        [Test]
        public void SetFillOnGroup()
        {
            InitGroups();

            m_OuterGroup.Background = Brushes.Yellow;

            BrushValidator.AssertBrushesAreEqual(Brushes.Yellow, m_InnerRectangle.Fill);
            BrushValidator.AssertBrushesAreEqual(Brushes.Yellow, m_OuterEllipse.Fill);
        }

        [Test]
        public void SetOutlineOnGroup()
        {
            InitGroups();

            m_OuterGroup.BorderBrush = Brushes.Yellow;

            BrushValidator.AssertBrushesAreEqual(Brushes.Yellow, m_InnerRectangle.Stroke);
            BrushValidator.AssertBrushesAreEqual(Brushes.Yellow, m_OuterEllipse.Stroke);
        }

        [Test]
        public void SetOutlineThicknessOnGroup()
        {
            InitGroups();

            m_OuterGroup.BorderThickness = new Thickness(2);

            Assert.AreEqual(2, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(2, m_OuterEllipse.StrokeThickness);
        }

        [Test]
        public void SetSameOutlineThicknessOnGroupAgain()
        {
            InitGroups();

            m_OuterGroup.BorderThickness = new Thickness(2);
            m_InnerRectangle.StrokeThickness = 10;
            m_OuterEllipse.StrokeThickness = 5;

            m_OuterGroup.BorderThickness = new Thickness(2);

            Assert.AreEqual(2, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(2, m_OuterEllipse.StrokeThickness);
        }

        [Test]
        public void SetDefaultFillOnItemsWhenGroupFillIsNull()
        {
            InitGroups();

            m_OuterGroup.Background = Brushes.Yellow;
            m_OuterGroup.Background = null;

            BrushValidator.AssertBrushesAreEqual(Brushes.Blue, m_InnerRectangle.Fill);
            BrushValidator.AssertBrushesAreEqual(Brushes.Red, m_OuterEllipse.Fill);
            Assert.IsNull(m_OuterGroup.Background);
            Assert.IsNull(m_InnerGroup.Background);
        }

        [Test]
        public void SetDefaultOutlineOnItemsWhenGroupOutlineIsNull()
        {
            InitGroups();

            m_OuterGroup.BorderBrush = Brushes.Yellow;
            m_OuterGroup.BorderBrush = null;

            BrushValidator.AssertBrushesAreEqual(Brushes.Red, m_InnerRectangle.Stroke);
            BrushValidator.AssertBrushesAreEqual(Brushes.Blue, m_OuterEllipse.Stroke);
            Assert.IsNull(m_OuterGroup.BorderBrush);
            Assert.IsNull(m_InnerGroup.BorderBrush);
        }

        [Test]
        public void SetDefaultOutlineThicknessOnItemsWhenGroupOutlineThicknessIsUndefined()
        {
            InitGroups();

            m_OuterGroup.BorderThickness = new Thickness(2);
            m_OuterGroup.BorderThickness = Group.UndefinedThickness;

            Assert.AreEqual(10, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(5, m_OuterEllipse.StrokeThickness);
            Assert.AreEqual(Group.UndefinedThickness, m_OuterGroup.BorderThickness);
            Assert.AreEqual(Group.UndefinedThickness, m_InnerGroup.BorderThickness);
        }

        [Test]
        public void GroupFillIsNullByDefault()
        {
            InitGroups();

            Assert.IsNull(m_OuterGroup.Background);
            Assert.IsNull(m_InnerGroup.Background);
        }

        [Test]
        public void GroupStrokeIsNullByDefault()
        {
            InitGroups();

            Assert.IsNull(m_OuterGroup.BorderBrush);
            Assert.IsNull(m_InnerGroup.BorderBrush);
        }

        [Test]
        public void GroupStrokeThicknessIsUndefinedByDefault()
        {
            InitGroups();

            Assert.AreEqual(Group.UndefinedThickness, m_OuterGroup.BorderThickness);
            Assert.AreEqual(Group.UndefinedThickness, m_InnerGroup.BorderThickness);
        }

        private void InitGroups()
        {
            m_Window.Show();
        }

        private IGroup InnerGroup
        {
            get { return (IGroup)m_InnerGroup; }
        }

        private IGroup OuterGroup
        {
            get { return (IGroup)m_OuterGroup; }
        }
    }
}
#endif
