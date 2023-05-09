using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Terminal.Validation;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    class DeprecatedTargetValidatorTest
    {
        private IErrorListService m_ErrorListService;
        private IProjectManager m_ProjectManager;
        private ITerminalManagerService m_TerminalManagerService;
        private DeprecatedTargetValidator m_DeprecatedTargetValidator;

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();

            m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();

            m_ProjectManager = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            m_ProjectManager.Project = TestHelper.CreateAndAddServiceStub<IProject>();
            m_ProjectManager.Project.Terminal = MockRepository.GenerateStub<ITerminal>();

            m_TerminalManagerService = TestHelper.CreateAndAddServiceStub<ITerminalManagerService>();
            m_TerminalManagerService.Stub(service => service.GetTerminalList()).Return(new List<ITerminal>());

            m_DeprecatedTargetValidator = new DeprecatedTargetValidator(m_ProjectManager.ToILazy(), m_TerminalManagerService.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void ValidationFailsForDeprecatedTargets()
        {
            //ARRANGE
            m_ProjectManager.Project.Terminal.Stub(terminal => terminal.IsDeprecated).Return(true);

            //ACT
            bool isValidated = m_DeprecatedTargetValidator.Validate();

            //ASSERT
            Assert.That(isValidated, Is.EqualTo(false));
            m_ErrorListService.AssertWasCalled(service => service.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
        }

        [Test]
        public void ValidationSucceedsForNonDeprecatedTargets()
        {
            //ARRANGE
            m_ProjectManager.Project.Terminal.Stub(terminal => terminal.IsDeprecated).Return(false);

            //ACT
            bool isValidated = m_DeprecatedTargetValidator.Validate();

            //ASSERT
            Assert.That(isValidated, Is.EqualTo(true));
            m_ErrorListService.AssertWasNotCalled(service => service.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
        }
    }
}
