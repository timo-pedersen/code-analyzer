using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Utilities.Assertion;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Build.BuildManager.Implementations;
using NUnit.Framework;
using Rhino.Mocks;
using Assert = NUnit.Framework.Assert;

namespace Neo.ApplicationFramework.Tools.About
{
    [TestFixture]
    public class FeatureConfigurationViewModelTest
    {
        private IFeatureConfigurationFacade m_FeatureConfigurationFacade;
        private ILazy<IProjectManager> m_ProjectManagerStub;

        [SetUp]
        public void SetUp()
        {
            var list = new List<Guid>();

            m_FeatureConfigurationFacade = MockRepository.GenerateStub<IFeatureConfigurationFacade>();
            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>().ToILazy();

            m_FeatureConfigurationFacade.Stub(inv => inv.GetActivatedFeaturesControlledByRegistry()).Return(list.Select(id => new FeatureDto(id, "name", "friendlyName", "description")));
            m_FeatureConfigurationFacade.Stub(inv => inv.AddFeatureKey(Arg<Guid>.Is.Anything)).WhenCalled(inv => { list.Add((Guid)inv.Arguments[0]); inv.ReturnValue = true; }).Return(true);
            m_FeatureConfigurationFacade.Stub(inv => inv.FeatureActivated(Arg<Guid>.Is.Anything)).Return(false);
        }

        [Test]
        public void TestConstructingWithInsufficientArgs()
        {
            Assert.Throws<AssertException>(() => new FeatureConfigurationViewModel(null, null));
        }

        [Test]
        public void TestConstructingWithInsufficientArgs1()
        {
            Assert.Throws<AssertException>(() => new FeatureConfigurationViewModel(null));
        }

        [Test]
        public void TestInputGuidThatIsCorrect()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade, m_ProjectManagerStub);
            var correctGuid = Guid.NewGuid(); // correct guid should make the add work
            viewModel.InputGuid = correctGuid.ToString();
            viewModel.OkCommand.Execute(null); // execute the command
            m_FeatureConfigurationFacade.Raise(f => f.FeaturesChanged += null, m_FeatureConfigurationFacade, new GuidEventArgs(new Guid()));

            Assert.IsTrue(viewModel.Features[0].Id == correctGuid);
            Assert.IsTrue(viewModel.InputGuid == string.Empty); // the command should reset the string
            Assert.IsTrue(viewModel.IsFeatureConfigurationChanged);
        }

        [Test]
        public void TestInputGuidThatIsInCorrect()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade, m_ProjectManagerStub);
            viewModel.InputGuid = "NOT A GUID!";
            Assert.Throws<FormatException>(() => viewModel.OkCommand.Execute(null)); // execute the command
        }

        [Test]
        public void TestInputGuidThatIsInCorrectCheckFlag()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade, m_ProjectManagerStub);
            viewModel.InputGuid = "NOT A GUID!";
            try
            {
                viewModel.OkCommand.Execute(null); // execute the command
            }
            catch (FormatException) { }

            Assert.IsFalse(viewModel.IsFeatureConfigurationChanged);
        }
    }
}
