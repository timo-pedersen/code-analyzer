using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.Model
{
    /// <summary>
    /// Unit tests on <see cref="ImageComponent"/>
    /// </summary>
    [TestFixture]
    public class ImageComponentTest
    {
        private ImageComponent m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            string fullPath = Path.GetFullPath(@".\TestComponent.jpg");
            m_UnderTest = new ImageComponent(fullPath);
        }

        /// <summary>
        /// Calling LoadThumbnailAsync should return an image
        /// </summary>
        [Test]
        public async Task LoadThumbnailAsync_should_return_an_imageAsync()
        {
            ImageSource image = await m_UnderTest.LoadThumbnailAsync();
            Assert.That(image, Is.Not.Null);
        }

        /// <summary>
        /// Calling CopyToClipboard should add the file to the clipboard
        /// </summary>
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CopyToClipboard_should_add_file_to_clipboard()
        {
            Clipboard.Clear();

            m_UnderTest.CopyToClipboard();

            Assert.That(Clipboard.ContainsFileDropList());
            Assert.That(Clipboard.GetFileDropList(), Is.EquivalentTo(new[] { m_UnderTest.FullFileName }));
        }

        /// <summary>
        /// Calling CreateDataObject should return an IDataObject with FileDrop format
        /// </summary>
        [Test]
        public void CreateDataObject_should_return_IDataObject_with_FileDrop_format()
        {
            var result = m_UnderTest.CreateDataObject() as IDataObject;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetDataPresent(DataFormats.FileDrop));
        }
    }
}
