using System.Collections.ObjectModel;

namespace CodeAnalyzer.Data;

public class Project : Data
{
    private bool _loaded;
    public bool Loaded
    {
        get => _loaded;
        set
        {
            _loaded = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Document> Documents { get; } = new();

    public Project(string path) : base(path){}
}