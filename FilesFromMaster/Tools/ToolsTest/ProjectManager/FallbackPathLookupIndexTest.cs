using System;
using System.ComponentModel;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class FallbackPathLookupIndexTest
    {
        private IFallbackPathLookupIndex m_PathLookupIndex;
        private IProjectManager m_ProjectManager;

        private const string GlobalsName = StringConstants.Globals;
        private const string ScreensName = StringConstants.Screens;
        private const string Screen1Name = "Screen1";
        private const string Screen2Name = "Screen2";
        private const string ButtonName = "Button1";
        private const string TextBoxName = "TextBox";
        private const string TextBoxInGroupName = "TextBoxInGroup";
        private const string GroupName = "Group1";

        private Button m_ButtonScreen1;
        private TextBox m_TextBoxScreen1;
        private TextBox m_TextBoxInGroupScreen2;
        private Group m_GroupScreen2;

        private ITestSubItemsComponent m_ContainedScreen1Object;
        private ITestSubItemsComponent m_ContainedScreen2Object;
        private IProjectItem m_ScreensGroup;
        private IDesignerProjectItem m_Screen1Item;
        private IDesignerProjectItem m_Screen2Item;
        private IProject m_Project;


        [OneTimeSetUp]
        public void Setup()
        {
            // Screen 1
            TestSite screenSite = new TestSite { Name = Screen1Name };
            m_ContainedScreen1Object = MockRepository.GenerateStub<ITestSubItemsComponent>();
            m_ContainedScreen1Object.Site = screenSite;

            m_ButtonScreen1 = new Button { Name = ButtonName };
            m_TextBoxScreen1 = new TextBox { Name = TextBoxName };
            System.Windows.FrameworkElement[] elements = { m_ButtonScreen1, m_TextBoxScreen1 };
            m_ContainedScreen1Object.Stub(x => x.Items).Return(elements);

            m_Screen1Item = MockRepository.GenerateStub<IDesignerProjectItem>();
            m_Screen1Item.Name = Screen1Name;
            m_Screen1Item.Stub(x => x.ContainedObject).Return(m_ContainedScreen1Object);
            m_Screen1Item.Stub(x => x.ProjectItems).Return(new IProjectItem[0]);
            m_Screen1Item.Stub(x => x.DesignerType).Return(typeof(Screen.ScreenDesign.Screen));
            // Screen 2
            TestSite screen2Site = new TestSite { Name = Screen2Name };
            m_ContainedScreen2Object = MockRepository.GenerateStub<ITestSubItemsComponent>();
            m_ContainedScreen2Object.Site = screen2Site;

            m_TextBoxInGroupScreen2 = new TextBox { Name = TextBoxInGroupName };
            m_GroupScreen2 = new Group { Name = GroupName };
            m_GroupScreen2.Items.Add(m_TextBoxInGroupScreen2);

            System.Windows.FrameworkElement[] elementsScreen2 = { m_ButtonScreen1, m_GroupScreen2 };
            m_ContainedScreen2Object.Stub( x => x.Items).Return(elementsScreen2);

            m_Screen2Item = MockRepository.GenerateStub<IDesignerProjectItem>();
            m_Screen2Item.Name = Screen2Name;
            m_Screen2Item.Stub(x => x.ContainedObject).Return(m_ContainedScreen2Object);
            m_Screen2Item.Stub(x => x.ProjectItems).Return(new IProjectItem[0]);
            m_Screen2Item.Stub(x => x.DesignerType).Return(typeof(Screen.ScreenDesign.Screen));

            IDesignerProjectItem[] screenItems = { m_Screen1Item, m_Screen2Item };
            m_ScreensGroup = MockRepository.GenerateStub<IProjectItem>();
            m_ScreensGroup.Name = ScreensName;
            m_ScreensGroup.Stub(x => x.ContainedObject).Return(null);
            m_ScreensGroup.Stub(x => x.ProjectItems).Return(screenItems);

            IProjectItem[] groupItems = { m_ScreensGroup };
            m_Project = MockRepository.GenerateStub<IProject>();
            m_Project.Stub(x => x.ProjectItems).Return(groupItems);
            m_Project.Stub(x => x.GetDesignerProjectItems()).Return(screenItems);

            m_ProjectManager = MockRepository.GenerateStub<IProjectManager>();
            m_ProjectManager.Project = m_Project;
            m_PathLookupIndex = new FallbackPathLookupIndex(new LazyWrapper<IProjectManager>(() => m_ProjectManager));
        }

        [Test]
        public void SupportsInterface()
        {
            Assert.IsNotNull(m_PathLookupIndex, "GlobalReferenceService does not support the interface.");
        }

        [Test]
        public void GetObjects()
        {
            object[] objects = m_PathLookupIndex.GetObjects<object>(new string[0], true);
            Assert.AreEqual(6, objects.Length, "Wrong number of objects.");
        }

        [Test]
        public void GetNoButtonObjects()
        {
            object[] components = m_PathLookupIndex.GetObjects<Button>(new string[0], false);
            Assert.AreEqual(0, components.Length, "GetNoButtonObjects shold not get a button object");
        }

        [Test]
        public void GetButtonObjectsGlobally()
        {
            object[] components = m_PathLookupIndex.GetObjects<Button>(new string[0], true);
            Assert.AreEqual(2, components.Length, "GetButtonObjects could not get button.");
        }

        [Test]
        public void GetButtonObjectInScreen()
        {
            ISubItemsServiceIde service = new SubItemsServiceIde();
            var components = service.GetObjects(m_ContainedScreen2Object, typeof(Button));
            Assert.AreEqual(1, components.Length, "GetButtonObjects could not get button.");
        }

        [Test]
        public void GetButtonObjectsGeneric()
        {
            Button[] components = m_PathLookupIndex.GetObjects<Button>(new string[0], true);
            Assert.AreEqual(2, components.Length, "GetButtonObjectsGeneric could not get the buttons.");
        }

        [Test]
        public void GetButtonObjectInSourceGeneric()
        {
            ISubItemsServiceIde service = new SubItemsServiceIde();
            var components = service.GetObjects(m_ContainedScreen2Object, typeof(Button));
            Assert.AreEqual(1, components.Length, "GetButtonObjectInSourceGeneric could not get the button.");
        }

        [Test]
        public void GetButtonObjectFromFullName()
        {
            string fullName = string.Format("{0}.{1}.{2}", GlobalsName, Screen1Name, ButtonName);
            object buttonObject = m_PathLookupIndex.GetObject<object>(fullName);
            Assert.IsNotNull(buttonObject, "Could not find button object from string " + fullName);
        }

        [Test]
        public void GetButtonObjectFromPartialFullName()
        {
            string fullName = string.Format("{0}.{1}", Screen1Name, ButtonName);
            object buttonObject = m_PathLookupIndex.GetObject<object>(fullName);
            Assert.IsNotNull(buttonObject, "Could not find button object from string " + fullName);
        }

        [Test]
        public void GetScreenName()
        {
            string screenName = ElementHelper.GetVariableName(m_ContainedScreen1Object);
            Assert.AreEqual(Screen1Name, screenName, "Wrong screen name");
        }
        
        [Test]
        public void GetTextBoxName()
        {
            string textBoxName = ElementHelper.GetVariableName(m_TextBoxScreen1);
            Assert.AreEqual(TextBoxName, textBoxName, "Wrong text box name");
        }

        [Test]
        public void GetTextBoxInGroupName()
        {
            string textBoxName = ElementHelper.GetVariableName(m_TextBoxInGroupScreen2);
            Assert.AreEqual(TextBoxInGroupName, textBoxName, "Wrong text box name for text box in group");
        }

        [Test]
        public void GetScreen1FromName()
        {
            Assert.IsNotNull(m_PathLookupIndex.GetObject<object>("Globals." + Screen1Name));
        }

        [Test]
        public void GetButtonOnScreen1FromName()
        {
            Assert.IsNotNull(m_PathLookupIndex.GetObject<object>("Globals.Screen1." + ButtonName));
        }

        [Test]
        public void GetTextBoxInGroupFromName()
        {
            Assert.IsNotNull(m_PathLookupIndex.GetObject<object>("Globals.Screen2.Group1." + TextBoxInGroupName));
        }

        [Test]
        public void GetObjectsDoesNotLoadDesigner()
        {
            IDesignerProjectItem designerMock = MockRepository.GenerateMock<IDesignerProjectItem>();
            IDesignerProjectItem[] designerItems = { designerMock };
            
            designerMock.Stub(x => x.ProjectItems).Return(new IDesignerProjectItem[] { });
            designerMock.Stub(x => x.DesignerType).Return(typeof(NotImplementedException)); //Could be ANY type


            IProject project = MockRepository.GenerateMock<IProject>();
            project.Stub(x => x.ProjectItems).Return(designerItems);
            project.Stub(x => x.GetDesignerProjectItems()).Return(designerItems);

            IProjectManager projectManager = MockRepository.GenerateMock<IProjectManager>();
            projectManager.Stub(x => x.Project).Return(project);

            IFallbackPathLookupIndex globalReferenceService = new FallbackPathLookupIndex(new LazyWrapper<IProjectManager>(() => projectManager));

            designerMock.Expect(x => x.ContainedObject).Repeat.Never();
            designerMock.Expect(x => x.DesignerHost).Repeat.Never();

            globalReferenceService.GetObjects<Screen.ScreenDesign.Screen>(new string[0], false);
            globalReferenceService.GetObjects<Screen.ScreenDesign.Screen>(new string[0], false);

            designerMock.VerifyAllExpectations();
        }
    }

    public interface ITestSubItemsComponent : IComponent, ISubItems
    {

    }
}
