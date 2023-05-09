using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    public class InputMemberFixture
    {
        public static IInputMember BarString
        {
            get
            {
                var im = MockRepository.GenerateStub<IInputMember>();
                im.Stub(s => s.Name).Return("bar");
                im.Stub(s => s.InputType).Return(typeof(string));
                return im;
            }
        }

        public static IInputMember BazString
        {
            get
            {
                var im = MockRepository.GenerateStub<IInputMember>();
                im.Stub(s => s.Name).Return("baz");
                im.Stub(s => s.InputType).Return(typeof(string));
                return im;
            }
        }
    }
}