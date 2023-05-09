using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.Feature;
using Neo.ApplicationFramework.Utilities.Assertion;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Neo.ApplicationFramework.Tools.About
{
    [TestFixture]
    public class FeatureConfigurationViewModelTest
    {
        private IFeatureConfigurationFacade m_FeatureConfigurationFacade;

        [SetUp]
        public void SetUp()
        {
            var list = new List<Guid>();

            m_FeatureConfigurationFacade = Substitute.For<IFeatureConfigurationFacade>();

            m_FeatureConfigurationFacade.GetActivatedFeaturesControlledByRegistry().Returns(list.Select(id => new FeatureDto(id, "name", "friendlyName", "description")));
            m_FeatureConfigurationFacade.AddFeatureKey(Arg.Any<Guid>())
                .Returns(inv => { list.Add((Guid)inv[0]); return true; });
            m_FeatureConfigurationFacade.FeatureActivated(Arg.Any<Guid>()).Returns(false);
        }

        [Test]
        public void TestConstructingWithInsufficientArgs()
        {
            Assert.Throws<AssertException>(() => new FeatureConfigurationViewModel((IFeatureConfigurationFacade)null));
        }

        [Test]
        public void TestConstructingWithInsufficientArgs1()
        {
            Assert.Throws<AssertException>(() => new FeatureConfigurationViewModel((IFeatureSecurityServiceIde)null));
        }

        [Test]
        public void TestInputGuidThatIsCorrect()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade);
            var correctGuid = Guid.NewGuid(); // correct guid should make the add work
            viewModel.InputGuid = correctGuid.ToString();
            viewModel.OkCommand.Execute(null); // execute the command
            Raise.EventWith(m_FeatureConfigurationFacade, EventArgs.Empty);

            Assert.IsTrue(viewModel.Features[0].Id == correctGuid);
            Assert.IsTrue(viewModel.InputGuid == string.Empty); // the command should reset the string
            Assert.IsTrue(viewModel.IsFeatureConfigurationChanged);
        }

        [Test]
        public void TestInputGuidThatIsInCorrect()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade);
            viewModel.InputGuid = "NOT A GUID!";
            Assert.Throws<FormatException>(() => viewModel.OkCommand.Execute(null)); // execute the command
        }

        [Test]
        public void TestInputGuidThatIsInCorrectCheckFlag()
        {
            var viewModel = new FeatureConfigurationViewModel(m_FeatureConfigurationFacade);
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
