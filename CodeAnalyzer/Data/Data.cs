using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeAnalyzer.Data;

public class Data : IData, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Path { get; }
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    protected Data(string path)
    {
        Path = path;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
