using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Keyboard
{
    [TestFixture]
    public class KeyboardLayoutInfoTest
    {
        private string m_LayoutFilesFolderPath;
        private readonly Guid EnglishUSDefaultSipClassId = new Guid("42429667-ae04-11d0-a4f8-00aa00a749b9");

        [SetUp]
        public void Setup()
        {
            string testDirectory = TestHelper.CurrentDirectory;
#if DEBUG
            const string relativeToLayoutFileFolder = @"..\..\..\..\Output\Debug";
#else
            const string relativeToLayoutFileFolder = @"..\..\..\..\Output\Release";
#endif

#if !VNEXT_TARGET
            string currentDirectory = Path.Combine(testDirectory, relativeToLayoutFileFolder);
#else
            string currentDirectory = Path.Combine(testDirectory, @"..\..\", relativeToLayoutFileFolder);
#endif
            m_LayoutFilesFolderPath = Path.Combine(currentDirectory, ApplicationConstants.KeyboardLayoutsFolder);
            Assert.IsTrue(Directory.Exists(m_LayoutFilesFolderPath));

#if VNEXT_TARGET
            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());
#else
            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelperCF());
#endif
        }

        [Test]
        public void CanParseOnScreenKeyboardFile()
        {
            string swedishLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Swedish.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(swedishLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.NeoOnScreenKeyboard, info.KeyboardHost);
            Assert.AreEqual(EnglishUSDefaultSipClassId, info.SipClassId);
            Assert.AreEqual("Swedish", info.Name);
        }

        [Test]
        [TestCase("nonexistingfile", Description = "Nonexisting file")]
        [TestCase("app.config", Description = "Existing file, but not keyboardlayout file.")]
        public void ReturnsNullOnMissingFileAndInvalidFile(string fileName)
        {
            string filePath = Path.Combine(TestHelper.CurrentDirectory, fileName);
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(filePath);

            Assert.IsNull(info);
        }

        [Test]
        public void CanParseSystemDefaultLayoutFile()
        {
            string systemDefaultLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "System Default.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(systemDefaultLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(EnglishUSDefaultSipClassId, info.SipClassId);
            Assert.AreEqual("System Default", info.Name);
            Assert.AreEqual("00000409", info.Hkl);
            Assert.AreEqual(false, info.HasIme);
        }

        [Test]
        public void CanParseSimplifiedChineseLayoutFile()
        {
            string simplifiedChineseLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Simplified Chinese.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(simplifiedChineseLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(new Guid("28bd0fff-23e2-4976-a685-c419cb8011dc"), info.SipClassId);
            Assert.AreEqual("Simplified Chinese", info.Name);
            Assert.AreEqual("E0010804", info.Hkl);
            Assert.AreEqual(true, info.HasIme);
        }

        [Test]
        public void CanParseTraditionalChineseLayoutFile()
        {
            string traditionalChineseLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Traditional Chinese.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(traditionalChineseLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(new Guid("FB300D3D-890F-4249-8D7D-5A29D3E1369C"), info.SipClassId);
            Assert.AreEqual("Traditional Chinese", info.Name);
            Assert.AreEqual("E0010404", info.Hkl);
            Assert.AreEqual(true, info.HasIme);
            Assert.AreEqual(true, info.HasPasswordApi);
            Assert.IsTrue(info.DependentFiles.Contains("IQQI_UI_API_WCE.dll"));

            //When iq-t have fixed their keyboards to use system font instead of Arial this test should be removed (see support case #17307)
            Assert.GreaterOrEqual(info.FontLinks.Count, 2);
            Assert.AreEqual(1, info.FontLinks.Count(fontlink => fontlink.StartsWith("Arial")));
        }

        [Test]
        public void CanParseKoreanLayoutFile()
        {
            string koreanLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Korean.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(koreanLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(new Guid("73001917-361f-48b9-959e-9390204c5c62"), info.SipClassId);
            Assert.AreEqual("Korean", info.Name);
            Assert.AreEqual("E0010412", info.Hkl);
            Assert.AreEqual(true, info.HasIme);
            Assert.AreEqual(true, info.HasPasswordApi);
            Assert.IsTrue(info.DependentFiles.Contains("IQQI_UI_API_WCE.dll"));

            //When iq-t have fixed their keyboards to use system font instead of Arial this test should be removed (see support case #17307)
            Assert.GreaterOrEqual(info.FontLinks.Count, 2);
            Assert.AreEqual(1, info.FontLinks.Count(fontlink => fontlink.StartsWith("Arial")));
        }

        [Test]
        public void CanParseArabicLayoutFile()
        {
            string arabicLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Arabic.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(arabicLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(new Guid("096CA6E1-214A-4653-9B53-3821D809D887"), info.SipClassId);
            Assert.AreEqual("Arabic", info.Name);
            Assert.AreEqual("00000401", info.Hkl);
            Assert.AreEqual(true, info.HasIme);
            Assert.AreEqual(true, info.HasPasswordApi);
            Assert.IsTrue(info.DependentFiles.Contains("IQQI_UI_API_WCE.dll"));
        }

        [Test]
        public void CanParseHebrewLayoutFile()
        {
            string hebrewLayoutFilePath = Path.Combine(m_LayoutFilesFolderPath, "Hebrew.kbl");
            IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(hebrewLayoutFilePath);

            Assert.IsNotNull(info);
            Assert.AreEqual(KeyboardHostType.SoftwareInputPanel, info.KeyboardHost);
            Assert.AreEqual(new Guid("ECB0E61E-3A79-4969-B230-A3F489F93287"), info.SipClassId);
            Assert.AreEqual("Hebrew", info.Name);
            Assert.AreEqual("0000040D", info.Hkl);
            Assert.AreEqual(true, info.HasIme);
            Assert.AreEqual(true, info.HasPasswordApi);
            Assert.IsTrue(info.DependentFiles.Contains("IQQI_UI_API_WCE.dll"));
        }

        [Test]
        public void VerifyAtLeastOneSystemDefaultLayoutFileExists()
        {
            string[] files = Directory.GetFiles(m_LayoutFilesFolderPath, "*.kbl");
            IList<IKeyboardLayoutInfo> layoutInfos = new List<IKeyboardLayoutInfo>();

            foreach (string fileName in files)
            {
                IKeyboardLayoutInfo info = KeyboardLayoutInfo.ParseFile(fileName);
                Assert.IsNotNull(info);
                layoutInfos.Add(info);
            }

            Assert.AreEqual(true, layoutInfos.Any(x => x.IsSystemDefault));
        }
    }
}
