namespace TestProject;

public interface IClass1
{
    event EventHandler<SomeEvent1Args> SomeEvent;
    int SomeEventCallCount { get; set; }
    int AddTwoNumbers(int a, int b);
    int LengthOfSomeString(string s);

    // Uses class2 -----------------------
    public void SetFirstName(string name);
    public void SetLastName(string name);
    public string GetFullName1(string first, string last);
    public string GetFullName2(string first, string last);
}