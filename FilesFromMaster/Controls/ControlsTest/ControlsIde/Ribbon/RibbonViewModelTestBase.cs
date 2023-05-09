using System;
using System.IO.Packaging;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Commands;
using NUnit.Framework;
using Rhino.Mocks;

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
            GlobalCommandServiceStub = MockRepository.GenerateMock<IGlobalCommandService>();
            GlobalSelectionServiceStub = MockRepository.GenerateMock<IGlobalSelectionService>();
            TargetServiceStub = MockRepository.GenerateMock<ITargetService>();
            ProjectManagerStub = MockRepository.GenerateMock<IProjectManager>();
            ScreenCacheSetupServiceStub = MockRepository.GenerateStub<IScreenCacheSetupService>();

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
            GlobalCommandServiceStub.AssertWasCalled(x => x.SetProperty(propertyName, propertyValue,
                undoDescription));
        }

        protected void AssertSetPropertyInCommandServiceWasNotCalled<TTypeOfPropertyValue>(string propertyName, TTypeOfPropertyValue propertyValue, string undoDescription)
        {
            GlobalCommandServiceStub.AssertWasNotCalled(x => x.SetProperty(propertyName, propertyValue,
                undoDescription));
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
