using System;
using System.Linq;
using System.Xml.Linq;
using Core.Api.Feature;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Brand;
using Neo.ApplicationFramework.Tools.Serialization.Converters.Samples;
using Neo.ApplicationFramework.Tools.Storage.Features;
using NUnit.Framework;
using Rhino.Mocks;
using Neo.ApplicationFramework.Tools.Serialization.Conversion;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    [TestFixture]
    class SetSqLiteStorageProviderConverterTest
    {
        private const string SqLitePlatformName = "SQLite Database";
        private const string SqlCePlatformName = "SQL CE Database";

        private XDocument m_SqlCeProjectDocument;
        private XDocument m_SqLiteProjectDocument;
        private IFeatureSecurityServiceIde m_FeatureSecurityServiceIde;
        private IXmlConverter m_SetSqLiteStorageProviderConverter;

        [SetUp]
        public void SetUp()
        {
            m_SqlCeProjectDocument = XDocument.Parse(FileResources.ProjectWithSqlCeIn220);
            m_SqLiteProjectDocument = XDocument.Parse(FileResources.ProjectWithSqLiteIn220);

            TestHelper.ClearServices();
            m_FeatureSecurityServiceIde = TestHelper.CreateAndAddServiceStub<IFeatureSecurityServiceIde>();

            m_SetSqLiteStorageProviderConverter = new SetSqLiteStorageProviderConverter(m_FeatureSecurityServiceIde.ToILazy());
        }

        [TestCase(true, BrandToolHelper.PanelBrandId8, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId8, true)]
        [TestCase(true, BrandToolHelper.PanelBrandId5, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId5, true)]
        [TestCase(true, BrandToolHelper.PanelBrandId4, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId4, true)]
        [TestCase(true, BrandToolHelper.PanelBrandId9, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId9, true)]
        [TestCase(true, BrandToolHelper.PanelBrandId11, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId11, false)]   //Caterpillar is the only brand which should not be converted at the moment
        [TestCase(true, 0, false)]
        [TestCase(false, 0, true)]
        [Test]
        public void ConvertSqlCeProjectForBrand(bool enableStorageProviderSelectionFeature, int panelBrandId, bool expectedResult)
        {
            //ARRANGE
            m_FeatureSecurityServiceIde.Stub(x => x.IsActivated<EnableStorageProviderSelectionFeature>()).Return(enableStorageProviderSelectionFeature);
            BrandToolHelper.Instance = new BrandToolHelper(panelBrandId);

            //ACT
            bool isConverted = m_SetSqLiteStorageProviderConverter.ConvertProject(string.Empty, m_SqlCeProjectDocument);
            string newStorageProviderName = GetStorageProviderName(m_SqlCeProjectDocument);

            //ASSERT
            Assert.That(isConverted, Is.EqualTo(expectedResult));
            Assert.That(newStorageProviderName, Is.EqualTo(expectedResult ? SqLitePlatformName : SqlCePlatformName));
        }

        [TestCase(true, BrandToolHelper.PanelBrandId8, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId8, false)]
        [TestCase(true, BrandToolHelper.PanelBrandId5, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId5, false)]
        [TestCase(true, BrandToolHelper.PanelBrandId4, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId4, false)]
        [TestCase(true, BrandToolHelper.PanelBrandId9, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId9, false)]
        [TestCase(true, BrandToolHelper.PanelBrandId11, false)]
        [TestCase(false, BrandToolHelper.PanelBrandId11, false)]
        [TestCase(true, 0, false)]
        [TestCase(false, 0, false)]
        [Test]
        public void ConvertSqLiteProjectForBrand(bool enableStorageProviderSelectionFeature, int panelBrandId, bool expectedResult)
        {
            //ARRANGE
            m_FeatureSecurityServiceIde.Stub(x => x.IsActivated<EnableStorageProviderSelectionFeature>()).Return(enableStorageProviderSelectionFeature);
            BrandToolHelper.Instance = new BrandToolHelper(panelBrandId);

            //ACT
            bool isConverted = m_SetSqLiteStorageProviderConverter.ConvertProject(string.Empty, m_SqLiteProjectDocument);
            string newStorageProviderName = GetStorageProviderName(m_SqLiteProjectDocument);

            //ASSERT
            Assert.That(isConverted, Is.EqualTo(expectedResult));
            Assert.That(newStorageProviderName, Is.EqualTo(SqLitePlatformName));
        }

        private string GetStorageProviderName(XDocument projectDocument)
        {
            string StorageProviderSettingsElementName = "StorageProviderSettings";
            string DatabasePlatformDisplayName = "DisplayName";
            XElement storageProviderSettingsElement = projectDocument.Descendants(StorageProviderSettingsElementName).FirstOrDefault();

            XElement objectElement = storageProviderSettingsElement?.Descendants(XmlConverterManager.ObjectNodeName).FirstOrDefault();

            XAttribute displayName = objectElement?.Attributes(DatabasePlatformDisplayName).FirstOrDefault();
            if (displayName == null)
                return string.Empty;
            return displayName.Value;
        }
    }
}
