using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using Core.Api.Tools;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture(DataSourceUpdateMode.OnPropertyChanged, typeof(ControlStubINotifyPropertyChanged))]
    [TestFixture(DataSourceUpdateMode.OnPropertyChanged, typeof(ControlStubIValue))]
    [TestFixture(DataSourceUpdateMode.OnPropertyChanged, typeof(ControlStubSpecificPropertyEvent))]
    [TestFixture(DataSourceUpdateMode.Never, typeof(ControlStub))]
    public class DynamicBindingTest
    {
        private IEventRaiser m_DataItemEventRaiser;
        private IDataItemProxy m_DataItemProxy;
        private ControlStub m_ControlStub;
        private readonly Type m_ControlStubType;
        //private IToolManager m_ToolManager;
        private MockRepository m_Mocker;
        private readonly DataSourceUpdateMode m_DataSourceUpdateMode;

        public DynamicBindingTest(DataSourceUpdateMode dataSourceUpdateMode, Type controlStubType)
        {
            m_DataSourceUpdateMode = dataSourceUpdateMode;
            m_ControlStubType = controlStubType;
        }

        [SetUp]
        public void SetUp()
        {
            //m_ToolManager = MockRepository.GenerateStub<IToolManager>();
            //m_ToolManager.Stub(x => x.RunTime).Return(true);
            //TestHelper.AddService<IToolManager>(m_ToolManager);

            m_Mocker = new MockRepository();

            m_DataItemProxy = m_Mocker.Stub<IDataItemProxy>();
            m_DataItemProxy.Value = "value";
            m_DataItemEventRaiser = m_DataItemProxy.GetEventRaiser(x => x.PropertyChanged += null);

            m_ControlStub = (ControlStub)Activator.CreateInstance(m_ControlStubType);


            var toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(inv => inv.Runtime).Return(true);
        }

        [TearDown]
        public void TearDown()
        {
            m_ControlStub.Dispose();
        }

        [Test]
        public void BindingReactsOnPropertyChangedOnDataSource()
        {
            DynamicBinding dynamicBinding = new DynamicBinding("Value", m_DataItemProxy, "Value", true, m_DataSourceUpdateMode);
            m_ControlStub.DataBindings.Add(dynamicBinding);

            Assert.That(m_ControlStub.Value, Is.EqualTo("value"));

            SetDataItemProxyValue(x => x.Value, "kotte");

            Assert.That(m_ControlStub.Value, Is.EqualTo("kotte"));
        }

        [Test]
        public void BindingNotReactingOnAnotherPropertyChangedOnDataSource()
        {
            DynamicBinding dynamicBinding = new DynamicBinding("Value", m_DataItemProxy, "Value", true, m_DataSourceUpdateMode);
            m_ControlStub.DataBindings.Add(dynamicBinding);

            Assert.That(m_ControlStub.Value, Is.EqualTo("value"));

            SetDataItemProxyValue(x => x.FullName, "kotte");

            Assert.That(m_ControlStub.Value, Is.EqualTo("value"));
        }

        [Test]
        public void BindingReactsOnPropertyChangedOnControl()
        {
            if (m_DataSourceUpdateMode == DataSourceUpdateMode.Never)
                return;

            DynamicBinding dynamicBinding = new DynamicBinding("Value", m_DataItemProxy, "Value", true, m_DataSourceUpdateMode);
            m_ControlStub.DataBindings.Add(dynamicBinding);

            Assert.That(m_DataItemProxy.Value, Is.EqualTo("value"));

            m_ControlStub.Value = "kotte";

            Assert.That(m_DataItemProxy.Value, Is.EqualTo("kotte"));
        }

        [Test]
        public void BindingNotReactingOnAnotherPropertyChangedOnControl()
        {
            if (m_DataSourceUpdateMode == DataSourceUpdateMode.Never)
                return;

            DynamicBinding dynamicBinding = new DynamicBinding("Value", m_DataItemProxy, "Value", true, m_DataSourceUpdateMode);
            m_ControlStub.DataBindings.Add(dynamicBinding);

            Assert.That(m_ControlStub.Value, Is.EqualTo("value"));

            m_ControlStub.Name = "kotte";

            Assert.That(m_ControlStub.Value, Is.EqualTo("value"));
        }

        private void SetDataItemProxyValue<T>(Expression<Func<IDataItemProxy, T>> propertyExpression, object newValue)
        {
            MemberExpression member = propertyExpression.Body as MemberExpression;
            ((PropertyInfo)member.Member).SetValue(m_DataItemProxy, newValue, null);
            m_DataItemEventRaiser.Raise(m_DataItemProxy, new PropertyChangedEventArgs(member.Member.Name));
        }
    }
}