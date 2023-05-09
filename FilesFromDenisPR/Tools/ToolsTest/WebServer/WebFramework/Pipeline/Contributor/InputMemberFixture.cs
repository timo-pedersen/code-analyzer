using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    public class InputMemberFixture
    {
        public static IInputMember BarString
        {
            get
            {
                var im = Substitute.For<IInputMember>();
                im.Name.Returns("bar");
                im.InputType.Returns(typeof(string));
                return im;
            }
        }

        public static IInputMember BazString
        {
            get
            {
                var im = Substitute.For<IInputMember>();
                im.Name.Returns("baz");
                im.InputType.Returns(typeof(string));
                return im;
            }
        }
    }
}