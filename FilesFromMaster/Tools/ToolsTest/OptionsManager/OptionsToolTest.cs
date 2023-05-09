using System;
using Core.Api.Feature;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Options;
using NUnit.Framework;
using Rhino.Mocks;

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

            IServiceContainerCF serviceContainer = MockRepository.GenerateMock<IServiceContainerCF>();

            serviceContainer.Expect(x => x.AddService(Arg<Type>.Is.Anything, Arg<object>.Is.Anything, Arg<bool>.Is.Anything, Arg<bool>.Is.Anything)).Repeat.Once();
            optionsTool.Owner = serviceContainer;
            optionsTool.RegisterServices();

            serviceContainer.VerifyAllExpectations();
        }
    }
}
