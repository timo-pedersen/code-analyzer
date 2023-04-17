using System;
using System.Windows.Forms;

namespace Analyzer;

public static class Util
{
    public static IEnumerable<FileInfo> GetSolutionsInFolder(string folder)
    {
        DirectoryInfo currentDir = new DirectoryInfo(folder.Trim());
        IEnumerable<FileInfo> slnFiles = currentDir.EnumerateFiles("*.sln", SearchOption.AllDirectories);
        //.Where(x => x.Extension.ToLower() == filter); //

        return slnFiles;
    }

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
