using System;
using System.Windows.Threading;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient.Bindings
{
    /// <summary>
    /// Tests for <see cref="TagDataSourceProvider"/>.
    /// </summary>
    [TestFixture]
    public class TagBindingSourceProviderTest
    {
        private ITagChangedNotificationServiceCF m_TagChangedNotificationService;

        [SetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.AddServiceStub<IOpcClientServiceIde>();
            m_TagChangedNotificationService = TestHelper.AddServiceStub<ITagChangedNotificationServiceCF>();
        }

        [Test]
        public void Event_sent_when_tag_added()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new TagDataSourceProvider(new ProtectableItemServiceIde().ToILazy<IProtectableItemServiceIde>());
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();

            // Raise event
            m_TagChangedNotificationService
                .GetEventRaiser(tcns => tcns.TagAdded += null)
                .Raise(m_TagChangedNotificationService, new TagChangedEventArgs(globalDataItem));

            // Wait with assertion until event execution is finished
            InvokeOnDispatcher(() => Assert.That(eventSent, Is.True));
        }

        [Test]
        public void Event_sent_when_tag_deleted()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new TagDataSourceProvider(new ProtectableItemServiceIde().ToILazy<IProtectableItemServiceIde>());
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();

            // Raise event
            m_TagChangedNotificationService
                .GetEventRaiser(tcns => tcns.TagDeleted += null)
                .Raise(m_TagChangedNotificationService, new TagChangedEventArgs(globalDataItem));

            InvokeOnDispatcher(() => Assert.That(eventSent, Is.True));
        }

        [Test]
        public void EventSentWhenTagPropertyDataitemsChanged()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new TagDataSourceProvider(new ProtectableItemServiceIde().ToILazy<IProtectableItemServiceIde>());
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var eventArgs = new TagPropertyChangedEventArgs(
                MockRepository.GenerateStub<IGlobalDataItem>(),
                "DataItems");

            // Raise event
            m_TagChangedNotificationService
                .GetEventRaiser(tcns => tcns.TagPropertyChanged += null)
                .Raise(m_TagChangedNotificationService, eventArgs);

            InvokeOnDispatcher(() => Assert.That(eventSent, Is.True));
        }

        [Test]
        public void EventNotSentWhenOtherTagPropertyChanged()
        {
            bool eventSent = false;

            // Create provider and listen for data source changed event
            var provider = new TagDataSourceProvider(new ProtectableItemServiceIde().ToILazy<IProtectableItemServiceIde>());
            provider.DataSourceChanged += (sender, e) => eventSent = true;

            // Create event payload
            var eventArgs = new TagPropertyChangedEventArgs(
                MockRepository.GenerateStub<IGlobalDataItem>(),
                "SomeProperty");

            // Raise event
            m_TagChangedNotificationService
                .GetEventRaiser(tcns => tcns.TagPropertyChanged += null)
                .Raise(m_TagChangedNotificationService, eventArgs);

            InvokeOnDispatcher(() => Assert.That(eventSent, Is.False));
        }

        private static void InvokeOnDispatcher(System.Action action)
        {
            Dispatcher.CurrentDispatcher.Invoke(action, DispatcherPriority.ApplicationIdle);
        }
    }
}