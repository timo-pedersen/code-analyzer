using System;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    // AliasCrossReferenceFacade is a facade class that merely contain routing, which
    // means that these tests are of an integration nature.
    [TestFixture]
    public class AliasCrossReferenceFacade_Ctor_Tests
    {
        [Test]
        public void MissingAliasInstanceRenamerThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasCrossReferenceFacade(
                null,
                MockRepository.GenerateStub<IUpdateAliasReferencesToDataItems>()));
        }

        [Test]
        public void MissingDataItemReferenceUpdaterThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasCrossReferenceFacade(
                MockRepository.GenerateStub<IRenameAliasInstances>(),
                null));
        }
    }

    [TestFixture]
    public class AliasCrossReferenceFacade_TryRenameAliasInstance_Tests
    {
        private IRenameAliasInstances m_AliasInstanceRenamer;
        private AliasCrossReferenceFacade m_AliasCrossReferenceFacade;

        [SetUp]
        public void SetUp()
        {
            m_AliasInstanceRenamer = MockRepository.GenerateMock<IRenameAliasInstances>();

            m_AliasCrossReferenceFacade = new AliasCrossReferenceFacade(
                m_AliasInstanceRenamer,
                MockRepository.GenerateStub<IUpdateAliasReferencesToDataItems>());
        }

        [Test]
        public void Return_false_if_CanRename_returns_false()
        {
            m_AliasInstanceRenamer.Stub(renamer => renamer.CanRename(null, null)).IgnoreArguments().Return(false);
            m_AliasInstanceRenamer.Expect(renamer => renamer.Rename(null, null, null, null)).IgnoreArguments().Repeat.Never();

            bool result = m_AliasCrossReferenceFacade.TryRenameAliasInstance(null, null, null, null);

            Assert.That(result, Is.False);
            m_AliasInstanceRenamer.VerifyAllExpectations();
        }

        [Test]
        public void Calls_Rename_and_returns_true_if_CanRename_returns_true()
        {
            m_AliasInstanceRenamer.Stub(renamer => renamer.CanRename(null, null)).IgnoreArguments().Return(true);
            m_AliasInstanceRenamer.Expect(renamer => renamer.Rename(null, null, null, null)).IgnoreArguments().Repeat.Once();

            bool result = m_AliasCrossReferenceFacade.TryRenameAliasInstance(null, null, null, null);

            Assert.That(result, Is.True);
            m_AliasInstanceRenamer.VerifyAllExpectations();
        }
    }

    [TestFixture]
    public class AliasCrossReferenceFacade_UpdateDataItemReferences_Tests
    {
        private IUpdateAliasReferencesToDataItems m_Updater;
        private AliasCrossReferenceFacade m_AliasCrossReferenceFacade;

        [SetUp]
        public void SetUp()
        {
            m_Updater = MockRepository.GenerateMock<IUpdateAliasReferencesToDataItems>();

            m_AliasCrossReferenceFacade = new AliasCrossReferenceFacade(
                MockRepository.GenerateStub<IRenameAliasInstances>(),
                m_Updater);
        }

        [Test]
        public void Route_to_UpdateDataItemReferences_on_IAliasDataItemReferenceItem_dependency()
        {
            var referenceItem = MockRepository.GenerateStub<IAliasDataItemReferenceItem>();
            AliasInstance aliasInstance = new AliasInstance();
            const string oldName = "OldName";
            const string newName = "NewName";

            m_Updater.Expect(renamer => renamer.UpdateTargetReference(referenceItem, aliasInstance, oldName, newName)).Repeat.Once();

            m_AliasCrossReferenceFacade.UpdateDataItemReferences(referenceItem, aliasInstance, oldName, newName);

            m_Updater.VerifyAllExpectations();
        }
    }
}
