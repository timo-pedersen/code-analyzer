using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Design;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Design
{
    [TestFixture]
    public class NameCreationServiceCreateProjectItemNameTest
    {
        private INamingConstraints m_NamingConstraints;

        [SetUp]
        public void Setup()
        {
            m_NamingConstraints = Substitute.For<INamingConstraints>();
            m_NamingConstraints.IsNameLengthValid(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
            m_NamingConstraints.ReservedApplicationNames.Returns(new HashSet<string>());
            m_NamingConstraints.ReservedSystemNames.Returns(new HashSet<string>());
        }

        [Test]
        public void FirstChildForBaseNameHasSuffix1()
        {
            IProjectItem projectItem = Substitute.For<IProjectItem>();
            string baseName = "Button";
            string expectedProjectItemName = baseName + "1";
            string actualProjectItemName;
            IProjectNameCreationServiceIde projectNameCreationService = new ProjectNameCreationService(new NameCreationService(m_NamingConstraints).ToILazy<INameCreationService>());

            projectItem.HasChild(Arg.Any<string>()).ReturnsForAnyArgs(false);

            actualProjectItemName = projectNameCreationService.CreateProjectItemName(projectItem, baseName);

            Assert.AreEqual(expectedProjectItemName, actualProjectItemName);
            projectItem.Received().HasChild(Arg.Any<string>());
        }

        [Test]
        public void SecondChildForBaseNameHasSuffix2()
        {
            IProjectItem projectItem = Substitute.For<IProjectItem>();
            string baseName = "Button";
            string expectedProjectItemName = baseName + "2";
            string actualProjectItemName;
            IProjectNameCreationServiceIde projectNameCreationService = new ProjectNameCreationService(new NameCreationService(m_NamingConstraints).ToILazy<INameCreationService>());

            projectItem.HasChild(Arg.Any<string>()).ReturnsForAnyArgs(true, false);

            actualProjectItemName = projectNameCreationService.CreateProjectItemName(projectItem, baseName);

            Assert.AreEqual(expectedProjectItemName, actualProjectItemName);
            projectItem.Received().HasChild(Arg.Any<string>());
        }
    }
}
