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
using CodeAnalyzer;
using System.Windows.Threading;
using CodeAnalyzer.Data;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Documents;
using Microsoft.CodeAnalysis.CSharp;
using WpfAnalyserGUI.FlowDoc;
using System.Windows.Controls;

namespace WpfAnalyserGUI.VMs
{
    internal class SolutionComparer  : INotifyPropertyChanged
    {
        private string m_SolutionPath1 = "";
        private string m_SolutionPath2 = "";
        private string m_SolutionText1 = "";
        private string m_SolutionText2 = "";

        #region RelayCommands ------------------------------
        public ICommand BrowseSolutionCommand { get; }
        #endregion Commands

        #region Observables ---------------------------------

        public string SolutionPath1
        {
            get => m_SolutionPath1;
            set
            {
                m_SolutionPath1 = value;
                OnPropertyChanged();
            }
        }

        public string SolutionPath2
        {
            get => m_SolutionPath2;
            set
            {
                m_SolutionPath2 = value;
                OnPropertyChanged();
            }
        }

        public string SolutionText1
        {
            get => m_SolutionText1;
            set
            {
                m_SolutionText1 = value;
                OnPropertyChanged();
            }
        }

        public string SolutionText2
        {
            get => m_SolutionText2;
            set
            {
                m_SolutionText2 = value;
                OnPropertyChanged();
            }
        }



        #endregion ------------------------------------------

        public SolutionComparer()
        {
            BrowseSolutionCommand = new RelayCommand(BrowseSolution);
        }

        private void BrowseSolution(object? obj)
        {
            int solutionNo = Convert.ToInt32(obj);

            if (solutionNo < 1 || solutionNo > 2)
                return;

            // ReSharper disable ConvertToLocalFunction
            Func<int, string> getPath = i => i == 1 ? SolutionPath1 : SolutionPath2;
            Action<int, string> setPath = (i, s) =>
            {
                if (i == 1)
                    SolutionPath1 = s;
                else
                    SolutionPath2 = s;
            };
            // ReSharper restore ConvertToLocalFunction

            (string path, bool ok) = Dlg.OpenSelectFileBrowser(getPath(solutionNo), "Select solution");

            if (!ok)
                return;

            setPath(solutionNo,  path);
        }


        #region INotifyPropertyChanged ------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion ------------------------------------------
    }
}
