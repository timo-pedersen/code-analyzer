#if !VNEXT_TARGET
using System;
using System.Windows.Threading;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using NSubstitute;
using NUnit.Framework;

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
            var globalDataItem = Substitute.For<IGlobalDataItem>();

            // Raise event
            Raise.EventWith(m_TagChangedNotificationService, new TagChangedEventArgs(globalDataItem));

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
            var globalDataItem = Substitute.For<IGlobalDataItem>();

            // Raise event
            Raise.EventWith(m_TagChangedNotificationService, new TagChangedEventArgs(globalDataItem));

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
                Substitute.For<IGlobalDataItem>(),
                "DataItems");

            // Raise event
            Raise.EventWith(m_TagChangedNotificationService, eventArgs);

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
                Substitute.For<IGlobalDataItem>(),
                "SomeProperty");

            // Raise event
            Raise.EventWith(m_TagChangedNotificationService, eventArgs);

            InvokeOnDispatcher(() => Assert.That(eventSent, Is.False));
        }

        private static void InvokeOnDispatcher(System.Action action)
        {
            Dispatcher.CurrentDispatcher.Invoke(action, DispatcherPriority.ApplicationIdle);
        }
    }
}
#endif
