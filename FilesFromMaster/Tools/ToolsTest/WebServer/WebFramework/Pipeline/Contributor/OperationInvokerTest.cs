using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class OperationInvokerTest
    {
        private OperationInvoker m_OperationInvoker;

        private ICommunicationContext m_Context;

        [SetUp]
        public void Setup()
        {
            m_OperationInvoker = new OperationInvoker();
            m_Context = CommunicationContextFixture.AfterOperationCandidateGeneration;
        }

        [Test]
        public void Should_respond_with_400_bad_request_if_there_are_no_operations_with_valid_inputs()
        {
            m_Context.PipelineData.CandidateOperations = new[] { OperationFixture.Invalid };

            m_OperationInvoker.Process(m_Context);

            Assert.That(m_Context.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Should_wrap_non_operation_results_in_ok_operation_result()
        {
            IOperation operationMock = OperationFixture.Valid;
            object operationResult = new object();
            operationMock.Stub(m => m.Invoke()).Return(operationResult).Repeat.Any();
            m_Context.PipelineData.CandidateOperations = new[] { operationMock };

            m_OperationInvoker.Process(m_Context);

            Assert.That(m_Context.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(200));
            Assert.That(m_Context.PipelineData.OperationResult.Result, Is.SameAs(operationResult));
        }      
        
        [Test]
        public void Should_use_operation_result_when_created_by_operation()
        {
            IOperation operationMock = OperationFixture.Valid;
            OperationResult operationResult = OperationResult.Forbidden();
            operationMock.Stub(m => m.Invoke()).Return(operationResult).Repeat.Any();
            m_Context.PipelineData.CandidateOperations = new[] { operationMock };

            m_OperationInvoker.Process(m_Context);

            Assert.That(m_Context.PipelineData.OperationResult, Is.SameAs(operationResult));
        }      
        
        [Test]
        public void Should_invoke_the_operation_with_most_parameters_which_also_valid()
        {
            IOperation opOne = OperationFixture.FooWithBarStringParameter;
            IOperation opTwo = OperationFixture.FooWithBarAndBazStringParameters;
            IOperation opThree = OperationFixture.Invalid;
            opOne.Stub(m => m.IsAllInputValid).Return(true);
            opTwo.Stub(m => m.IsAllInputValid).Return(true);
            m_Context.PipelineData.CandidateOperations = new[] { opOne, opTwo, opThree };

            m_OperationInvoker.Process(m_Context);

            opTwo.AssertWasCalled(m => m.Invoke());
        }
    }
}