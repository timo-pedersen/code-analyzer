using Core.Api.Tools;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools
{
    internal static class TestHelperExtensions
    {
        public static IToolManager AddServiceToolManager(bool runtime)
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Runtime.Returns(runtime);

            return toolManager;
        }
    }
}
