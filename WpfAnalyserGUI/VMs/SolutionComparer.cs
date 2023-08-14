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
        private string _solutionPath1;
        private string _solutionPath2;

        #region Observables ---------------------------------

        public string SolutionPath1
        {
            get => _solutionPath1;
            set
            {
                _solutionPath2 = value;
                OnPropertyChanged();
            }
        }

        public string SolutionPath2
        {
            get => _solutionPath2;
            set
            {
                _solutionPath2 = value;
                OnPropertyChanged();
            }
        }

        #endregion ------------------------------------------

        



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
