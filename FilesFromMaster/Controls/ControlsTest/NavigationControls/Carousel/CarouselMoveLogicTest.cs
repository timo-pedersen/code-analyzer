using System.Collections.Generic;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.NavigationControls.Carousel
{
    [TestFixture]
    public class CarouselMoveLogicTest
    {
        private CarouselMoveLogic<string> m_CarouselMoveLogic;

     

        #region Moving Right

        [Test]
        public void NextItemOnRightMoveIsNullWhenNumberOfContainersAreEqualOrGreaterThanNumberOfItems()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveRight();

            Assert.IsNull(m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(-1, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnRightMoveReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnTwoRightMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("C", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(1, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnThreeRightMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("B", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(0, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnFourRightMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("A", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnRightMoveReturnsTheCorrectItemAndIndexWhenStartIndexIsTwo()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 2);

            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(1, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        #endregion

        #region Moving Left

        [Test]
        public void NextItemOnLeftMoveIsNullWhenNumberOfContainersAreEqualOrGreaterThanNumberOfItems()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();

            Assert.IsNull(m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(-1, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnLeftMoveReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(0, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnTwoLeftMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();

            Assert.AreEqual("A", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(1, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnThreeLeftMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();

            Assert.AreEqual("B", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnFourLeftMovesReturnsTheCorrectItemAndIndex()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveLeft();

            Assert.AreEqual("C", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(0, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnLeftMoveReturnsTheCorrectItemAndIndexWhenStartIndexIsTwo()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 2);

            m_CarouselMoveLogic.MoveLeft();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        #endregion

        #region Combined Moving

        [Test]
        public void NextItemOnCombinedMoveReturnsTheCorrectItemAndIndexWhenStartIndexIsZero()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 0);

            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        [Test]
        public void NextItemOnCombinedMoveReturnsTheCorrectItemAndIndexWhenStartIndexIsTwo()
        {
            List<string> carouselItems = new List<string>() { "A", "B", "C", "D" };
            m_CarouselMoveLogic = new CarouselMoveLogic<string>(carouselItems, 3, 2);

            m_CarouselMoveLogic.MoveLeft();
            m_CarouselMoveLogic.MoveRight();
            m_CarouselMoveLogic.MoveRight();

            Assert.AreEqual("D", m_CarouselMoveLogic.ItemToShiftIn);
            Assert.AreEqual(2, m_CarouselMoveLogic.ContainerIndexToShift);
        }

        #endregion
    }
}
