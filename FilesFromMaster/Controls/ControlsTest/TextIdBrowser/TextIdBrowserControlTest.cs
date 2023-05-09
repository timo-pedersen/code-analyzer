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
using NUnit.Framework;
using Rhino.Mocks;

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
            m_SelectionService = MockRepository.GenerateMock<ISelectionService>();

            // Setup project
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.VisibleLanguageColumns = new List<string>
            {
                TextsIde.DefaultText
            };

            // Setup languages
            var languageInfo1 = MockRepository.GenerateStub<ILanguageInfo>();
            languageInfo1.Name = "Language 1";

            var languageInfo2 = MockRepository.GenerateStub<ILanguageInfo>();
            languageInfo2.Name = "Language 2";

            // Setup multi language service
            m_MultiLanguageService = MockRepository.GenerateStub<IMultiLanguageService>();
            m_MultiLanguageService
                .Stub(service => service.Languages)
                .Return(new ExtendedBindingList<ILanguageInfo>
                    {
                        languageInfo1,
                        languageInfo2
                    });

            // Setup text id service
            m_TextIdService = MockRepository.GenerateStub<ITextIdService>();
            m_TextIdService
                .Stub(service => service.TextIDResourceItems)
                .Return(new[]
                    {
                       TextIdBrowserPopupViewModelTest.CreateTextIdResourceItem(1, 2)
                    });

            // Setup global reference service
            m_GlobalReferenceService = MockRepository.GenerateStub<IGlobalReferenceService>();
            m_GlobalReferenceService
                .Stub(service => service.GetObject<object>(null)).IgnoreArguments()
                .Return(new TextIdBrowserPopupViewModelTest.DependencyObjectStub());

            // Set up binding service
            m_BindingService = MockRepository.GenerateStub<IBindingService>();
        }

        [TearDown]
        public void TearDown()
        {
            m_SelectionService.BackToRecord();
            m_SelectionService.Replay();
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

            var designerResourceItem = MockRepository.GenerateStub<IDesignerResourceItem>();

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
            var label1 = MockRepository.GenerateMock<TestLabel>();
            var label2 = new Label();
            var label3 = new Label();

            var selectedList = new List<Label>() { label1, label2, label3 };

            m_SelectionService.Stub(x => x.GetSelectedComponents())
                              .Return(selectedList);

            var screenUndoService = MockRepository.GenerateMock<IScreenUndoService>();
            screenUndoService.Expect(x => x.OpenParentUndo(null))
                             .IgnoreArguments()
                             .Return(new ParentUndoUnit(UndoDescription, screenUndoService))
                             .Repeat.Once();

            screenUndoService.Expect(x => x.RegisterUndoUnit(null))
                             .IgnoreArguments()
                             .Repeat.Times(4);      //Once per selected item and once for the parent (3 + 1 = 4)

            screenUndoService.Expect(x => x.CloseParentUndo(null))
                             .IgnoreArguments()
                             .Repeat.Once();

            label1.Stub(x => x.GetService(typeof(IScreenUndoService)))
                  .Return(screenUndoService);

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
            screenUndoService.VerifyAllExpectations();
    }

        [Test]
        public void UndoIsRegisteredForMultiSelectedNonTextItemObjects()
        {
            //SETUP
            var analogNumeric1 = MockRepository.GenerateMock<TestAnalogNumericFX>();
            var analogNumeric2 = new AnalogNumericFX();
            var analogNumeric3 = new AnalogNumericFX();

            var selectedList = new List<AnalogNumericFX>() { analogNumeric1, analogNumeric2, analogNumeric3 };

            m_SelectionService.Stub(x => x.GetSelectedComponents())
                              .Return(selectedList);

            var screenUndoService = MockRepository.GenerateMock<IScreenUndoService>();
            screenUndoService.Expect(x => x.OpenParentUndo(null))
                             .IgnoreArguments()
                             .Return(new ParentUndoUnit(UndoDescription, screenUndoService))
                             .Repeat.Once();

            screenUndoService.Expect(x => x.CreatePropertyUndoUnit(null, string.Empty, string.Empty))
                             .IgnoreArguments()
                             .Repeat.Times(3);

            screenUndoService.Expect(x => x.CloseParentUndo(null))
                             .IgnoreArguments()
                             .Repeat.Once();

            screenUndoService.Expect(x => x.RegisterUndoUnit(null))
                             .IgnoreArguments()
                             .Repeat.Once();      //Once for the parent

            analogNumeric1.Stub(x => x.GetService(typeof(IScreenUndoService)))
                  .Return(screenUndoService);

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
            screenUndoService.VerifyAllExpectations();
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
