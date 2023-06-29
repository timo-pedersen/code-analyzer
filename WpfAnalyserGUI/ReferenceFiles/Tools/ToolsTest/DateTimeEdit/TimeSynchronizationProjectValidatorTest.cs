#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.ProjectTarget;
using Core.Api.ProjectValidation;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.SNTP;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ProjectManager;
using NSubstitute;
using NUnit.Framework;

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
            m_ProjectItemFinder = Substitute.For<IProjectItemFinder>();

            m_DesignerProjectItems = new List<IDesignerProjectItem>();
            m_SntpClientRootComponent = Substitute.For<ISntpClientRootComponent>();

            var projectItem = Substitute.For<IDesignerProjectItem>();
            m_DesignerProjectItems.Add(projectItem);
            projectItem.ContainedObject.Returns(m_SntpClientRootComponent);
            m_ProjectItemFinder.GetProjectItems(Arg.Any<Type>()).Returns(m_DesignerProjectItems.ToArray());

            m_Validator = new TimeSynchronizationProjectValidator(m_ProjectItemFinder.ToILazy());

	        m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();

	        m_TargetService = TestHelper.CreateAndAddServiceStub<ITargetService>();

	        m_Terminal = Substitute.For<ITerminal>();
	        m_Terminal.SupportCloud.Returns(m_SupportCloud);
			m_TargetService.CurrentTargetInfo.TerminalDescription.Returns(m_Terminal);
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

			m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(new ProjectItem());

            m_Validator.Validate();

            string actualWarningMessage = string.Empty;
            m_ErrorListService.AddNewCompilerError(Arg.Do<string>(x => actualWarningMessage = x), Arg.Any<bool>());

            Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
        }

        [Test]
        public void SntpTimeSynchEnabledWithWrongServerTest()
        {
            m_SupportCloud = true;

            m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(new ProjectItem());

            m_SntpClientRootComponent.IsEnabled = true;
            m_SntpClientRootComponent.ServerName = "wrong.server.com";

            m_Validator.Validate();

            string actualWarningMessage = string.Empty;
            m_ErrorListService.AddNewCompilerError(Arg.Do<string>(x => actualWarningMessage = x), Arg.Any<bool>());

            Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
        }

        [Test]
        public void SntpTimeSynchEnabledWithRecommededServerTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(new ProjectItem());

            m_SntpClientRootComponent.IsEnabled = true;
            m_SntpClientRootComponent.ServerName = ApplicationConstantsCF.DefaultSntpServer;

            m_Validator.Validate();

            m_ErrorListService.DidNotReceiveWithAnyArgs().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void NoCloudFunctionalityEnabledTest()
        {
	        m_SupportCloud = true;

			m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(x => null);

            m_Validator.Validate();

            m_ErrorListService.DidNotReceiveWithAnyArgs().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

	    [Test]
	    public void CloudFunctionalitySupportedTest()
	    {
		    m_SupportCloud = true;

			m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(new ProjectItem());

			m_Validator.Validate();

            string actualWarningMessage = string.Empty;
            m_ErrorListService.AddNewCompilerError(Arg.Do<string>(x => actualWarningMessage = x), Arg.Any<bool>());

            Assert.That(actualWarningMessage, Does.Contain(TextsIde.RecommendedToUseSntpTimeSynchForCloud));
	    }

		[Test]
		public void CloudFunctionalityNotSupportedTest()
		{
			m_SupportCloud = false;

			m_ProjectItemFinder.GetProjectItem(ApplicationConstantsCF.CloudConfigurationName).Returns(new ProjectItem());

			m_Validator.Validate();

			m_ErrorListService.DidNotReceiveWithAnyArgs().AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
		}

	}
}
#endif
