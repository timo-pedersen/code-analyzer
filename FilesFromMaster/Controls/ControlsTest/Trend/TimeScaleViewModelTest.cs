using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.IO.Packaging;

namespace Neo.ApplicationFramework.Controls.Trend
{
    [TestFixture]
    public class TimeScaleViewModelTest
    {
        private TimeScaleViewModel m_TimeScaleViewModel;

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            TestHelper.AddService(MockRepository.GenerateStub<ICommandManagerService>());
            TestHelper.AddService(MockRepository.GenerateStub<IGlobalCommandService>());
            string s = PackUriHelper.UriSchemePack;

            m_TimeScaleViewModel = new TimeScaleViewModel();
        }

        [Test]
        [TestCase(10, 10)]
        [TestCase(100, 100)]
        [TestCase(1000, 0)]
        public void VerifyTimeScaleValues(int value, int expectedValue)
        {
            m_TimeScaleViewModel.TimeScaleMajorTickCount = value;
            Assert.That(m_TimeScaleViewModel.TimeScaleMajorTickCount, Is.EqualTo(expectedValue));
        }
    }
}
