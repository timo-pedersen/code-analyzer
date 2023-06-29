using System.Reflection;

namespace Utils;

public static class Fs
{
    public static string ApplicationPath => AppContext.BaseDirectory;

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

    /// <summary>
    /// Saves a string to MyDocuments/dirName/fileName. Easy way to save app data as text.
    /// N.B: Overwites file
    /// </summary>
    /// <param name="textfile">Defaults to application name</param>
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
    /// <param name="textfile">Defaults to application name</param>
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
}
