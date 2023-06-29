using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using NUnit.Framework;
using NSubstitute;
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
            IDesignerMetadata designerMetadataStub = Substitute.For<IDesignerMetadata>();
            m_DesignerInfoStub = Substitute.For<IDesignerInfo>();
            m_DesignerInfoStub.Metadata.Returns(designerMetadataStub);

            m_ImportProductSettingsStub = Substitute.For<IImportProductSettings>();
            m_FileSettingsServiceIde = Substitute.For<IFileSettingsServiceIde>();
            m_FileSettingsServiceIde.CommonApplicationDataFolder.Returns(".");
            ServiceContainerCF.Instance.AddService(typeof(IFileSettingsServiceIde), m_FileSettingsServiceIde);

            var gapService = Substitute.For<IGapService>();
            gapService.IsSubjectConsideredGap(Arg.Any<Type>()).Returns(false);
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
            var typeListService = (ITypeListService)Substitute.For<TypeListService>(m_ImportProductSettingsStub);
            typeListService.GetDesignerInfo(Arg.Any<Type>()).Returns(m_DesignerInfoStub);

            // ACT, ASSERT
            Assert.IsTrue(typeListService.IsDesignerTypeAddable(type));
        }

        [Test]
        [TestCase(typeof(Reporting.Reports))]
        [TestCase(typeof(AlarmDistributorServer.AlarmDistributorServer))]
        public void IsDesignerTypeAddable_IsNotAddable_ReturnsFalse(Type type)
        {
            // ARRANGE
            var typeListService = (ITypeListService)Substitute.For<TypeListService>(m_ImportProductSettingsStub);
            typeListService.GetDesignerInfo(Arg.Any<Type>()).Returns(m_DesignerInfoStub);

            // ACT
            typeListService.RegisterNonAddableDesignerType(typeof(Reporting.Reports));
            typeListService.RegisterNonAddableDesignerType(typeof(AlarmDistributorServer.AlarmDistributorServer));

            // ASSERT
            Assert.IsFalse(typeListService.IsDesignerTypeAddable(type));
        }
    }
}
