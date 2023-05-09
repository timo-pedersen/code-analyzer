using System;
using Core.Api.Feature;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Options;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OptionsManager
{
    [TestFixture]
    public class OptionsToolTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IFeatureSecurityServiceIde>();
        }

        [Test]
        public void CreateTool()
        {
            OptionsTool optionsTool = new OptionsTool();

            Assert.IsNotNull(optionsTool);
        }

        [Test]
        public void RegisterIDEOptionsService()
        {
            ITool optionsTool = new OptionsTool();

            IServiceContainerCF serviceContainer = Substitute.For<IServiceContainerCF>();

            optionsTool.Owner = serviceContainer;
            optionsTool.RegisterServices();

            serviceContainer.Received().AddService(Arg.Any<Type>(), Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>());
        }
    }
}
