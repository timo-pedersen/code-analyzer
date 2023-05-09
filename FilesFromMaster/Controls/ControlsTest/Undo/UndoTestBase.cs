using System.Windows.Controls;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Undo
{
    public abstract class UndoTestBase
    {
        protected const string RectangleOneName = "rectangleOne";
        protected const string RectangleTwoName = "rectangleTwo";

        private INeoDesignerHost m_DesignerHostStub;
        protected IScreenRootDesigner ScreenRootDesignerStub;
        protected Rectangle RectangleOne;
        protected Rectangle RectangleTwo;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());

            m_DesignerHostStub = MockRepository.GenerateStub<INeoDesignerHost>();

            ScreenRootDesignerStub = MockRepository.GenerateStub<IScreenRootDesigner>(); //TestHelper.AddServiceStub<IScreenEditorDesigner>();
            m_DesignerHostStub.Stub(x => x.RootDesigner).Return(ScreenRootDesignerStub as IDesignerBase);

            Canvas canvas = new Canvas();

            RectangleOne = new Rectangle();
            RectangleOne.Name = RectangleOneName;
            canvas.Children.Add(RectangleOne);

            RectangleTwo = new Rectangle();
            RectangleTwo.Name = RectangleTwoName;
            canvas.Children.Add(RectangleTwo);

            ScreenRootDesignerStub.Stub(x => x.FindElementByName(RectangleOneName)).Return(RectangleOne);
            ScreenRootDesignerStub.Stub(x => x.FindElementByName(RectangleTwoName)).Return(RectangleTwo);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        protected INeoDesignerHost DesignerHost
        {
            get { return m_DesignerHostStub; }
        }
    }
}