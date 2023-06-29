#if!VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Data;
using Core.Api.GlobalReference;
using Core.Controls.Api.Bindings;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Controls.TextIdBrowser.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.Tools.UndoManager;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TextIdBrowser
{
    [TestFixture]
    public class TextIdBrowserControlTest
    {
        private ISelectionService m_SelectionService;
        private IProject m_Project;
        private IMultiLanguageService m_MultiLanguageService;
        private ITextIdService m_TextIdService;
        private IGlobalReferenceService m_GlobalReferenceService;
        private IBindingService m_BindingService;

        private const string LocalizableTextPropertyName = "Text";
        private const string UndoDescription = "UndoDesc";

        [SetUp]
        public void TestFixtureSetUp()
        {
            m_SelectionService = Substitute.For<ISelectionService>();

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

            // Setup text id service
            m_TextIdService = Substitute.For<ITextIdService>();
            m_TextIdService.TextIDResourceItems
                .Returns(new[]
                    {
                       TextIdBrowserPopupViewModelTest.CreateTextIdResourceItem(1, 2)
                    });

            // Setup global reference service
            m_GlobalReferenceService = Substitute.For<IGlobalReferenceService>();
            m_GlobalReferenceService.GetObject<object>(Arg.Any<string>())
                .Returns(new TextIdBrowserPopupViewModelTest.DependencyObjectStub());

            // Set up binding service
            m_BindingService = Substitute.For<IBindingService>();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void IsTargetValidReturnsFalseIfTargetIsNull()
        {
            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = null,
                LocalizablePropertyName = "Text"
            };
            Assert.IsFalse(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void IsTargetValidReturnsFalseIfPropertyNameIsEmpty()
        {
            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = new Label(),
                LocalizablePropertyName = ""
            };
            Assert.IsFalse(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void IsTargetValidReturnsFalseIfBoundToDynamicsThatNotHaveTextIdConverter()
        {
            DependencyObject label = new Label();
            Binding binding = new Binding() { Converter = new RawConverter(), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(label, label.GetDependencyProperty("Text"), binding);

            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = label,
                LocalizablePropertyName = LocalizableTextPropertyName
            };
            Assert.IsFalse(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void IsTargetValidReturnsTrueIfNoDynamicsBinding()
        {
            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = new Label(),
                LocalizablePropertyName = LocalizableTextPropertyName
            };
            Assert.IsTrue(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void IsTargetValidReturnsTrueIfBoundToDesignerResourceItem()
        {
            DependencyObject label = new Label();

            var designerResourceItem = Substitute.For<IDesignerResourceItem>();

            Binding resourceBinding = new Binding();
            resourceBinding.Path = new PropertyPath("CurrentValue");
            resourceBinding.Source = designerResourceItem;
            resourceBinding.Mode = BindingMode.TwoWay;
            resourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            BindingOperations.SetBinding(label, label.GetDependencyProperty(LocalizableTextPropertyName), resourceBinding);

            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = label,
                LocalizablePropertyName = LocalizableTextPropertyName
            };

            Assert.IsTrue(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void IsTargetValidReturnsTrueIfBoundToDynamicsWithTextIdConverter()
        {
            DependencyObject label = new Label();
            Binding binding = new Binding() { Converter = new TextIdConverter(), Mode = BindingMode.OneWay };
            BindingOperations.SetBinding(label, label.GetDependencyProperty(LocalizableTextPropertyName), binding);

            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = label,
                LocalizablePropertyName = LocalizableTextPropertyName
            };
            Assert.IsTrue(textIdBrowserControl.IsTargetValid);
        }

        [Test]
        public void UndoIsRegisteredForMultiSelectedTextItemObjects()
        {
            //SETUP
            var label1 = Substitute.For<TestLabel>();
            var label2 = new Label();
            var label3 = new Label();

            var selectedList = new List<Label>() { label1, label2, label3 };

            m_SelectionService.GetSelectedComponents().Returns(selectedList);

            var screenUndoService = Substitute.For<IScreenUndoService>();
            screenUndoService.OpenParentUndo(Arg.Any<string>()).Returns(new ParentUndoUnit(UndoDescription, screenUndoService));

            label1.GetService(typeof(IScreenUndoService)).Returns(screenUndoService);

            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = label1,
                LocalizablePropertyName = LocalizableTextPropertyName,
                UndoText = UndoDescription
            };

            //TEST
            textIdBrowserControl.Text = "NewText";
            textIdBrowserControl.RegisterUndoUnitsAndUpdateTexts();

            //ASERT
            screenUndoService.ReceivedWithAnyArgs().OpenParentUndo(Arg.Any<string>());
            screenUndoService.ReceivedWithAnyArgs(4).RegisterUndoUnit(Arg.Any<IUndoUnit>());
            screenUndoService.ReceivedWithAnyArgs(1).CloseParentUndo(Arg.Any<IParentUndoUnit>());
        }

        [Test]
        public void UndoIsRegisteredForMultiSelectedNonTextItemObjects()
        {
            //SETUP
            var analogNumeric1 = Substitute.For<TestAnalogNumericFX>();
            var analogNumeric2 = new AnalogNumericFX();
            var analogNumeric3 = new AnalogNumericFX();

            var selectedList = new List<AnalogNumericFX>() { analogNumeric1, analogNumeric2, analogNumeric3 };

            m_SelectionService.GetSelectedComponents().Returns(selectedList);

            var screenUndoService = Substitute.For<IScreenUndoService>();
            screenUndoService.OpenParentUndo(Arg.Any<string>()).Returns(new ParentUndoUnit(UndoDescription, screenUndoService));

            analogNumeric1.GetService(typeof(IScreenUndoService)).Returns(screenUndoService);

            TextIdBrowserControl textIdBrowserControl = new TextIdBrowserControl(m_SelectionService.ToILazy())
            {
                Target = analogNumeric1,
                LocalizablePropertyName = "Suffix",
                UndoText = UndoDescription
            };

            //TEST
            textIdBrowserControl.Text = "NewSuffix";
            textIdBrowserControl.RegisterUndoUnitsAndUpdateTexts();

            //ASERT
            screenUndoService.ReceivedWithAnyArgs(1).OpenParentUndo(Arg.Any<string>());
            screenUndoService.ReceivedWithAnyArgs(3).CreatePropertyUndoUnit(Arg.Any<FrameworkElement>(), Arg.Any<string>(), Arg.Any<string>());
            screenUndoService.ReceivedWithAnyArgs(1).CloseParentUndo(Arg.Any<IParentUndoUnit>());
            screenUndoService.ReceivedWithAnyArgs(1).RegisterUndoUnit(Arg.Any<IUndoUnit>());
        }

    }
    /// <summary>
    /// Test class for Label to be able to stub GetService
    /// </summary>
    public class TestLabel : Label, IServiceProvider
    {
        public virtual object GetService(Type serviceType)
        {
            // No implementation needed, stub GetService
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Test class for AnalogNumeric to be able to stub GetService
    /// </summary>
    public class TestAnalogNumericFX : AnalogNumericFX, IServiceProvider
    {
        public virtual object GetService(Type serviceType)
        {
            // No implementation needed, stub GetService
            throw new NotImplementedException();
        }
    }

}
#endif
