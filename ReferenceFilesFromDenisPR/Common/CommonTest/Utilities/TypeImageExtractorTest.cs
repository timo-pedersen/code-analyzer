using System;
using System.IO;
using Neo.ApplicationFramework.Controls.Symbol;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Utilities
{
    [TestFixture]
    public class TypeImageExtractorTest
    {
        private TypeImageExtractor m_TypeImageExtractor;

        [SetUp]
        public void SetUp()
        {
            m_TypeImageExtractor = new TypeImageExtractor();
        }

        [Test]
        public void GetImageForMultiPicture()
        {
            IControlMetadata controlMetadataStub = Substitute.For<IControlMetadata>();
            controlMetadataStub.ImageName.Returns("AnalogNumeric");
            controlMetadataStub.DisplayName.Returns("Analog Numeric");

            IControlInfo controlInfoStub = Substitute.For<IControlInfo>();
            controlInfoStub.Metadata.Returns(controlMetadataStub);
            controlInfoStub.Type.Returns(typeof(Symbol));
            
            System.Drawing.Bitmap bitmap = m_TypeImageExtractor.GetBitmap(controlInfoStub);

            Assert.That(bitmap, Is.Not.Null);
        }

        [Test]
        public void GetImageForTypeInWpfToolkit()
        {
            Type type = typeof(Microsoft.Windows.Controls.DataGrid);

            Stream stream = m_TypeImageExtractor.GetStreamForWpfType(type, 16, 16);

            Assert.That(stream, Is.Not.Null);
        }
    }
}
