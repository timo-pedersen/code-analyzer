using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using Core.Component.Api.Design;
using Core.Controls.Api.Bindings;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.DataSourcesObservers;
using Neo.ApplicationFramework.Controls.Expressions;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl
{
    [TestFixture]
    public class CommittingDataSourceSelectorViewModelTest
    {
        private IBindingService m_BindingService;
        private IDataSourcesObserver m_DataSourcesObserver;
        private IDataCommandFacade m_DataCommandFacade;
        private IStructuredBindingSupportService m_StructuredBindingSupportService;
        private IPropertyBinderFactory m_PropertyBinderFactory;
        private IExpressionHelper m_ExpressionHelper;
        private INeoDesignerHost m_NeoDesignerHost;

        [SetUp]
        public void SetUp()
        {
            // Setup binding source description
            BindingSourceDescription bindingSourceDescription = CreateBindingSourceDescription("Binding source name");

            // Setup data source
            IDataSource dataSource = CreateDataSource("Data source");

            dataSource
                .Stub(ds => ds.Items)
                .Return(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription });

            // Setup binding service
            m_BindingService = MockRepository.GenerateMock<IBindingService>();

            // Setup data sources observer
            m_DataSourcesObserver = MockRepository.GenerateMock<IDataSourcesObserver>();
            m_DataSourcesObserver
                .Expect(dso => dso.DataSources)
                .Return(new[] { dataSource });

            // Setup data command facade
            m_DataCommandFacade = MockRepository.GenerateMock<IDataCommandFacade>();

            // Setup structured binding support service
            m_StructuredBindingSupportService = MockRepository.GenerateStub<IStructuredBindingSupportService>();

            // Setup property binder factory
            m_PropertyBinderFactory = MockRepository.GenerateMock<IPropertyBinderFactory>();

            // Setup expression helper
            m_ExpressionHelper = MockRepository.GenerateMock<IExpressionHelper>();

            // Setup designer event service
            m_NeoDesignerHost = MockRepository.GenerateMock<INeoDesignerHost>();
            
            var designerEventService = TestUtilities.TestHelper.AddServiceStub<IDesignerEventService>();
            designerEventService
                .Stub(mock => mock.ActiveDesigner)
                .Return(m_NeoDesignerHost);
        }

        [Test]
        public void IsEnabled()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            var viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, null);

            m_NeoDesignerHost
                .Stub(mock => mock.RootDesigner)
                .Return(MockRepository.GenerateMock<IScreenRootDesigner>());

            m_PropertyBinderFactory
                .Expect(pbf => pbf.IsDataBindingSupported(null)).IgnoreArguments()
                .Return(true);

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.True);
            m_BindingService.VerifyAllExpectations();
            m_PropertyBinderFactory.VerifyAllExpectations();
        }

        [Test]
        public void IsNotEnabledWhenTargetsAreNull()
        {
            // ARRANGE
            var viewModel = CreateCommittingDataSourceSelectorViewModel();

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
        }

        [Test]
        public void IsNotEnabledWhenTargetsAreEmpty()
        {
            // ARRANGE
            var targets = new object[0];

            var viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, null);

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
        }

        [Test]
        public void IsNotEnabledWhenBindingTypeDiffersFromWpf()
        {
            // ARRANGE
            var targets = new[]
            {
                new TextBox(),
                new TextBox()
            };

            var viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, null);

            m_NeoDesignerHost
                .Stub(mock => mock.RootDesigner)
                .Return(MockRepository.GenerateMock<IDesignerBase>());
            
            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
            m_BindingService.VerifyAllExpectations();
        }

        [Test]
        public void IsNotEnabledWhenDataBindingIsUnsupported()
        {
            // ARRANGE
            var targets = new[]
            {
                new TextBox(),
                new TextBox()
            };

            var viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, null);

            m_NeoDesignerHost
                .Stub(mock => mock.RootDesigner)
                .Return(MockRepository.GenerateMock<IScreenRootDesigner>());

            m_PropertyBinderFactory
                .Expect(pbf => pbf.IsDataBindingSupported(null)).IgnoreArguments()
                .Return(false);

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
            m_BindingService.VerifyAllExpectations();
        }

        [Test]
        public void IsNotUsingExpressionDueToPrimaryTargetIsNull()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            CommittingDataSourceSelectorViewModel viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, null);

            // ACT
            var isUsingExpression = viewModel.IsUsingExpression;

            // ASSERT
            Assert.That(isUsingExpression, Is.False);
        }

        [Test]
        public void IsNotUsingExpressionDueToNoConfiguredExpression()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            CommittingDataSourceSelectorViewModel viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, targets[0]);

            m_ExpressionHelper
                .Expect(eh => eh.GetExpressionName(null, "")).IgnoreArguments()
                .Return(null);

            // ACT
            var isUsingExpression = viewModel.IsUsingExpression;

            // ASSERT
            Assert.That(isUsingExpression, Is.False);
            m_ExpressionHelper.VerifyAllExpectations();
        }

        [Test]
        public void IsUsingExpression()
        {
            // ARRANGE
            var targets = new object[]
            {
                new TextBox(),
                new TextBox()
            };

            CommittingDataSourceSelectorViewModel viewModel = CreateCommittingDataSourceSelectorViewModel();

            viewModel.UpdateTargets(targets, targets[0]);

            m_ExpressionHelper
                .Expect(eh => eh.GetExpressionName(null, "")).IgnoreArguments()
                .Return("expression name");

            // ACT
            var isUsingExpression = viewModel.IsUsingExpression;

            // ASSERT
            Assert.That(isUsingExpression, Is.True);
            m_ExpressionHelper.VerifyAllExpectations();
        }

        #region Helper methods

        private CommittingDataSourceSelectorViewModel CreateCommittingDataSourceSelectorViewModel()
        {
            return new CommittingDataSourceSelectorViewModel(
                m_BindingService,
                m_DataSourcesObserver,
                m_DataCommandFacade,
                m_StructuredBindingSupportService,
                m_PropertyBinderFactory,
                m_ExpressionHelper);
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
