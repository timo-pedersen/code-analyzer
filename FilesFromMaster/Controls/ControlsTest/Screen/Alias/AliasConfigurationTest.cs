using System;
using System.ComponentModel;
using System.Linq;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Screen.Alias
{
    [TestFixture]
    public class AliasConfigurationTest
    {
        private AliasConfiguration m_AliasConfiguration;
        private IComponent m_Component;
        private IContainer m_Container;
        private IGlobalReferenceService m_GlobalReferenceService;
        private AliasInstance m_FirstAliasInstance;
        private const string m_AnAliasDefinitionName = "AnAliasDefinition";
        private AliasDefinition m_AliasDefinition;

        [SetUp]
        public void SetUp()
        {
            m_Container = MockRepository.GenerateStub<IContainer>();
            m_Component = MockRepository.GenerateStub<IComponent>();
            
            m_AliasConfiguration = new AliasConfiguration(m_Component);
            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_FirstAliasInstance = new AliasInstance { Name = "I1" };
            m_AliasDefinition = new AliasDefinition { Name = m_AnAliasDefinitionName };
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetAllInstancesIncludesDefaultInstance()
        {
            var defaultInstance = new AliasInstance();
            var aliasDefinitions = MockRepository.GenerateStub<AliasDefinitions>(new object[] { m_Component });
            aliasDefinitions.Stub(ad => ad.CreateDefaultInstance()).Return(defaultInstance);
            m_AliasConfiguration.Definitions = aliasDefinitions;
            AliasInstance[] allInstances = m_AliasConfiguration.GetAllInstances();

            Assert.IsTrue(allInstances.Contains(defaultInstance));
        }

        [Test]
        public void SettingDefaultValueUpdatesDataType()
        {
            Type expectedType = typeof(int);
            BEDATATYPE sourceBEDATATYPE;
            BEDATATYPEConverter.TryConvertFromType(expectedType, out sourceBEDATATYPE);

            m_AliasDefinition.DataType = "No way I exist as a type"; // DataType has to set to prevent exeption to be thrown when being compared with fetched dataitem type
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);

            var globalDataItemBase = MockRepository.GenerateStub<IGlobalDataItemBase>();
            globalDataItemBase.Stub(di => di.GlobalDataTypeOrDataTypeIfDefault).Return(sourceBEDATATYPE);
            m_GlobalReferenceService.Stub(grs => grs.GetObject<IGlobalDataItemBase>("")).IgnoreArguments().Return(globalDataItemBase);
             
            m_AliasDefinition.DefaultValue = "Setting DefaultValue will trigger the update";

            Assert.IsTrue(m_AliasDefinition.DataType == expectedType.ToString());
        }

        [Test]
        public void InstanceValuesAddsValueWhenANewDefinitionIsAdded()
        {
            m_AliasConfiguration.Instances.Add(m_FirstAliasInstance);
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);

            Assert.IsTrue(m_FirstAliasInstance.Values.Any(value => value.Name == m_AnAliasDefinitionName));
        }

        [Test]
        public void InstanceValuesRemovesCorrespondingValueWhenADefinitionIsRemoved()
        {
            m_AliasConfiguration.Instances.Add(m_FirstAliasInstance);
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);
            m_AliasConfiguration.Definitions.Remove(m_AliasDefinition);

            Assert.IsFalse(m_FirstAliasInstance.Values.Any(value => value.Name == m_AnAliasDefinitionName));
        }

        [Test]
        public void InstanceValuesUpdatesCorrespondingValueWhenADefinitionIsRenamed()
        {
            const string renamedName = "Renamed";
            m_AliasConfiguration.Instances.Add(m_FirstAliasInstance);
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);
            m_AliasDefinition.Name = renamedName;

            Assert.IsTrue(m_FirstAliasInstance.Values.Any(value => value.Name == renamedName));
            Assert.IsFalse(m_FirstAliasInstance.Values.Any(value => value.Name == m_AnAliasDefinitionName));
        }

        [Test]
        public void AddingDefinitionRaisesAliasChanged()
        {
            bool eventRaised = false;
  
            m_AliasConfiguration.AliasChanged += (sender, e) => eventRaised = true;
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void RemovingDefinitionRaisesAliasChanged()
        {
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);
            bool eventRaised = false;
            m_AliasConfiguration.AliasChanged += (sender, e) => eventRaised = true;
            m_AliasConfiguration.Definitions.Remove(m_AliasDefinition);

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void ChangingADefinitionPropertyRaisesAliasChanged()
        {
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition);

            bool eventRaised = false;
            m_AliasConfiguration.AliasChanged += (sender, e) => eventRaised = true;
            m_AliasDefinition.DefaultValue = "Setting DefaultValue should raise an event";

            Assert.IsTrue(eventRaised);
        }

        [Test]
        public void WhenDefinitionDefaultValueChangesCorrespondingInstancesWithIsDefaultSetUpdates()
        {
            const string setOnFirstInstance = "A";
            const string setOnDefinition = "B";
            var secondAliasInstance = new AliasInstance {Name = "I2"};
            m_AliasConfiguration.Instances.Add(m_FirstAliasInstance);
            m_AliasConfiguration.Instances.Add(secondAliasInstance);
            m_AliasConfiguration.Definitions.Add(m_AliasDefinition); // this causes Instances to be populated with default definition values

            AliasValue aliasValueFromFirstInstance = m_FirstAliasInstance.Values.First(av => av.Name == m_AliasDefinition.Name);
            AliasValue aliasValueFromSecondInstance = secondAliasInstance.Values.First(av => av.Name == m_AliasDefinition.Name);
            aliasValueFromFirstInstance.Value = setOnFirstInstance; // now IsDefault should be false and this instance should not be affected when we update definition default value
            m_AliasDefinition.DefaultValue = setOnDefinition; // this should propagate to each instance values and update if IsDefault == true

            Assert.IsTrue(aliasValueFromFirstInstance.Value == setOnFirstInstance);
            Assert.IsTrue(aliasValueFromSecondInstance.Value == setOnDefinition);
        }

        

    }
}
