using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Analyzer;
using WpfAnalyserGUI.Annotations;
using WpfAnalyzerGUI.Commands;

namespace WpfAnalyzerGUI.VMs;

internal class MainVM : INotifyPropertyChanged
{
    public ICommand BrowseFolderCommand { get; }
    //public ICommand ScanCommand { get; }

    public MainVM()
    {
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        //ScanCommand = RelayCommand();
    }

    private string _folderPath = "d:\\git3\\iXDeveloper\\";
    public string FolderPath
    {
        get => _folderPath;
        set
        {
            _folderPath = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void BrowseFolder(object? o)
    {
        (string path, bool ok) = Util.OpenFolderBrowser(FolderPath);
        if(ok) FolderPath = path;
    }
}

