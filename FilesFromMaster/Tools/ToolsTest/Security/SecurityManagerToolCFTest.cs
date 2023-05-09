using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.MultiLanguage;
using Neo.ApplicationFramework.Common.Serialization;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Security
{
    [TestFixture]
    public class SecurityManagerToolCFTest : SecurityManagerToolTestBase
    {
        private PasswordComplexity SetUpPasswordComplexityMock(string password)
        {
            var passwordComplexity = MockRepository.GenerateMock<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            passwordComplexity.Expect(x => x.AssertPasswordCriteria(password));

            return passwordComplexity;
        }

        [Test]
        public void AddUserChecksIfPasswordMeetsCriteria()
        {
            var passwordComplexity = MockRepository.GenerateMock<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            passwordComplexity.Expect(x => x.AssertPasswordCriteria(Password1));

            m_ISecurityManagerToolCF.AddUser(UserName, Password1);

            passwordComplexity.VerifyAllExpectations();
        }

        [Test]
        public void AddUserOverloadChecksIfPasswordMeetsCriteriaIfParameterIsTrue()
        {
            var passwordComplexity = MockRepository.GenerateMock<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            passwordComplexity.Expect(x => x.AssertPasswordCriteria(Password1));

            m_ISecurityManagerToolCF.AddUser(UserName, Password1, true);

            passwordComplexity.VerifyAllExpectations();
        }

        [Test]
        public void AddUserOverloadChecksIfPasswordMeetsCriteriaIfParameterIsFalse()
        {
            var passwordComplexity = MockRepository.GenerateMock<PasswordComplexity>();
            m_SecurityManagerToolCF.PasswordComplexity = passwordComplexity;

            passwordComplexity.Expect(x => x.AssertPasswordCriteria(null))
                              .IgnoreArguments()
                              .Repeat.Never();

            m_ISecurityManagerToolCF.AddUser(UserName, Password1, false);

            passwordComplexity.VerifyAllExpectations();
        }

        [Test]
        public void AddUserOverloadAddsUserToSecurityManager()
        {
            m_ISecurityManagerToolCF.AddUser(UserName, Password1, false);

            m_SecurityManagerStub.AssertWasCalled(stub => stub.AddUser(UserName, m_PasswordHash1));
        }

        [Test]
        public void LoginUserWorksIfUsernameAndPwIsCorrect()
        {
            var messageBoxServiceStub = MockRepository.GenerateMock<IMessageBoxServiceCF>();
            messageBoxServiceStub.Stub(x => x.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<bool>.Is.Anything, Arg<System.Action>.Is.Null)).NoOperation();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceStub);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
        }

        [Test]
        public void LoginUserShowsLoginSucceededDialogIfNotSpecified()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
        }

        [Test]
        public void LoginUserShowsLoginSucceededDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, true));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
        }

        [Test]
        public void LoginUserDoesNotShowLoginSucceededDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Never();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, false));
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
        }

        [Test]
        public void LoginUserDoesShowLoginFailedDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.LoginFailure)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongPass", true, true));
        }

        [Test]
        public void LoginUserDoesNotShowLoginFailedDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.LoginFailure)).Repeat.Never();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongPass", true, false));
        }

        [Test]
        public void LoginUserDoesNotShowLoginFailedDialogWhenSuccedLogin()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsTrue(m_ISecurityManagerToolCF.LoginUser(UserName, Password1, true, false));
        }

        [Test]
        public void LoginUserReturnsFalseWithWrongUsername()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.LoginFailure)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser("someuser", Password1));
        }

        [Test]
        public void LoginUserReturnsFalseWithWrongPassword()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.LoginFailure)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            Assert.IsFalse(m_ISecurityManagerToolCF.LoginUser(UserName, "wrongpassword"));
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

            ISecurityManager securityManagerMock = MockRepository.GenerateStub<ISecurityManager>();
            securityManagerMock.Stub(x => x.Users).Return(users).Repeat.Any();
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
            var passwordComplexity = SetUpPasswordComplexityMock(Password2);
            var simpleXmlSerializerFactoryStub = MockRepository.GenerateStub<IRunTimeXmlSerializerFactory>();
            m_SecurityManagerToolCF.SecurityUserSerializer = MockRepository.GenerateStub<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);

            m_ISecurityManagerToolCF.ChangePassword(UserName, Password2);

            passwordComplexity.VerifyAllExpectations();
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
            var passwordComplexity = SetUpPasswordComplexityMock(Password2);
            var simpleXmlSerializerFactoryStub = MockRepository.GenerateStub<IRunTimeXmlSerializerFactory>();
            m_SecurityManagerToolCF.SecurityUserSerializer = MockRepository.GenerateStub<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);

            m_ISecurityManagerToolCF.ChangePassword(UserName, Password1, Password2, Password2);

            passwordComplexity.VerifyAllExpectations();
        }

        [Test]
        public void LogoutUserWorks()
        {
            var messageBoxServiceStub = MockRepository.GenerateMock<IMessageBoxServiceCF>();
            messageBoxServiceStub.Stub(x => x.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything, Arg<bool>.Is.Anything, Arg<System.Action>.Is.Null)).NoOperation();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceStub);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser();
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
        }

        [Test]
        public void LogoutUserShowsLogoutSucceededDialogIfNotSpecified()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLogoutSuccess)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser();
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
        }

        [Test]
        public void LogoutUserShowsLogoutSucceededDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLogoutSuccess)).Repeat.Once();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser(true);
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
        }

        [Test]
        public void LogoutUserDoesNotShowLogoutSucceededDialog()
        {
            var messageBoxServiceMock = MockRepository.GenerateStrictMock<IMessageBoxServiceCF>();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLoginSuccess)).Repeat.Once();
            messageBoxServiceMock.Expect(x => MockShow(x, TextsCF.SecurityLogoutSuccess)).Repeat.Never();
            ServiceContainerCF.Instance.AddService(typeof(IMessageBoxServiceCF), messageBoxServiceMock);

            m_ISecurityManagerToolCF.LoginUser(UserName, Password1);
            Assert.AreEqual(m_ISecurityManagerToolCF.CurrentUser, UserName);
            m_ISecurityManagerToolCF.LogoutUser(false);
            Assert.IsTrue(string.IsNullOrEmpty(m_ISecurityManagerToolCF.CurrentUser));
        }

        [Test]
        public void LoadChecksFileExistsAndReturnsFalseWhenItDoesNotExist()
        {
            string fileName = "filename";

            FileHelperCF fileHelper = MockRepository.GenerateMock<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            fileHelper.Expect(mock => mock.Exists(fileName))
                      .Return(false);

            bool result = m_ISecurityManagerToolCF.Load(fileName);
            Assert.IsFalse(result);

            fileHelper.VerifyAllExpectations();
        }

        [Test]
        public void LoadChecksFileExistsAndReturnsTrueWhenItDoes()
        {
            string fileName = "filename";

            FileHelperCF fileHelper = MockRepository.GenerateMock<FileHelperCF>();
            m_SecurityManagerToolCF.FileHelper = fileHelper;

            var simpleXmlSerializerFactoryStub = MockRepository.GenerateStub<IRunTimeXmlSerializerFactory>();
            SecurityUserSerializer xmlTypeSerializer = MockRepository.GenerateMock<SecurityUserSerializer>(simpleXmlSerializerFactoryStub);
            m_SecurityManagerToolCF.SecurityUserSerializer = xmlTypeSerializer;

            fileHelper.Expect(mock => mock.Exists(fileName))
                      .Return(true);

            IList<ISecurityGroup> groups = new List<ISecurityGroup>();
            IList<ISecurityUser> users = new List<ISecurityUser>();

            xmlTypeSerializer.Expect(mock => mock.Load(fileName, out users, out groups)).OutRef(users, groups);

            bool result = m_ISecurityManagerToolCF.Load(fileName);
            Assert.IsTrue(result);

            fileHelper.VerifyAllExpectations();
            xmlTypeSerializer.VerifyAllExpectations();
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

            ISecurityManager securityManager = MockRepository.GenerateMock<ISecurityManager>();
            m_ISecurityManagerToolCF.SecurityManager = securityManager;

            securityManager.Expect(mock => mock.Users)
                           .Return(users)
                           .Repeat.AtLeastOnce();

            Assert.Throws<ArgumentNullException>(() => m_ISecurityManagerToolCF.Save(null));

            securityManager.VerifyAllExpectations();
        }


        private ISecurityUser GetSavedSecurityUser(BindingList<ISecurityUser> users)
        {
            IList<ISecurityUser> savedUsers = null;

            ISecurityManager securityManager = MockRepository.GenerateMock<ISecurityManager>();
            m_ISecurityManagerToolCF.SecurityManager = securityManager;


            securityManager.Expect(mock => mock.Users)
                           .Return(users)
                           .Repeat.AtLeastOnce();

            securityManager.Expect(mock => mock.Groups)
                           .Return(new BindingList<ISecurityGroup>())
                           .Repeat.AtLeastOnce();

            m_SecurityUserSerializer.Expect(mock => mock.Save(null, null, null))
                                    .Callback(new Func<string, IList<ISecurityUser>, IList<ISecurityGroup>, bool>(
                                      (str, userList, groupList) =>
                                      {
                                          savedUsers = userList;
                                          return true;
                                      }));

            m_ISecurityManagerToolCF.Save(null);

            securityManager.VerifyAllExpectations();
            m_SecurityUserSerializer.VerifyAllExpectations();

            Assert.AreEqual(1, savedUsers.Count);

            return savedUsers[0];
        }
        
        private static void MockShow(IMessageBoxServiceCF x, string caption)
        {
            x.Show(Arg<string>.Is.Anything,
                Arg<string>.Is.Equal(caption.GetTranslation()),
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<System.Action>.Is.Null);
        }
    }
}
