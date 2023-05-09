using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Shapes;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ScreenObjectDecorator
{
    [TestFixture]
    public class ScreenObjectDecoratorServiceTest
    {
        private IScreenObjectDecoratorService m_ScreenObjectDecoratorService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceMock<ITargetService>();
            m_ScreenObjectDecoratorService = new ScreenObjectDecoratorService();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void SetPropertiesOnButtonTest()
        {
            // Arrange
            var buttonElement = new Button();

            // Act
            m_ScreenObjectDecoratorService.SetPropertiesOnFrameworkElement(buttonElement);

            // Assert
            Assert.AreEqual(buttonElement.GetType().Name, buttonElement.Text);
        }

        [Test]
        public void SetPropertiesOnContentControlWithNoContentTest()
        {
            // Arrange
            var element = new Needle();

            // Act
            m_ScreenObjectDecoratorService.SetPropertiesOnFrameworkElement(element);

            // Assert
            Assert.AreEqual(element.GetType().Name, element.Content);
        }

        [Test]
        public void SetPropertiesOnLabelTest()
        {
            // Arrange
            var element = new Label();

            // Act
            m_ScreenObjectDecoratorService.SetPropertiesOnFrameworkElement(element);

            // Assert
            Assert.AreEqual(TextsIde.DefaultTextInLabel, element.Text);
        }
    }
}
