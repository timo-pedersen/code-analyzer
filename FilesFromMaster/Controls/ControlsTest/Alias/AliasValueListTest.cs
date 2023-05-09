using System;
using System.ComponentModel;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Alias
{
    [TestFixture]
    public class AliasValueListTest
    {
        private AliasValueList m_AliasInstanceList;

        [SetUp]
        public void SetUp()
        {
            m_AliasInstanceList = new AliasValueList();
        }

       

        [Test]
        public void UnableToAddValueWithNameIfSuchAlreadyExists()
        {
            string name = "I'm not unique";
            AliasValue first = new AliasValue();
            AliasValue second = new AliasValue();
            first.Name = name;
            second.Name = name;

            m_AliasInstanceList.Add(first);
            m_AliasInstanceList.Add(second);

            Assert.IsFalse(m_AliasInstanceList.Contains(second));
            Assert.IsTrue(m_AliasInstanceList.Contains(first));
        }

        [Test]
        public void UnableToAddValueWithNameAsNullIfSuchAlreadyExists()
        {
            AliasValue first = new AliasValue();
            AliasValue second = new AliasValue();
            

            m_AliasInstanceList.Add(first);
            m_AliasInstanceList.Add(second);

            Assert.IsFalse(m_AliasInstanceList.Contains(second));
            Assert.IsTrue(m_AliasInstanceList.Contains(first));
        }

        [Test]
        public void AddingValueRaisesPropertyChangedEvent()
        {
            AliasValue aliasValue = new AliasValue();
            bool propertyChangedRaised = false;
            var v = m_AliasInstanceList as INotifyPropertyChanged;
            v.PropertyChanged += (sender, e) => propertyChangedRaised = true;

            m_AliasInstanceList.Add(aliasValue);

            Assert.IsTrue(propertyChangedRaised);
        }

        [Test]
        public void RemovingValueRaisesPropertyChangedEvent()
        {
            AliasValue aliasValue = new AliasValue();
            m_AliasInstanceList.Add(aliasValue);

            bool propertyChangedRaised = false;
            var v = m_AliasInstanceList as INotifyPropertyChanged;
            v.PropertyChanged += (sender, e) => propertyChangedRaised = true;

            m_AliasInstanceList.Remove(aliasValue);

            Assert.IsTrue(propertyChangedRaised);
        }

        [Test]
        public void BubblesPropertyChangedEventOnValuePropertyChanged()
        {
            bool propertyChangedRaised = CheckPropertyChangedRaised(action => action.Value = "Some Value");

            Assert.IsTrue(propertyChangedRaised);
        }

        [Test]
        public void BubblesPropertyChangedEventOnDefaultValuePropertyChanged()
        {
            bool propertyChangedRaised = CheckPropertyChangedRaised(action => action.DefaultValue = "Some Value");

            Assert.IsTrue(propertyChangedRaised);
        }

        [Test]
        public void BubblesPropertyChangedEventOnIsDefaultPropertyChanged()
        {
            bool propertyChangedRaised = CheckPropertyChangedRaised(action => action.IsDefault = !action.IsDefault);

            Assert.IsTrue(propertyChangedRaised);
        }



        private bool CheckPropertyChangedRaised(Action<AliasValue> whateverCausingPropertyChangedToFire)
        {
            AliasValue aliasValue = new AliasValue();
            m_AliasInstanceList.Add(aliasValue);

            bool propertyChangedRaised = false;
            var v = m_AliasInstanceList as INotifyPropertyChanged;
            v.PropertyChanged += (sender, e) => propertyChangedRaised = true;

            whateverCausingPropertyChangedToFire(aliasValue);

            return propertyChangedRaised;
        }


    }
}
