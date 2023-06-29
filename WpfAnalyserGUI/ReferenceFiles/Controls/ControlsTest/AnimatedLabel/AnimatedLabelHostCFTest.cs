using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.AnimatedLabel
{
    public class AnimatedLabelHostCFTest
    {
        private AnimatedLabelHostCF m_AnimatedLabelHostCF;

       
        [SetUp]
        public void Setup()
        {
            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Create<IAnimatedLabel>().Returns(new AnimatedLabelControl());

            m_AnimatedLabelHostCF = new AnimatedLabelHostCF();
        }

    
        [Test]
        public void SetAnimatedSpeedHigherThanMaximumResultsInAnimatedSpeedNotBeingChanged()
        {
            int animationSpeed = m_AnimatedLabelHostCF.AnimationSpeed;
            m_AnimatedLabelHostCF.AnimationSpeed = (int)AnimatedLabelHostCF.MaximumAnimationSpeed + 1;
            Assert.That(m_AnimatedLabelHostCF.AnimationSpeed, Is.EqualTo(animationSpeed));
        }

        [Test]
        public void SetAnimatedSpeedLowerThanMinimumResultsInAnimatedSpeedNotBeingChanged()
        {
            int animationSpeed = m_AnimatedLabelHostCF.AnimationSpeed;
            m_AnimatedLabelHostCF.AnimationSpeed = (int)AnimatedLabelHostCF.MinimumAnimationSpeed - 1;
            Assert.That(m_AnimatedLabelHostCF.AnimationSpeed, Is.EqualTo(animationSpeed));
        }

    }
}
