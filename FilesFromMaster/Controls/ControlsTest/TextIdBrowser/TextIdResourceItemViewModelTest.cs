using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.TextIdBrowser
{
    [TestFixture]
    public class TextIdResourceItemViewModelTest
    {
        private ITextIDResourceItem m_TextIdResourceItem;


        [SetUp]
        public void SetUp()
        {
            m_TextIdResourceItem = MockRepository.GenerateStub<ITextIDResourceItem>();
        }


        [Test]
        public void Id()
        {
            // ARRANGE
            m_TextIdResourceItem.TextID = 23;

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ASSERT
            Assert.That(viewModel.Id, Is.EqualTo(23));
        }


        [Test]
        public void DefaultText()
        {
            // ARRANGE
            m_TextIdResourceItem.ReferenceValue = "Default Text";

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ASSERT
            Assert.That(viewModel.DefaultText, Is.EqualTo("Default Text"));
        }


        [Test]
        public void Translations()
        {
            // ARRANGE
            m_TextIdResourceItem
                .Stub(textIdResourceItem => textIdResourceItem.LanguageValues)
                .Return(new Dictionary<string, object>
                    {
                        { "some language 1", "some translation 1"},
                        { "some language 2", "some translation 2"},
                        { "some language 3", "some translation 3"}
                    });

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ASSERT
            Assert.That(viewModel.Translations.Count(), Is.EqualTo(3));
            Assert.That(viewModel.Translations.ElementAt(0), Is.EqualTo("some translation 1"));
            Assert.That(viewModel.Translations.ElementAt(1), Is.EqualTo("some translation 2"));
            Assert.That(viewModel.Translations.ElementAt(2), Is.EqualTo("some translation 3"));
        }


        [Test]
        public void GetKnownTranslations()
        {
            // ARRANGE
            m_TextIdResourceItem
                .Stub(textIdResourceItem => textIdResourceItem.LanguageValues)
                .Return(new Dictionary<string, object>
                    {
                        { "some language 1", "some translation 1"},
                        { "some language 2", "some translation 2"},
                        { "some language 3", "some translation 3"}
                    });

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ASSERT
            Assert.That(viewModel.GetTranslation("some language 1"), Is.EqualTo("some translation 1"));
            Assert.That(viewModel.GetTranslation("some language 2"), Is.EqualTo("some translation 2"));
            Assert.That(viewModel.GetTranslation("some language 3"), Is.EqualTo("some translation 3"));
        }


        [Test]
        public void GetUnknownTranslations()
        {
            m_TextIdResourceItem
                .Stub(textIdResourceItem => textIdResourceItem.LanguageValues)
                .Return(new Dictionary<string, object>
                    {
                        { "some language", "some translation"}
                    });

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ASSERT
            Assert.That(viewModel.GetTranslation("some unknown language"), Is.Null);
        }


        [Test]
        public void SetKnownTranslations()
        {
            // ARRANGE
            m_TextIdResourceItem
                .Stub(textIdResourceItem => textIdResourceItem.LanguageValues)
                .Return(new Dictionary<string, object>
                    {
                        { "some language 1", "some translation 1"},
                        { "some language 2", "some translation 2"},
                        { "some language 3", "some translation 3"}
                    });

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ACT
            viewModel.SetTranslation("some language 1", "some new translation 1");
            viewModel.SetTranslation("some language 2", "some new translation 2");
            viewModel.SetTranslation("some language 3", "some new translation 3");

            // ASSERT
            Assert.That(viewModel.GetTranslation("some language 1"), Is.EqualTo("some new translation 1"));
            Assert.That(viewModel.GetTranslation("some language 2"), Is.EqualTo("some new translation 2"));
            Assert.That(viewModel.GetTranslation("some language 3"), Is.EqualTo("some new translation 3"));
        }


        [Test]
        public void SetUnknownTranslations()
        {
            // ARRANGE
            m_TextIdResourceItem
                .Stub(textIdResourceItem => textIdResourceItem.LanguageValues)
                .Return(new Dictionary<string, object>
                    {
                        { "some language", "some translation"}
                    });

            var viewModel = new TextIdResourceItemViewModel(m_TextIdResourceItem);

            // ACT
            viewModel.SetTranslation("some unknown language", "some new translation");

            // ASSERT
            Assert.That(viewModel.Translations.Count(), Is.EqualTo(2));
            Assert.That(viewModel.GetTranslation("some unknown language"), Is.EqualTo("some new translation"));
        }
    }
}