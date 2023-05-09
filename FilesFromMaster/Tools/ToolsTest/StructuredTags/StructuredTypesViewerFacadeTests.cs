using System;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Services;
using Neo.ApplicationFramework.Interfaces.StructuredType;
using Neo.ApplicationFramework.Interfaces.StructuredType.Services;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUa;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer;
using Neo.ApplicationFramework.Tools.StructuredTag.Facades.StructuredTypesViewer.Private;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class StructuredTypesViewerFacadeTests
    {
        private IStructuredTypesViewerFacade m_StructuredTypesViewerFacade;
        private IStructuredTypeService m_StructuredTypeService;
        private IStructuredTagService m_StructuredTagService;

        private const string TestType = "TestType";
        private const int NumberOfTypes = 10;

        [SetUp]
        public void SetUp()
        {
            m_StructuredTypeService = TestHelper.CreateAndAddServiceStub<IStructuredTypeService>();
            m_StructuredTypeService.Stub(inv => inv.GetAllTypes()).Return(GetMockedStructuredTypes());
            m_StructuredTagService = TestHelper.CreateAndAddServiceStub<IStructuredTagService>();


            m_StructuredTypesViewerFacade = new StructuredTypesViewerFacade(m_StructuredTypeService, m_StructuredTagService);

        }

        [TearDown]
        public void Cleanup()
        {
            TestHelper.ClearServices();
        }

        private static IEnumerable<IStructuredTypeRootComponent> GetMockedStructuredTypes()
        {
            List<IStructuredTypeRootComponent> list = new List<IStructuredTypeRootComponent>();

            for (int i = 0; i < NumberOfTypes; i++)
            {
                var type = MockRepository.GenerateStub<IStructuredTypeRootComponent>();
                type.Name = "typeWithName" + i;
                type.ControllerName = "Ctrl1";
                type.AddressDescriptor = new OpcUaStringNodeID("namespace", "identifier" + i, "browseName" + i);
                type.Stub(inv => inv.Members).Return(Enumerable.Empty<ITag>());
                list.Add(type);
            }
            return list;
        }


        [Test]
        public void TestGetAllTypes()
        {
            var types = m_StructuredTypesViewerFacade.GetTypes().ToArray();
            Assert.IsTrue(types.Count() == NumberOfTypes);
            for (int i = 0; i < types.Length; i++)
            {
                Assert.IsTrue(types[i].Name == "typeWithName" + i);
            }
        }

        [Test]
        public void TestGetType()
        {
            var typeEntity = m_StructuredTypesViewerFacade.GetTypes().First();
            var structType = GetMockedStructuredTypes().First();
            Assert.IsTrue(typeEntity.Name == structType.Name);
            Assert.IsTrue(typeEntity.ControllerName == structType.ControllerName);
            Assert.IsTrue(typeEntity.UniqueIdentifier == structType.AddressDescriptor.UniqueIdentifier);
        }


        [Test]
        public void TestIsTypeDeletableWhenTypeIsUsedByAnyTag()
        {
            m_StructuredTagService.Stub(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.Equal(TestType))).Return(true);
            m_StructuredTagService.Stub(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.NotEqual(TestType))).Return(false);
            bool result = m_StructuredTypesViewerFacade.IsTypeDeletable(TestType);
            m_StructuredTagService.AssertWasCalled(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.Equal(TestType)));
            Assert.IsFalse(result);
        }

        [Test]
        public void TestIsTypeDeletableWhenTypeIsUsedByNoTag()
        {
            m_StructuredTagService.Stub(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.Equal(TestType))).Return(false);
            m_StructuredTagService.Stub(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.NotEqual(TestType))).Return(true);
            bool result = m_StructuredTypesViewerFacade.IsTypeDeletable(TestType);
            m_StructuredTagService.AssertWasCalled(inv => inv.IsTypeUsedByAnyTag(Arg<string>.Is.Equal(TestType)));
            Assert.IsTrue(result);
        }

        [Test]
        public void TestDeleteType()
        {
            m_StructuredTypesViewerFacade.DeleteType(TestType);
            m_StructuredTypeService.AssertWasCalled(inv => inv.DeleteTypes(Arg<IEnumerable<string>>.Is.Anything));
        }


        [Test]
        public void TestTypesChangedRegistration()
        {
            m_StructuredTypesViewerFacade.TypesChanged += StructuredTypesViewerFacadeOnTypesChanged;
            m_StructuredTypeService.AssertWasCalled(inv => inv.TypesChanged += Arg<EventHandler>.Is.Anything);
        }

        [Test]
        public void TestTypesChangedDeregistration()
        {
            m_StructuredTypesViewerFacade.TypesChanged -= StructuredTypesViewerFacadeOnTypesChanged;
            m_StructuredTypeService.AssertWasCalled(inv => inv.TypesChanged -= Arg<EventHandler>.Is.Anything);
        }


        private void StructuredTypesViewerFacadeOnTypesChanged(object sender, EventArgs eventArgs) { }
    }
}
