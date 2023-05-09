#if !VNEXT_TARGET
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using Core.Component.Engine.Design;
using Core.Controls.Api.Bindings;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.DataSourcesObservers;
using Neo.ApplicationFramework.Controls.Expressions;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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

            dataSource.Items
                .Returns(new ObservableCollection<BindingSourceDescription> { bindingSourceDescription });

            // Setup binding service
            m_BindingService = Substitute.For<IBindingService>();

            // Setup data sources observer
            m_DataSourcesObserver = Substitute.For<IDataSourcesObserver>();
            m_DataSourcesObserver.DataSources.Returns(new[] { dataSource });

            // Setup data command facade
            m_DataCommandFacade = Substitute.For<IDataCommandFacade>();

            // Setup structured binding support service
            m_StructuredBindingSupportService = Substitute.For<IStructuredBindingSupportService>();

            // Setup property binder factory
            m_PropertyBinderFactory = Substitute.For<IPropertyBinderFactory>();

            // Setup expression helper
            m_ExpressionHelper = Substitute.For<IExpressionHelper>();

            // Setup designer event service
            m_NeoDesignerHost = Substitute.For<INeoDesignerHost>();
            
            var designerEventService = TestUtilities.TestHelper.AddServiceStub<IDesignerEventService>();
            designerEventService.ActiveDesigner.Returns(m_NeoDesignerHost);
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

            m_NeoDesignerHost.RootDesigner.Returns(Substitute.For<IScreenRootDesigner>());

            m_PropertyBinderFactory.IsDataBindingSupported(null).Returns(true);

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.True);
            m_PropertyBinderFactory.Received().IsDataBindingSupported(null);
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

            m_NeoDesignerHost.RootDesigner.Returns(Substitute.For<IDesignerBase>());
            
            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
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

            m_NeoDesignerHost.RootDesigner.Returns(Substitute.For<IScreenRootDesigner>());

            m_PropertyBinderFactory.IsDataBindingSupported(null).Returns(false);

            // ASSERT
            Assert.That(viewModel.IsEnabled, Is.False);
            m_PropertyBinderFactory.Received().IsDataBindingSupported(null);
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

            m_ExpressionHelper.GetExpressionName(null, "").Returns(x => null);

            // ACT
            var isUsingExpression = viewModel.IsUsingExpression;

            // ASSERT
            Assert.That(isUsingExpression, Is.False);
            m_ExpressionHelper.Received().GetExpressionName(null, "");
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

            m_ExpressionHelper.GetExpressionName(null, "").Returns("expression name");

            // ACT
            var isUsingExpression = viewModel.IsUsingExpression;

            // ASSERT
            Assert.That(isUsingExpression, Is.True);
            m_ExpressionHelper.Received().GetExpressionName(null, "");
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
            var dataSource = Substitute.For<IDataSource>();

            dataSource.Name.Returns(name);

            return dataSource;
        }

#endregion
    }
}
#endif
