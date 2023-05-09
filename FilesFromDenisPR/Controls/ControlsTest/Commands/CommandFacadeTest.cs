#if!VNEXT_TARGET
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Commands
{
    [TestFixture]
    public class CommandFacadeTest
    {
        [Test]
        public void GetPropertyReturnsDefaultValueWhenObjectIsNull()
        {
            object nullObject = null;
            string dummyPropertyName = "whatever";
            object expectedResult = "expectedResult";

            CommandFacade commandFacade = new CommandFacade();
            object actualResult=commandFacade.GetProperty(nullObject,dummyPropertyName,expectedResult);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetPropertyReturnsDefaultValueForNonExistingPropertyOnObject()
        {
            object propertyObject = "notnull";
            string nonExistingPropertyName = "nonExistingPropertyName";
            object expectedResult = "expectedResult";

            CommandFacade commandFacade = new CommandFacade();
            object actualResult = commandFacade.GetProperty(propertyObject, nonExistingPropertyName, expectedResult);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetPropertyReturnsCorrectPropertyValueForExistingPropertyOnObject()
        {
            string propertyObject = "notnull";
            string lengthPropertyName = "Length";
            object expectedResult = propertyObject.Length;
            string dummyDefaultValue = "dummyDefaultValue";

            CommandFacade commandFacade = new CommandFacade();
            object actualResult = commandFacade.GetProperty(propertyObject, lengthPropertyName, dummyDefaultValue);
            Assert.AreEqual(expectedResult, actualResult);
        }

    }
}
#endif
