namespace TestProject;

public interface IClass2
{
    string Name1 { get; set; }
    string Name2 { get; set; }
    int SomeEventCallCount { get; set; }
    event EventHandler<SomeEventArgs> SomeEvent;
    void OnSomeEvent();
    string GetFullName1();
    string GetFullName2();
}