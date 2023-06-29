#if !VNEXT_TARGET
using System.Windows;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class ScreenObjectComponentTest
    {
        private ScreenObjectComponent m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_UnderTest = new ScreenObjectComponent("TestComponent.lib");
        }

        /// <summary>
        /// Calling LoadThumbnailAsync should return an image
        /// </summary>
        [Test]
        public void LoadThumbnailAsync_should_return_an_image()
        {
            var image = m_UnderTest.LoadThumbnailAsync().Result;
            Assert.That(image, Is.Not.Null);
        }

        /// <summary>
        /// Calling CopyToClipboard should add data to the clipboard
        /// </summary>
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CopyToClipboard_should_add_data_to_clipboard()
        {
            Clipboard.Clear();

            m_UnderTest.CopyToClipboard();

            Assert.That(Clipboard.ContainsData(ScreenDataObject.ClipboardFormat));
        }

        /// <summary>
        /// Calling CreateDataObject should return an IDataObject with FileDrop format
        /// </summary>
        [Test]
        public void CreateDataObject_should_return_ScreenDataObject()
        {
            var result = m_UnderTest.CreateDataObject();

            Assert.That(result, Is.TypeOf<ScreenDataObject>());
        }
    }
}
#endif
