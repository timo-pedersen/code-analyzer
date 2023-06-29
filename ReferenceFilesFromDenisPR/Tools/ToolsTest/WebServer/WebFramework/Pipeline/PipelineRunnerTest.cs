using System;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NSubstitute;
using NUnit.Framework;

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
            m_First = Substitute.For<IPipelineContributor>();
            m_Second = Substitute.For<IPipelineContributor>();
            m_Renderer = Substitute.For<CombinedIPipelineContributorIRenderer>();
        }

        [Test]
        public void Should_execute_next_contributor_when_continuation_is_continue()
        {
            m_First.Process(m_Context).Returns(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.Received().Process(m_Context);
        }
        
        [Test]
        public void Should_not_execute_next_contributor_when_continuation_is_abort()
        {
            m_First.Process(m_Context).Returns(PipelineContinuation.Abort);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.DidNotReceive().Process(m_Context);
        }
        
        [Test]
        public void Should_skip_to_first_redering_contributor_when_continuation_is_render_now()
        {
            m_First.Process(m_Context).Returns(PipelineContinuation.RenderNow);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second, m_Renderer });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Second.DidNotReceive().Process(m_Context);
            m_Renderer.Received().Process(m_Context);
        }

        [Test]
        public void Should_set_response_to_500_Internal_server_error_if_contributor_throws()
        {
            m_First.Process(m_Context).Returns(x => throw new Exception("Dooh"));
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Context.Response.Received().SetErrorResponse(Arg.Is(500), Arg.Any<string>(), Arg.Any<string>());
        }   
        
        [Test]
        public void Should__not_set_error_response_when_pipeline_executes_without_errors()
        {
            m_First.Process(m_Context).Returns(PipelineContinuation.Continue);
            m_Second.Process(m_Context).Returns(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First, m_Second });

            pipelineRunner.ExecutePipeline(m_Context);

            m_Context.Response.DidNotReceiveWithAnyArgs().SetErrorResponse(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Should_give_context_unqiue_id()
        {
            ICommunicationContext contextOne = CommunicationContextFixture.AfterRouteResolve;
            ICommunicationContext contextTwo = CommunicationContextFixture.AfterRouteResolve;
            ICommunicationContext contextThree = CommunicationContextFixture.AfterRouteResolve;
            m_First.Process(Arg.Any<ICommunicationContext>()).Returns(PipelineContinuation.Continue);
            PipelineRunner pipelineRunner = new PipelineRunner(new[] { m_First });

            pipelineRunner.ExecutePipeline(contextOne);
            pipelineRunner.ExecutePipeline(contextTwo);
            pipelineRunner.ExecutePipeline(contextThree);

            contextOne.Received().Id = 1;
            contextTwo.Received().Id = 2;
            contextThree.Received().Id = 3;
        }

        public interface CombinedIPipelineContributorIRenderer : IPipelineContributor, IRenderer
        {
        }
    }
}