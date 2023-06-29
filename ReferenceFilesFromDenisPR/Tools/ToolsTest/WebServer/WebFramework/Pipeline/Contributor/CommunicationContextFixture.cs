using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    public class CommunicationContextFixture
    {
        public static ICommunicationContext Empty
        {
            get
            {
                ICommunicationContext cc = Substitute.For<ICommunicationContext>();
                cc.Request.Returns(Substitute.For<IRequest>());
                cc.Response.Returns(Substitute.For<IResponse>());
                cc.PipelineData.Returns(new PipelineData());
                cc.Session.Returns(Substitute.For<ISession>());
                return cc;
            }
        }

        public static ICommunicationContext AfterOperationCandidateGeneration
        {
            get
            {
                var cc = AfterRouteResolve;
                cc.PipelineData.CandidateOperations = new[] { OperationFixture.ParameterLessOperationNamedFoo };
                return cc;
            }
        }

        public static ICommunicationContext AfterRouteResolve
        {
            get
            {
                var cc = Empty;
                cc.PipelineData.Route = new RouteData { ResourceType = typeof(object) };
                return cc;
            }
        }
    }
}