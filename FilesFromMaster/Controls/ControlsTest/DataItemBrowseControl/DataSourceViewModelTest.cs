using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.Builders;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class DataSourceViewModelTest
    {
        private IDataSource m_DataSource;
        private BindingSourceDescription m_BindingSourceDescription;
        private IBindingSourceDescriptionRepresentationBuilder m_RepresentationBuilder;

        [SetUp]
        public void SetUp()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());


            // Setup representation builder
            m_RepresentationBuilder = new BindingSourceDescriptionRepresentationBuilder();

            // Setup binding source description
            m_BindingSourceDescription = CreateBindingSourceDescription("Binding source description");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Stub(ds => ds.Name)
                .Return("Data source");
        }

        [Test]
        public async Task Name()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            string name = viewModel.Name;

            // ASSERT
            Assert.That(name, Is.EqualTo("Data source"));
            m_DataSource.VerifyAllExpectations();
        }

        [Test]
        public async Task AddButtonVisibility_Visible()
        {
            // ARRANGE
            m_DataSource
                .Expect(ds => ds.IsAddSupported)
                .Return(true);

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Visible));
            m_DataSource.VerifyAllExpectations();
        }

        [Test]
        public async Task AddButtonVisibility_CollapsedDueToDataSource()
        {
            // ARRANGE
            m_DataSource
                .Stub(ds => ds.IsAddSupported)
                .Return(false);

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public async Task AddButtonVisibility_CollapsedDueToViewModelConfiguration()
        {
            // ARRANGE
            m_DataSource
                .Stub(ds => ds.IsAddSupported)
                .Return(true);

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);
            viewModel.IsAddSupported = false;

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public async Task AddIsSupported()
        {
            // ARRANGE
            m_DataSource
                .Stub(ds => ds.IsAddSupported)
                .Return(true);

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);
            viewModel.IsAddSupported = true;

            // ACT
            bool isAddSupported = viewModel.IsAddSupported;
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(isAddSupported, Is.True);
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public async Task AddIsNotSupported()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);
            viewModel.IsAddSupported = false;

            // ACT
            bool isAddSupported = viewModel.IsAddSupported;
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(isAddSupported, Is.False);
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public async Task RepresentationTypeIsInitiallyList()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.List));
        }

        [Test]
        public async Task ChangeRepresentationTypeOnce()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Stub(ds => ds.Name)
                .Return("Data source");

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);
            viewModel.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            // ACT, toggle representation type once
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.Tree));
        }

        [Test]
        public async Task ChangeRepresentationTypeTwice()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Stub(ds => ds.Name)
                .Return("Data source");

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);
            viewModel.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            // ACT, toggle representation type twice
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.List));
        }

        [Test]
        public async Task ChangeRepresentationTypeVisibility_Visible()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Stub(ds => ds.Name)
                .Return("Data source");

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            Visibility changeRepresentationTypeVisibility = viewModel.ChangeRepresentationTypeVisibility;

            // ASSERT
            Assert.That(changeRepresentationTypeVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public async Task ChangeRepresentationTypeVisibility_Collapsed()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Tag1");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Expect(ds => ds.Name)
                .Return("Data source");

            var viewModel = CreateDataSourceViewModel();
            await viewModel.UpdateDataSourceAsync(m_DataSource, null);

            // ACT
            Visibility changeRepresentationTypeVisibility = viewModel.ChangeRepresentationTypeVisibility;

            // ASSERT
            Assert.That(changeRepresentationTypeVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public async Task GlobalRepresentationType()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = MockRepository.GenerateMock<IDataSource>();

            m_DataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource
                .Stub(ds => ds.Name)
                .Return("Data source");

            var viewModel1 = CreateDataSourceViewModel();
            await viewModel1.UpdateDataSourceAsync(m_DataSource, null);
            viewModel1.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            var viewModel2 = CreateDataSourceViewModel();
            await viewModel2.UpdateDataSourceAsync(m_DataSource, null);
            viewModel2.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            // ACT
            viewModel1.ChangeRepresentationTypeCommand.Execute(null);

            // ASSERT
            Assert.That(viewModel1.RepresentationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.Tree));
            Assert.That(viewModel2.RepresentationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.Tree));
        }
        
        #region Helper methods

        private DataSourceViewModel CreateDataSourceViewModel()
        {
            return new DataSourceViewModel(m_RepresentationBuilder, m_DataSource);
        }

        private static BindingSourceDescription CreateBindingSourceDescription(string name)
        {
            return new BindingSourceDescription(name, name, name, false);
        }

        #endregion
    }
}