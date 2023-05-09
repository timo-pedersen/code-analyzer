using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Design;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Design
{
    [TestFixture]
    public class NameCreationServiceCreateProjectItemNameTest
    {
        private INamingConstraints m_NamingConstraints;

        [SetUp]
        public void Setup()
        {
            m_NamingConstraints = MockRepository.GenerateMock<INamingConstraints>();
            m_NamingConstraints.Stub(inv => inv.IsNameLengthValid(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(true);
            m_NamingConstraints.Stub(inv => inv.ReservedApplicationNames).Return(new HashSet<string>());
            m_NamingConstraints.Stub(inv => inv.ReservedSystemNames).Return(new HashSet<string>());
        }

        [Test]
        public void FirstChildForBaseNameHasSuffix1()
        {
            MockRepository mockRepository = new MockRepository();
            IProjectItem projectItem = mockRepository.StrictMock<IProjectItem>();
            String baseName = "Button";
            String expectedProjectItemName = baseName + "1";
            String actualProjectItemName;
            IProjectNameCreationServiceIde projectNameCreationService = new ProjectNameCreationService(new NameCreationService(m_NamingConstraints).ToILazy<INameCreationService>());

            using (mockRepository.Record())
            {
                Expect.Call(projectItem.HasChild(null)).IgnoreArguments().Return(false);
            }

            using (mockRepository.Playback())
            {
                actualProjectItemName = projectNameCreationService.CreateProjectItemName(projectItem, baseName);
            }
                        
            Assert.AreEqual(expectedProjectItemName, actualProjectItemName);
        }

        [Test]
        public void SecondChildForBaseNameHasSuffix2()
        {
            MockRepository mockRepository = new MockRepository();
            IProjectItem projectItem = mockRepository.StrictMock<IProjectItem>();
            String baseName = "Button";
            String expectedProjectItemName = baseName + "2";
            String actualProjectItemName;
            IProjectNameCreationServiceIde projectNameCreationService = new ProjectNameCreationService(new NameCreationService(m_NamingConstraints).ToILazy<INameCreationService>());

            using (mockRepository.Record())
            {
                Expect.Call(projectItem.HasChild(null)).IgnoreArguments().Return(true);
                Expect.Call(projectItem.HasChild(null)).IgnoreArguments().Return(false);
            }

            using (mockRepository.Playback())
            {
                actualProjectItemName = projectNameCreationService.CreateProjectItemName(projectItem, baseName);
            }

            Assert.AreEqual(expectedProjectItemName, actualProjectItemName);            
        }
    }
}
