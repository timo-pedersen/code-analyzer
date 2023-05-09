using Neo.ApplicationFramework.Tools.WebServer.WebFramework.OperationModel;
using Rhino.Mocks;

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
                var op = MockRepository.GenerateMock<IOperation>();
                op.Stub(m => m.Name).Return("Foo");
                op.Stub(m => m.Inputs).Return(new[] { InputMemberFixture.BarString });
                op.Stub(m => m.IsAllInputValid).Return(false);
                return op;
            } 
        }

        public static IOperation ParameterLessOperationNamedFoo
        {
            get
            {
                var op = MockRepository.GenerateMock<IOperation>();
                op.Stub(m => m.Name).Return("Foo");
                op.Stub(m => m.Inputs).Return(new IInputMember[0]);
                op.Stub(m => m.IsAllInputValid).Return(true);
                return op;
            }
        }

        public static IOperation FooWithBarStringParameter
        {
            get
            {
                var op = MockRepository.GenerateMock<IOperation>();
                op.Stub(m => m.Name).Return("Foo");
                op.Stub(m => m.Inputs).Return(new[] { InputMemberFixture.BarString });
                op.Stub(m => m.IsAllInputValid).Return(true);
                op.Stub(m => m.Invoke()).Return(true);
                return op;
            }
        }

        public static IOperation FooWithBarAndBazStringParameters
        {
            get
            {
                var op = MockRepository.GenerateMock<IOperation>();
                op.Stub(m => m.Name).Return("Foo");
                op.Stub(m => m.Inputs).Return(new[] { InputMemberFixture.BarString, InputMemberFixture.BazString });
                return op;
            }
        }
    }
}