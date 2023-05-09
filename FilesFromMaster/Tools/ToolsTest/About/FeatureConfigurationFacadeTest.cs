using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.Feature;
using Neo.ApplicationFramework.Common.Features;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.About
{
    [TestFixture]
    public class FeatureConfigurationFacadeTest
    {
        private IFeatureSecurityServiceIde m_FeatureSecurityServiceIdeStub;

        [SetUp]
        public void SetUp()
        {
            m_FeatureSecurityServiceIdeStub = MockRepository.GenerateStub<IFeatureSecurityServiceIde>();
        }

        [Test]
        public void OnlyActivatedFeaturesAreReturned()
        {
            // ARRANGE
            var featureList = new List<ISecuredFeature>();
            ISecuredFeature featureActive = new TestFeatureActive();
            ISecuredFeature featureInactive = new TestFeatureInactive();
            featureList.Add(featureActive);
            featureList.Add(featureInactive);

            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.GetAllFeatures()).Return(featureList);
            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.GetAllFeaturesNotBranded()).Return(featureList);
            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.IsActivated(typeof(TestFeatureActive))).Return(true);
            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.IsActivated(typeof(TestFeatureInactive))).Return(false);

            // ACT
            var featureConfigurationFacade = new FeatureConfigurationFacade(m_FeatureSecurityServiceIdeStub);
            var featureDtoList = featureConfigurationFacade.GetActivatedFeaturesControlledByRegistry().ToList();

            // ASSERT
            Assert.IsTrue(featureDtoList.Count == 1);
            Assert.AreEqual(featureDtoList[0].FriendlyName, featureActive.FriendlyName);
        }

        [Test]
        public void AddFeatureKeyFiresFeaturesChanged()
        {
            // ARRANGE
            bool eventWasFired = false;
            ISecuredFeature testFeature1 = new TestFeatureActive();

            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.AddFeatureKey(testFeature1.InternalId)).Return(true);
            var featureConfigurationFacade = new FeatureConfigurationFacade(m_FeatureSecurityServiceIdeStub);
            featureConfigurationFacade.FeaturesChanged += (sender, e) => eventWasFired = true;

            // ACT
            featureConfigurationFacade.AddFeatureKey(testFeature1.InternalId);

            // ASSERT
            Assert.IsTrue(eventWasFired);
        }

        [Test]
        public void RemoveFeatureFiresFeaturesChanged()
        {
            // ARRANGE
            bool eventWasFired = false;
            ISecuredFeature testFeature1 = new TestFeatureActive();

            m_FeatureSecurityServiceIdeStub.Stub(fss => fss.RemoveFeature(testFeature1.InternalId)).Return(true);
            var featureConfigurationFacade = new FeatureConfigurationFacade(m_FeatureSecurityServiceIdeStub);
            featureConfigurationFacade.FeaturesChanged += (sender, e) => eventWasFired = true;

            // ACT
            featureConfigurationFacade.RemoveFeature(testFeature1.InternalId);

            // ASSERT
            Assert.IsTrue(eventWasFired);
        }
    }

    #region Helper classes

    class TestFeatureActive : SecuredFeature
    {
        /// <summary>
        /// Run time-features should always be active!
        /// Default ctor is needed for xml-deserialization
        /// </summary>
        public TestFeatureActive()
            : base(true, true)
        {
        }

        protected override Guid InternalId
        {
            get { return new Guid("3E106D5D-10D7-489A-A5A6-0F0967F15CE9"); }
        }

        protected override int ExternalId
        {
            get { return -1; }
        }

        protected override SecurityLevel RequiredSecurityLevel
        {
            get { return SecurityLevel.Low; }
        }

        protected override string FriendlyName
        {
            get { return "TestFeatureActive_FriendlyName"; }
        }

        protected override string Description
        {
            get { return "TestFeatureActive_Desc"; }
        }

        protected override bool IsEssential
        {
            get { return true; }
        }

        protected override bool RuntimeActivated
        {
            get { return false; }
        }
    }

    class TestFeatureInactive : SecuredFeature
    {
        /// <summary>
        /// Run time-features should always be active!
        /// Default ctor is needed for xml-deserialization
        /// </summary>
        public TestFeatureInactive()
            : base(true, true)
        {
        }

        protected override Guid InternalId
        {
            get { return new Guid(); }
        }

        protected override int ExternalId
        {
            get { return -1; }
        }

        protected override SecurityLevel RequiredSecurityLevel
        {
            get { return SecurityLevel.Low; }
        }

        protected override string FriendlyName
        {
            get { return "TestFeatureInactive_FriendlyName"; }
        }

        protected override string Description
        {
            get { return "TestFeatureInactive_Desc"; }
        }

        protected override bool IsEssential
        {
            get { return true; }
        }

        protected override bool RuntimeActivated
        {
            get { return false; }
        }
    }

    #endregion
}