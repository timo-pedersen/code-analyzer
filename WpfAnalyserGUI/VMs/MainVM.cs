using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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
using System.Windows.Threading;
using System.Windows.Controls;

namespace WpfAnalyzerGUI.VMs;

internal class MainVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand BrowseFolderCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand ScanAllCommand { get; }
    public ICommand SelectionChangeCommand { get; }

    public ObservableCollection<CodeAnalyzer.Data.Solution> Solutions { get; } = new ();
    
    #region Observables =========================================================
    private int _progressMax1;
    public int ProgressMax1
    {
        get => _progressMax1;
        set
        {
            _progressMax1 = value;
            OnPropertyChanged();
        }
    }

    private int _progressValue1;
    public int ProgressValue1
    {
        get => _progressValue1;
        set
        {
            _progressValue1 = value;
            OnPropertyChanged();
        }
    }

    private int _progressMax2;
    public int ProgressMax2
    {
        get => _progressMax2;
        set
        {
            _progressMax2 = value;
            OnPropertyChanged();
        }
    }

    private int _progressValue2;
    public int ProgressValue2
    {
        get => _progressValue2;
        set
        {
            _progressValue2 = value;
            OnPropertyChanged();
        }
    }

    #endregion ============================================================

    public MainVM()
    {
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        ScanCommand = new RelayCommand(Scan, ScanCanExecute);
        ScanAllCommand = new RelayCommand(ScanAll, ScanCanExecute);
        SelectionChangeCommand = new RelayCommand(x => SelectedItem = x);
        ProgressMax1 = 100;
        ProgressValue1 = 0;
        ProgressMax2 = 100;
        ProgressValue2 = 0;
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

    private string _selectedSolutionPath => SelectedItem is CodeAnalyzer.Data.Solution sln ? sln.Path : String.Empty;
    public string SelectedSolutionPath
    {
        get => _selectedSolutionPath;
        //set
        //{
        //    _selectedSolutionPath = value;
        //    OnPropertyChanged();
        //}
    }

    private object? _selectedItem;
    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (value is TreeView tv)
            {
                _selectedItem = tv.SelectedItem;
                OnPropertyChanged();
            }
        }
    }

    public CodeAnalyzer.Data.Solution? SelectedSolution
    {
        get => Solutions.FirstOrDefault(x => x.Path == SelectedSolutionPath);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void BrowseFolder(object? o)
    {
        (string path, bool ok) = Dlg.OpenFolderBrowser(FolderPath);
        if(ok) FolderPath = path;

        List<FileInfo> solutionFiles = await GetSolutionFiles(FolderPath);
        
        Solutions.Clear();
        foreach (FileInfo solution in solutionFiles)
        {
            Solutions.Add(new CodeAnalyzer.Data.Solution(solution.FullName));
        }
    }

    private async void ScanAll(object? o)
    {
        IProgress<int> progress1 = new Progress<int>(val => { ProgressValue1 = val; });
        IProgress<int> progress2 = new Progress<int>(val => { ProgressValue2 = val; });
        IProgress<int> progressMax2 = new Progress<int>(val => { ProgressMax2 = val; });

        Stopwatch sw = Stopwatch.StartNew();

        List<FileInfo> solutionFiles = await GetSolutionFiles(FolderPath);

        string collectedMsg = "";

        int p = 1;
        progress1.Report(p);
        var tasks = new List<Task>();

        var dispatcher = Dispatcher.CurrentDispatcher;
        Solutions.Clear();
        foreach (FileInfo solution in solutionFiles)
        {
            tasks.Add(Task.Run(() =>
                {
                    CodeAnalyzer.Data.Solution slnData = Analyzer.AnalyzeSolution(solution.FullName, progress2, progressMax2);
                    progress1.Report(p++);
                    dispatcher.Invoke(() => Solutions.Add(slnData));
                    if (slnData.Message.Length > 0)
                    {
                        collectedMsg += "\r" + slnData.Message;
                    }
                }
            ));
        }

        await Task.WhenAll(tasks);

        sw.Stop();
        MessageBox.Show($"Took {sw.ElapsedMilliseconds / 1000} seconds.\r" + collectedMsg);
    }

    private async void Scan(object? o)
    {
        if (SelectedSolution == null)
            return;

        IProgress<int> progress = new Progress<int>(val => { ProgressValue2 = val; });
        IProgress<int> progressMax = new Progress<int>(val => { ProgressMax2 = val; });

        Stopwatch sw = Stopwatch.StartNew();

        var tasks = new List<Task>();

        var dispatcher = Dispatcher.CurrentDispatcher;

        CodeAnalyzer.Data.Solution slnData = await Analyzer.AnalyzeSolutionAsync(SelectedSolutionPath, progress, progressMax);
        dispatcher.Invoke(() =>
        {
            SelectedSolution.Loaded = true;
            SelectedSolution.Projects = slnData.Projects;
            //OnPropertyChanged(nameof(SelectedSolution));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Solutions)));
            Solutions.
        });

        sw.Stop();
        MessageBox.Show($"Took {sw.ElapsedMilliseconds / 1000} seconds.\r" + slnData.Message);
    }


    private async Task<List<FileInfo>> GetSolutionFiles(string path)
    {
        Task <List<FileInfo>> t =  Task.Run(() =>
        {
            List<FileInfo> solutionFiles = Fs.GetFilesInFolder(path, true, "*.sln")
                //.Where(x => x.Name.Contains("Neo.sln"))
                .ToList();
            ProgressMax1 = solutionFiles.Count;
            ProgressValue1 = 0;
            return solutionFiles;
        });

        await t;
        return t.Result;
    }

    private bool ScanCanExecute(object? o)
    {
        return Fs.GetFilesInFolder(FolderPath, true, "*.sln").Any();
    }

 
}

