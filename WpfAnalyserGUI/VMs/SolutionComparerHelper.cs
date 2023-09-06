using System.Collections.Generic;
using System.Linq;
using CodeAnalyzer.Data;
using WpfAnalyserGUI.Reports;

namespace WpfAnalyserGUI.VMs
{
    public static class SolutionComparerHelper
    {
        public static void ScanNeo2(List<FileReport> report, Solution neoSln2, int folderLength2, string neoShortName2)
        {
            string documentPath;
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
                            DocumentName = document.Name,
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
                        else if (fr.IsRhino2 != document.IsRhino)
                            fr.Comment += $"# Neo2 - Rhino status differs. Neo2 IsRhino: {document.IsRhino}. ";
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment +=
                                $"### ERROR: Neo2 - {found.Count} duplicate of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }
        }

        public static void ScanvNextTargets2(List<FileReport> report, Solution vNextTargetsSln2, int folderLength2, string vnextTargetsShortName2)
        {
            string documentPath;
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
                            DocumentName = document.Name,
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
                            fr.Comment +=
                                $"### ERROR: vNextTargets2 - {found.Count} duplicates of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }
        }

        public static void ScanNeo1(List<FileReport> report, Solution neoSln1, int folderLength1, string neoShortName1)
        {
            string documentPath;
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
                            DocumentName = document.Name,
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
                        if (!fr.In_V1 && fr.IsRhino1 != document.IsRhino)
                            fr.Comment += $"# Neo1 - Rhino status differs. Neo1 doc isRhino: {document.IsRhino}. ";
                    }
                    else // found more than one - surely an error
                    {
                        foreach (var fr in found)
                        {
                            fr.Comment +=
                                $"### ERROR: Neo1 - {found.Count} duplicates of {documentPath}. Project: {proj.Name} ";
                        }
                    }
                }
            }
        }

        public static void ScanvNextTargets1(List<FileReport> report, Solution vNextTargetsSln1, int folderLength1, string vnextTargetsShortName1)
        {
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
                            DocumentName = document.Name,
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
                            fr.Comment +=
                                $"### ERROR: vNextTargets1 - {found.Count} duplicates of {documentPath}. Project: {proj.Name}. ";
                        }
                    }
                }
            }
        }

    }
}
