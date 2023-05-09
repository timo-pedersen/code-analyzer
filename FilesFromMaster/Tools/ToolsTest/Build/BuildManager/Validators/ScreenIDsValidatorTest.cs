using System.Linq;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen;
using Neo.ApplicationFramework.Tools.Screen.ScreenId.Validation;
using NUnit.Framework;
using Rhino.Mocks;

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

            m_ScreenIDXmlFileReader = MockRepository.GenerateStub<ScreenIDXmlFileReader>();

            ITargetInfo target = MockRepository.GenerateStub<ITargetInfo>();
            target.ProjectPath = "";

            ITargetService targetService = TestHelper.AddServiceStub<ITargetService>();
            targetService.Stub(x => x.CurrentTargetInfo).Return(target);

            m_ScreenIDsProjectValidator = new ScreenIDsProjectValidator(m_ScreenIDXmlFileReader);
            m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();
        }

        [Test]
        public void WhenNoDuplicateScreenIdsIsFoundItDoesNotAddWarnings()
        {
            m_ScreenIDXmlFileReader.Stub(x => x.Load("")).IgnoreArguments().Return(new ScreenIDInformation[] { });

            m_ScreenIDsProjectValidator.Validate();

            m_ErrorListService.AssertWasNotCalled(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
        }

        [Test]
        public void WarnsAboutEachOfTheDuplicatedIDs()
        {
            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Twice();

            m_ScreenIDXmlFileReader.Stub(x => x.Load("")).IgnoreArguments().Return(new[] {
                new ScreenIDInformation(1,"Screen1"),
                new ScreenIDInformation(1,"Screen2"),
                new ScreenIDInformation(3,"Screen3"),
                new ScreenIDInformation(3,"Screen4"),
            });

            m_ScreenIDsProjectValidator.Validate();

            m_ErrorListService.VerifyAllExpectations();
        }

        [Test]
        public void WarnsDetailedInformationAboutTheDuplicatedIDs()
        {
            string expectedPartOfMessage = "Screen ID 1 was duplicated on screens Screen1 and Screen2.";

            m_ScreenIDXmlFileReader.Stub(x => x.Load("")).IgnoreArguments().Return(new[] {
                new ScreenIDInformation(1,"Screen1"),
                new ScreenIDInformation(1,"Screen2")
            });

            m_ScreenIDsProjectValidator.Validate();

            var actualWarningMessage = m_ErrorListService
                .GetCallsMadeOn(errorListService => errorListService.AddNewCompilerError(default(string), default(bool))).First()
                .Arguments.First() as string;

            Assert.That(actualWarningMessage, Does.Contain(expectedPartOfMessage));
        }
    }
}