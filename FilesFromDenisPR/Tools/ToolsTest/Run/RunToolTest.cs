using System;
using Core.Api.Service;
using Core.Api.Tools;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Run
{
    [TestFixture]
    public class RunToolTest
    {
        [Test]
        public void CreateRunTool()
        {
            RunTool runTool = new RunTool();

            Assert.IsNotNull(runTool);
        }

        [Test]
        public void RegisterService()
        {
            ITool tool = new RunTool();

            IServiceContainerCF serviceContainer = Substitute.For<IServiceContainerCF>();

            tool.Owner = serviceContainer;
            tool.RegisterServices();

            serviceContainer.Received(1).AddService(Arg.Any<Type>(), Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>());
        }
    }
}
