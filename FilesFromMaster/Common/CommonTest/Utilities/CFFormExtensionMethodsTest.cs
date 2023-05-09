using System;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen.Momentary;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Form = System.Windows.Forms.Form;

namespace Neo.ApplicationFramework.Common.Utilities
{
    [TestFixture]
    public class CFFormExtensionMethodsTest
    {
        private IMainScreen m_Parent;
        private IScreenManager m_ScreenManager;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IMomentaryServiceWpf>();

            m_Parent = new MainWindow();

            m_ScreenManager = TestHelper.CreateAndAddServiceMock<IScreenManager>();
            m_ScreenManager
                .Expect(screenManager => screenManager.MainScreen)
                .Return(m_Parent);

            TestHelper.AddService(typeof(INativeAPI), new NativeAPI());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void CheckIsRuntimeParentSet()
        {
            // ARRANGE
            var form = new Form();
            INativeAPI nativeApi = new NativeAPI();

            // ACT
            form.SetRuntimeParent();
            IntPtr parentHandle = nativeApi.GetWindowLong(form.Handle, GWL_STYLE.HWNDPARENT);

            // ASSERT
            Assert.That(m_Parent.WindowHandle, Is.Not.EqualTo(IntPtr.Zero), "Probably because WindowHandle (" + m_Parent.WindowHandle + ") cannot be cast to uint. Talk to MKG.");
            Assert.That(parentHandle, Is.EqualTo(m_Parent.WindowHandle), "Probably because WindowHandle (" + m_Parent.WindowHandle + ") cannot be cast to uint. Talk to MKG.");
        }

        [Test]
        public void RuntimeParentIsResetWhenClosing()
        {
            // ARRANGE
            var form = new Form();
            form.SetRuntimeParent();

            IntPtr parentHandle = new IntPtr(1);

            INativeAPI nativeApi = new NativeAPI();
            form.FormClosed += (sender, args) => parentHandle = nativeApi.GetWindowLong(((Form)sender).Handle, GWL_STYLE.HWNDPARENT);

            // ACT
            form.Close();

            // ASSERT
            Assert.That(parentHandle, Is.EqualTo(IntPtr.Zero));
        }
    }
}
