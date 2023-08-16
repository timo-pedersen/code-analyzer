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
//using CodeAnalyzer.Data;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Documents;
using Microsoft.CodeAnalysis.CSharp;
using WpfAnalyserGUI.FlowDoc;
using System.Windows.Controls;
using CodeAnalyzer.Data;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace WpfAnalyserGUI.VMs
{
    internal class SolutionComparer  : INotifyPropertyChanged
    {
        #region Backing vars ----------------------------
        private string m_SolutionPath1 = "";
        private string m_SolutionPath2 = "";
        private string m_SolutionText1 = "";
        private string m_SolutionText2 = "";
        private int m_ProjectCount1;
        private int m_RhinoDocsCount1;
        private int m_TotalDocsCount1;
        private int m_ProjectCount2;
        private int m_RhinoDocsCount2;
        private int m_TotalDocsCount2;
        #endregion Backing vars --------------------------

        #region RelayCommands ------------------------------
        public ICommand BrowseSolutionCommand { get; }
        #endregion Commands --------------------------------

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

        public int ProjectCount1
        {
            get => m_ProjectCount1;
            set
            {
                m_ProjectCount1 = value;
                OnPropertyChanged();
            }
        }

        public int RhinoDocsCount1
        {
            get => m_RhinoDocsCount1;
            set
            {
                m_RhinoDocsCount1 = value;
                OnPropertyChanged();
            }
        }

        public int TotalDocsCount1
        {
            get => m_TotalDocsCount1;
            set
            {
                m_TotalDocsCount1 = value;
                OnPropertyChanged();
            }
        }

        public int ProjectCount2
        {
            get => m_ProjectCount2;
            set
            {
                m_ProjectCount2 = value;
                OnPropertyChanged();
            }
        }

        public int RhinoDocsCount2
        {
            get => m_RhinoDocsCount2;
            set
            {
                m_RhinoDocsCount2 = value;
                OnPropertyChanged();
            }
        }

        public int TotalDocsCount2
        {
            get => m_TotalDocsCount2;
            set
            {
                m_TotalDocsCount2 = value;
                OnPropertyChanged();
            }
        }



        #endregion ------------------------------------------

        public SolutionComparer()
        {
            BrowseSolutionCommand = new RelayCommand(BrowseSolution);
        }

        #region Command Implementations -------------------------

        // obj = 1 or 2, referring to solution number
        private async void BrowseSolution(object? obj)
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
            Action<int, string> setText = (i, s) =>
            {
                if (i == 1)
                    SolutionText1 = s;
                else
                    SolutionText2 = s;
            };
            // ReSharper restore ConvertToLocalFunction

            (string path, bool ok) = Dlg.OpenSelectFileBrowser(getPath(solutionNo), "Select solution");

            if (!ok)
                return;

            setPath(solutionNo,  path);

            List<string> solutionResult = await ScanSolution(getPath(solutionNo), solutionNo);
            setText(solutionNo, string.Join(Constants.Const.NL, solutionResult));
        }

        #endregion Command Implementations ----------------------

        /// <summary>
        /// Returns a list of solutions, projects and documents, ready for printing or processing.
        /// Sorted in alphabetical order.
        /// Ex:
        ///     MyProject1.csproj
        ///     MyDocument1.cs
        ///     MyDocument2.cs
        ///     MyProject2.csproj
        ///     MyDocument1.cs
        /// </summary>
        /// <param name="solutionData"></param>
        /// <returns></returns>
        public async Task<List<string>> ScanSolution(string solutionPath, int solutionNo)
        {
            List<string> result = new List<string>();

            var sw = Stopwatch.StartNew();
            CodeAnalyzer.Data.Solution slnData = await Analyzer.GetAllTestInSolutionAsync(solutionPath/*, progress, progressMax*/);
            sw.Stop();

            int docCount = 0;
            int rhinoCount = 0;
            foreach (var project in slnData.Projects.OrderBy(x => x.Name))
            {
                result.Add(project.Name);
                foreach (var projectDocument in project.Documents.OrderBy(x => x.Name))
                {
                    if(projectDocument.IsRhino)
                    {
                        result.Add("  - " + projectDocument.Name);
                        rhinoCount++;
                    }
                    else
                        result.Add("    " + projectDocument.Name);

                    docCount++;
                }
            }

            if(solutionNo == 1)
            {
                ProjectCount1 = slnData.ProjectCount;
                TotalDocsCount1 = docCount;
                RhinoDocsCount1 = rhinoCount;
            }
            else
            {
                ProjectCount2 = slnData.ProjectCount;
                TotalDocsCount2 = docCount;
                RhinoDocsCount2 = rhinoCount;
            }

            return result;
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
