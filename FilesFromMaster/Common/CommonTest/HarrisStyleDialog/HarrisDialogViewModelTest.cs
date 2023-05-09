using System;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.HarrisStyleDialog
{
    [TestFixture]
    public class HarrisDialogViewModelTest
    {
        private HarrisDialogViewModel m_HarrisDialogViewModel;

        [SetUp]
        public void SetUp()
        {
            m_HarrisDialogViewModel = new HarrisDialogViewModel(MockRepository.GenerateStub<IBrandServiceIde>().ToILazy());
        }

        [Test]
        public void DefaultTitleTest()
        {
            // ASSERT
            Assert.AreEqual(m_HarrisDialogViewModel.MainTitle, "MainTitle");
        }

        [Test]
        public void OkTextTest()
        {
            // ASSERT
            Assert.AreEqual(m_HarrisDialogViewModel.OKText, "OK");
        }

        [Test]
        public void CancelTextTest()
        {
            // ASSERT
            Assert.AreEqual(m_HarrisDialogViewModel.CancelText, "Cancel");
        }

        [Test]
        public void SubSubTitleDefaultEmpty()
        {
            // ASSERT
            Assert.IsNull(m_HarrisDialogViewModel.SubSubTitle);
        }
    }
}
