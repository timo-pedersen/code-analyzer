#if !VNEXT_TARGET
using System.Windows;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    [TestFixture]
    public class TextComponentTest
    {
        private TextComponent m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_UnderTest = new TextComponent("TestComponent.txt");
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
        /// Calling CopyToClipboard should add the text to the clipboard
        /// </summary>
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CopyToClipboard_should_add_file_to_clipboard()
        {
            Clipboard.Clear();

            m_UnderTest.CopyToClipboard();

            Assert.That(Clipboard.ContainsText());
            Assert.That(Clipboard.GetText(), Is.EquivalentTo("Data for a TextComponent"));
        }

        /// <summary>
        /// Calling CreateDataObject should return an IDataObject with text format
        /// </summary>
        [Test]
        public void CreateDataObject_should_return_IDataObject_with_Text_format()
        {
            var result = m_UnderTest.CreateDataObject() as IDataObject;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetDataPresent(DataFormats.Text));
        }

    }
}
#endif
