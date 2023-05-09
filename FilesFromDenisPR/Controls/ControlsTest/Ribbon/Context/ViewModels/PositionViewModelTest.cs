#if !VNEXT_TARGET
using System.IO.Packaging;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class PositionViewModelTest
    {
        private ITargetService m_TargetServiceMock;
        private ITarget m_TargetMock;
        private IGlobalCommandService m_GlobalCommandService;
        
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string s = PackUriHelper.UriSchemePack;
        }

        [SetUp]
        public void Setup()
        {
            m_TargetMock = Substitute.For<ITarget>();
            
            m_TargetServiceMock = Substitute.For<ITargetService>();
            m_TargetServiceMock.CurrentTarget.Returns(m_TargetMock);
            
            m_GlobalCommandService = Substitute.For<IGlobalCommandService>();

            TestHelper.AddService<ITargetService>(m_TargetServiceMock);
            TestHelper.AddService<IGlobalCommandService>(m_GlobalCommandService);
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
        }

        [Test]
        public void SetPositionOnPcProject()
        {
            var viewModel = new PositionViewModel();
            
            m_TargetMock.Id.Returns(TargetPlatform.Windows);
            
            viewModel.XOne = 5.5;
            viewModel.YOne = 5.5;
            viewModel.XTwo = 10.5;
            viewModel.YTwo = 10.5;

            Assert.That(viewModel.XOne, Is.EqualTo(5.5));
            Assert.That(viewModel.YOne, Is.EqualTo(5.5));
            Assert.That(viewModel.XTwo, Is.EqualTo(10.5));
            Assert.That(viewModel.YTwo, Is.EqualTo(10.5));
        }

        [Test]
        public void SetPositionOnNonPcProject()
        {
            var viewModel = new PositionViewModel();

            m_TargetMock.Id.Returns(TargetPlatform.WindowsCE);

            viewModel.XOne = 5.5;
            viewModel.YOne = 5.5;
            viewModel.XTwo = 10.5;
            viewModel.YTwo = 10.5;

            Assert.That(viewModel.XOne, Is.EqualTo(5));
            Assert.That(viewModel.YOne, Is.EqualTo(5));
            Assert.That(viewModel.XTwo, Is.EqualTo(10));
            Assert.That(viewModel.YTwo, Is.EqualTo(10));
        }
    }
}
#endif
