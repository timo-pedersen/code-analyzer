﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfAnalyserGUI.Annotations;
using WpfAnalyzerGUI.Commands;
using Utils;
using WinFormUtils;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using CodeAnalyzer;

namespace WpfAnalyzerGUI.VMs;

internal class MainVM : INotifyPropertyChanged
{
    public ICommand BrowseFolderCommand { get; }
    public ICommand ScanCommand { get; }

    public ObservableCollection<SolutionData> Solutions { get; } = new ObservableCollection<SolutionData>();
    public ObservableCollection<string> SolutionsFiles { get; } = new ObservableCollection<string>();

    public MainVM()
    {
        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        ScanCommand = new RelayCommand(Scan, ScanCanExecute);
    }

    //private string _folderPath = "d:\\git3\\iXDeveloper\\";
    private string _folderPath = "E:\\git_tpp\\code-analyzer\\";
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

    private void Scan(object? o)
    {
        Stopwatch sw = Stopwatch.StartNew();

        if (ScanCommand is null)
            return;

        List<System.IO.FileInfo> solutions = Fs.GetFilesInFolder(FolderPath, true, "*.sln").ToList();
        solutions.ForEach(solution => { SolutionsFiles.Add(solution.FullName); });

        string collectedMsg = "";
        solutions.ForEach(solution =>
        {
            (SolutionData? data, string msg) = Analyzer.AnalyzeSolution(solution.FullName);
            if(data != null)
                Solutions.Add(data);
            
            collectedMsg += "\r" + msg;
        });
        sw.Stop();
        MessageBox.Show($"Took {sw.ElapsedMilliseconds / 1000} seconds.\r" + collectedMsg);
    }

    private bool ScanCanExecute(object? o)
    {
        return Fs.GetFilesInFolder(FolderPath, true, "*.sln").Any();
    }

}

