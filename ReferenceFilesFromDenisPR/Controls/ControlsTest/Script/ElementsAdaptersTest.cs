#if !VNEXT_TARGET
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Graphics.Logic;
using Neo.ApplicationFramework.Common.TypeConverters;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Controls.Script;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.PropertyGrid;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls
{
    [TestFixture]
    public class ElementsAdaptersTest
    {
        public const string FillPropertyName = "Fill";
        public const string OutlinePropertyName = "Outline";
        public const string OutlineThicknessPropertyName = "OutlineThickness";

        private WPFToCFTypeDescriptionProvider m_WPFToCFTypeDescriptionProvider;
        private IToolManager m_ToolManager;
        private ElementCanvas m_ElementCanvas;

        [SetUp]
        public void TestFixtureSetup()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_WPFToCFTypeDescriptionProvider = new WPFToCFTypeDescriptionProvider(typeof(object));
            TypeDescriptor.AddProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            m_ToolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_ToolManager.Runtime.Returns(true);

            TestHelper.CreateAndAddServiceStub<IPropertyBinderFactory>();
            TestHelper.AddService<IObjectPropertyService>(new ObjectPropertyService());
            TestHelper.SetupServicePlatformFactory(Substitute.For<IKeyboardHelper>());

            TestHelper.UseTestWindowThreadHelper = true;

            m_ElementCanvas = new ElementCanvas();
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            TypeDescriptor.RemoveProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            TestHelper.ClearServices();
            m_ElementCanvas.Children.Clear();
        }

#region Text Support

        [Test]
        public void TestThatButtonIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            button.Text = "Button1";

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            IButtonAdapter buttonAdapter = elementAdapterService.CreateAdapter<ButtonAdapter>(button);
            Assert.IsNotNull(buttonAdapter, "Could not create an adapter for button.");

            Assert.AreEqual(button.Text, buttonAdapter.Text, "Text not equal for Text.");
        }

        [Test]
        public void TestThatLabelIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Label label = new Neo.ApplicationFramework.Controls.Label();
            label.Text = "Label1";

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            ITextControlAdapter textElementAdapter = elementAdapterService.CreateAdapter<TextElementAdapter>(label);
            Assert.IsNotNull(textElementAdapter, "Could not create an adapter for label.");

            Assert.AreEqual(label.Text, textElementAdapter.Text, "Text not equal for Text.");
        }

        [Test]
        public void TestThatTextBoxIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.TextBox textbox = new Neo.ApplicationFramework.Controls.TextBox();
            textbox.Text = "Text";

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            ITextBoxAdapter textBoxAdapter = elementAdapterService.CreateAdapter<TextBoxAdapter>(textbox);
            Assert.IsNotNull(textBoxAdapter, "Could not create an adapter for textbox.");

            Assert.AreEqual(textbox.Text, textBoxAdapter.Text, "Text not equal for TextBox.");
        }

#endregion

#region AnalogNumeric

        [Test]
        public void TestThatAnalogNumericCFIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.AnalogNumericFX analogNumeric = new Neo.ApplicationFramework.Controls.AnalogNumericFX();
            analogNumeric.Value = "123";

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            IAnalogNumericAdapter analogNumericAdapter = elementAdapterService.CreateAdapter<AnalogNumericAdapter>(analogNumeric);
            Assert.IsNotNull(analogNumericAdapter, "Could not create an adapter for AnalogNumeric.");

            Assert.AreEqual(analogNumeric.Value.ToString(), analogNumericAdapter.Value.ToString(), "Value not equal for AnalogNumeric.");
        }

#endregion

#region Line

        [Test]
        public void TestThatLineIsSupportedByAnAdapter()
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.X1 = 10;
            line.Y1 = 11;
            line.X2 = 12;
            line.Y2 = 13;

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            LineAdapter lineAdapter = elementAdapterService.CreateAdapter<LineAdapter>(line);
            Assert.IsNotNull(lineAdapter, "Could not create an adapter for line.");

            Assert.AreEqual(line.X1, lineAdapter.X1, "X1 not equal for line");
            Assert.AreEqual(line.Y1, lineAdapter.Y1, "Y1 not equal for line");
            Assert.AreEqual(line.X2, lineAdapter.X2, "X2 not equal for line");
            Assert.AreEqual(line.Y2, lineAdapter.Y2, "Y2 not equal for line");
        }

