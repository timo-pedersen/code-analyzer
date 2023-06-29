using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TouchComboBox
{
    public class TouchComboBoxHostCFTest
    {
        private TouchComboBoxHostCF m_TouchComboBoxHostCF;

      
        [SetUp]
        public void Setup()
        {
            TestHelper.AddServiceStub<ISecurityServiceCF>();

            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Create<ITouchComboBox>().Returns(new TouchComboBoxControl());
            m_TouchComboBoxHostCF = new TouchComboBoxHostCF();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void SetItemHeightHigherThanMaximumResultsInItemHeihtNotBeingChanged()
        {
            int itemHeight = m_TouchComboBoxHostCF.ItemHeight;
            m_TouchComboBoxHostCF.ItemHeight = (int)TouchComboBoxHostCF.MaximumItemHeight + 1;
            Assert.That(m_TouchComboBoxHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetItemHeightLowerThanMinimumResultsInItemHeightNotBeingChanged()
        {
            int itemHeight = m_TouchComboBoxHostCF.ItemHeight;
            m_TouchComboBoxHostCF.ItemHeight = (int)TouchComboBoxHostCF.MinimumItemHeight - 1;
            Assert.That(m_TouchComboBoxHostCF.ItemHeight, Is.EqualTo(itemHeight));
        }

        [Test]
        public void SetArrowBoxWidthHigherThanMaximumResultsInArrowBoxWidthNotBeingChanged()
        {
            int arrowBoxWidth = m_TouchComboBoxHostCF.ArrowBoxWidth;
            m_TouchComboBoxHostCF.ArrowBoxWidth = (int)TouchComboBoxHostCF.MaximumArrowBoxWidth + 1;
            Assert.That(m_TouchComboBoxHostCF.ArrowBoxWidth, Is.EqualTo(arrowBoxWidth));
        }

        [Test]
        public void SetArrowBoxWidthLowerThanMinimumResultsInArrowBoxWidthNotBeingChanged()
        {
            int arrowBoxWidth = m_TouchComboBoxHostCF.ArrowBoxWidth;
            m_TouchComboBoxHostCF.ArrowBoxWidth = (int)TouchComboBoxHostCF.MinimumArrowBoxWidth - 1;
            Assert.That(m_TouchComboBoxHostCF.ArrowBoxWidth, Is.EqualTo(arrowBoxWidth));
        }

        [Test]
        public void SetScrollBarWidthHigherThanMaximumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollBarWidth = m_TouchComboBoxHostCF.ScrollBarWidth;
            m_TouchComboBoxHostCF.ScrollBarWidth = (int)TouchComboBoxHostCF.MaximumScrollBarWidth + 1;
            Assert.That(m_TouchComboBoxHostCF.ScrollBarWidth, Is.EqualTo(scrollBarWidth));
        }

        [Test]
        public void SetScrollBarWidthLowerThanMinimumResultsInScrollBarWidthNotBeingChanged()
        {
            int scrollbarWidth = m_TouchComboBoxHostCF.ScrollBarWidth;
            m_TouchComboBoxHostCF.ScrollBarWidth = (int)TouchComboBoxHostCF.MinimumScrollBarWidth - 1;
            Assert.That(m_TouchComboBoxHostCF.ScrollBarWidth, Is.EqualTo(scrollbarWidth));
        }

        [Test]
        public void MultiTextPropertyAttributeMapsToValidStringIntervalMapperPC()
        {
            Assert.IsNotNull(StringIntervalHelper.GetMultiTextPropertyMappedStringIntervalMapperPropertyInfo(new Neo.ApplicationFramework.Controls.TouchComboBox.TouchComboBoxHost()));
        }
    }
}
