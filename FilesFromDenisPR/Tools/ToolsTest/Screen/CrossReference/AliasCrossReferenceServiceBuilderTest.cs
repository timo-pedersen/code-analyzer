#if !VNEXT_TARGET
using System;
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.Alias.CrossReference;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasCrossReferenceServiceBuilderTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService(Substitute.For<IProjectManager>());
            TestHelper.AddService(Substitute.For<IMessageBoxService>());
            TestHelper.AddService(Substitute.For<ICrossReferenceRebinderService>());

        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CallingBuildTwiceThrowsInvalidOperationException()
        {
            try
            {
                var service = AliasCrossReferenceServiceFactory.CreateAliasCrossReferenceServiceSingleton();
                Assert.That(service, Is.Not.Null);
            }
            catch (InvalidOperationException)
            {
                Assert.Fail("An InvalidOperationException was thrown on the FIRST call to CreateAliasCrossReferenceServiceSingleton.");
            }

            try
            {
                AliasCrossReferenceServiceFactory.CreateAliasCrossReferenceServiceSingleton();
                Assert.Fail("An InvalidOperationException was NOT thrown on the SECOND call to CreateAliasCrossReferenceServiceSingleton.");
            }
            catch (InvalidOperationException) { }
        }
    }
}
#endif
