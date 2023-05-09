﻿using Core.Api.DI.PlatformFactory;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.TouchListBox
{
    public class TouchListBoxHostCFTest
    {
        private TouchListBoxHostCF m_TouchListBoxHostCF;
        
        [SetUp]
        public void Setup()
        {
            IPlatformFactoryService platformFactoryServiceStub = TestHelper.AddServiceStub<IPlatformFactoryService>();
            platformFactoryServiceStub.Stub(x => x.Create<ICheckBoxCell>()).Return(new CheckBoxCell());
            platformFactoryServiceStub.Stub(x => x.Create<ITouchListBox>()).Return(new TouchListBoxControl());
            m_TouchListBoxHostCF = new TouchListBoxHostCF();
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
