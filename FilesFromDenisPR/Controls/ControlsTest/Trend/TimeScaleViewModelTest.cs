#if!VNEXT_TARGET
using System.IO.Packaging;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            TestHelper.AddService(Substitute.For<ICommandManagerService>());
            TestHelper.AddService(Substitute.For<IGlobalCommandService>());
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
#endif
