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
using System.IO.Enumeration;
using System.Threading.Tasks;
using System.Windows.Automation;
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
        private async Task<string> GenerateFileReport(string folder1, string folder2)
        {
            if (folder1.EndsWith(".sln"))
                folder1 = Path.GetDirectoryName(folder1) ?? "";
            if (folder2.EndsWith(".sln"))
                folder2 = Path.GetDirectoryName(folder2) ?? "";

            if ((folder1 + folder1).IsNullOrWhiteSpace())
                return "ERROR: Invalid folders";

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

            // Scanning the four solutions, starting with vNextTargets 1 (the reference),
            // in order to build up the report.
            // Beware - ugly code below.

            int folderLength1 = folder1.Length + 1;
            int folderLength2 = folder2.Length + 1;
            string documentPath = "";
            // SCAN vNextTargets 1 (assumed to be reference solution)
            foreach (var proj in vNextTargetsSln1.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    documentPath = document.Path[folderLength1..];
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = proj.Name,
                            vNextTargetsPath1 = documentPath,
                            ExistsInvNextTargets1 = true,
                            FileIsRhino1 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment += $"### ERROR: vNextTargets1 - {found.Count} duplicates of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }

            // SCAN Neo 1
            foreach (var proj in neoSln1.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    documentPath = document.Path[folderLength1..];
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = proj.Name,
                            NeoPath1 = documentPath,
                            ExistsInNeo1 = true,
                            FileIsRhino1 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.ExistsInNeo1 = true;
                        fr.Project += $", {proj.Name}";
                        fr.NeoPath1 = documentPath;
                        fr.Comment += $"# Neo1 - File is in both vNextTargets and Neo. ";
                        if(fr.FileIsRhino1 != document.IsRhino)
                            fr.Comment += $"# Neo1 - Rhino status differs. Neo1 doc isRhino: {document.IsRhino}. ";
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment += $"### ERROR: Neo1 - {found.Count} duplicates of {documentPath}. Project: {proj.Name} ";
                        }
                    }
                }
            }

            // SCAN vNextTargets 2
            foreach (var proj in vNextTargetsSln2.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    documentPath = document.Path[folderLength2..];
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = proj.Name,
                            vNextTargetsPath2 = documentPath,
                            ExistsInvNextTargets2 = true,
                            FileIsRhino2 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Project += $", {proj.Name}";
                        fr.vNextTargetsPath2 = documentPath;
                        fr.ExistsInvNextTargets2 = true;
                        fr.FileIsRhino2 = document.IsRhino;
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment += $"### ERROR: vNextTargets2 - {found.Count} duplicates of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }


            // SCAN Neo 2
            foreach (var proj in neoSln2.Projects)
            {
                foreach (Document document in proj.Documents)
                {
                    documentPath = document.Path[folderLength1..];
                    List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = proj.Name,
                            NeoPath2 = documentPath,
                            ExistsInNeo2 = true,
                            FileIsRhino2 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Project += $", {proj.Name}";
                        fr.NeoPath2 = documentPath;
                        fr.ExistsInNeo2 = true;
                        if(fr.FileIsRhino2 != document.IsRhino)
                            fr.Comment += $"# Neo2 - Rhino status differs. Neo2 doc isRhino: {document.IsRhino}. ";
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment += $"### ERROR: Neo2 - {found.Count} duplicate of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }

            foreach (var line in report)
            {

                // Compare all found paths if not empty
                List<string> p = new List<string>();
                if(!line.NeoPath1.IsNullOrWhiteSpace())
                    p.Add(line.NeoPath1);

                if(!line.NeoPath2.IsNullOrWhiteSpace())
                    p.Add(line.NeoPath2);

                if(!line.vNextTargetsPath1.IsNullOrWhiteSpace())
                    p.Add(line.vNextTargetsPath1);

                if(!line.vNextTargetsPath2.IsNullOrWhiteSpace())
                    p.Add(line.vNextTargetsPath2);

                bool equal = true;
                for (int i = 0; i < p.Count; i++)
                {
                    for (int j = i + 1; j < p.Count; j++)
                    {
                        if(p[i] != p[j])
                        {
                            equal = false;
                            break;
                        }
                    }

                    if (!equal) break;
                }

                line.FileMoved = equal;
            }

            string cvs = "SEP=;" + Environment.NewLine + report.ToCvs("; ");

            return cvs;
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
