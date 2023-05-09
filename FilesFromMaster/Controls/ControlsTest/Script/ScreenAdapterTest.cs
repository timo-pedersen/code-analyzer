using Core.Api.DI.PlatformFactory;
using Core.Api.GlobalReference;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.MessageFilter;
using Neo.ApplicationFramework.Common.Security;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Common.Utilities.Threading;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen.Momentary;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Storage.Threading;

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
            platformFactoryService.Stub(x => x.Create<DelayMouseInputMessageFilterBase>()).Return(new DelayMouseInputMessageFilter());
            platformFactoryService.Stub(x => x.Create<SecurityMessageFilterBase>()).Return(new SecurityMessageFilter());

            var securityService = TestHelper.CreateAndAddServiceStub<ISecurityServiceCF>();
            securityService.Stub(x => x.IsAccessGranted(null)).IgnoreArguments().Return(true);

            m_ToolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_ScreenManager = TestHelper.CreateAndAddServiceStub<IScreenManager>();
            m_ScreenManager.Stub(manager => manager.CloseActiveScreen(null)).IgnoreArguments().WhenCalled(invocation => ((System.Windows.Forms.Form)invocation.Arguments[0]).Close());
            m_ScreenManager.Stub(manager => manager.StartupScreenHasBeenShown).Return(true);

            m_ToolManager.Stub(x => x.Runtime).Return(false);
            m_Form = new Form(
                new LazyCF<IScopeService>(() => new ScopeServiceCF()), 
                new LazyWrapper<IGlobalReferenceService>(() => MockRepository.GenerateStub<IGlobalReferenceService>()),
                MockRepository.GenerateStub<IMultiLanguageServiceCF>(),
                false);

            m_ScreenManager.Stub(x => x.RegisterScreen(m_Form)).Return(true);

            m_ToolManager.Stub(x => x.Runtime).Return(true);

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
