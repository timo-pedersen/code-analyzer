using System;
using System.Windows.Forms;
using Core.Api.Feature;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.WindowManagement;
using Neo.ApplicationFramework.Tools.Options;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OptionsManager
{
    [TestFixture]
    public class IDEOptionsServiceTest
    {
        private ILazy<IFeatureSecurityServiceIde> m_FeatureSecurityServiceIde;

        [SetUp]
        public void FixtureSetup()
        {
            m_FeatureSecurityServiceIde = MockRepository.GenerateStub<IFeatureSecurityServiceIde>().ToILazy();
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

            IFileSettingsService fileSettingsService = MockRepository.GenerateMock<IFileSettingsService>();
            optionsService.FileSettingsService = fileSettingsService;

            fileSettingsService.Expect(x => x.LoadUserSettings<object>()).Return(new object()).IgnoreArguments();

            ideOptionsService.AddOption<object>();

            Assert.AreEqual(optionsService.Options.Count, 1);
            fileSettingsService.VerifyAllExpectations();
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

            IWindowServiceIde windowService = MockRepository.GenerateMock<IWindowServiceIde>();
            optionsService.WindowService = windowService;

            windowService.Expect(x => x.CreateModalForm(Arg<Control>.Is.Anything, Arg<string>.Is.Anything)).Return(null).Repeat.Once();

            ideOptionsService.Show();

            windowService.VerifyAllExpectations();
        }

        [Test]
        public void ShowWhenWindowServiceIsAvailableFormReturned()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            ideOptionsService.AddOption<object>();
            IWindowServiceIde windowService = MockRepository.GenerateMock<IWindowServiceIde>();
            optionsService.WindowService = windowService;

            TestForm testForm = new TestForm();
            windowService.Expect(x => x.CreateModalForm(Arg<Control>.Is.Anything, Arg<string>.Is.Anything)).Return(testForm).Repeat.Once();

            ideOptionsService.Show();

            windowService.VerifyAllExpectations();
        }

        [Test]
        public void SaveOptionsOnApplicationExitWithFileSettingsServiceAvailable()
        {
            IIDEOptionsService ideOptionsService = new IDEOptionsService(m_FeatureSecurityServiceIde);
            IDEOptionsService optionsService = ideOptionsService as IDEOptionsService;

            ideOptionsService.AddOption<object>();

            IFileSettingsService fileSettingsService = MockRepository.GenerateMock<IFileSettingsService>();
            optionsService.FileSettingsService = fileSettingsService;

            fileSettingsService.Expect(x => x.SaveUserSettings(Arg<object>.Is.Anything)).Repeat.Once();
            
            optionsService.OnApplicationExit(null, EventArgs.Empty);

            fileSettingsService.VerifyAllExpectations();
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
