#if !VNEXT_TARGET
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaClient.Validation;
using NUnit.Framework;
using NSubstitute;
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
            m_OpcClientServiceIde = Substitute.For<IOpcClientServiceIde>();
            m_Validator = new PasswordEncryptionValidator(m_OpcClientServiceIde.ToILazy());
            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();
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
            m_OpcClientServiceIde.Controllers.Returns(m_Controllers);
            
            //ACT
            m_Validator.Validate();
            
            //ASSERT
            m_ErrorListService.Received(buildErrors).AddNewCompilerError(Arg.Any<string>(), true);
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
            var controller = Substitute.For<IDataSourceContainer>();
            controller.OpcUaAllowNonSecureConnections = nonSecureConnectionsAllowed;
            controller.DataSourceType = DataSourceType.DataSourceOpcUaExternal;
            return controller;
        }
    }
}
#endif
