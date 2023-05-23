namespace TestProject;

public class Class1 : IClass1
{
    public IClass2 TheClass2 { get; }

    public event EventHandler<SomeEvent1Args> SomeEvent;

    public int SomeEventCallCount { get; set; } = 0;
    public int Class2EventCount { get; set; } = 0;

    public Class1(Class2 class2)
    {
        TheClass2 = class2;

        TheClass2.SomeEvent += TheClass2SomeEventHandler;
    }

    private void TheClass2SomeEventHandler(object? sender, SomeEvent2Args e)
    {
        Class2EventCount++;
    }

    public void OnSomeEvent()
    {
        SomeEvent?.Invoke(this, new SomeEvent1Args("Class1: Some event happened"));
        SomeEventCallCount++;
    }

    // Add two numbers and raise event
    public int AddTwoNumbers(int a, int b)
    {
        OnSomeEvent();
        return a + b;
    }

    public int LengthOfSomeString(string s)
    {
        return s.Length;
    }

    // Uses class2 -----------------------
    public void SetFirstName(string name)
    {
        TheClass2.Name1 = name;
    }

    public void SetLastName(string name)
    {
        TheClass2.Name2 = name;
    }

    public string GetFullName1(string first, string last)
    {
        return TheClass2.GetFullName1();
    }

    public string GetFullName2(string first, string last)
    {
        return TheClass2.GetFullName2();
    }
}