using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Tag
{
    [TestFixture]
    public class TagUsageValidatorTest
    {
        private ICrossReferenceQueryService m_CrossReferenceQueryServiceStub;
        private IProjectManager m_ProjectManagerStub;
        private IErrorListService m_ErrorListServiceStub;
        private IProject m_ProjectStub;
        private IProjectItem[] m_MainProjectStub;
        private IProjectItem m_ScriptOwnerMock;
        private List<string> m_ValidateWarnings;

        [SetUp]
        public void SetUp()
        {
            m_CrossReferenceQueryServiceStub = Substitute.For<ICrossReferenceQueryService>();
            m_ProjectManagerStub = Substitute.For<IProjectManager>();
            m_ErrorListServiceStub = Substitute.For<IErrorListService>();
            m_ProjectStub = Substitute.For<IProject>();
            m_ScriptOwnerMock = Substitute.For<IProjectItem, IScriptOwner>();
            m_ValidateWarnings = new List<string>();

            m_ErrorListServiceStub.WhenForAnyArgs(x => x.Add(Arg.Any<CompilerError>()))
                .Do(y => m_ValidateWarnings.Add(((CompilerError)y[0]).ErrorText));

            string[] tagNames =
            {
                "Tags.Tag1", "Tags.Tag2", "Tags.Tag3", "Tags.Tag4"
            };

            m_CrossReferenceQueryServiceStub.GetReferences<ICrossReferenceItem>(new[] { "Script" })
                .Returns(
                    Enumerable.Range(0, tagNames.Length).Select(
                        i => new CrossReferenceItem
                        {
                            SourceFullName = tagNames[i]
                        }).ToList());

            m_MainProjectStub = new[]
            {
                TestHelper.CreateAndAddServiceStub<IProjectItem>()
            };

            m_MainProjectStub[0].ProjectItems.Returns(new[]
            {
                m_ScriptOwnerMock
            });

            m_ProjectStub.ProjectItems.Returns(m_MainProjectStub);
            m_ProjectManagerStub.Project = m_ProjectStub;
        }

        [Test]
        public void NoUsageFoundTest()
        {
            ((IScriptOwner)m_ScriptOwnerMock).ScriptText.Returns(@"
// Script block without Tag variables
public void SomeMethod()
{
    someValue.Value += 42;
}
");

            var tagValidator = new TagUsageValidator(m_ProjectManagerStub, m_CrossReferenceQueryServiceStub.ToILazy(), m_ErrorListServiceStub.ToILazy());
            Assert.That(tagValidator.Validate(), Is.True);
            m_ErrorListServiceStub.DidNotReceiveWithAnyArgs().Add(Arg.Any<CompilerError>());
        }

        [Test]
        public void AllOperandsTagUsageTest()
        {
            ((IScriptOwner)m_ScriptOwnerMock).ScriptText.Returns(@"
// Script block with Tag variables
public void SomeMethod()
{
    Globals.Tags.Tag1.Value += 1;
    Globals.Tags.Tag2.Value -= 1;
    Globals.Tags.Tag3.Value *= 1;
    Globals.Tags.Tag4.Value /= 1;
}
");
            
            var tagValidator = new TagUsageValidator(m_ProjectManagerStub, m_CrossReferenceQueryServiceStub.ToILazy(), m_ErrorListServiceStub.ToILazy());
            tagValidator.Validate();
            Assert.That(m_ValidateWarnings.Count, Is.EqualTo(4));
            Assert.That(m_ValidateWarnings[0], Is.EqualTo("Using '+=' is obsolete: Please use 'IncrementAnalog' instead (Occurrences: 1)."));
            Assert.That(m_ValidateWarnings[1], Is.EqualTo("Using '-=' is obsolete: Please use 'IncrementAnalog' instead (Occurrences: 1)."));
            Assert.That(m_ValidateWarnings[2], Is.EqualTo("Using '*=' is obsolete: Please use 'IncrementAnalog' instead (Occurrences: 1)."));
            Assert.That(m_ValidateWarnings[3], Is.EqualTo("Using '/=' is obsolete: Please use 'IncrementAnalog' instead (Occurrences: 1)."));
        }

        [Test]
        public void DifferentCodeFormattingTest()
        {
            ((IScriptOwner)m_ScriptOwnerMock).ScriptText.Returns(@"
// Script block with different code formatting
public void SomeMethod()
{
    Globals.Tags.Tag1.Value      +=     1;   // Spaces
    Globals.Tags.Tag2.Value      -=       1; // Tabs
    Globals.Tags.Tag3.Value       *=    1;   // Tabs and spaces
    Globals.Tags.Tag4.Value       
         /= 1;                              // New line

             Global.Tags.Tag1.Value   -=    1;
}
");

            var tagValidator = new TagUsageValidator(m_ProjectManagerStub, m_CrossReferenceQueryServiceStub.ToILazy(), m_ErrorListServiceStub.ToILazy());
            tagValidator.Validate();
            Assert.That(m_ValidateWarnings.Count, Is.EqualTo(4));
            StringAssert.Contains("Occurrences: 2", m_ValidateWarnings[1]);
        }
    }
}