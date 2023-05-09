using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ActionMenu
{
    public class ActionMenuHostCFTest
    {
        private ActionMenuHostCF m_ActionMenuHostCF;

        [SetUp]
        public void Setup()
        {
            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Create<IActionMenu>().Returns(new ActionMenuControl());
            m_ActionMenuHostCF = new ActionMenuHostCF();
        }
        
        [Test]
        public void SetItemWidthHigherThanMaximumResultsInItemWidthNotBeingChanged()
        {
            int itemWidth = m_ActionMenuHostCF.ItemWidth;
            m_ActionMenuHostCF.ItemWidth = (int)ActionMenuHostCF.MaximumItemWidth + 1;
            Assert.That(m_ActionMenuHostCF.ItemWidth, Is.EqualTo(itemWidth));
        }

        [Test]
        public void SetItemWidthLowerThanMinimumResultsInItemWidthNotBeingChanged()
        {
            int itemWidth = m_ActionMenuHostCF.ItemWidth;
            m_ActionMenuHostCF.ItemWidth = (int)ActionMenuHostCF.MinimumItemWidth - 1;
            Assert.That(m_ActionMenuHostCF.ItemWidth, Is.EqualTo(itemWidth));
        }

        [Test]
        public void SetItemHeightHigherThanMaximumResultsInItemHeightNotBeingChanged()
        {
            int itemHeight = m_ActionMenuHostCF.ItemHeight;
            m_ActionMenuHostCF.ItemHeight = (int)ActionMenuHostCF.MaximumItemHeight + 1;
            Assert.That(m_ActionMenuHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetItemHeightLowerThanMinimumResultsInItemHeightNotBeingChanged()
        {
            int itemHeight = m_ActionMenuHostCF.ItemHeight;
            m_ActionMenuHostCF.ItemHeight = (int)ActionMenuHostCF.MinimumItemHeight - 1;
            Assert.That(m_ActionMenuHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetImageWidthHigherThanMaximumResultsInImageWidthNotBeingChanged()
        {
            int imageWidth = m_ActionMenuHostCF.ImageWidth;
            m_ActionMenuHostCF.ImageWidth = (int)ActionMenuHostCF.MaximumImageWidth + 1;
            Assert.That(m_ActionMenuHostCF.ImageWidth, Is.EqualTo(imageWidth));
        }

        [Test]
        public void SetImageWidthLowerThanMinimumResultsInImageWidthNotBeingChanged()
        {
            int imageWidth = m_ActionMenuHostCF.ImageWidth;
            m_ActionMenuHostCF.ImageWidth = (int)ActionMenuHostCF.MinimumImageWidth - 1;
            Assert.That(m_ActionMenuHostCF.ImageWidth, Is.EqualTo(imageWidth));
        }

        [Test]
        public void SetImageHeightHigherThanMaximumResultsInImageHeightNotBeingChanged()
        {
            int imageHeight = m_ActionMenuHostCF.ImageHeight;
            m_ActionMenuHostCF.ImageHeight = (int)ActionMenuHostCF.MaximumImageHeight + 1;
            Assert.That(m_ActionMenuHostCF.ImageHeight, Is.EqualTo(imageHeight));
        }

        [Test]
        public void SetImageHeightLowerThanMinimumResultsInImageHeightNotBeingChanged()
        {
            int imageHeight = m_ActionMenuHostCF.ImageHeight;
            m_ActionMenuHostCF.ImageHeight = (int)ActionMenuHostCF.MinimumImageWidth - 1;
            Assert.That(m_ActionMenuHostCF.ImageHeight, Is.EqualTo(imageHeight));
        }

        [Test]
        public void SetScrollBarWidthHigherThanMaximumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollBarWidth = m_ActionMenuHostCF.ScrollBarWidth;
            m_ActionMenuHostCF.ScrollBarWidth = (int)ActionMenuHostCF.MaximumScrollBarWidth + 1;
            Assert.That(m_ActionMenuHostCF.ScrollBarWidth, Is.EqualTo(scrollBarWidth));
        }

        [Test]
        public void SetScrollBarWidthLowerThanMinimumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollBarWidth = m_ActionMenuHostCF.ScrollBarWidth;
            m_ActionMenuHostCF.ScrollBarWidth = (int)ActionMenuHostCF.MinimumScrollBarWidth - 1;
            Assert.That(m_ActionMenuHostCF.ScrollBarWidth, Is.EqualTo(scrollBarWidth));
        }
    }
}
