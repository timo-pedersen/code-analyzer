using System.Windows.Forms;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Controls.Screen.Bindings;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Design.Bindings;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen.Bindings
{
    [TestFixture]
    public class LocalPropertyBindingProviderTests
    {
        private LocalPropertyBindingProvider m_Provider;

        [SetUp]
        public void SetUp()
        {
            m_Provider = new LocalPropertyBindingProvider();
        }

        [Test]
        public void ProvideWinFormsBinding()
        {
            // ARRANGE
            BindingSourceDescription bindingSourceDescription = new LocalPropertyBindingSourceDescription("Level");
            var screen = MockRepository.GenerateStub<IScreen>();

            // ACT
            Binding dynamicBinding = m_Provider.ProvideWinFormsBinding(bindingSourceDescription, "Value", screen, null, true);

            // ASSERT
            Assert.That(dynamicBinding, Is.Not.Null);
            Assert.That(dynamicBinding.DataSource, Is.EqualTo(screen));
            Assert.That(dynamicBinding.BindingMemberInfo.BindingMember, Is.EqualTo(bindingSourceDescription.Name));
        }
    }
}
