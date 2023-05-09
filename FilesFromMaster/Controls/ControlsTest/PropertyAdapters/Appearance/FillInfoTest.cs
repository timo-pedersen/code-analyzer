using System.Windows.Media;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.PropertyGrid;
using Neo.ApplicationFramework.TestUtilities.Brush;
using NUnit.Framework;
using WPFShape = System.Windows.Shapes.Shape;

namespace Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance
{
    [TestFixture]
    public class FillInfoTest : AppearanceAdapterTestBase
    {
        private Brush m_BlueBrush;
        private Rectangle m_Rectangle;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            NeoDesignerProperties.IsInDesignMode = true;

            m_BlueBrush = new SolidColorBrush(Colors.Blue);
            m_Rectangle = new Rectangle();

            TestHelper.AddService<IObjectPropertyService>(new ObjectPropertyService());
        }

        public override void TearDown()
        {
            base.TearDown();
            TestHelper.ClearServices();
            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void Clone()
        {
            m_Rectangle.Fill = m_BlueBrush;

            IFillInfo fillInfo = new FillInfo(m_Rectangle, WPFShape.FillProperty);
            IFillInfo targetInfo = (IFillInfo)fillInfo.Clone(true);

            Assert.IsTrue(fillInfo.DependencyObject == targetInfo.DependencyObject);
            BrushValidator.AssertBrushesAreEqual(fillInfo.Brush, targetInfo.Brush);
            Assert.IsTrue(targetInfo.IsBrushChanged, "Brush was not changed, but was expected to be.");
            Assert.AreEqual(typeof(FillInfo), targetInfo.GetType());
        }

        [Test]
        public void ApplyToNewOwnerWhenBrushIsChanged()
        {
            Rectangle targetRectangle = new Rectangle();

            IFillInfo fillInfo = new FillInfo(m_Rectangle, WPFShape.FillProperty);
            fillInfo.Brush = m_BlueBrush;

            IFillInfo targetInfo = (IFillInfo)fillInfo.Clone(targetRectangle, fillInfo.DependencyProperty);
            targetInfo.Apply();

            Assert.IsFalse(fillInfo.DependencyObject == targetInfo.DependencyObject);
            BrushValidator.AssertBrushesAreEqual(m_BlueBrush, targetRectangle.Fill);
            Assert.AreEqual(typeof(FillInfo), targetInfo.GetType());
        }

        [Test]
        public void ApplyToNewOwnerWhenBrushIsNotChanged()
        {
            Rectangle targetRectangle = new Rectangle();
            targetRectangle.Fill = Brushes.Red;

            m_Rectangle.Fill = m_BlueBrush;
            IFillInfo fillInfo = new FillInfo(m_Rectangle, WPFShape.FillProperty);

            IFillInfo targetInfo = (IFillInfo)fillInfo.Clone(targetRectangle, fillInfo.DependencyProperty);
            targetInfo.Apply();

            Assert.IsFalse(fillInfo.DependencyObject == targetInfo.DependencyObject);
            BrushValidator.AssertBrushesAreEqual(Brushes.Red, targetRectangle.Fill);
            Assert.AreEqual(typeof(FillInfo), targetInfo.GetType());
        }

        [Test]
        public void ApplyFillInfoToGroupWithChildren()
        {
            m_Rectangle.Fill = m_BlueBrush;

            Button button = new Button();
            button.Background = Brushes.Green;

            Group group = new Group();
            group.Items.Add(m_Rectangle);
            group.Items.Add(button);

            IAppearanceAdapter appearanceAdapter = m_AppearanceAdapterService.GetAppearanceAdapter(group);
            IFillInfo fillInfo = new FillInfo(group, Group.BackgroundProperty);
            fillInfo.Brush = Brushes.Red;
            appearanceAdapter.ApplyInfo<IFillInfo>(Group.BackgroundProperty.Name, fillInfo);

            BrushValidator.AssertBrushesAreEqual(Brushes.Red, m_Rectangle.Fill);
            BrushValidator.AssertBrushesAreEqual(Brushes.Red, button.Background);
        }
    }
}