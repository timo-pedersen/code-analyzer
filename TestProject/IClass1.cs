namespace TestProject;

public interface IClass1
{
    event EventHandler<SomeEventArgs> SomeEvent;
    int SomeEventCallCount { get; set; }
    int AddTwoNumbers(int a, int b);
    void MethodThatRaisesSomeEvent();
    bool AnotherMethodThatRaisesSomeEvent(bool b);
    int LengthOfSomeString(string s);
}