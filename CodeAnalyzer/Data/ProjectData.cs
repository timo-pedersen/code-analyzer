using System.Collections.ObjectModel;

namespace CodeAnalyzer.Data;

public class Project
{
    public string Path { get; }

    public string Name
    {
        get => System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    public ObservableCollection<Document> Documents { get; } = new();

    public int Matches
    {
        get
        {
            int count = 0;
            foreach (Document data in Documents)
            {
                count += data.Matches;
            }

            return count;
        }
    }


    public Project(string path)
    {
        Path = path;
    }


}