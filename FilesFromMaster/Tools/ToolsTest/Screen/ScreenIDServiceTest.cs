using Core.Api.GlobalReference;
using Core.TestUtilities.Utilities;
using Neo.ApplicationFramework.Common.Runtime.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Screen
{
    [TestFixture]
    public class ScreenIDServiceTest
    {
        private IScreenIDService m_ScreenIdService;
        private ScreenIDXmlFileReader m_ScreenIDXmlFileReaderStub;
        private IGlobalReferenceService m_GlobalReferenceService;

        private static IScreen ScreenStub
        {
            get
            {
                IScreen screen = MockRepository.GenerateStub<IScreen>();
                screen.ScreenID = 1;
                return screen;
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;
            TestHelper.ClearServices();

            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            m_ScreenIDXmlFileReaderStub = MockRepository.GenerateStub<ScreenIDXmlFileReader>();
            m_ScreenIdService = new ScreenIDService(m_ScreenIDXmlFileReaderStub, new LazyWrapper<IGlobalReferenceService>(() => m_GlobalReferenceService));
        }

        [Test]
        public void WhenOpeningScreenByIdAndItExitsItShowsTheScreen()
        {
            string screenName = "screen1";
            IScreen screen = ScreenStub;
            m_ScreenIDXmlFileReaderStub.Stub(x => x.Load()).Return(new[] { new ScreenIDInformation(1, screenName) });
            m_GlobalReferenceService.Stub(x => x.GetObject<IScreen>(ScreenPathHelper.GetPath(screenName))).Return(screen);

            m_ScreenIdService.OpenScreenByID(1);

            screen.AssertWasCalled(x => x.Show());
        }

        [Test]
        public void WhenOpeningScreenByIdAndItDoesntExitsItReturnsWithoutShowingTheScreen()
        {
            IScreen screen = MockRepository.GenerateStub<IScreen>();
            screen.ScreenID = 1;
            m_ScreenIDXmlFileReaderStub.Stub(x => x.Load()).Return(new[] { new ScreenIDInformation(1, "") });
            m_GlobalReferenceService.Stub(x => x.GetObject<IScreen>("")).Return(screen);

            m_ScreenIdService.OpenScreenByID(2);

            screen.AssertWasNotCalled(x => x.Show());
        }

        [Test]
        public void WhenParsingScreenValueToTagValueItTreatsNullAsMinusOne()
        {
            Assert.That(m_ScreenIdService.ParseScreenIDValueToTagValue(null), Is.EqualTo(-1));
        }

        [Test]
        public void CanParseScreenValueToTagValue()
        {
            Assert.That(m_ScreenIdService.ParseScreenIDValueToTagValue(1), Is.EqualTo(1));
        }
    }

}
