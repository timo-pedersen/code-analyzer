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
using NSubstitute;
using NUnit.Framework;

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
            m_ToolboxManagerService = TestHelper.CreateAndAddServiceStub<IToolboxManagerService>();
            m_ReflectionOnlyService = TestHelper.CreateAndAddServiceStub<IReflectionOnlyService>();
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
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
            m_ToolboxManagerService.GetToolboxItems().Returns(new ToolboxItemCollection(itemsToLoad.ToArray()));
        }

        private ControlToolboxItem CreateToolboxItem(string displayName)
        {
            IControlInfo controlInfo = Substitute.For<IControlInfo>();
            IControlMetadata controlMetadata = Substitute.For<IControlMetadata>();
            controlMetadata.DisplayName.Returns(displayName);
            controlInfo.Metadata.Returns(controlMetadata);
            return new ControlToolboxItem(controlInfo) { DisplayName = displayName };
        }

        private void StubCheckOfAvailabilityOnToolBoxItem(ControlToolboxItem toolboxItem, bool available)
        {
            IRemoteAssembly remoteAssembly = Substitute.For<IRemoteAssembly>();
            remoteAssembly.Location = string.Empty;

            IRemoteType remoteType = Substitute.For<IRemoteType>();
            remoteType.FullName.Returns(toolboxItem.DisplayName);
            remoteType.Assembly.Returns(remoteAssembly);

            m_ReflectionOnlyService.GetType(toolboxItem).Returns(remoteType);
            m_ReflectionOnlyService.IsControlType(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            m_ReflectionOnlyService.IsControlTypeSupported(remoteType).Returns(available);
        }

        [Test]
        public void ShouldRevaluateAvailableToolboxItemsOnProjectOpen()
        {
            ControlToolboxItem toolBoxItem = CreateToolboxItem("AvailableItem!");
            StubLoadOfToolBoxItems(new[] { toolBoxItem });
            ToolboxViewModel toolboxViewModel = new ToolboxViewModel();

            Assert.That(toolboxViewModel.ToolboxItems.First().IsEnabled, Is.True);
            StubCheckOfAvailabilityOnToolBoxItem(toolBoxItem, false);

            Raise.EventWith(ProjectManagerStub, new ProjectOpenedEventArgs(ProjectOriginEnum.Created));

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
            GlobalCommandServiceStub.Received().ActivateTool(controlToolboxItem);
        }
    }
}
