using System.Collections.Generic;
using System.Linq;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Tag;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.OpcClient.LookupIndex
{
    [TestFixture]
    public class TagPathLookupIndexTest
    {
        [Test]
        public void TestGetObjectWithPathReturnsValueFromDefaultLookupIndex()
        {
            // ARRANGE
            const string defaultProviderObject = "defaultProviderObject";
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            var defaultLookupIndex = MockRepository.GenerateStub<IPathLookupIndex>();
            defaultLookupIndex.Stub(i => i.GetObject<object>(Arg<string>.Is.Anything)).Return(defaultProviderObject);
            
            IPathLookupIndex unitUnderTest = new TagPathLookupIndex(globalController, defaultLookupIndex);

            // ACT
            bool same = unitUnderTest.GetObject<object>("path").Equals(defaultProviderObject);

            // ASSERT
            Assert.IsTrue(same);
        }

        [Test]
        public void TestRootNameIsFromDefaultLookupIndex()
        {
            // ARRANGE
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            var defaultLookupIndex = MockRepository.GenerateStub<IPathLookupIndex>();
            const string rootName = "Root";
            defaultLookupIndex.Stub(i => i.RootName).Return(rootName); 

            IPathLookupIndex unitUnderTest = new TagPathLookupIndex(globalController, defaultLookupIndex);

            // ACT
            bool same = unitUnderTest.RootName == rootName;
            
            // ASSERT
            Assert.IsTrue(same);
        }


        [Test]
        public void TestGetObjectWithPathReturnsValueFromBothIndexesWhenRealTagInterfaceIsRequested()
        {
            // ARRANGE
            IBasicTag defaultProviderObject = MockRepository.GenerateStub<IBasicTag>();
            defaultProviderObject.Name = "defaultTag";

            IBasicTag []defaultProviderObjects = { defaultProviderObject };
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            globalController.Stub(g => g.GetAllTags<IBasicTag>(Arg<TagsPredicate>.Is.Anything)).Return(CreateTags().ToArray());

            var defaultLookupIndex = MockRepository.GenerateStub<IPathLookupIndex>();
            defaultLookupIndex.Stub(l => l.GetObjects<IBasicTag>(Arg<bool>.Is.Anything)).Return(defaultProviderObjects.ToArray());

            IPathLookupIndex unitUnderTest = new TagPathLookupIndex(globalController, defaultLookupIndex);

            // ACT
            bool sameWhenNotIncludingMembers = SequenceEqual(unitUnderTest.GetObjects<IBasicTag>(false), defaultProviderObjects.ToArray());
            bool sameWhenIncludingMembers = SequenceEqual(unitUnderTest.GetObjects<IBasicTag>(true), defaultProviderObjects.Concat(CreateTags()).ToArray());

            // ASSERT
            Assert.IsTrue(sameWhenNotIncludingMembers);
            Assert.IsTrue(sameWhenIncludingMembers);
        }


        [Test]
        public void TestGetObjectWithPathReturnsValueFromBothIndexesWhenNoRealTagInterfaceIsRequested()
        {
            // ARRANGE
            ITag defaultProviderObject = MockRepository.GenerateStub<ITag>();
            defaultProviderObject.Name = "defaultTag";

            ITag[] defaultProviderObjects = { defaultProviderObject };
            var globalController = MockRepository.GenerateStub<IGlobalController>();
            globalController.Stub(g => g.GetAllTags<ITag>(Arg<TagsPredicate>.Is.Anything)).Return(CreateTags().ToArray());

            var defaultLookupIndex = MockRepository.GenerateStub<IPathLookupIndex>();
            defaultLookupIndex.Stub(l => l.GetObjects<ITag>(Arg<bool>.Is.Anything)).Return(defaultProviderObjects.ToArray());

            IPathLookupIndex unitUnderTest = new TagPathLookupIndex(globalController, defaultLookupIndex);

            // ACT
            bool sameWhenNotIncludingMembers = SequenceEqual(unitUnderTest.GetObjects<ITag>(false), defaultProviderObjects.ToArray());
            bool sameRegardlessMembersIncluded = SequenceEqual(unitUnderTest.GetObjects<ITag>(true), unitUnderTest.GetObjects<ITag>(false));
            
            // ASSERT
            Assert.IsTrue(sameWhenNotIncludingMembers);
            Assert.IsTrue(sameRegardlessMembersIncluded);
        }

        #region UTILITIES

        private static bool SequenceEqual(IEnumerable<IName> source, IEnumerable<IName> target) => source.Select(item => item.Name)
            .SequenceEqual(target.Select(item => item.Name));


        private static IEnumerable<IBasicTag> CreateTags()
        {
            for (int i = 0; i < 100; i++)
            {
                var tag = MockRepository.GenerateStub<IBasicTag>();
                tag.Name = $"tag${i}";
                yield return tag;
            }
        }
        #endregion
    }
}
