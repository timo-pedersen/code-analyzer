using System.Collections.Generic;
using System.Windows;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.InformationDesignerImport.ConverterManager;
using Neo.ApplicationFramework.Tools.InformationDesignerImport.ConverterManager.Converters;
using Neo.ApplicationFramework.Tools.InformationDesignerImport.ConverterManager.Converters.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.InformationDesignerImport.Converters
{
    [TestFixture]
    public class AnalogDynamicsPropertyConverterTest
    {
        private IInformationDesignerImportSettingsService m_InformationDesignerImportSettingsService;
        private AnalogDynamicsPropertyConverterFakeClass m_AnalogDynamicsPropertyConverter;
        private IInformationDesignerImportService m_InformationDesignerImportService;
        private ILazy<IConverterManager> m_ConverterManager;
        private ILazy<IConverterApiService> m_ConverterApiService;
        
        private readonly string InformationDesignerBackgroundColor = "BGCOL";
        private readonly string InformationDesignerForegroundColor = "FGCOL";

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            m_InformationDesignerImportService = Substitute.For<IInformationDesignerImportService>();
            m_InformationDesignerImportSettingsService = Substitute.For<IInformationDesignerImportSettingsService>();
            m_ConverterManager = Substitute.For<ILazy<IConverterManager>>();
            m_ConverterApiService = Substitute.For<ILazy<IConverterApiService>>();

            var lazyInformationDesignerImportSettingsService = Substitute.For<ILazy<IInformationDesignerImportSettingsService>>();
            lazyInformationDesignerImportSettingsService.Value.Returns(m_InformationDesignerImportSettingsService);
            m_AnalogDynamicsPropertyConverter = new AnalogDynamicsPropertyConverterFakeClass(m_InformationDesignerImportService, lazyInformationDesignerImportSettingsService, m_ConverterManager, m_ConverterApiService);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetDefaultColorMapForBGCOLWhenFeatureIsActivatedButNoColorMapFileExists()
        {
            // ARRANGE
            m_InformationDesignerImportSettingsService.ColorConversion = true;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>());

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerBackgroundColor);

            // ASSERT
            Assert.That(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(m_AnalogDynamicsPropertyConverter.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void GetCustomColorMapForBGCOLWhenFeatureIsActivatedAndAColorMapFileExists()
        {
            // ARRANGE
            var customColorMap = new Dictionary<int, int>()
            {
                {12500, 12500},
                {25000, 25000},
                {5000, 5000}
            };

            m_InformationDesignerImportSettingsService.ColorConversion = true;
            m_InformationDesignerImportService.CustomColorMap.Returns(customColorMap);
            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerBackgroundColor);

            // ASSERT
            Assert.IsFalse(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(m_AnalogDynamicsPropertyConverter.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void ConversionForBGCOLWhenNoFeatureIsActivatedAndNoColorMapFileExists()
        {
            // ARRANGE
            m_InformationDesignerImportSettingsService.ColorConversion = false;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>());

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerBackgroundColor);

            // ASSERT
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(ColorMapHelper.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void ConversionForBGCOLWhenNoFeatureIsActivatedAColorMapFileExists()
        {
            // ARRANGE
            var customColorMap = new Dictionary<int, int>()
            {
                {12500, 12500},
                {25000,25000},
                {5000, 5000}
            };

            m_InformationDesignerImportSettingsService.ColorConversion = false;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>(customColorMap));

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerBackgroundColor);

            // ASSERT
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(ColorMapHelper.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void GetDefaultColorMapForFGCOLWhenFeatureIsActivatedButNoColorMapFileExists()
        {
            // ARRANGE
            m_InformationDesignerImportSettingsService.ColorConversion = true;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>());

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerForegroundColor);

            // ASSERT
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(m_AnalogDynamicsPropertyConverter.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void GetCustomColorMapForFGCOLWhenFeatureIsActivatedAndAColorMapFileExists()
        {
            // ARRANGE
            var customColorMap = new Dictionary<int, int>
            {
                {12500, 12500},
                {25000, 25000},
                {5000, 5000}
            };

            m_InformationDesignerImportSettingsService.ColorConversion = true;
            m_InformationDesignerImportService.CustomColorMap.Returns(customColorMap);

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerForegroundColor);

            // ASSERT
            Assert.IsFalse(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(m_AnalogDynamicsPropertyConverter.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void ConversionForFGCOLWhenNoFeatureIsActivatedAndNoColorMapFileExists()
        {
            // ARRANGE
            m_InformationDesignerImportSettingsService.ColorConversion = false;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>());

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerForegroundColor);

            // ASSERT
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(ColorMapHelper.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        [Test]
        public void ConversionForFGCOLWhenNoFeatureIsActivatedAColorMapFileExists()
        {
            // ARRANGE
            var customColorMap = new Dictionary<int, int>
            {
                {12500, 12500},
                {25000, 25000},
                {5000, 5000}
            };

            m_InformationDesignerImportSettingsService.ColorConversion = false;
            m_InformationDesignerImportService.CustomColorMap.Returns(new Dictionary<int, int>(customColorMap));

            // ACT
            m_AnalogDynamicsPropertyConverter.SetPropertyOrLogConversionSkipped(InformationDesignerForegroundColor);

            // ASSERT
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMap.Equals(ColorMapHelper.DefaultColorMap));
            Assert.IsTrue(m_AnalogDynamicsPropertyConverter.ColorMapDynamicsIsApplied);
        }

        internal sealed class AnalogDynamicsPropertyConverterFakeClass : AnalogDynamicsPropertyConverter
        {
            public AnalogDynamicsPropertyConverterFakeClass(IInformationDesignerImportService informationDesignerImportService, 
                ILazy<IInformationDesignerImportSettingsService> informationDesignerImportSettingsService,
                ILazy<IConverterManager> converterManger, 
                ILazy<IConverterApiService> converterApiService)
                : base(informationDesignerImportService, converterManger, converterApiService)
            {
                ColorMapDynamicsIsApplied = false;
                ColorMap = null;
                ParentConverter = new GraphicsControlConverter { ParentConverter = new GraphicsControlConverter() };
                ConvertedControl = new FrameworkElement();
                InformationDesignerImportSettingsService = informationDesignerImportSettingsService;
            }

            protected override IDynamicsBinder SetupBindingAndConverterAndIntervalMapper(
                DependencyProperty dependencyProperty,
                out BrushDynamicsConverter brushDynamicsConverter,
                out BrushCFIntervalMapper brushCFIntervalMapper)
            {
                brushDynamicsConverter = new BrushDynamicsConverter();
                brushCFIntervalMapper = new BrushCFIntervalMapper();
                return null;
            }

            protected override void DoApplyColorMapDynamics(IDictionary<int, int> colorMap, BrushCFIntervalMapper brushCFIntervalMapper, BrushDynamicsConverter brushDynamicsConverter, IDynamicsBinder brushDynamicsBinder)
            {
                ColorMap = colorMap;
                ColorMapDynamicsIsApplied = true;
            }

            public IDictionary<int, int> ColorMap { get; private set; }

            public bool ColorMapDynamicsIsApplied { get; set; }

            public IDictionary<int, int> DefaultColorMap { get { return ColorMapHelper.DefaultColorMap; } }
        }
    }
}