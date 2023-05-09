#if !VNEXT_TARGET
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityUserSerializerTest
    {
        private IRunTimeXmlSerializerFactory m_RunTimeXmlSerializerFactoryStub;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var namingConstraints = Substitute.For<INamingConstraints>();
            namingConstraints.IsNameLengthValid(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>()).Returns(true);
            namingConstraints.ReservedApplicationNames.Returns(new HashSet<string>());
            namingConstraints.ReservedSystemNames.Returns(new HashSet<string>());

            TestHelper.AddService(typeof(INameCreationService), new NameCreationService(namingConstraints));
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            TestHelper.RemoveService<IProjectNameCreationServiceIde>();
        }

        [SetUp]
        public void SetUp()
        {
            m_RunTimeXmlSerializerFactoryStub = Substitute.For<IRunTimeXmlSerializerFactory>();
        }

        [Test]
        public void LoadReturnsUserWithGroups()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?>
                            <Security SchemaVersion='1.0.6073.0'>
                              <SecurityGroups>
                                <Group ID='Group_01' Name='Administrators' />
                              </SecurityGroups>
                              <SecurityUsers>
                                <User>
                                  <Username>User1</Username>
                                  <PasswordHash>HHyqDQgp8v3yHPshl6yfweZIym8=</PasswordHash>
                                  <Description></Description>
                                  <Groups>
                                    <Group>Group_01</Group>
                                  </Groups>
                                </User>
                              </SecurityUsers>
                            </Security>";

            ISecurityUser user = LoadAndGetFirstUser(xml);
            Assert.That(user.Groups == SecurityGroups.Group_01);
        }

        [Test]
        public void LoadCanLoadOldVersion()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?>
                            <ArrayOfSecurityUser xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                              <SecurityUser>
                                <Username>Administrator</Username>
                                <Password>admin</Password>
                                <Description />
                                <Groups>Group_01 Group_02</Groups>
                              </SecurityUser>
                            </ArrayOfSecurityUser>";

            ISecurityUser user = LoadAndGetFirstUser(xml);
            Assert.That(user.Groups == (SecurityGroups.Group_01 | SecurityGroups.Group_02));
        }

        [Test]
        public void LoadCanLoadOldVersionWithPassword()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?>
                            <ArrayOfSecurityUser xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                              <SecurityUser>
                                <Username>Administrator</Username>
                                <Password>admin</Password>
                                <Description />
                                <Groups>Group_01 Group_02</Groups>
                              </SecurityUser>
                            </ArrayOfSecurityUser>";

            ISecurityUser user = LoadAndGetFirstUser(xml);
            Assert.That(user.Password == "admin");
        }

        [Test]
        public void LoadCanLoadOldVersionWithoutGroups()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?>
                            <ArrayOfSecurityUser xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                              <SecurityUser>
                                <Username>Administrator</Username>
                                <Password>admin</Password>
                                <Description />
                              </SecurityUser>
                            </ArrayOfSecurityUser>";

            ISecurityUser user = LoadAndGetFirstUser(xml);

            Assert.That(user.Username == "Administrator");
        }

        [Test]
        public void LoadCanLoadUserNameOperatorEvenIfNotAllowedByNameService()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?>
                            <ArrayOfSecurityUser xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                              <SecurityUser>
                                <Username>operator</Username>
                                <Password>IAmTheOperator</Password>
                                <Description />
                              </SecurityUser>
                            </ArrayOfSecurityUser>";

            ISecurityUser user = LoadAndGetFirstUser(xml);

            Assert.That(user.Username == "operator");
        }

        private ISecurityUser LoadAndGetFirstUser(string xml)
        {
            IList<ISecurityUser> users;
            IList<ISecurityGroup> groups;

            LoadUsersAndGroups(xml, out users, out groups);

            return users[0];
        }

        private void LoadUsersAndGroups(string xml, out IList<ISecurityUser> users, out IList<ISecurityGroup> groups)
        {
            XElement doc = XElement.Load(new StringReader(xml));

            SecurityUserSerializer serializer = new SecurityUserSerializer(m_RunTimeXmlSerializerFactoryStub);

            serializer.LoadXElement(doc, out users, out groups);
        }
    }
}
#endif
