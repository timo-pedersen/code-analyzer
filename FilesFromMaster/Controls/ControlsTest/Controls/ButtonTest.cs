using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class ButtonTest
    {

        [SetUp]
        public void Setup()
        {
            var terminalStub = MockRepository.GenerateStub<ITerminal>();
            var targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;

            ITarget targetStub = MockRepository.GenerateStub<ITarget>();

            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.Stub(x => x.CurrentTargetInfo).Return(targetInfoStub);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }


        [Test]
        public void MultiTextPropertyAttributeMapsToValidStringIntervalMapperPC()
        {
            Assert.IsNotNull(StringIntervalHelper.GetMultiTextPropertyMappedStringIntervalMapperPropertyInfo(new Neo.ApplicationFramework.Controls.Button()));
        }

    }
}
