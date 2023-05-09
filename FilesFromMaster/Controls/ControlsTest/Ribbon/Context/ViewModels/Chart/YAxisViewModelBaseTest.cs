using System.IO.Packaging;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Chart
{
    [TestFixture]
    public class YAxisViewModelBaseTest
    {
        private YAxisViewModelBase m_YAxisViewModelBase;

        [SetUp]
        public void TestFixtureSetup()
        {
            TestHelper.AddServiceStub<IGlobalCommandService>();
            string s = PackUriHelper.UriSchemePack;
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
            m_YAxisViewModelBase = new Y1AxisViewModel();
        }

        [Test]
        public void SetAxisAvailableToTrueAndAxisAutomaticToTrueResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisAutomatic = true;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAvailableToFalseAndAxisAutomaticToTrueResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            m_YAxisViewModelBase.IsAxisAutomatic = true;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAvailableToTrueAndAxisAutomaticToFalseResultsInMinMaxEnabledBeingTrue()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisAutomatic = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAutomatic = false;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.True);
        }

        [Test]
        public void SetAxisAvailableToFalseAndAxisAutomaticToFalseResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            m_YAxisViewModelBase.IsAxisAutomatic = true;
            m_YAxisViewModelBase.IsAxisAutomatic = false;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAutomaticToTrueAndAxisAvailableToTrueResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisAutomatic = true;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAutomaticToFalseAndAxisAvailableToTrueResultsInMinMaxEnabledBeingTrue()
        {
            m_YAxisViewModelBase.IsAxisAutomatic = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAutomatic = false;
            m_YAxisViewModelBase.IsAxisAvailable = true;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.True);
        }

        [Test]
        public void SetAxisAutomaticToTrueAndAxisAvailableToFalseResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            m_YAxisViewModelBase.IsAxisAutomatic = true;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAutomaticToFalseAndAxisAvailableToFalseResultsInMinMaxEnabledBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAutomatic = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAutomatic = false;
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisAvailable = false;
            Assert.That(m_YAxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAvailableToTrueAndAxisVisibleToTrueResultsInGridVisibilityBeingTrue()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisVisible = true;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.True);
        }

        [Test]
        public void SetAxisAvailableToFalseAndAxisVisibleToTrueResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            m_YAxisViewModelBase.IsAxisVisible = true;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }

        [Test]
        public void SetAxisAvailableToTrueAndAxisVisibleToFalseResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;
            m_YAxisViewModelBase.IsAxisVisible = true;      // trigger value changed
            m_YAxisViewModelBase.IsAxisVisible = false;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }

        [Test]
        public void SetAxisAvailableToFalseAndAxisVisibleToFalseResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            m_YAxisViewModelBase.IsAxisVisible = true;
            m_YAxisViewModelBase.IsAxisVisible = false;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }

        [Test]
        public void SetAxisVisibleToTrueAndAxisAvailableToTrueResultsInGridVisibilityBeingTrue()
        {
            m_YAxisViewModelBase.IsAxisVisible = true;
            m_YAxisViewModelBase.IsAxisAvailable = true;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.True);
        }

        [Test]
        public void SetAxisVisibleToFalseAndAxisAvailableToTrueResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisVisible = true;      // trigger value changed
            m_YAxisViewModelBase.IsAxisVisible = false;
            m_YAxisViewModelBase.IsAxisAvailable = true;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }

        [Test]
        public void SetAxisVisibleToTrueAndAxisAvailableToFalseResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisVisible = true;
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }

        [Test]
        public void SetAxisVisibleToFalseAndAxisAvailableToFalseResultsInGridVisibilityBeingFalse()
        {
            m_YAxisViewModelBase.IsAxisVisible = true;
            m_YAxisViewModelBase.IsAxisVisible = false;
            m_YAxisViewModelBase.IsAxisAvailable = true;    // trigger value changed
            m_YAxisViewModelBase.IsAxisAvailable = false;
            Assert.That(m_YAxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }
    }
}
