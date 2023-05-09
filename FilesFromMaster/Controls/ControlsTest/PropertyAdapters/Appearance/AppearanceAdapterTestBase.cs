using System.ComponentModel;
using System.Windows;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Core.Api.Tools;
using Neo.ApplicationFramework.Common.TypeConverters;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance
{
    [TestFixture]
    public abstract class AppearanceAdapterTestBase
    {
        protected WPFToCFTypeDescriptionProvider m_WPFToCFTypeDescriptionProvider;
        protected IAppearanceAdapterService m_AppearanceAdapterService;
        protected IToolManager m_ToolManagerMock;

        [SetUp]
        public virtual void SetUp()
        {
            m_ToolManagerMock = TestHelper.CreateAndAddServiceMock<IToolManager>();

            m_WPFToCFTypeDescriptionProvider = new WPFToCFTypeDescriptionProvider(typeof(object));
            TypeDescriptor.AddProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            m_AppearanceAdapterService = new AppearanceAdapterService();
            TestHelper.AddService(m_AppearanceAdapterService);

            ITarget target = new Target(TargetPlatform.WindowsCE, string.Empty, string.Empty);
            var targetServiceMock = TestHelper.CreateAndAddServiceMock<ITargetService>();
            targetServiceMock.Expect(x => x.CurrentTarget).Return(target);
        }

        [TearDown]
        public virtual void TearDown()
        {
            TypeDescriptor.RemoveProvider(m_WPFToCFTypeDescriptionProvider, typeof(object));

            TestHelper.ClearServices();
        }

        protected virtual T CreateElement<T>() where T : UIElement, new()
        {
            T element = new T();

            return element;
        }


    }
}