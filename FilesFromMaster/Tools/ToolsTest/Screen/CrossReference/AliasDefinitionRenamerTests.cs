using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Core.Api.CrossReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Controls.Screen.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.Alias;
using Neo.ApplicationFramework.Tools.Screen.CrossReference.Helpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasDefinitionRenamerTests
    {
        private const string OldName = "OldName";
        private const string NewName = "NewName";

        private AliasDefinition m_Alias;
        private IList<FrameworkElement> m_Elements;
        private AliasInstanceList m_AliasInstances;
        private INeoDesignerHost m_DesignerHost;
        private IFindAliasWpfBindings m_AliasWpfBindingFinder;
        private ICrossReferenceRenameService m_RenamerByCrossReferenceService;

        // Service under test
        private IRenameAliases m_AliasRenamer;

        [SetUp]
        public void SetUp()
        {
            m_Alias = new AliasDefinition() { Name = OldName };

            m_Elements = new List<FrameworkElement>();

            m_AliasInstances = new AliasInstanceList(null);

            var aliasConfiguration = MockRepository.GenerateStub<IAliasConfiguration>();
            aliasConfiguration.Instances = m_AliasInstances;

            var screen = MockRepository.GenerateStub<ScreenDesign.Screen>();
            screen.AliasConfiguration = aliasConfiguration;

            var rootDesigner = MockRepository.GenerateStub<IScreenRootDesigner>();
            rootDesigner.Stub(des => des.Component).Return(screen);
            rootDesigner.Stub(des => des.Elements).Return(m_Elements);

            m_DesignerHost = MockRepository.GenerateStub<INeoDesignerHost>();
            m_DesignerHost.Stub(host => host.RootDesigner).Return(rootDesigner);

            m_AliasWpfBindingFinder = MockRepository.GenerateStub<IFindAliasWpfBindings>();
            m_RenamerByCrossReferenceService = MockRepository.GenerateMock<ICrossReferenceRenameService>();

            m_AliasRenamer = new AliasDefinitionRenamer(
                m_AliasWpfBindingFinder,
                MockRepository.GenerateMock<IUpdateAliasBindings>(),
                m_RenamerByCrossReferenceService.ToILazy(),
                MockRepository.GenerateMock<ICrossReferenceQueryService>().ToILazy());

            var targetService = MockRepository.GenerateMock<ITargetService>();
            var target = MockRepository.GenerateMock<ITarget>();
            target.Stub(inv => inv.Id).Return(TargetPlatform.Windows);
            targetService.Stub(inv => inv.CurrentTarget).Return(target);
            targetService.Stub(inv => inv.CurrentTargetInfo).Return(MockRepository.GenerateMock<ITargetInfo>());
            TestHelper.AddService<ITargetService>(targetService);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void RenameAliasNameIsUpdated()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            m_AliasWpfBindingFinder
                .Stub(x => x.FindAliasBindings(m_Elements, OldName))
                .Return(Enumerable.Empty<WpfBindingInfo>());

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            Assert.That(m_Alias.Name, Is.EqualTo(NewName));
        }

        [Test]
        public void RenameAliasInstancesAreUpdated()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            m_AliasWpfBindingFinder
                .Stub(x => x.FindAliasBindings(m_Elements, OldName))
                .Return(Enumerable.Empty<WpfBindingInfo>());
            AddAliasInstance();

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(NewName)));
        }

        [Test]
        public void RenameInstancesAreNotUpdatedIfAliasRenameFails()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            m_Alias.Site = new FakeSite(OldName);
            AddAliasInstance();

            // Act
            try
            {
                m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);
                Assert.Fail("Expected NotImplementedException thrown from FakeSite.Name");
            }
            catch (NotImplementedException) { }

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(OldName)));
        }

        [Test]
        public void RenameWhenNameShouldNotBeUpdatedVerifyNoUpdate()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(false);
            AddAliasInstance();

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(OldName)));
        }

        private void AddAliasInstance()
        {
            m_AliasInstances.Add(new AliasInstance() { Name = "inst1", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst2", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst3", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
        }
    }
}
