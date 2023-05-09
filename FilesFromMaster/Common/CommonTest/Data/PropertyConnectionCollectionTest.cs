using System.Windows.Forms;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture]
    public class PropertyConnectionCollectionTest
    {
        private PropertyConnectionCollection m_PropertyConnectionCollection;
        private IPropertyConnection m_PropertyConnectionStub;
        private IBindableComponent m_BindableComponentStub;

        [SetUp]
        public void Setup()
        {
            m_BindableComponentStub = MockRepository.GenerateStub<IBindableComponent>();
            m_PropertyConnectionCollection = new PropertyConnectionCollection(m_BindableComponentStub);
            m_PropertyConnectionStub = MockRepository.GenerateStub<IPropertyConnection>();
            m_PropertyConnectionStub.Stub(x => x.Key).Return("Key");
        }

        [Test]
        public void AddingItemSetsUpConnection()
        {
            m_PropertyConnectionCollection.Add(m_PropertyConnectionStub);
            m_PropertyConnectionStub.AssertWasCalled(d => d.SetupConnection(m_BindableComponentStub));
        }

        [Test]
        public void ClearingItemRemovesConnection()
        {
            m_PropertyConnectionCollection.Add(m_PropertyConnectionStub);
            m_PropertyConnectionCollection.Clear();
            m_PropertyConnectionStub.AssertWasCalled(d => d.TearDownConnection());
        }

        [Test]
        public void RemovingItemRemovesConnection()
        {
            m_PropertyConnectionCollection.Add(m_PropertyConnectionStub);
            m_PropertyConnectionCollection.Remove(m_PropertyConnectionStub);
            m_PropertyConnectionStub.AssertWasCalled(d => d.TearDownConnection());
        }

        [Test]
        public void AddingEqualItemsDoesNotAffectCollection()
        {
            var propertyConnectionCollection = new PropertyConnectionCollection(new BindableComponentFake());
            IDataItemProxy dataItemProxy = new DataItemProxyFake { FullName = StringConstants.TagsRoot + "Tag1" };
            propertyConnectionCollection.Add(new PropertyConnection("ConnectedProperty", dataItemProxy));
            propertyConnectionCollection.Add(new PropertyConnection("ConnectedProperty", dataItemProxy));
            Assert.AreEqual(propertyConnectionCollection.Count, 1);
        }
    }
}