#endregion

#region Size and Position Support

        [Test]
        public void TestThatARectangleSupportsSizeAndPosition()
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Height = 10;
            rectangle.Width = 20;

            System.Windows.Controls.Canvas canvas = new System.Windows.Controls.Canvas();
            canvas.Width = 100;
            canvas.Height = 100;
            canvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, 30);
            Canvas.SetTop(rectangle, 40);

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            ElementAdapter elementAdapter = elementAdapterService.CreateAdapter<ElementAdapter>(rectangle);
            Assert.IsNotNull(elementAdapter, "Could not create an adapter for rectangle");

            Assert.AreEqual(rectangle.Height, elementAdapter.Height, "The heights are not equal.");
            Assert.AreEqual(rectangle.Width, elementAdapter.Width, "The heights are not equal.");
            Assert.AreEqual(Convert.ToInt32(Canvas.GetLeft(rectangle)), elementAdapter.Left, "The heights are not equal.");
            Assert.AreEqual(Convert.ToInt32(Canvas.GetTop(rectangle)), elementAdapter.Top, "The heights are not equal.");
        }

        [Test]
        public void TestThatAButtonSupportsSizeAndPosition()
        {
            Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
            button.Height = 10;
            button.Width = 20;

            System.Windows.Controls.Canvas canvas = new System.Windows.Controls.Canvas();
            canvas.Width = 100;
            canvas.Height = 100;
            canvas.Children.Add(button);
            Canvas.SetLeft(button, 30);
            Canvas.SetTop(button, 40);

            IElementAdapterService elementAdapterService = new ElementAdapterService();
            ButtonAdapter buttonAdapter = elementAdapterService.CreateAdapter<ButtonAdapter>(button);
            Assert.IsNotNull(buttonAdapter, "Could not create an adapter for button");

            Assert.AreEqual(button.Height, buttonAdapter.Height, "The heights are not equal.");
            Assert.AreEqual(button.Width, buttonAdapter.Width, "The heights are not equal.");
            Assert.AreEqual(Convert.ToInt32(Canvas.GetLeft(button)), buttonAdapter.Left, "The heights are not equal.");
            Assert.AreEqual(Convert.ToInt32(Canvas.GetTop(button)), buttonAdapter.Top, "The heights are not equal.");
        }

#endregion

        private LinearGradientBrush GetLinearGradientBrush(Color startColor, Color endColor)
        {
            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            gradientStopCollection.Add(new GradientStop(startColor, 0));
            gradientStopCollection.Add(new GradientStop(endColor, 1));
            return new LinearGradientBrush(gradientStopCollection, new System.Windows.Point(0, 0.5), new System.Windows.Point(1, 0.5));
        }

        private BrushCF GetLinearGradientBrushCF(System.Drawing.Color startColor, System.Drawing.Color endColor)
        {
            return new BrushCF(startColor, endColor, FillDirection.HorizontalLeftToRight);
        }


