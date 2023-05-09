using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Design;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Design
{
    [TestFixture]
    public class NameCreationServiceTest
    {
        protected INameCreationService NameCreationService;
        private IProjectNameCreationServiceIde m_ProjectNameCreationService;

        [SetUp]
        public void SetUp()
        {
            ISet<string> reservedSystemNames = new []
                                        {
                                            "main",
                                            "microsoft",
                                            "neo",
                                            "system",
                                            "multiaction",
                                            "com1",
                                            "com2",
                                            "com3",
                                            "com4",
                                            "com5",
                                            "com6",
                                            "com7",
                                            "com8",
                                            "com9"
                                        }.ToSet();

            var namingConstraints = Substitute.For<INamingConstraints>();
            namingConstraints.IsNameLengthValid(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
            namingConstraints.ReservedApplicationNames.Returns(new HashSet<string>());
            namingConstraints.ReservedSystemNames.Returns(reservedSystemNames);
            NameCreationService = new NameCreationService(namingConstraints);
            m_ProjectNameCreationService = new ProjectNameCreationService(NameCreationService.ToILazy());
        }

        [TestCase("A.a.a", "A_a_a")]
        [TestCase("Börja", "Börja")]
        [TestCase("Two Words", "Two_Words")]
        [TestCase("Two_Words", "Two_Words")]
        [TestCase("WithNumber20", "WithNumber20")]
        [TestCase("WithNumber_20", "WithNumber_20")]
        [TestCase("WithNumber.20", "WithNumber_20")]
        [TestCase("SollDruck_MPC.X_Werte", "SollDruck_MPC_X_Werte")]
        [TestCase("namespace", "_namespace")]
        [TestCase("class", "_class")]
        [TestCase("int", "_int")]
        [TestCase("1apa", "_apa")]
        [TestCase("1apa1", "_apa1")]
        public void RemoveIllegalCharactersMakesNameValid(string name, string expectedName)
        {
            Assert.That(NameCreationService.RemoveIllegalCharactersInName(name), Is.EqualTo(expectedName));
        }



        [TestCase("", "ProjectItem")]
        [TestCase("MAIN", "ProjectItem")]
        [TestCase("  SomeName ", "__SomeName_")]
        public void GetUniqueNameReturnsDefaultNameIfInvalid(string inputName, string expectedOutputName)
        {
            IProjectItem projectItem = Substitute.For<IProjectItem>();
            projectItem.NameExists(null, null).ReturnsForAnyArgs(false);

            IProject project = Substitute.For<IProject>();
            project.ProjectItems.Returns(new[] { projectItem });

            string outputName = m_ProjectNameCreationService.GetUniqueName(project, inputName);

            Assert.That(outputName, Is.EqualTo(expectedOutputName));
        }

        [TestCase("ProjectItem1", "ProjectItem11")]
        public void GetUniqueNameIncreasesTheSuffixNumberIfNameExistsInProject(string inputName, string expectedOutputName)
        {
            IProjectItem projectItem = Substitute.For<IProjectItem>();
            projectItem.NameExists(projectItem, inputName).Returns(true);

            IProject project = Substitute.For<IProject>();
            project.ProjectItems.Returns(new[] { projectItem });

            string outputName = m_ProjectNameCreationService.GetUniqueName(project, inputName);

            Assert.That(outputName, Is.EqualTo(expectedOutputName));
        }
    }
}
