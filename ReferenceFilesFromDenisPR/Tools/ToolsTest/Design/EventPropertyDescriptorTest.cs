using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Design
{
    [TestFixture]
    public class EventPropertyDescriptorTest
    {
        [Test]
        public void GetValueReturnsNullOnComponentWithoutSite()
        {
            IServiceProvider serviceProviderMock = Substitute.For<IServiceProvider>();
            SampleEventBindingService sampleEventBindingService = new SampleEventBindingService(serviceProviderMock);

            IEventBindingService eventBindingService = sampleEventBindingService as IEventBindingService;
            Assert.IsNotNull(eventBindingService, "IEventBindingService not implemented.");

            Component component = new Component();
            EventDescriptorCollection eventDescriptorCollection = TypeDescriptor.GetEvents(component);
            EventDescriptor eventDescriptor = eventDescriptorCollection[0];
            PropertyDescriptor eventPropertyDescriptor = eventBindingService.GetEventProperty(eventDescriptor);

            Assert.IsNull(eventPropertyDescriptor.GetValue(component));
        }
    }
}