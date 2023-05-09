using Core.Api.DI.PlatformFactory;
using Core.Api.GlobalReference;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.MessageFilter;
using Neo.ApplicationFramework.Common.Security;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen.Momentary;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Threading;
using Neo.ApplicationFramework.Utilities.Lazy;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Script
{
    [TestFixture]
    public class ScreenAdapterTest
    {
        //private WindowThreadHelper m_OldWindowThreadHelper;
        private IToolManager m_ToolManager;
        private Form m_Form;
        private ScreenAdapter m_ScreenAdapter;
        private IScreenManager m_ScreenManager;

        [SetUp]
        public void TestFixtureSetup()
        {
            TestHelper.CreateAndAddServiceStub<IMomentaryServiceCF>();

            TestHelper.UseTestWindowThreadHelper = true;
       
            var platformFactoryService = TestHelper.CreateAndAddServiceStub<IPlatformFactoryService>();
            TestHelper.AddService(typeof(INativeAPI), new NativeAPI());
            platformFactoryService.Create<DelayMouseInputMessageFilterBase>().Returns(new DelayMouseInputMessageFilter());
            platformFactoryService.Create<SecurityMessageFilterBase>().Returns(new SecurityMessageFilter());

            var securityService = TestHelper.CreateAndAddServiceStub<ISecurityServiceCF>();
            securityService.IsAccessGranted(Arg.Any<object>()).Returns(true);

            m_ToolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_ScreenManager = TestHelper.CreateAndAddServiceStub<IScreenManager>();
            m_ScreenManager.WhenForAnyArgs(x => x.CloseActiveScreen(Arg.Any<IScreen>()))
                .Do(invocation => ((System.Windows.Forms.Form)invocation[0]).Close());
            m_ScreenManager.StartupScreenHasBeenShown.Returns(true);

            m_ToolManager.Runtime.Returns(false);
            m_Form = new Form(
                new LazyCF<IScopeService>(() => new ScopeServiceCF()), 
                new LazyWrapper<IGlobalReferenceService>(() => Substitute.For<IGlobalReferenceService>()), 
                false);

            m_ScreenManager.RegisterScreen(m_Form).Returns(true);

            m_ToolManager.Runtime.Returns(true);

            m_ScreenAdapter = new ScreenAdapter();
            m_ScreenAdapter.AdaptedObject = m_Form;
        }

        [TearDown]
        public void Teardown()
        {
            m_ScreenAdapter.AdaptedObject = null;

            m_Form.Dispose();
        }

        [Test]
        public void OpenedCalledOnce()
        {
            int callCount = 0;

            m_ScreenAdapter.Load += (sender, args) => callCount += 1;
            m_Form.Show();
            m_Form.Close();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ClosingCalledOnce()
        {
            int callCount = 0;

            m_ScreenAdapter.Closing += (sender, args) => callCount += 1;
            m_Form.Show();
            m_Form.Close();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ClosedCalledOnce()
        {
            int callCount = 0;

            m_ScreenAdapter.Closed += (sender, args) => callCount += 1;
            m_Form.Show();
            m_Form.Close();

            Assert.AreEqual(1, callCount);
        }
    }
}
