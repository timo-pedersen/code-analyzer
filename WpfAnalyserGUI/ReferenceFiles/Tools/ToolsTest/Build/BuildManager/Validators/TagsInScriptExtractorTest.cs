using System;
using System.Collections.Generic;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Tools.Script.Validation;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    public class TagsInScriptExtractorTest
    {

        private const string Script1 = "value + 1;";
        private const string Script2 = "Globals.Tags.T + 2 yiuyasd8h jkhasd euwiuy weiuy 98iusad ";
        private const string Script3 = "T1 + _T - T1k2j3lk2j3";
        private const string Script4 = "Globals.Tags.T + Globals.Tags.T1 + Globals.Tags._T + Globals.Tags.T1k2j3lk2j3";
        private const string Script5 = "lkjsdflkjsdfGlobals.lkjsdflksjdf +Globals.Tags + Globals.Tags.T1k2j3lk2j3";
        private const string Script6 = "";
        private const string Script7 = "T1 + Globals.Tags.T1k2j3lk2j3 + T";

        private const string Tag1 = "Tags.T";
        private const string Tag2 = "Tags.T1";
        private const string Tag3 = "Tags._T";
        private const string Tag4 = "Tags.T1k2j3lk2j3";

        [Test]
        [TestCase(Script1, new string[] { })]
        [TestCase(Script2, new string[] { Tag1 })]
        [TestCase(Script3, new string[] { })]
        [TestCase(Script4, new string[] { Tag1, Tag2, Tag3, Tag4 })]
        [TestCase(Script5, new string[] { Tag4 })]
        [TestCase(Script6, new string[] { })]
        [TestCase(Script7, new string[] { Tag4 })]
        public void CanExtract(string script, string[] tagsInScript)
        {
            ISet<string> tags = new[] { Tag1, Tag2, Tag3, Tag4 }.ToSet();
            IExpressionsService expressionsService = Substitute.For<IExpressionsService>();
            var expression = Substitute.For<IExpression>();
            expression.Script = script;
            expression.Name = "E";
            expressionsService.Expressions.Returns(new ExtendedBindingList<IExpression>() { expression });

            TagsInScriptExtractor tagsInScriptExtractor = new TagsInScriptExtractor(tags, LazyCreateNameCreationService(), expressionsService.ToILazy());
            IEnumerable<string> extractedTags = tagsInScriptExtractor.ExtractReferencedTags("E");

            Assert.True(extractedTags.ContainsSameElements(tagsInScript), "The following tags were expected in script {1}: {0}{2}{0} But these were found {0}{3}".CurrentCultureFormat(Environment.NewLine, script, tagsInScript.ToText(), extractedTags.ToText()));
        }

        private static ILazy<INameCreationService> LazyCreateNameCreationService()
        {
            return new LazyWrapper<INameCreationService>(
                () =>
                {
                    var namingConstraints = Substitute.For<INamingConstraints>();
                    namingConstraints.IsNameLengthValid(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
                    namingConstraints.ReservedApplicationNames.Returns(new HashSet<string>());
                    namingConstraints.ReservedSystemNames.Returns(new HashSet<string>());
                    return new NameCreationService(namingConstraints);
                });
        }

    }
}
