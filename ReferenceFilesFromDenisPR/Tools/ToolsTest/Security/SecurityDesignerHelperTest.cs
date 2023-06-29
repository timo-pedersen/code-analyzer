#if !VNEXT_TARGET
using System.ComponentModel;
using Core.TestUtilities.Utilitites;
using Neo.ApplicationFramework.Common.Security;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityDesignerHelperTest
    {
        private SecurityDesignerHelper m_SecurityDesignerHelper;
        private ISecurityService m_SecurityServiceStub;

        [SetUp]
        public void SetUp()
        {
            m_SecurityServiceStub = TestHelper.CreateAndAddServiceStub<ISecurityService>();
            m_SecurityDesignerHelper = new SecurityDesignerHelper(new LazyWrapper<ISecurityService>(() => m_SecurityServiceStub));
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void NextSecurityGroupFoundWhenAFewGroupsExistsTest()
        {
            // ARRANGE
            BindingList<ISecurityGroup> securityGroupsList = new BindingList<ISecurityGroup>
            {
                new SecurityGroup("operatorGroup", SecurityGroups.Group_01),
                new SecurityGroup("bananaGroup", SecurityGroups.Group_03),
                new SecurityGroup("visitorGroup", SecurityGroups.Group_04)
            };

            m_SecurityServiceStub.Groups.Returns(securityGroupsList);

            // ACT
            SecurityGroups nextSecurityGroup = m_SecurityDesignerHelper.GetNextSecurityGroupsEnum();

            // ASSERT
            Assert.IsTrue(nextSecurityGroup == SecurityGroups.Group_02, "Wrong SecurityGroup created");
        }

        [Test]
        public void GetFirstSecurityGroupWhenNoGroupsAreAddedTest()
        {
            // ARRANGE
            BindingList<ISecurityGroup> securityGroupsList = new BindingList<ISecurityGroup>();
            m_SecurityServiceStub.Groups.Returns(securityGroupsList);

            // ACT
            SecurityGroups nextSecurityGroup = m_SecurityDesignerHelper.GetNextSecurityGroupsEnum();

            // ASSERT
            Assert.IsTrue(nextSecurityGroup == SecurityGroups.Group_01, "Wrong SecurityGroup created");
        }


        [Test]
        public void FirstGroupIsReturnedForNonEmptyGroupListThatLacksTheFirstOneTest()
        {
            // ARRANGE
            BindingList<ISecurityGroup> securityGroupsList = new BindingList<ISecurityGroup>
            {
                new SecurityGroup("bananaGroup", SecurityGroups.Group_03),
                new SecurityGroup("visitorGroup", SecurityGroups.Group_04)
            };

            m_SecurityServiceStub.Groups.Returns(securityGroupsList);

            // ACT
            SecurityGroups nextSecurityGroup = m_SecurityDesignerHelper.GetNextSecurityGroupsEnum();

            // ASSERT
            Assert.IsTrue(nextSecurityGroup == SecurityGroups.Group_01, "Wrong SecurityGroup created");
        }

        [Test]
        public void SecurityGroupNoneIsReturnedWhenAllSecurityGroupsAreUsedTest()
        {
            // ARRANGE
            BindingList<ISecurityGroup> securityGroupsList = new BindingList<ISecurityGroup>
            {
                new SecurityGroup("operatorGroup", SecurityGroups.Group_01),
                new SecurityGroup("specialGroup", SecurityGroups.Group_02),
                new SecurityGroup("bananaGroup", SecurityGroups.Group_03),
                new SecurityGroup("visitorGroup", SecurityGroups.Group_04),
                new SecurityGroup("appleGroup", SecurityGroups.Group_05),
                new SecurityGroup("orangeGroup", SecurityGroups.Group_06),
                new SecurityGroup("pineappleGroup", SecurityGroups.Group_07),
                new SecurityGroup("lemonGroup", SecurityGroups.Group_08),
                new SecurityGroup("pearGroup", SecurityGroups.Group_09),
                new SecurityGroup("plumGroup", SecurityGroups.Group_10),
                new SecurityGroup("cherryGroup", SecurityGroups.Group_11),
                new SecurityGroup("raspberryGroup", SecurityGroups.Group_12),
                new SecurityGroup("strawberryGroup", SecurityGroups.Group_13),
                new SecurityGroup("blackberryGroup", SecurityGroups.Group_14),
                new SecurityGroup("blueberryGroup", SecurityGroups.Group_15),
                new SecurityGroup("mangoGroup", SecurityGroups.Group_16),
                new SecurityGroup("limeGroup", SecurityGroups.Group_17),
                new SecurityGroup("clementineGroup", SecurityGroups.Group_18),
                new SecurityGroup("kiwiGroup", SecurityGroups.Group_19),
                new SecurityGroup("coconutGroup", SecurityGroups.Group_20),
                new SecurityGroup("starfruitGroup", SecurityGroups.Group_21),
                new SecurityGroup("peachGroup", SecurityGroups.Group_22),
                new SecurityGroup("grapefruitGroup", SecurityGroups.Group_23),
                new SecurityGroup("grapesGroup", SecurityGroups.Group_24),
                new SecurityGroup("nectarineGroup", SecurityGroups.Group_25),
                new SecurityGroup("watermelonGroup", SecurityGroups.Group_26),
                new SecurityGroup("apricotGroup", SecurityGroups.Group_27),
                new SecurityGroup("passionfruitGroup", SecurityGroups.Group_28),
                new SecurityGroup("papayaGroup", SecurityGroups.Group_29),
                new SecurityGroup("guavaGroup", SecurityGroups.Group_30),
                new SecurityGroup("rhubarbGroup", SecurityGroups.Group_31)
            };

            m_SecurityServiceStub.Groups.Returns(securityGroupsList);

            // ACT
            SecurityGroups nextSecurityGroup = m_SecurityDesignerHelper.GetNextSecurityGroupsEnum();

            // ASSERT
            Assert.IsTrue(nextSecurityGroup == SecurityGroups.None, "Wrong SecurityGroup created");
        }
    }
}
#endif
