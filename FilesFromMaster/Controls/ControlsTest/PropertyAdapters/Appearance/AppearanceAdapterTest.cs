using System.Collections.Generic;
using System.Windows.Media;
using Neo.ApplicationFramework.Common.Bevel;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Brush;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance
{
    [TestFixture]
    public class AppearanceAdapterTest : AppearanceAdapterTestBase
    {
        private System.Windows.Shapes.Line m_Line;
        private Meter m_Meter;
        private NeoShape m_Rectangle;
        private ScreenWindow m_ScreenWindow;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            IObjectPropertyService objectPropertyServiceStub = TestHelper.AddServiceStub<IObjectPropertyService>();
            objectPropertyServiceStub.Stub(x => x.GetDisplayName(Arg<object>.Is.Anything, Arg<string>.Is.Equal("IndicatorColor"))).Return("IndicatorColor");
            objectPropertyServiceStub.Stub(x => x.GetDisplayName(Arg<object>.Is.Anything, Arg<string>.Is.Equal("ScaleColor"))).Return("ScaleColor");

            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            TestHelper.AddServiceStub<IMultiLanguageServiceCF>();

            m_Line = CreateElement<System.Windows.Shapes.Line>();
            m_Line.Stroke = Brushes.Black;

            m_Meter = new LinearMeter();
            m_Meter.Background = Brushes.Black;
            m_Meter.IndicatorColor = Brushes.Blue;
            m_Meter.ScaleColor = Brushes.Brown;
            m_Meter.BorderBrush = Brushes.White;
            m_Meter.Foreground = Brushes.White;

            m_Rectangle = new Rectangle();
            m_Rectangle.Fill = new SolidColorBrush(Colors.White);
            m_Rectangle.Stroke = new SolidColorBrush(Colors.Black);
            m_Rectangle.StrokeThickness = 3;

            m_ScreenWindow = new ScreenWindow();
        }

        [Test]
        public void DefaultFillInfoForLineIsNull()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Line);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;

            Assert.IsNull(fillInfo);
        }

        [Test]
        public void DefaultStrokeInfoForScreenWindowIsNull()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_ScreenWindow);
            IStrokeInfo strokeInfo = appearanceAdapter.DefaultStrokeInfo;

            Assert.IsNull(strokeInfo);
        }

        [Test]
        public void GetDefaultStrokeInfoForMeter()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Meter);
            IStrokeInfo strokeInfo = appearanceAdapter.DefaultStrokeInfo;

            Assert.IsNotNull(strokeInfo);
        }

        [Test]
        public void GetDefaultFillInfoForMeter()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Meter);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;

            Assert.IsNotNull(fillInfo);
        }

        [Test]
        public void DefaultFillInfoIsNotNullForPolylineWhenTargetIsCE()
        {
            System.Windows.Shapes.Polyline polyline = new System.Windows.Shapes.Polyline();
            polyline.Stroke = new SolidColorBrush(Colors.Black);

            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(polyline);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;

            Assert.IsNotNull(fillInfo);
        }

        [Test]
        public void DefaultEffectsInfoIsNullForEllipseWhenTargetIsCE()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = new SolidColorBrush(Colors.White);
            ellipse.Stroke = new SolidColorBrush(Colors.Black);

            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(ellipse);
            IEffectsInfo effectsInfo = appearanceAdapter.DefaultEffectsInfo;

            Assert.IsNull(effectsInfo);
        }

        [Test]
        public void DefaultEffectsInfoIsNullForPolylineWhenTargetIsCE()
        {
            System.Windows.Shapes.Polyline polyline = new System.Windows.Shapes.Polyline();
            polyline.Stroke = new SolidColorBrush(Colors.Black);

            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(polyline);
            IEffectsInfo effectsInfo = appearanceAdapter.DefaultEffectsInfo;

            Assert.IsNull(effectsInfo);
        }

        [Test]
        public void GetFillInfoListForMeter()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Meter);
            List<IFillInfo> fillInfoList = appearanceAdapter.FillInfoList;

            Assert.IsNotNull(fillInfoList);
            Assert.AreEqual(2, fillInfoList.Count);
        }

        [Test]
        public void GetFillInfoListForRectangle()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            List<IFillInfo> fillInfoList = appearanceAdapter.FillInfoList;

            Assert.IsNotNull(fillInfoList);
            Assert.AreEqual(0, fillInfoList.Count); // No extra fill infos except for Default should exist
        }

        [Test]
        public void RevertFillInfo()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;
            fillInfo.Brush = Brushes.PeachPuff;

            appearanceAdapter.DefaultFillInfo = fillInfo;
            BrushValidator.AssertBrushesAreEqual(Brushes.PeachPuff, m_Rectangle.Fill);

            appearanceAdapter.RevertAppearance();
            BrushValidator.AssertBrushesAreEqual(Brushes.White, m_Rectangle.Fill);
        }

        [Test]
        public void RevertStrokeInfo()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IStrokeInfo strokeInfo = appearanceAdapter.DefaultStrokeInfo;
            strokeInfo.Brush = Brushes.PeachPuff;
            strokeInfo.Thickness = 10;

            appearanceAdapter.DefaultStrokeInfo = strokeInfo;
            BrushValidator.AssertBrushesAreEqual(Brushes.PeachPuff, m_Rectangle.Stroke);
            Assert.AreEqual(10, m_Rectangle.StrokeThickness);

            appearanceAdapter.RevertAppearance();
            BrushValidator.AssertBrushesAreEqual(Brushes.Black, m_Rectangle.Stroke);
            Assert.AreEqual(3, m_Rectangle.StrokeThickness);
        }

        [Test]
        public void RevertFontInfo()
        {
            Button button = new Button();
            FontInfoTest.SetFont(button, Brushes.White, "Times New Roman", 10, false, false, false);

            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(button);
            IFontInfo fontInfo = appearanceAdapter.DefaultFontInfo;
            FontInfoTest.SetFont(fontInfo, Brushes.PeachPuff, "SegoeUI", 16, true, true, true);

            appearanceAdapter.DefaultFontInfo = fontInfo;
            FontInfoTest.AssertFontsAreEqual(button, Brushes.PeachPuff, "SegoeUI", 16, true, true, true);

            appearanceAdapter.RevertAppearance();
            FontInfoTest.AssertFontsAreEqual(button, Brushes.White, "Times New Roman", 10, false, false, false);
        }

        [Test]
        public void RevertEffectsInfo()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IEffectsInfo effectsInfo = appearanceAdapter.DefaultEffectsInfo;
            effectsInfo.BevelStyle = BevelStyle.Raised;
            effectsInfo.BevelWidth = 10;

            appearanceAdapter.DefaultEffectsInfo = effectsInfo;
            BevelEffectInfo bevelEffectInfo = m_Rectangle.BevelEffectInfo;
            Assert.AreEqual(BevelStyle.Raised, bevelEffectInfo.Style);
            Assert.AreEqual(10, bevelEffectInfo.Width);

            appearanceAdapter.RevertAppearance();
            Assert.IsNull(m_Rectangle.BitmapEffect);
        }

        [Test]
        public void RevertHasNoEffectAfterCommit()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IStrokeInfo strokeInfo = appearanceAdapter.DefaultStrokeInfo;

            strokeInfo.Brush = Brushes.CadetBlue;
            appearanceAdapter.DefaultStrokeInfo = strokeInfo;

            appearanceAdapter.CommitAppearance();

            IStrokeInfo defaultStrokeInfo = appearanceAdapter.DefaultStrokeInfo;
            BrushValidator.AssertBrushesAreEqual(strokeInfo.Brush, defaultStrokeInfo.Brush);
            Assert.AreEqual(strokeInfo.Thickness, defaultStrokeInfo.Thickness);

            appearanceAdapter.RevertAppearance();

            defaultStrokeInfo = appearanceAdapter.DefaultStrokeInfo;
            BrushValidator.AssertBrushesAreEqual(strokeInfo.Brush, defaultStrokeInfo.Brush);
            Assert.AreEqual(strokeInfo.Thickness, defaultStrokeInfo.Thickness);
        }

        [Test]
        public void SetLinearGradient()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;

            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Colors.Gold, Colors.Violet, 0);
            fillInfo.Brush = linearGradientBrush.Clone();

            appearanceAdapter.DefaultFillInfo = fillInfo;
            appearanceAdapter.CommitAppearance();

            Assert.AreEqual(linearGradientBrush.GetType(), appearanceAdapter.DefaultFillInfo.Brush.GetType());
            Assert.AreEqual(linearGradientBrush.GradientStops[0].Color, ((LinearGradientBrush)appearanceAdapter.DefaultFillInfo.Brush).GradientStops[0].Color);
            Assert.AreEqual(linearGradientBrush.GradientStops[1].Color, ((LinearGradientBrush)appearanceAdapter.DefaultFillInfo.Brush).GradientStops[1].Color);
        }

        [Test]
        public void SetMultipleGradient()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo fillInfo = appearanceAdapter.DefaultFillInfo;

            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(new GradientStop(Colors.Violet, 0));
            gradientStops.Add(new GradientStop(Colors.Violet, 1));
            gradientStops.Add(new GradientStop(Colors.Blue, 0.5));
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStops, 90);
            fillInfo.Brush = linearGradientBrush;

            appearanceAdapter.DefaultFillInfo = fillInfo;
            appearanceAdapter.CommitAppearance();

            Assert.AreEqual(linearGradientBrush.GetType(), appearanceAdapter.DefaultFillInfo.Brush.GetType());
            Assert.AreEqual(linearGradientBrush.GradientStops[0].Color, ((LinearGradientBrush)appearanceAdapter.DefaultFillInfo.Brush).GradientStops[0].Color);
            Assert.AreEqual(linearGradientBrush.GradientStops[1].Color, ((LinearGradientBrush)appearanceAdapter.DefaultFillInfo.Brush).GradientStops[1].Color);
            Assert.AreEqual(linearGradientBrush.GradientStops[2].Color, ((LinearGradientBrush)appearanceAdapter.DefaultFillInfo.Brush).GradientStops[2].Color);
        }

        #region Null values

        [Test]
        public void GetFillInfoForNullFill()
        {
            m_Rectangle.Fill = null;
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo defaultFill = appearanceAdapter.DefaultFillInfo;
            Assert.IsNotNull(defaultFill);

            Assert.IsNull(defaultFill.Brush);
        }

        [Test]
        public void SetFillInfoForNullFill()
        {
            m_Rectangle.Fill = null;
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo defaultFill = appearanceAdapter.DefaultFillInfo;

            defaultFill.Brush = Brushes.Black;

            appearanceAdapter.DefaultFillInfo = defaultFill;

            Assert.IsNotNull(m_Rectangle.Fill);
            BrushValidator.AssertBrushesAreEqual(Brushes.Black, m_Rectangle.Fill);
        }

        [Test]
        public void SetNullFill()
        {
            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(m_Rectangle);
            IFillInfo defaultFill = appearanceAdapter.DefaultFillInfo;

            defaultFill.Brush = null;
            appearanceAdapter.DefaultFillInfo = defaultFill;

            Assert.IsNull(m_Rectangle.Fill);
        }

        #endregion
    }
}
