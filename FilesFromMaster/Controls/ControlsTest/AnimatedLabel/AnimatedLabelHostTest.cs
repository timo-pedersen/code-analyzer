using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.AnimatedLabel
{
    public class AnimatedLabelHostTest
    {
        private AnimatedLabelHost m_AnimatedLabelHost;

      
        [SetUp]
        public void Setup()
        {
            TestHelper.AddServiceStub<ISecurityServiceCF>();

            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Stub(x => x.Create<IAnimatedLabel>()).Return(new AnimatedLabelControl());
            m_AnimatedLabelHost = new AnimatedLabelHost();
        }

      
        [Test]
        public void SetAnimatedSpeedHigherThanMaximumResultsInAnimatedSpeedNotBeingChanged()
        {
            int animationSpeed = m_AnimatedLabelHost.AnimationSpeed;
            m_AnimatedLabelHost.AnimationSpeed = (int)AnimatedLabelHostCF.MaximumAnimationSpeed + 1;
            Assert.That(m_AnimatedLabelHost.AnimationSpeed, Is.EqualTo(animationSpeed));
        }

        [Test]
        public void SetAnimatedSpeedLowerThanMinimumResultsInAnimatedSpeedNotBeingChanged()
        {
            int animationSpeed = m_AnimatedLabelHost.AnimationSpeed;
            m_AnimatedLabelHost.AnimationSpeed = (int)AnimatedLabelHostCF.MinimumAnimationSpeed - 1;
            Assert.That(m_AnimatedLabelHost.AnimationSpeed, Is.EqualTo(animationSpeed));
        }

    }
}
