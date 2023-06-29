using System.Collections.Specialized;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    [TestFixture]
    public class RouteResolverTest
    {
        private RouteResolver m_RouteResolver;

        private ICommunicationContext m_CommunicationContextStub;

        private IUriTemplateMatcher m_UriTemplateMatcherStub;

        private RouteTable m_RouteTable;

        [SetUp]
        public void Setup()
        {
            m_RouteTable = new RouteTable();
            m_UriTemplateMatcherStub = Substitute.For<IUriTemplateMatcher>();
            m_CommunicationContextStub = CommunicationContextFixture.Empty;

            m_RouteTable.AddRoute(new RouteData());

            m_RouteResolver = new RouteResolver(m_RouteTable, m_UriTemplateMatcherStub);
        }

        [Test]
        public void Should_return_404_Not_Found_when_no_route_matches()
        {
            m_UriTemplateMatcherStub.MatchesUriTemplate(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<NameValueCollection>())
                .Returns(false);

            m_RouteResolver.Process(m_CommunicationContextStub);

            Assert.That(m_CommunicationContextStub.PipelineData.OperationResult.HttpStatusCode, Is.EqualTo(404));
        }
        
        [Test]
        public void Should_render_now_when_no_route_matches()
        {
            m_UriTemplateMatcherStub.MatchesUriTemplate(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<NameValueCollection>())
                .Returns(false);

            PipelineContinuation continuation = m_RouteResolver.Process(m_CommunicationContextStub);

            Assert.That(continuation, Is.EqualTo(PipelineContinuation.RenderNow));
        }
        
        [Test]
        public void Should_continue_pipeline_when_route_is_found()
        {
            m_UriTemplateMatcherStub.MatchesUriTemplate(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<NameValueCollection>())
                .Returns(true);

            PipelineContinuation continuation = m_RouteResolver.Process(m_CommunicationContextStub);

            Assert.That(continuation, Is.EqualTo(PipelineContinuation.Continue));
        }

        [Test]
        public void Should_set_uri_template_parameters_when_route_is_found()
        {
            NameValueCollection nameValueCollection = null;
            m_UriTemplateMatcherStub.MatchesUriTemplate(Arg.Any<string>(), Arg.Any<string>(), out Arg.Any<NameValueCollection>())
                .Returns(x => 
                {
                    nameValueCollection = (NameValueCollection)x[2];
                    return true; 
                });

            m_RouteResolver.Process(m_CommunicationContextStub);

            Assert.That(m_CommunicationContextStub.PipelineData.UriTemplateParameters, Is.SameAs(nameValueCollection));
        }
    }
}