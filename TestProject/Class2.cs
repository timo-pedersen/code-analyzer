namespace TestProject;

 // A class to be mocked

public class Class2 : IClass2
{
    public string Name1 { get; set; }
    public string Name2 { get; set; }
    public int SomeEventCallCount { get; set; } = 0;

    public event EventHandler<SomeEvent2Args> SomeEvent;

    public Class2(string name1, string name2)
    {
        Name1 = name1;
        Name2 = name2;
    }

    public void FireSomeEvent()
    {
        SomeEvent?.Invoke(this, new SomeEvent2Args("Class2: Some event happened"));
        SomeEventCallCount++;
    }

    public string GetFullName1()
    {
        FireSomeEvent();
        return Name1 + " " + Name2;
    }

    public string GetFullName2()
    {
        FireSomeEvent();
        return Name2 + ", " + Name1;
    }
}

