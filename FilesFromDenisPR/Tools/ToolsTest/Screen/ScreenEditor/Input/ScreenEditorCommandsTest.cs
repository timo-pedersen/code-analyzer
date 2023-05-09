using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Tools;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor.HotSpot;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap;
using NSubstitute;
using NUnit.Framework;
using Size = System.Drawing.Size;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Input
{
    [TestFixture]
    public class ScreenEditorCommandsTest
    {
        private IScreenRootDesigner m_ScreenRootDesigner;
        private IScreenDesignerView m_ScreenDesignerView;
        private IUndoService m_IUndoService;

        private const double MasterLeft = 50;
        private const double MasterTop = 80;
        private const double MasterWidth = 320;
        private const double MasterHeight = 104;

        private ScreenEditorTestWindow m_ScreenEditor;
        private Rectangle m_MasterRectangle;
        private Rectangle m_SecondaryMasterRectangle;
        private Rectangle m_SlaveRectangle;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;
        private IScreenEditorCommands m_ScreenEditorCommands;
        private SnapManager m_SnapManager;
        private SnapGridStategy m_SnapGridStategy;
        private SnapLineStrategy m_SnapLineStrategy;
        private List<FrameworkElement> m_SelectionList;
        private List<FrameworkElement> m_Elements;
        private List<Rect> m_BoundingBoxes;
        private IHotSpotService m_HotSpotService;


        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            ISnapServiceGlobal snapServiceGlobal = new SnapManagerGlobal();
            TestHelper.AddService<ISnapServiceGlobal>(snapServiceGlobal);


            var targetService = Substitute.For<ITargetService>();
            var target = Substitute.For<ITarget>();
            var targetInfo = Substitute.For<ITargetInfo>();
            var terminalDescr = Substitute.For<ITerminalDescription, ITerminal>();
            targetInfo.TerminalDescription.Returns(terminalDescr);
            target.Id.Returns(TargetPlatform.Windows);
            targetService.CurrentTarget.Returns(target);
            targetService.CurrentTargetInfo.Returns(targetInfo);
            TestHelper.AddService<ITargetService>(targetService);

            IToolManager toolManager = Substitute.For<IToolManager>();
            toolManager.Runtime.Returns(false);
            TestHelper.AddService<IToolManager>(toolManager);

            m_SelectionList = new List<FrameworkElement>();

            IGlobalSelectionService globalSelectionService = TestHelper.AddServiceStub<IGlobalSelectionService>();
            globalSelectionService.GetSelectedComponents().Returns(m_SelectionList);

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();

            m_SnapManager = new SnapManager();
            ((ISnapService)m_SnapManager).GridSize = new Point(8, 8);

            m_MasterRectangle = new Rectangle();
            m_MasterRectangle.Stroke = Brushes.Black;
            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterRectangle);
            m_ScreenEditor.Canvas.Children.Add(m_MasterRectangle);

            m_Elements = new List<FrameworkElement>();
            m_Elements.Add(m_MasterRectangle);

            m_SelectionList.Clear();
            m_SelectionList.Add(m_MasterRectangle);

            m_BoundingBoxes = new List<Rect>();

            m_HotSpotService = Substitute.For<IHotSpotService>();

            INeoDesignerHost neoDesignerHost = Substitute.For<INeoDesignerHost>();
            neoDesignerHost.GetService<ISelectionService>().Returns(x => null);
            neoDesignerHost.GetService<IHotSpotService>().Returns(m_HotSpotService);

            m_ScreenRootDesigner = Substitute.For<IScreenRootDesigner>();
            neoDesignerHost.RootDesigner.Returns(m_ScreenRootDesigner);
            m_ScreenRootDesigner.DesignerHost.Returns(neoDesignerHost);
            m_ScreenRootDesigner.SelectedElements.Returns(m_SelectionList);
            m_ScreenRootDesigner.PrimarySelectedElement.Returns(m_MasterRectangle);
            m_ScreenRootDesigner.FindElementByName(Arg.Any<string>()).Returns(x => null);
            m_ScreenRootDesigner.Select(Arg.Do<FrameworkElement>(x => ReplaceSelection(x)));
            m_ScreenRootDesigner.Select(Arg.Do<IList<FrameworkElement>>(x => ReplaceSelection(x)));

            m_ScreenDesignerView = Substitute.For<IScreenDesignerView>();
            m_ScreenDesignerView.RootElements.Returns(m_Elements);
            m_ScreenDesignerView.GetBoundingBoxes().Returns(m_BoundingBoxes);
            m_ScreenDesignerView.Designer = m_ScreenRootDesigner;
            m_ScreenDesignerView.RootDesigner.Returns(m_ScreenRootDesigner);
            m_ScreenDesignerView.AddElement(Arg.Do<FrameworkElement>(x => m_ScreenEditor.Canvas.Children.Add(x)));
            m_ScreenDesignerView.RemoveElement(Arg.Do<FrameworkElement>(x => RemoveElement(x)));
            neoDesignerHost.GetService<IScreenDesignerView>().Returns(m_ScreenDesignerView);

            Canvas.SetLeft(m_MasterRectangle, MasterLeft);
            Canvas.SetTop(m_MasterRectangle, MasterTop);
            m_MasterRectangle.Width = MasterWidth;
            m_MasterRectangle.Height = MasterHeight;
            m_MasterRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterRectangle.RenderTransform = Transform.Identity;
            m_MasterRectangle.UpdateLayout();

            m_ScreenEditorCommands = new ScreenEditorCommands(m_ScreenDesignerView);

            ((ISnapService)m_SnapManager).SnapStyle = SnapStyle.None;
            neoDesignerHost.GetService<ISnapService>().Returns(m_SnapManager);

            // Setup undoservice
            IUndoManager undoManager = new UndoManager.UndoManager();
            m_IUndoService = undoManager.CreateUndoService(neoDesignerHost);
            neoDesignerHost.GetService<IUndoService>().Returns(m_IUndoService);
        }

        [TearDown]
        public void TearDown()
        {
            m_ScreenEditor.Canvas.Children.Clear();
            m_SelectionList.Clear();
            m_ScreenEditor.Close();
        }


        private void ReplaceSelection(FrameworkElement newSelection)
        {
            m_SelectionList.Clear();
            m_SelectionList.Add(newSelection);
        }

        private void ReplaceSelection(IList<FrameworkElement> newSelection)
        {
            m_SelectionList.Clear();
            m_SelectionList.AddRange(newSelection);
        }

        private void RemoveElement(FrameworkElement element)
        {
            if (m_ScreenEditor.Canvas.Children.Contains(element))
            {
                m_ScreenEditor.Canvas.Children.Remove(element);
            }
            else if (element.Parent is Group)
            {
                ((Group)element.Parent).Items.Remove(element);
            }
        }


        [Test]
        public void MoveNotPossibleWhenHotSpotIsInEditMode()
        {
            m_HotSpotService.IsInEditMode.Returns(true);
            bool result = Move(Direction.Right);

            Assert.IsFalse(result, "Move should return false");
        }

        [Test]
        public void MoveRightWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            List<FrameworkElement> rectangles = new List<FrameworkElement>();
            rectangles.Add(m_MasterRectangle);
            List<LayoutData> beforeMove = GetLayoutData(rectangles);

            bool result = Move(Direction.Right);

            m_MasterRectangle.UpdateLayout();

            Assert.IsTrue(result, "MoveRight should return true");
            AssertOffset(beforeMove, rectangles, Direction.Right);
        }

        [Test]
        public void MoveMultipleRightWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                List<FrameworkElement> rectangles = new List<FrameworkElement>();
                rectangles.Add(m_MasterRectangle);
                rectangles.Add(m_SecondaryMasterRectangle);
                List<LayoutData> beforeMove = GetLayoutData(rectangles);

                bool result = Move(Direction.Right);

                m_MasterRectangle.UpdateLayout();

                Assert.IsTrue(result, "MoveRight should return true");
                AssertOffset(beforeMove, rectangles, Direction.Right);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveLeftWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            List<FrameworkElement> rectangles = new List<FrameworkElement>();
            rectangles.Add(m_MasterRectangle);
            List<LayoutData> beforeMove = GetLayoutData(rectangles);

            bool result = Move(Direction.Left);

            m_MasterRectangle.UpdateLayout();

            Assert.IsTrue(result, "MoveLeft should return true");
            AssertOffset(beforeMove, rectangles, Direction.Left);
        }

        [Test]
        public void MoveMultipleLeftWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                List<FrameworkElement> rectangles = new List<FrameworkElement>();
                rectangles.Add(m_MasterRectangle);
                rectangles.Add(m_SecondaryMasterRectangle);
                List<LayoutData> beforeMove = GetLayoutData(rectangles);

                bool result = Move(Direction.Left);

                m_MasterRectangle.UpdateLayout();

                Assert.IsTrue(result, "MoveLeft should return true");
                AssertOffset(beforeMove, rectangles, Direction.Left);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveUpWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            List<FrameworkElement> rectangles = new List<FrameworkElement>();
            rectangles.Add(m_MasterRectangle);
            List<LayoutData> beforeMove = GetLayoutData(rectangles);

            bool result = Move(Direction.Up);

            m_MasterRectangle.UpdateLayout();

            Assert.IsTrue(result, "MoveUp should return true");
            AssertOffset(beforeMove, rectangles, Direction.Up);
        }

        [Test]
        public void MoveMultipleUpWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                List<FrameworkElement> rectangles = new List<FrameworkElement>();
                rectangles.Add(m_MasterRectangle);
                rectangles.Add(m_SecondaryMasterRectangle);
                List<LayoutData> beforeMove = GetLayoutData(rectangles);

                bool result = Move(Direction.Up);

                m_MasterRectangle.UpdateLayout();

                Assert.IsTrue(result, "MoveUp should return true");
                AssertOffset(beforeMove, rectangles, Direction.Up);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveDownWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            List<FrameworkElement> rectangles = new List<FrameworkElement>();
            rectangles.Add(m_MasterRectangle);
            List<LayoutData> beforeMove = GetLayoutData(rectangles);

            bool result = Move(Direction.Down);

            m_MasterRectangle.UpdateLayout();

            Assert.IsTrue(result, "MoveDown should return true");
            AssertOffset(beforeMove, rectangles, Direction.Down);
        }

        [Test]
        public void MoveMultipleDownWithoutSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                List<FrameworkElement> rectangles = new List<FrameworkElement>();
                rectangles.Add(m_MasterRectangle);
                rectangles.Add(m_SecondaryMasterRectangle);
                List<LayoutData> beforeMove = GetLayoutData(rectangles);

                bool result = Move(Direction.Down);

                m_MasterRectangle.UpdateLayout();

                Assert.IsTrue(result, "MoveDown should return true");
                AssertOffset(beforeMove, rectangles, Direction.Down);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveWithoutSnap()
        {
            Point startPoint = GetTopLeftCorner(m_MasterRectangle);

            bool result = m_ScreenEditorCommands.InitialMousePosition(startPoint);
            Assert.IsTrue(result);

            result = m_ScreenEditorCommands.Move(new Point(60, 90), true);
            Point endPoint = GetTopLeftCorner(m_MasterRectangle);
            Assert.AreEqual(-10, startPoint.Y - endPoint.Y);
            Assert.AreEqual(-10, startPoint.X - endPoint.X);
            Assert.IsTrue(result);

            result = m_ScreenEditorCommands.FinalizeMove();
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveRightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            Point startPoint = GetTopLeftCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.MoveRight();
            Point endPoint = GetTopLeftCorner(m_MasterRectangle);

            Point expectedPoint = new Point(startPoint.X + ((ISnapService)m_SnapManager).GridSize.X, startPoint.Y);

            Assert.AreEqual(expectedPoint, endPoint);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveMultipleRightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.MoveRight();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startMasterPoint.X + ((ISnapService)m_SnapManager).GridSize.X, startMasterPoint.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X + ((ISnapService)m_SnapManager).GridSize.X, startSecondPoint.Y);

                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveLeftWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            Point startPoint = GetTopLeftCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.MoveLeft();
            Point endPoint = GetTopLeftCorner(m_MasterRectangle);

            Point expectedPoint = new Point(startPoint.X - ((ISnapService)m_SnapManager).GridSize.X, startPoint.Y);

            Assert.AreEqual(expectedPoint, endPoint);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveMultipleLeftWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.MoveLeft();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startMasterPoint.X - ((ISnapService)m_SnapManager).GridSize.X, startMasterPoint.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X - ((ISnapService)m_SnapManager).GridSize.X, startSecondPoint.Y);

                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveUpWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            Point startPoint = GetTopLeftCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.MoveUp();
            Point endPoint = GetTopLeftCorner(m_MasterRectangle);

            Point expectedPoint = new Point(startPoint.X, startPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y);

            Assert.AreEqual(expectedPoint, endPoint);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveMultipleUpWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.MoveUp();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startMasterPoint.X, startMasterPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X, startSecondPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y);

                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveDownWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            Point startPoint = GetTopLeftCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.MoveDown();
            Point endPoint = GetTopLeftCorner(m_MasterRectangle);

            Point expectedPoint = new Point(startPoint.X, startPoint.Y + ((ISnapService)m_SnapManager).GridSize.Y);

            Assert.AreEqual(expectedPoint, endPoint);
            Assert.IsTrue(result);
        }

        [Test]
        public void MoveMultipleDownWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.MoveDown();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startMasterPoint.X, startMasterPoint.Y + ((ISnapService)m_SnapManager).GridSize.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X, startSecondPoint.Y + ((ISnapService)m_SnapManager).GridSize.Y);

                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveWithSnapToGrid()
        {
            SetSnapToGrid();

            Point startPoint = GetTopLeftCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.InitialMousePosition(startPoint);
            Assert.IsTrue(result);

            Point newPosition = new Point(startPoint.X, startPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y);
            result = m_ScreenEditorCommands.Move(newPosition, true);
            Assert.IsTrue(result);

            Point endPoint = GetTopLeftCorner(m_MasterRectangle);
            Point expectedPoint = new Point(startPoint.X, startPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y);
            Assert.AreEqual(expectedPoint, endPoint);

            result = m_ScreenEditorCommands.FinalizeMove();
            Assert.IsTrue(result);

            // Move again. This time rectangle should be snapped to the same gridline.
            startPoint = GetTopLeftCorner(m_MasterRectangle);
            result = m_ScreenEditorCommands.InitialMousePosition(startPoint);
            Assert.IsTrue(result);

            newPosition = new Point(startPoint.X, startPoint.Y - ((ISnapService)m_SnapManager).GridSize.Y + 4);
            result = m_ScreenEditorCommands.Move(newPosition, true);
            Assert.IsFalse(result);

            endPoint = GetTopLeftCorner(m_MasterRectangle);
            expectedPoint = startPoint;
            Assert.AreEqual(expectedPoint, endPoint);

            result = m_ScreenEditorCommands.FinalizeMove();
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseWidthWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.IncreaseWidth();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Assert.AreEqual(topLeftStart, topLeftEnd);

            Assert.AreEqual(-1, bottomRightStart.X - bottomRightEnd.X);
            Assert.AreEqual(0, bottomRightStart.Y - bottomRightEnd.Y);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseMultipleWidthWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseWidth();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Assert.AreEqual(-1, bottomRightMasterStart.X - bottomRightMasterEnd.X, "MasterRect");
                Assert.AreEqual(0, bottomRightMasterStart.Y - bottomRightMasterEnd.Y, "MasterRect");
                Assert.AreEqual(-1, bottomRightSecondStart.X - bottomRightSecondEnd.X, "SecondRect");
                Assert.AreEqual(0, bottomRightSecondStart.Y - bottomRightSecondEnd.Y, "SecondRect");
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseWidthWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.DecreaseWidth();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Assert.AreEqual(topLeftStart, topLeftEnd);

            Assert.AreEqual(1, bottomRightStart.X - bottomRightEnd.X);
            Assert.AreEqual(0, bottomRightStart.Y - bottomRightEnd.Y);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseMultipleWidthWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseWidth();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Assert.AreEqual(1, bottomRightMasterStart.X - bottomRightMasterEnd.X, "MasterRect");
                Assert.AreEqual(0, bottomRightMasterStart.Y - bottomRightMasterEnd.Y, "MasterRect");
                Assert.AreEqual(1, bottomRightSecondStart.X - bottomRightSecondEnd.X, "SecondRect");
                Assert.AreEqual(0, bottomRightSecondStart.Y - bottomRightSecondEnd.Y, "SecondRect");
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseHeightWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.IncreaseHeight();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Assert.AreEqual(topLeftStart, topLeftEnd);

            Assert.AreEqual(0, bottomRightStart.X - bottomRightEnd.X);
            Assert.AreEqual(-1, bottomRightStart.Y - bottomRightEnd.Y);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseMultipleHeightWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseHeight();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Assert.AreEqual(0, bottomRightMasterStart.X - bottomRightMasterEnd.X, "MasterRect");
                Assert.AreEqual(-1, bottomRightMasterStart.Y - bottomRightMasterEnd.Y, "MasterRect");
                Assert.AreEqual(0, bottomRightSecondStart.X - bottomRightSecondEnd.X, "SecondRect");
                Assert.AreEqual(-1, bottomRightSecondStart.Y - bottomRightSecondEnd.Y, "SecondRect");
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseHeightWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.DecreaseHeight();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Assert.AreEqual(topLeftStart, topLeftEnd);

            Assert.AreEqual(0, bottomRightStart.X - bottomRightEnd.X);
            Assert.AreEqual(1, bottomRightStart.Y - bottomRightEnd.Y);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseMultipleHeightWithoutSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.None);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseHeight();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Assert.AreEqual(0, bottomRightMasterStart.X - bottomRightMasterEnd.X, "MasterRect");
                Assert.AreEqual(1, bottomRightMasterStart.Y - bottomRightMasterEnd.Y, "MasterRect");
                Assert.AreEqual(0, bottomRightSecondStart.X - bottomRightSecondEnd.X, "SecondRect");
                Assert.AreEqual(1, bottomRightSecondStart.Y - bottomRightSecondEnd.Y, "SecondRect");
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseWidthWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.IncreaseWidth();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Point expectedEndPoint = new Point(bottomRightStart.X + ((ISnapService)m_SnapManager).GridSize.X, bottomRightStart.Y);

            Assert.AreEqual(topLeftStart, topLeftEnd);
            Assert.AreEqual(expectedEndPoint, bottomRightEnd);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseMultipleWidthWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseWidth();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Point expectedMasterEndPoint = new Point(bottomRightMasterStart.X + ((ISnapService)m_SnapManager).GridSize.X, bottomRightMasterStart.Y);
                Point expectedSecondEndPoint = new Point(bottomRightSecondStart.X + ((ISnapService)m_SnapManager).GridSize.X, bottomRightSecondStart.Y);

                Assert.AreEqual(expectedMasterEndPoint, bottomRightMasterEnd);
                Assert.AreEqual(expectedSecondEndPoint, bottomRightSecondEnd);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseWidthWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.DecreaseWidth();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Point expectedEndPoint = new Point(bottomRightStart.X - ((ISnapService)m_SnapManager).GridSize.X, bottomRightStart.Y);

            Assert.AreEqual(topLeftStart, topLeftEnd);
            Assert.AreEqual(expectedEndPoint, bottomRightEnd);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseMultipleWidthWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseWidth();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Point expectedMasterEndPoint = new Point(bottomRightMasterStart.X - ((ISnapService)m_SnapManager).GridSize.X, bottomRightMasterStart.Y);
                Point expectedSecondEndPoint = new Point(bottomRightSecondStart.X - ((ISnapService)m_SnapManager).GridSize.X, bottomRightSecondStart.Y);

                Assert.AreEqual(expectedMasterEndPoint, bottomRightMasterEnd);
                Assert.AreEqual(expectedSecondEndPoint, bottomRightSecondEnd);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseHeightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.IncreaseHeight();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Point expectedEndPoint = new Point(bottomRightStart.X, bottomRightStart.Y + ((ISnapService)m_SnapManager).GridSize.Y);

            Assert.AreEqual(topLeftStart, topLeftEnd);
            Assert.AreEqual(expectedEndPoint, bottomRightEnd);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncreaseMultipleHeightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseHeight();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Point expectedMasterEndPoint = new Point(bottomRightMasterStart.X, bottomRightMasterStart.Y + ((ISnapService)m_SnapManager).GridSize.Y);
                Point expectedSecondEndPoint = new Point(bottomRightSecondStart.X, bottomRightSecondStart.Y + ((ISnapService)m_SnapManager).GridSize.Y);

                Assert.AreEqual(expectedMasterEndPoint, bottomRightMasterEnd);
                Assert.AreEqual(expectedSecondEndPoint, bottomRightSecondEnd);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseHeightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();

            Point topLeftStart = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightStart = GetBottomRightCorner(m_MasterRectangle);
            bool result = m_ScreenEditorCommands.DecreaseHeight();
            Point topLeftEnd = GetTopLeftCorner(m_MasterRectangle);
            Point bottomRightEnd = GetBottomRightCorner(m_MasterRectangle);

            Point expectedEndPoint = new Point(bottomRightStart.X, bottomRightStart.Y - ((ISnapService)m_SnapManager).GridSize.Y);

            Assert.AreEqual(topLeftStart, topLeftEnd);
            Assert.AreEqual(expectedEndPoint, bottomRightEnd);
            Assert.IsTrue(result);
        }

        [Test]
        public void DecreaseMultipleHeightWithSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Grid);
            try
            {
                Point topLeftMasterStart = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterStart = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondStart = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondStart = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseHeight();
                Point topLeftMasterEnd = GetTopLeftCorner(m_MasterRectangle);
                Point bottomRightMasterEnd = GetBottomRightCorner(m_MasterRectangle);
                Point topLeftSecondEnd = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point bottomRightSecondEnd = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Assert.AreEqual(topLeftMasterStart, topLeftMasterEnd);
                Assert.AreEqual(topLeftSecondStart, topLeftSecondEnd);

                Point expectedMasterEndPoint = new Point(bottomRightMasterStart.X, bottomRightMasterStart.Y - ((ISnapService)m_SnapManager).GridSize.Y);
                Point expectedSecondEndPoint = new Point(bottomRightSecondStart.X, bottomRightSecondStart.Y - ((ISnapService)m_SnapManager).GridSize.Y);

                Assert.AreEqual(expectedMasterEndPoint, bottomRightMasterEnd);
                Assert.AreEqual(expectedSecondEndPoint, bottomRightSecondEnd);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveWithoutObjectsSelectedNoSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            m_SelectionList.Clear();

            bool result = m_ScreenEditorCommands.MoveRight();
            Assert.IsFalse(result, "MoveRight without selected objects should return false");
            result = m_ScreenEditorCommands.MoveLeft();
            Assert.IsFalse(result, "MoveLeft without selected objects should return false");
            result = m_ScreenEditorCommands.MoveUp();
            Assert.IsFalse(result, "MoveUp without selected objects should return false");
            result = m_ScreenEditorCommands.MoveDown();
            Assert.IsFalse(result, "MoveDown without selected objects should return false");
        }

        [Test]
        public void MoveWithoutObjectsSelectedSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            SetSnapToGrid();
            m_SelectionList.Clear();

            bool result = m_ScreenEditorCommands.MoveRight();
            Assert.IsFalse(result, "MoveRight without selected objects should return false");
            result = m_ScreenEditorCommands.MoveLeft();
            Assert.IsFalse(result, "MoveLeft without selected objects should return false");
            result = m_ScreenEditorCommands.MoveUp();
            Assert.IsFalse(result, "MoveUp without selected objects should return false");
            result = m_ScreenEditorCommands.MoveDown();
            Assert.IsFalse(result, "MoveDown without selected objects should return false");
        }

        [Test]
        public void ResizeWithoutObjectsSelectedNoSnap()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            m_SelectionList.Clear();

            bool result = m_ScreenEditorCommands.IncreaseWidth();
            Assert.IsFalse(result, "IncreaseWidth without selected objects should return false");
            result = m_ScreenEditorCommands.DecreaseWidth();
            Assert.IsFalse(result, "DecreaseWidth without selected objects should return false");
            result = m_ScreenEditorCommands.IncreaseHeight();
            Assert.IsFalse(result, "IncreaseHeight without selected objects should return false");
            result = m_ScreenEditorCommands.DecreaseHeight();
            Assert.IsFalse(result, "DecreaseHeight without selected objects should return false");
        }

        [Test]
        public void ResizeWithoutObjectsSelectedSnapToGrid()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            SetSnapToGrid();
            m_SelectionList.Clear();

            bool result = m_ScreenEditorCommands.IncreaseWidth();
            Assert.IsFalse(result, "IncreaseWidth without selected objects should return false");
            result = m_ScreenEditorCommands.DecreaseWidth();
            Assert.IsFalse(result, "DecreaseWidth without selected objects should return false");
            result = m_ScreenEditorCommands.IncreaseHeight();
            Assert.IsFalse(result, "IncreaseHeight without selected objects should return false");
            result = m_ScreenEditorCommands.DecreaseHeight();
            Assert.IsFalse(result, "DecreaseHeight without selected objects should return false");
        }

        [Test]
        public void MoveRightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSlavePoint = GetTopLeftCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveRight();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);

                Point expectedPoint = new Point(startSlavePoint.X, startMasterPoint.Y);

                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);


                // Move again
                result = m_ScreenEditorCommands.MoveRight();
                endMasterPoint = GetTopLeftCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedPoint = new Point(startSlavePoint.X + snapLine.SnappingDistance + 1, startMasterPoint.Y);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);


            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveMultipleRightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.Line);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startSlavePoint = GetTopLeftCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveRight();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startSlavePoint.X, startMasterPoint.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X + (endMasterPoint.X - startMasterPoint.X), startSecondPoint.Y + (endMasterPoint.Y - startMasterPoint.Y));
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);

                Assert.IsTrue(result);


                // Move again
                startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.MoveRight();
                endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterPoint = new Point(startSlavePoint.X + snapLine.SnappingDistance + 1, startMasterPoint.Y);
                expectedSecondPoint = new Point(startSecondPoint.X + snapLine.SnappingDistance + 1, startSecondPoint.Y);
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveLeftWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveLeft();
                Point endMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedPoint = new Point(startSlavePoint.X, startMasterPoint.Y);

                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);


                // Move again
                result = m_ScreenEditorCommands.MoveLeft();
                endMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedPoint = new Point(startSlavePoint.X - snapLine.SnappingDistance - 1, startMasterPoint.Y);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);


            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveMultipleLeftWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.Line);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                Point startSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveLeft();
                Point endMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point endSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startSlavePoint.X, startMasterPoint.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X + (endMasterPoint.X - startMasterPoint.X), startSecondPoint.Y + (endMasterPoint.Y - startMasterPoint.Y));
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);

                Assert.IsTrue(result);


                // Move again
                startSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.MoveLeft();
                endMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                endSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterPoint = new Point(startSlavePoint.X - snapLine.SnappingDistance - 1, startMasterPoint.Y);
                expectedSecondPoint = new Point(startSecondPoint.X - snapLine.SnappingDistance - 1, startSecondPoint.Y);
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveDownWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSlavePoint = GetTopLeftCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveDown();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);

                Point expectedPoint = new Point(startMasterPoint.X, startSlavePoint.Y);

                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);

                // Move again
                result = m_ScreenEditorCommands.MoveDown();
                endMasterPoint = GetTopLeftCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedPoint = new Point(startMasterPoint.X, startSlavePoint.Y + snapLine.SnappingDistance + 1);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveMultipleDownWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondaryMasterAndSetSnap(SnapStyle.Line);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startSlavePoint = GetTopLeftCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveDown();
                Point endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                Point expectedMasterPoint = new Point(startMasterPoint.X, startSlavePoint.Y);
                Point expectedSecondPoint = new Point(startSecondPoint.X + (endMasterPoint.X - startMasterPoint.X), startSecondPoint.Y + (endMasterPoint.Y - startMasterPoint.Y));
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);

                Assert.IsTrue(result);


                // Move again
                startSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.MoveDown();
                endMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterPoint = new Point(startMasterPoint.X, startSlavePoint.Y + snapLine.SnappingDistance + 1);
                expectedSecondPoint = new Point(startSecondPoint.X, startSecondPoint.Y + snapLine.SnappingDistance + 1);
                Assert.AreEqual(expectedMasterPoint, endMasterPoint);
                Assert.AreEqual(expectedSecondPoint, endSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveUpWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);

            try
            {
                Point startMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.MoveUp();
                Point endMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedPoint = new Point(startMasterPoint.X, startSlavePoint.Y);

                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);

                // Move again
                result = m_ScreenEditorCommands.MoveUp();
                endMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedPoint = new Point(startMasterPoint.X, startSlavePoint.Y - snapLine.SnappingDistance - 1);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void MoveWithSnapToLine()
        {
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startBottomRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startTopLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startBottomRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                bool result = m_ScreenEditorCommands.InitialMousePosition(startTopLeftMasterPoint);
                Assert.IsTrue(result);

                SnapLine snapLine = new SnapLine();
                Point newPosition = new Point(startTopLeftMasterPoint.X, startTopLeftMasterPoint.Y - (startBottomRightMasterPoint.Y - startBottomRightSlavePoint.Y) - snapLine.SnappingDistance);
                result = m_ScreenEditorCommands.Move(newPosition, true);
                Assert.IsTrue(result);

                Point endMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point expectedPoint = new Point(startBottomRightMasterPoint.X, startBottomRightSlavePoint.Y);

                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);

                result = m_ScreenEditorCommands.FinalizeMove();
                Assert.IsTrue(result);

                // Move again. Master Rectangle should still be snapped.
                startTopLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                result = m_ScreenEditorCommands.InitialMousePosition(startTopLeftMasterPoint);
                Assert.IsTrue(result);

                newPosition = new Point(startTopLeftMasterPoint.X, startTopLeftMasterPoint.Y - 1);
                result = m_ScreenEditorCommands.Move(newPosition, true);
                Assert.IsTrue(result);

                endMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                expectedPoint = new Point(startBottomRightMasterPoint.X, startBottomRightSlavePoint.Y);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);

                result = m_ScreenEditorCommands.FinalizeMove();
                Assert.IsTrue(result);

                // Move again. Master Rectangle should be unsnapped.
                startTopLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                result = m_ScreenEditorCommands.InitialMousePosition(startTopLeftMasterPoint);
                Assert.IsTrue(result);

                newPosition = new Point(startTopLeftMasterPoint.X, startTopLeftMasterPoint.Y - snapLine.SnappingDistance - 1);
                result = m_ScreenEditorCommands.Move(newPosition, true);
                Assert.IsTrue(result);

                endMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                expectedPoint = new Point(startBottomRightMasterPoint.X, startBottomRightSlavePoint.Y - snapLine.SnappingDistance - 1);
                Assert.AreEqual(expectedPoint, endMasterPoint);
                Assert.IsTrue(result);

                result = m_ScreenEditorCommands.FinalizeMove();
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseWidthWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseWidth();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedRightPoint = new Point(startRightSlavePoint.X, startRightMasterPoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);


                // Move again
                result = m_ScreenEditorCommands.IncreaseWidth();
                m_MasterRectangle.UpdateLayout();

                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedRightPoint = new Point(startRightSlavePoint.X + snapLine.SnappingDistance + 1, startRightMasterPoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseMultipleWidthWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseWidth();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Point expectedMasterLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedMasterRightPoint = new Point(startRightSlavePoint.X, startRightMasterPoint.Y);
                Point expectedSecondLeftPoint = new Point(startLeftSecondPoint.X, startLeftSecondPoint.Y);
                Point expectedSecondRightPoint = new Point(startRightSecondPoint.X + (endRightMasterPoint.X - startRightMasterPoint.X), startRightSecondPoint.Y);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);

                // Move again
                startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.IncreaseWidth();
                m_MasterRectangle.UpdateLayout();

                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterRightPoint = new Point(startRightSlavePoint.X + snapLine.SnappingDistance + 1, startRightMasterPoint.Y);
                expectedSecondRightPoint = new Point(startRightSecondPoint.X + snapLine.SnappingDistance + 1, startRightSecondPoint.Y);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseWidthWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseWidth();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedRightPoint = new Point(startRightSlavePoint.X, startRightMasterPoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);


                // Move again
                result = m_ScreenEditorCommands.DecreaseWidth();
                m_MasterRectangle.UpdateLayout();

                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedRightPoint = new Point(startRightSlavePoint.X - snapLine.SnappingDistance - 1, startRightMasterPoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseMultipleWidthWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.DecreaseWidth();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Point expectedMasterLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedMasterRightPoint = new Point(startRightSlavePoint.X, startRightMasterPoint.Y);
                Point expectedSecondLeftPoint = new Point(startLeftSecondPoint.X, startLeftSecondPoint.Y);
                Point expectedSecondRightPoint = new Point(startRightSecondPoint.X + (endRightMasterPoint.X - startRightMasterPoint.X), startRightSecondPoint.Y);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);

                // Move again
                startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.DecreaseWidth();
                m_MasterRectangle.UpdateLayout();

                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterRightPoint = new Point(startRightSlavePoint.X - snapLine.SnappingDistance - 1, startRightMasterPoint.Y);
                expectedSecondRightPoint = new Point(startRightSecondPoint.X - snapLine.SnappingDistance - 1, startRightSecondPoint.Y);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseHeightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseHeight();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);

                // Move again
                result = m_ScreenEditorCommands.IncreaseHeight();
                m_MasterRectangle.UpdateLayout();

                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y + snapLine.SnappingDistance + 1);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void IncreaseMultipleHeightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondLargerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                bool result = m_ScreenEditorCommands.IncreaseHeight();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Point expectedMasterLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedMasterRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y);
                Point expectedSecondLeftPoint = new Point(startLeftSecondPoint.X, startLeftSecondPoint.Y);
                Point expectedSecondRightPoint = new Point(startRightSecondPoint.X, startRightSecondPoint.Y + (endRightMasterPoint.Y - startRightMasterPoint.Y));

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);

                // Move again
                startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.IncreaseHeight();
                m_MasterRectangle.UpdateLayout();

                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y + snapLine.SnappingDistance + 1);
                expectedSecondRightPoint = new Point(startRightSecondPoint.X, startRightSecondPoint.Y + snapLine.SnappingDistance + 1);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseHeightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                bool result = m_ScreenEditorCommands.DecreaseHeight();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                Point expectedLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);

                // Move again
                result = m_ScreenEditorCommands.DecreaseHeight();
                m_MasterRectangle.UpdateLayout();

                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y - snapLine.SnappingDistance - 1);

                Assert.AreEqual(expectedLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedRightPoint, endRightMasterPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void DecreaseMultipleHeightWithSnapToLine()
        {
            m_HotSpotService.IsInEditMode.Returns(false);
            CreateSecondaryMasterAndSetSnap(SnapStyle.Grid);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.Line);
            try
            {
                Point startLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point startLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point startRightSlavePoint = GetBottomRightCorner(m_SlaveRectangle);
                Point startRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                bool result = m_ScreenEditorCommands.DecreaseHeight();
                m_MasterRectangle.UpdateLayout();

                Point endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                Point endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                Point endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);
                Point endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);

                Point expectedMasterLeftPoint = new Point(startLeftMasterPoint.X, startLeftMasterPoint.Y);
                Point expectedMasterRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y);
                Point expectedSecondLeftPoint = new Point(startLeftSecondPoint.X, startLeftSecondPoint.Y);
                Point expectedSecondRightPoint = new Point(startRightSecondPoint.X, startRightSecondPoint.Y + (endRightMasterPoint.Y - startRightMasterPoint.Y));

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);

                // Move again
                startRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                result = m_ScreenEditorCommands.DecreaseHeight();
                m_MasterRectangle.UpdateLayout();

                endRightMasterPoint = GetBottomRightCorner(m_MasterRectangle);
                endRightSecondPoint = GetBottomRightCorner(m_SecondaryMasterRectangle);
                endLeftMasterPoint = GetTopLeftCorner(m_MasterRectangle);
                endLeftSecondPoint = GetTopLeftCorner(m_SecondaryMasterRectangle);

                SnapLine snapLine = new SnapLine();
                expectedMasterRightPoint = new Point(startRightMasterPoint.X, startRightSlavePoint.Y - snapLine.SnappingDistance - 1);
                expectedSecondRightPoint = new Point(startRightSecondPoint.X, startRightSecondPoint.Y - snapLine.SnappingDistance - 1);

                Assert.AreEqual(expectedMasterLeftPoint, endLeftMasterPoint);
                Assert.AreEqual(expectedMasterRightPoint, endRightMasterPoint);
                Assert.AreEqual(expectedSecondLeftPoint, endLeftSecondPoint);
                Assert.AreEqual(expectedSecondRightPoint, endRightSecondPoint);
                Assert.IsTrue(result);
            }
            finally
            {
                RemoveSlaveRect();
            }
        }

        [Test]
        public void SelectNextObjectWithTabKey()
        {
            m_SecondaryMasterRectangle = new Rectangle();
            m_Elements.Add(m_SecondaryMasterRectangle);

            m_ScreenRootDesigner.Select(m_SecondaryMasterRectangle, SelectionTypes.Replace);

            bool result = m_ScreenEditorCommands.SelectNextElementInTabOrder(TabDirection.Up);

            Assert.IsTrue(result);
        }

        [Test]
        public void Copy()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_ScreenDesignerView.GetScreenDataObject().Returns(Substitute.For<IScreenDataObject>());
            bool result = m_ScreenEditorCommands.Copy();

            Assert.IsTrue(result);
        }

        [Test]
        public void Cut()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            bool result = m_ScreenEditorCommands.Cut();

            Assert.IsTrue(result);
        }

        [Test]
        public void Paste()
        {
            //Stub.On(m_ScreenDesignerView).Method("Paste").WithNoArguments().Will(Return.Value(null));
            bool result = m_ScreenEditorCommands.Paste();

            Assert.IsTrue(result);
        }

        [Test]
        public void CanExecuteReturnsTrueOnSingleObjectSelected()
        {
            Assert.AreEqual(1, m_SelectionList.Count);

            bool canExecute = m_ScreenEditorCommands.CanExecuteDelete();
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void CanExecuteReturnsTrueOnMultipleObjectsSelected()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);

            Assert.AreEqual(2, m_SelectionList.Count);

            bool canExecute = m_ScreenEditorCommands.CanExecuteDelete();
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void CanExecuteDeleteReturnsFalseWhenScreenWindowIsSelected()
        {
            FrameworkElement screenStub = Substitute.For<ScreenWindowMock>();
            m_SelectionList.Add(screenStub);

            bool canExecute = m_ScreenEditorCommands.CanExecuteDelete();
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void CanExecuteDeleteReturnsFalseWhenAnySelectedObjectIsInsideGroup()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            m_ScreenEditorCommands.Group();

            m_SelectionList.Add(m_MasterRectangle);

            bool canExecute = m_ScreenEditorCommands.CanExecuteDelete();
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void GroupingRectanglesChangesSelection()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);

            Assert.AreEqual(2, m_SelectionList.Count);
            Assert.IsInstanceOf<Rectangle>(m_SelectionList[0]);
            Assert.IsInstanceOf<Rectangle>(m_SelectionList[1]);

            bool result = m_ScreenEditorCommands.Group();
            Assert.IsTrue(result, "Group should return true");

            Assert.AreEqual(1, m_SelectionList.Count);
            Assert.IsInstanceOf<Group>(m_SelectionList[0]);
        }

        [Test]
        public void UngroupingRectanglesGroupChangesSelection()
        {
            GroupingRectanglesChangesSelection();

            Assert.AreEqual(1, m_SelectionList.Count);
            Assert.IsInstanceOf<Group>(m_SelectionList[0]);

            bool result = m_ScreenEditorCommands.Ungroup();
            Assert.IsTrue(result, "Ungroup should return true");

            Assert.AreEqual(2, m_SelectionList.Count);
            Assert.IsInstanceOf<Rectangle>(m_SelectionList[0]);
            Assert.IsInstanceOf<Rectangle>(m_SelectionList[1]);
        }

        [Test]
        public void UngroupRemovesGroupFromCanvas()
        {
            UngroupingRectanglesGroupChangesSelection();
            Assert.IsEmpty(m_ScreenEditor.Canvas.Children.OfType<Group>().ToArray());
        }

        [Test]
        public void GroupedElementsStayInPlace()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);

            IList<LayoutData> beforeGroupLayout = GetLayoutData(m_SelectionList);

            m_ScreenEditorCommands.Group();

            IList<LayoutData> afterGroupLayout = GetLayoutData(new List<FrameworkElement>() { m_MasterRectangle, m_SecondaryMasterRectangle });

            AssertPositions(beforeGroupLayout, afterGroupLayout);
        }

        [Test]
        public void UngroupedElementsStayInPlace()
        {
            GroupingRectanglesChangesSelection();

            List<FrameworkElement> rectangles = new List<FrameworkElement>() { m_MasterRectangle, m_SecondaryMasterRectangle };
            IList<LayoutData> beforeUngroupLayout = GetLayoutData(rectangles);

            m_ScreenEditorCommands.Ungroup();

            IList<LayoutData> afterUngroupLayout = GetLayoutData(rectangles);

            AssertPositions(beforeUngroupLayout, afterUngroupLayout);
        }

        [Test]
        public void CanGroupAnotherGroup()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            m_ScreenEditorCommands.Group();

            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.None);
            m_SelectionList.Add(m_SlaveRectangle);

            m_ScreenEditorCommands.Group();

            Assert.AreEqual(1, m_SelectionList.Count);
            Assert.IsInstanceOf<Group>(m_SelectionList[0]);
        }

        [Test]
        public void ObjectsInGroupStaysInPlaceWhenGrouping()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.None);
            List<FrameworkElement> rectangles = new List<FrameworkElement>() { m_MasterRectangle, m_SecondaryMasterRectangle, m_SlaveRectangle };
            IList<LayoutData> beforeGroupInGroupLayout = GetLayoutData(rectangles);

            m_ScreenEditorCommands.Group();

            m_SelectionList.Add(m_SlaveRectangle);
            m_ScreenEditorCommands.Group();

            IList<LayoutData> afterGroupInGroupLayout = GetLayoutData(rectangles);

            AssertPositions(beforeGroupInGroupLayout, afterGroupInGroupLayout);
        }

        [Test]
        public void ObjectsInGroupStaysInPlaceWhenUngrouping()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.None);
            List<FrameworkElement> rectangles = new List<FrameworkElement>() { m_MasterRectangle, m_SecondaryMasterRectangle, m_SlaveRectangle };
            IList<LayoutData> beforeUngroupUngroupLayout = GetLayoutData(rectangles);

            m_ScreenEditorCommands.Group();

            m_SelectionList.Add(m_SlaveRectangle);
            m_ScreenEditorCommands.Group();

            m_ScreenEditorCommands.Ungroup();
            Assert.AreEqual(2, m_SelectionList.Count);
            Assert.IsInstanceOf<Group>(m_SelectionList[0]);
            Assert.IsInstanceOf<Rectangle>(m_SelectionList[1]);

            m_ScreenEditorCommands.Ungroup();
            IList<LayoutData> afterUngroupUngroupLayout = GetLayoutData(rectangles);

            AssertPositions(beforeUngroupUngroupLayout, afterUngroupUngroupLayout);
        }

        [Test(Description = "Tests a feature that isn't available in the designer since Undo doesn't properly support it.")]
        public void ObjectsInGroupStaysInPlaceWhenUngroupingInnerGroup()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.None);
            List<FrameworkElement> rectangles = new List<FrameworkElement>() { m_MasterRectangle, m_SecondaryMasterRectangle, m_SlaveRectangle };
            IList<LayoutData> beforeUngroupInnerGroupLayout = GetLayoutData(rectangles);

            m_ScreenEditorCommands.Group();
            Group innerGroup = m_SelectionList[0] as Group;

            m_SelectionList.Add(m_SlaveRectangle);
            m_ScreenEditorCommands.Group();

            m_SelectionList.Clear();
            m_SelectionList.Add(innerGroup);
            m_ScreenEditorCommands.Ungroup();

            IList<LayoutData> afterUngroupInnerGroupLayout = GetLayoutData(rectangles);

            AssertPositions(beforeUngroupInnerGroupLayout, afterUngroupInnerGroupLayout);
        }

        [Test(Description = "Tests a feature that isn't available in the designer since Undo doesn't properly support it.")]
        public void InnerGroupIsRemovedOnUngroupOnIt()
        {
            CreateSecondaryMasterAndSetSnap(SnapStyle.None);
            CreateSecondSmallerRectAndSetSnapLines(SnapStyle.None);

            m_ScreenEditorCommands.Group();
            Group innerGroup = m_SelectionList[0] as Group;

            m_SelectionList.Add(m_SlaveRectangle);
            m_ScreenEditorCommands.Group();
            Group outerGroup = m_SelectionList[0] as Group;
            Assert.IsTrue(outerGroup.Items.Contains(innerGroup));

            m_SelectionList.Clear();
            m_SelectionList.Add(innerGroup);
            m_ScreenEditorCommands.Ungroup();

            Assert.IsFalse(outerGroup.Items.Contains(innerGroup));
        }

        #region Helpers
        private List<LayoutData> GetLayoutData(FrameworkElement element)
        {
            List<FrameworkElement> elements = new List<FrameworkElement>() { element };
            return GetLayoutData(elements);
        }

        private List<LayoutData> GetLayoutData(IList<FrameworkElement> elements)
        {
            List<LayoutData> layouts = new List<LayoutData>();
            foreach (FrameworkElement element in elements)
            {
                LayoutData layoutData = new LayoutData(element);
                layouts.Add(layoutData);
            }
            return layouts;
        }

        private bool Move(Direction direction)
        {
            bool result = false;
            switch (direction)
            {
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Right:
                    result = m_ScreenEditorCommands.MoveRight();
                    break;
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Left:
                    result = m_ScreenEditorCommands.MoveLeft();
                    break;
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Up:
                    result = m_ScreenEditorCommands.MoveUp();
                    break;
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Down:
                    result = m_ScreenEditorCommands.MoveDown();
                    break;
            }
            return result;
        }

        private void AssertPositions(IList<LayoutData> beforeChange, IList<LayoutData> afterChange)
        {
            bool areEqual = beforeChange.SequenceEqual(afterChange);
            Assert.IsTrue(areEqual);
        }

        private void AssertOffset(List<LayoutData> beforeMove, List<FrameworkElement> rectangles, Direction direction)
        {
            List<LayoutData> afterMove = GetLayoutData(rectangles);

            //Get the desired offsets.
            int offsetX = GetOffsetX(direction);
            int offsetY = GetOffsetY(direction);

            for (int index = 0; index < afterMove.Count; index++)
            {
                CompareRectangles(beforeMove[index], afterMove[index], offsetX, offsetY);
            }
        }

        private void CompareRectangles(LayoutData master, LayoutData slave, int offsetX, int offsetY)
        {
            Assert.AreEqual(offsetX, master.Left - slave.Left, "The offsetX is not correct");
            Assert.AreEqual(offsetY, master.Top - slave.Top, "The offsetY is not correct");
        }

        private int GetOffsetX(Direction direction)
        {
            switch (direction)
            {
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Right:
                    return -1;
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Left:
                    return 1;
            }
            return 0;
        }

        private int GetOffsetY(Direction direction)
        {
            switch (direction)
            {
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Up:
                    return 1;
                case Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Snap.Direction.Down:
                    return -1;
            }
            return 0;
        }

        private Point GetTopLeftCorner(Rectangle rect)
        {
            return new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));
        }

        private Point GetBottomRightCorner(Rectangle rect)
        {
            return new Point(Canvas.GetLeft(rect) + rect.Width, Canvas.GetTop(rect) + rect.Height);
        }

        private void SetSnapToGrid()
        {
            ((ISnapService)m_SnapManager).SnapStyle = SnapStyle.Grid;
            m_SnapGridStategy = m_SnapManager.CurrentStategy as SnapGridStategy;

            Canvas.SetLeft(m_MasterRectangle, 4 * ((ISnapService)m_SnapManager).GridSize.X);
            m_MasterRectangle.UpdateLayout();
        }

        private void CreateSecondSmallerRectAndSetSnapLines(SnapStyle snapStyle)
        {
            ((ISnapService)m_SnapManager).SnapStyle = snapStyle;
            m_SnapLineStrategy = m_SnapManager.CurrentStategy as SnapLineStrategy;
            SnapLine snapLine = new SnapLine();

            m_SlaveRectangle = new Rectangle();
            m_SlaveRectangle.Stroke = Brushes.Black;

            m_ScreenEditor.Canvas.Children.Add(m_SlaveRectangle);

            Canvas.SetLeft(m_SlaveRectangle, MasterLeft + snapLine.SnappingDistance + 1);
            Canvas.SetTop(m_SlaveRectangle, MasterTop + snapLine.SnappingDistance + 1);
            m_SlaveRectangle.Width = MasterWidth - (snapLine.SnappingDistance + 1) * 2;
            m_SlaveRectangle.Height = MasterHeight - (snapLine.SnappingDistance + 1) * 2;
            m_SlaveRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_SlaveRectangle.RenderTransform = Transform.Identity;
            m_SlaveRectangle.UpdateLayout();

            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(m_SlaveRectangle);
            m_BoundingBoxes.Add(layoutObjectAdapter.BoundingBox);
        }

        private void CreateSecondLargerRectAndSetSnapLines(SnapStyle snapStyle)
        {
            ((ISnapService)m_SnapManager).SnapStyle = snapStyle;
            m_SnapLineStrategy = m_SnapManager.CurrentStategy as SnapLineStrategy;
            SnapLine snapLine = new SnapLine();

            m_SlaveRectangle = new Rectangle();
            m_SlaveRectangle.Stroke = Brushes.Black;

            m_ScreenEditor.Canvas.Children.Add(m_SlaveRectangle);

            Canvas.SetLeft(m_SlaveRectangle, MasterLeft - snapLine.SnappingDistance - 1);
            Canvas.SetTop(m_SlaveRectangle, MasterTop - snapLine.SnappingDistance - 1);
            m_SlaveRectangle.Width = MasterWidth + (snapLine.SnappingDistance + 1) * 2;
            m_SlaveRectangle.Height = MasterHeight + (snapLine.SnappingDistance + 1) * 2;
            m_SlaveRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_SlaveRectangle.RenderTransform = Transform.Identity;
            m_SlaveRectangle.UpdateLayout();

            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(m_SlaveRectangle);
            m_BoundingBoxes.Add(layoutObjectAdapter.BoundingBox);
        }

        private void CreateSecondaryMasterAndSetSnap(SnapStyle snapStyle)
        {
            ((ISnapService)m_SnapManager).SnapStyle = snapStyle;
            m_SnapLineStrategy = m_SnapManager.CurrentStategy as SnapLineStrategy;
            SnapLine snapLine = new SnapLine();

            m_SecondaryMasterRectangle = new Rectangle();
            m_SecondaryMasterRectangle.Stroke = Brushes.Black;

            m_ScreenEditor.Canvas.Children.Add(m_SecondaryMasterRectangle);

            Canvas.SetLeft(m_SecondaryMasterRectangle, MasterLeft + MasterWidth + snapLine.SnappingDistance);
            Canvas.SetTop(m_SecondaryMasterRectangle, MasterTop - snapLine.SnappingDistance);
            m_SecondaryMasterRectangle.Width = MasterWidth + (snapLine.SnappingDistance + 1) * 2;
            m_SecondaryMasterRectangle.Height = MasterHeight + (snapLine.SnappingDistance + 1) * 2;
            m_SecondaryMasterRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_SecondaryMasterRectangle.RenderTransform = Transform.Identity;
            m_SecondaryMasterRectangle.UpdateLayout();

            m_SelectionList.Add(m_SecondaryMasterRectangle);
        }

        private void RemoveSlaveRect()
        {
            m_BoundingBoxes.Clear();
        }

        #endregion

    }
    /// <summary>
    /// Helpclass for remembering coordinates for the rectangles.
    /// </summary>
    [DebuggerDisplay("LayoutData, Left: {Left}, Top: {Top}, Width: {Width}, Height: {Height}")]
    public class LayoutData
    {
        private Rect m_Position;

        public LayoutData(FrameworkElement element)
        {
            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(element);
            if (layoutObjectAdapter != null)
            {
                Width = layoutObjectAdapter.Width;
                Height = layoutObjectAdapter.Height;
            }

            ApplyOffset(element);
        }

        private void ApplyOffset(FrameworkElement element)
        {
            if (element == null || element is NeoElementCanvas)
                return;

            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(element);
            if (layoutObjectAdapter != null)
            {
                Left += layoutObjectAdapter.Left;
                Top += layoutObjectAdapter.Top;
            }
            ApplyOffset(element.Parent as FrameworkElement);
        }

        public double Left
        {
            get { return m_Position.X; }
            set { m_Position.X = value; }
        }

        public double Top
        {
            get { return m_Position.Y; }
            set { m_Position.Y = value; }
        }

        public double Width
        {
            get { return m_Position.Width; }
            set { m_Position.Width = value; }
        }

        public double Height
        {
            get { return m_Position.Height; }
            set { m_Position.Height = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is LayoutData)
                return m_Position == ((LayoutData)obj).m_Position;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_Position.GetHashCode();
        }
    }

    public abstract class ScreenWindowMock : FrameworkElement, IScreenWindow
    {
        public event EventHandler<ActiveScreenChangedEventArgs> Shown;
        public event CancelEventHandler BeforeClosing;
        public event EventHandler Closed;
        public abstract string InstanceName { get; set; }
        public abstract System.Drawing.Point ScreenPosition { get; set; }
        public abstract Size ScreenSize { get; set; }
        public abstract SecurityGroups SecurityGroups { get; set; }
        public abstract bool PopupScreen { get; set; }
        public abstract bool ModalScreen { get; set; }
        public abstract ITemplateScreen TemplateScreen { get; set; }
        public abstract string StyleName { get; set; }
        public abstract ScreenBorderStyle BorderStyle { get; set; }
        public abstract string ScreenTitle { get; set; }
        public abstract ushort? ScreenID { get; set; }
        public abstract IScreenAdapter Adapter { get; }
        public abstract Brush Background { get; set; }
        public abstract Canvas Canvas { get; }
        public abstract WindowState WindowState { get; set; }
        public abstract string ProxyCookie { get; }
        public abstract void Close();
        public abstract void CloseNavigation();
        public abstract void Hide();
        public abstract void Show();
        public abstract void Show(System.Drawing.Point position);
        public abstract void Show(int left, int top);
        public abstract void StartScreen();
        public abstract void ForwardScreen();
        public abstract void BackScreen();
        public abstract void InitializeSecurity();
        public abstract bool IsCachedDeactivated { get; }
        public abstract bool IsInCache { get; }
        public abstract void UpdateScreenSize(Size newSize);
        public abstract string GetText();
        public abstract void BindAliases();
        public abstract void NavigatedClose();
        public abstract void DisconnectProxies();
        public abstract ITagActions GetBoundDataItem(string aliasName);
        public abstract bool IsCacheable { get; set; }
        public abstract void DeactivateProxies();
        public abstract void ActivateProxies();
        public abstract void InvokeCloseHandlers();


        protected virtual void OnShown(ActiveScreenChangedEventArgs e)
        {
            if (Shown != null)
            {
                Shown(this, e);
            }
        }

        protected virtual void OnBeforeClosing(CancelEventArgs e)
        {
            if (BeforeClosing != null)
            {
                BeforeClosing(this, e);
            }
        }

        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
            {
                Closed(this, e);
            }
        }
    }
}