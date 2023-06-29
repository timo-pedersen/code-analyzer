using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Api.ProjectTarget;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.ComPorts;
using Neo.ApplicationFramework.Controls.Ribbon.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon
{
    [TestFixture]
    public class ComPortsViewModelTest
    {
        private IGlobalCommandService GlobalCommandServiceStub { get; set; }
        private ITargetService TargetServiceStub { get; set; }
        private IProject ProjectStub { get; set; }
        private ITargetInfo TargetInfo { get; set; }
        private IEventBrokerService EventBrokerServiceStub { get; set; }
        private IObjectNotificationService ObjectNotificationService { get; set; }
        private ITerminal TerminalFake { get; set; }
        private IDictionary<ComPort, PortMode> ComPortModes { get; set; }

        [SetUp]
        public void Setup()
        {
            AddServices();

            if (!UriParser.IsKnownScheme("pack"))
                new System.Windows.Application();
        }

        private void AddServices()
        {
            GlobalCommandServiceStub = Substitute.For<IGlobalCommandService>();
            EventBrokerServiceStub = Substitute.For<IEventBrokerService>();
            ObjectNotificationService = Substitute.For<IObjectNotificationService>();

            ProjectStub = Substitute.For<IProject>();

            ComPortModes = new Dictionary<ComPort, PortMode>();
            ProjectStub.ComPortModes = ComPortModes;

            IProjectManager projectManagerMock = Substitute.For<IProjectManager>();
            projectManagerMock.Project = ProjectStub;

            TestHelper.ClearServices();
            TestHelper.AddService(GlobalCommandServiceStub);
            TestHelper.AddService(projectManagerMock);
            TestHelper.AddService(EventBrokerServiceStub);
            TestHelper.AddService(ObjectNotificationService);
            TestHelper.AddService(Substitute.For<ICommandManagerService>());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void AssertThatAllPortsAreSupportedForBumblebee()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.True);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatOnlyCom1AndCom2AreSupportedForExter()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatOnlyCom1AndCom2AreSupportedForQTerm()
        {
            ComPortsViewModel model = SetUpQTermTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatAllPortsAreSupportedForGalvatron()
        {
            ComPortsViewModel model = SetUpGalvatronTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatAllPortsAreSupportedForExtreme()
        {
            ComPortsViewModel model = SetUpExtremeTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatAllRequiredPortsAreSupportedForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.True);
            Assert.That(model.IsCom5Supported, Is.True);
            Assert.That(model.IsCom6Supported, Is.False);
        }

        [Test]
        public void AssertThatCom4IsDisabledForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            Assert.That(model.IsCom4Enabled, Is.False);
        }

        [Test]
        public void AssertThatAllRequiredPortsAreSupportedForTxF3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.True);
            Assert.That(model.IsCom5Supported, Is.True);
            Assert.That(model.IsCom6Supported, Is.True);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForBumbleBee()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom4PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForExter()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForQTerm()
        {
            ComPortsViewModel model = SetUpQTermTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForGalvatron()
        {
            ComPortsViewModel model = SetUpGalvatronTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForExtreme()
        {
            ComPortsViewModel model = SetUpExtremeTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom5PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom6PortMode, Is.Null);
        }

        [Test]
        public void VerifyDefaultValuesForSelectedComPortsForTxF3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom5PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom6PortMode.Name, Is.EqualTo("RS-485"));
        }

        [Test]
        public void VerifyThatProjectHoldsPortModesForAll4ComPortsForBumbleBee()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectOnlyHoldsPortModesFor2ComPortsForExter()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectOnlyHoldsPortModesFor2ComPortsForQTerm()
        {
            ComPortsViewModel model = SetUpQTermTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectOnlyHoldsPortModesFor2ComPortsForGalvatron()
        {
            ComPortsViewModel model = SetUpGalvatronTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectOnlyHoldsPortModesFor2ComPortsForExtreme()
        {
            ComPortsViewModel model = SetUpExtremeTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectOnlyHoldsPortModesFor3ComPortsForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.False);
        }

        [Test]
        public void VerifyThatProjectHoldsPortModesForAll6ComPortsForTxf3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out _), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM5, out _), Is.True);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM6, out _), Is.True);
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForBumbleBee()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();
            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM4, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForExter()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();
            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForQTerm()
        {
            ComPortsViewModel model = SetUpQTermTest();
            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs232));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForGalvatron()
        {
            ComPortsViewModel model = SetUpGalvatronTest();
            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForExtreme()
        {
            ComPortsViewModel model = SetUpExtremeTest();
            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            PortMode portMode;

            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            model.ComPortModes.TryGetValue(ComPort.COM5, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));
        }

        [Test]
        public void VerifyDefaultValuesForComPortsInProjectForTxF3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            PortMode portMode;

            model.ComPortModes.TryGetValue(ComPort.COM1, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            model.ComPortModes.TryGetValue(ComPort.COM4, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            model.ComPortModes.TryGetValue(ComPort.COM5, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            model.ComPortModes.TryGetValue(ComPort.COM6, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom2ForBumbleBee()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();
            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");

            PortMode portMode;
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom2AndCom3IsEnabledForGalvatron()
        {
            ComPortsViewModel model = SetUpGalvatronTest();
            PortMode portMode;
            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");

            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));

            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom2AndCom3IsEnabledForExtreme()
        {
            ComPortsViewModel model = SetUpExtremeTest();
            PortMode portMode;
            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));


            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "CAN");
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Can));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));

            model.SelectedCom3PortMode = model.Com3PortModes.First(comPort => comPort.Name == "CAN");
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Can));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Can));

            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.Can));

            model.SelectedCom3PortMode = model.Com3PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM2, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom2AndCom3IsEnabledForTxF3()
        {
            ComPortsViewModel model = SetupTxF3Test();
            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM2, out PortMode portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom2AndCom3IsEnabledForTxF3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            model.SelectedCom2PortMode = model.Com2PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM2, out PortMode portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
            model.ComPortModes.TryGetValue(ComPort.COM3, out portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void VerifyThatSelectedPortModeCanBeChangedForCom5ForTxF3Ext()
        {
            ComPortsViewModel model = SetupTxF3ExtTest();
            model.SelectedCom5PortMode = model.Com5PortModes.First(comPort => comPort.Name == "RS-485");
            model.ComPortModes.TryGetValue(ComPort.COM5, out PortMode portMode);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        [Test]
        public void ChangingTargetFromBumbleBeeToExterShouldSetDefaultValuesForExter()
        {
            ComPortsViewModel model = SetUpBumbleBeeTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            //TerminalFake = new TerminalFake { ComPortModes = ExterTrinityComPorts, TerminalGroup = TerminalGroup.Default };
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromExterToBumbleBeeShouldSetDefaultValuesForBumbleBee()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(BumblebeeComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.True);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom4PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));
        }

        [Test]
        public void ChangingTargetFromExterToExterShouldSetSameValuesForExter()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();
            model.SelectedCom1PortMode = model.Com1PortModes.Where(comPort => comPort.Name == "RS-485").First();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromQTermToExterShouldSetSameValuesForExter()
        {
            ComPortsViewModel model = SetUpQTermTest();
            model.SelectedCom1PortMode = model.Com1PortModes.Where(comPort => comPort.Name == "RS-485").First();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);

            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromQTermToExterShouldSetSameDefaultValuesForExterWhenOldSelectedValueIsUnsupported()
        {
            ComPortsViewModel model = SetUpQTermTest();
            model.SelectedCom1PortMode = model.Com1PortModes.First(comPort => comPort.Name == "RS-232");

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromGalvatronToExterShouldSetDefaultValuesForExter()
        {
            ComPortsViewModel model = SetUpGalvatronTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            //TerminalFake = new TerminalFake { ComPortModes = ExterTrinityComPorts, TerminalGroup = TerminalGroup.Default };
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromExtremeToExterShouldSetDefaultValuesForExter()
        {
            ComPortsViewModel model = SetUpExtremeTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.False);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom3PortMode, Is.Null);
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromExterToGalvatronShouldSetDefaultValuesForGalvatron()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(GalvatronComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingTargetFromExterToExtremeShouldSetDefaultValuesForExtreme()
        {
            ComPortsViewModel model = SetUpExterTrinityTest();

            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExtremeComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.IsCom1Supported, Is.True);
            Assert.That(model.IsCom2Supported, Is.True);
            Assert.That(model.IsCom3Supported, Is.True);
            Assert.That(model.IsCom4Supported, Is.False);
            Assert.That(model.IsCom5Supported, Is.False);
            Assert.That(model.IsCom6Supported, Is.False);

            Assert.That(model.SelectedCom1PortMode.Name, Is.EqualTo("RS-232"));
            Assert.That(model.SelectedCom2PortMode.Name, Is.EqualTo("RS-422"));
            Assert.That(model.SelectedCom3PortMode.Name, Is.EqualTo("RS-485"));
            Assert.That(model.SelectedCom4PortMode, Is.Null);
            Assert.That(model.SelectedCom5PortMode, Is.Null);
            Assert.That(model.SelectedCom6PortMode, Is.Null);

            PortMode portMode;
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM1, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.Nonconfigurable));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);
            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM4, out portMode), Is.False);
        }

        [Test]
        public void ChangingPortSettingsFromProjectUpdatesViewModel()
        {
            PortMode portMode;
            ComPortsViewModel model = SetUpGalvatronTest();
            ServiceContainerCF.Instance.RemoveService(typeof(ITargetService), true);
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(GalvatronComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            model.Init();

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs422));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.False);

            ProjectStub.WhenForAnyArgs(x => x.SetComPortMode(Arg.Any<ComPort>(), Arg.Any<PortMode>()))
                .Do(y => SetComPortMode((ComPort)y[0], (PortMode)y[1]));
            ProjectStub.SetComPortMode(ComPort.COM2, PortMode.rs485);

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM2, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));

            Assert.That(model.ComPortModes.TryGetValue(ComPort.COM3, out portMode), Is.True);
            Assert.That(portMode, Is.EqualTo(PortMode.rs485));
        }

        private void SetComPortMode(ComPort comPort, PortMode portMode)
        {
            if (ComPortModes == null)
            {
                ComPortModes = new Dictionary<ComPort, PortMode>();
            }
            else if (ComPortModes.ContainsKey(comPort))
            {
                PortMode oldPortMode;
                ComPortModes.TryGetValue(comPort, out oldPortMode);
                if (oldPortMode == portMode)
                    return;

                ComPortModes.Remove(comPort);
            }

            ComPortModes.Add(comPort, portMode);

            FireComPortModesChanged();
        }

        private void FireComPortModesChanged()
        {
            Raise.Event<PropertyChangedEventHandler>(ProjectStub, new PropertyChangedEventArgs(nameof(ProjectStub.ComPortModes)));
        }

        private ComPortsViewModel SetUpBumbleBeeTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(BumblebeeComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetUpExterTrinityTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExterTrinityComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);

            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetUpQTermTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(QTermComPorts);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetUpGalvatronTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(GalvatronComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetUpExtremeTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(ExtremeComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetupTxF3Test()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(TxF3ComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private ComPortsViewModel SetupTxF3ExtTest()
        {
            TerminalFake = Substitute.For<ITerminal>();
            TerminalFake.ComPortModes.Returns(TxF3ExtComPorts);
            TerminalFake.TerminalGroup.Returns(TerminalGroup.Default);
            SetupTargetMock();
            return SetUpViewModel();
        }

        private void SetupTargetMock()
        {
            TargetInfo = new TargetInfo();
            TargetInfo.TerminalDescription = TerminalFake;

            TargetServiceStub = Substitute.For<ITargetService>();
            TargetServiceStub.CurrentTargetInfo.Returns(TargetInfo);
            TestHelper.AddService(TargetServiceStub);
        }

        private ComPortsViewModel SetUpViewModel()
        {
            ComPortsViewModel comPortsViewModel = new ComPortsViewModel();
            comPortsViewModel.Init();
            comPortsViewModel.UpdateComPortModes();
            return comPortsViewModel;
        }

        private Dictionary<ComPort, IComPortSettings> BumblebeeComPorts
        {
            get
            {
                int[] disables = {};
                Dictionary<ComPort, IComPortSettings> bumbleBeeComPorts = new Dictionary<ComPort, IComPortSettings>();
                bumbleBeeComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 0, disables) }, false));
                bumbleBeeComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 0, disables), new ComPortMode("RS-485", 0,disables) }, true));
                bumbleBeeComPorts.Add(ComPort.COM3, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 0, disables) }, false));
                bumbleBeeComPorts.Add(ComPort.COM4, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 0, disables), new ComPortMode("RS-485", 0,disables) }, true));
                return bumbleBeeComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> ExterTrinityComPorts
        {
            get
            {
                int[] disables = {};
                Dictionary<ComPort, IComPortSettings> bumbleBeeComPorts = new Dictionary<ComPort, IComPortSettings>();
                bumbleBeeComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 0, disables), new ComPortMode("RS-485", 0, disables) }, true));
                bumbleBeeComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 0, disables) }, false));
                return bumbleBeeComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> QTermComPorts
        {
            get
            {
                int[] disables = {};
                Dictionary<ComPort, IComPortSettings> bumbleBeeComPorts = new Dictionary<ComPort, IComPortSettings>();
                bumbleBeeComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 0, disables), new ComPortMode("RS-422", 0, disables), new ComPortMode("RS-485", 0, disables) }, true));
                bumbleBeeComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 0, disables) }, false));
                return bumbleBeeComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> GalvatronComPorts
        {
            get
            {
                int[] disables = {};
                int[] disables422 = {4};

                Dictionary<ComPort, IComPortSettings> galavatronComPorts = new Dictionary<ComPort, IComPortSettings>();
                galavatronComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 1, disables) }, false));
                galavatronComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 2, disables422), new ComPortMode("RS-485", 3, disables) }, true));
                galavatronComPorts.Add(ComPort.COM3, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 4, disables) }, true));
                return galavatronComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> ExtremeComPorts
        {
            get
            {
                int[] disables = { };
                int[] disables422 = { 5,6 };

                Dictionary<ComPort, IComPortSettings> galavatronComPorts = new Dictionary<ComPort, IComPortSettings>();
                galavatronComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 1, disables) }, false));
                galavatronComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 2, disables422), new ComPortMode("RS-485", 3, disables), new ComPortMode("CAN", 4, disables) }, true));
                galavatronComPorts.Add(ComPort.COM3, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 5, disables), new ComPortMode("CAN", 6, disables) }, true));
                return galavatronComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> TxF3ComPorts
        {
            get
            {
                int[] disables = { };
                int[] disables422 = { 4 };

                Dictionary<ComPort, IComPortSettings> txf3ComPorts = new Dictionary<ComPort, IComPortSettings>();
                txf3ComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 1, disables) }, false));
                txf3ComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 2, disables422), new ComPortMode("RS-485", 3, disables) }, true));
                txf3ComPorts.Add(ComPort.COM3, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 4, disables) }, false));
                txf3ComPorts.Add(ComPort.COM4, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 5, disables) }, false, true));
                txf3ComPorts.Add(ComPort.COM5, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 6, disables) }, false));
                return txf3ComPorts;
            }
        }

        private Dictionary<ComPort, IComPortSettings> TxF3ExtComPorts
        {
            get
            {
                int[] disables = { };
                int[] disables422 = { 4 };

                Dictionary<ComPort, IComPortSettings> txf3extComPorts = new Dictionary<ComPort, IComPortSettings>();
                txf3extComPorts.Add(ComPort.COM1, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 1, disables) }, false));
                txf3extComPorts.Add(ComPort.COM2, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 2, disables422), new ComPortMode("RS-485", 3, disables) }, true));
                txf3extComPorts.Add(ComPort.COM3, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 4, disables) }, false));
                txf3extComPorts.Add(ComPort.COM4, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-232", 5, disables) }, false));
                txf3extComPorts.Add(ComPort.COM5, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-422", 6, disables), new ComPortMode("RS-485", 7, disables) }, true));
                txf3extComPorts.Add(ComPort.COM6, new ComPortSettings(new List<IComPortMode>() { new ComPortMode("RS-485", 8, disables) }, false));
                return txf3extComPorts;
            }
        }
    }
}