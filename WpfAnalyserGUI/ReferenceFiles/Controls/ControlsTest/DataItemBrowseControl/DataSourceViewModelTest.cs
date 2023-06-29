#if!VNEXT_TARGET
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.Builders;
using NSubstitute;
using NUnit.Framework;

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
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");
        }

        [Test]
        public void Name()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            string name = viewModel.Name;

            // ASSERT
            Assert.That(name, Is.EqualTo("Data source"));
        }

        [Test]
        public void AddButtonVisibility_Visible()
        {
            // ARRANGE
            m_DataSource.IsAddSupported.Returns(true);

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void AddButtonVisibility_CollapsedDueToDataSource()
        {
            // ARRANGE
            m_DataSource.IsAddSupported.Returns(false);

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void AddButtonVisibility_CollapsedDueToViewModelConfiguration()
        {
            // ARRANGE
            m_DataSource.IsAddSupported.Returns(true);

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);
            viewModel.IsAddSupported = false;

            // ACT
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void AddIsSupported()
        {
            // ARRANGE
            m_DataSource.IsAddSupported.Returns(true);

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);
            viewModel.IsAddSupported = true;

            // ACT
            bool isAddSupported = viewModel.IsAddSupported;
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(isAddSupported, Is.True);
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void AddIsNotSupported()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);
            viewModel.IsAddSupported = false;

            // ACT
            bool isAddSupported = viewModel.IsAddSupported;
            Visibility addButtonVisibility = viewModel.AddButtonVisibility;

            // ASSERT
            Assert.That(isAddSupported, Is.False);
            Assert.That(addButtonVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void RepresentationTypeIsInitiallyList()
        {
            // ARRANGE
            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.List));
        }

        [Test]
        public void ChangeRepresentationTypeOnce()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);
            viewModel.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            // ACT, toggle representation type once
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.Tree));
        }

        [Test]
        public void ChangeRepresentationTypeTwice()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);
            viewModel.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            // ACT, toggle representation type twice
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            viewModel.ChangeRepresentationTypeCommand.Execute(null);
            BindingSourceDescriptionRepresentationType representationType = viewModel.RepresentationType;

            // ASSERT
            Assert.That(representationType, Is.EqualTo(BindingSourceDescriptionRepresentationType.List));
        }

        [Test]
        public void ChangeRepresentationTypeVisibility_Visible()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            Visibility changeRepresentationTypeVisibility = viewModel.ChangeRepresentationTypeVisibility;

            // ASSERT
            Assert.That(changeRepresentationTypeVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void ChangeRepresentationTypeVisibility_Collapsed()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Tag1");

            // Setup data source
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");

            var viewModel = CreateDataSourceViewModel();
            viewModel.UpdateDataSource(m_DataSource, null);

            // ACT
            Visibility changeRepresentationTypeVisibility = viewModel.ChangeRepresentationTypeVisibility;

            // ASSERT
            Assert.That(changeRepresentationTypeVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void GlobalRepresentationType()
        {
            // ARRANGE
            m_BindingSourceDescription = CreateBindingSourceDescription("Saw1.BeepNumber");

            // Setup data source
            m_DataSource = Substitute.For<IDataSource>();

            m_DataSource.Items.Returns(new ObservableCollection<BindingSourceDescription> { m_BindingSourceDescription });

            m_DataSource.Name.Returns("Data source");

            var viewModel1 = CreateDataSourceViewModel();
            viewModel1.UpdateDataSource(m_DataSource, null);
            viewModel1.RepresentationType = BindingSourceDescriptionRepresentationType.List;

            var viewModel2 = CreateDataSourceViewModel();
            viewModel2.UpdateDataSource(m_DataSource, null);
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
#endif
