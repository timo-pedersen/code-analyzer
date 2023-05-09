using System.Collections.Specialized;
using System.Linq;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel
{
    [TestFixture]
    public class UriParametersFilterTest
    {
        private ICommunicationContext m_CommunicationContextMock;
        private IOperation m_OperationStub;
        private UriParametersFilter m_Filter;
        private IInputMember m_InputMemberStub;

        [SetUp]
        public void Setup()
        {
            m_CommunicationContextMock = CommunicationContextFixture.Empty;
            m_CommunicationContextMock.PipelineData.UriTemplateParameters = new NameValueCollection();

            m_InputMemberStub = MockRepository.GenerateStub<IInputMember>();
            
            m_OperationStub = MockRepository.GenerateStub<IOperation>();

            m_Filter = new UriParametersFilter();
        }

        [Test]
        public void Should_match_all_operation_when_route_has_no_template_parameters()
        {
            bool operationIsMatch = m_Filter.IsMatch(m_OperationStub, m_CommunicationContextMock);

            Assert.That(operationIsMatch, Is.True);
        }

        [Test]
        public void Should_not_match_when_template_parameters_does_not_match_input_members()
        {
            IOperation fooWithBarStringParameter = OperationFixture.FooWithBarStringParameter;
            m_CommunicationContextMock.PipelineData.UriTemplateParameters.Add("not_named_bar", "abc");
            m_InputMemberStub.Stub(m => m.TrySetFromString(null, null)).IgnoreArguments().Return(false);
            
            bool operationIsMatch = m_Filter.IsMatch(fooWithBarStringParameter, m_CommunicationContextMock);

            Assert.That(operationIsMatch, Is.False);
        }
        
        [Test]
        public void Should_match_when_all_template_parameters_match_input_members()
        {
            IOperation operationWithTwoParams = OperationFixture.FooWithBarAndBazStringParameters;
            m_CommunicationContextMock.PipelineData.UriTemplateParameters.Add("bar", "ccc");
            m_CommunicationContextMock.PipelineData.UriTemplateParameters.Add("baz", "bbb");
            operationWithTwoParams.Inputs.ToList()[0].Stub(i => i.TrySetFromString("bar", "ccc")).Return(true);
            operationWithTwoParams.Inputs.ToList()[1].Stub(i => i.TrySetFromString("baz", "bbb")).Return(true);

            bool operationIsMatch = m_Filter.IsMatch(operationWithTwoParams, m_CommunicationContextMock);

            Assert.That(operationIsMatch, Is.True);
        }
    }
}