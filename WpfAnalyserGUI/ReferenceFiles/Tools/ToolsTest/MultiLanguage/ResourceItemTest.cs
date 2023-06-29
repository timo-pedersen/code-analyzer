using System.Collections.Generic;
using System.ComponentModel;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MultiLanguage
{
    [TestFixture]
    public class ResourceItemTest
    {
        IMultiLanguageServiceCF m_MultiLanguageService;

        private const string CurrentValueProperty = "CurrentValue";
        private const string ALanguage = "Swedish";

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            m_MultiLanguageService = TestHelper.AddServiceStub<IMultiLanguageServiceCF>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CurrentValueSetter_sets_ReferenceValue_when_it_is_the_selected_language_CurrentLanguage_is_emptypstring()
        {
            m_MultiLanguageService.CurrentLanguage = string.Empty;

            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;
            item.CurrentValue = "newValue";

            Assert.AreEqual("newValue", item.ReferenceValue);
        }

        [Test]
        public void CurrentValueSetter_sets_ReferenceValue_when_it_is_the_selected_language_CurrentLanguage_is_null()
        {
            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            item.CurrentValue = "newValue";

            Assert.AreEqual("newValue", item.ReferenceValue);
        }

        [Test]
        public void CurrentValueSetter_fires_PropertyChanged_on_ReferenceValue()
        {
            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.CurrentValue = "newValue";

            Assert.That(propertiesChangedNotified.Contains("ReferenceValue"));
        }

        [Test]
        public void CurrentValueSetter_fires_PropertyChanged_once_when_CurrentLanguage_is_default()
        {
            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.CurrentValue = "newValue";

            Assert.That(propertiesChangedNotified.Contains(CurrentValueProperty));
            propertiesChangedNotified.Remove(CurrentValueProperty);
            Assert.IsFalse(propertiesChangedNotified.Contains(CurrentValueProperty));
        }

        [Test]
        public void CurrentValueSetter_sets_selected_language_value()
        {
            m_MultiLanguageService.CurrentLanguage = ALanguage;

            IDesignerResourceItem item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            item.CurrentValue = "newValue";

            Assert.That(item.LanguageValues.ContainsKey(ALanguage));
            Assert.AreEqual("newValue", item.LanguageValues[ALanguage]);
        }

        [Test]
        public void CurrentValueSetter_fires_PropertyChanged_on_SelectedLanguage()
        {
            m_MultiLanguageService.CurrentLanguage = ALanguage;

            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.CurrentValue = "newValue";

            Assert.That(propertiesChangedNotified.Contains(ALanguage));
        }

        [Test]
        public void CurrentValueSetter_fires_no_event_when_value_is_set_to_same_and_language_is_default()
        {
            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;
            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            string newValue = "newValue";
            item.CurrentValue = newValue;

            propertiesChangedNotified.Clear();

            item.CurrentValue = string.Copy(newValue);

            Assert.AreEqual(0, propertiesChangedNotified.Count);
        }

        [Test]
        public void CurrentValueSetter_fires_no_event_when_value_is_set_to_same_and_language_is_NOT_default()
        {
            m_MultiLanguageService.CurrentLanguage = ALanguage;

            var item = new DesignerResourceItem();
            item.ReferenceValue = string.Empty;

            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            string newValue = "newValue";
            item.CurrentValue = newValue;

            propertiesChangedNotified.Clear();

            item.CurrentValue = string.Copy(newValue);

            Assert.AreEqual(0, propertiesChangedNotified.Count);
        }

        [Test]
        public void CurrentValueGetter_returns_ReferenceValue_when_CurrentLanguage_is_default()
        {
            IDesignerResourceItem item = new DesignerResourceItem();

            item.LanguageValues["lang"] = "langvalue";
            item.ReferenceValue = "refvalue";

            Assert.AreEqual("refvalue", item.CurrentValue);

            m_MultiLanguageService.CurrentLanguage = "lang";

            Assert.AreEqual("langvalue", item.CurrentValue);
        }

        [Test]
        public void When_ReferenceValue_and_current_language_is_default_then_CurrentValue_should_change()
        {
            IDesignerResourceItem item = new DesignerResourceItem();
            item.ReferenceValue = "newValue";

            Assert.AreEqual("newValue", item.CurrentValue);
        }

        [Test]
        public void ReferenceValueSetter_fires_PropertyChanged_on_CurrentValue_after_ReferenceValuePropertyChanged_was_fired()
        {
            var item = new DesignerResourceItem();
            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.ReferenceValue = "newValue";

            Assert.AreEqual(2, propertiesChangedNotified.Count);
            Assert.That(propertiesChangedNotified[1] == "CurrentValue");
        }

        [Test]
        public void SetValue_sets_value()
        {
            IDesignerResourceItem item = new DesignerResourceItem();

            item.SetValue(ALanguage, "newValue");

            Assert.AreEqual("newValue", item.LanguageValues[ALanguage]);
        }

        [Test]
        public void SetValue_fires_PropertyChanged_on_the_property_name()
        {
            IDesignerResourceItem item = new DesignerResourceItem();
            INotifyPropertyChanged propertyChanged = item;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.SetValue(ALanguage, "newValue");

            Assert.That(propertiesChangedNotified.Contains(ALanguage));
        }

        [Test]
        public void SetValue_sets_CurrentValue_when_the_language_is_selected()
        {
            IDesignerResourceItem item = new DesignerResourceItem();
            m_MultiLanguageService.CurrentLanguage = ALanguage;

            item.SetValue(ALanguage, "newValue");

            Assert.AreEqual("newValue", item.CurrentValue);
        }

        [Test]
        public void SetValue_fires_PropertyChanged_on_CurrentValue_when_language_is_the_current_one()
        {
            IDesignerResourceItem item = new DesignerResourceItem();
            INotifyPropertyChanged propertyChanged = item;

            m_MultiLanguageService.CurrentLanguage = ALanguage;

            List<string> propertiesChangedNotified = new List<string>();
            propertyChanged.PropertyChanged += (obj, args) => propertiesChangedNotified.Add(args.PropertyName);

            item.SetValue(ALanguage, "newValue");

            Assert.That(propertiesChangedNotified.Contains("CurrentValue"));
        }

        [Test]
        public void Update_fires_CurrentValue_PropertyChanged()
        {
            IDesignerResourceItem resourceItem = new DesignerResourceItem();
            resourceItem.ReferenceValue = string.Empty;
            INotifyPropertyChanged notifyPropertyChanged = resourceItem;

            bool wasRaised = false;
            notifyPropertyChanged.PropertyChanged += (sender, args) => wasRaised = true;
            resourceItem.CurrentValue = "newValue";
            resourceItem.Update();

            Assert.IsTrue(wasRaised);
        }

        [Test]
        public void CurrentValueSetsDynamicStringOnAllLanguages()
        {
            //SETUP
            IDesignerResourceItem resourceItem = new DesignerResourceItem();
            resourceItem.ReferenceValue = string.Empty;
            var languageInfo = new LanguageInfo(ALanguage, "US");
            IExtendedBindingList<ILanguageInfo> languages = new ExtendedBindingList<ILanguageInfo>();
            languages.Add(languageInfo);
            m_MultiLanguageService.Languages.Returns(languages);

            //TEST
            resourceItem.CurrentValue = "{0} Hello {1}";

            //ASSERT
            Assert.AreEqual(resourceItem.LanguageValues[ALanguage], "*NEEDS TRANSLATION* {0} {1}");
        }

        [Test]
        public void CurrentValueSetsDynamicStringOnDefaultLanguage()
        {
            //SETUP
            IDesignerResourceItem resourceItem = new DesignerResourceItem();
            var languageInfo = new LanguageInfo(ALanguage, "US");
            IExtendedBindingList<ILanguageInfo> languages = new ExtendedBindingList<ILanguageInfo>();
            languages.Add(languageInfo);
            m_MultiLanguageService.Languages.Returns(languages);
            m_MultiLanguageService.CurrentLanguage = ALanguage;
            resourceItem.ReferenceValue = "oldValue";

            //TEST
            resourceItem.CurrentValue = "{0} Hello {1}";

            //ASSERT
            Assert.AreEqual(resourceItem.ReferenceValue, "*NEEDS TRANSLATION* {0} {1}");
        }
    }
}
