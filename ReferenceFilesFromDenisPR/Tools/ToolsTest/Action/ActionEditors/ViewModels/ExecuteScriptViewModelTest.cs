using Core.Api.GlobalReference;
using Core.Api.Service;
using Neo.ApplicationFramework.Controls.Action.ActionEditors;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Action.ActionEditors.ViewModels
{
    [TestFixture]
    public class ExecuteScriptViewModelTest
    {
        [OneTimeTearDown]
        public void TestFixtureTeardown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void VerifyThatTheCorrectScriptMethodIsSelectedInTheViewBasedOnActionWithoutOverwriterItsValues()
        {
            ScriptModule.ScriptModule scriptModule = new ScriptModule.ScriptModule();
            scriptModule.Name = "ScriptModule";

            IGlobalReferenceService globalReferenceServiceStub = Substitute.For<IGlobalReferenceService>();
            globalReferenceServiceStub.GetObjects<ScriptModule.ScriptModule>(false).Returns(new[] { scriptModule });
            globalReferenceServiceStub.GetObject<ScriptModule.ScriptModule>(Arg.Any<string>()).Returns(scriptModule);
            ServiceContainerCF.Instance.AddService(typeof(IGlobalReferenceService), globalReferenceServiceStub);

            IProjectItem scriptOwnerMock = Substitute.For<IProjectItem, IScriptOwner>();
            ((IScriptOwner)scriptOwnerMock).ScriptText.Returns(@"
                public static void StaticMethod()
                public void Method()
                public void MethodWithParam(IScriptTag param)
                public void MethodWithParams(string param1, string param2)                
                public void MethodWithParams(string param1, ref string param2)
                public void MethodWithParams(int param1, int[] arrayParam2)
                public void MethodWithUnsupportedParam(CustomClass object)
                public void MethodWithSourceParam(IScriptObject sourceParam, int param2)");

            IProjectItemFinder projectItemFinderStub = Substitute.For<IProjectItemFinder>();
            projectItemFinderStub.GetProjectItem(scriptModule).Returns(scriptOwnerMock);
            ServiceContainerCF.Instance.AddService(typeof(IProjectItemFinder), projectItemFinderStub);

            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();

            ExecuteScriptViewModel viewModelToTest = new ExecuteScriptViewModel();

            Action action = new Action("Execute Script");
            action.ActionMethodInfo = new ActionMethodInfo("MethodWithParams");
            action.ActionMethodInfo.ObjectName = "ScriptModule";
            action.ActionMethodInfo.ActionParameterInfo = new ActionParameterInfoList() { 
                new ActionParameterInfo() {
                    ParameterName = "param1",
                    ParameterType = typeof(string),
                    Value = "value of param1"
                },
                new ActionParameterInfo() {
                    ParameterName = "param2",
                    ParameterType = typeof(string),
                    Value = "value of param2"
                }
            };

            // Set project's action value
            ((IActionEditor)viewModelToTest).Action = action;

            // Verify the number of parsed and accepted methods
            Assert.That(viewModelToTest.ScriptMethods.Count, Is.EqualTo(5));

            // Verify the selected script method in the view, that has been selected based on the action
            ActionParameterInfoList scriptParameters = viewModelToTest.SelectedScriptMethod.ParameterInfos;
            Assert.That(scriptParameters.Count, Is.EqualTo(2));
            Assert.That(scriptParameters.Count, Is.EqualTo(2));
            Assert.That(scriptParameters[0].ParameterType.FullName, Is.EqualTo("System.String"));
            Assert.That(scriptParameters[0].Value, Is.Null); // This is just the definition, i.e. has no value

            ActionParameterInfoList actionParameters = ((IActionEditor)viewModelToTest).Action.ActionMethodInfo.ActionParameterInfo;
            Assert.That(actionParameters.Count, Is.EqualTo(scriptParameters.Count));
            
            // Verify that the action's values not have been overwritten by the view model
            Assert.That(actionParameters[0].Value, Is.EqualTo("value of param1"));
            
            // Compare script parameters from action paramters
            for (int i = 0; i < scriptParameters.Count; i++)
            {
                if (scriptParameters[i].ParameterType != actionParameters[i].ParameterType)
                {
                    Assert.Fail("Parameter type missmatch");
                }
            }

            // Verify a method with a source parameter, hence the source paramter shall be a non visible parameter
            ScriptTextItemInfo scriptMethod = viewModelToTest.ScriptMethods[4];
            Assert.That(scriptMethod.ParameterInfos.Count, Is.EqualTo(2));
            Assert.That(scriptMethod.ParameterInfos[0].ParameterTypeAlias, Is.EqualTo(typeof(IScriptObject).Name));
            Assert.That(scriptMethod.Description, Is.EqualTo("int param2"));
        }
    }
}
