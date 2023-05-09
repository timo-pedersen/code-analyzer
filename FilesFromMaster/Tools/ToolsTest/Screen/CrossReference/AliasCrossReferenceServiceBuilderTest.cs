using System;
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.Alias.CrossReference;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasCrossReferenceServiceBuilderTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IProjectManager>(MockRepository.GenerateStub<IProjectManager>());
            TestHelper.AddService<IMessageBoxService>(MockRepository.GenerateStub<IMessageBoxService>());
            TestHelper.AddService<ICrossReferenceRebinderService>(MockRepository.GenerateStub<ICrossReferenceRebinderService>());

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
