using System.Windows;

namespace WpfUtils;

public static class MessageBox
{
    public static void ShowList<T>(List<T> list, string header = "Show list")
    {
        var message = string.Join(Environment.NewLine, list);
        System.Windows.MessageBox.Show(message);
    }
}
