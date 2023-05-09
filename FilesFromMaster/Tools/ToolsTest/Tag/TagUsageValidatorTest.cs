using Core.Api.CrossReference;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

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
            m_CrossReferenceQueryServiceStub = MockRepository.GenerateStub<ICrossReferenceQueryService>();
            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();
            m_ErrorListServiceStub = MockRepository.GenerateStub<IErrorListService>();
            m_ProjectStub = MockRepository.GenerateStub<IProject>();
            m_ScriptOwnerMock = MockRepository.GenerateMock<IProjectItem, IScriptOwner>();
            m_ValidateWarnings = new List<string>();

            m_ErrorListServiceStub.Stub(x => x.Add(Arg<CompilerError>.Is.Anything))
                .WhenCalled(x => m_ValidateWarnings.Add(((CompilerError)x.Arguments[0]).ErrorText));

            string[] tagNames =
            {
                "Tags.Tag1", "Tags.Tag2", "Tags.Tag3", "Tags.Tag4"
            };

            m_CrossReferenceQueryServiceStub.Stub(
                    x => x.GetReferences<ICrossReferenceItem>(
                        Arg<string[]>.Is.Equal(
                            new[]
                            {
                                "Script"
                            })))
                .Return(
                    Enumerable.Range(0, tagNames.Length).Select(
                        i => new CrossReferenceItem
                        {
                            SourceFullName = tagNames[i]
                        }).ToList());

            m_MainProjectStub = new[]
            {
                TestHelper.CreateAndAddServiceMock<IProjectItem>()
            };

            m_MainProjectStub[0].Stub(x => x.ProjectItems).Return(new[]
            {
                m_ScriptOwnerMock
            });

            m_ProjectStub.Stub(x => x.ProjectItems).Return(m_MainProjectStub);
            m_ProjectManagerStub.Project = m_ProjectStub;
        }

        [Test]
        public void NoUsageFoundTest()
        {
            m_ScriptOwnerMock.Stub(scriptOwner => ((IScriptOwner)scriptOwner).ScriptText).Return(@"
// Script block without Tag variables
public void SomeMethod()
{
    someValue.Value += 42;
}
");

            var tagValidator = new TagUsageValidator(m_ProjectManagerStub, m_CrossReferenceQueryServiceStub.ToILazy(), m_ErrorListServiceStub.ToILazy());
            Assert.That(tagValidator.Validate(), Is.True);
            m_ErrorListServiceStub.AssertWasNotCalled(x => x.Add(Arg<CompilerError>.Is.Anything));
        }

        [Test]
        public void AllOperandsTagUsageTest()
        {
            m_ScriptOwnerMock.Stub(scriptOwner => ((IScriptOwner)scriptOwner).ScriptText).Return(@"
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
            m_ScriptOwnerMock.Stub(scriptOwner => ((IScriptOwner)scriptOwner).ScriptText).Return(@"
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