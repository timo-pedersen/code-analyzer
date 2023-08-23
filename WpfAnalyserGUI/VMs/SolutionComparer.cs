﻿using System;
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
using System.IO.Enumeration;
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
using WpfAnalyserGUI.Reports;

namespace WpfAnalyserGUI.VMs
{
    internal class SolutionComparer  : INotifyPropertyChanged
    {
        #region Backing vars ----------------------------
        private string m_SolutionPath1 = "d:\\git4\\vNextRef";
        private string m_SolutionPath2 = "d:\\git4\\vNext";
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
        public ICommand ScanSolutionCommand { get; }
        public ICommand GenerateFullCvsCommand { get; } 
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

        private Solution? _solution1;
        private Solution? _solution2;

        public SolutionComparer()
        {
            BrowseSolutionCommand = new RelayCommand(BrowseSolution);
            ScanSolutionCommand = new RelayCommand(ScanSolution);
            GenerateFullCvsCommand = new RelayCommand(GenerateFullCvs);
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
            // ReSharper restore ConvertToLocalFunction

            (string path, bool ok) = Dlg.OpenSelectFileBrowser(getPath(solutionNo), "Select solution");

            if (!ok)
                return;

            setPath(solutionNo,  path);
        }

        // obj = 1 or 2, referring to solution number
        private async void ScanSolution(object? obj)
        {
            int solutionNo = Convert.ToInt32(obj);

            if (solutionNo < 1 || solutionNo > 2)
                return;

            // ReSharper disable ConvertToLocalFunction
            Func<int, string> getPath = i => i == 1 ? SolutionPath1 : SolutionPath2;
            Action<int, string> setText = (i, s) =>
            {
                if (i == 1)
                    SolutionText1 = s;
                else
                    SolutionText2 = s;
            };
            // ReSharper restore ConvertToLocalFunction

            List<string> solutionResult = await DoScanSolution(getPath(solutionNo), solutionNo);
            setText(solutionNo, string.Join(Constants.Const.NL, solutionResult));
        }

        private async void GenerateFullCvs(object? obj)
        {
            (string savePath, bool ok) = Dlg.OpenSaveFileBrowser(Fs.MyDocumentsDir, "Save CVS report");
            if (!ok) return;

            string cvs = await GenerateFileReport(SolutionPath1, SolutionPath2);

            await File.WriteAllTextAsync(savePath, cvs);
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
        /// <param name="solutionPath">Path to solution</param>
        /// <param name="solutionNo">Scan solution 1 or 2</param>
        /// <returns></returns>
        public async Task<List<string>> DoScanSolution(string solutionPath, int solutionNo)
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


        /// <summary>
        /// Generate a cvs doc based on the two solutions
        /// Will automatically fetch Neo and vNext solutions from corresponding folders
        ///
        /// Fields
        ///     FileName
        ///     Path1
        ///     Path2
        ///     FileMoved (y/blank)
        ///     Size1
        ///     Size2
        ///     SizeDiff (Size2-Size1)
        ///     ExistsInNeo1
        ///     ExistsInvNextTargets1
        ///     FileIsRhino1
        ///     Neo2
        ///     ExistsInvNextTargets2
        ///     ExistsInFileIsRhino2
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateFileReport(string folder1, string folder2)
        {
            if (folder1.EndsWith(".sln"))
                folder1 = Path.GetDirectoryName(folder1);
            if (folder2.EndsWith(".sln"))
                folder2 = Path.GetDirectoryName(folder2);

            const string neo = "Neo.sln";
            const string vNextTargets = "vNextTargets.sln";

            string neoPath1 = Path.Combine(folder1, neo);
            string neoPath2 = Path.Combine(folder2, neo);
            string vNextTargetsPath1 = Path.Combine(folder1, vNextTargets);
            string vNextTargetsPath2 = Path.Combine(folder2, vNextTargets);

            // Load solutions. This will take a while. 2-3 minutes.
            // Would be nice to run this in parallel, but that gives spurious conflicts.
            var sw = Stopwatch.StartNew();
            Solution neoSln1 = await Analyzer.GetAllTestInSolutionAsync(neoPath1);
            Solution neoSln2 = await Analyzer.GetAllTestInSolutionAsync(neoPath2);
            Solution vNextTargetsSln1 = await Analyzer.GetAllTestInSolutionAsync(vNextTargetsPath1);
            Solution vNextTargetsSln2 = await Analyzer.GetAllTestInSolutionAsync(vNextTargetsPath2);
            sw.Stop();

            // Now we have all projects and documents alphabetically sorted

            // The report we will return
            var report = new List<FileReport>();

            // SCAN vNextTargets 1 (assumed to be reference solution)
            foreach (var proj in vNextTargetsSln1.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Path1 = document.Path.Substring(folder1.Length),
                            ExistsInvNextTargets1 = true,
                        };

                        report.Add(fr);
                    }
                    else
                    {
                        foreach (var fr in found)
                        {
                            fr.Error += $"Duplicate vNextTargets1 {document.Path}";
                        }
                    }
                }
            }

            // SCAN Neo 1
            foreach (var proj in neoSln1.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Path1 = document.Path.Substring(folder1.Length),
                            ExistsInNeo1 = true,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.ExistsInNeo1 = true;
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Error += "Found more than one when scanning Neo1. ";
                        }
                    }
                }
            }

            // SCAN vNextTargets 2
            foreach (var proj in vNextTargetsSln2.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Path2 = document.Path.Substring(folder2.Length),
                            ExistsInvNextTargets2 = true,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Path2 = document.Path.Substring(folder2.Length);
                        fr.ExistsInvNextTargets2 = true;
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Error += "Found more than one when scanning vNextTargets2. ";
                        }
                    }
                }
            }


            // SCAN Neo 2
            foreach (var proj in neoSln2.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Path2 = document.Path.Substring(folder2.Length),
                            ExistsInNeo2 = true,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Path2 = document.Path.Substring(folder2.Length);
                        fr.ExistsInNeo2 = true;
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Error += "Found more than one when scanning Neo2. ";
                        }
                    }
                }
            }


            return report.ToCvs();
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
