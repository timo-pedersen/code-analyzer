using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class PasswordComplexityTest
    {
        [Test]
        public void MeetCriteriaChecksPasswordLength()
        {
            var securityManager = Substitute.For<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            Assert.IsFalse(passwordComplexity.MeetCriteria("ab"));
            Assert.IsTrue(passwordComplexity.MeetCriteria("abc"));
        }

        [Test]
        public void AssertCriteriaThrowsOnTooSmallPassword()
        {
            var securityManager = Substitute.For<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            Assert.Throws<PasswordComplexityNotMetException>(() => passwordComplexity.AssertPasswordCriteria("ab"));
        }

        [Test]
        public void AssertCriteriaDoesNothingOnCorrectPassword()
        {
            var securityManager = Substitute.For<ISecurityManager>();
            securityManager.MinimumPasswordLength = 3;
            PasswordComplexity passwordComplexity = new PasswordComplexity(securityManager);

            passwordComplexity.AssertPasswordCriteria("abc");
        }
    }
}
