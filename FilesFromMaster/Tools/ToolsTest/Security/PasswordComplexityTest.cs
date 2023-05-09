using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class PasswordComplexityTest
    {
        [Test]
        public void MeetCriteriaChecksPasswordLength()
        {
            var securityManager = MockRepository.GenerateStub<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            Assert.IsFalse(passwordComplexity.MeetCriteria("ab"));
            Assert.IsTrue(passwordComplexity.MeetCriteria("abc"));
        }

        [Test]
        public void AssertCriteriaThrowsOnTooSmallPassword()
        {
            var securityManager = MockRepository.GenerateStub<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            Assert.Throws<PasswordComplexityNotMetException>(() => passwordComplexity.AssertPasswordCriteria("ab"));
        }

        [Test]
        public void AssertCriteriaDoesNothingOnCorrectPassword()
        {
            var securityManager = MockRepository.GenerateStub<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            passwordComplexity.AssertPasswordCriteria("abc");
        }
    }
}
