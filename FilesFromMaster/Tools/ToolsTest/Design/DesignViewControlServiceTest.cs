using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Design
{
    [TestFixture]
    class DesignViewControlServiceTest
    {
        private IDesignerViewControlService m_DesignerViewControlService;
        private IDesignerViewControl m_ControlStub;
        private const string Identifier = "identifier";

        [SetUp]
        public void Setup()
        {
            m_DesignerViewControlService = new DesignerViewControlService();
            m_ControlStub = MockRepository.GenerateMock<IDesignerViewControl>();
            m_ControlStub.Stub(x => x.Identifier).Return(Identifier);
        }

        [Test]
        public void SetButtonStatesActionIsCalled()
        {
            bool wasCalled = false;
            m_DesignerViewControlService.ReplaceSetButtonStatesAction(Identifier, control=>wasCalled=true, ButtonStateActionPriority.StandardAction);
            m_DesignerViewControlService.SetButtonStates(m_ControlStub);

            Assert.IsTrue(wasCalled); 
        }

        [Test]
        [TestCase(ButtonStateActionPriority.StandardAction, ButtonStateActionPriority.StandardAction, false, true)]
        [TestCase(ButtonStateActionPriority.StandardAction, ButtonStateActionPriority.FeatureReplacedAction, false, true)]
        [TestCase(ButtonStateActionPriority.FeatureReplacedAction, ButtonStateActionPriority.StandardAction, true, false)]
        [TestCase(ButtonStateActionPriority.FeatureReplacedAction, ButtonStateActionPriority.FeatureReplacedAction, false, true)]
        public void VerifyReplacmentPriority(ButtonStateActionPriority first, ButtonStateActionPriority second, bool expectFirstCalled, bool expectSecondCalled)
        {
            bool firstWasCalled = false;
            bool secondWasCalled = false;
            m_DesignerViewControlService.ReplaceSetButtonStatesAction(Identifier, control => firstWasCalled = true, first);
            m_DesignerViewControlService.ReplaceSetButtonStatesAction(Identifier, control => secondWasCalled = true, second);
            m_DesignerViewControlService.SetButtonStates(m_ControlStub);

            Assert.IsTrue(firstWasCalled ==expectFirstCalled);
            Assert.IsTrue(secondWasCalled == expectSecondCalled);
        }
    }
}
