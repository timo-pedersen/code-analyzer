using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Feature;
using Core.Api.Tools;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance;
using Neo.ApplicationFramework.Controls.Undo;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Undo
{
    [TestFixture]
    public class UndoUnitTest
    {
        private const double MasterLeft = 50;
        private const double MasterTop = 80;
        private const double MasterWidth = 320;
        private const double MasterHeight = 104;

        private INeoDesignerHost m_DesignerHostStub;
        private ScreenEditorTestWindow m_ScreenEditorWindow;
        private IScreenDesignerView m_ScreenDesignerView;
        private IScreenRootDesigner m_ScreenRootDesigner;
        private ServiceContainer m_ServiceProvider;
        private Rectangle m_MasterRectangle;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;
        private IUndoService m_UndoService;
        private ISelectionService m_SelectionService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            TestHelper.AddServiceStub<IXmlConverterService>();
            TestHelper.AddServiceStub<IFeatureSecurityServiceIde>();

            var toolManagerStub = MockRepository.GenerateStub<IToolManager>();
            toolManagerStub.Stub(x => x.Runtime).Return(false);
            TestHelper.AddService(toolManagerStub);

            m_ServiceProvider = new ServiceContainer();

            // Setup undo service
            IUndoManager undoManager = new UndoManager.UndoManager();
            m_ServiceProvider.AddService(typeof(IUndoManager), undoManager);

            SetupScreenDesignerStub();

            m_UndoService = m_ServiceProvider.GetService(typeof(IUndoService)) as IUndoService;

            m_MasterRectangle = new Rectangle();
            m_MasterRectangle.Stroke = Brushes.Black;
            m_MasterLayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_MasterRectangle);

            m_MasterRectangle.Width = MasterWidth;
            m_MasterRectangle.Height = MasterHeight;
            m_MasterRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterRectangle.RenderTransform = Transform.Identity;
            m_MasterRectangle.Name = "MasterRectangle";
            m_MasterRectangle.UpdateLayout();
        }

        [TearDown]
        public void TearDown()
        {
            m_ScreenEditorWindow.Canvas.Children.Clear();
            m_ScreenEditorWindow.Close();

            TestHelper.ClearServices();
        }

        [Test]
        public void UndoEmptyStack()
        {
            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
            Assert.IsFalse(IsUndoEnabled, "Undo should not be enabled");

            m_UndoService.Undo();

            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
            Assert.IsFalse(IsUndoEnabled);
        }

        [Test]
        public void RedoEmptyStack()
        {
            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
            Assert.IsFalse(IsUndoEnabled, "Undo should not be enabled");

            m_UndoService.Redo();

            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
            Assert.IsFalse(IsUndoEnabled);
        }

        [Test]
        public void UndoAddObject()
        {
            m_ScreenDesignerView.AddElement(m_MasterRectangle);
            Assert.AreEqual(1, NumChildren, "Children of canvas should be 1");
            Assert.IsTrue(IsUndoEnabled);

            m_UndoService.Undo();
            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
            Assert.IsFalse(IsUndoEnabled, "Undo should not be enabled");
        }

        [Test]
        public void UndoAddObjects()
        {
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle("SlaveRectangle", 10);
            Assert.AreEqual(2, NumChildren, "Children of canvas should be 2");

            m_SelectionService.SetSelectedComponents(ObjectList);
            Assert.AreEqual(2, NumSelectedObjects, "Should have 2 selected objects");

            UndoAdd undoAdd = new UndoAdd("Undo Add Elements", ObjectList, m_DesignerHostStub);
            undoAdd.Register();

            m_UndoService.Undo();
            Assert.AreEqual(0, NumChildren, "Children should be 0.");
        }

        [Test]
        public void RedoAddObject()
        {
            m_ScreenDesignerView.AddElement(m_MasterRectangle);
            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(1, NumChildren, "Children of canvas should be 1");
            Assert.IsTrue(IsUndoEnabled);
            Assert.AreEqual(1, NumSelectedObjects, "The object should have focus again");
        }

        [Test]
        public void RedoAddObjects()
        {
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle("SlaveRectangle", 10);

            m_SelectionService.SetSelectedComponents(ObjectList);

            UndoAdd undoAdd = new UndoAdd("Undo Add Elements", ObjectList, m_DesignerHostStub);
            undoAdd.Register();

            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(2, NumSelectedObjects, "Both objects should get focus.");
        }

        [Test]
        public void UndoDeleteObject()
        {
            m_ScreenDesignerView.AddElement(m_MasterRectangle);
            Assert.AreEqual(1, NumChildren, "Children of canvas should be 1");
            Assert.IsTrue(IsUndoEnabled);

            UndoDelete undoDelete = new UndoDelete("Delete Elements", ObjectList, m_DesignerHostStub);
            undoDelete.Register();

            m_ScreenDesignerView.RemoveElement(m_MasterRectangle);
            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");

            m_UndoService.Undo();
            Assert.AreEqual(1, NumSelectedObjects);
            Assert.AreEqual(1, NumChildren);
            FrameworkElement frameworkElement = m_SelectionService.PrimarySelection as FrameworkElement;
            Assert.AreEqual("MasterRectangle", frameworkElement.Name, "Name should be correct");
        }

        [Test]
        public void UndoDeleteObjects()
        {
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle("SlaveRectangle", 10);
            Assert.AreEqual(2, NumChildren, "Children of canvas should be 2");

            m_SelectionService.SetSelectedComponents(ObjectList);
            Assert.AreEqual(2, NumSelectedObjects, "Should have 2 selected objects");

            UndoDelete undoDelete = new UndoDelete("Delete Elements", ObjectList, m_DesignerHostStub);
            undoDelete.Register();

            IScreenEditorCommands screenEditorCommands = m_DesignerHostStub.GetService<IScreenEditorCommands>();
            screenEditorCommands.Delete();
            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");

            m_UndoService.Undo();
            Assert.AreEqual(2, NumChildren, "Both objects should be recreated");
            Assert.AreEqual(2, NumSelectedObjects, "Both objects should be selected");
        }

        [Test]
        public void RedoDeleteObject()
        {
            m_ScreenDesignerView.AddElement(m_MasterRectangle);

            UndoDelete undoDelete = new UndoDelete("Delete Elements", ObjectList, m_DesignerHostStub);
            undoDelete.Register();

            m_ScreenDesignerView.RemoveElement(m_MasterRectangle);

            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
        }

        [Test]
        public void RedoDeleteObjects()
        {
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle("SlaveRectangle", 10);

            m_SelectionService.SetSelectedComponents(ObjectList);

            UndoDelete undoDelete = new UndoDelete("Delete Elements", ObjectList, m_DesignerHostStub);
            undoDelete.Register();

            IScreenEditorCommands screenEditorCommands = m_DesignerHostStub.GetService<IScreenEditorCommands>();
            screenEditorCommands.Delete();

            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(0, NumChildren, "Children of canvas should be 0");
        }

        [Test]
        public void UndoMove()
        {
            string slaveName = "SlaveRectangle";
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle(slaveName, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);
            Rectangle slaveRect = GetRectangle(slaveName);
            Assert.IsTrue(CompareRectangles(slaveRect, m_MasterRectangle));

            UndoMove undoMove = new UndoMove("Move Right", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoMove.Register();

            m_MasterLayoutAdapter.Left += 1;
            m_MasterRectangle.UpdateLayout();
            slaveRect.UpdateLayout();
            Assert.IsFalse(CompareRectangles(slaveRect, m_MasterRectangle));

            m_UndoService.Undo();

            m_MasterRectangle.UpdateLayout();
            slaveRect.UpdateLayout();
            Assert.IsTrue(CompareRectangles(slaveRect, m_MasterRectangle));
        }

        [Test]
        public void UndoMoveMultiple()
        {
            string firstSlaveName = "SlaveRectangle1";
            string secondSlaveName = "SlaveRectangle2";
            AddRectangle(firstSlaveName, 0);
            AddRectangle(secondSlaveName, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);
            AddRectangle(m_MasterRectangle, 0);

            Rectangle firstSlave = GetRectangle(firstSlaveName);
            Rectangle secondSlave = GetRectangle(secondSlaveName);
            Assert.IsTrue(CompareRectangles(m_MasterRectangle, firstSlave));
            Assert.IsTrue(CompareRectangles(m_MasterRectangle, secondSlave));

            UndoMove undoMove = new UndoMove("Move", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoMove.Register();

            ILayoutObjectAdapter firstLayout = new LayoutObjectAdapter(firstSlave);
            ILayoutObjectAdapter secondLayout = new LayoutObjectAdapter(secondSlave);

            firstLayout.Left += 10;
            secondLayout.Left += 5;

            firstSlave.UpdateLayout();
            secondSlave.UpdateLayout();
            Assert.IsFalse(CompareRectangles(m_MasterRectangle, firstSlave));
            Assert.IsFalse(CompareRectangles(m_MasterRectangle, secondSlave));
            Assert.IsFalse(CompareRectangles(firstSlave, secondSlave));

            m_UndoService.Undo();

            firstSlave.UpdateLayout();
            secondSlave.UpdateLayout();
            Assert.IsTrue(CompareRectangles(m_MasterRectangle, firstSlave));
            Assert.IsTrue(CompareRectangles(m_MasterRectangle, secondSlave));
            Assert.AreEqual(2, NumSelectedObjects);
        }

        [Test]
        public void RedoMove()
        {
            string slaveName = "SlaveRectangle";
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle(slaveName, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);
            Rectangle slaveRect = GetRectangle(slaveName);

            UndoMove undoMove = new UndoMove("Move Right", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoMove.Register();

            m_MasterLayoutAdapter.Left += 1;
            m_MasterRectangle.UpdateLayout();
            slaveRect.UpdateLayout();

            m_UndoService.Undo();

            m_MasterRectangle.UpdateLayout();
            slaveRect.UpdateLayout();

            m_UndoService.Redo();
            
            slaveRect.UpdateLayout();
            Assert.IsFalse(CompareRectangles(m_MasterRectangle, slaveRect));
        }

        [Test]
        public void RedoMoveMultiple()
        {
            string firstSlaveName = "SlaveRectangle1";
            string secondSlaveName = "SlaveRectangle2";
            AddRectangle(firstSlaveName, 0);
            AddRectangle(secondSlaveName, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);
            AddRectangle(m_MasterRectangle, 0);

            Rectangle firstSlave = GetRectangle(firstSlaveName);
            Rectangle secondSlave = GetRectangle(secondSlaveName);

            UndoMove undoMove = new UndoMove("Move", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoMove.Register();

            ILayoutObjectAdapter firstLayout = new LayoutObjectAdapter(firstSlave);
            ILayoutObjectAdapter secondLayout = new LayoutObjectAdapter(secondSlave);

            firstLayout.Left += 10;
            secondLayout.Left += 5;

            firstSlave.UpdateLayout();
            secondSlave.UpdateLayout();

            m_UndoService.Undo();

            firstSlave.UpdateLayout();
            secondSlave.UpdateLayout();
            Rectangle slaveOne = GetRectangle("SlaveRectangle1");
            Rectangle slaveTwo = GetRectangle("SlaveRectangle2");

            m_UndoService.Redo();

            slaveOne.UpdateLayout();
            slaveTwo.UpdateLayout();
            Assert.IsFalse(CompareRectangles(m_MasterRectangle, slaveOne));
            Assert.IsFalse(CompareRectangles(m_MasterRectangle, slaveTwo));
            Assert.IsFalse(CompareRectangles(slaveOne, slaveTwo));
        }

        [Test]
        public void UndoResizeObject()
        {
            AddRectangle(m_MasterRectangle, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);

            UndoResize undoResize = new UndoResize("Resize", m_ScreenRootDesigner.SelectedElements, Corner.TopLeft, m_DesignerHostStub);
            undoResize.Register();

            m_MasterLayoutAdapter.Resize(m_MasterRectangle.Width + 50, m_MasterRectangle.Height + 50, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            Assert.AreEqual(MasterWidth + 50, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight + 50, m_MasterRectangle.Height);

            m_UndoService.Undo();
            Assert.AreEqual(MasterWidth, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight, m_MasterRectangle.Height);
        }

        [Test]
        public void UndoResizeObjects()
        {
            string slaveName = "SlaveRectangle";
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle(slaveName, 10);
            m_SelectionService.SetSelectedComponents(ObjectList);

            Rectangle slaveRectangle = GetRectangle(slaveName);
            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(slaveRectangle);

            UndoResize undoResize = new UndoResize("Resize", m_ScreenRootDesigner.SelectedElements, Corner.TopLeft, m_DesignerHostStub);
            undoResize.Register();

            m_MasterLayoutAdapter.Resize(m_MasterRectangle.Width + 50, m_MasterRectangle.Height + 50, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            layoutObjectAdapter.Resize(slaveRectangle.Width + 100, slaveRectangle.Height + 100, Corner.TopLeft);
            slaveRectangle.UpdateLayout();

            Assert.AreEqual(MasterWidth + 50, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight + 50, m_MasterRectangle.Height);
            Assert.AreEqual(MasterWidth + 100, slaveRectangle.Width);
            Assert.AreEqual(MasterHeight + 100, slaveRectangle.Height);

            m_UndoService.Undo();
            Assert.AreEqual(MasterWidth, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight, m_MasterRectangle.Height);
            Assert.AreEqual(MasterWidth, slaveRectangle.Width);
            Assert.AreEqual(MasterHeight, slaveRectangle.Height);
        }

        [Test]
        public void RedoResizeObject()
        {
            AddRectangle(m_MasterRectangle, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);

            UndoResize undoResize = new UndoResize("Resize", m_ScreenRootDesigner.SelectedElements, Corner.TopLeft, m_DesignerHostStub);
            undoResize.Register();

            m_MasterLayoutAdapter.Resize(m_MasterRectangle.Width + 50, m_MasterRectangle.Height + 50, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(MasterWidth + 50, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight + 50, m_MasterRectangle.Height);
        }

        [Test]
        public void RedoResizeObjects()
        {
            string slaveName = "SlaveRectangle";
            AddRectangle(m_MasterRectangle, 0);
            AddRectangle(slaveName, 10);
            m_SelectionService.SetSelectedComponents(ObjectList);

            Rectangle slaveRectangle = GetRectangle(slaveName);
            ILayoutObjectAdapter layoutObjectAdapter = new LayoutObjectAdapter(slaveRectangle);

            UndoResize undoResize = new UndoResize("Resize", m_ScreenRootDesigner.SelectedElements, Corner.TopLeft, m_DesignerHostStub);
            undoResize.Register();

            m_MasterLayoutAdapter.Resize(m_MasterRectangle.Width + 50, m_MasterRectangle.Height + 50, Corner.TopLeft);
            m_MasterRectangle.UpdateLayout();

            layoutObjectAdapter.Resize(slaveRectangle.Width + 100, slaveRectangle.Height + 100, Corner.TopLeft);
            slaveRectangle.UpdateLayout();

            m_UndoService.Undo();
            m_UndoService.Redo();

            Assert.AreEqual(MasterWidth + 50, m_MasterRectangle.Width);
            Assert.AreEqual(MasterHeight + 50, m_MasterRectangle.Height);
            Assert.AreEqual(MasterWidth + 100, slaveRectangle.Width);
            Assert.AreEqual(MasterHeight + 100, slaveRectangle.Height);
        }

        [Test]
        public void UndoChangeZIndexTest()
        {
            AddRectangle(m_MasterRectangle, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);
            int zindex = m_MasterLayoutAdapter.ZIndex;

            UndoChangeZIndex undoChangeZIndex = new UndoChangeZIndex("Change ZIndex", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoChangeZIndex.Register();

            m_MasterLayoutAdapter.ZIndex += 1;
            Assert.IsFalse(zindex == m_MasterLayoutAdapter.ZIndex);

            m_UndoService.Undo();
            Assert.IsTrue(zindex == m_MasterLayoutAdapter.ZIndex);
        }

        [Test]
        public void RedoChangeZIndex()
        {
            AddRectangle(m_MasterRectangle, 0);
            m_SelectionService.SetSelectedComponents(ObjectList);

            UndoChangeZIndex undoChangeZIndex = new UndoChangeZIndex("Change ZIndex", m_ScreenRootDesigner.SelectedElements, m_DesignerHostStub);
            undoChangeZIndex.Register();

            m_MasterLayoutAdapter.ZIndex += 1;

            m_UndoService.Undo();
            int zindex = m_MasterLayoutAdapter.ZIndex;
            m_UndoService.Redo();

            Assert.IsFalse(zindex == m_MasterLayoutAdapter.ZIndex);
        }

        #region Helpers

        private int NumChildren
        {
            get
            {
                return m_ScreenDesignerView.Elements.Count;
            }
        }

        private bool IsUndoEnabled
        {
            get
            {
                return m_UndoService.IsUndoAvailable();
            }
        }

        private int NumSelectedObjects
        {
            get
            {
                return m_SelectionService.GetSelectedComponents().Count;
            }
        }

        private List<FrameworkElement> ObjectList
        {
            get { return m_ScreenDesignerView.Elements.ToList(); }
        }

        private Rectangle GetRectangle(string name)
        {
            Rectangle retVal = new Rectangle();
            foreach (Rectangle rectangle in m_ScreenDesignerView.Elements)
            {
                FrameworkElement frameworkElement = rectangle;
                if (frameworkElement != null && frameworkElement.Name == name)
                    return rectangle;
            }
            return retVal;
        }

        private bool CompareRectangles(Rectangle expectedValue, Rectangle realValue)
        {
            ILayoutObjectAdapter expected = new LayoutObjectAdapter(expectedValue);
            ILayoutObjectAdapter real = new LayoutObjectAdapter(realValue);

            if (expected.Left != real.Left)
                return false;

            return true;
        }

        private void AddRectangle(string name, double displacement)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Stroke = Brushes.Black;

            rectangle.Width = MasterWidth;
            rectangle.Height = MasterHeight;
            rectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            rectangle.RenderTransform = Transform.Identity;
            rectangle.Name = name;
            rectangle.UpdateLayout();

            AddRectangle(rectangle, displacement);
        }

        private void AddRectangle(Rectangle rectangle, double displacement)
        {
            m_ScreenDesignerView.AddElement(rectangle);

            Canvas.SetLeft(rectangle, MasterLeft + displacement);
            Canvas.SetTop(rectangle, MasterTop + displacement);
            Canvas.SetZIndex(rectangle, 0);
            rectangle.UpdateLayout();
        }

        private void SetupScreenDesignerStub()
        {
            NeoDesignerHostStub designerHostStub = NeoDesignerHostStub.CreateScreenDesignerHost(m_ServiceProvider);
            m_DesignerHostStub = designerHostStub;
            m_SelectionService = designerHostStub.SelectionService;
            m_ScreenEditorWindow = designerHostStub.ScreenEditorWindow;

            m_ServiceProvider.AddService(typeof(IAppearanceAdapterService), new AppearanceAdapterService());

            m_ScreenRootDesigner = designerHostStub.ScreenRootDesigner;
            m_ScreenDesignerView = designerHostStub.ScreenDesignerView;
        }

        #endregion
    }
}
