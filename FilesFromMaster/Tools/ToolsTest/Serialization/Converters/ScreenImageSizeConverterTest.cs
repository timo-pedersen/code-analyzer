using System.Xml.Linq;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    public class ScreenImageSizeConverterTest : XamlConverterBaseTest
    {
        private readonly string m_ScreenName = "Screen1";
        private ScreenImageSizeConverter m_Converter;
        private FileHelper m_FileHelper;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_FileHelper = MockRepository.GenerateStub<FileHelper>();
            m_Converter = new ScreenImageSizeConverter {FileHelper = m_FileHelper};

            m_XmlConverterManager.RegisterConverter(m_Converter);
        }

        [Test]
        public void ConvertsDeletesTheBackgroundAndForgroundImagesIfTheyExistsOnDisk()
        {
            string expectedPath = "\\";
            string expectedThumbnailPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailFileFormat, m_ScreenName);
            string expectedBackgroundPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailBackgroundFileFormat, m_ScreenName);

            m_FileHelper.Stub(x => x.Exists(expectedThumbnailPath)).Return(true);
            m_FileHelper.Stub(x => x.Exists(expectedBackgroundPath)).Return(true);

            m_ProjectConversionService.ConvertProject(string.Empty, XDocument.Parse(FileResources.ProjectWithScreen));

            m_FileHelper.AssertWasCalled(x => x.Delete(expectedThumbnailPath));
            m_FileHelper.AssertWasCalled(x => x.Delete(expectedBackgroundPath));
        }

        [Test]
        public void ConvertsDontTryToDeleteTheBackgroundAndForgroundImagesIfTheyDoesntExistsOnDisk()
        {
            string expectedPath = "\\";
            string expectedThumbnailPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailFileFormat, m_ScreenName);
            string expectedBackgroundPath = expectedPath + string.Format(ProjectItemConstants.ThumbnailBackgroundFileFormat, m_ScreenName);

            m_FileHelper.Stub(x => x.Exists(expectedThumbnailPath)).Return(false);
            m_FileHelper.Stub(x => x.Exists(expectedBackgroundPath)).Return(false);

            m_ProjectConversionService.ConvertProject(string.Empty, XDocument.Parse(FileResources.ProjectWithScreen));

            m_FileHelper.AssertWasNotCalled(x => x.Delete(expectedThumbnailPath));
            m_FileHelper.AssertWasNotCalled(x => x.Delete(expectedBackgroundPath));
        }


    }
}
