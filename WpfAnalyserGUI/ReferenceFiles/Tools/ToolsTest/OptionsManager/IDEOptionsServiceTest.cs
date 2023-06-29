using System;
using System.Windows.Forms;
using Core.Api.Feature;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.WindowManagement;
using Neo.ApplicationFramework.Tools.Options;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OptionsManager
{
    [TestFixture]
    public class IDEOptionsServiceTest
    {
        private ILazy<IFeatureSecurityServiceIde> m_FeatureSecurityServiceIde;

        [SetUp]
        public void FixtureSetup()
        {
            m_FeatureSecurityServiceIde = Substitute.For<IFeatureSecurityServiceIde>().ToILazy();
        }

        [Test]
        public void CreateService()
        {
            IDEOptionsService iDEOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);

            Assert.IsNotNull(iDEOptionsService);
        }

        [Test]
        public void AddOptionFileSettingsServiceUnavailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);

            Assert.IsNotNull(ideOptionsService.AddOption<object>());

            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            Assert.AreEqual(optionsService.Options.Count, 1);
        }

        [Test]
        public void AddOptionFileSettingsServiceAvailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            IFileSettingsService fileSettingsService = Substitute.For<IFileSettingsService>();
            optionsService.FileSettingsService = fileSettingsService;

            fileSettingsService.LoadUserSettings<object>().Returns(new object());

            ideOptionsService.AddOption<object>();

            Assert.AreEqual(optionsService.Options.Count, 1);
            fileSettingsService.Received().LoadUserSettings<object>();
        }

        /// <summary>
        /// Negative test
        /// </summary>
        [Test]
        public void ShowWhenNoServiceAvailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);

            ideOptionsService.Show();
        }

        [Test]
        public void ShowWhenWindowServiceIsAvailableNullFormReturned()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            IWindowServiceIde windowService = Substitute.For<IWindowServiceIde>();
            optionsService.WindowService = windowService;

            windowService.CreateModalForm(Arg.Any<Control>(), Arg.Any<string>()).Returns(x => null);

            ideOptionsService.Show();

            windowService.ReceivedWithAnyArgs(1).CreateModalForm(Arg.Any<Control>(), Arg.Any<string>());
        }

        [Test]
        public void ShowWhenWindowServiceIsAvailableFormReturned()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            ideOptionsService.AddOption<object>();
            IWindowServiceIde windowService = Substitute.For<IWindowServiceIde>();
            optionsService.WindowService = windowService;

            TestForm testForm = new TestForm();
            windowService.CreateModalForm(Arg.Any<Control>(), Arg.Any<string>()).Returns(testForm);

            ideOptionsService.Show();

            windowService.ReceivedWithAnyArgs(1).CreateModalForm(Arg.Any<Control>(), Arg.Any<string>());
        }

        [Test]
        public void SaveOptionsOnApplicationExitWithFileSettingsServiceAvailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            ideOptionsService.AddOption<object>();

            IFileSettingsService fileSettingsService = Substitute.For<IFileSettingsService>();
            optionsService.FileSettingsService = fileSettingsService;

            optionsService.OnApplicationExit(null, EventArgs.Empty);

            fileSettingsService.Received(1).SaveUserSettings(Arg.Any<object>());
        }

        /// <summary>
        /// Negative test
        /// </summary>
        [Test]
        public void SaveOptionsOnApplicationExitWithFileSettingsServiceUnavailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            ideOptionsService.AddOption<object>();

            optionsService.OnApplicationExit(null, EventArgs.Empty);
        }
    }
}
