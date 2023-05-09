using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Tools;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class LayoutObjectAdapterWithDynamicsMoveTest
    {
        #region Fields

        private const double MasterLeft = 50;
        private const double MasterTop = 80;
        private const double MasterWidth = 300;
        private const double MasterHeight = 100;

        private ScreenEditorTestWindow m_ScreenEditor;
        private Rectangle m_MasterRectangle;
        private ILayoutObjectAdapter m_MasterLayoutAdapter;
        private IPropertyBinderWpf m_PropertyBinder;
        private object m_DummyBinding;

        #endregion

        #region TestFixtureSetUp

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.ClearServices();

            var toolManagerMock = MockRepository.GenerateStub<IToolManager>();
            toolManagerMock.Stub(x => x.Runtime).Return(false);
            TestHelper.AddService<IToolManager>(toolManagerMock);

            m_DummyBinding = new object();

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.ClearServices();

            m_ScreenEditor.Close();
        }

        #endregion

        #region SetUp

        [SetUp]
        public void SetUp()
        {
            m_MasterRectangle = new Rectangle();
            m_MasterRectangle.Stroke = Brushes.Black;
            m_MasterRectangle.Width = MasterWidth;
            m_MasterRectangle.Height = MasterHeight;
            m_MasterRectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            m_MasterRectangle.RenderTransform = Transform.Identity;

            Canvas.SetLeft(m_MasterRectangle, MasterLeft);
            Canvas.SetTop(m_MasterRectangle, MasterTop);

            m_ScreenEditor.Canvas.Children.Add(m_MasterRectangle);
            m_MasterRectangle.UpdateLayout();

            m_PropertyBinder = MockRepository.GenerateStub<IPropertyBinderWpf>();
            m_PropertyBinder.Stub(x => x.GetBinding(m_MasterRectangle, Canvas.LeftProperty)).Return(m_DummyBinding);
            m_PropertyBinder.Stub(x => x.GetBinding(m_MasterRectangle, Canvas.TopProperty)).Return(m_DummyBinding);
            m_PropertyBinder.Stub(x => x.GetBinding(m_MasterRectangle, FrameworkElement.WidthProperty)).Return(null);
            m_PropertyBinder.Stub(x => x.GetBinding(m_MasterRectangle, FrameworkElement.HeightProperty)).Return(null);

            m_MasterLayoutAdapter = new LayoutObjectAdapter(m_MasterRectangle, m_PropertyBinder); 
        }

        [TearDown]
        public void TearDown()
        {
            m_ScreenEditor.Canvas.Children.Remove(m_MasterRectangle);
        }

        #endregion

        #region Tests

        [Test]
        public void ResizeUsingBottomRightCornerChangesSizeButLeavesPositionUnchanged()
        {
            m_MasterLayoutAdapter.Resize(MasterWidth * 2, MasterHeight * 2, Corner.TopLeft);

            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left, "Left");
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top, "Top");
            Assert.AreEqual(MasterWidth * 2, m_MasterLayoutAdapter.Width, "Width");
            Assert.AreEqual(MasterHeight * 2, m_MasterLayoutAdapter.Height, "Height");
        }

        [Test]
        public void ResizeUsingTopLeftCornerLeavesSizeAndPositionUnchanged()
        {
            m_MasterLayoutAdapter.Resize(MasterWidth * 2, MasterHeight * 2, Corner.BottomRight);

            Assert.AreEqual(MasterLeft, m_MasterLayoutAdapter.Left, "Left");
            Assert.AreEqual(MasterTop, m_MasterLayoutAdapter.Top, "Top");
            Assert.AreEqual(MasterWidth, m_MasterLayoutAdapter.Width, "Width");
            Assert.AreEqual(MasterHeight, m_MasterLayoutAdapter.Height, "Height");
        }

        #endregion

    }
}