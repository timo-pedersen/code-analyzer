using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfAnalyserGUI.Annotations;
using WpfAnalyzerGUI.Commands;
using Utils;
using WinFormUtils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CodeAnalyzer;

namespace WpfAnalyzerGUI.VMs;

internal class MainVM : INotifyPropertyChanged
{
    public ICommand BrowseFolderCommand { get; }
    public ICommand ScanCommand { get; }

    public ObservableCollection<CodeAnalyzer.Data.Solution> Solutions { get; } = new ();

    public int ProgressValue { get; set; }

    public MainVM()
    {
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        ScanCommand = new RelayCommand(Scan, ScanCanExecute);
    }

    //private string _folderPath = "d:\\git3\\iXDeveloper\\";
    private string _folderPath = "D:\\git4\\iXDeveloper\\";
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
        (string path, bool ok) = Dlg.OpenFolderBrowser(FolderPath);
        if(ok) FolderPath = path;
    }

    private async void Scan(object? o)
    {
        IProgress<int> progress = new Progress<int>(percent => { ProgressValue = percent; });

        Stopwatch sw = Stopwatch.StartNew();

        List<FileInfo> solutionFiles = Fs.GetFilesInFolder(FolderPath, true, "*.sln").ToList();

        string collectedMsg = "";

        int p = 0;
        var tasks = new List<Task>();

        foreach (FileInfo solution in solutionFiles)
        {
            tasks.Add(Task.Run(() =>
                {
                    (CodeAnalyzer.Data.Solution slnData, string msg) = Analyzer.AnalyzeSolution(solution.FullName);
                    Solutions.Add(slnData);
                }
            ));
        }

        await Task.WhenAll(tasks);

        ProgressValue = p++;
        //if(msg.Length > 0) {
        //    collectedMsg += "\r" + msg;
        //}

        sw.Stop();
        MessageBox.Show($"Took {sw.ElapsedMilliseconds / 1000} seconds.\r" + collectedMsg);
    }

    private bool ScanCanExecute(object? o)
    {
        return Fs.GetFilesInFolder(FolderPath, true, "*.sln").Any();
    }

}

