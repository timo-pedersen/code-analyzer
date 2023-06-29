#if !VNEXT_TARGET
using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Commands
{
    [TestFixture]
    public class CommandFacadeWithPrimarySelectionTests
    {
        [SetUp]
        public void SetUp()
        {
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

            object actualResult = commandFacade.GetProperty(dummyPropertyName, expectedResult);

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

            object actualResult = commandFacade.GetProperty(nonExistingPropertyName, expectedResult);

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

            object actualResult = commandFacade.GetProperty(lengthPropertyName, dummyDefaultValue);

            Assert.AreEqual(expectedResult, actualResult);
        }

        private void SetupPrimarySelection(object primarySelection)
        {
            IGlobalSelectionService globalSelectionServiceMock = Substitute.For<IGlobalSelectionService>();

            ServiceContainerCF.Instance.Clear();
            TestHelper.AddService(typeof(IGlobalSelectionService), globalSelectionServiceMock);

            globalSelectionServiceMock.PrimarySelection.Returns(primarySelection);
        }
    }
}
#endif
