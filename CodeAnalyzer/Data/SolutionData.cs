using System.Collections.ObjectModel;

namespace CodeAnalyzer.Data;

public class Solution
{
    public string Path { get; }

    public string Name
    {
        get => System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    public bool Loaded { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public ObservableCollection<Project> Projects { get; } = new();

    /// <summary>
    /// Time to load in seconds
    /// </summary>
    public double TimeToLoad { get; set; }

    public int Matches
    {
        get
        {
            int count = 0;
            foreach (Project data in Projects)
            {
                count += data.Matches;
            }

            return count;
        }
    }

    public Solution(string path)
    {
        Path = path;
    }

    public int ProjectCount
    {
        get => Loaded ? Projects.Count : 0;
    }

    public override string ToString()
    {
        return $"{Name}, Projects: {Projects.Count}";
    }

}