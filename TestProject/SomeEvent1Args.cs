namespace TestProject;

public class SomeEvent1Args : EventArgs
{
    public string Message { get; }

    public SomeEvent1Args(string msg)
    {
        Message = msg;
    }
}

