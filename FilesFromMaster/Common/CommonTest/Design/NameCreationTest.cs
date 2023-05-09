using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Tools.Design;
using NUnit.Framework;
using Rhino.Mocks;
using INameCreationService = Neo.ApplicationFramework.Interfaces.INameCreationService;

namespace Neo.ApplicationFramework.Common.Design
{
    [TestFixture]
    public class NameCreationTest
    {
        private List<string> m_Names;
        private string m_Name;
        private INamingConstraints m_NamingConstraints;
        private IContainer m_ContainerMock;


        [SetUp]
        public void SetUpTest()
        {
            m_Names = new List<string>();
            m_Name = string.Empty;

            m_NamingConstraints = MockRepository.GenerateMock<INamingConstraints>();
            m_NamingConstraints.Stub(inv => inv.IsNameLengthValid(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(true);
            m_NamingConstraints.Stub(inv => inv.ReservedApplicationNames).Return(new HashSet<string>());
            m_NamingConstraints.Stub(inv => inv.ReservedSystemNames).Return(new HashSet<string>());

            m_ContainerMock = MockRepository.GenerateStub<IContainer>();
        }

        [Test]
        public void CreateUniqueNameUsingNullNameAndNullNames()
        {
            m_Names = null;
            m_Name = null;
            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual(string.Empty, uniqueName, "Created name not empty");
        }

        [Test]
        public void CreateUniqueNameUsingNullNameAndNoNames()
        {
            m_Name = null;
            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual(string.Empty, uniqueName, "Created name not empty");
        }

        [Test]
        public void CreateUniqueNameUsingNotEmptyNameAndOneNames()
        {
            m_Name = "Test";
            m_Names.Add(m_Name);

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("Test1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateUniqueNameUsingNotEmptyNameAndTwoNames()
        {
            m_Name = "Test";
            m_Names.Add(m_Name);
            m_Names.Add("Test1");

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("Test2", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateUniqueNameUsingNotEmptyNameAndTwoNamesNewNameInTheMiddle()
        {
            m_Name = "Test";
            m_Names.Add(m_Name);
            m_Names.Add("Test2");

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("Test1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateUniqueNameUsingNotEmptyNameAndTwoNamesFallbackToOriginal()
        {
            m_Name = "Test";
            m_Names.Add("Test1");
            m_Names.Add("Test2");

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("Test", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateUniqueNameVeryLongName()
        {
            m_Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong";
            m_Names.Add(m_Name);

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateUniqueNameVeryLongWithNewSuffix()
        {
            m_Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong";
            m_Names.Add(m_Name);
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon1");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon2");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon3");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon4");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon5");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon6");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon7");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon8");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon9");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon0");
            m_Names.Add("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLon");

            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            string uniqueName = nameCreationService.CreateUniqueName(m_Name, m_Names);

            Assert.AreEqual("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateNameWithinContainerNoNamesBefore()
        {
            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);

            m_ContainerMock.Stub(x => x.Components).Return(new ComponentCollection(
                new IComponent[]{})
            );

            m_Name = "Test";

            string uniqueName = nameCreationService.CreateName(m_ContainerMock, m_Name);

            Assert.AreEqual("Test1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateNameWithinContainerNamesExist()
        {
            m_ContainerMock.Stub(x => x.Components).Return(new ComponentCollection(
                new IComponent[]
                {
                    new Button
                    {
                        Name = "Test1",
                        Site = new TestSite {Name = "Test1"}
                    },
                    new Button
                    {
                        Name = "Test2",
                        Site = new TestSite {Name = "Test2"}
                    }
                })
            );


            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);

            m_Name = "Test";

            string uniqueName = nameCreationService.CreateName(m_ContainerMock, m_Name);

            Assert.AreEqual("Test3", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateNameWithinContainerNamesExistNameInTheMiddle()
        {
            m_ContainerMock.Stub(x => x.Components).Return(new ComponentCollection(
                new IComponent[]
                {
                    new Button
                    {
                        Name = "Test1",
                        Site = new TestSite {Name = "Test1"}
                    },
                    new Button
                    {
                        Name = "Test3",
                        Site = new TestSite {Name = "Test3"}
                    }
                })
            );


            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);

            m_Name = "Test";

            string uniqueName = nameCreationService.CreateName(m_ContainerMock, m_Name);

            Assert.AreEqual("Test2", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateNameWithinContainerNoNamesBeforeVeryLongName()
        {
            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);

            m_ContainerMock.Stub(x => x.Components).Return(new ComponentCollection(
                new IComponent[] { })
            );

            m_Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong";

            string uniqueName = nameCreationService.CreateName(m_ContainerMock, m_Name);

            Assert.AreEqual("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong1", uniqueName, "Created name is not valid");
        }

        [Test]
        public void CreateNameWithinContainerNamesExistVeryLongName()
        {
            m_ContainerMock.Stub(x => x.Components).Return(new ComponentCollection(
                new IComponent[]
                {
                    new Button
                    {
                        Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong1",
                        Site = new TestSite {Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong1"}
                    },
                    new Button
                    {
                        Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong2",
                        Site = new TestSite {Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong2"}
                    }
                })
            );


            INameCreationService nameCreationService = new NameCreationService(m_NamingConstraints);
            m_Name = "LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong";
            string uniqueName = nameCreationService.CreateName(m_ContainerMock, m_Name);

            Assert.AreEqual("LongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLongLong3", uniqueName, "Created name is not valid");
        }
    }
}
