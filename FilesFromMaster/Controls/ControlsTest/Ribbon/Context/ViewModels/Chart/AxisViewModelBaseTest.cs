using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Chart
{
    [TestFixture]
    public class AxisViewModelBaseTest
    {

        private AxisViewModelBase m_AxisViewModelBase;
        
        [SetUp]
        public void Setup()
        {
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
            TestHelper.AddServiceStub<IGlobalCommandService>();
            string s = System.IO.Packaging.PackUriHelper.UriSchemePack;

            m_AxisViewModelBase = new XAxisViewModel();
        }


        [Test]
        public void SetAxisAutomaticToTrueResultsInMinMaxEnabledBeingFalse()
        {
            m_AxisViewModelBase.IsAxisAutomatic = true;
            Assert.That(m_AxisViewModelBase.IsMinMaxEnabled, Is.False);
        }

        [Test]
        public void SetAxisAutomaticToFalseResultsInMinMaxEnabledBeingTrue()
        {
            m_AxisViewModelBase.IsAxisAutomatic = true; // trigger value changed
            m_AxisViewModelBase.IsAxisAutomatic = false;
            Assert.That(m_AxisViewModelBase.IsMinMaxEnabled, Is.True);
        }

        [Test]
        public void SetAxisVisibleToTrueResultsInGridVisibilityBeingTrue()
        {
            m_AxisViewModelBase.IsAxisVisible = true;
            Assert.That(m_AxisViewModelBase.IsGridVisibilityEnabled, Is.True);
        }

        [Test]
        public void SetAxisVisibleToFalseResultsInGridVisibilityBeingFalse()
        {
            m_AxisViewModelBase.IsAxisVisible = true; // trigger value changed
            m_AxisViewModelBase.IsAxisVisible = false;
            Assert.That(m_AxisViewModelBase.IsGridVisibilityEnabled, Is.False);
        }
    }
}
