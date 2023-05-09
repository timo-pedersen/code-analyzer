using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Terminal.Validation;
using NSubstitute;
using NUnit.Framework;

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

            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();

            m_ProjectManager = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            m_ProjectManager.Project = TestHelper.CreateAndAddServiceStub<IProject>();
            m_ProjectManager.Project.Terminal = Substitute.For<ITerminal>();

            m_TerminalManagerService = TestHelper.CreateAndAddServiceStub<ITerminalManagerService>();
            m_TerminalManagerService.GetTerminalList().Returns(new List<ITerminal>());

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
            m_ProjectManager.Project.Terminal.IsDeprecated.Returns(true);

            //ACT
            bool isValidated = m_DeprecatedTargetValidator.Validate();

            //ASSERT
            Assert.That(isValidated, Is.EqualTo(false));
            m_ErrorListService.Received().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void ValidationSucceedsForNonDeprecatedTargets()
        {
            //ARRANGE
            m_ProjectManager.Project.Terminal.IsDeprecated.Returns(false);

            //ACT
            bool isValidated = m_DeprecatedTargetValidator.Validate();

            //ASSERT
            Assert.That(isValidated, Is.EqualTo(true));
            m_ErrorListService.DidNotReceiveWithAnyArgs().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }
    }
}
