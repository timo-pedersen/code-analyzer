#if !VNEXT_TARGET
using Core.Controls.Api.Bindings;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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
            var actionParameter = Substitute.For<IActionParameterInfo>();
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
            var actionParameter = Substitute.For<IActionParameterInfo>();
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
            var actionParameter = Substitute.For<IActionParameterInfo>();
            var actionParameter2 = Substitute.For<IActionParameterInfo>();

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
            var actionParameter = Substitute.For<IActionParameterInfo>();
            var actionParameter2 = Substitute.For<IActionParameterInfo>();

            actionParameter.ParameterType = typeof(int);
            actionParameter2.ParameterType = typeof(string);
            actionParameter2.ParameterTypeAlias.Returns("string");

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
#endif