#region ColorPropertiesBrowsableTest
        [Test]
        public void TestButtonColorPropertiesAreBrowsable()
        {
            ButtonAdapter buttonAdapter = new ButtonAdapter();
            buttonAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.Button();

            Assert.IsTrue(IsBrowsable(buttonAdapter, FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(buttonAdapter, OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(buttonAdapter, OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestTextBoxColorPropertiesAreBrowsable()
        {
            TextBoxAdapter textBoxAdapter = new TextBoxAdapter();
            textBoxAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.TextBox();

            Assert.IsTrue(IsBrowsable(textBoxAdapter, FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(textBoxAdapter, OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(textBoxAdapter, OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestAnalogNumericColorPropertiesAreBrowsable()
        {
            AnalogNumericAdapter analogNumericAdapter = new AnalogNumericAdapter();
            analogNumericAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.AnalogNumericFX();

            Assert.IsTrue(IsBrowsable(analogNumericAdapter, FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(analogNumericAdapter, OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(analogNumericAdapter, OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestRectangleColorPropertiesAreBrowsable()
        {
            ColorElementAdapter elementAdapter = new ColorElementAdapter();
            elementAdapter.AdaptedObject = new Rectangle();

            Assert.IsTrue(IsBrowsable(elementAdapter, FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(elementAdapter, OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(elementAdapter, OutlineThicknessPropertyName), "Border thickness is not browsable");
        }
#endregion

#region ButtonColorProperites
        [Test]
        public void TestButtonFill()
        {
            Button button = CreateAndAddElementToNeoCanvas<Button>();
            button.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);

            ButtonAdapter buttonAdapter = new ButtonAdapter();
            buttonAdapter.AdaptedObject = button;

            Assert.AreEqual(System.Drawing.Color.Red.ToArgb(), buttonAdapter.Fill.StartColor.ToArgb());
            Assert.AreEqual(System.Drawing.Color.Blue.ToArgb(), buttonAdapter.Fill.EndColor.ToArgb());

            buttonAdapter.Fill = GetLinearGradientBrushCF(System.Drawing.Color.Black, System.Drawing.Color.Brown);

            AssertBrushesAreEqual(GetLinearGradientBrush(Colors.Black, Colors.Brown), button.Background);
        }

        //[Test]
        //public void TestButtonForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.Button button = new Neo.ApplicationFramework.Controls.Button();
        //    button.Foreground = Brushes.Black;

        //    ButtonAdapter buttonAdapter = new ButtonAdapter();
        //    buttonAdapter.AdaptedObject = button;

        //    Assert.AreEqual(Brushes.Black.ToString(), buttonAdapter.FontColor.ToString());

        //    buttonAdapter.FontColor = new BrushCF(System.Drawing.Color.Yellow);

        //    AssertBrushesAreEqual(Brushes.Yellow, button.Foreground);
        //}

        [Test]
        public void TestButtonBorderColor()
        {
            Button button = CreateAndAddElementToNeoCanvas<Button>();
            button.BorderBrush = new SolidColorBrush(Colors.Black);

            ButtonAdapter buttonAdapter = new ButtonAdapter();
            buttonAdapter.AdaptedObject = button;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), buttonAdapter.Outline.StartColor.ToArgb());

            buttonAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), button.BorderBrush);
        }
#endregion

#region TextBoxColorProperties
        [Test]
        public void TestTextBoxPrimaryColor()
        {
            TextBox textBox = CreateAndAddElementToNeoCanvas<TextBox>();
            textBox.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);

            TextBoxAdapter textBoxAdapter = new TextBoxAdapter();
            textBoxAdapter.AdaptedObject = textBox;

            Assert.AreEqual(System.Drawing.Color.Red.ToArgb(), textBoxAdapter.Fill.StartColor.ToArgb());
            Assert.AreEqual(System.Drawing.Color.Blue.ToArgb(), textBoxAdapter.Fill.EndColor.ToArgb());

            textBoxAdapter.Fill = GetLinearGradientBrushCF(System.Drawing.Color.Yellow, System.Drawing.Color.Blue);

            AssertBrushesAreEqual(GetLinearGradientBrush(Colors.Yellow, Colors.Blue), textBox.Background);
        }


        //[Test]
        //public void TestTextBoxForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.TextBox textBox = new Neo.ApplicationFramework.Controls.TextBox();
        //    textBox.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
        //    textBox.Foreground = new SolidColorBrush(Colors.Black);

        //    TextBoxAdapter textBoxAdapter = new TextBoxAdapter();
        //    textBoxAdapter.AdaptedObject = textBox;

        //    Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), textBoxAdapter.ForegroundColor.ToArgb());

        //    textBoxAdapter.ForegroundColor = System.Drawing.Color.Yellow;

        //    AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), textBox.Foreground);
        //}

        [Test]
        public void TestTextBoxBorderColor()
        {
            TextBox textBox = CreateAndAddElementToNeoCanvas<TextBox>();
            textBox.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
            textBox.Foreground = new SolidColorBrush(Colors.Black);
            textBox.BorderBrush = new SolidColorBrush(Colors.Black);

            TextBoxAdapter textBoxAdapter = new TextBoxAdapter();
            textBoxAdapter.AdaptedObject = textBox;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), textBoxAdapter.Outline.StartColor.ToArgb());

            textBoxAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), textBox.BorderBrush);
        }
#endregion

#region AnalogNumericColorProperties

        [Test]
        public void TestAnalogNumericFill()
        {
            AnalogNumericFX analogNumeric = CreateAndAddElementToNeoCanvas<AnalogNumericFX>();
            analogNumeric.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);

            AnalogNumericAdapter analogNumericAdapter = new AnalogNumericAdapter();
            analogNumericAdapter.AdaptedObject = analogNumeric;

            Assert.AreEqual(System.Drawing.Color.Red.ToArgb(), analogNumericAdapter.Fill.StartColor.ToArgb());
            Assert.AreEqual(System.Drawing.Color.Blue.ToArgb(), analogNumericAdapter.Fill.EndColor.ToArgb());

            analogNumericAdapter.Fill = GetLinearGradientBrushCF(System.Drawing.Color.Yellow, System.Drawing.Color.Blue);

            AssertBrushesAreEqual(GetLinearGradientBrush(Colors.Yellow, Colors.Blue), analogNumeric.Background);
        }

        //[Test]
        //public void TestAnalogNumericForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.AnalogNumericFX analogNumeric = new Neo.ApplicationFramework.Controls.AnalogNumericFX();
        //    analogNumeric.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
        //    analogNumeric.Foreground = new SolidColorBrush(Colors.Black);

        //    AnalogNumericAdapter analogNumericAdapter = new AnalogNumericAdapter();
        //    analogNumericAdapter.AdaptedObject = analogNumeric;

        //    Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), analogNumericAdapter.ForegroundColor.ToArgb());

        //    analogNumericAdapter.ForegroundColor = System.Drawing.Color.Yellow;

        //    AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), analogNumeric.Foreground);
        //}

        [Test]
        public void TestAnalogNumericBorderColor()
        {
            AnalogNumericFX analogNumeric = CreateAndAddElementToNeoCanvas<AnalogNumericFX>();
            analogNumeric.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
            analogNumeric.Foreground = new SolidColorBrush(Colors.Black);
            analogNumeric.BorderBrush = new SolidColorBrush(Colors.Black);

            AnalogNumericAdapter analogNumericAdapter = new AnalogNumericAdapter();
            analogNumericAdapter.AdaptedObject = analogNumeric;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), analogNumericAdapter.Outline.StartColor.ToArgb());

            analogNumericAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), analogNumeric.BorderBrush);
        }
#endregion

#region Label
        [Test]
        public void TestLabelFill()
        {
            Label label = CreateAndAddElementToNeoCanvas<Label>();
            label.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);

            ColorElementAdapter labelAdapter = new ColorElementAdapter();
            labelAdapter.AdaptedObject = label;

            Assert.AreEqual(System.Drawing.Color.Red.ToArgb(), labelAdapter.Fill.StartColor.ToArgb());
            Assert.AreEqual(System.Drawing.Color.Blue.ToArgb(), labelAdapter.Fill.EndColor.ToArgb());

            labelAdapter.Fill = GetLinearGradientBrushCF(System.Drawing.Color.Yellow, System.Drawing.Color.Blue);

            AssertBrushesAreEqual(GetLinearGradientBrush(Colors.Yellow, Colors.Blue), label.Background);
        }

        //[Test]
        //public void TestLabelForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.Label label = new Neo.ApplicationFramework.Controls.Label();
        //    label.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
        //    label.Foreground = new SolidColorBrush(Colors.Black);

        //    ColorControlAdapter labelAdapter = new ColorControlAdapter();
        //    labelAdapter.AdaptedObject = label;

        //    Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), labelAdapter.ForegroundColor.ToArgb());

        //    labelAdapter.ForegroundColor = System.Drawing.Color.Yellow;

        //    AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), label.Foreground);
        //}

        [Test]
        public void TestLabelBorderColor()
        {
            Label label = CreateAndAddElementToNeoCanvas<Label>();
            label.Background = GetLinearGradientBrush(Colors.Red, Colors.Blue);
            label.Foreground = new SolidColorBrush(Colors.Black);
            label.BorderBrush = new SolidColorBrush(Colors.Black);

            ColorElementAdapter labelAdapter = new ColorElementAdapter();
            labelAdapter.AdaptedObject = label;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), labelAdapter.Outline.StartColor.ToArgb());

            labelAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), label.BorderBrush);
        }
