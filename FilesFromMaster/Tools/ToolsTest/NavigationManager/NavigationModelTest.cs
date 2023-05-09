using System.Reflection;
using Core.Api.Feature;
using Core.Api.Service;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Action;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.NavigationManager
{
    [TestFixture]
    public class NavigationModelTest
    {
        private NavigationModel m_NavigationModel;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var featureSecurityServiceIdeStub = MockRepository.GenerateStub<IFeatureSecurityServiceIde>();
            featureSecurityServiceIdeStub.Stub(x => x.IsActivated(null)).IgnoreArguments().Return(true);
            
            var gapServiceStubLazy = MockRepository.GenerateStub<ILazy<IGapService>>();
            gapServiceStubLazy.Stub(s => s.Value).Return(MockRepository.GenerateStub<IGapService>());
            gapServiceStubLazy.Value.Stub(x => x.IsSubjectConsideredGap(Arg<MemberInfo>.Is.Anything)).Return(false);

            ActionService actionService = new ActionService(featureSecurityServiceIdeStub, gapServiceStubLazy);
            TestHelper.AddService(typeof(IActionService), actionService);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.ClearServices();
        }

        [SetUp]
        public void SetUp()
        {
            m_NavigationModel = new NavigationModel(null);
        }

        [Test]
        public void CreateShowScreenAction()
        {
            IActionService actionService = ServiceContainerCF.GetService<IActionService>();
            actionService.AddNoneAction();
            actionService.AddActionType(typeof(Screen.ScreenDesign.Screen));


            IAction action = m_NavigationModel.CreateShowScreenAction("Screen2");
            Assert.IsNotNull(action);
        }
    }
}
