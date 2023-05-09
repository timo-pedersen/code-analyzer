using System;
using System.ComponentModel;
using System.Drawing;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Graphics.Logic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class GroupCFTest
    {
        public const int InnerRectangleLeft = 10;
        public const int InnerRectangleTop = 10;
        public const int InnerRectangleWidth = 100;
        public const int InnerRectangleHeight = 50;

        public const int InnerGroupLeft = 50;
        public const int InnerGroupTop = 150;
        public const int InnerGroupWidth = 100;
        public const int InnerGroupHeight = 50;

        public const int OuterEllipseLeft = 0;
        public const int OuterEllipseTop = 0;
        public const int OuterEllipseWidth = 200;
        public const int OuterEllipseHeight = 100;

        public const int OuterGroupLeft = 20;
        public const int OuterGroupTop = 30;
        public const int OuterGroupWidth = 200;
        public const int OuterGroupHeight = 200;

        private RectangleCF m_InnerRectangle;
        private GroupCF m_InnerGroup;

        private EllipseCF m_OuterEllipse;
        private GroupCF m_OuterGroup;

        private IToolManager m_ToolManager;

        [SetUp]
        public void SetUp()
        {
            m_ToolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_ToolManager.Stub(x => x.Runtime).Return(true);

            ISecurityServiceCF securityServiceCF = TestHelper.CreateAndAddServiceStub<ISecurityServiceCF>();
            securityServiceCF.Stub(x => x.IsAccessGranted(null, null)).IgnoreArguments().Return(true);

            m_InnerRectangle = new RectangleCF();
            m_InnerRectangle.Fill = new BrushCF(Color.Blue);
            m_InnerRectangle.Stroke = new BrushCF(Color.Red);
            m_InnerRectangle.StrokeThickness = 10;
            m_InnerRectangle.Left = InnerRectangleLeft;
            m_InnerRectangle.Top = InnerRectangleTop;
            m_InnerRectangle.Width = InnerRectangleWidth;
            m_InnerRectangle.Height = InnerRectangleHeight;

            m_InnerGroup = new GroupCF();
            m_InnerGroup.Left = InnerGroupLeft;
            m_InnerGroup.Top = InnerGroupTop;
            m_InnerGroup.Width = InnerGroupWidth;
            m_InnerGroup.Height = InnerGroupHeight;
            m_InnerGroup.Components.Add(m_InnerRectangle);

            m_OuterEllipse = new EllipseCF();
            m_OuterEllipse.Fill = new BrushCF(Color.Red);
            m_OuterEllipse.Stroke = new BrushCF(Color.Blue);
            m_OuterEllipse.StrokeThickness = 5;
            m_OuterEllipse.Left = OuterEllipseLeft;
            m_OuterEllipse.Top = OuterEllipseTop;
            m_OuterEllipse.Width = OuterEllipseWidth;
            m_OuterEllipse.Height = OuterEllipseHeight;

            m_OuterGroup = new GroupCF();
            m_OuterGroup.Left = OuterGroupLeft;
            m_OuterGroup.Top = OuterGroupTop;
            m_OuterGroup.Width = OuterGroupWidth;
            m_OuterGroup.Height = OuterGroupHeight;
            m_OuterGroup.Components.Add(m_OuterEllipse);
            m_OuterGroup.Components.Add(m_InnerGroup);

            m_InnerGroup.ParentGroup = m_OuterGroup;
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void OriginalGroupBounds()
        {
            InitGroups();

            Assert.AreEqual(InnerGroupLeft, m_InnerGroup.GroupLogic.OriginalGroupBounds.Left);
            Assert.AreEqual(InnerGroupTop, m_InnerGroup.GroupLogic.OriginalGroupBounds.Top);
            Assert.AreEqual(InnerGroupWidth, m_InnerGroup.GroupLogic.OriginalGroupBounds.Width);
            Assert.AreEqual(InnerGroupHeight, m_InnerGroup.GroupLogic.OriginalGroupBounds.Height);

            Assert.AreEqual(OuterGroupLeft, m_OuterGroup.GroupLogic.OriginalGroupBounds.Left);
            Assert.AreEqual(OuterGroupTop, m_OuterGroup.GroupLogic.OriginalGroupBounds.Top);
            Assert.AreEqual(OuterGroupWidth, m_OuterGroup.GroupLogic.OriginalGroupBounds.Width);
            Assert.AreEqual(OuterGroupHeight, m_OuterGroup.GroupLogic.OriginalGroupBounds.Height);
        }

        [Test]
        public void OriginalComponentBounds()
        {
            InitGroups();

            Assert.AreEqual(InnerGroupLeft, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Left);
            Assert.AreEqual(InnerGroupTop, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Top);
            Assert.AreEqual(InnerGroupWidth, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Width);
            Assert.AreEqual(InnerGroupHeight, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_InnerGroup].Height);

            Assert.AreEqual(OuterEllipseLeft, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Left);
            Assert.AreEqual(OuterEllipseTop, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Top);
            Assert.AreEqual(OuterEllipseWidth, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Width);
            Assert.AreEqual(OuterEllipseHeight, m_OuterGroup.GroupLogic.OriginalObjectBounds[m_OuterEllipse].Height);

            Assert.AreEqual(InnerRectangleLeft, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Left);
            Assert.AreEqual(InnerRectangleTop, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Top);
            Assert.AreEqual(InnerRectangleWidth, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Width);
            Assert.AreEqual(InnerRectangleHeight, m_InnerGroup.GroupLogic.OriginalObjectBounds[m_InnerRectangle].Height);
        }

        [Test]
        public void Calling_EndInit_on_outer_group_before_inner()
        {
            ((ISupportInitialize)m_OuterGroup).EndInit();
            ((ISupportInitialize)m_InnerGroup).EndInit();

            AssertLocationsAfterEndInit();
        }

        [Test]
        public void Calling_EndInit_on_inner_group_before_outer()
        {
            ((ISupportInitialize)m_InnerGroup).EndInit();
            ((ISupportInitialize)m_OuterGroup).EndInit();

            AssertLocationsAfterEndInit();
        }

        private void AssertLocationsAfterEndInit()
        {
            Assert.AreEqual(InnerGroupLeft + OuterGroupLeft, m_InnerGroup.GroupLogic.AccumulatedGroupLocation.X);
            Assert.AreEqual(InnerGroupTop + OuterGroupTop, m_InnerGroup.GroupLogic.AccumulatedGroupLocation.Y);

            Assert.AreEqual(OuterGroupLeft, m_OuterGroup.GroupLogic.AccumulatedGroupLocation.X);
            Assert.AreEqual(OuterGroupTop, m_OuterGroup.GroupLogic.AccumulatedGroupLocation.Y);

            Assert.AreEqual(InnerGroupLeft + OuterGroupLeft, m_InnerGroup.Left);
            Assert.AreEqual(InnerGroupTop + OuterGroupTop, m_InnerGroup.Top);

            Assert.AreEqual(InnerRectangleLeft + InnerGroupLeft + OuterGroupLeft, m_InnerRectangle.Left);
            Assert.AreEqual(InnerRectangleTop + InnerGroupTop + OuterGroupTop, m_InnerRectangle.Top);

            Assert.AreEqual(OuterGroupLeft, m_OuterGroup.Left);
            Assert.AreEqual(OuterGroupTop, m_OuterGroup.Top);

            Assert.AreEqual(OuterEllipseLeft + OuterGroupLeft, m_OuterEllipse.Left);
            Assert.AreEqual(OuterEllipseTop + OuterGroupTop, m_OuterEllipse.Top);
        }

        [Test]
        public void ChangeOuterGroupWidth()
        {
            InitGroups();

            int widthFactor = 2;

            m_OuterGroup.Width *= widthFactor;

            Assert.AreEqual(InnerGroupLeft * widthFactor + OuterGroupLeft, m_InnerGroup.Left);
            Assert.AreEqual(InnerGroupWidth * widthFactor, m_InnerGroup.Width);

            Assert.AreEqual(InnerRectangleLeft * widthFactor + InnerGroupLeft * widthFactor + OuterGroupLeft, m_InnerRectangle.Left);
            Assert.AreEqual(InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);

            Assert.AreEqual(OuterGroupLeft, m_OuterGroup.Left);
            Assert.AreEqual(OuterGroupWidth * widthFactor, m_OuterGroup.Width);

            Assert.AreEqual(OuterEllipseLeft * widthFactor + OuterGroupLeft, m_OuterEllipse.Left);
            Assert.AreEqual(OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void ChangeOuterGroupHeight()
        {
            InitGroups();

            int heightFactor = 2;

            m_OuterGroup.Height *= heightFactor;

            Assert.AreEqual(InnerGroupTop * heightFactor + OuterGroupTop, m_InnerGroup.Top);
            Assert.AreEqual(InnerGroupHeight * heightFactor, m_InnerGroup.Height);

            Assert.AreEqual(InnerRectangleTop * heightFactor + InnerGroupTop * heightFactor + OuterGroupTop, m_InnerRectangle.Top);
            Assert.AreEqual(InnerRectangleHeight * heightFactor, m_InnerRectangle.Height);

            Assert.AreEqual(OuterGroupTop, m_OuterGroup.Top);
            Assert.AreEqual(OuterGroupHeight * heightFactor, m_OuterGroup.Height);

            Assert.AreEqual(OuterEllipseTop * heightFactor + OuterGroupTop, m_OuterEllipse.Top);
            Assert.AreEqual(OuterEllipseHeight * heightFactor, m_OuterEllipse.Height);
        }

        [Test]
        public void ChangeOuterGroupLeft()
        {
            InitGroups();

            int leftDistance = 200;

            m_OuterGroup.Left += leftDistance;

            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft + leftDistance, m_InnerGroup.Left);
            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft + InnerRectangleLeft + leftDistance, m_InnerRectangle.Left);
            Assert.AreEqual(OuterGroupLeft + leftDistance, m_OuterGroup.Left);
            Assert.AreEqual(OuterGroupLeft + OuterEllipseLeft + leftDistance, m_OuterEllipse.Left);
        }

        [Test]
        public void ChangeOuterGroupTop()
        {
            InitGroups();

            int topDistance = 200;

            m_OuterGroup.Top += topDistance;

            Assert.AreEqual(OuterGroupTop + InnerGroupTop + topDistance, m_InnerGroup.Top);
            Assert.AreEqual(OuterGroupTop + InnerGroupTop + InnerRectangleTop + topDistance, m_InnerRectangle.Top);
            Assert.AreEqual(OuterGroupTop + topDistance, m_OuterGroup.Top);
            Assert.AreEqual(OuterGroupTop + OuterEllipseTop + topDistance, m_OuterEllipse.Top);
        }

        [Test]
        public void ChangeOuterGroupLeftThenWidth()
        {
            InitGroups();

            int leftDistance = 200;
            int widthFactor = 2;

            m_OuterGroup.Left += leftDistance;
            m_OuterGroup.Width *= widthFactor;

            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft * widthFactor + leftDistance, m_InnerGroup.Left);
            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft * widthFactor + InnerRectangleLeft * widthFactor + leftDistance, m_InnerRectangle.Left);
            Assert.AreEqual(OuterGroupLeft + leftDistance, m_OuterGroup.Left);
            Assert.AreEqual(OuterGroupLeft + OuterEllipseLeft * widthFactor + leftDistance, m_OuterEllipse.Left);

            Assert.AreEqual(InnerGroupWidth * widthFactor, m_InnerGroup.Width);
            Assert.AreEqual(InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);
            Assert.AreEqual(OuterGroupWidth * widthFactor, m_OuterGroup.Width);
            Assert.AreEqual(OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void ChangeOuterGroupWidthThenLeft()
        {
            InitGroups();

            int leftDistance = 200;
            int widthFactor = 2;

            m_OuterGroup.Width *= widthFactor;
            m_OuterGroup.Left += leftDistance;

            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft * widthFactor + leftDistance, m_InnerGroup.Left);
            Assert.AreEqual(OuterGroupLeft + InnerGroupLeft * widthFactor + InnerRectangleLeft * widthFactor + leftDistance, m_InnerRectangle.Left);
            Assert.AreEqual(OuterGroupLeft + leftDistance, m_OuterGroup.Left);
            Assert.AreEqual(OuterGroupLeft + OuterEllipseLeft * widthFactor + leftDistance, m_OuterEllipse.Left);

            Assert.AreEqual(InnerGroupWidth * widthFactor, m_InnerGroup.Width);
            Assert.AreEqual(InnerRectangleWidth * widthFactor, m_InnerRectangle.Width);
            Assert.AreEqual(OuterGroupWidth * widthFactor, m_OuterGroup.Width);
            Assert.AreEqual(OuterEllipseWidth * widthFactor, m_OuterEllipse.Width);
        }

        [Test]
        public void MoveGroupWithLine()
        {
            int lineLeft = 10;
            int lineTop = 10;
            int lineRight = OuterGroupWidth - 10;
            int lineBottom = OuterGroupHeight - 10;

            Line line = new Line();
            line.X1 = lineLeft;
            line.Y1 = lineBottom;
            line.X2 = lineRight;
            line.Y2 = lineTop;
            m_OuterGroup.Components.Add(line);

            InitGroups();

            int leftDistance = 200;
            int topDistance = 100;

            m_OuterGroup.Left += leftDistance;
            m_OuterGroup.Top += topDistance;

            Assert.AreEqual(OuterGroupLeft + lineLeft + leftDistance, line.Left);
            Assert.AreEqual(OuterGroupTop + lineTop + topDistance, line.Top);
        }

        [Test]
        public void ResizeGroupWithLine()
        {
            int lineLeft = 10;
            int lineTop = 10;
            int lineRight = OuterGroupWidth - 10;
            int lineBottom = OuterGroupHeight - 10;

            Line line = new Line();
            line.X1 = lineLeft;
            line.Y1 = lineBottom;
            line.X2 = lineRight;
            line.Y2 = lineTop;
            m_OuterGroup.Components.Add(line);

            InitGroups();

            int widthFactor = 2;
            int heightFactor = 3;

            m_OuterGroup.Width *= widthFactor;
            m_OuterGroup.Height *= heightFactor;

            Assert.AreEqual(OuterGroupLeft + lineLeft * widthFactor, line.Left);
            Assert.AreEqual((lineRight - lineLeft) * widthFactor, line.Width);

            Assert.AreEqual(OuterGroupTop + lineTop * heightFactor, line.Top);
            Assert.AreEqual((lineBottom - lineTop) * heightFactor, line.Height);
        }

        [Test]
        public void SetFillOnGroup()
        {
            InitGroups();

            m_InnerRectangle.Fill = new BrushCF(Color.Blue);
            m_OuterEllipse.Fill = new BrushCF(Color.Red);

            BrushCF yellowBrush = new BrushCF(Color.Yellow);
            m_OuterGroup.Fill = yellowBrush;

            Assert.AreEqual(yellowBrush, m_InnerRectangle.Fill);
            Assert.AreEqual(yellowBrush, m_OuterEllipse.Fill);
        }

        [Test]
        public void SetOutlineOnGroup()
        {
            InitGroups();

            m_InnerRectangle.Stroke = new BrushCF(Color.Blue);
            m_OuterEllipse.Stroke = new BrushCF(Color.Red);

            BrushCF yellowBrush = new BrushCF(Color.Yellow);
            m_OuterGroup.Stroke = yellowBrush;

            Assert.AreEqual(yellowBrush.StartColor, m_InnerRectangle.Stroke.StartColor);
            Assert.AreEqual(yellowBrush.StartColor, m_OuterEllipse.Stroke.StartColor);
        }

        [Test]
        public void SetOutlineThicknessOnGroup()
        {
            InitGroups();

            m_InnerRectangle.StrokeThickness = 10;
            m_OuterEllipse.StrokeThickness = 5;

            m_OuterGroup.StrokeThickness = 2;

            Assert.AreEqual(2, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(2, m_OuterEllipse.StrokeThickness);
        }

        [Test]
        public void SetSameOutlineThicknessOnGroupAgain()
        {
            InitGroups();

            m_OuterGroup.StrokeThickness = 2;
            m_InnerRectangle.StrokeThickness = 10;
            m_OuterEllipse.StrokeThickness = 5;

            m_OuterGroup.StrokeThickness = 2;

            Assert.AreEqual(2, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(2, m_OuterEllipse.StrokeThickness);
        }

        [Test]
        public void SetDefaultFillOnItemsWhenGroupFillIsNull()
        {
            InitGroups();

            m_OuterGroup.Fill = new BrushCF(Color.Yellow);
            m_OuterGroup.Fill = null;

            Assert.AreEqual(Color.Blue, m_InnerRectangle.Fill.StartColor);
            Assert.AreEqual(Color.Red, m_OuterEllipse.Fill.StartColor);
            Assert.IsNull(m_OuterGroup.Fill);
            Assert.IsNull(m_InnerGroup.Fill);
        }

        [Test]
        public void SetDefaultOutlineOnItemsWhenGroupOutlineIsNull()
        {
            InitGroups();

            m_OuterGroup.Stroke = new BrushCF(Color.Yellow);
            m_OuterGroup.Stroke = null;

            Assert.AreEqual(Color.Red, m_InnerRectangle.Stroke.StartColor);
            Assert.AreEqual(Color.Blue, m_OuterEllipse.Stroke.StartColor);
            Assert.IsNull(m_OuterGroup.Stroke);
            Assert.IsNull(m_InnerGroup.Stroke);
        }

        [Test]
        public void SetDefaultOutlineThicknessOnItemsWhenGroupOutlineThicknessIsUndefined()
        {
            InitGroups();

            m_OuterGroup.StrokeThickness = 2;
            m_OuterGroup.StrokeThickness = GroupCF.UndefinedThickness;

            Assert.AreEqual(10, m_InnerRectangle.StrokeThickness);
            Assert.AreEqual(5, m_OuterEllipse.StrokeThickness);
            Assert.AreEqual(GroupCF.UndefinedThickness, m_OuterGroup.StrokeThickness);
            Assert.AreEqual(GroupCF.UndefinedThickness, m_InnerGroup.StrokeThickness);
        }

        [Test]
        public void GroupFillIsNullByDefault()
        {
            InitGroups();

            Assert.IsNull(m_OuterGroup.Fill);
            Assert.IsNull(m_InnerGroup.Fill);
        }

        [Test]
        public void GroupStrokeIsNullByDefault()
        {
            InitGroups();

            Assert.IsNull(m_OuterGroup.Stroke);
            Assert.IsNull(m_InnerGroup.Stroke);
        }

        [Test]
        public void GroupStrokeThicknessIsUndefinedByDefault()
        {
            InitGroups();

            Assert.AreEqual(GroupCF.UndefinedThickness, m_OuterGroup.StrokeThickness);
            Assert.AreEqual(GroupCF.UndefinedThickness, m_InnerGroup.StrokeThickness);
        }

        [Test]
        public void GroupObjectForwardMouseActionsToChildren()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0);
            MockRepository mockRepository = new MockRepository();
            BaseObject baseObject = mockRepository.StrictMock<BaseObject>();
            using (mockRepository.Record())
            {
                Expect.Call(baseObject.HitTestIgnoreEnabled(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(baseObject.HitTest(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(baseObject.Enabled).Repeat.Times(2).Return(true);
                baseObject.OnClick(mouseEventArgs);
                baseObject.OnMouseDown(mouseEventArgs);
                baseObject.OnMouseUp(mouseEventArgs);
            }

            using (mockRepository.Playback())
            {
                m_InnerGroup.Components.Add(baseObject);

                m_InnerGroup.OnClick(mouseEventArgs);
                m_InnerGroup.OnMouseDown(mouseEventArgs);
                m_InnerGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void ExpectNoMouseEventWhenClickOutsideChild()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0);

            MockRepository mockRepository = new MockRepository();
            BaseObject baseObject = mockRepository.StrictMock<BaseObject>();
            IEventSubscriber eventSubscriber = mockRepository.StrictMock<IEventSubscriber>();

            using (mockRepository.Record())
            {
                Expect.Call(baseObject.HitTestIgnoreEnabled(0, 0)).Repeat.Times(3).Return(false);
                Expect.Call(baseObject.HitTest(0, 0)).Repeat.Times(3).Return(false);

                m_InnerGroup.Click += eventSubscriber.Handler;
                m_InnerGroup.MouseDown += eventSubscriber.Handler;
                m_InnerGroup.MouseUp += eventSubscriber.Handler;
            }

            using (mockRepository.Playback())
            {
                m_InnerGroup.Components.Add(baseObject);

                m_InnerGroup.OnClick(mouseEventArgs);
                m_InnerGroup.OnMouseDown(mouseEventArgs);
                m_InnerGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void ExpectMouseEventFromGroupWhenClickInsideChild()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0);

            MockRepository mockRepository = new MockRepository();
            BaseObject baseObject = mockRepository.StrictMock<BaseObject>();
            IEventSubscriber eventSubscriber = mockRepository.StrictMock<IEventSubscriber>();

            using (mockRepository.Record())
            {
                Expect.Call(baseObject.HitTestIgnoreEnabled(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(baseObject.HitTest(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(baseObject.Enabled).Repeat.Times(2).Return(true);
                baseObject.OnClick(mouseEventArgs);
                baseObject.OnMouseDown(mouseEventArgs);
                baseObject.OnMouseUp(mouseEventArgs);

                m_InnerGroup.Click += eventSubscriber.Handler;
                m_InnerGroup.MouseDown += eventSubscriber.Handler;
                m_InnerGroup.MouseUp += eventSubscriber.Handler;

                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
            }

            using (mockRepository.Playback())
            {
                m_InnerGroup.Components.Add(baseObject);

                m_InnerGroup.OnClick(mouseEventArgs);
                m_InnerGroup.OnMouseDown(mouseEventArgs);
                m_InnerGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void ExpectMouseEventFromGroupButNoChildEventsWhenClickingInsideADisabledChild()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0);

            MockRepository mockRepository = new MockRepository();
            BaseObject baseObject = mockRepository.StrictMock<BaseObject>();
            IEventSubscriber eventSubscriber = mockRepository.StrictMock<IEventSubscriber>();

            using (mockRepository.Record())
            {
                Expect.Call(baseObject.HitTestIgnoreEnabled(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(baseObject.HitTest(0, 0)).Repeat.Times(3).Return(true);
                Expect.Call(baseObject.Enabled).Repeat.Times(3).Return(false);

                m_InnerGroup.Click += eventSubscriber.Handler;
                m_InnerGroup.MouseDown += eventSubscriber.Handler;
                m_InnerGroup.MouseUp += eventSubscriber.Handler;

                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
                eventSubscriber.Handler(m_InnerGroup, EventArgs.Empty);
            }

            using (mockRepository.Playback())
            {
                m_InnerGroup.Components.Add(baseObject);

                m_InnerGroup.OnClick(mouseEventArgs);
                m_InnerGroup.OnMouseDown(mouseEventArgs);
                m_InnerGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void ExpectMouseEventWhenClickingAGroupWithinAGroup()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, 0);

            MockRepository mockRepository = new MockRepository();
            GroupCF innerGroupMock = mockRepository.StrictMock<GroupCF>();

            using (mockRepository.Record())
            {
                Expect.Call(innerGroupMock.HitTestIgnoreEnabled(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(innerGroupMock.HitTest(0, 0)).Repeat.Times(2).Return(true);
                Expect.Call(innerGroupMock.Enabled).Repeat.Times(2).Return(true);

                innerGroupMock.OnClick(mouseEventArgs);
                innerGroupMock.OnMouseUp(mouseEventArgs);
                innerGroupMock.OnMouseDown(mouseEventArgs);
            }

            using (mockRepository.Playback())
            {
                m_OuterGroup.Components.Add(innerGroupMock);

                m_OuterGroup.OnClick(mouseEventArgs);
                m_OuterGroup.OnMouseDown(mouseEventArgs);
                m_OuterGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void ExpectMouseEventWhenClickingAChildWithinAGroupInAGroup()
        {
            InitGroups();

            System.Windows.Forms.MouseEventArgs mouseEventArgs = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, m_InnerGroup.Left, m_InnerGroup.Top, 0);

            MockRepository mockRepository = new MockRepository();
            BaseObject baseObject = mockRepository.StrictMock<BaseObject>();

            using (mockRepository.Record())
            {
                Expect.Call(baseObject.HitTestIgnoreEnabled(0, 0)).IgnoreArguments().Repeat.Any().Return(true);
                Expect.Call(baseObject.HitTest(0, 0)).IgnoreArguments().Repeat.Times(2).Return(true);
                Expect.Call(baseObject.Enabled).Repeat.Times(2).Return(true);

                baseObject.OnClick(mouseEventArgs);
                baseObject.OnMouseUp(mouseEventArgs);
                baseObject.OnMouseDown(mouseEventArgs);
            }

            using (mockRepository.Playback())
            {
                m_InnerGroup.Components.Add(baseObject);

                m_OuterGroup.OnClick(mouseEventArgs);
                m_OuterGroup.OnMouseDown(mouseEventArgs);
                m_OuterGroup.OnMouseUp(mouseEventArgs);
            }

            mockRepository.VerifyAll();
        }

        private void InitGroups()
        {
            ((ISupportInitialize)m_InnerGroup).EndInit();
            ((ISupportInitialize)m_OuterGroup).EndInit();
        }
    }
}
