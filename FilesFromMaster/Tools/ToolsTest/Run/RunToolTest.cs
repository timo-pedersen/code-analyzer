using System;
using Core.Api.Service;
using Core.Api.Tools;
using NUnit.Framework;
using Rhino.Mocks;

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

            IServiceContainerCF serviceContainer = MockRepository.GenerateMock<IServiceContainerCF>();
            serviceContainer.Expect(x => x.AddService(Arg<Type>.Is.Anything, Arg<object>.Is.Anything, Arg<bool>.Is.Anything, Arg<bool>.Is.Anything)).Repeat.Once();

            tool.Owner = serviceContainer;
            tool.RegisterServices();

            serviceContainer.VerifyAllExpectations();
        }
    }
}
