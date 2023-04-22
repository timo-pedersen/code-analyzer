using System;
using System.Configuration.Assemblies;

namespace Utils;

public static class Fs
{
    public static IEnumerable<FileInfo> GetFilesInFolder(string folder, bool recurse = false, string filter = "*.*")
    {
        DirectoryInfo currentDir = new DirectoryInfo(folder.Trim());
        IEnumerable<FileInfo> files = new List<FileInfo>();
        try
        {
            files = currentDir.EnumerateFiles(filter, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        catch { } // return empty if error, like if path not found
        
        return files;
    } 
}
