using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.Graphics.Logic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Script
{
    [TestFixture]
    public class ComponentAdaptersTest
    {
        private readonly IAdapterService m_AdapterService;
        private IToolManager m_ToolManager;

        public ComponentAdaptersTest()
        {
            m_AdapterService = new ComponentAdapterService();
        }

        [SetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;

            m_ToolManager = TestHelper.CreateAndAddServiceMock<IToolManager>();
            m_ToolManager.Stub(x => x.Runtime).Return(true);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        #region Text Support

        [Test]
        public void TestThatButtonCFIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Controls.Button buttonCF = new Neo.ApplicationFramework.Controls.Controls.Button();
            buttonCF.Text = "Button1";

            IButtonAdapter buttonCFAdapter = m_AdapterService.CreateAdapter<ButtonCFAdapter>(buttonCF);
            Assert.IsNotNull(buttonCFAdapter, "Could not create an adapter for CF button.");

            Assert.AreEqual(buttonCF.Text, buttonCFAdapter.Text, "Text not equal for Text.");
        }

        [Test]
        public void TestThatLabelIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Controls.Label label = new Neo.ApplicationFramework.Controls.Controls.Label();
            label.Text = "Label1";

            ITextControlAdapter textControlAdapter = m_AdapterService.CreateAdapter<TextControlCFAdapter>(label);
            Assert.IsNotNull(textControlAdapter, "Could not create an adapter for CF label.");

            Assert.AreEqual(label.Text, textControlAdapter.Text, "Text not equal for Text.");
        }

        [Test]
        public void TestThatTextBoxCFIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Controls.TextBox textbox = new Neo.ApplicationFramework.Controls.Controls.TextBox();
            textbox.Text = "Text";

            ITextBoxAdapter textBoxCFAdapter = m_AdapterService.CreateAdapter<TextBoxCFAdapter>(textbox);
            Assert.IsNotNull(textBoxCFAdapter, "Could not create an adapter for CF textbox.");

            Assert.AreEqual(textbox.Text, textBoxCFAdapter.Text, "Text not equal for TextBox.");
        }

        #endregion

        #region AnalogNumeric

        [Test]
        public void TestThatAnalogNumericCFIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Controls.AnalogNumeric analogNumeric = new Neo.ApplicationFramework.Controls.Controls.AnalogNumeric();
            analogNumeric.Value = "123";

            IAnalogNumericAdapter analogNumericCFAdapter = m_AdapterService.CreateAdapter<AnalogNumericCFAdapter>(analogNumeric);
            Assert.IsNotNull(analogNumericCFAdapter, "Could not create an adapter for AnalogNumeric.");

            Assert.AreEqual(analogNumeric.Value.ToString(), analogNumericCFAdapter.Value.ToString(), "Value not equal for AnalogNumeric.");
        }

        #endregion

        #region Line

        [Test]
        public void TestThatLineCFIsSupportedByAnAdapter()
        {
            Neo.ApplicationFramework.Controls.Controls.Line line = new Neo.ApplicationFramework.Controls.Controls.Line();
            line.X1 = 10;
            line.Y1 = 11;
            line.X2 = 12;
            line.Y2 = 13;

            LineCFAdapter lineCFAdapter = m_AdapterService.CreateAdapter<LineCFAdapter>(line);
            Assert.IsNotNull(lineCFAdapter, "Could not create an adapter for CF line.");

            Assert.AreEqual(line.X1, lineCFAdapter.X1, "X1 not equal for line cf");
            Assert.AreEqual(line.Y1, lineCFAdapter.Y1, "Y1 not equal for line cf");
            Assert.AreEqual(line.X2, lineCFAdapter.X2, "X2 not equal for line cf");
            Assert.AreEqual(line.Y2, lineCFAdapter.Y2, "Y2 not equal for line cf");
        }

        #endregion

        #region Size and position Support

        [Test]
        public void TestThatARectangleSupportsSizeAndPosition()
        {
            Neo.ApplicationFramework.Controls.Controls.RectangleCF rectangle = new Neo.ApplicationFramework.Controls.Controls.RectangleCF();
            rectangle.Height = 10;
            rectangle.Width = 20;
            rectangle.Left = 30;
            rectangle.Top = 40;

            ControlCFAdapter controlsAdapter = m_AdapterService.CreateAdapter<ControlCFAdapter>(rectangle);
            Assert.IsNotNull(controlsAdapter, "Could not create an adapter for rectangle");

            Assert.AreEqual(rectangle.Height, controlsAdapter.Height, "The heights are not equal.");
            Assert.AreEqual(rectangle.Width, controlsAdapter.Width, "The heights are not equal.");
            Assert.AreEqual(rectangle.Left, controlsAdapter.Left, "The heights are not equal.");
            Assert.AreEqual(rectangle.Top, controlsAdapter.Top, "The heights are not equal.");
        }

        [Test]
        public void TestThatAButtonSupportsSizeAndPosition()
        {
            Neo.ApplicationFramework.Controls.Controls.Button button = new Neo.ApplicationFramework.Controls.Controls.Button();
            button.Height = 10;
            button.Width = 20;
            button.Left = 30;
            button.Top = 40;

            ButtonCFAdapter buttonAdapter = m_AdapterService.CreateAdapter<ButtonCFAdapter>(button);
            Assert.IsNotNull(buttonAdapter, "Could not create an adapter for button");

            Assert.AreEqual(button.Height, buttonAdapter.Height, "The heights are not equal.");
            Assert.AreEqual(button.Width, buttonAdapter.Width, "The heights are not equal.");
            Assert.AreEqual(button.Left, buttonAdapter.Left, "The heights are not equal.");
            Assert.AreEqual(button.Top, buttonAdapter.Top, "The heights are not equal.");
        }

        #endregion

        #region ColorPropertiesBrowsableTest
        [Test]
        public void TestButtonColorPropertiesAreBrowsable()
        {
            ButtonCFAdapter buttonAdapter = new ButtonCFAdapter();
            buttonAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.Controls.Button();

            Assert.IsTrue(IsBrowsable(buttonAdapter, ElementsAdaptersTest.FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(buttonAdapter, ElementsAdaptersTest.OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(buttonAdapter, ElementsAdaptersTest.OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestTextBoxColorPropertiesAreBrowsable()
        {
            TextBoxCFAdapter textBoxAdapter = new TextBoxCFAdapter();
            textBoxAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.Controls.TextBox();

            Assert.IsTrue(IsBrowsable(textBoxAdapter, ElementsAdaptersTest.FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(textBoxAdapter, ElementsAdaptersTest.OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(textBoxAdapter, ElementsAdaptersTest.OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestAnalogNumericColorPropertiesAreBrowsable()
        {
            AnalogNumericCFAdapter analogNumericAdapter = new AnalogNumericCFAdapter();
            analogNumericAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.Controls.AnalogNumeric();

            Assert.IsTrue(IsBrowsable(analogNumericAdapter, ElementsAdaptersTest.FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(analogNumericAdapter, ElementsAdaptersTest.OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(analogNumericAdapter, ElementsAdaptersTest.OutlineThicknessPropertyName), "Border thickness is not browsable");
        }

        [Test]
        public void TestRectangleColorPropertiesAreBrowsable()
        {
            ShapeCFAdapter shapeAdapter = new ShapeCFAdapter();
            shapeAdapter.AdaptedObject = new Neo.ApplicationFramework.Controls.Controls.RectangleCF();

            Assert.IsTrue(IsBrowsable(shapeAdapter, ElementsAdaptersTest.FillPropertyName), "Primary Background color is not browsable");
            Assert.IsTrue(IsBrowsable(shapeAdapter, ElementsAdaptersTest.OutlinePropertyName), "Border color is not browsable");
            Assert.IsTrue(IsBrowsable(shapeAdapter, ElementsAdaptersTest.OutlineThicknessPropertyName), "Border thickness is not browsable");
        }
        #endregion

        #region ButtonColorProperites
        [Test]
        public void TestButtonFill()
        {
            Neo.ApplicationFramework.Controls.Controls.Button button = new Neo.ApplicationFramework.Controls.Controls.Button();
            button.Background = new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom);

            ButtonCFAdapter buttonAdapter = new ButtonCFAdapter();
            buttonAdapter.AdaptedObject = button;

            AssertBrushesAreEqual(new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom), buttonAdapter.Fill);

            buttonAdapter.Fill = new BrushCF(Color.White, Color.Tomato, FillDirection.HorizontalLeftToRight);

            AssertBrushesAreEqual(new BrushCF(Color.White, Color.Tomato, FillDirection.HorizontalLeftToRight), buttonAdapter.Fill);
        }

        //[Test]
        //public void TestButtonForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.Controls.Button button = new Neo.ApplicationFramework.Controls.Controls.Button();
        //    button.Foreground = Color.Black;

        //    ButtonCFAdapter buttonAdapter = new ButtonCFAdapter();
        //    buttonAdapter.AdaptedObject = button;

        //    Assert.AreEqual(Color.Black.ToArgb(), buttonAdapter.ForegroundColor.ToArgb());

        //    buttonAdapter.ForegroundColor = Color.Yellow;

        //    Assert.AreEqual(Color.Yellow.ToArgb(), button.Foreground.ToArgb());
        //}

        [Test]
        public void TestButtonBorderColor()
        {
            Neo.ApplicationFramework.Controls.Controls.Button button = new Neo.ApplicationFramework.Controls.Controls.Button();
            button.BorderBrush = new BrushCF(Color.Black);

            ButtonCFAdapter buttonAdapter = new ButtonCFAdapter();
            buttonAdapter.AdaptedObject = button;

            Assert.AreEqual(Color.Black.ToArgb(), buttonAdapter.Outline.StartColor.ToArgb());

            buttonAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), button.BorderBrush.StartColor.ToArgb());
        }
        #endregion

        #region TextBoxColorProperties
        [Test]
        public void TestTextBoxFill()
        {
            Neo.ApplicationFramework.Controls.Controls.TextBox textBox = new Neo.ApplicationFramework.Controls.Controls.TextBox();
            textBox.Background = new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom);

            TextBoxCFAdapter textBoxAdapter = new TextBoxCFAdapter();
            textBoxAdapter.AdaptedObject = textBox;

            AssertBrushesAreEqual(new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom), textBox.Background);

            textBoxAdapter.Fill = new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight);

            AssertBrushesAreEqual(new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight), textBox.Background);
        }

        //[Test]
        //public void TestTextBoxForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.Controls.TextBox textBox = new Neo.ApplicationFramework.Controls.Controls.TextBox();
        //    textBox.Foreground = Color.Black;

        //    TextBoxCFAdapter textBoxAdapter = new TextBoxCFAdapter();
        //    textBoxAdapter.AdaptedObject = textBox;

        //    Assert.AreEqual(Color.Black.ToArgb(), textBoxAdapter.ForegroundColor.ToArgb());

        //    textBoxAdapter.ForegroundColor = Color.Yellow;

        //    Assert.AreEqual(Color.Yellow.ToArgb(), textBox.Foreground.ToArgb());
        //}

        [Test]
        public void TestTextBoxBorderColor()
        {
            Neo.ApplicationFramework.Controls.Controls.TextBox textBox = new Neo.ApplicationFramework.Controls.Controls.TextBox();
            textBox.BorderBrush = new BrushCF(Color.Black);

            TextBoxCFAdapter textBoxAdapter = new TextBoxCFAdapter();
            textBoxAdapter.AdaptedObject = textBox;

            Assert.AreEqual(Color.Black.ToArgb(), textBoxAdapter.Outline.StartColor.ToArgb());

            textBoxAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), textBox.BorderBrush.StartColor.ToArgb());
        }
        #endregion

        #region AnalogNumericColorProperties
        [Test]
        public void TestAnalogNumericFill()
        {
            Neo.ApplicationFramework.Controls.Controls.AnalogNumeric textBox = new Neo.ApplicationFramework.Controls.Controls.AnalogNumeric();
            textBox.Background = new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom);

            AnalogNumericCFAdapter analogNumericAdapter = new AnalogNumericCFAdapter();
            analogNumericAdapter.AdaptedObject = textBox;

            AssertBrushesAreEqual(new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom), analogNumericAdapter.Fill);

            analogNumericAdapter.Fill = new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight);

            AssertBrushesAreEqual(new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight), textBox.Background);
        }

        [Test]
        public void TestAnalogNumericBorderColor()
        {
            Neo.ApplicationFramework.Controls.Controls.AnalogNumeric textBox = new Neo.ApplicationFramework.Controls.Controls.AnalogNumeric();
            textBox.BorderBrush = new BrushCF(Color.Black);

            AnalogNumericCFAdapter analogNumericAdapter = new AnalogNumericCFAdapter();
            analogNumericAdapter.AdaptedObject = textBox;

            Assert.AreEqual(Color.Black.ToArgb(), analogNumericAdapter.Outline.StartColor.ToArgb());

            analogNumericAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), textBox.BorderBrush.StartColor.ToArgb());
        }
        #endregion

        #region Label
        [Test]
        public void TestLabelFill()
        {
            Neo.ApplicationFramework.Controls.Controls.Label label = new Neo.ApplicationFramework.Controls.Controls.Label();
            label.Background = new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom);

            TextControlCFAdapter labelAdapter = new TextControlCFAdapter();
            labelAdapter.AdaptedObject = label;

            AssertBrushesAreEqual(new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom), labelAdapter.Fill);

            labelAdapter.Fill = new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight);

            AssertBrushesAreEqual(new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight), label.Background);
        }

        //[Test]
        //public void TestLabelForegroundColor()
        //{
        //    Neo.ApplicationFramework.Controls.Controls.Label label = new Neo.ApplicationFramework.Controls.Controls.Label();
        //    label.Foreground = Color.Black;

        //    TextControlCFAdapter labelAdapter = new TextControlCFAdapter();
        //    labelAdapter.AdaptedObject = label;

        //    Assert.AreEqual(Color.Black.ToArgb(), labelAdapter.ForegroundColor.ToArgb());

        //    labelAdapter.ForegroundColor = Color.Yellow;

        //    Assert.AreEqual(Color.Yellow.ToArgb(), label.Foreground.ToArgb());
        //}

        [Test]
        public void TestLabelBorderColor()
        {
            Neo.ApplicationFramework.Controls.Controls.Label label = new Neo.ApplicationFramework.Controls.Controls.Label();
            label.BorderBrush = new BrushCF(Color.Black);

            TextControlCFAdapter labelAdapter = new TextControlCFAdapter();
            labelAdapter.AdaptedObject = label;

            Assert.AreEqual(Color.Black.ToArgb(), labelAdapter.Outline.StartColor.ToArgb());

            labelAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), label.BorderBrush.StartColor.ToArgb());
        }
        #endregion

        #region RectangleColorProperties
        [Test]
        public void TestRectangleFill()
        {
            Neo.ApplicationFramework.Controls.Controls.RectangleCF rectangle = new Neo.ApplicationFramework.Controls.Controls.RectangleCF();
            rectangle.Fill = new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom);

            ShapeCFAdapter shapeAdapter = new ShapeCFAdapter();
            shapeAdapter.AdaptedObject = rectangle;

            AssertBrushesAreEqual(new BrushCF(Color.Coral, Color.White, FillDirection.VerticalTopToBottom), shapeAdapter.Fill);

            shapeAdapter.Fill = new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight);

            AssertBrushesAreEqual(new BrushCF(Color.Yellow, Color.Tomato, FillDirection.HorizontalLeftToRight), rectangle.Fill);
        }

        [Test]
        public void TestRectangleBorderColor()
        {
            Neo.ApplicationFramework.Controls.Controls.RectangleCF rectangle = new Neo.ApplicationFramework.Controls.Controls.RectangleCF();
            rectangle.Stroke = new BrushCF(Color.Black);

            ShapeCFAdapter shapeAdapter = new ShapeCFAdapter();
            shapeAdapter.AdaptedObject = rectangle;

            Assert.AreEqual(Color.Black.ToArgb(), shapeAdapter.Outline.StartColor.ToArgb());

            shapeAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), rectangle.Stroke.StartColor.ToArgb());
        }

        #endregion

        #region LineColorProperties
        [Test]
        public void TestLineStrokeColor()
        {
            Neo.ApplicationFramework.Controls.Controls.Line line = new Neo.ApplicationFramework.Controls.Controls.Line();
            line.Stroke = new BrushCF(Color.Black, Color.Empty, FillDirection.None);

            LineCFAdapter lineAdapter = new LineCFAdapter();
            lineAdapter.AdaptedObject = line;

            Assert.AreEqual(Color.Black.ToArgb(), lineAdapter.Outline.StartColor.ToArgb());

            lineAdapter.Outline = new BrushCF(Color.Yellow);

            Assert.AreEqual(Color.Yellow.ToArgb(), line.Stroke.StartColor.ToArgb());
        }

        #endregion

        #region Adapted property names

        [Test]
        public void AdaptedPropertyNamesForShapes()
        {
            ShapeCFAdapter shapeCFAdapter = new ShapeCFAdapter();
            Assert.AreEqual("Fill", shapeCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.FillPropertyName));
            Assert.AreEqual("Stroke", shapeCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.OutlinePropertyName));
            Assert.AreEqual("StrokeThickness", shapeCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.OutlineThicknessPropertyName));
        }

        [Test]
        public void AdaptedPropertyNamesForTextControls()
        {
            TextControlCFAdapter textControlCFAdapter = new TextControlCFAdapter();
            Assert.AreEqual("Background", textControlCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.FillPropertyName));
            Assert.AreEqual("BorderBrush", textControlCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.OutlinePropertyName));
            Assert.AreEqual("BorderThickness", textControlCFAdapter.GetAdaptedProperty(SpecialAdaptionProperties.OutlineThicknessPropertyName));
        }

        #endregion


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

        private void AssertBrushesAreEqual(BrushCF expected, BrushCF actual)
        {
            Assert.AreEqual(expected.EndColor.ToArgb(), actual.EndColor.ToArgb(), "End Colors are not equal");
            Assert.AreEqual(expected.StartColor.ToArgb(), actual.StartColor.ToArgb(), "Start Colors are not equal");
            Assert.AreEqual(expected.FillDirection, actual.FillDirection, "Fill directions are not equal");
        }
    }
}
