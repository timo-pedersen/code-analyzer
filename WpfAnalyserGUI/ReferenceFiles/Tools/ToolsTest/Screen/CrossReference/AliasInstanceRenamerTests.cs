#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.CrossReference;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasInstanceRenamer_Ctor_Tests
    {
        [Test]
        public void Missing_ICrossReferenceQueryService_dependency_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasInstanceRenamer(
                Substitute.For<ICrossReferenceRenameService>().ToILazy(),
                null));
        }

        [Test]
        public void Missing_IRenamerByCrossReferenceService_dependency_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasInstanceRenamer(
                null,
                Substitute.For<ICrossReferenceQueryService>().ToILazy()));
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
                Substitute.For<ICrossReferenceRenameService>().ToILazy(),
                Substitute.For<ICrossReferenceQueryService>().ToILazy());
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
            bool result = m_AliasInstanceRenamer.CanRename(Substitute.For<INeoDesignerHost>(), null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Return_true_if_designerHost_and_aliasInstance_are_both_not_null()
        {
            bool result = m_AliasInstanceRenamer.CanRename(Substitute.For<INeoDesignerHost>(), new AliasInstance());

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
            var actionCrossReferenceItem = Substitute.For<IActionCrossReferenceItem>();
            actionCrossReferenceItem.SourceFullName.Returns(screenName);
            actionCrossReferenceItem.ActionParam = OldName;
            m_ActionCrossReferences = new List<IActionCrossReferenceItem> { actionCrossReferenceItem };
            m_AliasInstance = new AliasInstance { Name = OldName };

            m_DesignerHost = Substitute.For<INeoDesignerHost>();
            m_DesignerHost.RootComponentClassName.Returns(screenName);

            m_RenamerByCrossReferenceService = Substitute.For<ICrossReferenceRenameService>();
            m_CrossReferenceQueryService = Substitute.For<ICrossReferenceQueryService>();
            m_AliasInstanceRenamer = new AliasInstanceRenamer(
                m_RenamerByCrossReferenceService.ToILazy(),
                m_CrossReferenceQueryService.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void RenameNoUpdateWhenUserClicksNo()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), 
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>())
                .Returns(false);
            GetReferencesReturns(m_ActionCrossReferences);

            // Act
            m_AliasInstanceRenamer.Rename(m_DesignerHost, m_AliasInstance, OldName, NewName);

            // Assert
            Assert.That(m_AliasInstance.Name, Is.EqualTo(OldName));
            m_RenamerByCrossReferenceService.DidNotReceiveWithAnyArgs()
                .UpdateNameByCrossReferences<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>());
        }

        [Test]
        public void RenameNameChangesButNoReferencesFound()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
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
            m_RenamerByCrossReferenceService
                .NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>())
                .Returns(true);
            GetReferencesReturns(m_ActionCrossReferences);

            // Act
            m_AliasInstanceRenamer.Rename(m_DesignerHost, m_AliasInstance, OldName, NewName);

            // Assert
            Assert.That(m_AliasInstance.Name, Is.EqualTo(NewName));

            m_RenamerByCrossReferenceService.Received(1)
                .UpdateNameByCrossReferences<IActionCrossReferenceItem>(OldName, NewName, 
                    Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), CrossReferenceTypes.Screen.ToString());
        }

        private void GetReferencesReturns(IEnumerable<IActionCrossReferenceItem> referenceItems)
        {
            m_CrossReferenceQueryService.GetScreenAliasInstanceReferences().Returns(referenceItems);
        }
    }
}
#endif
