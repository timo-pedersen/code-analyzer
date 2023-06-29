using System.Collections.Generic;
using System.Linq;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Tag;
using NUnit.Framework;
using NSubstitute;

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
            var globalController = Substitute.For<IGlobalController>();
            var defaultLookupIndex = Substitute.For<IPathLookupIndex>();
            defaultLookupIndex.GetObject<object>(Arg.Any<string>()).Returns(defaultProviderObject);
            
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
            var globalController = Substitute.For<IGlobalController>();
            var defaultLookupIndex = Substitute.For<IPathLookupIndex>();
            const string rootName = "Root";
            defaultLookupIndex.RootName.Returns(rootName); 

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
            IBasicTag defaultProviderObject = Substitute.For<IBasicTag>();
            defaultProviderObject.Name = "defaultTag";

            IBasicTag []defaultProviderObjects = { defaultProviderObject };
            var globalController = Substitute.For<IGlobalController>();
            globalController.GetAllTags<IBasicTag>(Arg.Any<TagsPredicate>()).Returns(CreateTags().ToArray());

            var defaultLookupIndex = Substitute.For<IPathLookupIndex>();
            defaultLookupIndex.GetObjects<IBasicTag>(Arg.Any<bool>()).Returns(defaultProviderObjects.ToArray());

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
            ITag defaultProviderObject = Substitute.For<ITag>();
            defaultProviderObject.Name = "defaultTag";

            ITag[] defaultProviderObjects = { defaultProviderObject };
            var globalController = Substitute.For<IGlobalController>();
            globalController.GetAllTags<ITag>(Arg.Any<TagsPredicate>()).Returns(CreateTags().ToArray());

            var defaultLookupIndex = Substitute.For<IPathLookupIndex>();
            defaultLookupIndex.GetObjects<ITag>(Arg.Any<bool>()).Returns(defaultProviderObjects.ToArray());

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
                var tag = Substitute.For<IBasicTag>();
                tag.Name = $"tag${i}";
                yield return tag;
            }
        }
        #endregion
    }
}
