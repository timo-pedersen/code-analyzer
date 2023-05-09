using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Layout;
using Neo.ApplicationFramework.Controls.Symbol;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.ScreenEditor.Layout
{
    [TestFixture]
    public class LayoutObjectAdapterPictureTest
    {
        private const int ImageLeft = 100;
        private const int ImageTop = 50;
        private const int ImageWidth = 640;
        private const int ImageHeight = 480;

        private Picture m_Picture;
        private ILayoutObjectAdapter m_LayoutAdapter;
        private ScreenEditorTestWindow m_ScreenEditor;
        private BitmapSource m_BitmapSource;

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            TestHelper.ClearServices();
        }

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Expect(x => x.Runtime).Return(false);

            m_BitmapSource = MockRepository.GenerateMock<BitmapSource>();
            m_BitmapSource.Stub(x => x.PixelWidth).Return(ImageWidth).Repeat.Any();
            m_BitmapSource.Stub(x => x.PixelHeight).Return(ImageHeight).Repeat.Any();

            ISymbolService symbolService = TestHelper.AddServiceStub<ISymbolService>();
            symbolService.Expect(x => x.GetSymbolFx("Glurp")).Return(m_BitmapSource).Repeat.Any();

            m_ScreenEditor = new ScreenEditorTestWindow();
            m_ScreenEditor.Show();

            m_Picture = new Picture();
            m_ScreenEditor.Canvas.Children.Add(m_Picture);

            Canvas.SetLeft(m_Picture, ImageLeft);
            Canvas.SetTop(m_Picture, ImageTop);
            m_Picture.Width = ImageWidth;
            m_Picture.Height = ImageHeight;
            
            m_Picture.UpdateLayout();
            m_Picture.SymbolName = "Glurp";

            m_LayoutAdapter = LayoutObjectAdapterFactory.Instance.GetLayoutObjectAdapter(m_Picture);
        }

        [TearDown]
        public void TearDown()
        {
            m_ScreenEditor.Close();

            TestHelper.ClearServices();
        }

        [Test]
        public void ResizeWidthOnSideIsAcceptedWhenStretchIsTrue()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(1024, ImageHeight, Corner.Left);

            Assert.AreEqual(1024, m_LayoutAdapter.Width);
            Assert.AreEqual(ImageHeight, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeWidthOnSideIsRejectedWhenStretchIsFalse()
        {
            m_Picture.Stretch = false;
            m_LayoutAdapter.Resize(1024, ImageHeight, Corner.Left);

            Assert.AreEqual(ImageWidth, m_LayoutAdapter.Width);
            Assert.AreEqual(ImageHeight, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeHeightOnSideIsAcceptedWhenStretchIsTrue()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(ImageWidth, 768, Corner.Top);

            Assert.AreEqual(ImageWidth, m_LayoutAdapter.Width);
            Assert.AreEqual(768, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeHeightOnSideIsRejectedWhenStretchIsFalse()
        {
            m_Picture.Stretch = false;
            m_LayoutAdapter.Resize(ImageWidth, 768, Corner.Top);

            Assert.AreEqual(ImageWidth, m_LayoutAdapter.Width);
            Assert.AreEqual(ImageHeight, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeWidthOnCornerKeepsAspectRatio()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(1024, ImageHeight, Corner.TopLeft);

            double newHeight = 1024 * ImageHeight / ImageWidth;
            Assert.AreEqual(1024, m_LayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeHeightOnCornerKeepsAspectRatio()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(ImageWidth, 768, Corner.TopLeft);

            double newWidth = 768 * ImageWidth / ImageHeight;
            Assert.AreEqual(newWidth, m_LayoutAdapter.Width);
        }

        [Test]
        public void ResizeBothWidthAndHeightOnCornerWhereWidthIsIncreasedMoreKeepsAspectRatio()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(1024, ImageHeight + 100, Corner.TopLeft);

            double newHeight = 1024 * ImageHeight / ImageWidth;
            Assert.AreEqual(1024, m_LayoutAdapter.Width);
            Assert.AreEqual(newHeight, m_LayoutAdapter.Height);
        }

        [Test]
        public void ResizeBothWidthAndHeightOnCornerWhereHeightIsIncreasedMoreKeepsAspectRatio()
        {
            m_Picture.Stretch = true;
            m_LayoutAdapter.Resize(ImageWidth + 100, 768, Corner.TopLeft);

            double newWidth = 768 * ImageWidth / ImageHeight;
            Assert.AreEqual(newWidth, m_LayoutAdapter.Width);
        }

        [Test]
        public void ResizeOnCornerKeepsNewAspectRatioEvenWhenItDiffersFromOriginalAspectRatio()
        {
            double newHeight = 200;

            m_Picture.Stretch = true;
            m_LayoutAdapter.IsResizing = true;
            m_LayoutAdapter.Resize(ImageWidth, newHeight, Corner.Top);
            m_LayoutAdapter.IsResizing = false;

            double newAspectRatio = ImageWidth / newHeight;

            m_LayoutAdapter.IsResizing = true;
            m_LayoutAdapter.Resize(ImageWidth + 100, newHeight, Corner.TopLeft);
            m_LayoutAdapter.IsResizing = false;

            Assert.That(m_LayoutAdapter.Width / m_LayoutAdapter.Height, Is.EqualTo(newAspectRatio));
        }
    }
}
