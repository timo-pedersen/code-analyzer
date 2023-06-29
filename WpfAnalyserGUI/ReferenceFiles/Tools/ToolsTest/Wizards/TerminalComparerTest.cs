using System.Collections;
using System.Windows;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Wizards
{
    [TestFixture]
    public class TerminalComparerTest
    {
        private const int FirstItemHigherPriority = -1;
        private const int SecondItemHigherPriority = 1;
        private ITerminal m_TerminalAStub;
        private ITerminal m_TerminalBStub;
        private IComparer m_Comparer;

        [SetUp]
        public void SetUp()
        {
            m_Comparer = new TerminalComparer();

            m_TerminalAStub = Substitute.For<ITerminal>();
            m_TerminalBStub = Substitute.For<ITerminal>();
        }

        [Test]
        public void NullReturnEqualWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(true);

            Assert.That(m_Comparer.Compare(null, m_TerminalAStub), Is.EqualTo(0));
        }

        [Test]
        public void NullReturnEqualWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(true);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, 0), Is.EqualTo(0));
        }

        [Test]
        public void PCPanelHasHigherPriorityThanPanelWhenPCPanelIsFirst()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalBStub.IsPC.Returns(false);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void PCPanelHasHigherPriorityThanPanelWhenPCPanelIsLast()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalBStub.IsPC.Returns(false);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }



        #region PCs

        [Test]
        public void SortingGroupForPCHasHigherPriorityWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.SortingPriority.Returns(1);
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.SortingPriority.Returns(10);
            m_TerminalBStub.ScreenSize = new Size(10, 10);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void SortingGroupForPCHasHigherPriorityWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.SortingPriority.Returns(1);
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.SortingPriority.Returns(10);
            m_TerminalBStub.ScreenSize = new Size(10, 10);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeHasHigherPriorityForPCPanelsWhenLargestFirst()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.ScreenSize = new Size(10, 10);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void ScreenSizeHasHigherPriorityForPCPanelsWhenLargestLast()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.ScreenSize = new Size(10, 10);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeEqualForPCPanelsSortsDecreasingAlphabeticallyWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.Name.Returns("BPanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeEqualForPCPanelsSortsDecreasingAlphabeticallyWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.Name.Returns("BPanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void ReturnsEqualWhenPCsEqual()
        {
            m_TerminalAStub.IsPC.Returns(true);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(true);
            m_TerminalBStub.Name.Returns("APanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(0));
        }

        #endregion

        #region Panels

        [Test]
        public void SortingGroupForPanelHasHigherPriorityWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.SortingPriority.Returns(1);
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.SortingPriority.Returns(10);
            m_TerminalBStub.ScreenSize = new Size(10, 10);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void SortingGroupForPanelHasHigherPriorityWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.SortingPriority.Returns(1);
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.SortingPriority.Returns(10);
            m_TerminalBStub.ScreenSize = new Size(10, 10);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeHasHigherPriorityForPanelsWhenLargestFirst()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.ScreenSize = new Size(10, 10);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void ScreenSizeHasHigherPriorityForPanelsWhenLargestLast()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.ScreenSize = new Size(10, 10);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeEqualForPanelsSortsDecreasingAlphabeticallyWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.Name.Returns("BPanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(SecondItemHigherPriority));
        }

        [Test]
        public void ScreenSizeEqualForPanelsSortsDecreasingAlphabeticallyWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.Name.Returns("BPanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void ReturnsEqualWhenPanelsEqual()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.Name.Returns("APanel");
            m_TerminalAStub.ScreenSize = new Size(5, 5);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.Name.Returns("APanel");
            m_TerminalBStub.ScreenSize = new Size(5, 5);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(0));
        }

        [Test]
        public void TouchPanelHasHigherPriorityThanKeyPanelWhenFirst()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.IsKeyPanel.Returns(false);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.IsKeyPanel.Returns(true);

            Assert.That(m_Comparer.Compare(m_TerminalAStub, m_TerminalBStub), Is.EqualTo(FirstItemHigherPriority));
        }

        [Test]
        public void TouchPanelHasHigherPriorityThanKeyPanelWhenLast()
        {
            m_TerminalAStub.IsPC.Returns(false);
            m_TerminalAStub.IsKeyPanel.Returns(false);
            m_TerminalBStub.IsPC.Returns(false);
            m_TerminalBStub.IsKeyPanel.Returns(true);

            Assert.That(m_Comparer.Compare(m_TerminalBStub, m_TerminalAStub), Is.EqualTo(SecondItemHigherPriority));
        }

        #endregion

    }
}
