using Core.Controls.Api.Bindings;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Action.ActionEditors.ViewModels
{
    [TestFixture]
    public class SelectParametersViewModelTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<IGlobalSelectionService>();
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
            TestHelper.CreateAndAddServiceStub<IBindingService>();
            TestHelper.CreateAndAddServiceStub<IStructuredBindingSupportService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void IfErrorIsSetIsValidReturnsFalse()
        {
            var actionParameter = MockRepository.GenerateStub<IActionParameterInfo>();
            var actionParameters = new[]
            {
                actionParameter
            };
            SelectParametersViewModel viewModel = new SelectParametersViewModel(actionParameters);

            viewModel.Items[0].HasError = true;

            Assert.That(viewModel.IsValid, Is.False);
        }

        [Test]
        public void IfAllTagNamesSetIsValidReturnsTrue()
        {
            var actionParameter = MockRepository.GenerateStub<IActionParameterInfo>();
            actionParameter.ParameterType = typeof(int);
            
            var actionParameters = new[]
            {
                actionParameter
            };
            SelectParametersViewModel viewModel = new SelectParametersViewModel(actionParameters);

            viewModel.Items[0].TagName = "Tag1";

            Assert.That(viewModel.IsValid, Is.True);

        }

        [Test]
        public void IfOneParameterIsNotSetIsValidReturnsFalse()
        {
            var actionParameter = MockRepository.GenerateStub<IActionParameterInfo>();
            var actionParameter2 = MockRepository.GenerateStub<IActionParameterInfo>();

            actionParameter.ParameterType = typeof(int);
            actionParameter2.ParameterType = typeof(int);

            var actionParameters = new[]
            {
                actionParameter, actionParameter2
            };
            SelectParametersViewModel viewModel = new SelectParametersViewModel(actionParameters);

            viewModel.Items[0].TagName = "Tag1";

            Assert.That(viewModel.IsValid, Is.False);
        }

        [Test]
        public void IfOneParameterIsNotSetButStringIsValidReturnsTrue()
        {
            var actionParameter = MockRepository.GenerateStub<IActionParameterInfo>();
            var actionParameter2 = MockRepository.GenerateStub<IActionParameterInfo>();

            actionParameter.ParameterType = typeof(int);
            actionParameter2.ParameterType = typeof(string);
            actionParameter2.Stub(a => a.ParameterTypeAlias).Return("string");

            var actionParameters = new[]
            {
                actionParameter, actionParameter2
            };
            SelectParametersViewModel viewModel = new SelectParametersViewModel(actionParameters);

            viewModel.Items[0].TagName = "Tag1";

            Assert.That(viewModel.IsValid, Is.True);
        }
    }
}