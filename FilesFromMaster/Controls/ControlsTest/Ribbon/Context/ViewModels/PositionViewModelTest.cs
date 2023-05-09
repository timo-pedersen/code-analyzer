using System.IO.Packaging;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_TargetMock = MockRepository.GenerateMock<ITarget>();
            
            m_TargetServiceMock = MockRepository.GenerateMock<ITargetService>();
            m_TargetServiceMock
                .Expect(targetServiceMock => targetServiceMock.CurrentTarget)
                .Return(m_TargetMock);
            
            m_GlobalCommandService = MockRepository.GenerateMock<IGlobalCommandService>();

            TestHelper.AddService<ITargetService>(m_TargetServiceMock);
            TestHelper.AddService<IGlobalCommandService>(m_GlobalCommandService);
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();
        }

        [Test]
        public void SetPositionOnPcProject()
        {
            var viewModel = new PositionViewModel();
            
            m_TargetMock
                .Expect(target => target.Id)
                .Return(TargetPlatform.Windows);
            
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

            m_TargetMock
                .Expect(target => target.Id)
                .Return(TargetPlatform.WindowsCE);

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