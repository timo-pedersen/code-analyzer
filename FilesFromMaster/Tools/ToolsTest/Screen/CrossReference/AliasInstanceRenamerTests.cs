using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.CrossReference;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasInstanceRenamer_Ctor_Tests
    {
        [Test]
        public void Missing_ICrossReferenceQueryService_dependency_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasInstanceRenamer(
                MockRepository.GenerateStub<ICrossReferenceRenameService>().ToILazy(),
                null));
        }

        [Test]
        public void Missing_IRenamerByCrossReferenceService_dependency_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasInstanceRenamer(
                null,
                MockRepository.GenerateStub<ICrossReferenceQueryService>().ToILazy()));
        }        
    }

    [TestFixture]
    public class AliasInstanceRenamer_CanRename_Tests
    {
        private AliasInstanceRenamer m_AliasInstanceRenamer;

        [SetUp]
        public void SetUp()
        {
            m_AliasInstanceRenamer = new AliasInstanceRenamer(                
                MockRepository.GenerateStub<ICrossReferenceRenameService>().ToILazy(),
                MockRepository.GenerateStub<ICrossReferenceQueryService>().ToILazy());
        }

        [Test]
        public void Return_false_if_designerHost_and_aliasInstance_are_null()
        {
            bool result = m_AliasInstanceRenamer.CanRename(null, null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Return_false_if_designerHost_is_null()
        {
            bool result = m_AliasInstanceRenamer.CanRename(null, new AliasInstance());

            Assert.That(result, Is.False);
        }

        [Test]
        public void Return_false_if_aliasInstance_is_null()
        {
            bool result = m_AliasInstanceRenamer.CanRename(MockRepository.GenerateStub<INeoDesignerHost>(), null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Return_true_if_designerHost_and_aliasInstance_are_both_not_null()
        {
            bool result = m_AliasInstanceRenamer.CanRename(MockRepository.GenerateStub<INeoDesignerHost>(), new AliasInstance());

            Assert.That(result, Is.True);
        }
    }

    [TestFixture]
    public class AliasInstanceRenamer_Rename_Tests
    {
        const string OldName = "OldName";
        const string NewName = "NewName";

        private AliasInstanceRenamer m_AliasInstanceRenamer;
        private INeoDesignerHost m_DesignerHost;
        private List<IActionCrossReferenceItem> m_ActionCrossReferences;
        private AliasInstance m_AliasInstance;
        private ICrossReferenceRenameService m_RenamerByCrossReferenceService;
        private ICrossReferenceQueryService m_CrossReferenceQueryService;

        [SetUp]
        public void SetUp()
        {
            const string screenName = "MyScreen";
            var actionCrossReferenceItem = MockRepository.GenerateStub<IActionCrossReferenceItem>();
            actionCrossReferenceItem.Stub(x => x.SourceFullName).Return(screenName);
            actionCrossReferenceItem.ActionParam = OldName;
            m_ActionCrossReferences = new List<IActionCrossReferenceItem> { actionCrossReferenceItem };
            m_AliasInstance = new AliasInstance { Name = OldName };

            m_DesignerHost = MockRepository.GenerateStub<INeoDesignerHost>();
            m_DesignerHost.Stub(host => host.RootComponentClassName).Return(screenName);

            m_RenamerByCrossReferenceService = MockRepository.GenerateStub<ICrossReferenceRenameService>();
            m_CrossReferenceQueryService = MockRepository.GenerateStub<ICrossReferenceQueryService>();
            m_AliasInstanceRenamer = new AliasInstanceRenamer(                                
                m_RenamerByCrossReferenceService.ToILazy(),
                m_CrossReferenceQueryService.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            m_RenamerByCrossReferenceService.VerifyAllExpectations();
        }

        [Test]
        public void RenameNoUpdateWhenUserClicksNo()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(false);
            GetReferencesReturns(m_ActionCrossReferences);

            m_RenamerByCrossReferenceService
                .Expect(x => x.UpdateNameByCrossReferences<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Repeat.Never();

            // Act
            m_AliasInstanceRenamer.Rename(m_DesignerHost, m_AliasInstance, OldName, NewName);

            // Assert
            Assert.That(m_AliasInstance.Name, Is.EqualTo(OldName));
        }

        [Test]
        public void RenameNameChangesButNoReferencesFound()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            GetReferencesReturns(Enumerable.Empty<IActionCrossReferenceItem>());

            // Act
            m_AliasInstanceRenamer.Rename(m_DesignerHost, m_AliasInstance, OldName, NewName);

            // Assert
            Assert.That(m_AliasInstance.Name, Is.EqualTo(NewName));
        }

        [Test]
        public void RenameReferencesAreUpdated()
        {
            // Arrange
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg<string>.Is.Anything, Arg<Func<string, IEnumerable<ICrossReferenceItem>>>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
            GetReferencesReturns(m_ActionCrossReferences);

            m_RenamerByCrossReferenceService
                .Expect(x => x.UpdateNameByCrossReferences<IActionCrossReferenceItem>(OldName, NewName, null, CrossReferenceTypes.Screen.ToString()))
                .Repeat.Once();

            // Act
            m_AliasInstanceRenamer.Rename(m_DesignerHost, m_AliasInstance, OldName, NewName);

            // Assert
            Assert.That(m_AliasInstance.Name, Is.EqualTo(NewName));
        }

        private void GetReferencesReturns(IEnumerable<IActionCrossReferenceItem> referenceItems)
        {
            m_CrossReferenceQueryService.Stub(x => x.GetScreenAliasInstanceReferences()).Return(referenceItems);
        }
    }
}
