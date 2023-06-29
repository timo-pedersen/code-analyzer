using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.MultiLanguage;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.Utilities.IO;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityManagerToolCFTest : SecurityManagerToolTestBase
    {
        [Test]
        public void AddUserChecksIfPasswordMeetsCriteria()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            m_ISecurityManagerToolCF.AddUser(UserName, Password1);

            passwordComplexity.Received().AssertPasswordCriteria(Password1);
        }

        [Test]
        public void AddUserOverloadChecksIfPasswordMeetsCriteriaIfParameterIsTrue()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            m_ISecurityManagerToolCF.AddUser(UserName, Password1, true);

            passwordComplexity.Received().AssertPasswordCriteria(Password1);
        }

        [Test]
        public void AddUserOverloadChecksIfPasswordMeetsCriteriaIfParameterIsFalse()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            m_ISecurityManagerToolCF.AddUser(UserName, Password1, false);

            passwordComplexity.DidNotReceiveWithAnyArgs().AssertPasswordCriteria(Arg.Any<string>());
        }

        [Test]
        public void AddUserOverloadAddsUserToSecurityManager()
        {
            m_ISecurityManagerToolCF.AddUser(UserName, Password1, false);

            m_SecurityManagerStub.Received().AddUser(UserName, m_PasswordHash1);
        }

        [Test]
        public void LoginUserWorksIfUsernameAndPwIsCorrect()
        {
            var messageBoxServiceStub = Substitute.For<IMessageBoxServiceCF>();

            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceStub);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            //messageBoxServiceStub.Show(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), null); // NoOperation() ???
        }

        [Test]
        public void LoginUserShowsLoginSucceededDialogIfNotSpecified()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
        }

        [Test]
        public void LoginUserShowsLoginSucceededDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, true));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
        }

        [Test]
        public void LoginUserDoesNotShowLoginSucceededDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, false));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 0);
        }

        [Test]
        public void LoginUserDoesShowLoginFailedDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongPass", true, true));
            MockShow(messageBoxServiceMock, TextsCF.LoginFailure, 1);
        }

        [Test]
        public void LoginUserDoesNotShowLoginFailedDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongPass", true, false));
            MockShow(messageBoxServiceMock, TextsCF.LoginFailure, 0);
        }

        [Test]
        public void LoginUserDoesNotShowLoginFailedDialogWhenSuccedLogin()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, true, false));
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
        }

        [Test]
        public void LoginUserReturnsFalseWithWrongUsername()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser("someuser", Password1));
            MockShow(messageBoxServiceMock, TextsCF.LoginFailure, 1);
        }

        [Test]
        public void LoginUserReturnsFalseWithWrongPassword()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongpassword"));
            MockShow(messageBoxServiceMock, TextsCF.LoginFailure, 1);
        }

        [Test]
        public void LoginUserThrowsOnLoginPasswordNull()
        {
            Assert.Throws<ArgumentNullException>(() => m_ISecurityManagerToolCF.LoginUser(UserName, null));
        }

        [Test]
        public void LoginUserThrowsOnLoginUsernameNull()
        {
            Assert.Throws<ArgumentNullException>(() => m_ISecurityManagerToolCF.LoginUser(null, "somepw"));
        }

        [Test]
        public void LoginUserThrowsOnFoundUsersPasswordNull()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>();
            SecurityUser securityUser = new SecurityUser(UserName, null);
            users.Add(securityUser);

            ISecurityManager securityManagerMock = Substitute.For<ISecurityManager>();
            securityManagerMock.Users.Returns(users);
            m_ISecurityManagerToolCF.SecurityManager = securityManagerMock;

            Assert.Throws<NullReferenceException>(() => m_ISecurityManagerToolCF.LoginUser(UserName, Password1));
        }

        [Test]
        public void ChangePasswordThrowsOnWrongUsername()
        {
            Assert.Throws<InvalidUsernameOrPasswordException>(() => m_ISecurityManagerToolCF.ChangePassword("someuser", Password1, Password2, Password2));
        }

        [Test]
        public void ChangePasswordThrowsOnWrongPassword()
        {
            Assert.Throws<InvalidUsernameOrPasswordException>(() => m_ISecurityManagerToolCF.ChangePassword(UserName, "wrongpw", Password2, Password2));
        }

        [Test]
        public void ChangePasswordThrowsOnDifferentNewPasswords()
        {
            Assert.Throws<InvalidConfirmingPasswordException>(() => m_ISecurityManagerToolCF.ChangePassword(UserName, Password1, Password2.ToUpper(), Password2.ToLower()));
        }

        [Test]
        public void ChangePasswordChangesPassword()
        {
            m_ISecurityManagerToolCF.ChangePassword(UserName, Password1, Password2, Password2);

            Assert.AreEqual(m_PasswordHash2, m_UsersList[0].PasswordHash);
        }

        [Test]
        public void ChangePasswordOverloadThrowsOnWrongUsername()
        {
            Assert.Throws<InvalidUsernameOrPasswordException>(() => m_ISecurityManagerToolCF.ChangePassword("someuser", Password2));
        }

        [Test]
        public void ChangePasswordOverloadChangesPassword()
        {
            m_ISecurityManagerToolCF.ChangePassword(UserName, Password2);

            Assert.AreEqual(m_PasswordHash2, m_UsersList[0].PasswordHash);
        }

        [Test]
        public void ChangePasswordOverloadChecksIfPasswordMeetsCriteria()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            var simpleXmlSerializerFactoryStub = Substitute.For<IRunTimeXmlSerializerFactory>();
            m_SecurityManagerToolCF.SecurityUserSerializer = Substitute.For<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);

            m_ISecurityManagerToolCF.ChangePassword(UserName, Password2);

            passwordComplexity.Received().AssertPasswordCriteria(Password2);
        }

        //[Test]
        //public void ChangePasswordSavesPasswordFile()
        //{
        //    string filename = "somepasswordfile";

        //    m_ISecurityManagerToolCF.Load(filename);

        //    //Load clears the list of users so we'll set the stub again
        //    SetSecurityManagerMock();

        //    m_SecurityUserSerializer.Expect(mock => mock.Save(null, null, null))
        //                            .Constraints(Is.Equal(filename), Is.Anything(), Is.Anything());

        //    m_ISecurityManagerToolCF.ChangePassword(UserName, Password1, Password2, Password2);

        //    m_SecurityUserSerializer.VerifyAllExpectations();
        //}

        [Test]
        public void ChangePasswordChecksIfPasswordMeetsCriteria()
        {
            var passwordComplexity = Substitute.For<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            var simpleXmlSerializerFactoryStub = Substitute.For<IRunTimeXmlSerializerFactory>();
            m_SecurityManagerToolCF.SecurityUserSerializer = Substitute.For<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);

            m_ISecurityManagerToolCF.ChangePassword(UserName, Password1, Password2, Password2);

            passwordComplexity.Received().AssertPasswordCriteria(Password2);
        }

        [Test]
        public void LogoutUserWorks()
        {
            var messageBoxServiceStub = Substitute.For<IMessageBoxServiceCF>();

            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceStub);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser();
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
            //messageBoxServiceStub.Received().Show(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), null)); //.NoOperation();
        }

        [Test]
        public void LogoutUserShowsLogoutSucceededDialogIfNotSpecified()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser();
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLogoutSuccess, 1);
            //MockShow(messageBoxServiceMock, TextsCF.SecurityLogoutSuccess, 1); // ???
        }

        [Test]
        public void LogoutUserShowsLogoutSucceededDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser(true);
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLogoutSuccess, 1);
        }

        [Test]
        public void LogoutUserDoesNotShowLogoutSucceededDialog()
        {
            var messageBoxServiceMock = Substitute.For<IMessageBoxServiceCF>();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser(false);
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
            MockShow(messageBoxServiceMock, TextsCF.SecurityLoginSuccess, 1);
            MockShow(messageBoxServiceMock, TextsCF.SecurityLogoutSuccess, 0);
        }

        [Test]
        public void LoadChecksFileExistsAndReturnsFalseWhenItDoesNotExist()
        {
            string fileName = "filename";

            FileHelperCF fileHelper = Substitute.For<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            fileHelper.Exists(fileName).Returns(false);

            bool result = m_ISecurityManagerToolCF.Load(fileName);
            Assert.IsFalse(result);

            fileHelper.Received().Exists(fileName);
        }

        [Test]
        public void LoadChecksFileExistsAndReturnsTrueWhenItDoes()
        {
            string fileName = "filename";

            FileHelperCF fileHelper = Substitute.For<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            var simpleXmlSerializerFactoryStub = Substitute.For<IRunTimeXmlSerializerFactory>();
            SecurityUserSerializer xmlTypeSerializer = Substitute.For<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);
            xmlTypeSerializer.When(x => x.Load(fileName, out Arg.Any<IList<ISecurityUser>>(), out Arg.Any<IList<ISecurityGroup>>()))
                .Do(y =>
                {
                    y[1] = new List<ISecurityUser>();
                    y[2] = new List<ISecurityGroup>();
                });
            m_SecurityManagerToolCF.SecurityUserSerializer = xmlTypeSerializer;

            fileHelper.Exists(fileName).Returns(true);

            bool result = m_ISecurityManagerToolCF.Load(fileName);
            Assert.IsTrue(result);

            fileHelper.Received().Exists(fileName);
            xmlTypeSerializer.Received().Load(fileName, out Arg.Any<IList<ISecurityUser>>(), out Arg.Any<IList<ISecurityGroup>>());
        }

        [Test]
        public void SaveMovesPasswordToPasswordHashOnSecurityUser()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>
                                           {
                                               new SecurityUser
                                                   {
                                                       Username = "user1",
                                                       Password = Password1
                                                   }
                                           };

            ISecurityUser user = GetSavedSecurityUser(users);
            Assert.IsNull(user.Password);
            Assert.AreEqual(m_PasswordHash1, user.PasswordHash);
        }

        [Test]
        public void SaveDoesNotMovePasswordToPasswordHashWhenPasswordIsNullAndHashIsNot()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>
                                           {
                                               new SecurityUser
                                                   {
                                                       Username = "user1",
                                                       Password = null,
                                                       PasswordHash = m_PasswordHash1
                                                   }
                                           };

            ISecurityUser user = GetSavedSecurityUser(users);
            Assert.IsNull(user.Password);
            Assert.AreEqual(m_PasswordHash1, user.PasswordHash);
        }

        [Test]
        public void SaveMovesEmptyPasswordToPasswordHashWhenHashIsNull()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>
                                           {
                                               new SecurityUser
                                                   {
                                                       Username = "user1",
                                                       Password = string.Empty
                                                   }
                                           };

            ISecurityUser user = GetSavedSecurityUser(users);
            Assert.IsNull(user.Password);
            Assert.AreEqual(m_EmptyPasswordHash, user.PasswordHash);
        }

        [Test]
        public void SaveDoesNotMovePasswordToPasswordHashWhenPasswordHashIsNotNull()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>
                                           {
                                               new SecurityUser
                                                   {
                                                       Username = "user1",
                                                       Password = "somerandompw",
                                                       PasswordHash = m_PasswordHash2
                                                   }
                                           };

            ISecurityUser user = GetSavedSecurityUser(users);
            Assert.IsNull(user.Password);
            Assert.AreEqual(m_PasswordHash2, user.PasswordHash);
        }

        [Test]
        public void SaveThrowsExceptionWhenPasswordAndHashIsNull()
        {
            BindingList<ISecurityUser> users = new BindingList<ISecurityUser>
                                           {
                                               new SecurityUser
                                                   {
                                                       Username = "user1",
                                                       Password = null,
                                                       PasswordHash = null
                                                   }
                                           };

            ISecurityManager securityManager = Substitute.For<ISecurityManager>();
            m_ISecurityManagerToolCF.SecurityManager = securityManager;

            securityManager.Users.Returns(users);

            Assert.Throws<ArgumentNullException>(() => m_ISecurityManagerToolCF.Save(null));
        }


        private ISecurityUser GetSavedSecurityUser(BindingList<ISecurityUser> users)
        {
            IList<ISecurityUser> savedUsers = null;

            ISecurityManager securityManager = Substitute.For<ISecurityManager>();
            m_ISecurityManagerToolCF.SecurityManager = securityManager;


            securityManager.Users.Returns(users);

            securityManager.Groups.Returns(new BindingList<ISecurityGroup>());

            m_SecurityUserSerializer.When(x => x.Save(Arg.Any<string>(), Arg.Any<IList<ISecurityUser>>(), Arg.Any<IList<ISecurityGroup>>()))
                                    .Do(y => savedUsers = (IList<ISecurityUser>)y[1]);

            m_ISecurityManagerToolCF.Save(null);

            Assert.AreEqual(1, savedUsers.Count);

            return savedUsers[0];
        }
        
        private static void MockShow(IMessageBoxServiceCF x, string caption, short numOfRepeats)
        {
            x.Received(numOfRepeats)
                .Show(Arg.Any<string>(),
                    caption.GetTranslation(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    null);
        }
    }
}
