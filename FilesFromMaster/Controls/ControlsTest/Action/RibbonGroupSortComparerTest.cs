using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Action
{
    [TestFixture]
    public class RibbonGroupSortComparerTest
    {
        private RibbonGroupSortComparer m_Comparer;
        private IActionInfo m_ActionInfoX;
        private IActionInfo m_ActionInfoY;

        [SetUp]
        public void SetUp()
        {
            m_Comparer = new RibbonGroupSortComparer();
            m_ActionInfoX = MockRepository.GenerateStub<IActionInfo>();
            m_ActionInfoY = MockRepository.GenerateStub<IActionInfo>();
        }

        [Test]
        public void NoGroupName()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = null;
            m_ActionInfoY.GroupName = null;

            m_ActionInfoX.Name = "Name X";
            m_ActionInfoY.Name = "Name Y";

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void NoGroupNameAndNoName()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = null;
            m_ActionInfoY.GroupName = null;

            m_ActionInfoX.Name = null;
            m_ActionInfoY.Name = null;

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void OnlyXHaveGroupName()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = "Group X";
            m_ActionInfoY.GroupName = null;

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void OnlyYHaveGroupName()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = null;
            m_ActionInfoY.GroupName = "Group X";

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void SameGroupName()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = "Group";
            m_ActionInfoY.GroupName = "Group";

            m_ActionInfoX.Name = "Name X";
            m_ActionInfoY.Name = "Name Y";

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void XIsRecent()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.RecentActionsGroupName;
            m_ActionInfoY.GroupName = "Some group";

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void YIsRecent()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = "Some group";
            m_ActionInfoY.GroupName = TextsIde.RecentActionsGroupName;

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void RecentBeforeNone()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.None;
            m_ActionInfoY.GroupName = TextsIde.RecentActionsGroupName;

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void XIsNone()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = "Group name X";
            m_ActionInfoY.GroupName = "Group name Y";

            m_ActionInfoX
                .Stub(info => info.ActionName)
                .Return(TextsIde.None);
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return("Some group");

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void ScreenBeforeNamed()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.ScreenActionGroup;
            m_ActionInfoY.GroupName = "Group name Y";

            m_ActionInfoX
                .Stub(info => info.ActionName)
                .Return(TextsIde.PrintScreen);
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return(TextsIde.None);

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void NoneBeforeScreen()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.ScreenActionGroup;
            m_ActionInfoY.GroupName = TextsIde.NoneActionGroup;

            m_ActionInfoX
                .Stub(info => info.ActionName)
                .Return(TextsIde.PrintScreen);
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return(TextsIde.None);

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }


        [Test]
        public void NullGroupIsLast()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = null;
            m_ActionInfoY.GroupName = TextsIde.NoneActionGroup;

            m_ActionInfoX
                .Stub(info => info.ActionName)
                .Return(TextsIde.PrintScreen);
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return(TextsIde.None);

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }


        [Test]
        public void XIsScreen()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.ScreenActionGroup;
            m_ActionInfoY.GroupName = "Group name Y";

            m_ActionInfoX
                .Stub(info => info.ActionName)
                .Return(TextsIde.PrintScreen);
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return("Some group");
            
            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void ScreenPreceedsOthersThatAreNotSpcial()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.TagActionGroup;
            m_ActionInfoY.GroupName = TextsIde.ScreenActionGroup;

            m_ActionInfoX
                 .Stub(info => info.ActionName)
                 .Return("X");
            m_ActionInfoY
                .Stub(info => info.ActionName)
                .Return("X");

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void XIsOther()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = TextsIde.OtherActionGroup;
            m_ActionInfoY.GroupName = "Group name Y";

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void YIsOther()
        {
            // ARRANGE
            m_ActionInfoX.GroupName = "Group name Y";
            m_ActionInfoY.GroupName = TextsIde.OtherActionGroup;

            // ACT
            int result = m_Comparer.Compare(m_ActionInfoX, m_ActionInfoY);

            // ASSERT
            Assert.That(result, Is.LessThan(0));
        }
    }
}