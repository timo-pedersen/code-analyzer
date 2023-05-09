using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class OperationCandidateGeneratorTest
    {
        private OperationCandidateGenerator m_OperationCandidateGenerator;
        private ICommunicationContext m_Context;
        private OperationCreator m_OperationCreator;
        private OperationFilter m_OperationFilter;

        [SetUp]
        public void SetUp()
        {
            m_OperationCreator = MockRepository.GenerateMock<OperationCreator>();
            m_OperationFilter = MockRepository.GenerateMock<OperationFilter>();
            m_OperationCandidateGenerator = new OperationCandidateGenerator(m_OperationCreator, m_OperationFilter);
            m_Context = CommunicationContextFixture.AfterRouteResolve;
        }

        [Test]
        public void Should_respond_not_found_when_no_operations_are_eligable()
        {
            m_OperationCreator.Stub(m => m.CreateForHandler(null)).IgnoreArguments().Return(new IOperation[0]);
            m_OperationFilter.Stub(m => m.ApplyFilter(null, null)).IgnoreArguments().Return(new IOperation[0]);
            
            PipelineContinuation continuation = m_OperationCandidateGenerator.Process(m_Context);
            
            Assert.That(continuation, Is.EqualTo(PipelineContinuation.RenderNow));
            Assert.That(m_Context.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(404));
        }
        
        [Test]
        public void Should_add_candidate_operation_to_pipeline_data()
        {
            IOperation operation = MockRepository.GenerateMock<IOperation>();
            m_OperationCreator.Stub(m => m.CreateForHandler(null)).IgnoreArguments().Return(new IOperation[0]);
            m_OperationFilter.Stub(m => m.ApplyFilter(null, null)).IgnoreArguments().Return(new[] { operation });
            
            PipelineContinuation continuation = m_OperationCandidateGenerator.Process(m_Context);
            
            Assert.That(continuation, Is.EqualTo(PipelineContinuation.Continue));
            Assert.That(m_Context.PipelineData.CandidateOperations, Contains.Item(operation));
        }
    }
}