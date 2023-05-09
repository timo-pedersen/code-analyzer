using System;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline
{
    [TestFixture]
    public class PipelineRunnerTest
    {
        private ICommunicationContext m_Context;

        private IPipelineContributor m_First;

        private IPipelineContributor m_Second;

        private IPipelineContributor m_Renderer;

        [SetUp]
        public void SetUp()
        {
            m_Context = CommunicationContextFixture.Empty;
            m_First = MockRepository.GenerateMock<IPipelineContributor>();
            m_Second = MockRepository.GenerateMock<IPipelineContributor>();
            m_Renderer = MockRepository.GenerateMock<CombinedIPipelineContributorIRenderer>();
        }

        [Test]
        public void Should_execute_next_contributor_when_continuation_is_continue()
        {
            m_First.Stub(m => m.Process(m_Context)).Return(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.AssertWasCalled(m => m.Process(m_Context));
        }
        
        [Test]
        public void Should_not_execute_next_contributor_when_continuation_is_abort()
        {
            m_First.Stub(m => m.Process(m_Context)).Return(PipelineContinuation.Abort);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.AssertWasNotCalled(m => m.Process(m_Context));
        }
        
        [Test]
        public void Should_skip_to_first_redering_contributor_when_continuation_is_render_now()
        {
            m_First.Stub(m => m.Process(m_Context)).Return(PipelineContinuation.RenderNow);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second, m_Renderer });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.AssertWasNotCalled(m => m.Process(m_Context));
            m_Renderer.AssertWasCalled(m => m.Process(m_Context));
        }

        [Test]
        public void Should_set_response_to_500_Internal_server_error_if_contributor_throws()
        {
            m_First.Stub(m => m.Process(m_Context)).Throw(new Exception("Dooh"));
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Context.Response.AssertWasCalled(m => m.SetErrorResponse(Arg.Is(500), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
        }   
        
        [Test]
        public void Should__not_set_error_response_when_pipeline_executes_without_errors()
        {
            m_First.Stub(m => m.Process(m_Context)).Return(PipelineContinuation.Continue);
            m_Second.Stub(m => m.Process(m_Context)).Return(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Context.Response.AssertWasNotCalled(m => m.SetErrorResponse(Arg<int>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
        }

        [Test]
        public void Should_give_context_unqiue_id()
        {
            ICommunicationContext contextOne = CommunicationContextFixture.AfterRouteResolve;
            ICommunicationContext contextTwo = CommunicationContextFixture.AfterRouteResolve;
            ICommunicationContext contextThree = CommunicationContextFixture.AfterRouteResolve;
            m_First.Stub(m => m.Process(null)).IgnoreArguments().Return(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First });

            pipelineRunner.ExecutePipeline(contextOne);
            pipelineRunner.ExecutePipeline(contextTwo);
            pipelineRunner.ExecutePipeline(contextThree);

            contextOne.AssertWasCalled(m => m.Id = 1);
            contextTwo.AssertWasCalled(m => m.Id = 2);
            contextThree.AssertWasCalled(m => m.Id = 3);
        }

        public interface CombinedIPipelineContributorIRenderer : IPipelineContributor, IRenderer
        {
        }
    }
}