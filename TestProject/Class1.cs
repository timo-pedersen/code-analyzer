using System.Diagnostics.Tracing;

namespace TestProject
{
    public class Class1 : IClass1
    {
        public IClass2 TheClass2 { get; }

        public event EventHandler<SomeEventArgs> SomeEvent;

        public int SomeEventCallCount { get; set; } = 0;
        public int Class2EventCount { get; set; } = 0;

        public Class1(Class2 class2)
        {
            TheClass2 = class2;

            TheClass2.SomeEvent += TheClass2OnSomeEvent;
        }

        private void TheClass2OnSomeEvent(object? sender, SomeEventArgs e)
        {
            Class2EventCount++;
        }

        public void OnSomeEvent()
        {
            SomeEvent?.Invoke(this, new SomeEventArgs("Class1: Some event happened"));
            SomeEventCallCount++;
        }

        public void OnSomeOtherEvent()
        {
            SomeEvent?.Invoke(this, new SomeEventArgs("Class1: Some event happened"));
            SomeEventCallCount++;
        }

        // Add two numbers and raise evnt on Class 2
        public int AddTwoNumbers(int a, int b)
        {
            TheClass2.OnSomeEvent();
            return a + b;
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