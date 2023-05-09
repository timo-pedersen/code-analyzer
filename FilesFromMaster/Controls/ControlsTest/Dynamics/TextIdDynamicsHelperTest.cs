using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Dynamics
{
    [TestFixture]
    public class TextIdDynamicsHelperTest
    {
        private const string DefaultPropertyName = "Value";

        private IGlobalSelectionService m_GlobalSelectionServiceStub;
        private StringIntervalMapper m_StringIntervalMapper;

        private DataItemProxyProviderMock m_ProxyProvider;

        [SetUp]
        public void SetUp()
        {
            TestHelper.Bindings.Wpf.RegisterSimpleDataItemBindingSourceProvider();
            TestHelper.AddService<IDataCommandFacade>(new DataCommandFacade());
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            m_ProxyProvider = new DataItemProxyProviderMock();
            m_GlobalSelectionServiceStub = MockRepository.GenerateStub<IGlobalSelectionService>();

            m_StringIntervalMapper = new StringIntervalMapper();
            var stringIntervalFactoryStub = MockRepository.GenerateStub<StringIntervalHelper.IStringIntervalFactory>();
            stringIntervalFactoryStub.Stub(helper => helper.GetStringIntervalMapper(null)).IgnoreArguments().Return(m_StringIntervalMapper);
            StringIntervalHelper.StringIntervalFactory = stringIntervalFactoryStub;


            var terminalStub = MockRepository.GenerateStub<ITerminal>();
            var targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;

            ITarget targetStub = MockRepository.GenerateStub<ITarget>();

            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.Stub(x => x.CurrentTargetInfo).Return(targetInfoStub);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.Bindings.Wpf.ClearProviders();
            StringIntervalHelper.ResetStringIntervalFactory();
            TestHelper.ClearServices();
        }

        [Test]
        public void WillNotAddConverterToResourcesIfNoIntervalMapperExists()
        {
            // ARRANGE
            Button button = new Button();
            ITextIdDynamicsHelper textIdDynamicsHelper = new TextIdDynamicsHelper(button, "Text", m_GlobalSelectionServiceStub);

            // ACT
            textIdDynamicsHelper.AddConverterToResources();

            // ASSERT
            Assert.IsTrue(button.Resources.Count == 0);
        }

        [Test]
        public void WillNotAddConverterToResourcesWhenNoDataItemBinding()
        {
            // ARRANGE
            var designerResourceItem = MockRepository.GenerateStub<IDesignerResourceItem>();
            Binding binding = new Binding();
            binding.Path = new PropertyPath("CurrentValue");
            binding.Source = designerResourceItem;

            Button button = SetupObjectWithIntervals<Button>();
            BindingOperations.SetBinding(button.TextIntervalMapper.Intervals[0], StringInterval.ValueProperty, binding);

            ITextIdDynamicsHelper textIdDynamicsHelper = new TextIdDynamicsHelper(button, "Texts[0]", m_GlobalSelectionServiceStub);

            // ACT
            textIdDynamicsHelper.AddConverterToResources();

            // ASSERT
            Assert.IsTrue(button.Resources.Count == 0);
        }

        [Test]
        public void WillAddConverterToResourcesWhenDataItemBindingExists()
        {
            // ARRANGE
            Binding binding = GetDataItemBinding(new DataItemProxyMock<int>(StringConstants.TagsRoot + "D0"));
            Button button = SetupObjectWithIntervals<Button>();

            BindingOperations.SetBinding(button.TextIntervalMapper.Intervals[0], StringInterval.ValueProperty, binding);
            string converterKeyName = DependencyObjectPropertyBinder.GetConverterKeyName(StringInterval.ValueProperty, button.TextIntervalMapper.Intervals[0]);

            ITextIdDynamicsHelper textIdDynamicsHelper = new TextIdDynamicsHelper(button, "Texts[0]", m_GlobalSelectionServiceStub);

            // ACT
            textIdDynamicsHelper.AddConverterToResources();

            // ASSERT
            Assert.IsTrue(button.Resources.Contains(converterKeyName));
        }

        private T SetupObjectWithIntervals<T>() where T : DependencyObject, ISupportStringIntervals, new()
        {
            m_StringIntervalMapper.Intervals.Add(new StringInterval());

            T t = new T();
            PropertyInfo propertyInfo = StringIntervalHelper.GetMultiTextPropertyMappedStringIntervalMapperPropertyInfo(t);
            propertyInfo.SetValue(t, m_StringIntervalMapper, null);

            return t;
        }

        private Binding GetDataItemBinding(IDataItemProxy dataItemProxy)
        {
            string path = string.Format("[{0}].{1}", dataItemProxy.FullName, DefaultPropertyName);
            Binding binding = new Binding(path);
            binding.Mode = BindingMode.TwoWay;
            binding.Source = m_ProxyProvider;

            m_ProxyProvider.ProxyList.Add(dataItemProxy.FullName, dataItemProxy);

            return binding;
        }
    }
}
