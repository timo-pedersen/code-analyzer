using System.Xml.Linq;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    public class ScreenImageSizeConverterTest : XamlConverterBaseTest
    {
        private ScreenImageSizeConverter m_Converter;
        private FileHelper m_FileHelper;
        private readonly string m_ScreenName = "Screen1";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_FileHelper = Substitute.For<FileHelper>();
            m_Converter = new ScreenImageSizeConverter {FileHelper = m_FileHelper};

            m_XmlConverterManager.RegisterConverter(m_Converter);
        }

        [Test]
        public void ConvertsDeletesTheBackgroundAndForgroundImagesIfTheyExistsOnDisk()
        {
            string expectedPath = "\\";
            string expectedThumbnailPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailFileFormat, m_ScreenName);
            string expectedBackgroundPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailBackgroundFileFormat, m_ScreenName);

            m_FileHelper.Exists(expectedThumbnailPath).Returns(true);
            m_FileHelper.Exists(expectedBackgroundPath).Returns(true);

            m_XmlConverterManager.ConvertProject(string.Empty, XDocument.Parse(FileResources.ProjectWithScreen));

            m_FileHelper.Received().Delete(expectedThumbnailPath);
            m_FileHelper.Received().Delete(expectedBackgroundPath);
        }

        [Test]
        public void ConvertsDontTryToDeleteTheBackgroundAndForgroundImagesIfTheyDoesntExistsOnDisk()
        {
            string expectedPath = "\\";
            string expectedThumbnailPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailFileFormat, m_ScreenName);
            string expectedBackgroundPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailBackgroundFileFormat, m_ScreenName);

            m_FileHelper.Exists(expectedThumbnailPath).Returns(false);
            m_FileHelper.Exists(expectedBackgroundPath).Returns(false);

            m_XmlConverterManager.ConvertProject(string.Empty, XDocument.Parse(FileResources.ProjectWithScreen));

            m_FileHelper.DidNotReceive().Delete(expectedThumbnailPath);
            m_FileHelper.DidNotReceive().Delete(expectedBackgroundPath);
        }
    }
}
