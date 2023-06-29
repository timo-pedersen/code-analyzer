#if!VNEXT_TARGET
using System.Collections.ObjectModel;
using System.Linq;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.TestUtilities.Utilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class DataSourceSelectorDropDownViewModelTest
    {
        private BindingSourceDescription m_BindingSourceDescription;
        private IDataSource m_DataSource;

        [SetUp]
        public void SetUp()
        {
            // Setup BindingSourceDescription
            m_BindingSourceDescription = new BindingSourceDescription("Binding source description name", null, null, false);

            // Setup IDataSource
            m_DataSource = Substitute.For<IDataSource>();
            m_DataSource.Name.Returns("Data source name");

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });
        }

        [Test]
        public void Filter()
        {
            // ARRANGE
            var viewModel = new DataSourceSelectorDropDownViewModel(() => true);

            // ACT
            viewModel.Filter = "Some filter";

            // ASSERT
            Assert.That(viewModel.Filter, Is.EqualTo("Some filter"));
            Assert.That(viewModel.DataSources.All(dataSource => dataSource.Filter == "Some filter"), Is.True);
        }

        [Test]
        public void Ok()
        {
            // ARRANGE
            var viewModel = new DataSourceSelectorDropDownViewModel(() => true);

            var eventAsserter = new EventAsserter();
            viewModel.Ok += eventAsserter.Handler;

            // ACT
            viewModel.OkCommand.Execute(null);

            // ASSERT
            Assert.That(eventAsserter.Count, Is.EqualTo(0));
        }

        [Test]
        public void Cancel()
        {
            // ARRANGE
            var viewModel = new DataSourceSelectorDropDownViewModel(() => true);

            var eventAsserter = new EventAsserter();
            viewModel.Cancel += eventAsserter.Handler;

            // ACT
            viewModel.CancelCommand.Execute(null);

            // ASSERT
            Assert.That(eventAsserter.Count, Is.EqualTo(1));
        }
    }
}
#endif
