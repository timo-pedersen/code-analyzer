using System.Windows;
using System.Windows.Controls;
using Neo.ApplicationFramework.Common.Utilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls
{
    [TestFixture]
    public class DescriptionTextBoxTest
    {
        private const string EmptyText = "Enter...";
        private DescriptionTextBox m_DescriptionTextBox;
        private FrameworkElement m_EmptyDescriptionTextBlock;

        [SetUp]
        public void Setup()
        {
            m_DescriptionTextBox = new DescriptionTextBox();
            m_DescriptionTextBox.BeginInit();
            m_DescriptionTextBox.EndInit();
            m_DescriptionTextBox.ApplyTemplate();
            m_EmptyDescriptionTextBlock = VisualTreeNavigator.FindElementOfType(m_DescriptionTextBox, typeof(TextBlock));
        }

        [Test]
        public void IsEmptyAfterDefiningEmptyText()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;

            Assert.IsTrue(m_DescriptionTextBox.IsEmpty);
        }

        [Test]
        public void IsNotEmptyWhenContentSet()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;
            m_DescriptionTextBox.Text = "Text";

            Assert.IsFalse(m_DescriptionTextBox.IsEmpty);
            Assert.AreEqual("Text", m_DescriptionTextBox.Text);
        }

        [Test]
        public void EmptyStyleIsAppliedWhenEmpty()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;

            Assert.AreEqual(Visibility.Visible, m_EmptyDescriptionTextBlock.Visibility);
        }

        [Test]
        public void NormalStyleIsAppliedWhenNotEmpty()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;
            m_DescriptionTextBox.Text = "Text";

            Assert.AreEqual(Visibility.Collapsed, m_EmptyDescriptionTextBlock.Visibility);
        }

        [Test]
        public void IsEmptyWhenContentCleared()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;
            m_DescriptionTextBox.Text = "Text";
            m_DescriptionTextBox.Text = string.Empty;

            Assert.IsTrue(m_DescriptionTextBox.IsEmpty);
            Assert.AreEqual(string.Empty, m_DescriptionTextBox.Text);
        }

        [Test]
        public void NotEmptyWhenWritingEmptyText()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;
            m_DescriptionTextBox.Text = string.Empty;
            m_DescriptionTextBox.Text = EmptyText;

            Assert.IsFalse(m_DescriptionTextBox.IsEmpty);
            Assert.AreEqual(EmptyText, m_DescriptionTextBox.Text);
        }

        [Test]
        public void IsEmptyWhenNoContentAndNoEmptyTextSpecified()
        {
            Assert.IsTrue(m_DescriptionTextBox.IsEmpty);

            Assert.AreEqual(string.Empty, m_DescriptionTextBox.Text);
        }

        [Test]
        public void IsNotEmptyWhenContentSetAndNoEmptyTextSpecified()
        {
            m_DescriptionTextBox.Text = "Text";

            Assert.IsFalse(m_DescriptionTextBox.IsEmpty);
            Assert.AreEqual("Text", m_DescriptionTextBox.Text);
        }

        [Test]
        public void IsEmptyWhenContentClearedAndNoEmptyTextSpecified()
        {
            m_DescriptionTextBox.Text = "Text";
            m_DescriptionTextBox.Text = string.Empty;

            Assert.IsTrue(m_DescriptionTextBox.IsEmpty);
            Assert.AreEqual(string.Empty, m_DescriptionTextBox.Text);
        }

        [Test]
        public void TextDoesNotReturnEmptyTextWhenContentCleared()
        {
            m_DescriptionTextBox.EmptyText = EmptyText;
            m_DescriptionTextBox.Text = string.Empty;

            Assert.AreEqual(string.Empty, m_DescriptionTextBox.Text);
        }
    }
}
