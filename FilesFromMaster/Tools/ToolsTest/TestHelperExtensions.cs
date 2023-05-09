using Core.Api.Tools;
using Neo.ApplicationFramework.TestUtilities;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools
{
    internal static class TestHelperExtensions
    {

        public static IToolManager AddServiceToolManager(bool runtime)
        {
            IToolManager toolManager = TestHelper.AddServiceStub<IToolManager>();
            toolManager.Stub(x => x.Runtime).Return(runtime);

            return toolManager;
        }

    }
}