#endregion

#region RectangleColorProperties
        [Test]
        public void TestRectangleFill()
        {
            Rectangle rectangle = CreateAndAddElementToNeoCanvas<Rectangle>();
            rectangle.Fill = GetLinearGradientBrush(Colors.Red, Colors.Blue);

            ColorElementAdapter elementAdapter = new ColorElementAdapter();
            elementAdapter.AdaptedObject = rectangle;

            Assert.AreEqual(System.Drawing.Color.Red.ToArgb(), elementAdapter.Fill.StartColor.ToArgb());
            Assert.AreEqual(System.Drawing.Color.Blue.ToArgb(), elementAdapter.Fill.EndColor.ToArgb());

            elementAdapter.Fill = GetLinearGradientBrushCF(System.Drawing.Color.Yellow, System.Drawing.Color.Blue);

            AssertBrushesAreEqual(GetLinearGradientBrush(Colors.Yellow, Colors.Blue), rectangle.Fill);
        }


        [Test]
        public void TestRectangleBorderColor()
        {
            Rectangle rectangle = CreateAndAddElementToNeoCanvas<Rectangle>();
            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            ColorElementAdapter elementAdapter = new ColorElementAdapter();
            elementAdapter.AdaptedObject = rectangle;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), elementAdapter.Outline.StartColor.ToArgb());

            elementAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), rectangle.Stroke);
        }

