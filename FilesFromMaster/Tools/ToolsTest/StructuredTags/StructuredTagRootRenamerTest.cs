using System;
using System.Linq;
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Entities;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Services;
using Neo.ApplicationFramework.Interfaces.StructuredType.Services;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.StructuredTag.Misc;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using Neo.ApplicationFramework.Tools.StructuredType.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class StructuredTagRootRenamerTest
    {
        private StructuredTagRootRenamer m_StructuredTagRootRenamer;
        private IStructuredTagService m_StructuredTagService;
        private IMessageBoxServiceIde m_MessageBoxService;
        private ICrossReferenceRenameService m_RenamerByCrossReferenceService;
        private IStructuredTypeService m_StructuredTypeService;
        private ICrossReferenceService m_CrossReferenceService;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IDataItemCountingService>(new OpcClientToolIde());
            m_StructuredTagService = MockRepository.GenerateStub<IStructuredTagService>();
            m_RenamerByCrossReferenceService = MockRepository.GenerateMock<ICrossReferenceRenameService>();
            m_MessageBoxService = MockRepository.GenerateStub<IMessageBoxServiceIde>();
            m_StructuredTypeService = MockRepository.GenerateStub<IStructuredTypeService>();
            m_CrossReferenceService = MockRepository.GenerateStub<ICrossReferenceService>();
            var tagChangedNotificationService = MockRepository.GenerateStub<ITagChangedNotificationServiceCF>();
            TestHelper.AddService(m_StructuredTypeService);

            m_CrossReferenceService.Stub(x => x.GetReferences<ICrossReferenceItem>()).IgnoreArguments().Return(Enumerable.Empty<ICrossReferenceItem>());

            m_StructuredTagRootRenamer = new StructuredTagRootRenamer(
                m_RenamerByCrossReferenceService.ToILazy(),
                m_StructuredTagService.ToLazy(),
                m_MessageBoxService.ToILazy(), 
                m_CrossReferenceService.ToILazy(),
                tagChangedNotificationService.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [TestCase(typeof(StructuredTagInstance), true)]
        [TestCase(typeof(StructuredTypeReference), false)]
        [TestCase(typeof(object), false)]
        [TestCase(typeof(GlobalDataItem), false)]
        public void OnlyAllowRenamesOnStructuredTagInstances(Type typeToRename, bool canRename)
        {
            object objectToRename = Activator.CreateInstance(typeToRename);
            bool couldRename = m_StructuredTagRootRenamer.CanRename(objectToRename);
            Assert.IsTrue(couldRename == canRename);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CanOnlyRenameIfNewNameIsValid(bool isValidName)
        {
            IStructuredTagInstance structuredTagInstance = CreateStructuredTagInstance();
            m_StructuredTagService.Stub(x => x.IsValidNewName("", null, true)).IgnoreArguments().Return(isValidName);
            StubRenamerValidation();
            bool couldRename = m_StructuredTagRootRenamer.TryRename(structuredTagInstance, "someNewName");
            
            Assert.IsTrue(couldRename == isValidName);
        }

        [Test]
        public void LooksForCrossreferencesForAllChildren()
        {
            const int numberOfPossibleUpdates = 4;
            IStructuredTagInstance structuredTagInstance = CreateStructuredTagInstance();

            StubAllValidations();
            m_StructuredTagRootRenamer.TryRename(structuredTagInstance, "a");
            
            m_RenamerByCrossReferenceService.AssertWasCalled(x => x.UpdateNameByCrossReferences<ICrossReferenceItem>("", "", null), o => o.IgnoreArguments().Repeat.Times(numberOfPossibleUpdates));
        }

        [Test]
        public void RenamesStructuredTagAndMapping()
        {
            const string newName = "NewName";
            IStructuredTagInstance structuredTagInstance = CreateStructuredTagInstance();
            string oldName = structuredTagInstance.Name;

            StubAllValidations();
            m_StructuredTagRootRenamer.TryRename(structuredTagInstance, newName);

            Assert.AreEqual(structuredTagInstance.Name, newName);
            Assert.AreEqual(structuredTagInstance.InstanceMapping.Name, newName);
            Assert.AreNotEqual(structuredTagInstance.InstanceMapping.Name, oldName);
        }

        private void StubAllValidations()
        {
            StubNameValidation();
            StubRenamerValidation();
        }

        private void StubRenamerValidation()
        {
            m_RenamerByCrossReferenceService.Stub(x => x.NameShouldBeUpdated<ICrossReferenceItem>("")).IgnoreArguments().Return(true);
            m_RenamerByCrossReferenceService.Stub(x => x.UpdateNameByCrossReferences<ICrossReferenceItem>("", "", null)).IgnoreArguments().Return(true);
        }

        private void StubNameValidation()
        {
            m_StructuredTagService.Stub(x => x.IsValidNewName(Arg<string>.Is.Anything, Arg<INamingContext>.Is.Anything, Arg<bool>.Is.Anything)).Return(true);
        }

        private static IStructuredTagInstance CreateStructuredTagInstance()
        {
            IStructuredTagInstance structuredTagInstance = new StructuredTagInstance();
            structuredTagInstance.Name = "1";
            var level1Mapping = new StructuredTagInstanceMapping("1");
            level1Mapping.GlobalDataItemMappings.Add(new GlobalDataItemMapping("1.c1"));
            var level2Mapping = new StructuredTagInstanceMapping("1.2");
            level2Mapping.GlobalDataItemMappings.Add(new GlobalDataItemMapping("1.2.c1"));
            level1Mapping.StructuredTagMappings.Add(level2Mapping);
            structuredTagInstance.InstanceMapping = level1Mapping;
            return structuredTagInstance;
        }
    }
}
