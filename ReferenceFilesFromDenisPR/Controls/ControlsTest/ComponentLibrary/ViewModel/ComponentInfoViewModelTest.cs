#if!VNEXT_TARGET
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ComponentLibrary.ViewModel
{
    [TestFixture]
    public class ComponentInfoViewModelTest
    {
        private ComponentInfoViewModel m_UnderTest;
        private IComponentInfo m_ComponentInfo;
        private IComponentInfoCommandHandler m_CommandHandler;

        [SetUp]
        public void SetUp()
        {
            m_ComponentInfo = Substitute.For<IComponentInfo>();
            var tcs = new TaskCompletionSource<ImageSource>();
            tcs.SetResult(null);
            m_ComponentInfo.LoadThumbnailAsync().Returns(tcs.Task);

            m_CommandHandler = Substitute.For<IComponentInfoCommandHandler>();

            m_UnderTest = new ComponentInfoViewModel(m_ComponentInfo, m_CommandHandler);
        }

        [Test]
        public void Should_fire_PropertyChanged()
        {
            var newComponent = Substitute.For<IComponentInfo>();
            newComponent.DisplayName.Returns("NewName");
            m_CommandHandler.RenameComponent(m_ComponentInfo, "NewName").Returns(newComponent);

            Assert.That(m_UnderTest.NotifiesOn(x => x.DisplayName).When(x => x.DisplayName = "NewName"));
            Assert.That(m_UnderTest.NotifiesOn(x => x.Size).When(x => x.Size = 200));
            Assert.That(m_UnderTest.NotifiesOn(x => x.Thumbnail).When(x => x.Thumbnail = new BitmapImage()));
            Assert.That(m_UnderTest.NotifiesOn(x => x.IsSelected).When(x => x.IsSelected = true));
            Assert.That(m_UnderTest.NotifiesOn(x => x.IsEditMode).When(x => x.IsEditMode = true));
        }

        [Test]
        public void Setting_DisplayName_should_clear_IsEditMode()
        {
            // Arrange
            m_UnderTest.IsEditMode = true;
            m_ComponentInfo.DisplayName.Returns("TestName");
            m_CommandHandler.RenameComponent(m_ComponentInfo, "TestName").Returns(m_ComponentInfo);

            // Act
            m_UnderTest.DisplayName = "TestName";

            // Assert
            Assert.That(m_UnderTest.IsEditMode, Is.False);
            Assert.That(m_UnderTest.DisplayName, Is.EqualTo("TestName"));
        }

        [Test]
        public void ExitEditModeCommand_should_clear_IsEditMode()
        {
            // Arrange
            m_UnderTest.IsEditMode = true;

            // Act
            m_UnderTest.ExitEditModeCommand.Execute(null);

            // Assert
            Assert.That(m_UnderTest.IsEditMode, Is.False);
        }

        [Test]
        public void Unselecting_should_clear_IsEditMode()
        {
            // Arrange
            m_UnderTest.IsSelected = true;
            m_UnderTest.IsEditMode = true;

            // Act
            m_UnderTest.IsSelected = false;

            // Assert
            Assert.That(m_UnderTest.IsEditMode, Is.False);
        }

        [Test]
        public void CreateDataObject_should_call_model()
        {
            // Arrange
            var dataObject = Substitute.For<IDataObject>();
            m_ComponentInfo.CreateDataObject().Returns(dataObject);

            // Act
            var actual = m_UnderTest.CreateDataObject();

            // Assert
            m_ComponentInfo.Received().CreateDataObject();
            Assert.That(actual, Is.SameAs(dataObject));
        }

        [Test]
        public void Commands_should_be_relayed_from_CommandHandler()
        {
            // Arrange
            var command = Substitute.For<ICommand>();
            m_CommandHandler.CutComponentCommand.Returns(command);
            m_CommandHandler.CopyComponentCommand.Returns(command);
            m_CommandHandler.PasteComponentCommand.Returns(command);
            m_CommandHandler.DeleteComponentCommand.Returns(command);
            m_CommandHandler.RenameComponentCommand.Returns(command);

            // Act & Assert
            Assert.That(m_UnderTest.CutComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.CopyComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.PasteComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.DeleteComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.RenameComponentCommand, Is.SameAs(command));
        }
    }
}
#endif
