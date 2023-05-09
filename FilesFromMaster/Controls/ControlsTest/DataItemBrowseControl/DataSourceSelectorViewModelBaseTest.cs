using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Common.Bindings;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.DataSourcesObservers;
using Neo.ApplicationFramework.TestUtilities.Utilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class DataSourceSelectorViewModelBaseTest
    {
        private IDataSourcesObserver m_DataSourcesObserver;
        private IDataCommandFacade m_DataCommandFacade;
        private IStructuredBindingSupportService m_StructuredBindingSupportService;

        [SetUp]
        public void SetUp()
        {
            // Setup binding source description
            var bindingSourceDescription = CreateBindingSourceDescription("Binding source name");

            // Setup data source
            IDataSource dataSource = CreateDataSource("Data source");

            dataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription })
                .Repeat.Any();

            // Setup binding service
            m_DataSourcesObserver = MockRepository.GenerateStub<IDataSourcesObserver>();
            m_DataSourcesObserver
                .Expect(bs => bs.DataSources)
                .Return(new[] { dataSource });

            // Setup data command facade
            m_DataCommandFacade = MockRepository.GenerateStub<IDataCommandFacade>();

            // Setup structured binding support service
            m_StructuredBindingSupportService = MockRepository.GenerateStub<IStructuredBindingSupportService>();
        }

        [Test]
        public void Text()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(false);

            // ACT
            viewModel.Text = "New text";

            // ASSERT
            Assert.That(viewModel.Text, Is.EqualTo("New text"));
            Assert.That(viewModel.DropDownViewModel.Filter, Is.EqualTo("New text"));
        }

        [Test]
        public void SelectedIBindingSourceDescriptionIsNullBindingSourceDescription()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(false);

            // ACT
            viewModel.SelectedBindingSourceDescription = null;

            // ASSERT
            Assert.That(viewModel.SelectedIBindingSourceDescription, Is.EqualTo(NullBindingSourceDescription.Instance));
        }

        [Test]
        public void SelectedIBindingSourceDescriptionIsNotNull()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(false);

            var bindingSourceDescription = new BindingSourceDescription();
            var bindingSourceDescriptionViewModel = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ACT
            viewModel.SelectedBindingSourceDescription = bindingSourceDescriptionViewModel;

            // ASSERT
            Assert.That(viewModel.SelectedIBindingSourceDescription, Is.EqualTo(bindingSourceDescription));
        }

        [Test]
        public void UpdateTargets()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(false);

            var eventAsserter = new EventAsserter();
            ((INotifyPropertyChanged)viewModel).PropertyChanged += eventAsserter.Handler;

            var targets = new[]
            {
                new TextBox(),
                new TextBox()
            };

            var primaryTarget = targets.First();

            // ACT
            viewModel.UpdateTargets(targets, primaryTarget);

            // ASSERT
            Assert.That(eventAsserter.Count, Is.EqualTo(3));
            Assert.That(eventAsserter.Dequeue<PropertyChangedEventArgs>().PropertyName, Is.EqualTo("Targets"));
            Assert.That(eventAsserter.Dequeue<PropertyChangedEventArgs>().PropertyName, Is.EqualTo("PrimaryTarget"));
            Assert.That(eventAsserter.Dequeue<PropertyChangedEventArgs>().PropertyName, Is.EqualTo("IsEnabled"));
        }

        [Test]
        public void AvailableDataSourcesForActiveDesignerChanged()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(false);

            // Setup first data source
            IDataSource firstDataSource = CreateDataSource("First data source");
            var bindingSourceDescription = CreateBindingSourceDescription("Binding source description on first data source");

            firstDataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription });

            // Setup second data source
            IDataSource secondDataSource = CreateDataSource("Second data source");
            var secondBindingSourceDescription = CreateBindingSourceDescription("Binding source description on second data source");

            secondDataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { secondBindingSourceDescription });

            // Setup binding service
            m_DataSourcesObserver
                .Expect(bs => bs.DataSources)
                .Return(new[] { firstDataSource, secondDataSource }).Repeat.Any();

            // ACT
            m_DataSourcesObserver.Raise(bs => bs.DataSourcesUpdated += null, m_DataSourcesObserver, EventArgs.Empty);

            // ASSERT
            Assert.That(viewModel.BindingSourceDescriptions.Count, Is.EqualTo(2));
            Assert.That(viewModel.BindingSourceDescriptions.ElementAt(0).BindingSourceDescription, Is.EqualTo(bindingSourceDescription));
            Assert.That(viewModel.BindingSourceDescriptions.ElementAt(1).BindingSourceDescription, Is.EqualTo(secondBindingSourceDescription));
        }

        [Test]
        public void CanConfigureExpression()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            m_DataCommandFacade
                .Expect(dcf => dcf.GetBindingSourceDescription(null, null)).IgnoreArguments()
                .Return(new BindingSourceDescription());

            viewModel.UpdateTargets(targets, targets[0]);

            var bindingSourceDescription = new BindingSourceDescription(null, null, null, true);

            viewModel.SelectedBindingSourceDescription = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ACT
            bool canConfigureExpression = viewModel.ExpressionCommand.CanExecute(null);

            // ASSERT
            Assert.That(canConfigureExpression, Is.True);
        }

        [Test]
        public void CanNotConfigureExpressionDueToPrimaryTarget()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            viewModel.UpdateTargets(targets, null);

            // ACT
            bool canConfigureExpression = viewModel.ExpressionCommand.CanExecute(null);

            // ASSERT
            Assert.That(canConfigureExpression, Is.False);
        }

        [Test]
        public void CanNotConfigureExpressionDueToSelectedBindingSourceDescriptionIsNull()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            m_DataCommandFacade
                .Expect(dcf => dcf.GetBindingSourceDescription(null, null)).IgnoreArguments()
                .Return(new BindingSourceDescription());

            viewModel.UpdateTargets(targets, targets[0]);

            // ACT
            bool canConfigureExpression = viewModel.ExpressionCommand.CanExecute(null);

            // ASSERT
            Assert.That(canConfigureExpression, Is.False);
            Assert.That(viewModel.SelectedBindingSourceDescription, Is.Null);
        }

        [Test]
        public void CanNotConfigureExpressionDueToSelectedBindingSourceDescriptionNotSupportingIt()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            m_DataCommandFacade
                .Expect(dcf => dcf.GetBindingSourceDescription(null, null)).IgnoreArguments()
                .Return(new BindingSourceDescription());

            viewModel.UpdateTargets(targets, targets[0]);

            var bindingSourceDescription = new BindingSourceDescription();

            viewModel.SelectedBindingSourceDescription = BindingSourceDescriptionViewModel.Create(bindingSourceDescription);

            // ACT
            bool canConfigureExpression = viewModel.ExpressionCommand.CanExecute(null);

            // ASSERT
            Assert.That(canConfigureExpression, Is.False);
            Assert.That(viewModel.SelectedBindingSourceDescription, Is.Not.Null);
        }

        [Test]
        public void CanNotBindUsingText()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            IDataSource dataSource = CreateDataSource("Data source");
            var bindingSourceDescription = CreateBindingSourceDescription("Binding source description");

            dataSource
                .Expect(ds => ds.IsDefault)
                .Return(false)
                .Repeat.Any();

            dataSource
                .Expect(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription })
                .Repeat.Any();

            // Setup binding service
            m_DataSourcesObserver
                .Expect(bs => bs.DataSources)
                .Return(new[] { dataSource })
                .Repeat.Any();

            // Populate class with data sources
            m_DataSourcesObserver.Raise(bs => bs.DataSourcesUpdated += null, m_DataSourcesObserver, EventArgs.Empty);

            // ACT
            bool canBindUsingText = viewModel.EnterKeyCommand.CanExecute(null);

            // ASSERT
            Assert.That(canBindUsingText, Is.False);
            dataSource.VerifyAllExpectations();
        }

        [Test]
        public void CanBindUsingText()
        {
            // ARRANGE
            DataSourceSelectorViewModelBaseImpl viewModel = CreateDataSourceSelectorViewModelBaseImpl(true);

            IDataSource dataSource = CreateDataSource("Data source");
            var bindingSourceDescription = CreateBindingSourceDescription("Binding source description");

            dataSource
                .Expect(ds => ds.IsDefault)
                .Return(true)
                .Repeat.Any();

            dataSource
                .Expect(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription })
                .Repeat.Any();

            // Setup binding service
            m_DataSourcesObserver
                .Expect(bs => bs.DataSources)
                .Return(new[] { dataSource })
                .Repeat.Any();

            // Populate class with data sources
            m_DataSourcesObserver.Raise(bs => bs.DataSourcesUpdated += null, m_DataSourcesObserver, EventArgs.Empty);

            // ACT
            bool canBindUsingText = viewModel.EnterKeyCommand.CanExecute(null);

            // ASSERT
            Assert.That(canBindUsingText, Is.True);
            dataSource.VerifyAllExpectations();
        }

        #region Helper methods and classes

        class DataSourceSelectorViewModelBaseImpl : DataSourceSelectorViewModelBase
        {
            private readonly bool m_IsEnabled;

            public DataSourceSelectorViewModelBaseImpl(
                IDataSourcesObserver dataSourcesObserver,
                IDataCommandFacade dataCommandFacade,
                IStructuredBindingSupportService structuredBindingSupportService,
                bool isEnabled)
                : base(
                    dataSourcesObserver,
                    dataCommandFacade,
                    structuredBindingSupportService)
            {
                m_IsEnabled = isEnabled;
            }

            public override bool IsEnabled
            {
                get { return m_IsEnabled; }
            }

            public override string ExpressionName
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            protected override string GetPrimaryTargetPropertyName(BindingSourceDescription bindingSource)
            {
                return "SomePropertyName";
            }

            protected override bool IsDataBindingSupported(ICollection targets)
            {
                throw new NotImplementedException();
            }

            protected override void Bind(BindingSourceDescription source, ICollection targets)
            {
                throw new NotImplementedException();
            }

            protected override void ChangeExpression()
            {
                throw new NotImplementedException();
            }

            protected override void ExecuteEscape()
            {
                throw new NotImplementedException();
            }
        }

        private DataSourceSelectorViewModelBaseImpl CreateDataSourceSelectorViewModelBaseImpl(bool isEnabled)
        {
            return new DataSourceSelectorViewModelBaseImpl(
                m_DataSourcesObserver,
                m_DataCommandFacade,
                m_StructuredBindingSupportService,
                isEnabled);
        }

        private static BindingSourceDescription CreateBindingSourceDescription(string name)
        {
            return new BindingSourceDescription(name, name, name, false);
        }

        private static IDataSource CreateDataSource(string name)
        {
            var dataSource = MockRepository.GenerateStub<IDataSource>();

            dataSource
                .Stub(ds => ds.Name)
                .Return(name)
                .Repeat.Any();

            return dataSource;
        }

        #endregion
    }
}