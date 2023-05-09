using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class MultiLanguageServiceCFTest
    {
        public const string DesignerName = "Screen1";
        public const string ElementName = "Button1";
        public const string PropertyName = "Text";
        public const string ReferenceValue = "homer";
        public const string ReferenceValueWithNl = "homer\nsimpson";
        public const string ReferenceValueWithCrAndNl = "homer\r\nsimpson";

        private IMultiLanguageServiceCF m_IMultiLanguageServiceCF;
        private MultiLanguageService m_MultiLanguageService;
        private IMultiLanguageServer m_MultiLanguageServer;
        private ResourceItemList<DesignerResourceItem, IDesignerResourceItem> m_ResourceItems;
        private IExtendedBindingList<ILanguageInfo> m_Languages;

       
        [SetUp]
        public void SetUp()
        {
            m_IMultiLanguageServiceCF = m_MultiLanguageService = new MultiLanguageService();
            TestHelper.AddService(typeof(IMultiLanguageServiceCF), m_MultiLanguageService);

            m_MultiLanguageServer = MockRepository.GenerateStub<IMultiLanguageServer>();
            m_ResourceItems = new ResourceItemList<DesignerResourceItem, IDesignerResourceItem>();
            m_MultiLanguageServer.Expect(srv => srv.ResourceItems).Return(m_ResourceItems).Repeat.Any();
            m_Languages = m_IMultiLanguageServiceCF.CreateLanguageList();
            m_MultiLanguageServer.Expect(srv => srv.Languages).Return(m_Languages).Repeat.Any();
            m_IMultiLanguageServiceCF.MultiLanguageServer = m_MultiLanguageServer;
        }

      
        [Test]
        public void AddResourceItemAddsResourceItemForNewElements()
        {
            m_IMultiLanguageServiceCF.AddResourceItem(DesignerName, ElementName, PropertyName, ReferenceValue);

            Assert.AreEqual(1, m_ResourceItems.Count);
            AssertResourceItemEqual(m_ResourceItems[0], DesignerName, ElementName, PropertyName, ReferenceValue);
        }

        [Test]
        public void AddResourceItemAddsCarriageReturnToNewLine()
        {
            m_IMultiLanguageServiceCF.AddResourceItem(DesignerName, ElementName, PropertyName, ReferenceValueWithNl);

            Assert.AreEqual(1, m_ResourceItems.Count);
            AssertResourceItemEqual(m_ResourceItems[0], DesignerName, ElementName, PropertyName, ReferenceValueWithCrAndNl);
        }

        [Test]
        public void AddResourceItemAddsNoCarriageReturnToCarriageReturnAndNewLine()
        {
            m_IMultiLanguageServiceCF.AddResourceItem(DesignerName, ElementName, PropertyName, ReferenceValueWithCrAndNl);

            Assert.AreEqual(1, m_ResourceItems.Count);
            AssertResourceItemEqual(m_ResourceItems[0], DesignerName, ElementName, PropertyName, ReferenceValueWithCrAndNl);
        }

        [Test]
        public void AddResourceItemThrowsWhenElementFullNameIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => m_IMultiLanguageServiceCF.AddResourceItem(string.Empty, string.Empty, PropertyName, ReferenceValue));
        }

        [Test]
        public void AddResourceItemThrowsWhenElementFullNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => m_IMultiLanguageServiceCF.AddResourceItem(null, null, PropertyName, ReferenceValue));
        }

        [Test]
        public void AddResourceItemThrowsWhenElementFullNameIsIncomplete()
        {
            Assert.Throws<ArgumentNullException>(() => m_IMultiLanguageServiceCF.AddResourceItem(DesignerName, string.Empty, PropertyName, ReferenceValue));
        }

        [Test]
        public void AddResourceItemDoesNotChangeResourceItemWhenElementExists()
        {
            AddNewMockResourceItem(DesignerName, ElementName, PropertyName, "El Homero");

            m_IMultiLanguageServiceCF.AddResourceItem(DesignerName, ElementName, PropertyName, ReferenceValue);

            Assert.AreEqual(1, m_ResourceItems.Count);
            AssertResourceItemEqual(m_ResourceItems[0], DesignerName, ElementName, PropertyName, "El Homero");
        }

        private void AddNewMockResourceItem(string designerName, string objectName, string propertyName, string referenceValue)
        {
            m_ResourceItems.Add(new DesignerResourceItem
                                    {
                                        DesignerName = designerName,
                                        ObjectName = objectName,
                                        PropertyName = propertyName,
                                        ReferenceValue = referenceValue
                                    });
        }

        [Test]
        public void RemoveResourceItemRemovesResourceItemWhenItExists()
        {
            AddNewMockResourceItem(DesignerName, ElementName, PropertyName, "El Homero");

            m_IMultiLanguageServiceCF.RemoveResourceItem(DesignerName, ElementName, PropertyName);

            Assert.AreEqual(0, m_ResourceItems.Count);
        }

        [Test]
        public void RemoveResourceItemDoesNothingWhenItemDoesNotExist()
        {
            m_IMultiLanguageServiceCF.RemoveResourceItem(DesignerName, ElementName, PropertyName);
        }

        [Test]
        public void RemoveAllResourceItemsRemovesAllResourceItemsForASpecificObject()
        {
            AddNewMockResourceItem(DesignerName, ElementName, PropertyName, "El Homero");
            AddNewMockResourceItem(DesignerName, ElementName, "Offspring", "El Barto");
            AddNewMockResourceItem(DesignerName, "OtherElement", "SomeProperty", "SomeValue");

            m_IMultiLanguageServiceCF.RemoveAllResourceItems(DesignerName, ElementName);

            Assert.AreEqual(1, m_ResourceItems.Count);
        }

        private void AssertResourceItemEqual(IDesignerResourceItem resourceItem, string designerName, string elementName, string propertyName, string referenceValue)
        {
            Assert.AreEqual(designerName, resourceItem.DesignerName);
            Assert.AreEqual(elementName, resourceItem.ObjectName);
            Assert.AreEqual(propertyName, resourceItem.PropertyName);
            Assert.AreEqual(referenceValue, resourceItem.ReferenceValue);
        }
    }
}
