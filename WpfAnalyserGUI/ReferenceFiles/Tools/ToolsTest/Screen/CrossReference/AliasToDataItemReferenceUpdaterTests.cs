#if !VNEXT_TARGET
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Alias.Entities;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.CrossReference;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasToDataItemReferenceUpdaterTests
    {
        private const string OldName = "OldTagName";
        private const string NewName = "NewTagName";
        private const string ProjectItemName = "Screen1";
        private const string PropertyName = "Value";

        private AliasDefinition m_AliasDefinition;
        private ScreenDesign.Screen m_Screen;

        [SetUp]
        public void SetUp()
        {
            m_AliasDefinition = new AliasDefinition { Name = PropertyName, DefaultValue = OldName };
            m_Screen = Substitute.For<ScreenDesign.Screen>();

            AliasDefinitions aliasDefinitions = new AliasDefinitions(null) { m_AliasDefinition };

            var projectItemFinder = Substitute.For<IProjectItemFinder>();
            var screenDesignerProjectItem = Substitute.For<IScreenDesignerProjectItem>();
            var designerHost = Substitute.For<INeoDesignerHost>();
            var aliasConfiguration = Substitute.For<IAliasConfiguration>();
            m_Screen.AliasConfiguration = aliasConfiguration;
            projectItemFinder.GetProjectItem(ProjectItemName).Returns(screenDesignerProjectItem);
            screenDesignerProjectItem.DesignerHost.Returns(designerHost);
            designerHost.RootComponent.Returns(m_Screen);
            aliasConfiguration.Definitions = aliasDefinitions;

            TestHelper.AddService<IProjectItemFinder>(projectItemFinder);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void Changing_the_default_value_updates_the_default_value_on_the_AliasDefinition()
        {
            var updater = new AliasToDataItemReferenceUpdater();
            IAliasDataItemReferenceItem referenceItem = new AliasDataItemReferenceItem { TargetFullName = ProjectItemName, TargetPropertyName = PropertyName };

            AliasInstance aliasInstance = new AliasInstance() { Name = AliasInstanceCF.DefaultInstanceName };

            updater.UpdateTargetReference(referenceItem, aliasInstance, OldName, NewName);

            Assert.That(m_AliasDefinition.DefaultValue, Is.EqualTo(NewName));
        }

        [Test]
        public void Changing_a_non_default_value_updates_the_alias_value_on_the_AliasInstance()
        {
            var updater = new AliasToDataItemReferenceUpdater();
            IAliasDataItemReferenceItem referenceItem = new AliasDataItemReferenceItem { TargetFullName = ProjectItemName, TargetPropertyName = PropertyName };

            AliasValue aliasValue = new AliasValue { DefaultValue = "Tag1", Name = PropertyName, Value = OldName };
            AliasInstance aliasInstance = new AliasInstance { Name = "Tank5", Values = new AliasValueList { aliasValue } };

            updater.UpdateTargetReference(referenceItem, aliasInstance, OldName, NewName);

            Assert.That(aliasValue.Value, Is.EqualTo(NewName));
        }
    }
}
#endif
