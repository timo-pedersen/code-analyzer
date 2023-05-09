using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Authentication;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    public class CommunicationContextFixture
    {
        public static ICommunicationContext Empty
        {
            get
            {
                ICommunicationContext cc = MockRepository.GenerateMock<ICommunicationContext>();
                cc.Stub(m => m.Request).Return(MockRepository.GenerateMock<IRequest>());
                cc.Stub(m => m.Response).Return(MockRepository.GenerateMock<IResponse>());
                cc.Stub(m => m.PipelineData).Return(new PipelineData());
                cc.Stub(m => m.Session).Return(MockRepository.GenerateMock<ISession>());
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