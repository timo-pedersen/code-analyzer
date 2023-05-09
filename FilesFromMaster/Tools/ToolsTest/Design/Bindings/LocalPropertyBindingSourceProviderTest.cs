using System.ComponentModel.Design;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Design.Bindings
{
    /// <summary>
    /// Tests for <see cref="LocalPropertyDataSourceProvider"/>.
    /// </summary>
    [TestFixture]
    public class LocalPropertyBindingSourceProviderTest
    {
        private IDesignerEventService m_DesignerEventService;
        private IAliasConfiguration m_AliasConfiguration;
        private Screen.ScreenDesign.Screen m_Screen;
        
        [SetUp]
        public void TestFixtureSetUp()
        {
            m_DesignerEventService = TestHelper.AddServiceStub<IDesignerEventService>();
            m_AliasConfiguration = MockRepository.GenerateStub<IAliasConfiguration>();

            m_Screen = MockRepository.GenerateStub<Screen.ScreenDesign.Screen>();
            m_Screen.AliasConfiguration = m_AliasConfiguration;
            var designerHost = MockRepository.GenerateStub<IDesignerHost>();
            m_DesignerEventService
                .Stub(des => des.ActiveDesigner)
                .Return(designerHost);

            designerHost
                .Stub(dh => dh.RootComponent)
                .Return(m_Screen);
        }

        [Test]
        public void EventSentWhenAliasAdded()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new LocalPropertyDataSourceProvider();
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var args = new AliasChangedEventArgs(
                new AliasDefinition(),
                AliasAction.Add);

            // Raise event
            m_AliasConfiguration
                .GetEventRaiser(tcns => tcns.AliasChanged += null)
                .Raise(m_AliasConfiguration, args);

            Assert.That(eventSent, Is.True);
        }

        [Test]
        public void EventSentWhenAliasRemoved()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new LocalPropertyDataSourceProvider();
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var args = new AliasChangedEventArgs(
                new AliasDefinition(),
                AliasAction.Remove);

            // Raise event
            m_AliasConfiguration
                .GetEventRaiser(tcns => tcns.AliasChanged += null)
                .Raise(m_AliasConfiguration, args);

            Assert.That(eventSent, Is.True);
        }

        [Test]
        public void EventSentWhenAliasChanged()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new LocalPropertyDataSourceProvider();
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var args = new AliasChangedEventArgs(
                new AliasDefinition(),
                AliasAction.Changed);

            // Raise event
            m_AliasConfiguration
                .GetEventRaiser(tcns => tcns.AliasChanged += null)
                .Raise(m_AliasConfiguration, args);

            Assert.That(eventSent, Is.True);
        }
    }
}