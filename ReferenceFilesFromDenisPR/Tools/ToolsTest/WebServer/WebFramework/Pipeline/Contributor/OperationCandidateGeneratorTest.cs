using System.Collections.Generic;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NSubstitute;
using NUnit.Framework;

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
            m_OperationCreator = Substitute.For<OperationCreator>();
            m_OperationFilter = Substitute.For<OperationFilter>();
            m_OperationCandidateGenerator = new OperationCandidateGenerator(m_OperationCreator, m_OperationFilter);
            m_Context = CommunicationContextFixture.AfterRouteResolve;
        }

        [Test]
        public void Should_respond_not_found_when_no_operations_are_eligable()
        {
            m_OperationCreator.CreateForHandler(Arg.Any<ICommunicationContext>()).Returns(new IOperation[0]);
            m_OperationFilter.ApplyFilter(Arg.Any<IEnumerable<IOperation>>(), Arg.Any<ICommunicationContext>()).Returns(new IOperation[0]);
            
            PipelineContinuation continuation = m_OperationCandidateGenerator.Process(m_Context);
            
            Assert.That(continuation, Is.EqualTo(PipelineContinuation.RenderNow));
            Assert.That(m_Context.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(404));
        }
        
        [Test]
        public void Should_add_candidate_operation_to_pipeline_data()
        {
            IOperation operation = Substitute.For<IOperation>();
            m_OperationCreator.CreateForHandler(Arg.Any<ICommunicationContext>()).Returns(new IOperation[0]);
            m_OperationFilter.ApplyFilter(Arg.Any<IEnumerable<IOperation>>(), Arg.Any<ICommunicationContext>()).Returns(new[] { operation });
            
            PipelineContinuation continuation = m_OperationCandidateGenerator.Process(m_Context);
            
            Assert.That(continuation, Is.EqualTo(PipelineContinuation.Continue));
            Assert.That(m_Context.PipelineData.CandidateOperations, Contains.Item(operation));
        }
    }
}