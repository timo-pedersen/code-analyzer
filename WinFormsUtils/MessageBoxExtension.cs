namespace WinFormUtils;

public static class MessageBoxExtension
{
    public static void ShowList<T>(this MessageBox me, List<T> list)
    {
        var message = string.Join(Environment.NewLine, list);
        MessageBox.Show(message);
    }
}
