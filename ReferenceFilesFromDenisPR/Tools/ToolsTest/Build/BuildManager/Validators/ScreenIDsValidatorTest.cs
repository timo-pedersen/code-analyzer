using System.Linq;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen;
using Neo.ApplicationFramework.Tools.Screen.ScreenId.Validation;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    public class ScreenIDsValidatorTest
    {
        private ScreenIDsProjectValidator m_ScreenIDsProjectValidator;
        private ScreenIDXmlFileReader m_ScreenIDXmlFileReader;
        private IErrorListService m_ErrorListService;

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.ClearServices();

            m_ScreenIDXmlFileReader = Substitute.For<ScreenIDXmlFileReader>();

            ITargetInfo target = Substitute.For<ITargetInfo>();
            target.ProjectPath = "";

            ITargetService targetService = TestHelper.AddServiceStub<ITargetService>();
            targetService.CurrentTargetInfo.Returns(target);

            m_ScreenIDsProjectValidator = new ScreenIDsProjectValidator(m_ScreenIDXmlFileReader);
            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();
        }

        [Test]
        public void WhenNoDuplicateScreenIdsIsFoundItDoesNotAddWarnings()
        {
            m_ScreenIDXmlFileReader.Load(Arg.Any<string>()).Returns(new ScreenIDInformation[] { });

            m_ScreenIDsProjectValidator.Validate();

            m_ErrorListService.DidNotReceiveWithAnyArgs().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void WarnsAboutEachOfTheDuplicatedIDs()
        {
            m_ScreenIDXmlFileReader.Load(Arg.Any<string>()).Returns(new[] {
                new ScreenIDInformation(1,"Screen1"),
                new ScreenIDInformation(1,"Screen2"),
                new ScreenIDInformation(3,"Screen3"),
                new ScreenIDInformation(3,"Screen4"),
            });

            m_ScreenIDsProjectValidator.Validate();

            m_ErrorListService.AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void WarnsDetailedInformationAboutTheDuplicatedIDs()
        {
            string expectedPartOfMessage = "Screen ID 1 was duplicated on screens Screen1 and Screen2.";

            m_ScreenIDXmlFileReader.Load(Arg.Any<string>()).Returns(new[] {
                new ScreenIDInformation(1,"Screen1"),
                new ScreenIDInformation(1,"Screen2")
            });

            var actualWarningMessage = string.Empty;
            m_ErrorListService.WhenForAnyArgs(x => x.AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>()))
                .Do(y => actualWarningMessage = (string)y[0]);

            m_ScreenIDsProjectValidator.Validate();

            Assert.That(actualWarningMessage, Does.Contain(expectedPartOfMessage));
        }
    }
}