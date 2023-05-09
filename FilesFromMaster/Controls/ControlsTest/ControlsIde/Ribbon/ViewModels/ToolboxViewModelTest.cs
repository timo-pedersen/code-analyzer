using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Input;
using Neo.ApplicationFramework.Common.Toolbox;
using Neo.ApplicationFramework.Controls.Ribbon.Commands;
using Neo.ApplicationFramework.Controls.Ribbon.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Events;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.ViewModels
{
    [TestFixture]
    public class ToolboxViewModelTest : RibbonViewModelTestBase
    {
        private IReflectionOnlyService m_ReflectionOnlyService;
        private IToolboxManagerService m_ToolboxManagerService;

        [SetUp]
        public void SetUp()
        {
            m_ToolboxManagerService = TestHelper.CreateAndAddServiceMock<IToolboxManagerService>();
            m_ReflectionOnlyService = TestHelper.CreateAndAddServiceMock<IReflectionOnlyService>();
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
        }

        [TearDown]
        public void TearDown()
        {
            m_ReflectionOnlyService = null;
            m_ToolboxManagerService = null;
            TestHelper.ClearServices();
        }

        public void StubLoadOfToolBoxItems(IEnumerable<ToolboxItem> itemsToLoad)
        {
            m_ToolboxManagerService.Stub(x => x.GetToolboxItems()).Repeat.Any().Return(new ToolboxItemCollection(itemsToLoad.ToArray()));
        }

        private ControlToolboxItem CreateToolboxItem(string displayName)
        {
            IControlInfo controlInfo = MockRepository.GenerateStub<IControlInfo>();
            IControlMetadata controlMetadata = MockRepository.GenerateStub<IControlMetadata>();
            controlMetadata.Stub(x => x.DisplayName).Return(displayName);
            controlInfo.Stub(x => x.Metadata).Return(controlMetadata);
            return new ControlToolboxItem(controlInfo) { DisplayName = displayName };
        }

        private void StubCheckOfAvailabilityOnToolBoxItem(ControlToolboxItem toolboxItem, bool available)
        {
            IRemoteAssembly remoteAssembly = MockRepository.GenerateStub<IRemoteAssembly>();
            remoteAssembly.Location = String.Empty;

            IRemoteType remoteType = MockRepository.GenerateStub<IRemoteType>();
            remoteType.Stub(x => x.FullName).Return(toolboxItem.DisplayName);
            remoteType.Stub(x => x.Assembly).Return(remoteAssembly);

            m_ReflectionOnlyService.Stub(x => x.GetType(toolboxItem)).Return(remoteType);
            m_ReflectionOnlyService.Stub(x => x.IsControlType(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            m_ReflectionOnlyService.Stub(x => x.IsControlTypeSupported(remoteType)).Return(available);
        }

        [Test]
        public void ShouldRevaluateAvailableToolboxItemsOnProjectOpen()
        {
            ControlToolboxItem toolBoxItem = CreateToolboxItem("AvailableItem!");
            StubLoadOfToolBoxItems(new[] { toolBoxItem });
            ToolboxViewModel toolboxViewModel = new ToolboxViewModel();

            Assert.That(toolboxViewModel.ToolboxItems.First().IsEnabled, Is.True);
            StubCheckOfAvailabilityOnToolBoxItem(toolBoxItem, false);

            ProjectManagerStub.Raise(x => x.ProjectOpened += null, this, new ProjectOpenedEventArgs(ProjectOriginEnum.Created));

            Assert.That(toolboxViewModel.ToolboxItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddsOnlyControlToolboxItemsToTheList()
        {
            ControlToolboxItem controlToolboxItem = CreateToolboxItem("AvailableItem!");
            ToolboxItem noneControlToolboxItem = new ToolboxItem(this.GetType());
            StubLoadOfToolBoxItems(new[] { controlToolboxItem, noneControlToolboxItem });

            ToolboxViewModel toolboxViewModel = new ToolboxViewModel();


            Assert.That(toolboxViewModel.ToolboxItems.Count, Is.EqualTo(1));
            Assert.That(toolboxViewModel.ToolboxItems.First().ToolboxItem, Is.SameAs(controlToolboxItem));
        }

        [Test]
        public void CreatesCommandsForFoundToolboxItems()
        {
            ControlToolboxItem controlToolboxItem = CreateToolboxItem("ToolboxName");
            StubLoadOfToolBoxItems(new[] { controlToolboxItem });

            ToolboxViewModel toolboxViewModel = new ToolboxViewModel();

            ToolboxItemCommand createdCommand = toolboxViewModel.ToolboxItems.First();
            Assert.That(createdCommand.ToolboxItem, Is.SameAs(controlToolboxItem));
            Assert.That(createdCommand.ToolTipTitle, Is.EqualTo(string.Format("Select the {0}", controlToolboxItem.DisplayName)));
            Assert.That(createdCommand.Text, Is.EqualTo(controlToolboxItem.DisplayName));
        }

        [Test]
        public void CreatedCommandsActivatesToolboxItemWhenExecuted()
        {
            ControlToolboxItem controlToolboxItem = CreateToolboxItem("ToolboxName");
            StubLoadOfToolBoxItems(new[] { controlToolboxItem });

            ToolboxViewModel toolboxViewModel = new ToolboxViewModel();

            ((ICommand)toolboxViewModel.ToolboxItems.First()).Execute(null);
            GlobalCommandServiceStub.AssertWasCalled(x => x.ActivateTool(controlToolboxItem));
        }

    }
}
