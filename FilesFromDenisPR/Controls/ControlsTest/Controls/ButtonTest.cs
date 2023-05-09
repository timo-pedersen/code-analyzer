using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Controls
{
    [TestFixture]
    public class ButtonTest
    {

        [SetUp]
        public void Setup()
        {
            var terminalStub = Substitute.For<ITerminal>();
            var targetInfoStub = Substitute.For<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;

            ITarget targetStub = Substitute.For<ITarget>();

            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.CurrentTargetInfo.Returns(targetInfoStub);
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
