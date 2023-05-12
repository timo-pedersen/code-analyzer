namespace TestProject
{
    public class Class1 : IClass1
    {

        public Class2 TheClass2 { get; set; }

        public event EventHandler<SomeEventArgs> SomeEvent;
        public int SomeEventCallCount { get; set; } = 0;

        public void OnSomeEvent()
        {
            SomeEvent?.Invoke(this, new SomeEventArgs("Class1: Some event happened"));
            SomeEventCallCount++;
        }

        public Class1(Class2 class2)
        {
            TheClass2 = class2;
        }

        public void MethodThatRaisesSomeEvent()
        {
            OnSomeEvent();
        }

        public bool AnotherMethodThatRaisesSomeEvent(bool b)
        {
            OnSomeEvent();
            return b;
        }

        public int LengthOfSomeString(string s)
        {
            return s.Length;
        }
    }
}