#if!VNEXT_TARGET
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
using NSubstitute;
using NUnit.Framework;

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
            m_GlobalSelectionServiceStub = Substitute.For<IGlobalSelectionService>();

            m_StringIntervalMapper = new StringIntervalMapper();
            var stringIntervalFactoryStub = Substitute.For<StringIntervalHelper.IStringIntervalFactory>();
            stringIntervalFactoryStub.GetStringIntervalMapper(Arg.Any<DependencyObject>()).Returns(m_StringIntervalMapper);
            StringIntervalHelper.StringIntervalFactory = stringIntervalFactoryStub;


            var terminalStub = Substitute.For<ITerminal>();
            var targetInfoStub = Substitute.For<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;

            ITarget targetStub = Substitute.For<ITarget>();

            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.CurrentTargetInfo.Returns(targetInfoStub);
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
            var designerResourceItem = Substitute.For<IDesignerResourceItem>();
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
#endif
