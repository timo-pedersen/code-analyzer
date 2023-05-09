using System;
using System.Collections.Generic;
using System.Linq;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.StructuredTag.Services;
using Neo.ApplicationFramework.Interfaces.StructuredType;
using Neo.ApplicationFramework.Interfaces.StructuredType.Services;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using Neo.ApplicationFramework.Tools.StructuredTag.Model;
using Neo.ApplicationFramework.Tools.StructuredTag.Services;
using Neo.ApplicationFramework.Tools.StructuredTags.Common;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.StructuredTags
{
    [TestFixture]
    public class StructuredTagServiceTests
    {
        private IStructuredTagService m_StructuredTagService;
        private IStructuredTypeService m_StructuredTypeService;
        private IOpcClientServiceIde m_OpcClientServiceIde;
        private GlobalController m_GlobalController;
        private const string TestType = "TestType";

        [SetUp]
        public void Setup()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.CreateAndAddServiceStub<IDataItemCountingService>();

            var controllers = new ExtendedBindingList<IDataSourceContainer>();
            m_OpcClientServiceIde = MockRepository.GenerateStub<IOpcClientServiceIde>();
            m_OpcClientServiceIde.Stub(inv => inv.Controllers).Return(controllers);
            TestHelper.AddService<IOpcClientServiceCF>(m_OpcClientServiceIde);
            TestHelper.AddService<IOpcClientServiceIde>(m_OpcClientServiceIde);

            m_StructuredTypeService = StructuredTagsTestBase.CreateStructuredTagServiceStub();

            m_StructuredTagService = new StructuredTagService(
                MockRepository.GenerateStub<INameCreationService>(),
                m_StructuredTypeService,
                m_OpcClientServiceIde
            );
            m_GlobalController = new GlobalController();
            m_OpcClientServiceIde.Stub(inv => inv.GlobalController).Return(m_GlobalController);

            m_StructuredTypeService.Stub(
               inv =>
                   inv.FlattenHierarchy(
                       Arg<IStructuredTypeReference>.Is.Anything,
                       Arg<Func<IStructuredTypeRootComponent, IStructuredTypeReference, ITag, string, string>>.Is.Anything,
                       Arg<bool>.Is.Anything,
                       Arg<string>.Is.Anything)
                   )
                   .WhenCalled(inv =>
                   {
                       IStructuredTypeReference r = (IStructuredTypeReference)inv.Arguments[0];
                       inv.ReturnValue = r.TypeName == TestType ? new[] {TestType} : Enumerable.Empty<string>();
                   }).Return(default(IEnumerable<string>));
        }

        [TearDown]
        public void Cleanup()
        {
            m_GlobalController.Dispose();
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
        }

        [Test]
        public void TestIsTypeUsedByAnyTagWhenTypeIsUsed()
        {
            m_OpcClientServiceIde.GlobalController.GlobalStructuredTags.Add(new StructuredTagInstance("Test", TestType, null));

            bool result = m_StructuredTagService.IsTypeUsedByAnyTag(TestType);
            Assert.IsTrue(result);
        }

        [Test]
        public void TestIsTypeUsedByAnyTagWhenTypeIsNotUsed()
        {
            m_OpcClientServiceIde.GlobalController.GlobalStructuredTags.Add(new StructuredTagInstance("Test", "AnotherType", null));

            bool result = m_StructuredTagService.IsTypeUsedByAnyTag(TestType);
            Assert.IsFalse(result);
        }

    }
}
