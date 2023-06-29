using System;
using System.Collections.Generic;
using System.Windows;
using Neo.ApplicationFramework.Attributes;
using Neo.ApplicationFramework.Common.Ribbon;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.PropertyAdapters
{
    [TestFixture]
    public class RibbonAttributeContextProviderTest
    {
        RibbonAttributeContextProvider m_RibbonAttributeContextProvider;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // Needed in order to register pack://application for use in URIs.
            Application application = new Application();
        }

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();

            m_RibbonAttributeContextProvider = new RibbonAttributeContextProvider();
        }

        [Test]
        public void TestNullAsAttribute()
        {
            IList<Type> types = new List<Type>();
            types.Add(typeof(AnalogNumericFX));
            types.Add(typeof(Neo.ApplicationFramework.Controls.Label));

            Assert.IsFalse(m_RibbonAttributeContextProvider.AllTypesHasAttribute(types, null));
        }

        [Test]
        public void GetRibbonContextContainersDoesNotAddExternalResourceDictionariesWhenNotSpecified()
        {
            IRibbonService ribbonServiceStub = TestHelper.AddServiceStub<IRibbonService>();

            IRibbonContextProvider ribbonContextProvider = (IRibbonContextProvider)m_RibbonAttributeContextProvider;
            var ribbonContextContainers = ribbonContextProvider.GetRibbonContextContainers(new object[] { new ThirdPartyControlWithoutSpecifiedRibbonResourceDictionary() });

            ribbonServiceStub.DidNotReceive().AddResourceDictionary(Arg.Any<System.Windows.ResourceDictionary>());
            TestHelper.RemoveService<IRibbonService>();
        }

        [Test]
        public void GetRibbonContextContainersAddsExternalResourceDictionariesWhenSpecified()
        {
            IRibbonService ribbonServiceStub = TestHelper.AddServiceStub<IRibbonService>();

            IRibbonContextProvider ribbonContextProvider = (IRibbonContextProvider)m_RibbonAttributeContextProvider;
            var ribbonContextContainers = ribbonContextProvider.GetRibbonContextContainers(new object[] { new ThirdPartyControlWithSpecifiedRibbonResourceDictionary() });

            ribbonServiceStub.Received().AddResourceDictionary(Arg.Any<System.Windows.ResourceDictionary>());
            TestHelper.RemoveService<IRibbonService>();
        }
    }

    [RibbonDefaultContext("General")]
    [RibbonContextContainer("Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.VisibilityViewModel, ControlsIde", SortOrder = 1)]
    public class ThirdPartyControlWithoutSpecifiedRibbonResourceDictionary
    {
    }

    [RibbonDefaultContext("General")]
    [RibbonContextContainer("Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.VisibilityViewModel, ControlsIde", ResourceDictionaryUri = "pack://application:,,,/ControlsIde;component/Ribbon/Context/Views/Visibility.xaml", SortOrder = 1)]
    public class ThirdPartyControlWithSpecifiedRibbonResourceDictionary
    {
    }
}
