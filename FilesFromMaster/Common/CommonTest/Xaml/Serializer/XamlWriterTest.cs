using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Core.Api.GlobalReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Controls.Api.AsmMeta;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Data;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Chart;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Xaml.Serializer
{
    [TestFixture]
    public class XamlWriterTest
    {
        public static readonly string DefaultNamespaceDeclaration = string.Format("xmlns=\"{0}\"", XamlNamespaceManager.PresentationSchemaNamespace);
        public static readonly string XamlNamespaceDeclaration = string.Format("xmlns:{0}=\"{1}\"", XamlNamespaceManager.XamlSchemaPrefix, XamlNamespaceManager.XamlSchemaNamespace);

        private const int ButtonWidth = 50;
        private const int ButtonHeight = 25;
        private const string AnalogNumericFXName = "AnalogNumericFX1";
        private AnalogNumericFX m_AnalogNumericFX;
        private IXamlSerializer m_XamlSerializer;
        private XmlDocument m_XamlDocument;
        private System.ComponentModel.Design.ServiceContainer m_ServiceContainer;
        private TypeDescriptionProvider m_TypeDescriptionProvider;

        [SetUp]
        public void TestFixtureSetup()
        {
            m_AnalogNumericFX = new AnalogNumericFX
            {
                Name = AnalogNumericFXName,
                Width = ButtonWidth,
                Height = ButtonHeight
            };

            var binding = new Binding("[Controller1.D0].Value")
            {
                Source = DataItemProxyFactory.Instance
            };

            TestHelper.AddServiceStub<IGlobalReferenceService>();
            TestHelper.AddServiceStub<IMultiLanguageServiceCF>();

            m_AnalogNumericFX.SetBinding(m_AnalogNumericFX.GetDefaultDependencyProperty(), binding);
            m_ServiceContainer = new System.ComponentModel.Design.ServiceContainer();
            var securityServiceCF = TestHelper.CreateAndAddServiceStub<ISecurityServiceCF>();
            securityServiceCF.Stub(x => x.Groups).Return(new BindingList<ISecurityGroup>());

            m_TypeDescriptionProvider = new AsmMetaTypeDescriptionProviderBuilder(typeof(object))
                .Build();

            TypeDescriptor.AddProvider(m_TypeDescriptionProvider, typeof(object));
            TypeDescriptor.Refresh(typeof(SymbolIntervalMapper));

            var targetService = MockRepository.GenerateMock<ITargetService>();
            var target = MockRepository.GenerateMock<ITarget>();
            target.Stub(inv => inv.Id).Return(TargetPlatform.Windows);
            targetService.Stub(inv => inv.CurrentTarget).Return(target);
            targetService.Stub(inv => inv.CurrentTargetInfo).Return(MockRepository.GenerateMock<ITargetInfo>());
            TestHelper.AddService(targetService);

            m_XamlSerializer = new XamlSerializer(m_ServiceContainer);
            m_XamlDocument = new XmlDocument();
        }

        [TearDown]
        public void FixtureTearDown()
        {
            TypeDescriptor.RemoveProvider(m_TypeDescriptionProvider, typeof(object));
            TestHelper.ClearServices();
        }

        [Test]
        public void SerializeButton()
        {
            System.Windows.Controls.Button button = new System.Windows.Controls.Button();
            button.Content = "Button1";

            string xaml = XamlWriter.Save(button);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Button"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var contentAttribute = document.Root.Attribute("Content");
            Assert.That(contentAttribute.Value, Is.EqualTo(button.Content));
        }

        [Test]
        public void SerializeNeoTextBox()
        {
            Controls.TextBox textBox = new Controls.TextBox();
            textBox.Text = "Text with line break\r\nSecond row";

            string xaml = XamlWriter.Save(textBox);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            string textBoxNamespace = "clr-namespace:Neo.ApplicationFramework.Controls;assembly=Controls";

            var textBoxNamespaceAttribute = document.Root.Attribute(XNamespace.Xmlns + "nac");
            Assert.That(textBoxNamespaceAttribute, Is.Not.Null);
            Assert.That(textBoxNamespaceAttribute.Value, Is.EqualTo(textBoxNamespace));

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("TextBox"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(textBoxNamespace));

            var textAttribute = document.Root.Attribute("Text");
            Assert.That(textAttribute, Is.Not.Null);
            Assert.That(textAttribute.Value, Is.EqualTo(textBox.Text));
        }

        [Test]
        public void DeserializeNeoTextBox()
        {
            Controls.TextBox textBox = new Controls.TextBox();
            textBox.Text = "Text with line break\r\nSecond row";

            string xaml = XamlWriter.Save(textBox);

            Controls.TextBox deserializedTextBox = null;
            using (StringReader stringReader = new StringReader(xaml))
            {
                using (XmlTextReader xamlReader = new XmlTextReader(stringReader))
                {
                    object obj = XamlReader.Load(xamlReader);
                    Assert.IsTrue(obj is Controls.TextBox, "Deserialized object is of wrong type.");
                    deserializedTextBox = obj as Controls.TextBox;
                }
            }

            if (deserializedTextBox != null)
            {
                Assert.AreEqual(textBox.Text, deserializedTextBox.Text, "Corrupt or missing text of TextBox.");
            }
        }

        [Test]
        public void SerializeScreenWindow()
        {
            Type type = Type.GetType(TypeNames.ScreenWindow);
            object screenWindowObject = Activator.CreateInstance(type);

            string xaml = XamlWriter.Save(screenWindowObject);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            string screenNamespace = "clr-namespace:Neo.ApplicationFramework.Controls.Screen;assembly=Controls";

            var screenNamespaceAttribute = document.Root.Attribute(XNamespace.Xmlns + "nacs");
            Assert.That(screenNamespaceAttribute, Is.Not.Null);
            Assert.That(screenNamespaceAttribute.Value, Is.EqualTo(screenNamespace));

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("ScreenWindow"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(screenNamespace));
        }

        [Test]
        public void SerializeCollection()
        {
            ArrayList list = new ArrayList();
            list.Add("1");
            list.Add("2");
            list.Add("3");
            list.Add("4");

            string xaml = XamlWriter.Save(list);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            string collectionsNamespace = "clr-namespace:System.Collections;assembly=mscorlib";
            string systemNamespace = "clr-namespace:System;assembly=mscorlib";

            var collectionNamespaceAttribute = document.Root.Attribute(XNamespace.Xmlns + "sc");
            Assert.That(collectionNamespaceAttribute, Is.Not.Null);
            Assert.That(collectionNamespaceAttribute.Value, Is.EqualTo(collectionsNamespace));

            var systemNamespaceAttribute = document.Root.Attribute(XNamespace.Xmlns + "s");
            Assert.That(systemNamespaceAttribute, Is.Not.Null);
            Assert.That(systemNamespaceAttribute.Value, Is.EqualTo(systemNamespace));

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("ArrayList"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(collectionsNamespace));

            var itemsElement = document.Root.Element(XNamespace.Get(collectionsNamespace) + "ArrayList.Items");
            Assert.That(itemsElement, Is.Null);

            var stringElements = document.Root.Elements(XNamespace.Get(systemNamespace) + typeof(string).Name).ToList();
            Assert.That(stringElements, Has.Count.EqualTo(4));
            Assert.That(stringElements[0].Value, Is.EqualTo("1"));
            Assert.That(stringElements[1].Value, Is.EqualTo("2"));
            Assert.That(stringElements[2].Value, Is.EqualTo("3"));
            Assert.That(stringElements[3].Value, Is.EqualTo("4"));
        }

        [Test]
        public void SerializeCollectionPropertyWithObjectDotPropertyName()
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            string xaml = XamlWriter.Save(grid);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Grid"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var rowsElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "Grid.RowDefinitions");
            Assert.That(rowsElement, Is.Not.Null);

            var rowElement = rowsElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "RowDefinition");
            Assert.That(rowElement, Is.Not.Null);
        }

        [Test]
        public void SerializeCollectionPropertyWithOnlyPropertyName()
        {
            Grid grid = new Grid();
            grid.Children.Add(new System.Windows.Controls.Button());
            grid.Children.Add(new System.Windows.Controls.Button());

            string xaml = XamlWriter.Save(grid);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Grid"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var childrenElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "Grid.Children");
            Assert.That(childrenElement, Is.Null);

            var buttonElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "Button");
            Assert.That(buttonElement, Is.Not.Null);
        }

        [Test]
        public void SerializePointProperty()
        {
            System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
            rectangle.RenderTransformOrigin = new Point(0.75, 0.75);

            string xaml = XamlWriter.Save(rectangle);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Rectangle"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var renderTransformOriginAttribute = document.Root.Attribute("RenderTransformOrigin");
            Assert.That(renderTransformOriginAttribute, Is.Not.Null);
            Assert.That(renderTransformOriginAttribute.Value, Is.EqualTo("0.75,0.75"));
        }

        [Test]
        public void SerializeLinearGradient()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0f));
            linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.5));

            System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();

            rectangle.Fill = linearGradientBrush;

            string xaml = XamlWriter.Save(rectangle);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Rectangle"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var fillElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "Rectangle.Fill");
            Assert.That(fillElement, Is.Not.Null);

            var brushElement = fillElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "LinearGradientBrush");
            Assert.That(brushElement, Is.Not.Null);

            var gradientStopsElement = brushElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "LinearGradientBrush.GradientStops");
            Assert.That(gradientStopsElement, Is.Not.Null);

            var gradientStopCollectionElement = gradientStopsElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "GradientStopCollection");
            Assert.That(gradientStopCollectionElement, Is.Not.Null);
        }

        [Test]
        public void SerializeTextDecorationsProperty()
        {
            System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();
            textBox.Text = "Tjoho!";
            textBox.TextDecorations.Add(TextDecorations.Strikethrough);

            string xaml = XamlWriter.Save(textBox);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("TextBox"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var textDecorationsElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "TextBox.TextDecorations");
            Assert.That(textDecorationsElement, Is.Not.Null);

            var textDecorationCollectionElement = textDecorationsElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "TextDecorationCollection");
            Assert.That(textDecorationCollectionElement, Is.Not.Null);

            var textDecorationElement = textDecorationCollectionElement.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "TextDecoration");
            Assert.That(textDecorationElement, Is.Not.Null);

            var locationAttribute = textDecorationElement.Attribute("Location");
            Assert.That(locationAttribute, Is.Not.Null);
            Assert.That(locationAttribute.Value, Is.EqualTo("Strikethrough"));
        }

        [Test]
        public void SerializeMeterWithStyle()
        {
            CircularMeter circularMeter = new CircularMeter();

            string xaml = XamlWriter.Save(circularMeter);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("CircularMeter"));
            Assert.That(rootName.NamespaceName, Is.EqualTo("clr-namespace:Neo.ApplicationFramework.Controls.Controls;assembly=Controls"));

            var stylePropertyElement = document.Root.Element(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "CircularMeter.Style");
            Assert.That(stylePropertyElement, Is.Null);

            var styleElement = document.Root.Descendants(XNamespace.Get(XamlNamespaceManager.PresentationSchemaNamespace) + "Style").FirstOrDefault();
            Assert.That(styleElement, Is.Null);
        }

        [Test]
        public void SerializeSecurityProperties()
        {
            Controls.Button button = new Controls.Button();
            SecurityProperties.SetVisibilityOnAccessDenied(button, VisibilityModes.Disabled);

            string xaml = XamlWriter.Save(button);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Button"));
            Assert.That(rootName.NamespaceName, Is.EqualTo("clr-namespace:Neo.ApplicationFramework.Controls;assembly=Controls"));

            var securityPropertiesAttribute = document.Root.Attribute("{clr-namespace:Neo.ApplicationFramework.Interfaces;assembly=Interfaces}" + "SecurityProperties.VisibilityOnAccessDenied");
            Assert.That(securityPropertiesAttribute, Is.Not.Null);
            Assert.That(securityPropertiesAttribute.Value, Is.EqualTo("Disabled"));
        }

        [Test]
        public void SerializeWindowsFormsHostProperties()
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button();

            WindowsFormsHost windowsFormsHost = new WindowsFormsHost();
            windowsFormsHost.Child = button;

            string xaml = XamlWriter.Save(windowsFormsHost);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("WindowsFormsHost"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var buttonElement = document.Root.Element("{clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms}" + "Button");
            Assert.That(buttonElement, Is.Not.Null);

            var marginAttribute = buttonElement.Attribute("Margin");
            Assert.That(marginAttribute, Is.Not.Null);
            Assert.That(marginAttribute.Value, Is.EqualTo("0, 0, 0, 0"));
        }

        [Test]
        public void SerializeAndDeserializeFrameworkElementWithBinding()
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            elements.Add(m_AnalogNumericFX);
            m_XamlDocument.InnerXml = m_XamlSerializer.Serialize(elements);

            IList<FrameworkElement> deserializedElements = m_XamlSerializer.Deserialize(m_XamlDocument.InnerXml);

            Assert.AreEqual(1, deserializedElements.Count, "Could not deserialize document");

            var bindingExpression = BindingOperations.GetBindingExpression(deserializedElements[0], deserializedElements[0].GetDefaultDependencyProperty());

            Assert.IsNotNull(bindingExpression, "BindingExpression should not be null.");
            Assert.IsNotNull(bindingExpression.ParentBinding, "Binding should not be null.");
        }

        [Test]
        public void SerializeAndDeserializeTwoBindings()
        {
            DependencyProperty dependencyProperty = m_AnalogNumericFX.GetDependencyProperty("MinWidth");

            Binding binding = new Binding("[Controller1.D1].Value");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = DataItemProxyFactory.Instance;
            m_AnalogNumericFX.SetBinding(dependencyProperty, binding);

            List<FrameworkElement> elements = new List<FrameworkElement>();
            elements.Add(m_AnalogNumericFX);
            m_XamlDocument.InnerXml = m_XamlSerializer.Serialize(elements);

            IList<FrameworkElement> deserializedElements = m_XamlSerializer.Deserialize(m_XamlDocument.InnerXml);
            Assert.AreEqual(1, deserializedElements.Count, "Could not deserialize document");

            int bindingCount = GetNumberOfBoundProperties(deserializedElements[0]);

            Assert.AreEqual(2, bindingCount, "Object should contain 2 bindings.");
        }

        [Test]
        public void SerializedUserControlShouldHaveNoChildren()
        {
            TestUserControlFX testUserControl = new TestUserControlFX() { Name = "m_UserControl" };
            string xaml = XamlWriter.Save(testUserControl);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xaml);

            XmlNode xmlNode = xmlDocument.SelectSingleNode("//*[@Name='m_UserControl']");
            Assert.IsNotNull(xmlNode);
            Assert.AreEqual(0, xmlNode.ChildNodes.Count);
        }

        [Test]
        public void SerializeUserControlWithCollectionPropertySet()
        {
            TestUserControlFX testUserControl = new TestUserControlFX() { Name = "m_UserControl" };
            testUserControl.TestStrings.Add("Item1");
            testUserControl.TestStrings.Add("Item2");

            string xaml = XamlWriter.Save(testUserControl);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xaml);

            XmlNode xmlNode = xmlDocument.SelectSingleNode("//*[@Name='m_UserControl']");

            Assert.AreEqual(1, xmlNode.ChildNodes.Count);
            Assert.AreEqual("TestUserControlFX.TestStrings", xmlNode.ChildNodes[0].LocalName);

            XmlNode arrayListNode = xmlNode.ChildNodes[0];

            Assert.AreEqual(2, arrayListNode.ChildNodes.Count);
            Assert.AreEqual("Item1", arrayListNode.ChildNodes[0].InnerText);
            Assert.AreEqual("Item2", arrayListNode.ChildNodes[1].InnerText);
        }

        [Test]
        public void SerializationWhenUserControlIsInCanvas()
        {
            TestUserControlFX testUserControl = new TestUserControlFX() { Name = "m_UserControl" };
            testUserControl.TestStrings.Add("Item1");
            testUserControl.TestStrings.Add("Item2");

            Canvas canvas = new Canvas();
            canvas.Children.Add(testUserControl);

            string xaml = XamlWriter.Save(canvas);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xaml);

            XmlNode xmlNode = xmlDocument.SelectSingleNode("//*[@Name='m_UserControl']");

            Assert.AreEqual(1, xmlNode.ChildNodes.Count);
            Assert.AreEqual("TestUserControlFX.TestStrings", xmlNode.ChildNodes[0].LocalName);

            XmlNode arrayListNode = xmlNode.ChildNodes[0];

            Assert.AreEqual(2, arrayListNode.ChildNodes.Count);
            Assert.AreEqual("Item1", arrayListNode.ChildNodes[0].InnerText);
            Assert.AreEqual("Item2", arrayListNode.ChildNodes[1].InnerText);
        }

        [Test]
        public void WpfButtonIsInDefaultXamlNamespace()
        {
            Assert.That(XamlWriter.IsWPFDefaultNamespace(typeof(System.Windows.Controls.Button)), Is.True);
        }

        [Test]
        public void NeoButtonIsNotInDefaultXamlNamespace()
        {
            Assert.That(XamlWriter.IsWPFDefaultNamespace(typeof(Controls.Button)), Is.False);
        }

        [Test]
        public void SerializeButtonWithNoSymbolsDoesNotSerializeEmptySymbolIntervalMapper()
        {
            Controls.Button button = new Controls.Button();
            button.SymbolIntervalMapper = new SymbolIntervalMapper();

            string xaml = XamlWriter.Save(button);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            string buttonNamespace = "clr-namespace:Neo.ApplicationFramework.Controls;assembly=Controls";

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Button"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(buttonNamespace));

            var symbolIntervalMapperElement = document.Root.Descendants(XNamespace.Get(buttonNamespace) + "Button.SymbolIntervalMapper").FirstOrDefault();
            Assert.That(symbolIntervalMapperElement, Is.Null, "SymbolIntervalMapper shouldn't be serialized when empty.");
        }

        [Test]
        public void SerializeButtonWithSymbolsSerializesSymbolIntervalMapper()
        {
            Controls.Button button = new Controls.Button();
            button.SymbolIntervalMapper = new SymbolIntervalMapper();
            button.SymbolIntervalMapper.AddInterval(0, 0, null);

            string xaml = XamlWriter.Save(button);
            XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);

            AssertXamlRootIsValid(document);

            string buttonNamespace = "clr-namespace:Neo.ApplicationFramework.Controls;assembly=Controls";

            var rootName = document.Root.Name;
            Assert.That(rootName.LocalName, Is.EqualTo("Button"));
            Assert.That(rootName.NamespaceName, Is.EqualTo(buttonNamespace));

            var symbolIntervalMapperElement = document.Root.Descendants(XNamespace.Get(buttonNamespace) + "Button.SymbolIntervalMapper").FirstOrDefault();
            Assert.That(symbolIntervalMapperElement, Is.Not.Null, "SymbolIntervalMapper should be serialized when not empty.");
        }

        [Test]
        public void SerializeControlWithChangedDefaultValue()
        {
            HorizontalAlignment newAlignmentValue = HorizontalAlignment.Right;
            Assert.That(Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue, Is.Not.EqualTo(newAlignmentValue));

            Control control = new Control();
            control.HorizontalContentAlignment = newAlignmentValue;

            string xaml = XamlWriter.Save(control);

            Assert.That(xaml, Does.Contain(Control.HorizontalContentAlignmentProperty.Name));
        }

        [Test]
        public void SerializeControlWithDefaultValueDoesNotSerializeProperty()
        {
            Control control = new Control();

            string xaml = XamlWriter.Save(control);

            Assert.That(xaml, Is.Not.Contains(Control.HorizontalContentAlignmentProperty.Name));
        }

        [Test]
        public void SerializeControlWithDefaultValueChangedInStyleDoesNotSerializeProperty()
        {
            HorizontalAlignment styleAlignmentValue = HorizontalAlignment.Right;
            Assert.That(Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue, Is.Not.EqualTo(styleAlignmentValue));

            Style newStyle = new Style(typeof(Control));
            newStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, styleAlignmentValue));

            Control control = new Control();
            control.Style = newStyle;

            Assert.That(control.HorizontalContentAlignment, Is.EqualTo(styleAlignmentValue));

            string xaml = XamlWriter.Save(control);

            Assert.That(xaml, Is.Not.Contains(Control.HorizontalContentAlignmentProperty.Name));
        }

        [Test]
        public void SerializeControlWithDefaultValueChangedInStyleAndValueSetToSameAsMetadataDefault()
        {
            HorizontalAlignment styleAlignmentValue = HorizontalAlignment.Right;
            Assert.That(Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue, Is.Not.EqualTo(styleAlignmentValue));

            Style newStyle = new Style(typeof(Control));
            newStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, styleAlignmentValue));

            Control control = new Control();
            control.Style = newStyle;
            control.HorizontalContentAlignment = (HorizontalAlignment)Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue;

            string xaml = XamlWriter.Save(control);

            Assert.That(xaml, Does.Contain(Control.HorizontalContentAlignmentProperty.Name));
        }

        [Test]
        public void SerializeControlWithDefaultValueChangedInBasedOnStyleAndValueSetToSameAsMetadataDefault()
        {
            HorizontalAlignment styleAlignmentValue = HorizontalAlignment.Right;
            Assert.That(Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue, Is.Not.EqualTo(styleAlignmentValue));

            Style baseStyle = new Style(typeof(Control));
            baseStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, styleAlignmentValue));

            Style derivedStyle = new Style(typeof(Control), baseStyle);
            derivedStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Bottom));

            Control control = new Control();
            control.Style = derivedStyle;
            control.HorizontalContentAlignment = (HorizontalAlignment)Control.HorizontalContentAlignmentProperty.DefaultMetadata.DefaultValue;

            string xaml = XamlWriter.Save(control);

            Assert.That(xaml, Does.Contain(Control.HorizontalContentAlignmentProperty.Name));
        }

        [Test]
        public void SerializeLabelWithPropertySetInStyleDoesNotSerializeValue()
        {
            Controls.Label label = new Controls.Label();
            label.BeginInit();
            label.EndInit();
            label.ApplyTemplate();
            TextBlock containedTextBlock = VisualTreeNavigator.FindElementOfType(label, typeof(TextBlock)) as TextBlock;

            Assert.That(label.FontFamily, Is.EqualTo(ApplicationConstants.DefaultFontFamily));
            Assert.That(label.FontFamily, Is.EqualTo(containedTextBlock.FontFamily));

            string xaml = XamlWriter.Save(label);
            Assert.That(xaml, Is.Not.Contains(Control.FontFamilyProperty.Name));
        }

        [Test]
        public void SerializeLabelWithPropertySetInStyleAndThenResetToDefaultValueDoesSerializeValue()
        {
            Controls.Label label = new Controls.Label();
            label.BeginInit();
            label.EndInit();
            label.ApplyTemplate();
            TextBlock containedTextBlock = VisualTreeNavigator.FindElementOfType(label, typeof(TextBlock)) as TextBlock;

            Assert.That(label.FontFamily, Is.EqualTo(ApplicationConstants.DefaultFontFamily));
            Assert.That(label.FontFamily, Is.EqualTo(containedTextBlock.FontFamily));

            label.FontFamily = (FontFamily)Control.FontFamilyProperty.DefaultMetadata.DefaultValue;
            Assert.That(label.FontFamily, Is.EqualTo(containedTextBlock.FontFamily));

            string xaml = XamlWriter.Save(label);
            Assert.That(xaml, Does.Contain(Control.FontFamilyProperty.Name));
        }

        [Test]
        public void SerializeCollectionOfEmptyStrings()
        {
            LabelsList labelsList = new LabelsList() { "Kotte", string.Empty, "Melon" };

            string xaml = XamlWriter.Save(labelsList);
            Assert.That(xaml, Does.Contain("String.Empty"));
        }

        [Test]
        public void SerializeStyleWithNamespaceQualifiedPropertyName()
        {
            Style style = new Style();
            var setter = new Setter
            {
                Property = ActionProperties.ActionsProperty
            };

            style.Setters.Add(setter);

            string xaml = XamlWriter.Save(style);
            Assert.That(xaml, Does.Contain("Property=\"nai:ActionProperties.Actions\""));
        }

        private static void AssertXamlRootIsValid(XDocument document)
        {
            var processingInstruction = document.Nodes().OfType<XProcessingInstruction>().Where(instruction => instruction.Target == SerializerConstants.ProcessingInstructionNeoTargetName).FirstOrDefault();
            Assert.That(processingInstruction, Is.Not.Null);
            Assert.That(processingInstruction.Data.Contains("version="));

            Assert.That(document.Root, Is.Not.Null);

            var defaultNamespaceDeclaration = document.Root.Attribute(XamlNamespaceManager.XmlNamespaceDefinition);
            Assert.That(defaultNamespaceDeclaration, Is.Not.Null);
            Assert.That(defaultNamespaceDeclaration.Value, Is.EqualTo(XamlNamespaceManager.PresentationSchemaNamespace));

            var xamlNamespaceDeclaration = document.Root.Attribute(XNamespace.Xmlns + XamlNamespaceManager.XamlSchemaPrefix);
            Assert.That(xamlNamespaceDeclaration, Is.Not.Null);
            Assert.That(xamlNamespaceDeclaration.Value, Is.EqualTo(XamlNamespaceManager.XamlSchemaNamespace));
        }

        private static int GetNumberOfBoundProperties(FrameworkElement element)
        {
            return TypeDescriptor
                .GetProperties(element)
                .Cast<PropertyDescriptor>()
                .Count(propertyDescriptor =>
                           {
                               var dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(propertyDescriptor);
                               if (dependencyPropertyDescriptor == null)
                                   return false;

                               return BindingOperations.IsDataBound(element, dependencyPropertyDescriptor.DependencyProperty);
                           });
        }

    }
}
