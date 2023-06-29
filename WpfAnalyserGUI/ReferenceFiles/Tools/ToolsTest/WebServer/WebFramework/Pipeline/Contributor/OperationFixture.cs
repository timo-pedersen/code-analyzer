using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using NSubstitute;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework.Pipeline.Contributor
{
    public class OperationFixture
    {
        public static IOperation Valid
        {
          get
          {
              return FooWithBarStringParameter;
          }   
        }       
        
        public static IOperation Invalid
        {
          get
          {
              return FooWithUnsetBarStringParameter;
          }   
        }

        public static IOperation FooWithUnsetBarStringParameter 
        { 
            get
            {
                var op = Substitute.For<IOperation>();
                op.Name.Returns("Foo");
                op.Inputs.Returns(x => new[] { InputMemberFixture.BarString });
                op.IsAllInputValid.Returns(false);
                return op;
            } 
        }

        public static IOperation ParameterLessOperationNamedFoo
        {
            get
            {
                var op = Substitute.For<IOperation>();
                op.Name.Returns("Foo");
                op.Inputs.Returns(new IInputMember[0]);
                op.IsAllInputValid.Returns(true);
                return op;
            }
        }

        public static IOperation FooWithBarStringParameter
        {
            get
            {
                var op = Substitute.For<IOperation>();
                op.Name.Returns("Foo");
                op.Inputs.Returns(x => new[] { InputMemberFixture.BarString });
                op.IsAllInputValid.Returns(true);
                op.Invoke().Returns(true);
                return op;
            }
        }

        public static IOperation FooWithBarAndBazStringParameters
        {
            get
            {
                var op = Substitute.For<IOperation>();
                op.Name.Returns("Foo");
                op.Inputs.Returns(x => new[] { InputMemberFixture.BarString, InputMemberFixture.BazString });
                return op;
            }
        }
    }
}