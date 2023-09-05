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
using CodeAnalyzer;
using MessageBox = System.Windows.MessageBox;
using CodeAnalyzer.Data;
using WpfAnalyserGUI.Reports;
using Path = System.IO.Path;

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

        private Dictionary<string, bool> _PRFiles;

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
            //(string savePath, bool ok) = Dlg.OpenSaveFileBrowser(Fs.MyDocumentsDir, "Save CVS report");
            //if (!ok) return;

            string savePath = @"C:\Users\TIMPE\Documents\aa.xls";

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
        /// Fields: <see cref="FileReport"/>
        /// </summary>
        /// <returns></returns>
        private async Task<string> GenerateCvsFileReport(string folder1, string folder2)
        {
            if (folder1.EndsWith(".sln"))
                folder1 = Path.GetDirectoryName(folder1) ?? "";
            if (folder2.EndsWith(".sln"))
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
                    //List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    List<FileReport> found = report.Where(x => x.vNextTargetsPath1 == documentPath).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = $"{vnextTargetsShortName1}/{proj.Name}",
                            vNextTargetsPath1 = documentPath,
                            In_V1 = true,
                            IsRhino1 = document.IsRhino,
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
                    //List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    List<FileReport> found = report.Where(x => x.vNextTargetsPath1 == documentPath).ToList();

                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = $"{neoShortName1}/{proj.Name}",
                            NeoPath1 = documentPath,
                            In_N1 = true,
                            IsRhino1 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.In_N1 = true;
                        fr.Project += $", {neoShortName1}/{proj.Name}";
                        fr.NeoPath1 = documentPath;
                        if(!fr.In_V1 && fr.IsRhino1 != document.IsRhino)
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
                    //List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    List<FileReport> found = report.Where(
                        x => x.vNextTargetsPath1 == documentPath
                             || x.NeoPath1 == documentPath
                    ).ToList();

                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = $"{vnextTargetsShortName2}/{proj.Name}",
                            vNextTargetsPath2 = documentPath,
                            In_V2 = true,
                            IsRhino2 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Project += $", {vnextTargetsShortName2}/{proj.Name}";
                        fr.vNextTargetsPath2 = documentPath;
                        fr.In_V2 = true;
                        fr.IsRhino2 = document.IsRhino;
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
                foreach (Document document in proj.Documents.OrderBy(x => x.Name))
                {
                    documentPath = document.Path[folderLength2..];
                    //List<FileReport> found = report.Where(x => x.FileName == document.Name).ToList();
                    List<FileReport> found = report.Where(
                        x => x.vNextTargetsPath1 == documentPath
                        || x.NeoPath1 == documentPath
                        || x.vNextTargetsPath2 == documentPath
                        ).ToList();
                    
                    if (!found.Any())
                    {
                        FileReport fr = new()
                        {
                            FileName = document.Name,
                            Project = $"{neoShortName2}/{proj.Name}",
                            NeoPath2 = documentPath,
                            In_N2 = true,
                            IsRhino2 = document.IsRhino,
                        };

                        report.Add(fr);
                    }
                    else if (found.Count == 1)
                    {
                        FileReport fr = found[0];
                        fr.Project += $", {neoShortName2}/{proj.Name}";
                        fr.NeoPath2 = documentPath;
                        fr.In_N2 = true;
                        if (!fr.In_V2)
                            fr.IsRhino2 = document.IsRhino;
                        else
                            if(fr.IsRhino2 != document.IsRhino)
                                fr.Comment += $"# Neo2 - Rhino status differs. Neo2 IsRhino: {document.IsRhino}. ";
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

            // Processing results. Set 'Consider' flag.
            foreach (var line in report)
            {
                List<FileReport> found = report.Where(x => x.FileName == line.FileName).ToList();
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
                    fr.FileName = $"# ATT: {fileName}";
                }
                else if (ext == ".csproj" || ext == ".sln")
                {
                    fr.FileName = "# PROJ / SLN";
                }
                else if (ext == ".c" || ext == ".h" || ext == ".cpp" || ext == ".hpp")
                {
                    fr.FileName = "# C/CPP";
                }
                else
                {
                    fr.FileName = $"# UNKNOWN: {ext}";
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
            // Grab files present in sln 1
            foreach (FileReport line in report.Where(x => x.Duplicates > 0 
                                                          && (!x.vNextTargetsPath1.IsNullOrWhiteSpace() 
                                                              || !x.NeoPath1.IsNullOrWhiteSpace())
                         ))
            {
                // If there are many duplicates, bail out
                if (line.Duplicates > 1)
                {
                    line.Comment += "# Has more than one duplicate. ";
                    continue;
                }

                FileReport foundLine = report.Single(x => x.FileName == line.FileName);

                //bool diff = AreFilesEqual(lin)
            }

            string cvs = "SEP=;" + Environment.NewLine + report.ToCvs(";");
            return cvs;
        }

        // Read file dump from PR web page into list _PRFiles.
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

            return AreFilesEqual(filePath1, filePath2);
        }

        // Compare two files using hash. 
        private bool AreFilesEqual(string file1, string file2)
        {
            FileInfo file1Info = new FileInfo(file1);
            FileInfo file2Info = new FileInfo(file2);

            if (!file1Info.Exists && !file2Info.Exists)
                return true;
            if ((!file1Info.Exists && file2Info.Exists) || (file1Info.Exists && !file2Info.Exists))
                return false;
            if (file1Info.Length != file2Info.Length)
                return false;

            using FileStream file1Stream = file1Info.OpenRead();
            using FileStream file2Stream = file2Info.OpenRead();
            byte[] firstHash = MD5.Create().ComputeHash(file1Stream);
            byte[] secondHash = MD5.Create().ComputeHash(file2Stream);
            for (int i = 0; i < firstHash.Length; i++)
            {
                if (i >= secondHash.Length || firstHash[i] != secondHash[i])
                    return false;
            }

            return true;
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
