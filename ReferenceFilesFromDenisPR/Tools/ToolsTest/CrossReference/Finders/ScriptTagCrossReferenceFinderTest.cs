using System;
using System.Linq;
using System.Text;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Serialization.Encryption;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CrossReference.Finders
{
    [TestFixture]
    public class ScriptTagCrossReferenceFinderTest
    {
        private readonly IProjectManager m_ProjectManager = Substitute.For<IProjectManager>();
        private readonly ILazy<IEncryptionStrategyFactory> m_EncryptionFactoryLazy = Substitute.For<IEncryptionStrategyFactory>().ToILazy();
        private readonly IOpcClientServiceIde m_OpcClientService = Substitute.For<IOpcClientServiceIde>();

        [SetUp]
        public void SetUp()
        {
            var globalController = Substitute.For<IGlobalController>();
            var tag1 = Substitute.For<IGlobalDataItem>();
            tag1.Name = "Tag1";
            var tag2 = Substitute.For<IGlobalDataItem>();
            tag2.Name = "Tag2";
            var tag3 = Substitute.For<IGlobalDataItem>();
            tag3.Name = "Tag3";

            globalController.GetAllTags<IGlobalDataItem>(TagsPredicate.Tags).Returns(
                new[]
                {
                    tag1, tag2, tag3
                });
            m_OpcClientService.GlobalController.Returns(globalController);
        }

        [Test]
        public void AllTagsInContentFound()
        {
            // ARRANGE
            var scriptContent = new StringBuilder();
            scriptContent.AppendLine("var x1 = " + StringConstants.Globals + "." + StringConstants.TagsRoot + "Tag1.Value;");
            scriptContent.AppendLine("var x2 = " + StringConstants.Globals + "." + StringConstants.TagsRoot + "Tag2.Read(); ");
            scriptContent.AppendLine("var x3 = " + StringConstants.Globals + "." + StringConstants.TagsRoot + "Tag3;");
            scriptContent.AppendLine("var x4 = SomeMethod(" + StringConstants.Globals + "." + StringConstants.TagsRoot + "Tag4, 0);");
            scriptContent.AppendLine("var x5 = SomeMethod(" + StringConstants.Globals + "." + StringConstants.TagsRoot + "Tag5 , 0);");
            scriptContent.AppendLine("m_Tags.Add(" + StringConstants.Globals + "." + StringConstants.TagsRoot + "System_Controller1_StateId);");

            var expectedResult = new[]
            {
                StringConstants.TagsRoot + "Tag1",
                StringConstants.TagsRoot + "Tag2",
                StringConstants.TagsRoot + "Tag3",
                StringConstants.TagsRoot + "Tag4",
                StringConstants.TagsRoot + "Tag5",
                StringConstants.TagsRoot + "System_Controller1_StateId"
            };

            var scriptTagCrossReferenceFinder = new ScriptTagCrossReferenceFinder(m_ProjectManager, m_EncryptionFactoryLazy, m_OpcClientService);

            // ACT
            var result = scriptTagCrossReferenceFinder.GetAllTagNamesFromContent(scriptContent.ToString()).ToArray();

            // ASSERT
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [Test]
        public void AllTagsInTagsScriptContentFound()
        {
            // ARRANGE
            var scriptContent = new StringBuilder();
            scriptContent.AppendLine("Tag2.Value = Tag1.Value + 1;");
            scriptContent.AppendLine("bool Tag4 = Tag3.LogToAuditTrail;");

            var expectedResult = new[]
            {
                StringConstants.TagsRoot + "Tag1",
                StringConstants.TagsRoot + "Tag2",
                StringConstants.TagsRoot + "Tag3"
            };

            var scriptTagCrossReferenceFinder = new ScriptTagCrossReferenceFinder(m_ProjectManager, m_EncryptionFactoryLazy, m_OpcClientService);

            // ACT
            var result = scriptTagCrossReferenceFinder.GetAllTagNamesFromScriptContentInTagsScript(scriptContent.ToString()).ToArray();

            // ASSERT
            CollectionAssert.AreEquivalent(expectedResult, result);
        }

    }
}