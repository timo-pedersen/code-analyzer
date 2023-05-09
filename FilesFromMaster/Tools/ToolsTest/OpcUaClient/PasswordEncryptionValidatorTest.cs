using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaClient.Validation;
using NUnit.Framework;
using Rhino.Mocks;
using System;

namespace Neo.ApplicationFramework.Tools.OpcUaClient
{
    [TestFixture]
    class PasswordEncryptionValidatorTest
    {
        private IOpcClientServiceIde m_OpcClientServiceIde;
        private PasswordEncryptionValidator m_Validator;
        private IErrorListService m_ErrorListService;
        private IExtendedBindingList<IDataSourceContainer> m_Controllers;

        [SetUp]
        public void SetUp()
        {
            m_OpcClientServiceIde = MockRepository.GenerateStub<IOpcClientServiceIde>();
            m_Validator = new PasswordEncryptionValidator(m_OpcClientServiceIde.ToILazy());
            m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();
            m_Controllers = new ExtendedBindingList<IDataSourceContainer>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [TestCase(false, 1, 0)]
        [TestCase(false, 2, 0)]
        [TestCase(true, 1, 1)]
        [TestCase(true, 2, 1)]
        [TestCase(true, 5, 1)]
        public void AllowNonSecureConnectionsSetting_DuringBuild_WeGetCorrectNumberOfWarnings(bool nonSecureConnectionsAllowed, int numberOfControllers, int buildErrors)
        {
            //ARRANGE
            AddControllers(nonSecureConnectionsAllowed, numberOfControllers);
            m_OpcClientServiceIde.Stub(x => x.Controllers).Return(m_Controllers);
            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Equal(true))).Repeat.Times(buildErrors);
            //ACT
            m_Validator.Validate();
            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
        }

        private void AddControllers(bool nonSecureConnectionsAllowed, int numberOfControllers)
        {
            for (int i = 0; i < numberOfControllers; i++)
            {
                m_Controllers.Add(GetController(nonSecureConnectionsAllowed));
            }
        }

        private IDataSourceContainer GetController(bool nonSecureConnectionsAllowed)
        {
            var controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.OpcUaAllowNonSecureConnections = nonSecureConnectionsAllowed;
            controller.DataSourceType = DataSourceType.DataSourceOpcUaExternal;
            return controller;
        }
    }
}
