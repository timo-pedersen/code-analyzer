using Core.Api.ProjectTarget;
using Core.Api.ProjectValidation;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.SNTP;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ProjectManager;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.ApplicationFramework.Tools.DateTimeEdit
{
	[TestFixture]
    public class TimeSynchronizationProjectValidatorTest
    {
        private IProjectItemFinder m_ProjectItemFinder;
        private List<IDesignerProjectItem> m_DesignerProjectItems;
        private ISntpClientRootComponent m_SntpClientRootComponent;
        private IProjectValidator m_Validator;
        private IErrorListService m_ErrorListService;
	    private ITargetService m_TargetService;
	    private ITerminal m_Terminal;
	    private bool m_SupportCloud;

		[SetUp]
        public void SetupUp()
        {
            m_ProjectItemFinder = MockRepository.GenerateStub<IProjectItemFinder>();

            m_DesignerProjectItems = new List<IDesignerProjectItem>();
            m_SntpClientRootComponent = MockRepository.GenerateStub<ISntpClientRootComponent>();

            var projectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            m_DesignerProjectItems.Add(projectItem);
            projectItem.Stub(x => x.ContainedObject).Return(m_SntpClientRootComponent);
            m_ProjectItemFinder.Stub(mock => mock.GetProjectItems(Arg<Type>.Is.Anything)).Return(m_DesignerProjectItems.ToArray());

            m_Validator = new TimeSynchronizationProjectValidator(m_ProjectItemFinder.ToILazy());

	        m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();

	        m_TargetService = TestHelper.CreateAndAddServiceMock<ITargetService>();

	        m_Terminal = MockRepository.GenerateStub<ITerminal>();
	        m_Terminal.Stub(x => x.SupportCloud).Do(new Func<bool>(SupportCloud));
			m_TargetService.Stub(x => x.CurrentTargetInfo.TerminalDescription).Return(m_Terminal);
		}

		private bool SupportCloud()
		{
			return m_SupportCloud;
		}

		[TearDown]
		public void TearDown()
        {
			TestHelper.ClearServices();
        }

        [Test]
        public void SntpTimeSynchNotEnabledTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(new ProjectItem());

            m_Validator.Validate();

            var actualWarningMessage = m_ErrorListService
                .GetCallsMadeOn(errorListService => errorListService.AddNewCompilerError(default(string), default(bool))).First()
                .Arguments.First() as string;

            Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
        }

        [Test]
        public void SntpTimeSynchEnabledWithWrongServerTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(new ProjectItem());

            m_SntpClientRootComponent.IsEnabled = true;
            m_SntpClientRootComponent.ServerName = "wrong.server.com";

            m_Validator.Validate();

            var actualWarningMessage = m_ErrorListService
                .GetCallsMadeOn(errorListService => errorListService.AddNewCompilerError(default(string), default(bool))).First()
                .Arguments.First() as string;

            Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
        }

        [Test]
        public void SntpTimeSynchEnabledWithRecommededServerTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(new ProjectItem());

            m_SntpClientRootComponent.IsEnabled = true;
            m_SntpClientRootComponent.ServerName = ApplicationConstantsCF.DefaultSntpServer;

            m_Validator.Validate();

            m_ErrorListService.AssertWasNotCalled(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
        }

        [Test]
        public void NoCloudFunctionalityEnabledTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(null);

            m_Validator.Validate();

            m_ErrorListService.AssertWasNotCalled(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
        }

	    [Test]
	    public void CloudFunctionalitySupportedTest()
	    {
		    m_SupportCloud = true;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(new ProjectItem());

			m_Validator.Validate();

		    var actualWarningMessage = m_ErrorListService
			    .GetCallsMadeOn(errorListService => errorListService.AddNewCompilerError(default(string), default(bool))).First()
			    .Arguments.First() as string;

		    Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
	    }

		[Test]
		public void CloudFunctionalityNotSupportedTest()
		{
			m_SupportCloud = false;

			m_ProjectItemFinder.Stub(x => x.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName)).Return(new ProjectItem());

			m_Validator.Validate();

			m_ErrorListService.AssertWasNotCalled(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything));
		}

	}
}
