using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Expressions.Validation;
using Neo.ApplicationFramework.Tools.Script.Validation;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    public class CyclicExpressionValidatorTest
    {

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<IErrorListService>();
        }
        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        [TestCase(new[] { "1", "2", "3" }, new[] { "1", "2", "3" }, new[] { "2", "3", "1" }, true)]
        [TestCase(new[] { "1", "2", "3" }, new[] { "1", "2", "3" }, new[] { "1", "2", "3" }, true)]
        [TestCase(new[] { "1", "2", "3" }, new[] { "1", "2", "3" }, new[] { "", "", "" }, false)]
        public void DetectsCircularitiesTest(string[] tags, string[] expressions, string[] tagsInExpressions, bool shouldHaveCyclic)
        {
            var v = MockRepository.GenerateStub<IOpcClientServiceIde>();
            var tise = MockRepository.GenerateStub<ITagsInScriptExtractor>();
            IGlobalDataItem t1 = MockRepository.GenerateMock<IGlobalDataItem>();
            IGlobalDataItem t2 = MockRepository.GenerateMock<IGlobalDataItem>();
            IGlobalDataItem t3 = MockRepository.GenerateMock<IGlobalDataItem>();
            IGlobalDataItem t4 = MockRepository.GenerateMock<IGlobalDataItem>();

            t1.Stub(t => t.FullName).Return(StringConstants.TagsRoot + tags[0]);
            t2.Stub(t => t.FullName).Return(StringConstants.TagsRoot + tags[1]);
            t3.Stub(t => t.FullName).Return(StringConstants.TagsRoot + tags[2]);
            t4.Stub(t => t.FullName).Return(StringConstants.TagsRoot);
            t1.Stub(t => t.ReadExpression).Return("ExpRefTag" + expressions[0]);
            t2.Stub(t => t.ReadExpression).Return("ExpRefTag" + expressions[1]);
            t3.Stub(t => t.ReadExpression).Return("ExpRefTag" + expressions[2]);

            tise.Stub(x => x.ExtractReferencedTags("ExpRefTag" + expressions[0])).Return(new[] { StringConstants.TagsRoot + tagsInExpressions[0] });
            tise.Stub(x => x.ExtractReferencedTags("ExpRefTag" + expressions[1])).Return(new[] { StringConstants.TagsRoot + tagsInExpressions[1] });
            tise.Stub(x => x.ExtractReferencedTags("ExpRefTag" + expressions[2])).Return(new[] { StringConstants.TagsRoot + tagsInExpressions[2] });

            v.Stub(x => x.GlobalController.GetAllTags<IGlobalDataItem>(Arg<TagsPredicate>.Is.Anything)).Return(new[] { t1, t2, t3, t4 });

            var validator = new CyclicExpressionProjectValidator(v.ToILazy(), tise);
            bool hasCyclic = !validator.Validate();
            Assert.IsTrue(hasCyclic == shouldHaveCyclic);
        }
    }
}
