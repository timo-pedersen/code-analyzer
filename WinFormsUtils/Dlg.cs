namespace WinFormUtils;

public static class Dlg
{
    public static (string, bool) OpenFolderBrowser(string startPath)
    {

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select solution folder...",
            UseDescriptionForTitle = true,
            SelectedPath = startPath,
            InitialDirectory = startPath,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return (dialog.SelectedPath, true);
        }

        return (startPath, false);
    }
}
