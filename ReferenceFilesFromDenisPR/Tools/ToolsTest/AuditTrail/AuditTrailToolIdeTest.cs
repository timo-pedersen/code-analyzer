#if !VNEXT_TARGET
using System.Linq;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.AuditTrail
{
    [TestFixture]
    public class AuditTrailToolIdeTest
    {
        [Test]
        public void RuntimeToolsTest()
        {
            // ARRANGE
            var auditTrailToolIde = new AuditTrailToolIde();
            
            // ACT
            var res = auditTrailToolIde.RuntimeTools;

            // ASSERT
            Assert.AreEqual(res.First(), typeof(UserChangedAuditTrailToolCF));
        }
    }
}
#endif
