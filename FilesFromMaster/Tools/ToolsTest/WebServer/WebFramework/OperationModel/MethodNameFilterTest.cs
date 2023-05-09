using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel
{
    [TestFixture]
    public class MethodNameFilterTest
    {
        private ICommunicationContext m_CommunicationContextMock;

        private IOperation m_OperationStub;

        private MethodNameFilter m_Filter;

        [SetUp]
        public void Setup()
        {
            m_CommunicationContextMock = CommunicationContextFixture.Empty;
            
            m_OperationStub = MockRepository.GenerateStub<IOperation>();

            m_Filter = new MethodNameFilter();
        }

        [Test]
        public void Should_match_if_name_is_equal_to_http_verb()
        {
            AssertIsMatch("POST", "POST", Is.True);
        }

        [Test]
        public void Should_not_match_if_name_does_not_match_http_verb()
        {
            AssertIsMatch("Abc", "GET", Is.False);
        }

        [Test]
        public void Should_match_if_name_is_equal_to_http_verb_but_casing_is_different()
        {
            AssertIsMatch("Get", "GET", Is.True);
        }
        
        [Test]
        public void Should_match_if_name_starts_with_http_verb()
        {
            AssertIsMatch("GetByCountry", "GET", Is.True);
        }

        private void AssertIsMatch(string operationName, string httpVerb, IResolveConstraint isMatchContraint)
        {
            m_OperationStub.Stub(m => m.Name).Return(operationName);
            m_CommunicationContextMock.Request.Stub(m => m.Method).Return(httpVerb);

            bool operationIsMatch = m_Filter.IsMatch(m_OperationStub, m_CommunicationContextMock);

            Assert.That(operationIsMatch, isMatchContraint);
        }
    }
}