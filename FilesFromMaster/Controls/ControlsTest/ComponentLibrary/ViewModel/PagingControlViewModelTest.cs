using System;
using System.ComponentModel;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.ViewModel
{
    [TestFixture]
    public class PagingControlViewModelTest
    {
        private Action<int> m_MockAction;
        private PagingControlViewModel m_UnderTest;

        [SetUp]
        public void SetUp()
        {
            m_MockAction = MockRepository.GenerateMock<Action<int>>();
            m_UnderTest = new PagingControlViewModel(m_MockAction) { TotalPages = 3 };
        }

        [Test]
        [TestCase(1, false, false, true, true)]
        [TestCase(2, true, true, true, true)]
        [TestCase(3, true, true, false, false)]
        public void CommandsShouldUpdateWhenPageNumberChanges(int pageNumber, bool firstEnabled, bool previousEnabled, bool nextEnabled, bool lastEnabled)
        {
            m_UnderTest.PageNumber = pageNumber;

            Assert.AreEqual(firstEnabled, m_UnderTest.FirstPageCommand.CanExecute(null));
            Assert.AreEqual(previousEnabled, m_UnderTest.PreviousPageCommand.CanExecute(null));
            Assert.AreEqual(nextEnabled, m_UnderTest.NextPageCommand.CanExecute(null));
            Assert.AreEqual(lastEnabled, m_UnderTest.LastPageCommand.CanExecute(null));
        }

        [Test]
        [TestCase(0, Visibility.Collapsed)]
        [TestCase(1, Visibility.Collapsed)]
        [TestCase(2, Visibility.Visible)]
        public void ControlShouldBeVisibleWhenMoreThanOnPage(int totalPages, Visibility expected)
        {
            m_UnderTest.TotalPages = totalPages;

            Assert.AreEqual(expected, m_UnderTest.Visible);
        }

        [Test]
        public void ShouldFireNotifyPropertyChangedWhenSettingProperties()
        {
            Assert.That(m_UnderTest.NotifiesOn(x => x.PageNumber).When(x => x.PageNumber = 1));
            Assert.That(m_UnderTest.NotifiesOn(x => x.TotalPages).When(x => x.TotalPages = 1));
            Assert.That(m_UnderTest.NotifiesOn(x => x.Visible).When(x => x.Visible = Visibility.Visible));
        }

        [Test]
        public void FirstPageCommandShouldNavigateToFirstPage()
        {
            // Arrange
            m_MockAction.Expect(x => x.Invoke(1));
            m_UnderTest.PageNumber = 3;

            // Act
            m_UnderTest.FirstPageCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, m_UnderTest.PageNumber);
            m_MockAction.VerifyAllExpectations();
        }

        [Test]
        public void PreviousPageCommandShouldNavigateToPreviousPage()
        {
            // Arrange
            m_MockAction.Expect(x => x.Invoke(2));
            m_UnderTest.PageNumber = 3;

            // Act
            m_UnderTest.PreviousPageCommand.Execute(null);

            // Assert
            Assert.AreEqual(2, m_UnderTest.PageNumber);
            m_MockAction.VerifyAllExpectations();
        }

        [Test]
        public void NextPageCommandShouldNavigateToNextPage()
        {
            // Arrange
            m_MockAction.Expect(x => x.Invoke(2));
            m_UnderTest.PageNumber = 1;

            // Act
            m_UnderTest.NextPageCommand.Execute(null);

            // Assert
            Assert.AreEqual(2, m_UnderTest.PageNumber);
            m_MockAction.VerifyAllExpectations();
        }

        [Test]
        public void LastPagecommandShouldNavigateToLastPage()
        {
            // Arrange
            m_MockAction.Expect(x => x.Invoke(3));
            m_UnderTest.PageNumber = 1;

            // Act
            m_UnderTest.LastPageCommand.Execute(null);

            // Assert
            Assert.AreEqual(3, m_UnderTest.PageNumber);
            m_MockAction.VerifyAllExpectations();
        }
    }
}