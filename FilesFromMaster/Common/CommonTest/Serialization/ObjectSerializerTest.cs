using System.ComponentModel.Design.Serialization;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using INameCreationService = Neo.ApplicationFramework.Interfaces.INameCreationService;
using WinForms = System.Windows.Forms;

namespace Neo.ApplicationFramework.Common.Serialization
{
    [TestFixture]
    public class ObjectSerializerTest
    {
        private IDesignerLoaderHost m_DesignerHostMock;
        private INameCreationService m_NameCreationServiceMock;
        private WinForms.Form m_Form;
        private WinForms.Button m_Button;
        private WinForms.Button m_Button2;
        private readonly string m_SerializedButton = "<NeoItem d1p1:Serializer=\"Neo.ApplicationFramework.Common.Serialization.ObjectSerializer\" d1p1:SchemaVersion=\"1.0\" xmlns:d1p1=\"urn:Neo.ApplicationFramework.Serializer\"><Object d1p1:type=\"System.Windows.Forms.Button, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" d1p1:Site.Name=\"TestButton\" UseCompatibleTextRendering=\"True\" Location=\"0, 0\" Name=\"TestButton\" TabIndex=\"0\" /></NeoItem>";

        [SetUp]
        public void SetUp()
        {
            m_Form = new WinForms.Form();
            m_Button = new WinForms.Button();
            m_Button.Name = "TestButton";
            m_Button2 = new WinForms.Button();
            m_Button2.Name = "TestButton2";

            m_NameCreationServiceMock = MockRepository.GenerateStub<INameCreationService>();

            m_DesignerHostMock = MockRepository.GenerateStub<IDesignerLoaderHost>();
            m_DesignerHostMock.Stub(x => x.CreateComponent(m_Button.GetType(), m_Button.Name)).Return(m_Button);
            m_DesignerHostMock.Stub(x => x.CreateComponent(m_Button2.GetType(), m_Button2.Name)).Return(m_Button2);
            m_DesignerHostMock.Stub(x => x.CreateComponent(m_Button.GetType())).Return(m_Button);
            m_DesignerHostMock.Stub(x => x.CreateComponent(m_Button2.GetType())).Return(m_Button2);
            m_DesignerHostMock.Stub(x => x.GetService(typeof(INameCreationService))).Return(m_NameCreationServiceMock);
            m_DesignerHostMock.Stub(x => x.Container).Return(m_Form.Container);

            m_NameCreationServiceMock.Stub(x => x.CreateName(m_DesignerHostMock.Container, "TestButton")).Return("TestButton2");
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void SerializeButton()
        {
            ObjectSerializer serializer = new ObjectSerializer();
            string serializedObject = serializer.SerializeToString(m_Button);

            Assert.IsNotNull(serializedObject);
            Assert.IsNotEmpty(serializedObject);
        }

        [Test]
        public void DeserializeButton()
        {
            ObjectSerializer serializer = new ObjectSerializer();
            WinForms.Button button = serializer.DeseralizeString(m_SerializedButton) as WinForms.Button;

            Assert.IsNotNull(button);
            Assert.AreEqual(m_Button.Name, button.Name);
        }

        [Test]
        public void DeserializeWithAllowChangeName()
        {
            string errorText = string.Empty;
            m_NameCreationServiceMock.Stub(x => x.IsUniqueAndValidName("TestButton", m_DesignerHostMock.Container, ref errorText)).Return(true);
            ObjectSerializer serializer = new ObjectSerializer(new SerializedObjectComponentCreator(m_DesignerHostMock, true));
            WinForms.Button button = serializer.DeseralizeString(m_SerializedButton) as WinForms.Button;
            Assert.IsNotNull(button);
            Assert.AreEqual(m_Button.Name, button.Name);
        }

        [Test]
        public void DeserializeWithAllowChangeNameNameConflict()
        {
            string errorText = string.Empty;
            m_NameCreationServiceMock.Stub(x => x.IsUniqueAndValidName("TestButton", m_DesignerHostMock.Container, ref errorText)).Return(false);
            ObjectSerializer serializer = new ObjectSerializer(new SerializedObjectComponentCreator(m_DesignerHostMock, true));
            WinForms.Button button = serializer.DeseralizeString(m_SerializedButton) as WinForms.Button;
            Assert.IsNotNull(button);
            Assert.AreEqual("TestButton2", button.Name);
        }

        [Test]
        public void SerializeWithStaticPropertyInInstanceDescriptor()
        {
            System.Drawing.Color colorToSerialize = System.Drawing.Color.FromName("Black");

            ObjectSerializer serializer = new ObjectSerializer();
            string serializedColor = serializer.SerializeToString(colorToSerialize);

            System.Drawing.Color deserializedColor = (System.Drawing.Color)serializer.DeseralizeString(serializedColor);

            Assert.IsNotNull(deserializedColor);
            Assert.AreEqual(colorToSerialize, deserializedColor);
        }

        [Test]
        public void SerializeWithFactoryMethodInInstanceDescriptor()
        {
            System.Drawing.Color colorToSerialize = System.Drawing.Color.FromArgb(128, 128, 128, 128);

            ObjectSerializer serializer = new ObjectSerializer();
            string serializedColor = serializer.SerializeToString(colorToSerialize);

            System.Drawing.Color deserializedColor = (System.Drawing.Color)serializer.DeseralizeString(serializedColor);

            Assert.IsNotNull(deserializedColor);
            Assert.AreEqual(colorToSerialize, deserializedColor);
        }

        [Test]
        public void SerializeWithEnumInInstanceDescriptor()
        {
            System.Windows.GridLength colorToSerialize = new System.Windows.GridLength(11.1, System.Windows.GridUnitType.Star);

            ObjectSerializer serializer = new ObjectSerializer();
            string serializedColor = serializer.SerializeToString(colorToSerialize);

            System.Windows.GridLength deserializedColor = (System.Windows.GridLength)serializer.DeseralizeString(serializedColor);

            Assert.IsNotNull(deserializedColor);
            Assert.AreEqual(colorToSerialize, deserializedColor);
        }

        [Test]
        public void FiresCreatedEventWhenObjectIsFullyInitializedWithParameterizedConstructor()
        {
            ComponentTestObject testObject = new ComponentTestObject("Initialized!");

            string text = "Uninitialized!";
            byte counter = 0;
            var componentCreationInfo = new SerializedObjectComponentCreator(null, false, (sender, e) =>
            {
                text = ((ComponentTestObject)e.Component).Text;
                counter += 1;
            });

            ObjectSerializer serializer = new ObjectSerializer(componentCreationInfo);

            string serializedButton = serializer.SerializeToString(testObject);
            Assert.That(serializedButton, Does.Contain("<InstanceDescriptor"));

            

            ComponentTestObject deserializedTestObject = serializer.DeseralizeString(serializedButton) as ComponentTestObject;

            Assert.That(text, Is.EqualTo(testObject.Text));
            Assert.That(testObject, Is.Not.SameAs(deserializedTestObject));
            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void FiresCreatedEventWhenObjectIsFullyInitializedWithEmptyConstructor()
        {
            ComponentTestObject testObject = new ComponentTestObject("Initialized!");
            testObject.Description = "ForcesObjectToNotBeSerializedWithInstanceDescriptor";

            string description = "Uninitialized!";
            byte counter = 0;
            ISerializedObjectComponentCreator serializedObjectComponentCreator = new SerializedObjectComponentCreator(null, false, (sender, e) =>
            {
                description = ((ComponentTestObject)e.Component).Description;
                counter += 1;
            });

            ObjectSerializer serializer = new ObjectSerializer(serializedObjectComponentCreator);

            string serializedButton = serializer.SerializeToString(testObject);
            Assert.That(serializedButton, Is.Not.Contains("<InstanceDescriptor"));

            ComponentTestObject deserializedTestObject = serializer.DeseralizeString(serializedButton) as ComponentTestObject;

            Assert.That(description, Is.EqualTo(testObject.Description));
            Assert.That(testObject, Is.Not.SameAs(deserializedTestObject));
            Assert.That(counter, Is.EqualTo(1));
        }

    }

}
