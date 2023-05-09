using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api.CrossReference;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.Tools.StructuredTag.CodeDOM;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    public class LightweightTagCreatorTest
    {
        private Lazy<ICrossReferenceQueryService> m_CrossReferenceQueryService;
        private LightweightTagCreator m_LightweightTagCreator;

        private const int Interval = 500;
        private static readonly string TagUsedInScriptFullName = StringConstants.TagsRoot + "TagUsedInScript";

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var crossReferenceItem = MockRepository.GenerateStub<ICrossReferenceItem>();
            crossReferenceItem.Stub(x => x.SourceFullName).Return(TagUsedInScriptFullName);

            var crossReferenceQueryService = MockRepository.GenerateStub<ICrossReferenceQueryService>();
            crossReferenceQueryService.Stub(x => x.GetReferences<ICrossReferenceItem>(CrossReferenceTypes.Script.ToString())).Return(new[] { crossReferenceItem });

            m_CrossReferenceQueryService = new Lazy<ICrossReferenceQueryService>(() => crossReferenceQueryService);

            m_LightweightTagCreator = new LightweightTagCreator(m_CrossReferenceQueryService);
        }

        [Test]
        public void PropertiesCopiedCorrectly()
        {
            // ARRANGE
            var dataItem = MockRepository.GenerateStub<IDataItem>();
            dataItem.Stub(x => x.FullName).Return("Controller1.DataItem1");

            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.Name = "Tag1";
            globalDataItem.AlwaysActive = true;
            globalDataItem.Description = "description";
            globalDataItem.GlobalDataType = BEDATATYPE.DT_DEFAULT;
            globalDataItem.Stub(x => x.GlobalDataTypeOrDataTypeIfDefault).Return(BEDATATYPE.DT_INTEGER4);

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNotNull(lightweightTag);
            Assert.That(lightweightTag.DataItemFullName, Is.EqualTo(dataItem.FullName));
            Assert.That(lightweightTag.Name, Is.EqualTo(globalDataItem.Name));
            Assert.That(((IBasicTag)lightweightTag).UpdateRate, Is.EqualTo(globalDataItem.PollGroup.Interval));
            Assert.That(((IBasicTag)lightweightTag).GlobalDataTypeOrDataTypeIfDefault, Is.EqualTo(globalDataItem.GlobalDataTypeOrDataTypeIfDefault));
            Assert.That(((IBasicTag)lightweightTag).AlwaysActive, Is.EqualTo(globalDataItem.AlwaysActive));
            Assert.That(((IBasicTag)lightweightTag).Description, Is.EqualTo(globalDataItem.Description));
        }

        [Test]
        public void TagWithGainIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.Gain = 2.0;

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagWithOffsetIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.Gain = 10.0;

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        [TestCase(AccessRights.Read)]
        [TestCase(AccessRights.Write)]
        public void NonReadWriteTagsAreNotQualified(AccessRights accessRights)
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.AccessRight = accessRights;

            // ACT
            LightweightTag lightweightTag = new LightweightTagCreator(m_CrossReferenceQueryService).CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagWithReadExpressionIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.ReadExpression = "AnExpression";

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagWithWriteExpressionIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.WriteExpression = "AnExpression";

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagWithIndexRegisterIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.IndexRegisterNumber = 2;

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void NonVolatileTagIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.NonVolatile = true;

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagThatLogsToAuditTrailIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.LogToAuditTrail = true;

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagConnectedToMultipleControllersIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.DataItems.Add(MockRepository.GenerateStub<IDataItem>());
            globalDataItem.DataItems.Add(MockRepository.GenerateStub<IDataItem>());

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagUsedInScriptIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.Name = TagUsedInScriptFullName.Split('.').Last();

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void ArrayTagIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            globalDataItem.Stub(x => x.IsArrayTag).Return(true);

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void SystemTagIsNotQualified()
        {
            // ARRANGE
            var systemTag = MockRepository.GenerateStub<ISystemDataItem>();

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(systemTag);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        [Test]
        public void TagWithConfiguredDataExchangeDirectionsIsNotQualified()
        {
            // ARRANGE
            var globalDataItem = GetGlobalDataItemStub();
            var accessRights = new Dictionary<string, AccessRights>();
            accessRights.Add("Controller1", AccessRights.Read);
            globalDataItem.Stub(x => x.AccessRights).Return(accessRights);

            // ACT
            LightweightTag lightweightTag = m_LightweightTagCreator.CreateLightweightTag(globalDataItem);

            // ASSERT
            Assert.IsNull(lightweightTag);
        }

        private IGlobalDataItem GetGlobalDataItemStub()
        {
            var globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            globalDataItem.Stub(x => x.DataItems).Return(new List<IDataItem>());
            globalDataItem.Gain = 1.0;
            globalDataItem.AccessRight = AccessRights.ReadWrite;
            globalDataItem.ReadExpression = string.Empty;
            globalDataItem.WriteExpression = string.Empty;

            var pollGroup = MockRepository.GenerateStub<IPollGroup>();
            pollGroup.Interval = Interval;
            globalDataItem.PollGroup = pollGroup;

            return globalDataItem;
        }
    }
}
