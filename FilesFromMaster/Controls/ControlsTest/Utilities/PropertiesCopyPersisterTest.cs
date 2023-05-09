using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Core.Api.ProjectTarget;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common;
using Neo.ApplicationFramework.Common.MultiLanguage;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.TextIdBrowser;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Utilities
{
    [TestFixture]
    public class PropertiesCopyPersisterTest
    {
        private IMultiLanguagePropertyBinder m_MultiLanguagePropertyBinder;
        private ILazy<IPropertyBinderFactory> m_PropertyBinderFactory;
        private IScreenDesignerView m_ScreenDesignerView;
        private TestLabel m_FromObject;

        [SetUp]
        public void Setup()
        {
            m_MultiLanguagePropertyBinder = MockRepository.GenerateStub<IMultiLanguagePropertyBinder>();
            m_PropertyBinderFactory = MockRepository.GenerateStub<IPropertyBinderFactory>().ToILazy();

            var terminalStub = MockRepository.GenerateStub<ITerminal>();
            var targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;

            ITarget targetStub = MockRepository.GenerateStub<ITarget>();

            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.Stub(x => x.CurrentTargetInfo).Return(targetInfoStub);

            m_ScreenDesignerView = MockRepository.GenerateStub<IScreenDesignerView>();
            m_FromObject = MockRepository.GenerateMock<TestLabel>();
            m_FromObject.Stub(x => x.GetService(typeof(IScreenDesignerView)))
                  .Return(m_ScreenDesignerView);
        }

        [Test]
        public void CopyTextAlsoCopiesMultiText()
        {
            //SETUP
            TestHelper.UseTestWindowThreadHelper = true;

            var propertiesCopyPersister = new PropertiesCopyPersister(m_PropertyBinderFactory,
                                                                        m_MultiLanguagePropertyBinder);

            var descriptorCollection = TypeDescriptor.GetProperties(m_FromObject);
            var propertyDescriptor = descriptorCollection.Find("Text", false);
            var propertyDescriptors = new List<PropertyDescriptor>() { propertyDescriptor };

            //TEST
            propertiesCopyPersister.Copy(m_FromObject, propertyDescriptors);
            IDataObject dataObject = NeoClipboard.GetDataObject();
            var propertiesDataObject = dataObject.GetData(PropertiesDataObject.ClipboardFormat) as PropertiesDataObject;

            //ASSERT
            Assert.IsNotNull(propertiesDataObject);
            // 4 properties with same DependentPropertyGroupAttribute: Text, Texts, TextIntervalMapper, Value
            var textProperty = propertiesDataObject.PropertyValues.Find(x => x.PropertyName == "Text");
            Assert.IsNotNull(textProperty);

            Assert.AreEqual(4, propertiesDataObject.PropertyValues.Count);
            var textsProperty = propertiesDataObject.PropertyValues.Find(x => x.PropertyName == "Texts");
            Assert.IsNotNull(textsProperty);

            var textIntervalMapperProperty = propertiesDataObject.PropertyValues.Find(x => x.PropertyName == "TextIntervalMapper");
            Assert.IsNotNull(textIntervalMapperProperty);

            var valueProperty = propertiesDataObject.PropertyValues.Find(x => x.PropertyName == "Value");
            Assert.IsNotNull(valueProperty);
        }

        [Test]
        public void StylePropertyIsLast()
        {
            // ARRANGE
            TestHelper.UseTestWindowThreadHelper = true;

            var propertiesCopyPersister = new PropertiesCopyPersister(m_PropertyBinderFactory,
                                                                        m_MultiLanguagePropertyBinder);

            var button = MockRepository.GenerateMock<TestButton>();
            button.Stub(x => x.GetService(typeof(IScreenDesignerView)))
                  .Return(m_ScreenDesignerView);

            var descriptorCollection = TypeDescriptor.GetProperties(button);
            var stylePropertyDescriptor = descriptorCollection.Find(RenderableControl.StyleNamePropertyName, false);
            var valuePropertyDescriptor = descriptorCollection.Find("Value", false);

            var propertyDescriptors = new List<PropertyDescriptor>() { stylePropertyDescriptor, valuePropertyDescriptor };

            // ACT
            propertiesCopyPersister.Copy(button, propertyDescriptors);
            IDataObject dataObject = NeoClipboard.GetDataObject();
            var propertiesDataObject = dataObject.GetData(PropertiesDataObject.ClipboardFormat) as PropertiesDataObject;
            // ASSERT
            Assert.IsNotNull(propertiesDataObject);
            var styleIndex = propertiesDataObject.PropertyValues.FindIndex(x => x.PropertyName == RenderableControl.StyleNamePropertyName);
            Assert.AreEqual(styleIndex, propertiesDataObject.PropertyValues.Count - 1);
        }

    }

    /// <summary>
    /// Test class for Button to be able to stub GetService
    /// </summary>
    public class TestButton : Button, IServiceProvider
    {
        public virtual object GetService(Type serviceType)
        {
            // No implementation needed, stub GetService
            throw new NotImplementedException();
        }
    }
}
