using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TouchListBox
{
    public class TouchListBoxHostCFTest
    {
        private TouchListBoxHostCF m_TouchListBoxHostCF;

        [SetUp]
        public void Setup()
        {
            var checkBoxCell = new CheckBoxCell();
            var touchListBoxControl = new TouchListBoxControl();
            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Create<ICheckBoxCell>().Returns(checkBoxCell);
            platformFactoryServiceStub.Create<ITouchListBox>().Returns(touchListBoxControl);

            m_TouchListBoxHostCF = new TouchListBoxHostCF();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }
        
        [Test]
        public void SetItemHeightHigherThanMaximumResultsInItemHeightNotBeingChanged()
        {
            int itemHeight = m_TouchListBoxHostCF.ItemHeight;
            m_TouchListBoxHostCF.ItemHeight = (int)TouchListBoxHostCF.MaximumItemHeight + 1;
            Assert.That(m_TouchListBoxHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetItemHeightLowerThanMinimumResultsInItemHeightNotBeingChanged()
        {
            int itemHeight = m_TouchListBoxHostCF.ItemHeight;
            m_TouchListBoxHostCF.ItemHeight = (int)TouchListBoxHostCF.MinimumItemHeight - 1;
            Assert.That(m_TouchListBoxHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetScrollBarWidthHigherThanMaximumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollBarWidth = m_TouchListBoxHostCF.ScrollBarWidth;
            m_TouchListBoxHostCF.ScrollBarWidth = (int)TouchListBoxHostCF.MaximumScrollBarWidth + 1;
            Assert.That(m_TouchListBoxHostCF.ScrollBarWidth, Is.EqualTo(scrollBarWidth));
        }

        [Test]
        public void SetScrollBarWidthLowerThanMinimumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollbarWidth = m_TouchListBoxHostCF.ScrollBarWidth;
            m_TouchListBoxHostCF.ScrollBarWidth = (int)TouchListBoxHostCF.MinimumScrollBarWidth - 1;
            Assert.That(m_TouchListBoxHostCF.ScrollBarWidth, Is.EqualTo(scrollbarWidth));
        }
    }
}
