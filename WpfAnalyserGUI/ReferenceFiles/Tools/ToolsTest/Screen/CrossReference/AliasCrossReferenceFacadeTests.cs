#if !VNEXT_TARGET
using System;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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
                Substitute.For<IUpdateAliasReferencesToDataItems>()));
        }

        [Test]
        public void MissingDataItemReferenceUpdaterThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AliasCrossReferenceFacade(
                Substitute.For<IRenameAliasInstances>(),
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
            m_AliasInstanceRenamer = Substitute.For<IRenameAliasInstances>();

            m_AliasCrossReferenceFacade = new AliasCrossReferenceFacade(
                m_AliasInstanceRenamer,
                Substitute.For<IUpdateAliasReferencesToDataItems>());
        }

        [Test]
        public void Return_false_if_CanRename_returns_false()
        {
            m_AliasInstanceRenamer.CanRename(Arg.Any<INeoDesignerHost>(), Arg.Any<AliasInstance>()).Returns(false);

            bool result = m_AliasCrossReferenceFacade.TryRenameAliasInstance(null, null, null, null);

            Assert.That(result, Is.False);
            m_AliasInstanceRenamer.DidNotReceiveWithAnyArgs()
                .Rename(Arg.Any<INeoDesignerHost>(), Arg.Any<AliasInstance>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Calls_Rename_and_returns_true_if_CanRename_returns_true()
        {
            m_AliasInstanceRenamer.CanRename(Arg.Any<INeoDesignerHost>(), Arg.Any<AliasInstance>()).Returns(true);

            bool result = m_AliasCrossReferenceFacade.TryRenameAliasInstance(null, null, null, null);

            Assert.That(result, Is.True);
            m_AliasInstanceRenamer.ReceivedWithAnyArgs(1)
                .Rename(Arg.Any<INeoDesignerHost>(), Arg.Any<AliasInstance>(), Arg.Any<string>(), Arg.Any<string>());
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
            m_Updater = Substitute.For<IUpdateAliasReferencesToDataItems>();

            m_AliasCrossReferenceFacade = new AliasCrossReferenceFacade(
                Substitute.For<IRenameAliasInstances>(),
                m_Updater);
        }

        [Test]
        public void Route_to_UpdateDataItemReferences_on_IAliasDataItemReferenceItem_dependency()
        {
            var referenceItem = Substitute.For<IAliasDataItemReferenceItem>();
            AliasInstance aliasInstance = new AliasInstance();
            const string oldName = "OldName";
            const string newName = "NewName";

            m_AliasCrossReferenceFacade.UpdateDataItemReferences(referenceItem, aliasInstance, oldName, newName);

            m_Updater.ReceivedWithAnyArgs(1)
                .UpdateTargetReference(referenceItem, aliasInstance, oldName, newName);
        }
    }
}
#endif
