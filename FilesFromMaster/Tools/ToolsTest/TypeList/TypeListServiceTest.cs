using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using NUnit.Framework;
using Rhino.Mocks;
using System;

namespace Neo.ApplicationFramework.Tools.TypeList
{
    [TestFixture]
    sealed class TypeListServiceTest
    {
        private IImportProductSettings m_ImportProductSettingsStub;
        private IFileSettingsServiceIde m_FileSettingsServiceIde;
        private IDesignerInfo m_DesignerInfoStub;

        [SetUp]
        public void Setup()
        {
            IDesignerMetadata designerMetadataStub = MockRepository.GenerateStub<IDesignerMetadata>();
            m_DesignerInfoStub = MockRepository.GenerateStub<IDesignerInfo>();
            m_DesignerInfoStub.Stub(x => x.Metadata).Return(designerMetadataStub);

            m_ImportProductSettingsStub = MockRepository.GenerateStub<IImportProductSettings>();
            m_FileSettingsServiceIde = MockRepository.GenerateMock<IFileSettingsServiceIde>();
            m_FileSettingsServiceIde.Expect(i => i.CommonApplicationDataFolder).Return(".");
            ServiceContainerCF.Instance.AddService(typeof(IFileSettingsServiceIde), m_FileSettingsServiceIde);

            var gapService = MockRepository.GenerateStub<IGapService>();
            gapService.Stub(x => x.IsSubjectConsideredGap(Arg<Type>.Is.Anything)).Return(false);
            ServiceContainerCF.Instance.AddService<IGapService>(gapService);
        }

        [Test]
        [TestCase(typeof(Scheduler.Scheduler))]
        [TestCase(typeof(Recipe.Recipe))]
        [TestCase(typeof(Reporting.Reports))]
        [TestCase(typeof(AlarmDistributorServer.AlarmDistributorServer))]
        public void IsDesignerTypeAddable_IsAddable_ReturnsTrue(Type type)
        {
            // ARRANGE
            var typeListService = (ITypeListService)MockRepository.GeneratePartialMock<TypeListService>(m_ImportProductSettingsStub);
            typeListService.Stub(x => x.GetDesignerInfo(Arg<Type>.Is.Anything)).Return(m_DesignerInfoStub);

            // ACT, ASSERT
            Assert.IsTrue(typeListService.IsDesignerTypeAddable(type));
        }

        [Test]
        [TestCase(typeof(Reporting.Reports))]
        [TestCase(typeof(AlarmDistributorServer.AlarmDistributorServer))]
        public void IsDesignerTypeAddable_IsNotAddable_ReturnsFalse(Type type)
        {
            // ARRANGE
            var typeListService = (ITypeListService)MockRepository.GeneratePartialMock<TypeListService>(m_ImportProductSettingsStub);
            typeListService.Stub(x => x.GetDesignerInfo(Arg<Type>.Is.Anything)).Return(m_DesignerInfoStub);

            // ACT
            typeListService.RegisterNonAddableDesignerType(typeof(Reporting.Reports));
            typeListService.RegisterNonAddableDesignerType(typeof(AlarmDistributorServer.AlarmDistributorServer));

            // ASSERT
            Assert.IsFalse(typeListService.IsDesignerTypeAddable(type));
        }
    }
}
