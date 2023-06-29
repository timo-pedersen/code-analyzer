#if !VNEXT_TARGET
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.AnalogNumeric;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Logic
{
    [TestFixture]
    public class ValidationViewModelTest
    {

        private ValidationViewModel m_ValidationViewModel;
        private IGlobalCommandService m_CommandService;

        [SetUp]
        public void Setup()
        {
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();

            m_ValidationViewModel = new ValidationViewModel();

            m_CommandService = Substitute.For<IGlobalCommandService>();
            m_ValidationViewModel.GlobalCommandService = m_CommandService;

        }
        
        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();

            //Uncheck both
            m_ValidationViewModel.ValidateValueOnInput = false;
            m_ValidationViewModel.ValidateValueOnDisplay = false;
        }

        [Test]
        public void IsMinMaxNotAllowedDefault()
        {
            Assert.IsTrue(m_ValidationViewModel.IsMinMaxAllowed == false);
            
        }
        
        [Test]
        public void IsMinMaxAllowedAfterValidateValueOnInputIsChecked()
        {
            m_ValidationViewModel.ValidateValueOnInput = true;
            Assert.IsTrue(m_ValidationViewModel.IsMinMaxAllowed == true);
        }
        [Test]
        public void IsMinMaxAllowedAfterBothIsChecked()
        {
            m_ValidationViewModel.ValidateValueOnInput = true;
            m_ValidationViewModel.ValidateValueOnDisplay = true;
            Assert.IsTrue(m_ValidationViewModel.IsMinMaxAllowed == true);
        }
        [Test]
        public void IsMinMaxAllowedAndNotAllowed()
        {
            m_ValidationViewModel.ValidateValueOnInput = true;
            Assert.IsTrue(m_ValidationViewModel.IsMinMaxAllowed == true);
            m_ValidationViewModel.ValidateValueOnInput = false;
            Assert.IsTrue(m_ValidationViewModel.IsMinMaxAllowed == false);
        }


    }
}
#endif
