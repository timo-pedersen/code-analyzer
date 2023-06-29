using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.Serialization.Converters
{
    public class XamlConverterBaseTest
    {
        protected XmlConverterManager m_XmlConverterManager;
        
        [SetUp]
        public virtual void SetUp()
        {
            var brandService = TestHelper.AddServiceStub<IBrandServiceIde>();
            brandService.ScreenFileExtension.Returns(".neoxaml");

            m_XmlConverterManager = new XmlConverterManager();
            m_XmlConverterManager.Converters.Clear();
        }

        [TearDown]
        public virtual void TearDown()
        {
            TestHelper.RemoveService<IBrandServiceIde>();
        }
    }
}