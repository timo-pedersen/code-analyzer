using System;
using System.Xml.Linq;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    public class XamlCircularMeterRemoveMinMaxDynamicsConverterTest
    {
        private XmlConverterManager m_XmlConverterManager;
        private IXmlConverter m_Converter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestHelper.AddServiceStub<IBrandServiceIde>();

            m_XmlConverterManager = new XmlConverterManager();
            m_XmlConverterManager.Converters.Clear();

            var objectSerializerFactory = Substitute.For<IObjectSerializerFactoryIde>();
            objectSerializerFactory.GetXDocumentLoader().Load(Arg.Any<string>())
                .Returns(x => GetProjectDocument((string)x[0]));

            var directoryHelper = Substitute.ForPartsOf<DirectoryHelper>();
            directoryHelper.GetFiles(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => GetProjectFile((string)x[0], (string)x[1]));

            m_Converter = new XamlCircularMeterRemoveMinMaxDynamicsConverter(objectSerializerFactory.ToILazy(), directoryHelper);
            m_XmlConverterManager.RegisterConverter(m_Converter);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestHelper.RemoveService<IBrandServiceIde>();
        }

        [Test]
        public void ConvertXamlForPCProject_CalledWithNoCircularMeterDynamicsAtAll_ReturnsFalse()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.PCProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithOneCircularMeterAndNoDynamicsAtAll));

            // ASSERT
            Assert.AreEqual(false, isConverted);
        }

        [Test]
        public void ConvertXamlForPCProject_CalledWithCircularMeterMinMaxDynamics_ReturnsFalse()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.PCProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithTwoCircularMeterAndWithMinMaxDynamics));

            // ASSERT
            Assert.AreEqual(false, isConverted);
        }

        [Test]
        public void ConvertXamlForPCProject_CalledWithOnlyCircularMeterGeneralDynamics_ReturnsFalse()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.PCProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithOneCircularMeterAndOnlyGeneralDynamics));

            // ASSERT
            Assert.AreEqual(false, isConverted);
        }

        [Test]
        public void ConvertXamlForCEProject_CalledWithNoCircularMeterDynamicsAtAll_ReturnsFalse()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.CEProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithOneCircularMeterAndNoDynamicsAtAll));

            // ASSERT
            Assert.AreEqual(false, isConverted);
        }

        [Test]
        public void ConvertXamlForCEProject_CalledWithCircularMeterMinMaxDynamics_ReturnsTrue()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.CEProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithTwoCircularMeterAndWithMinMaxDynamics));

            // ASSERT
            Assert.AreEqual(true, isConverted);
        }

        [Test]
        public void ConvertXamlForCEProject_CalledWithOnlyCircularMeterGeneralDynamics_ReturnsFalse()
        {
            // ARRANGE
            m_Converter.ConvertTerminal(nameof(FileResources.CEProject));

            // ACT
            bool isConverted = m_Converter.ConvertXaml(XDocument.Parse(FileResources.ScreenWithOneCircularMeterAndOnlyGeneralDynamics));

            // ASSERT
            Assert.AreEqual(false, isConverted);
        }

        private string[] GetProjectFile(string projectNameAsPath, string extension)
        {
            return new[]
            {
                projectNameAsPath
            };
        }

        private XDocument GetProjectDocument(string resourceNameAsFilePath)
        {
            return XDocument.Parse(resourceNameAsFilePath == nameof(FileResources.PCProject) ? FileResources.PCProject : FileResources.CEProject);
        }
    }
}
