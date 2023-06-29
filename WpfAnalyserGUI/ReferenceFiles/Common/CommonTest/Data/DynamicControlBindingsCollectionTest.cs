using System;
using System.ComponentModel;
using System.Windows.Forms;
using Core.Api.Tools;
using Core.Controls.Api.Bindings.PropertyBinders;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.PropertyChangeHelpers;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture(typeof(DynamicControlBindingsCollection), typeof(EmptyValueConverter))]
    [TestFixture(typeof(DynamicControlBindingsCollection), typeof(object))]
    [TestFixture(typeof(ControlBindingsCollection), typeof(EmptyValueConverter))]
    [TestFixture(typeof(ControlBindingsCollection), typeof(object))]
    public class DynamicControlBindingsCollectionTest<TBindingsCollection, TValueConverter>
        where TBindingsCollection : ControlBindingsCollection
    {
        private const string TestText = "TestText";
        private const double TestValue = 3.14;
        private const bool DynamicBindingFormattingEnabled = true;

        private BindingContainerStub m_BindingContainer;
        private DataItemProxyStub m_DataSource;
        private IValueConverterCF m_ValueConverter;

        [SetUp]
        public void Setup()
        {
            var toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [SetUp]
        public void TestSetup()
        {
            m_ValueConverter = Activator.CreateInstance(typeof(TValueConverter)) as IValueConverterCF;

            m_BindingContainer = new BindingContainerStub();
            //m_BindingContainer.Text = "";
            //m_BindingContainer.Value = "";

            m_DataSource = new DataItemProxyStub();
            //m_DataSource.Text = "";
            //m_DataSource.Value = "";
        }

        [Test]
        public void CanAddNormalBinding()
        {
            m_BindingContainer.DataBindings.Add(new Binding("Value", m_DataSource, "Value", true, DataSourceUpdateMode.OnPropertyChanged, null));
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));
        }

        [Test]
        public void CanAddDynamicBinding()
        {
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Text", m_DataSource, "Text", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));
        }

        [Test]
        public void NormalBindingTransfersTextToBindingContainerOnBind()
        {
            m_DataSource.Text = TestText;
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));

            Assert.AreEqual(TestText, m_BindingContainer.Text);
        }

        [Test]
        public void NormalBindingTransfersTextToBindingContainer()
        {
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));

            m_DataSource.Text = TestText;
            Assert.AreEqual(TestText, m_BindingContainer.Text);
        }

        [Test]
        public void NormalBindingDoNotTransfersTextToDataSourceOnBind()
        {
            m_BindingContainer.Text = TestText;
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));

            Assert.AreNotEqual(TestText, m_DataSource.Text);
        }

        [Test]
        public void NormalBindingTransfersTextToDataSource()
        {
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));

            m_BindingContainer.Text = TestText;
            Assert.AreEqual(TestText, m_DataSource.Text);
        }

        [Test]
        public void NormalBindingTransfersNullValueToDataSource()
        {
            m_DataSource.Value = TestText;
            m_BindingContainer.Value = TestText;
            m_BindingContainer.DataBindings.Add(new Binding("Value", m_DataSource, "Value", true, DataSourceUpdateMode.OnPropertyChanged, null) { DataSourceNullValue = null });

            Assert.IsNotNull(m_DataSource.Value);

            m_BindingContainer.Value = null;
            Assert.IsNull(m_DataSource.Value);
        }

        [Test]
        public void NormalBindingTransfersDbNullValueToBindingContainerWhenObjectProperty()
        {
            m_DataSource.Value = TestText;
            m_BindingContainer.Value = TestText;
            m_BindingContainer.DataBindings.Add(new Binding("Value", m_DataSource, "Value", true, DataSourceUpdateMode.OnPropertyChanged, null));

            Assert.IsNotNull(m_BindingContainer.Value);

            m_DataSource.Value = null;
            Assert.AreSame(DBNull.Value, m_BindingContainer.Value);
        }

        [Test]
        public void NormalBindingTransfersEmptyStringValueToBindingContainerWhenStringProperty()
        {
            m_DataSource.Text = TestText;
            m_BindingContainer.Text = TestText;
            m_BindingContainer.DataBindings.Add(new Binding("Text", m_DataSource, "Text", true, DataSourceUpdateMode.OnPropertyChanged, null));

            Assert.IsNotNull(m_BindingContainer.Text);

            m_DataSource.Text = null;
            Assert.AreSame(string.Empty, m_BindingContainer.Text);
        }



        [Test]
        public void DynamicBindingTransfersTextToBindingContainerOnBind()
        {
            m_DataSource.Text = TestText;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Text", m_DataSource, "Text", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.AreEqual(TestText, m_BindingContainer.Text);
        }

        [Test]
        public void DynamicBindingTransfersValueToBindingContainerOnBind()
        {
            m_DataSource.Value = TestValue;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.AreEqual(TestValue, m_BindingContainer.Value);
        }

        [Test]
        public void DynamicBindingDoesNotTransfersValueToBindingContainerOnBindInDesignTime()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.RemoveService<IToolManager>();
            var toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(false);

            m_BindingContainer.DesignTime = true;

            m_DataSource.Value = TestValue;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.AreNotEqual(TestValue, m_BindingContainer.Value);
            Assert.IsNull(m_BindingContainer.Value);

            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void DynamicBindingTransfersTextToBindingContainer()
        {
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Text", m_DataSource, "Text", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_DataSource.Text = TestText;
            Assert.AreEqual(TestText, m_BindingContainer.Text);
        }

        [Test]
        public void DynamicBindingTransfersValueToBindingContainer()
        {
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_DataSource.Value = TestValue;
            Assert.AreEqual(TestValue, m_BindingContainer.Value);
        }

        [Test]
        public void DynamicBindingDoNotTransfersTextToDataSourceOnBind()
        {
            m_BindingContainer.Text = TestText;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Text", m_DataSource, "Text", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.AreNotEqual(TestText, m_DataSource.Text);
        }

        [Test]
        public void DynamicBindingDoNotTransfersValueToDataSourceOnBind()
        {
            m_BindingContainer.Value = TestValue;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.AreNotEqual(TestValue, m_DataSource.Value);
        }

        [Test]
        public void DynamicBindingTransfersTextToDataSource()
        {
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Text", m_DataSource, "Text", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_BindingContainer.Text = TestText;
            Assert.AreEqual(TestText, m_DataSource.Text);
        }

        [Test]
        public void DynamicBindingTransfersValueToDataSource()
        {
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_BindingContainer.Value = TestValue;
            Assert.AreEqual(TestValue, m_DataSource.Value);
        }

        [Test]
        public void DynamicBindingTransfersNullValueToDataSource()
        {
            m_DataSource.Value = TestText;
            m_BindingContainer.Value = TestText;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            Assert.IsNotNull(m_DataSource.Value);

            m_BindingContainer.Value = null;
            Assert.IsNull(m_DataSource.Value);
        }

        [Test]
        public void DynamicBindingOnlyTransfersValueToDataSourceWhenBoundPropertyChanged()
        {
            m_DataSource.Value = null;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_BindingContainer.Text = TestText;
            Assert.IsNull(m_DataSource.Value);
        }

        [Test]
        public void DynamicBindingOnlyTransfersValueToBindingContainerWhenBoundPropertyChanged()
        {
            m_DataSource.Value = TestValue;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.OnPropertyChanged, m_ValueConverter));

            m_BindingContainer.SetValueWithoutFiringChanged(null);
            Assert.IsNotNull(m_DataSource.Value);
            Assert.IsNull(m_BindingContainer.Value);
            m_DataSource.Text = TestText;
            Assert.IsNull(m_BindingContainer.Value);
        }

        [TestCase(typeof(object))]
        [TestCase(typeof(DuplicatorValueConverter))]
        public void DynamicBindingDoesNotTransferValueToDataSourceWhenOnewayBinding(Type valueConverterType)
        {
            const double InitialValue = 10;
            IValueConverterCF converter = Activator.CreateInstance(valueConverterType) as IValueConverterCF;
            if (converter == null)
            {
                converter = m_ValueConverter;
            }

            m_BindingContainer.Value = TestValue;
            m_DataSource.Value = InitialValue;
            m_BindingContainer.DataBindings.Add(new DynamicBinding("Value", m_DataSource, "Value", DynamicBindingFormattingEnabled, DataSourceUpdateMode.Never, converter));

            Assert.That(m_DataSource.Value, Is.EqualTo(InitialValue));

            m_BindingContainer.Value = 2 * TestValue;
            Assert.That(m_DataSource.Value, Is.EqualTo(InitialValue));

        }

        #region Stub classes
        public class BindingContainerStub : IBindableComponent, INotifyPropertyChanged
        {
            private readonly ControlBindingsCollection m_BindingsCollection;
            private readonly BindingContext m_BindingContext;
            private string m_Text;
            private object m_Value;
            private readonly NotifyPropertyChangedHelperCF m_ChangedHelper;
            private ISite m_Site;

            public BindingContainerStub()
            {
                m_BindingsCollection = (ControlBindingsCollection)Activator.CreateInstance(typeof(TBindingsCollection), (IBindableComponent)this);
                m_BindingsCollection.CollectionChanged += OnBindingsCollectionChanged;
                m_ChangedHelper = new NotifyPropertyChangedHelperCF(this);
                m_BindingContext = new BindingContext();
            }

            private void OnBindingsCollectionChanged(object sender, CollectionChangeEventArgs e)
            {
                if (e.Action == CollectionChangeAction.Add)
                {
                    UpdateBindings();
                }
            }

            private void UpdateBindings()
            {
                for (int i = 0; i < this.DataBindings.Count; i++)
                {
                    BindingContext.UpdateBinding(m_BindingContext, this.DataBindings[i]);
                }
            }

            public string Text
            {
                get { return m_Text; }
                set
                {
                    m_Text = value;
                    m_ChangedHelper.FirePropertyChanged("Text");
                }
            }

            public object Value
            {
                get { return m_Value; }
                set
                {
                    m_Value = value;
                    m_ChangedHelper.FirePropertyChanged("Value");
                }
            }

            public ControlBindingsCollection DataBindings
            {
                get
                {
                    return m_BindingsCollection;
                }
            }

            public void SetValueWithoutFiringChanged(object value)
            {
                m_Value = value;
            }

            public bool DesignTime
            {
                get { return (m_Site != null); }
                set
                {
                    if (value)
                    {
                        m_Site = new LightSite(this, null);
                    }
                    else
                    {
                        m_Site = null;
                    }
                }
            }


            #region INotifyPropertyChanged Members

            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
            {
                add { m_ChangedHelper.PropertyChanged += value; }
                remove { m_ChangedHelper.PropertyChanged -= value; }
            }

            #endregion

            #region IBindableComponent Members

            BindingContext IBindableComponent.BindingContext
            {
                get
                {
                    return m_BindingContext;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            ControlBindingsCollection IBindableComponent.DataBindings
            {
                get { return m_BindingsCollection; }
            }

            #endregion

            #region IComponent Members

            event EventHandler IComponent.Disposed
            {
                add { throw new NotImplementedException(); }
                remove { throw new NotImplementedException(); }
            }

            ISite IComponent.Site
            {
                get
                {
                    return m_Site;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class DataItemProxyStub : NotifyPropertyChangedCF, IDataItemProxy
        {
            private object m_Value;
            private string m_Text;

            public string Text
            {
                get { return m_Text; }
                set
                {
                    m_Text = value;
                    FirePropertyChanged("Text");
                }
            }

            public object Value
            {
                get { return m_Value; }
                set
                {
                    m_Value = value;
                    FirePropertyChanged("Value");
                }
            }

            #region IDataItemProxy Members

            string IDataItemProxy.FullName
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            object IDataItemProxy.Value
            {
                get { return Value; }
                set { Value = value; }
            }

            VariantValue[] IDataItemProxy.Values
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            void IDataItemProxy.Connect(bool addConnectedClients)
            {
                throw new NotImplementedException();
            }

            void IDataItemProxy.Disconnect()
            {
                throw new NotImplementedException();
            }

            BEDATATYPE IDataItemProxy.DataType
            {
                get { throw new NotImplementedException(); }
            }

            public Type Type
            {
                get { throw new NotImplementedException(); }
            }

            IDataItemProxySource IDataItemProxy.DataItem
            {
                get { return null; }
            }

            #endregion
        }

        #endregion
    }

    public class EmptyValueConverter : IValueConverterCF
    {

        object IValueConverterCF.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class DuplicatorValueConverter : IValueConverterCF
    {
        object IValueConverterCF.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                return (string)value + (string)value;
            }
            else if (value is double)
            {
                return (double)value * 2;
            }
            else if (value is int)
            {
                return (int)value * 2;
            }
            throw new ArgumentOutOfRangeException("Invalid type");
        }
    }

}