#endregion

#region LineColorProperties
        [Test]
        public void TestLineStrokeColor()
        {
            System.Windows.Shapes.Line line = CreateAndAddElementToNeoCanvas<System.Windows.Shapes.Line>();
            line.Stroke = new SolidColorBrush(Colors.Black);

            LineAdapter lineAdapter = new LineAdapter();
            lineAdapter.AdaptedObject = line;

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), lineAdapter.Outline.StartColor.ToArgb());

            lineAdapter.Outline = new BrushCF(System.Drawing.Color.Yellow);

            AssertBrushesAreEqual(new SolidColorBrush(Colors.Yellow), line.Stroke);
        }

#endregion

        private T CreateAndAddElementToNeoCanvas<T>() where T : UIElement, new()
        {
            T element = new T();
            m_ElementCanvas.Children.Add(element);
            return element;
        }

        public bool IsBrowsable(object obj, string property)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
            if (propertyInfo == null)
                Assert.Fail(string.Format("Property {0} was not found in {1}", property, obj.GetType().Name));

            object[] editorBrowsableAttributes = propertyInfo.GetCustomAttributes(typeof(EditorBrowsableAttribute), true);
            if (editorBrowsableAttributes.Length == 0)
                return false;

            foreach (EditorBrowsableAttribute editorBrowsableAttribute in editorBrowsableAttributes)
            {
                if (editorBrowsableAttribute.State == EditorBrowsableState.Never ||
                    editorBrowsableAttribute.State == EditorBrowsableState.Advanced)
                    return false;
            }
            return true;

        }

        public void AssertBrushesAreEqual(Brush expected, Brush actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                Assert.Fail("Expected brush of type:" + expected.GetType() + "\r\nbut was:" + actual.GetType());
            }

            if (expected is LinearGradientBrush)
            {
                LinearGradientBrush expectedLinearBrush = expected as LinearGradientBrush;
                LinearGradientBrush actualLinearBrush = actual as LinearGradientBrush;

                Assert.AreEqual(expectedLinearBrush.GradientStops.Count, actualLinearBrush.GradientStops.Count);
                for (int i = 0; i < expectedLinearBrush.GradientStops.Count; i++)
                {
                    Assert.AreEqual(expectedLinearBrush.GradientStops[i].Color, actualLinearBrush.GradientStops[i].Color);
                    Assert.AreEqual(expectedLinearBrush.GradientStops[i].Offset, actualLinearBrush.GradientStops[i].Offset);
                }

                Assert.AreEqual(expectedLinearBrush.StartPoint, actualLinearBrush.StartPoint);
                Assert.AreEqual(expectedLinearBrush.EndPoint, actualLinearBrush.EndPoint);

                return;
            }

            if (expected is SolidColorBrush)
            {
                SolidColorBrush expectedSolidBrush = expected as SolidColorBrush;
                SolidColorBrush actualSolidBrush = actual as SolidColorBrush;

                Assert.AreEqual(expectedSolidBrush.Color, actualSolidBrush.Color);

                return;
            }

            PropertyInfo[] expectedProperties = expected.GetType().GetProperties();
            foreach (PropertyInfo property in expectedProperties)
            {

                Assert.AreEqual(property.GetValue(expected, null), property.GetValue(actual, null));
            }
        }

        private void AssertGradientFillDirection(FillDirection fillDirection, Brush brush)
        {
            if (fillDirection == FillDirection.None)
            {
                Assert.IsTrue(brush is SolidColorBrush, "Brush is not solid");
                return;
            }

            LinearGradientBrush linearGradientBrush = brush as LinearGradientBrush;
            if (linearGradientBrush == null)
                Assert.Fail("Brush is not a gradient");


            switch (fillDirection)
            {
                case FillDirection.VerticalTopToBottom:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X == linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y < linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.VerticalBottomToTop:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X == linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y > linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.VerticalWithMiddleStop:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 3);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X == linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y < linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.HorizontalLeftToRight:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X < linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y == linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.HorizontalRightToLeft:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X > linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y == linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.HorizontalWithMiddleStop:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 3);
                    Assert.IsTrue(linearGradientBrush.StartPoint.X < linearGradientBrush.EndPoint.X);
                    Assert.IsTrue(linearGradientBrush.StartPoint.Y == linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.DiagonalTopLeftToBottomRight:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.AreEqual(0, linearGradientBrush.StartPoint.X);
                    Assert.AreEqual(0, linearGradientBrush.StartPoint.Y);
                    Assert.AreEqual(1, linearGradientBrush.EndPoint.X);
                    Assert.AreEqual(1, linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.DiagonalTopRightToBottomLeft:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.AreEqual(1, linearGradientBrush.StartPoint.X);
                    Assert.AreEqual(0, linearGradientBrush.StartPoint.Y);
                    Assert.AreEqual(0, linearGradientBrush.EndPoint.X);
                    Assert.AreEqual(1, linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.DiagonalBottomLeftToTopRight:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.AreEqual(0, linearGradientBrush.StartPoint.X);
                    Assert.AreEqual(1, linearGradientBrush.StartPoint.Y);
                    Assert.AreEqual(1, linearGradientBrush.EndPoint.X);
                    Assert.AreEqual(0, linearGradientBrush.EndPoint.Y);
                    break;
                case FillDirection.DiagonalBottomRightToTopLeft:
                    Assert.IsTrue(linearGradientBrush.GradientStops.Count == 2);
                    Assert.AreEqual(1, linearGradientBrush.StartPoint.X);
                    Assert.AreEqual(1, linearGradientBrush.StartPoint.Y);
                    Assert.AreEqual(0, linearGradientBrush.EndPoint.X);
                    Assert.AreEqual(0, linearGradientBrush.EndPoint.Y);
                    break;
                default:
                    Assert.Fail("Wrong type of gradient");
                    break;
            }
        }
    }
}
#endif
