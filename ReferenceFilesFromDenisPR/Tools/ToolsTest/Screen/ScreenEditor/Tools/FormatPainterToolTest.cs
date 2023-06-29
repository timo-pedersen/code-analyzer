using System.ComponentModel.Design;
using System.Windows.Media;
using Core.Api.Tools;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Brush;
using Neo.ApplicationFramework.TestUtilities.Utilities;
using Neo.ApplicationFramework.Tools.PropertyGrid;
using Neo.ApplicationFramework.Tools.Selection;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Tools
{
    [TestFixture]
    public class FormatPainterToolTest
    {
        private Rectangle m_SourceRectangle;
        private Rectangle m_TargetRectangle;
        private ElementCanvas m_ElementCanvas;

        private INeoDesignerHost m_DesignerHost;

        [SetUp]
        public void SetUp()
        {
            var toolManagerMock = TestHelper.CreateAndAddServiceStub<IToolManager>();
            toolManagerMock.Runtime.Returns(false);

            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            TestHelper.AddService<IAppearanceAdapterService>(new AppearanceAdapterService());
            TestHelper.AddService<IObjectPropertyService>(new ObjectPropertyService());

            m_ElementCanvas = ElementCanvasHelper.GetElementCanvasWithServiceProvider();

            m_SourceRectangle = new Rectangle();
            m_SourceRectangle.Fill = Brushes.Blue;
            m_SourceRectangle.Stroke = Brushes.Black;
            m_SourceRectangle.StrokeThickness = 10;
            m_SourceRectangle.Opacity = 0.5;

            m_TargetRectangle = new Rectangle();
            m_TargetRectangle.Fill = Brushes.Red;
            m_TargetRectangle.Stroke = Brushes.White;
            m_TargetRectangle.StrokeThickness = 5;
            m_TargetRectangle.Opacity = 1.0;

            m_ElementCanvas.Children.Add(m_SourceRectangle);
            m_ElementCanvas.Children.Add(m_TargetRectangle);

            TestHelper.AddService<ISelectionService>(new SelectionService());
            m_DesignerHost = Substitute.For<INeoDesignerHost>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
            m_ElementCanvas.Children.Clear();
        }

        [Test]
        public void CopyFormatInfo()
        {
            var formatPainterTool = new FormatPainterTool(m_DesignerHost);
            formatPainterTool.CopyFormatInfo(m_SourceRectangle, m_TargetRectangle, null);

            BrushValidator.AssertBrushesAreEqual(Brushes.Blue, m_TargetRectangle.Fill);
            BrushValidator.AssertBrushesAreEqual(Brushes.Black, m_TargetRectangle.Stroke);
            Assert.AreEqual(0.5, m_TargetRectangle.Opacity);
            Assert.AreEqual(10, m_TargetRectangle.StrokeThickness);
        }
    }
}