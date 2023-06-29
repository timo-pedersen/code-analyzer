using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.RollerPanel
{
    public class RollerPanelHostBaseCFTest
    {
        private RollerPanelHostBaseCF m_RollerPanelHostBaseCF;


        [SetUp]
        public void Setup()
        {
            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Create<IRollerPanel>().Returns(new RollerPanelControl());
            m_RollerPanelHostBaseCF = new RollerPanelHostCF();
        }

    
        [Test]
        public void SetTouchScrollFrictionHigherThanMaximumResultsInTouchScrollFrictionNotBeingChanged()
        {
            int touchScrollFriction = m_RollerPanelHostBaseCF.TouchScrollFriction;
            m_RollerPanelHostBaseCF.TouchScrollFriction = (int)RollerPanelHostBaseCF.MaximumTouchScrollFriction + 1;
            Assert.That(m_RollerPanelHostBaseCF.TouchScrollFriction, Is.EqualTo(touchScrollFriction));
        }

        [Test]
        public void SetTouchScrollFrictionLowerThanMinimumResultsInTouchScrollFrictionNotBeingChanged()
        {
            int touchScrollFriction = m_RollerPanelHostBaseCF.TouchScrollFriction;
            m_RollerPanelHostBaseCF.TouchScrollFriction = (int)RollerPanelHostBaseCF.MinimumTouchScrollFriction - 1;
            Assert.That(m_RollerPanelHostBaseCF.TouchScrollFriction, Is.EqualTo(touchScrollFriction));
        }
    }
}
