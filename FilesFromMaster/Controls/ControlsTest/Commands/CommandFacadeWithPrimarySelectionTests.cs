using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Commands
{
    [TestFixture]
    public class CommandFacadeWithPrimarySelectionTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            mockRepository = new MockRepository();
        }

        [TearDown]
        public void TestTearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void GetPropertyReturnsDefaultValueWhenPrimarySelectionIsNull()
        {
            object primarySelection = null;
            SetupPrimarySelection(primarySelection);

            string dummyPropertyName = "whatever";
            object expectedResult = "expectedResult";
            CommandFacade commandFacade = new CommandFacade();

            object actualResult;
            using ((mockRepository.Playback()))
            {
                actualResult = commandFacade.GetProperty(dummyPropertyName, expectedResult);
            }

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetPropertyReturnsDefaultValueForNonExistingPropertyOnNonNullPrimarySelection()
        {
            object primarySelection = "notnull";
            SetupPrimarySelection(primarySelection);

            string nonExistingPropertyName = "nonExistingPropertyName";
            object expectedResult = "expectedResult";
            CommandFacade commandFacade = new CommandFacade();

            object actualResult;
            using ((mockRepository.Playback()))
            {
                actualResult = commandFacade.GetProperty(nonExistingPropertyName, expectedResult);
            }

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetPropertyReturnsCorrectPropertyValueForExistingPropertyOnNonNullPrimarySelection()
        {
            string primarySelection = "notnull";
            string dummyDefaultValue = "dummyDefaultValue";
            SetupPrimarySelection(primarySelection);

            string lengthPropertyName = "Length";
            object expectedResult = primarySelection.Length;
            CommandFacade commandFacade = new CommandFacade();

            object actualResult;
            using ((mockRepository.Playback()))
            {
                actualResult = commandFacade.GetProperty(lengthPropertyName, dummyDefaultValue);
            }

            Assert.AreEqual(expectedResult, actualResult);
        }

        private void SetupPrimarySelection(object primarySelection)
        {
            IGlobalSelectionService globalSelectionServiceMock = mockRepository.Stub<IGlobalSelectionService>();

            ServiceContainerCF.Instance.Clear();
            TestHelper.AddService(typeof(IGlobalSelectionService), globalSelectionServiceMock);

            using (mockRepository.Record())
            {
                Expect.Call(globalSelectionServiceMock.PrimarySelection).Repeat.Any().Return(primarySelection);
            }
        }
    }
}