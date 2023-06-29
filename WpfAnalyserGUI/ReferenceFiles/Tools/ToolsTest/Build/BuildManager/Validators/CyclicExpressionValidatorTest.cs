using System;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Expressions.Validation;
using Neo.ApplicationFramework.Tools.Script.Validation;
using NSubstitute;
using NUnit.Framework;

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
            var v = Substitute.For<IOpcClientServiceIde>();
            var tise = Substitute.For<ITagsInScriptExtractor>();
            IGlobalDataItem t1 = Substitute.For<IGlobalDataItem>();
            IGlobalDataItem t2 = Substitute.For<IGlobalDataItem>();
            IGlobalDataItem t3 = Substitute.For<IGlobalDataItem>();
            IGlobalDataItem t4 = Substitute.For<IGlobalDataItem>();

            t1.FullName.Returns(StringConstants.TagsRoot + tags[0]);
            t2.FullName.Returns(StringConstants.TagsRoot + tags[1]);
            t3.FullName.Returns(StringConstants.TagsRoot + tags[2]);
            t4.FullName.Returns(StringConstants.TagsRoot);
            t1.ReadExpression.Returns("ExpRefTag" + expressions[0]);
            t2.ReadExpression.Returns("ExpRefTag" + expressions[1]);
            t3.ReadExpression.Returns("ExpRefTag" + expressions[2]);

            tise.ExtractReferencedTags("ExpRefTag" + expressions[0]).Returns(new[] { StringConstants.TagsRoot + tagsInExpressions[0] });
            tise.ExtractReferencedTags("ExpRefTag" + expressions[1]).Returns(new[] { StringConstants.TagsRoot + tagsInExpressions[1] });
            tise.ExtractReferencedTags("ExpRefTag" + expressions[2]).Returns(new[] { StringConstants.TagsRoot + tagsInExpressions[2] });

            v.GlobalController.GetAllTags<IGlobalDataItem>(Arg.Any<TagsPredicate>()).Returns(new[] { t1, t2, t3, t4 });

            var validator = new CyclicExpressionProjectValidator(v.ToILazy(), tise);
            bool hasCyclic = !validator.Validate();
            Assert.IsTrue(hasCyclic == shouldHaveCyclic);
        }
    }
}
