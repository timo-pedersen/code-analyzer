using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Controls.WindowsControls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.MultiLanguage
{
    [TestFixture]
    public class MultiTextBindingsAndResourceItemsManagerTest
    {
        private const string ComboBoxName = "ComboBox1";
        private const string FirstTextValue = "Test 1";
        private const string SecondTextValue = "Test 2";
        private const string SecondTextPropertyName = "Texts[1]";
        private const string FirstTextPropertyName = "Texts[0]";

        private MultiTextBindingsAndResourceItemsManager m_MultiTextBindingsAndResourceItemsManager;
        private IMultiLanguageServiceCF m_MultiLanguageServiceStub;
        private ITextIdService m_TextIDServiceStub;
        private IGlobalSelectionService m_GlobalSelectionServiceStub;

        private IDesignerResourceItem m_DesignerResourceItem;
        private ComboBox m_ComboBox;

        [SetUp]
        public void SetUp()
        {
            m_MultiLanguageServiceStub = TestHelper.AddServiceStub<IMultiLanguageServiceCF>();
            m_TextIDServiceStub = TestHelper.AddServiceStub<ITextIdService>();
            m_GlobalSelectionServiceStub = TestHelper.AddServiceStub<IGlobalSelectionService>();

            m_DesignerResourceItem = MockRepository.GenerateStub<IDesignerResourceItem>();
            m_DesignerResourceItem.ObjectName = ComboBoxName;
            m_DesignerResourceItem.PropertyName = FirstTextPropertyName;
            m_DesignerResourceItem.CurrentValue = FirstTextValue;

            m_MultiLanguageServiceStub.Stub(x => x.AddResourceItem(string.Empty, ComboBoxName, FirstTextPropertyName, FirstTextValue)).IgnoreArguments().Return(m_DesignerResourceItem);

            m_ComboBox = new ComboBox();
            m_ComboBox.Name = ComboBoxName;

            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new MultiTextConverter();
            m_ComboBox.SetBinding(ComboBox.TextsProperty, multiBinding);
            
            m_MultiTextBindingsAndResourceItemsManager = new MultiTextBindingsAndResourceItemsManager(m_ComboBox, ComboBox.TextsProperty);

            m_ComboBox.IntervalMapper.AddInterval(0, 0, FirstTextValue);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CreatingAnInstanceInitializesTheTextsPropertyFromComboBox()
        {
            Assert.AreEqual(1, m_MultiTextBindingsAndResourceItemsManager.Texts.Count);
            Assert.AreEqual(FirstTextValue, m_MultiTextBindingsAndResourceItemsManager.Texts[0].ToString());
        }

        [Test]
        public void SettingTextUnhooksPreviousBindingList()
        {
            MockRepository mockRepository = new MockRepository();

            IBindingList bindingListMock = mockRepository.StrictMultiMock<IBindingList>(typeof(IList));
            bindingListMock.Expect(x => x.ListChanged -= null).IgnoreArguments().Repeat.Once();
            bindingListMock.Expect(x => x.ListChanged += null).IgnoreArguments().Repeat.Any();
            bindingListMock.Replay();

            m_MultiTextBindingsAndResourceItemsManager.Texts = bindingListMock;

            m_MultiTextBindingsAndResourceItemsManager.Texts = new BindingList<string>();

            bindingListMock.VerifyAllExpectations();
        }

        [Test]
        public void SettingTextHooksupNewBindingList()
        {
            MockRepository mockRepository = new MockRepository();

            IBindingList bindingListMock = mockRepository.StrictMultiMock<IBindingList>(typeof(IList));
            bindingListMock.Expect(x => x.ListChanged += null).IgnoreArguments().Repeat.Once();
            bindingListMock.Replay();

            m_MultiTextBindingsAndResourceItemsManager.Texts = bindingListMock;

            bindingListMock.VerifyAllExpectations();
        }

        [Test]
        public void DisposingInstanceUnhooksBindingList()
        {
            MockRepository mockRepository = new MockRepository();

            IBindingList bindingListMock = mockRepository.StrictMultiMock<IBindingList>(typeof(IList));
            bindingListMock.Expect(x => x.ListChanged -= null).IgnoreArguments().Repeat.Once();
            bindingListMock.Expect(x => x.ListChanged += null).IgnoreArguments().Repeat.Any();
            bindingListMock.Replay();

            m_MultiTextBindingsAndResourceItemsManager.Texts = bindingListMock;

            ((IDisposable)m_MultiTextBindingsAndResourceItemsManager).Dispose();

            bindingListMock.VerifyAllExpectations();
        }

        [Test]
        public void ChangingTextUpdatesResourceItem()
        {
            m_MultiLanguageServiceStub.Stub(x => x.FindResourceItems(string.Empty, ComboBoxName)).Return(new IDesignerResourceItem[] { m_DesignerResourceItem });
            
            StringInterval stringInterval = (StringInterval)m_MultiTextBindingsAndResourceItemsManager.Texts[0];
            stringInterval.Value = "New Test 1";

            Assert.AreEqual("New Test 1", m_DesignerResourceItem.CurrentValue);
        }

        [Test]
        public void AddingTextCreatesBindingAndResourceItem()
        {
            m_MultiTextBindingsAndResourceItemsManager.Texts.Add(new StringInterval() { Value = SecondTextValue });

            MultiBinding multiBinding = BindingOperations.GetMultiBinding(m_ComboBox, ComboBox.TextsProperty);

            Assert.AreEqual(2, multiBinding.Bindings.Count);
            m_MultiLanguageServiceStub.AssertWasCalled(x => x.AddResourceItem(string.Empty, ComboBoxName, SecondTextPropertyName, SecondTextValue), options => options.Repeat.Once());
        }

        [Test]
        public void RemovingTextRemovesBindingAndResourceItemAndReindexesExistingResourceItems()
        {
            m_MultiTextBindingsAndResourceItemsManager.Texts.Add(new StringInterval() { Value = SecondTextValue });

            IDesignerResourceItem secondDesignerResourceItem = MockRepository.GenerateStub<IDesignerResourceItem>();
            secondDesignerResourceItem.ObjectName = ComboBoxName;
            secondDesignerResourceItem.PropertyName = SecondTextPropertyName;
            secondDesignerResourceItem.CurrentValue = SecondTextValue;

            m_MultiLanguageServiceStub.Stub(x => x.FindResourceItems(string.Empty, ComboBoxName)).Return(new IDesignerResourceItem[] { m_DesignerResourceItem, secondDesignerResourceItem });

            m_MultiTextBindingsAndResourceItemsManager.Texts.RemoveAt(0);

            MultiBinding multiBinding = BindingOperations.GetMultiBinding(m_ComboBox, ComboBox.TextsProperty);

            Assert.AreEqual(1, multiBinding.Bindings.Count);
            Assert.AreEqual(FirstTextPropertyName, secondDesignerResourceItem.PropertyName);
            m_MultiLanguageServiceStub.AssertWasCalled(x => x.RemoveResourceItem(m_DesignerResourceItem), options => options.Repeat.Once());
        }

        [Test]
        public void ClearingTextsRemovesAllBindingsAndResourceItems()
        {
            m_MultiTextBindingsAndResourceItemsManager.Texts.Clear();

            MultiBinding multiBinding = BindingOperations.GetMultiBinding(m_ComboBox, ComboBox.TextsProperty);

            Assert.AreEqual(0, multiBinding.Bindings.Count);
            m_MultiLanguageServiceStub.AssertWasCalled(x => x.RemoveResourceItem(m_DesignerResourceItem), options => options.Repeat.Once());
        }
    }
}
