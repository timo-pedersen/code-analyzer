using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ComponentInfo = MockRepository.GenerateMock<IComponentInfo>();
            var tcs = new TaskCompletionSource<ImageSource>();
            tcs.SetResult(null);
            m_ComponentInfo.Expect(i => i.LoadThumbnailAsync()).Return(tcs.Task);

            m_CommandHandler = MockRepository.GenerateMock<IComponentInfoCommandHandler>();

            m_UnderTest = new ComponentInfoViewModel(m_ComponentInfo, m_CommandHandler);
        }

        [Test]
        public void Should_fire_PropertyChanged()
        {
            var newComponent = MockRepository.GenerateMock<IComponentInfo>();
            newComponent.Expect(i => i.DisplayName).Return("NewName");
            m_CommandHandler.Expect(i => i.RenameComponent(m_ComponentInfo, "NewName")).Return(newComponent);

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
            m_ComponentInfo.Expect(i => i.DisplayName).Return("TestName");
            m_CommandHandler.Expect(i => i.RenameComponent(m_ComponentInfo, "TestName")).Return(m_ComponentInfo);

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
            var dataObject = MockRepository.GenerateMock<IDataObject>();
            m_ComponentInfo.Expect(i => i.CreateDataObject()).Return(dataObject);
            m_ComponentInfo.Replay();

            // Act
            var actual = m_UnderTest.CreateDataObject();

            // Assert
            m_ComponentInfo.VerifyAllExpectations();
            Assert.That(actual, Is.SameAs(dataObject));
        }

        [Test]
        public void Commands_should_be_relayed_from_CommandHandler()
        {
            // Arrange
            var command = MockRepository.GenerateMock<ICommand>();
            m_CommandHandler.Expect(i => i.CutComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.CopyComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.PasteComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.DeleteComponentCommand).Return(command);
            m_CommandHandler.Expect(i => i.RenameComponentCommand).Return(command);
            m_CommandHandler.Replay();

            // Act & Assert
            Assert.That(m_UnderTest.CutComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.CopyComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.PasteComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.DeleteComponentCommand, Is.SameAs(command));
            Assert.That(m_UnderTest.RenameComponentCommand, Is.SameAs(command));
            m_CommandHandler.VerifyAllExpectations();
        }
    }
}
