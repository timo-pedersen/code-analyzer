using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.NeoNativeSignature;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Font;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.FontUsageManager
{
    [TestFixture]
    public class FontServiceTest
    {
        private IFontService m_FontService;
        private FontManager m_FontManager;
        private CachedFontManager m_CachedFontManager;
        private ElementCanvas m_Canvas;

        [SetUp]
        public void TestSetup()
        {
            var toolManagerMock = Substitute.For<IToolManager>();
            toolManagerMock.Runtime.Returns(false);
            TestHelper.AddService<IToolManager>(toolManagerMock);
        
            m_FontManager = Substitute.For<FontManager>();
            m_CachedFontManager = new CachedFontManager(m_FontManager);
            m_FontService = new FontService(m_CachedFontManager);

            m_Canvas = new ElementCanvas();
            System.ComponentModel.Design.ServiceContainer localServiceContainer = new System.ComponentModel.Design.ServiceContainer(ServiceContainerCF.Instance as IServiceProvider);
            m_Canvas.ServiceProvider = localServiceContainer;
            localServiceContainer.AddService(typeof(IAppearanceAdapterService), new AppearanceAdapterService());
        }

        [Test]
        public void GetFontFileHitsCacheOnSameFontNameButNotSize()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Arial", 14);

            SetupRecordAndPlayback(1, 1);
        }

        [Test]
        public void GetFontFileDoesntHitCacheOnDifferentFontName()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Courier", 12);

            SetupRecordAndPlayback(2, 2);
        }

        [Test]
        public void GetFontFileDoesntHitCacheOnDifferentBoldValue()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Arial", 12, true, false, false);

            SetupRecordAndPlayback(2, 2);
        }

        [Test]
        public void GetFontFileDoesntHitCacheOnDifferentItalicValue()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Arial", 12, false, true, false);

            SetupRecordAndPlayback(2, 2);
        }

        [Test]
        public void GetFontFileDoesntHitCacheOnDifferentUnderlineValue()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Arial", 12, false, false, true);

            SetupRecordAndPlayback(2, 2);
        }

        [Test]
        public void GetFontFileDoesntHitCacheOnDifferentBoldItalicUnderlineValue()
        {
            AddLabelControl("Arial", 12);
            AddLabelControl("Arial", 12, true, true, true);

            SetupRecordAndPlayback(2, 2);
        }

        private void SetupRecordAndPlayback(int expectedNumberOfFontFileLookups, int expectedNumberOfUniqueFontFiles)
        {
            ((IFontManager)m_FontManager).GetFontFile(Arg.Any<FontManagerInfo>())
                .Returns(x => GetFontFileCallback((IFontManagerInfo)x[0]));

            IList<string> foundFontFiles = m_FontService.GetFontFiles(m_Canvas.Children);

            Assert.AreEqual(foundFontFiles.Count, expectedNumberOfUniqueFontFiles);
            ((IFontManager)m_FontManager).ReceivedWithAnyArgs(expectedNumberOfFontFileLookups).GetFontFile(Arg.Any<FontManagerInfo>());
        }

        private string GetFontFileCallback(IFontManagerInfo fontInfo)
        {
            return
                string.Format("Font: {0}, Bold: {1}, Italic: {2}, Underline: {3}"
                                , fontInfo.Font.Source, fontInfo.Bold
                                , fontInfo.Italic, fontInfo.Underline);
        }

        private void AddLabelControl(string fontName, double fontSize)
        {
            AddLabelControl(fontName, fontSize, false, false, false);
        }

        private void AddLabelControl(string fontName, double fontSize, bool bold, bool italic, bool underline)
        {
            Label label = new Label();
            label.FontFamily = new FontFamily(fontName);
            label.FontSize = fontSize;
            if (bold)
            {
                label.FontWeight = FontWeights.Bold;
            }
            if (italic)
            {
                label.FontStyle = FontStyles.Italic;
            }
            if (underline)
            {
                label.TextDecorations.Add(TextDecorations.Underline);
            }
            label.Width = 500;
            label.Height = 100;
            m_Canvas.Children.Add(label);
        }
    }
}
