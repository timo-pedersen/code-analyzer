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
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using CodeAnalyzer;
using MessageBox = System.Windows.MessageBox;
using CodeAnalyzer.Data;
using WpfAnalyserGUI.Reports;
using Path = System.IO.Path;

namespace WpfAnalyserGUI.VMs
{
    internal class SolutionComparerVM  : INotifyPropertyChanged
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
        public ICommand GenerateFullCsvCommand { get; }
        public ICommand GenerateSlnCsvCommand { get; }
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

        private Dictionary<string, bool> _PRFiles;

        public SolutionComparerVM()
        {
            BrowseSolutionCommand = new RelayCommand(BrowseSolution);
            ScanSolutionCommand = new RelayCommand(ScanSolution);
            GenerateFullCsvCommand = new RelayCommand(GenerateFullCvs);
            GenerateSlnCsvCommand = new RelayCommand(GenerateSlnCsv);
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
            //(string savePath, bool ok) = Dlg.OpenSaveFileBrowser(Fs.MyDocumentsDir, "Save CVS report");
            //if (!ok) return;

            string savePath = @"C:\Users\TIMPE\Documents\aaa.csv";

            string cvs = await GenerateCvsFileReport(SolutionPath1, SolutionPath2);

            bool ok = false;
            while (!ok)
            {
                try
                {
                    if (File.Exists(savePath))
                        File.Delete(savePath);

                    ok = true;
                }
                catch
                {
                    MessageBox.Show("Failed to delete existing file.", "Oups");
                }
            }

            await File.WriteAllTextAsync(savePath, cvs);
            
            MessageBoxResult result = MessageBox.Show($"Saved to: {savePath}{Constants.Const.NL}Open in associated application?", "Done", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo(savePath)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
        }

        private async void GenerateSlnCsv(object? obj)
        {
            int solutionNo = Convert.ToInt32(obj);
            if (solutionNo < 1 || solutionNo > 2)
                return;

            string savePath = $"C:\\Users\\TIMPE\\Documents\\aCsv{solutionNo}.csv";



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
        /// Generate a simple list of documents contained in a solution
        /// </summary>
        /// <param name="solutionPath">Path to solution. Can be path to sln file.</param>
        /// <param name="solutionNo">Solution no as provided by GUI</param>
        /// <returns></returns>
        public async Task<List<SimpleSolutionReport>> GenerateSimpleSolutionReport(string solutionPath)
        {
            var result = new List<SimpleSolutionReport>();

            var sw = Stopwatch.StartNew();
            CodeAnalyzer.Data.Solution slnData = await Analyzer.GetAllTestInSolutionAsync(solutionPath);
            sw.Stop();

            string solutionFolder = solutionPath;
            if (solutionPath.EndsWith(".sln"))
                solutionFolder = Path.GetDirectoryName(solutionFolder) ?? "";
            int folderLength = solutionFolder.Length + 1;

            int docCount = 0;
            int rhinoCount = 0;
            foreach (var project in slnData.Projects.OrderBy(x => x.Name))
            {
                foreach (var projectDocument in project.Documents.OrderBy(x => x.Name))
                {
                    SimpleSolutionReport sr = new()
                    {
                        Project = project.Name,
                        DocumentName = projectDocument.Name,
                        Path = projectDocument.Path[folderLength..],
                        IsRhino = projectDocument.IsRhino,
                    };
                    
                    result.Add(sr);

                    docCount++;
                    if(projectDocument.IsRhino)
                        rhinoCount++;
                }
            }

            //if(solutionNo == 1)
            //{
            //    ProjectCount1 = slnData.ProjectCount;
            //    TotalDocsCount1 = docCount;
            //    RhinoDocsCount1 = rhinoCount;
            //}
            //else
            //{
            //    ProjectCount2 = slnData.ProjectCount;
            //    TotalDocsCount2 = docCount;
            //    RhinoDocsCount2 = rhinoCount;
            //}

            return result;
        }


        /// <summary>
        /// Generate a cvs doc based on the two solutions
        /// Will automatically fetch Neo and vNext solutions from corresponding folders
        ///
        /// Fields: <see cref="FileReport"/>
        /// </summary>
        /// <returns></returns>
        private async Task<string> GenerateCvsFileReport(string sln1, string sln2)
        {
            string folder1 = sln1;
            string folder2 = sln2;

            if (sln1.EndsWith(".sln"))
                folder1 = Path.GetDirectoryName(folder1) ?? "";
            if (sln2.EndsWith(".sln"))
                folder2 = Path.GetDirectoryName(folder2) ?? "";

            if (folder1.IsNullOrWhiteSpace() || folder2.IsNullOrWhiteSpace())
                return "ERROR: Invalid folders";

            LoadPRFiles();

            const string neoShortName1 = "N1";
            const string neoShortName2 = "N2";
            const string vnextTargetsShortName1 = "V1";
            const string vnextTargetsShortName2 = "V2";
            const string neo = "Neo.sln";
            const string vNextTargets = "vNextTargets.sln";

            string neoPath1 = Path.Combine(folder1, neo);
            string neoPath2 = Path.Combine(folder2, neo);
            string vNextTargetsPath1 = Path.Combine(folder1, vNextTargets);
            string vNextTargetsPath2 = Path.Combine(folder2, vNextTargets);

            // Load solutions. This will take a while. 2-3 minutes.
            // Would be nice to run this in parallel, but that gives spurious conflicts.
            var sw = Stopwatch.StartNew();
            Solution vNextTargetsSln1 = await Analyzer.GetAllTestInSolutionAsync(vNextTargetsPath1);
            Solution neoSln1 = await Analyzer.GetAllTestInSolutionAsync(neoPath1);
            Solution vNextTargetsSln2 = await Analyzer.GetAllTestInSolutionAsync(vNextTargetsPath2);
            Solution neoSln2 = await Analyzer.GetAllTestInSolutionAsync(neoPath2);
            sw.Stop();

            // Now we have all projects and documents alphabetically sorted

            // The report we will return
            var report = new List<FileReport>();

            // Scanning the four solutions, starting with vNextTargets 1 (the reference),
            // in order to build up the report.

            int folderLength1 = folder1.Length + 1;
            int folderLength2 = folder2.Length + 1;

            SolutionComparerHelper.ScanvNextTargets1(report, vNextTargetsSln1, folderLength1, vnextTargetsShortName1);
            SolutionComparerHelper.ScanNeo1(report, neoSln1, folderLength1, neoShortName1);
            SolutionComparerHelper.ScanvNextTargets2(report, vNextTargetsSln2, folderLength2, vnextTargetsShortName2);
            SolutionComparerHelper.ScanNeo2(report, neoSln2, folderLength2, neoShortName2);

            // Processing results. Set 'Consider' flag.
            foreach (var line in report)
            {
                List<FileReport> found = report.Where(x => x.DocumentName == line.DocumentName).ToList();
                int duplicates = found.Count;
                if(duplicates > 1)
                    foreach (var foundLine in found)
                        foundLine.Duplicates = duplicates - 1;

                bool refFileFoundInPR = 
                    _PRFiles.ContainsKey(line.vNextTargetsPath1) 
                    || _PRFiles.ContainsKey(line.NeoPath1);
                bool masterFileFoundInPR = 
                     _PRFiles.ContainsKey(line.vNextTargetsPath2) 
                    || _PRFiles.ContainsKey(line.NeoPath2);

                // If file is referenced in the PR, then automatically consider
                if(refFileFoundInPR || masterFileFoundInPR)
                {
                    line.InPR = true;
                    line.Consider = true;
                    // Mark as found
                    _ = _PRFiles[line.vNextTargetsPath1.IsNullOrWhiteSpace() ? line.NeoPath1 : line.vNextTargetsPath1] = true;
                }

                if (line.InPR)
                    line.WhatToDo += "Include (present in PR). ";
                else if (line.In_V1 && !line.In_V2)
                {
                    line.WhatToDo += "Include (not present in PR). ";
                    line.Consider = true;
                }

                if ((line.IsRhino1 || line.IsRhino2) && line.In_V1)
                    line.WhatToDo += "Rhino. ";

            }

            // Catch all files in PR not (so far) included in report
            foreach (var prFilePath in _PRFiles.Where(x => !x.Value).Select(x => x.Key))
            {
                FileReport fr = new ()
                {
                    PRPath = prFilePath,
                    InPR = true,
                    Comment = "### Was not included in report. ",
                    WhatToDo = "Consider. ",
                    Consider = true, // Automatically consider
                };
                string fileName = Path.GetFileName(prFilePath);
                string ext = Path.GetExtension(prFilePath);
                if (ext == ".cs")
                {
                    fr.DocumentName = $"# ATT: {fileName}";
                }
                else if (ext == ".csproj" || ext == ".sln")
                {
                    fr.DocumentName = "# PROJ / SLN";
                }
                else if (ext == ".c" || ext == ".h" || ext == ".cpp" || ext == ".hpp")
                {
                    fr.DocumentName = "# C/CPP";
                }
                else
                {
                    fr.DocumentName = $"# UNKNOWN: {ext}";
                }

                report.Add(fr);
            }

            // Check if file in Ref differs from file in master
            foreach (FileReport line in report)
            {
                string filePath = line.vNextTargetsPath1.IsNullOrWhiteSpace() ? line.NeoPath1 : line.vNextTargetsPath1;
                if (filePath.IsNullOrWhiteSpace())
                    filePath = line.PRPath;
                
                if (filePath.IsNullOrWhiteSpace())
                    continue; // Nothing to compare

                line.HasDiff = !AreFileEqualInRefAndMaster(filePath);
            }

            // Now deal with duplicates
            // Grab files present in sln 1 only
            var linesWithDuplicates = report.Where(x => 
                x.Duplicates > 0
                && (x.In_V1 || x.In_N1));
            foreach (FileReport line in linesWithDuplicates)
            {
                // If there are many duplicates, bail out
                if (line.Duplicates > 1)
                {
                    line.Comment += "# Has more than one duplicate. ";
                    continue;
                }

                // Grab only files existing in sln2
                var foundLines = report.Where(x => 
                    x.DocumentName == line.DocumentName
                    && (x.In_V2 || x.In_N2)
                    );

                foreach (FileReport foundLine in foundLines)
                {
                    if(foundLine.In_V1 || foundLine.In_N1)
                        continue;

                    // Merge lines
                    line.Comment += $"# Line has been merged with: {(foundLine.vNextTargetsPath2.IsNullOrWhiteSpace() ? "N2:" + foundLine.NeoPath2 : "V2:" + foundLine.vNextTargetsPath2)} ";
                    foundLine.PRPath = "### DUPLICATE: " + foundLine.DocumentName;
                    line.In_N2 = foundLine.In_N2;
                    line.In_V2 = foundLine.In_V2;
                    line.IsRhino2 = foundLine.IsRhino2;
                    line.vNextTargetsPath2 = foundLine.vNextTargetsPath2;
                    line.NeoPath2 = foundLine.NeoPath2;

                    string pathFromN2 = foundLine.vNextTargetsPath2.IsNullOrWhiteSpace()
                        ? foundLine.NeoPath2
                        : foundLine.vNextTargetsPath2;
                    string pathFromN1 = line.vNextTargetsPath1.IsNullOrWhiteSpace()
                        ? line.NeoPath1
                        : line.vNextTargetsPath1;

                    line.HasDiff = !Fs.AreFilesEqual(Path.Combine(SolutionPath1, pathFromN1), Path.Combine(SolutionPath2, pathFromN2));
                    foundLine.DocumentName = "OVEREMOTIONAL"; // As suggested by spell check
                    line.Duplicates -= 1;
                }
            }

            // Clean up duplicates
            var linesToRemove = report.Where(x => x.DocumentName == "OVEREMOTIONAL").ToList();
            foreach (FileReport lineToRemove in linesToRemove)
                report.Remove(lineToRemove);

            // Finally double check result against files in solution V1
            // Add lines missing to report, and set Consider flag
            var filesInvNextTargets1 = await GenerateSimpleSolutionReport(vNextTargetsPath1);
            //var filesInvNeo1 = await GenerateSimpleSolutionReport(neoPath1);
            foreach (var v1File in filesInvNextTargets1)
            {
                //string fileName = Path.GetFileName(v1File.Path ?? "");
                if (report.All(x => x.DocumentName != v1File.DocumentName))
                {
                    FileReport fr = new ()
                    {
                        Comment = "### File not found in report but found in solution V1. ",
                        WhatToDo = "Consider. ",
                        Consider = true,
                        DocumentName = v1File.DocumentName,
                        vNextTargetsPath1 = v1File.Path,
                        In_V1 = true,
                        NeoPath1 = "<not checked>",
                        vNextTargetsPath2 = "<not checked>",
                        NeoPath2 = "<not checked>",
                    };

                    report.Add(fr);
                }
            }

            // And return as Csv
            string cvs = "SEP=;" + Environment.NewLine + report.ToCSV(";");
            return cvs;
        }


        // Read file dump from PR web page into list _PRFiles.
        // ReSharper disable once InconsistentNaming
        private void LoadPRFiles()
        {
            string path = Path.Combine(Fs.ApplicationPath, "PR7398_Files.txt");

            _PRFiles = File.ReadAllLines(path).Select(s => s.Replace("/", "\\")).ToDictionary(x => x, _ => false);

        }

        // Compares a file in both solutions, if it exists in both solutions. 
        // Will only return false if file
        // 1. Exists in both solutions, and
        // 2. Differs
        private bool AreFileEqualInRefAndMaster(string fileSlnPath)
        {
            string filePath1 = Path.Combine(SolutionPath1, fileSlnPath);
            string filePath2 = Path.Combine(SolutionPath2, fileSlnPath);

            FileInfo file1Info = new FileInfo(filePath1);
            FileInfo file2Info = new FileInfo(filePath2);

            // No comparison if either or both of the files do not exist
            if (!file1Info.Exists || !file2Info.Exists)
                return true;

            return Fs.AreFilesEqual(filePath1, filePath2);
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
