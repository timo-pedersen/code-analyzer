#if!VNEXT_TARGET
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Core.Api.GlobalReference;
using Core.Controls.Api.Bindings;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.MultiLanguage;
using Neo.ApplicationFramework.Controls.Bindings;
using Neo.ApplicationFramework.Controls.DataGrids;
using Neo.ApplicationFramework.Controls.DataItemBrowseControl.DataSourcesObservers;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TextIdBrowser
{
    [TestFixture]
    public class TextIdBrowserPopupViewModelTest
    {
        internal const string TextPropertyName = "Text";

        private IProject m_Project;
        private IMultiLanguageService m_MultiLanguageService;
        private ITextIdService m_TextIdService;
        private IGlobalReferenceService m_GlobalReferenceService;
        private IMultiLanguagePropertyBinder m_MultiLanguagePropertyBinder;
        private IBindingService m_BindingService;
        private IGlobalSelectionService m_GlobalSelectionService;
        private ICommandManagerService m_CommandManagerService;
        private IStructuredBindingSupportService m_StructuredBindingSupportService;
        private IDataSourcesObserver m_DataSourcesObserver;

        [SetUp]
        public void SetUp()
        {
            // Setup project
            m_Project = Substitute.For<IProject>();
            m_Project.VisibleLanguageColumns = new List<string>
            {
                TextsIde.DefaultText
            };

            // Setup languages
            var languageInfo1 = Substitute.For<ILanguageInfo>();
            languageInfo1.Name = "Language 1";

            var languageInfo2 = Substitute.For<ILanguageInfo>();
            languageInfo2.Name = "Language 2";

            // Setup multi language service
            m_MultiLanguageService = Substitute.For<IMultiLanguageService>();
            m_MultiLanguageService.Languages
                .Returns(new ExtendedBindingList<ILanguageInfo>
                    {
                        languageInfo1,
                        languageInfo2
                    });

            // Setup text id resource items
            var resourceItem1 = CreateTextIdResourceItem(1, 2);
            var resourceItem2 = CreateTextIdResourceItem(2, 2);
            var resourceItem3 = CreateTextIdResourceItem(3, 2);

            // Setup text id service
            m_TextIdService = Substitute.For<ITextIdService>();
            m_TextIdService.TextIDResourceItems
                .Returns(new[]
                    {
                        resourceItem1,
                        resourceItem2,
                        resourceItem3
                    });

            // Setup global reference service
            m_GlobalReferenceService = Substitute.For<IGlobalReferenceService>();
            m_GlobalReferenceService.GetObject<object>(Arg.Any<string>()).Returns(new DependencyObjectStub());

            // Setup multi-language property binder
            m_MultiLanguagePropertyBinder = Substitute.For<IMultiLanguagePropertyBinder>();

            // Set up binding service
            m_BindingService = Substitute.For<IBindingService>();
            
            // Setup GlobalSelectionService
            m_GlobalSelectionService = Substitute.For<IGlobalSelectionService>();

            // Setup CommandManagerService
            m_CommandManagerService = Substitute.For<ICommandManagerService>();

            // Setup StructuredBindingSupportService
            m_StructuredBindingSupportService = Substitute.For<IStructuredBindingSupportService>();


            m_DataSourcesObserver = Substitute.For<IDataSourcesObserver>();
            // Populate service locator
            TestHelper.AddService<IStructuredBindingSupportService>(m_StructuredBindingSupportService);
            TestHelper.AddServiceStub<IDesignerEventService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.RemoveService<IStructuredBindingSupportService>();
        }

        [Test]
        public void SelectedTextId()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();
            var firstTextId = viewModel.TextIds.UnwrappedItems.First();

            // ACT
            viewModel.SelectedTextId = firstTextId;

            // ASSERT
            Assert.That(viewModel.SelectedTextId, Is.Not.Null);
            Assert.That(viewModel.SelectedTextId.Id, Is.EqualTo(1));
        }
        
        [Test]
        public void FilterOnId()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // Lets take the first text id proxy and use it when testing the filter function
            var firstProxy = viewModel.TextIds.DynamicCollection.Cast<DynamicProxy>().ElementAt(0);
            var firstTextId = (TextIdResourceItemViewModel)firstProxy.ProxiedObject;

            // Set id
            firstTextId.Id = 1123;

            // ASSERT
            Assert.That(viewModel.FilterFunction(firstProxy, null), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, string.Empty), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "1"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "11"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "12"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "23"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "1123"), Is.True);

            Assert.That(viewModel.FilterFunction(firstProxy, "111"), Is.False);
            Assert.That(viewModel.FilterFunction(firstProxy, "122"), Is.False);
            Assert.That(viewModel.FilterFunction(firstProxy, "11233"), Is.False);
            Assert.That(viewModel.FilterFunction(firstProxy, "dummy"), Is.False);
        }
        
        [Test]
        public void FilterOnDefaultText()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // Lets take the first text id proxy and use it when testing the filter function
            var firstProxy = viewModel.TextIds.DynamicCollection.Cast<DynamicProxy>().ElementAt(0);
            var firstTextId = (TextIdResourceItemViewModel)firstProxy.ProxiedObject;

            // Set default text
            firstTextId.DefaultText = "Some default text";

            // ASSERT
            Assert.That(viewModel.FilterFunction(firstProxy, null), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, string.Empty), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "Some"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "some"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "me def"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "xt"), Is.True);

            Assert.That(viewModel.FilterFunction(firstProxy, "Some  "), Is.False);
            Assert.That(viewModel.FilterFunction(firstProxy, "Somedefault"), Is.False);
        }
        
        [Test]
        public void FilterOnTranslation()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // Lets take the first text id proxy and use it when testing the filter function
            var firstProxy = viewModel.TextIds.DynamicCollection.Cast<DynamicProxy>().ElementAt(0);
            var firstTextId = (TextIdResourceItemViewModel)firstProxy.ProxiedObject;

            // Set translation
            firstTextId.SetTranslation("Language 1", "New translation");

            // ASSERT
            Assert.That(viewModel.FilterFunction(firstProxy, null), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, string.Empty), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "New"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "new"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "ew tr"), Is.True);
            Assert.That(viewModel.FilterFunction(firstProxy, "New translation"), Is.True);

            Assert.That(viewModel.FilterFunction(firstProxy, "New  "), Is.False);
            Assert.That(viewModel.FilterFunction(firstProxy, "Newtranslation"), Is.False);
        }
        
        [Test]
        public void Add()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // We should investigate whether Closed event is fired
            bool isClosed = false;
            viewModel.RequestClose += (sender, e) => isClosed = true;

            m_TextIdService.CreateTextIDResourceItem().Returns(Substitute.For<ITextIDResourceItem>());

            // ASSERT
            Assert.That(viewModel.TextIds.DynamicCollection.Count(), Is.EqualTo(3));

            // ACT
            viewModel.AddCommand.Execute(null);

            // ASSERT
            m_TextIdService.Received(1).CreateTextIDResourceItem();
            Assert.That(viewModel.TextIds.DynamicCollection.Count(), Is.EqualTo(4));
            Assert.That(isClosed, Is.False);
        }
        
        [Test]
        public void TextIdReset()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // We should investigate whether Closed event is fired
            bool isClosed = false;
            viewModel.RequestClose += (sender, e) => isClosed = true;

            var textIdDesignerResourceItem = Substitute.For<ITextIDDesignerResourceItem>();
            textIdDesignerResourceItem.PropertyName = TextPropertyName;
            textIdDesignerResourceItem.TextID = 1;

            m_MultiLanguagePropertyBinder.ResetTextIDBinding(Arg.Any<object>(), Arg.Any<string>());

            // ACT
            viewModel.SetCurrentBoundItem(textIdDesignerResourceItem);
            viewModel.ResetCommand.Execute(null);

            // ASSERT
            m_MultiLanguagePropertyBinder.ReceivedWithAnyArgs(1).ResetTextIDBinding(Arg.Any<object>(), Arg.Any<string>());
            Assert.That(isClosed, Is.True);
        }
        
        [Test]
        public void TextIdCanReset()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            var textIdDesignerResourceItem = Substitute.For<ITextIDDesignerResourceItem>();
            textIdDesignerResourceItem.PropertyName = TextPropertyName;
            textIdDesignerResourceItem.TextID = 1;

            // ASSERT
            Assert.That(viewModel.ResetCommand.CanExecute(null), Is.False);

            // ACT
            viewModel.SetCurrentBoundItem(textIdDesignerResourceItem);

            // ASSERT
            Assert.That(viewModel.ResetCommand.CanExecute(null), Is.True);
        }
        
        [Test]
        public void TextIdOk()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // We should investigate whether Closed event is fired
            bool isClosed = false;
            viewModel.RequestClose += (sender, e) => isClosed = true;

            var textIdDesignerResourceItem = Substitute.For<ITextIDDesignerResourceItem>();
            textIdDesignerResourceItem.PropertyName = TextPropertyName;
            textIdDesignerResourceItem.TextID = 1;

            m_MultiLanguagePropertyBinder.BindToTextID(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<uint>());

            // ACT
            viewModel.SetCurrentBoundItem(textIdDesignerResourceItem);
            viewModel.OkCommand.Execute(null);

            // ASSERT
            m_MultiLanguagePropertyBinder.ReceivedWithAnyArgs(1).BindToTextID(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<uint>());
            Assert.That(isClosed, Is.True);
        }

        [Test]
        public void TextIdCanOk()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            var textIdDesignerResourceItem = Substitute.For<ITextIDDesignerResourceItem>();
            textIdDesignerResourceItem.PropertyName = TextPropertyName;
            textIdDesignerResourceItem.TextID = 1;

            // ASSERT
            Assert.That(viewModel.OkCommand.CanExecute(null), Is.False);

            // ACT
            viewModel.SetCurrentBoundItem(textIdDesignerResourceItem);

            // ASSERT
            Assert.That(viewModel.OkCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void TextIdCancel()
        {
            // ARRANGE
            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // We should investigate whether Closed event is fired
            bool isClosed = false;
            viewModel.RequestClose += (sender, e) => isClosed = true;

            // ACT
            viewModel.CancelCommand.Execute(null);

            // ASSERT
            Assert.That(isClosed, Is.True);
        }

        [Test]
        public void TextIdDynamicsTargetMustBeDependencyObject()
        {
            // ARRANGE
            IDesignerResourceItem designerResourceItem = Substitute.For<IDesignerResourceItem>();
            IAlarmServer alarmServer = Substitute.For<IAlarmServer>();
            designerResourceItem.DesignerName = "AlarmServer";
            designerResourceItem.ObjectName = "Default";
            designerResourceItem.PropertyName = "Text";

            IGlobalReferenceService globalReferenceService = Substitute.For<IGlobalReferenceService>();
            globalReferenceService.GetObject<object>(Arg.Any<string>()).Returns(alarmServer);

            m_GlobalReferenceService = globalReferenceService;

            TextIdBrowserPopupViewModel viewModel = GetTextIdBrowserPopupViewModel();

            // ACT
            viewModel.SetCurrentBoundItem(designerResourceItem);

            // ASSERT
            Assert.That(viewModel.IsValidTargetForDynamics, Is.False);

            // ARRANGE
            DependencyObject button = new Button();
            Binding binding = new Binding { Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(button, button.GetDependencyProperty("Text"), binding);

            // ACT
            viewModel.SetTarget(button, "Text");

            // ASSERT
            Assert.That(viewModel.IsValidTargetForDynamics, Is.True);
        }

        internal static ITextIDResourceItem CreateTextIdResourceItem(uint id, int languageValueCount)
        {
            var textIdResourceItem = Substitute.For<ITextIDResourceItem>();

            // Set id
            textIdResourceItem.TextID = id;

            // Create language values
            var languageValues = new Dictionary<string, object>();
            for (int i = 0; i < languageValueCount; i++)
            {
                string key = string.Format("Language {0}", i);
                string value = string.Format("Translation {0}", i);

                languageValues.Add(key, value);
            }

            textIdResourceItem.LanguageValues.Returns(languageValues);

            return textIdResourceItem;
        }

        private TextIdBrowserPopupViewModel GetTextIdBrowserPopupViewModel()
        {
            return new TextIdBrowserPopupViewModel(
                m_Project,
                m_MultiLanguageService,
                m_TextIdService,
                m_GlobalReferenceService,
                m_MultiLanguagePropertyBinder,
                m_BindingService,
                m_GlobalSelectionService,
                m_CommandManagerService,
                m_DataSourcesObserver
                );
        }

        internal class DependencyObjectStub : DependencyObject
        {
            public static readonly DependencyProperty TextProperty = DependencyProperty.Register(TextPropertyName, typeof(string), typeof(DependencyObjectStub));

            public string Text
            {
                get { return (string)GetValue(TextProperty); }
                set { SetValue(TextProperty, value); }
            }
        }
    }
}
#endif
