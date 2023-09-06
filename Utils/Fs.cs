using System.Reflection;
using System.Security.Cryptography;

namespace Utils;

public static class Fs
{
    public static string ApplicationPath => AppContext.BaseDirectory;
    public static string MyDocumentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    
    public static IEnumerable<FileInfo> GetFilesInFolder(string folder, bool recurse = false, string filter = "*.*")
    {
        DirectoryInfo currentDir = new DirectoryInfo(folder.Trim());
        IEnumerable<FileInfo> files = new List<FileInfo>();
        try
        {
            files = currentDir.EnumerateFiles(filter, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        catch { /*return empty if error, like if path not found*/ } 
        
        return files;
    }

    /// <summary>
    /// Saves a string to MyDocuments/dirName/fileName. Easy way to save app data as text.
    /// N.B: Overwrites file
    /// </summary>
    /// <param name="text">Defaults to application name</param>
    /// <param name="fileName">Name of file</param>
    /// <param name="dirName">Defaults to "data.bin"</param>
    public static bool SaveFileToMyDocuments(string text, string fileName = "", string dirName = "")
    {
        try
        {
            var myDocumentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dirName.IsNullOrWhiteSpace())
            {
                dirName = Assembly.GetEntryAssembly()?.GetName().Name ?? "ADirName";
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                fileName = "data.bin";
            }

            if(!Directory.Exists(Path.Combine(myDocumentsDir, dirName)))
                Directory.CreateDirectory(Path.Combine(myDocumentsDir, dirName));

            File.WriteAllText(Path.Combine(myDocumentsDir, dirName, fileName), text);

            return true;
        } 
        catch  
        { 
            return false;
        }
    }

    /// <summary>
    /// Reads text from MyDocuments/dirName/fileName. Easy way to load app data as text.
    /// </summary>
    /// <param name="fileName">Defaults to application name</param>
    /// <param name="dirName">Defaults to "data.bin"</param>
    public static (bool success, string data) ReadFileFromMyDocuments(string fileName = "", string dirName = "")
    {
        try
        {
            var myDocumentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dirName.IsNullOrWhiteSpace())
            {
                dirName = Assembly.GetEntryAssembly()?.GetName().Name ?? "ADirName";
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                fileName = "data.bin";
            }

            string res = File.ReadAllText(Path.Combine(myDocumentsDir, dirName, fileName));
            return (true, res);
        }
        catch
        {
            return (false, string.Empty);
        }
    }

    // Compare two files using hash. 
    public static bool AreFilesEqual(string file1, string file2)
    {
        FileInfo fileInfo1 = new FileInfo(file1);
        FileInfo fileInfo2 = new FileInfo(file2);

        return AreFilesEqual(fileInfo1, fileInfo2);
    }

    // Compare two files using hash. 
    public static bool AreFilesEqual(FileInfo fileInfo1, FileInfo fileInfo2)
    {
        if (!fileInfo1.Exists && !fileInfo2.Exists)
            return true;
        if ((!fileInfo1.Exists && fileInfo2.Exists) || (fileInfo1.Exists && !fileInfo2.Exists))
            return false;
        if (fileInfo1.Length != fileInfo2.Length)
            return false;

        using FileStream file1Stream = fileInfo1.OpenRead();
        using FileStream file2Stream = fileInfo2.OpenRead();
        byte[] firstHash = MD5.Create().ComputeHash(file1Stream);
        byte[] secondHash = MD5.Create().ComputeHash(file2Stream);
        for (int i = 0; i < firstHash.Length; i++)
        {
            if (i >= secondHash.Length || firstHash[i] != secondHash[i])
                return false;
        }

        return true;
    }

}
