using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CodeAnalyzer.Data;

public class Solution : Data, INotifyPropertyChanged
{
    private bool _loaded { get; set; } = false;
    public bool Loaded
    {
        get => _loaded;
        set
        {
            _loaded = value;
            OnPropertyChanged();
        }
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Project> _projects = new();
    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set
        {
            _projects = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Time to load in seconds
    /// </summary>
    public double TimeToLoad { get; set; }

    public Solution(string path) : base(path){}

    public int ProjectCount => Loaded ? Projects.Count : 0;

    public override string ToString()
    {
        return $"{Name}, Projects: {Projects.Count}";
    }

    public void FirePropertyChanged()
    {
        OnPropertyChanged(nameof(Solution));
    }
}