using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Serialization.Conversion;
using Neo.ApplicationFramework.Tools.Serialization.Conversion.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    /// <summary>
    /// Base class initializing helper classes & services required to test converters.
    /// </summary>
    public class XamlConverterBaseTest
    {
        protected XmlConverterManager m_XmlConverterManager;
        internal IDesignerConversionService m_DesignerConversionService;
        protected XamlConversionService m_XamlConversionService;
        internal ProjectConversionService m_ProjectConversionService;
        internal IDocumentIoHelper m_DocumentIoHelper;
        internal IBrandServiceIde m_BrandServiceIde;
        private INeoDocumentConversionHelper m_NeoDocumentConversionHelper;
        private IProjectConversionReferenceHelper m_ProjectConversionReferenceHelper;

        [SetUp]
        public virtual void SetUp()
        {
            m_BrandServiceIde = Substitute.For<IBrandServiceIde>();
            m_BrandServiceIde.ScreenFileExtension.Returns(".neoxaml");
            var informationProgressService = Substitute.For<IInformationProgressService>();
            m_DocumentIoHelper = Substitute.For<IDocumentIoHelper>();

            m_ProjectConversionReferenceHelper = new ProjectConversionReferenceHelper();
            m_XmlConverterManager = new XmlConverterManager();
            m_NeoDocumentConversionHelper = new NeoDocumentConversionHelper(m_XmlConverterManager);
            m_XamlConversionService = new XamlConversionService(m_XmlConverterManager, m_NeoDocumentConversionHelper);
            m_DesignerConversionService = new DesignerConversionService(m_BrandServiceIde, m_DocumentIoHelper, m_XmlConverterManager, m_NeoDocumentConversionHelper);
            m_ProjectConversionService = new ProjectConversionService(
                informationProgressService,
                m_DocumentIoHelper,
                m_XmlConverterManager,
                m_NeoDocumentConversionHelper,
                m_DesignerConversionService,
                m_ProjectConversionReferenceHelper);

            m_XmlConverterManager.Converters.Clear();
        }

        [TearDown]
        public virtual void TearDown()
        {
            TestHelper.RemoveService<IBrandServiceIde>();
            TestHelper.RemoveService<IProjectManager>();
            TestHelper.RemoveService<IObjectSerializerFactoryIde>();
            TestHelper.RemoveService<IDocumentIoHelper>();
        }
    }
}