namespace WinFormUtils;

public static class Dlg
{
    public static (string, bool) OpenFolderBrowser(string startPath, string description = "Select folder")
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = description,
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

    public static (string, bool) OpenSelectFileBrowser(string startPath, string description = "Select file")
    {
        using var dialog = new OpenFileDialog
        {
            Title = description,
            InitialDirectory = startPath,
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return (dialog.FileName, true);
        }

        return ("", false);
    }

}
