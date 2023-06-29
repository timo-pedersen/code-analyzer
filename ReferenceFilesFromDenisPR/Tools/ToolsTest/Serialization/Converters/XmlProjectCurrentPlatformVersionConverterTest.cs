using System.Xml.Linq;
using System.Xml.XPath;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    public class XmlProjectCurrentPlatformVersionConverterTest
    {
        private const string TargetPlatformVersionName = "CurrentTargetPlatformVersion";
        private FileHelper m_FileHelper;

        [SetUp]
        public void Setup()
        {
            m_FileHelper = Substitute.For<FileHelper>();
        }

        [Test]
        public void AddsProjectCurrentPlatformVersionIfNotExisting()
        {
            // Arrange
            m_FileHelper.Exists(Arg.Any<string>()).Returns(false);
            XDocument doc = XDocument.Parse(FileResources.ProjectWithoutCurrentPlatformVersion);
            var converter = new TestConverter(m_FileHelper);

            // Act
            var result = converter.Convert(doc);
            var objectElement = doc.XPathSelectElement(string.Format("/{0}/{1}", SerializerConstants.RootElementName, SerializerConstants.ObjectElementName));
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(objectElement.Attribute(TargetPlatformVersionName));
        }
    }

    class TestConverter : XmlProjectCurrentPlatformVersionConverter
    {
        public TestConverter(FileHelper fileHelper)
        {
            FileHelper = fileHelper;
        }

        public bool Convert(XDocument doc)
        {
            return ConvertProject("", doc);
        }
    }
}
