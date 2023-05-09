using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ControllerManager
{
    [TestFixture]
    public class ControllerManagerTest
    {
        private IControllerManagerService m_ControllerManagerService;
        private IBrandService m_BrandService;

        [SetUp]
        public void Setup()
        {
            m_BrandService = Substitute.For<IBrandService>();

            TestHelper.AddService(m_BrandService);
            
            m_ControllerManagerService = new ControllerManager();
        }

        [Test]
        public void ControllerManagerSupportsInterface()
        {

            Assert.IsNotNull(m_ControllerManagerService, "Problems to create ControllerManager");
        }

        [Test]
        public void PossibilityToGetControllerList()
        {
            m_BrandService.SupportedDrivers.Returns(131);
            IList<IController> controllerList = m_ControllerManagerService.GetControllers();
        }
    }
}
