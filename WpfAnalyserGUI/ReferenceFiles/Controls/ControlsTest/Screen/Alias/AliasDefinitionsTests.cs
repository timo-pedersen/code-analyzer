#if !VNEXT_TARGET
using System.ComponentModel;
using Neo.ApplicationFramework.Common.Alias.Entities;
using Neo.ApplicationFramework.Controls.Screen.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Screen.Alias
{
    [TestFixture]
    public class AliasDefinitionsTests
    {
        [Test]
        public void NamePropertyDescriptorIsReplaced()
        {
            ITypedList aliasDefinitions = new AliasDefinitions(null);
            var propertyDescriptors = aliasDefinitions.GetItemProperties(null);

            Assert.That(propertyDescriptors["Name"], Is.TypeOf<AliasDefinitionNamePropertyDescriptor>());
        }

        [Test]
        public void CreateDefaultInstanceWithoutAnyAliasesCreatesADefaultInstanceWithoutAliases()
        {
            var aliasDefinitions = new AliasDefinitions(null);
            var defaultInstance = aliasDefinitions.CreateDefaultInstance();

            Assert.That(defaultInstance, Is.Not.Null);
            Assert.That(defaultInstance.Name, Is.EqualTo(AliasInstanceCF.DefaultInstanceName));
            Assert.That(defaultInstance.Values.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateDefaultInstanceWithAliases()
        {
            var aliasDefinitions = new AliasDefinitions(null);
            aliasDefinitions.Add(new AliasDefinition() { Name = "Level", DefaultValue = StringConstants.TagsRoot + "Level" });
            aliasDefinitions.Add(new AliasDefinition() { Name = "Temperature", DefaultValue = StringConstants.TagsRoot + "Temperature" });

            var defaultInstance = aliasDefinitions.CreateDefaultInstance();

            Assert.That(defaultInstance, Is.Not.Null);
            Assert.That(defaultInstance.Name, Is.EqualTo(AliasInstanceCF.DefaultInstanceName));
            Assert.That(defaultInstance.Values.Count, Is.EqualTo(aliasDefinitions.Count));

            for (int i = 0; i < aliasDefinitions.Count; i++)
            {
                Assert.That(defaultInstance.Values[i].IsDefault, Is.True);
                Assert.That(defaultInstance.Values[i].DefaultValue, Is.EqualTo(aliasDefinitions[i].DefaultValue));
                Assert.That(defaultInstance.Values[i].Value, Is.EqualTo(aliasDefinitions[i].DefaultValue));
            }
        }
    }
}
#endif
