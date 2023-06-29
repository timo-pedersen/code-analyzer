#if!VNEXT_TARGET
using System.Windows.Controls;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

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

            m_DesignerHostStub = Substitute.For<INeoDesignerHost>();

            ScreenRootDesignerStub = Substitute.For<IScreenRootDesigner>(); //TestHelper.AddServiceStub<IScreenEditorDesigner>();
            m_DesignerHostStub.RootDesigner.Returns(ScreenRootDesignerStub as IDesignerBase);

            Canvas canvas = new Canvas();

            RectangleOne = new Rectangle();
            RectangleOne.Name = RectangleOneName;
            canvas.Children.Add(RectangleOne);

            RectangleTwo = new Rectangle();
            RectangleTwo.Name = RectangleTwoName;
            canvas.Children.Add(RectangleTwo);

            ScreenRootDesignerStub.FindElementByName(RectangleOneName).Returns(RectangleOne);
            ScreenRootDesignerStub.FindElementByName(RectangleTwoName).Returns(RectangleTwo);
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
#endif
