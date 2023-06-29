using System;
using System.IO.Packaging;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Commands;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon
{
    public abstract class RibbonViewModelTestBase
    {
        protected IGlobalCommandService GlobalCommandServiceStub { get; private set; }
        protected IGlobalSelectionService GlobalSelectionServiceStub { get; private set; }
        protected ITargetService TargetServiceStub { get; private set; }
        protected IProjectManager ProjectManagerStub { get; private set; }
        protected IScreenCacheSetupService ScreenCacheSetupServiceStub { get; private set; }

        [SetUp]
        protected virtual void Setup()
        {
            PackUriHelper.Create(new Uri("reliable://0"));
            GlobalCommandServiceStub = Substitute.For<IGlobalCommandService>();
            GlobalSelectionServiceStub = Substitute.For<IGlobalSelectionService>();
            TargetServiceStub = Substitute.For<ITargetService>();
            ProjectManagerStub = Substitute.For<IProjectManager>();
            ScreenCacheSetupServiceStub = Substitute.For<IScreenCacheSetupService>();

            TestHelper.ClearServices();
            TestHelper.AddService(GlobalCommandServiceStub);
            TestHelper.AddService(GlobalSelectionServiceStub);
            TestHelper.AddService(TargetServiceStub);
            TestHelper.AddService(ProjectManagerStub);
            TestHelper.AddService(ScreenCacheSetupServiceStub);

            ScreenCacheSetupServiceStub.IsScreenCacheEnabledByDefault = true;
        }

        protected void AssertSetPropertyInCommandServiceWasCalled<TTypeOfPropertyValue>(string propertyName, TTypeOfPropertyValue propertyValue, string undoDescription)
        {
            GlobalCommandServiceStub.Received().SetProperty(propertyName, propertyValue, undoDescription);
        }

        protected void AssertSetPropertyInCommandServiceWasNotCalled<TTypeOfPropertyValue>(string propertyName, TTypeOfPropertyValue propertyValue, string undoDescription)
        {
            GlobalCommandServiceStub.DidNotReceive().SetProperty(propertyName, propertyValue, undoDescription);
        }

        protected IGlobalCommandService RealGlobalCommandService
        {
            get
            {
                return new GlobalCommandService();
            }
        }
    }
}
