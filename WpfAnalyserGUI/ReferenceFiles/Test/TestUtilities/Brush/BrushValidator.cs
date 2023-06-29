using System.Reflection;
using Neo.ApplicationFramework.Utilities.Assertion;

namespace Neo.ApplicationFramework.TestUtilities.Brush
{
    public static class BrushValidator
    {

        public static void AssertBrushesAreEqual(System.Windows.Media.Brush expected, System.Windows.Media.Brush actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                Assert.Fail("Expected brush of type:" + expected.GetType() + "\r\nbut was:" + actual.GetType());
            }

            PropertyInfo[] expectedProperties = expected.GetType().GetProperties();
            foreach (PropertyInfo property in expectedProperties)
            {
                if ((property.Name == "IsFrozen") || (property.Name == "IsSealed") || (property.Name == "Dispatcher"))
                    continue;

                Assert.IsTrue(property.GetValue(expected, null).Equals( property.GetValue(actual, null)));
            }
        }
    }
}
