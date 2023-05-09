using System.Windows;
using System.Windows.Controls;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities.Utilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance
{
    [TestFixture]
    public class AppearanceAdapterServiceTest : AppearanceAdapterTestBase
    {
        private NeoShape m_RectangleOne;
        private NeoShape m_RectangleTwo;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_RectangleOne = CreateElement<Rectangle>();
            m_RectangleTwo = CreateElement<Rectangle>();
        }

        [Test]
        public void AppearanceAdapterServiceIsNotNullInRunTime()
        {
            m_ToolManagerMock.Expect(x => x.Runtime).Return(true);

            ElementCanvas elementCanvas = new ElementCanvas();
            elementCanvas.Children.Add(m_RectangleOne);
            IAppearanceAdapterService appearanceAdapterService = m_RectangleOne.GetService<IAppearanceAdapterService>();
            Assert.IsNotNull(appearanceAdapterService, "AppearanceAdapterService should not be null");
        }

        [Test]
        public void AppearanceAdapterServiceIsNotNullInDesignTime()
        {
            m_ToolManagerMock.Expect(x => x.Runtime).Return(false);

            ElementCanvas elementCanvas = ElementCanvasHelper.GetElementCanvasWithServiceProvider();
            elementCanvas.Children.Add(m_RectangleOne);
            IAppearanceAdapterService appearanceAdapterService = m_RectangleOne.GetService<IAppearanceAdapterService>();
            Assert.IsNotNull(appearanceAdapterService, "AppearanceAdapterService should not be null");
        }

        [Test]
        public void TestThatAShapeGetsAnAdapter()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_RectangleOne);
            Assert.IsNotNull(appearanceAdapter, "The factory cannot create an adapter for shapes");
        }

        [Test]
        public void TestThatTwoShapesGetsDifferentAdapters()
        {
            IAppearanceAdapter appearanceAdapterOne = m_AppearanceAdapterService.GetAppearanceAdapter(m_RectangleOne);
            IAppearanceAdapter appearanceAdapterTwo = m_AppearanceAdapterService.GetAppearanceAdapter(m_RectangleTwo);

            Assert.IsFalse(appearanceAdapterOne == appearanceAdapterTwo, "Two different shapes shall not get same adapter");
        }

        [Test]
        public void TestThatAShapeKeepsAnAdapter()
        {
            IAppearanceAdapter appearanceAdapterOne = m_AppearanceAdapterService.GetAppearanceAdapter(m_RectangleOne);
            IAppearanceAdapter appearanceAdapterTwo = m_AppearanceAdapterService.GetAppearanceAdapter(m_RectangleOne);

            Assert.AreSame(appearanceAdapterOne, appearanceAdapterTwo, "Only one adapter instance per object");
        }

        [Test]
        public void TestThatNullObjectReturnsNullApperanceAdapter()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(null);

            Assert.IsNull(appearanceAdapter, "No appearanceAdapter should be returned for null value");
        }

        [Test]
        public void TestThatAShapeHasAppearance()
        {
            Assert.IsTrue(HasAppearance<System.Windows.Shapes.Rectangle>());
        }

        [Test]
        public void TestThatAMeterHasAppearance()
        {
            Assert.IsTrue(HasAppearance<Meter>());
        }

        [Test]
        public void TestThatALineAndArcHasAppearance()
        {
            Assert.IsTrue(HasAppearance<System.Windows.Shapes.Line>());
            Assert.IsTrue(HasAppearance<Neo.ApplicationFramework.Controls.Shapes.Arc>());
        }

        [Test]
        public void ControlHasFontAppearance()
        {
            Control control = CreateElement<Control>();
            Assert.IsTrue(m_AppearanceAdapterService.HasFontAppearance(control));
        }

   
        private bool HasAppearance<T>() where T : UIElement, new()
        {
            T element = CreateElement<T>();
            return m_AppearanceAdapterService.HasAppearance(element);
        }
    }
}
