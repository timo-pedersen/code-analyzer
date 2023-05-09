using System;
using System.IO;
using Neo.ApplicationFramework.Controls.Symbol;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            IControlMetadata controlMetadataStub = MockRepository.GenerateStub<IControlMetadata>();
            controlMetadataStub.Stub(x => x.ImageName).Return("AnalogNumeric");
            controlMetadataStub.Stub(x => x.DisplayName).Return("Analog Numeric");

            IControlInfo controlInfoStub = MockRepository.GenerateStub<IControlInfo>();
            controlInfoStub.Stub(x => x.Metadata).Return(controlMetadataStub);
            controlInfoStub.Stub(x => x.Type).Return(typeof(Symbol));
            
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
