using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Locator; // Finding MsBuild on system
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp; // Roslyn analysis
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace CodeAnalyzer;

public static class Analyzer
{
    public static List<string> RhinoDocuments { get; } = new();

    static Analyzer()
    {
        // Needs to be here b/c MEF
        MSBuildLocator.RegisterDefaults();
    }

    public static (SolutionData?, string) AnalyzeSolution(string solutionPath)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        try
        {
            sln = workspace.OpenSolutionAsync(solutionPath).Result;
        }
        catch (Exception ex)
        {
            return (null, $"Could not open sln: {ex.Message}");
        }

        foreach (var project in sln.Projects.Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj")))
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;
            
            foreach (Document document in project.Documents)
            {
                //document.TryGetSyntaxTree(out SyntaxTree? syntaxTree);

                SyntaxTree? syntaxTree = document.GetSyntaxTreeAsync().Result;

                if (syntaxTree is null)
                    continue;

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                var collector = new Walkers.SpecificUsingCollector("Rhino.Mocks");
                collector.Visit(root);

                if (!collector.Usings.Any())
                    continue; // Nothing to see here - move on

                if (document.FilePath != null) 
                    RhinoDocuments.Add(document.FilePath);
            }
            
        }
        sw.Stop();
        return (null, $"Stopwatch (s): {(sw.ElapsedMilliseconds / (double)1000)}");
    }

    public static SolutionData? AnalyzeSolutionOld(string solutionPath)
    {
        // Currently only analyzes class comments

        //MSBuildLocator.RegisterMSBuildPath(new string[]{@"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"});//.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();

        Solution sln;
        try
        {
            sln = workspace.OpenSolutionAsync(solutionPath).Result;
        }
        catch
        {
            return null;
        }

        // Get some statistics for progress
        int projectCount = sln.Projects.Count();
        int documentCount = 0;
        int documentsWithTrivia = 0;
        int documentsWithDocumentationTrivia = 0;
        foreach (var project in sln.Projects)
            documentCount += project.Documents.Count();

        Dictionary<int, int> triviaKinds = new Dictionary<int, int>(); // <kind, count>

        SolutionData solutionData = new SolutionData { SolutionPath = solutionPath };
        foreach (var project in sln.Projects)
        {
            var prjCompilation = project.GetCompilationAsync().Result;

            string assemblyName = "System";
            var usingsInProject = prjCompilation.SyntaxTrees
                .First(/*x => x.FilePath.Contains("MainVM")*/)
                ?.GetCompilationUnitRoot()
                .Usings
                .Where(x => x.ToString().Contains(assemblyName))
                ?.Select(y => y.Name);

            ProjectData projectData = new ProjectData
            {
                Name = project.Name,
            };

            foreach (var document in project.Documents)
            {
                DocumentData docData = new DocumentData { Name = document.Name };

                SyntaxTree? tree = document.GetSyntaxTreeAsync().Result;
                //Microsoft.CodeAnalysis.CSharp.Kind
                // https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntaxkind?view=roslyn-dotnet-4.3.0
                //SyntaxTriviaList? xxx = tree?
                //    .GetRoot()
                //    .ChildNodes()
                //    .FirstOrDefault(x => x.RawKind == 8842)? // ns decl
                //    .ChildNodes()
                //    .FirstOrDefault(y => y.RawKind == 8855)? // class decl
                //    .GetLeadingTrivia();

                var xxx = tree?
                    .GetRoot()
                    .ChildNodes();
                    //.ChildNodes(x => x.RawKind == 8373)
      

                if (xxx != null)
                {
                    foreach (var syntaxTrivia in xxx)
                    {
                        switch ((SyntaxKind)syntaxTrivia.RawKind)
                        {
                            case SyntaxKind.DocumentationCommentExteriorTrivia:
                            case SyntaxKind.SingleLineDocumentationCommentTrivia:
                            case SyntaxKind.MultiLineDocumentationCommentTrivia:
                            {
                                string trivia = syntaxTrivia.ToFullString();
                                if (!string.IsNullOrWhiteSpace(trivia))
                                {
                                    docData.Trivia.Add(new TriviaData
                                    {
                                        FullText = trivia,
                                        RawKind = syntaxTrivia.RawKind,
                                    });

                                }

                                documentsWithTrivia++;
                                documentsWithDocumentationTrivia++;
                                break;
                            }
                            case SyntaxKind.SingleLineCommentTrivia:
                            case SyntaxKind.MultiLineCommentTrivia:
                            {
                                string trivia = syntaxTrivia.ToFullString();
                                if (!string.IsNullOrWhiteSpace(trivia))
                                {
                                    docData.Trivia.Add(new TriviaData
                                    {
                                        FullText = trivia,
                                        RawKind = syntaxTrivia.RawKind,
                                    });

                                }

                                documentsWithTrivia++;
                                break;
                            }
                        }

                        if (!triviaKinds.ContainsKey(syntaxTrivia.RawKind))
                            triviaKinds.Add(syntaxTrivia.RawKind, 0);
                        triviaKinds[syntaxTrivia.RawKind]++;

                    }
                }


                projectData.Documents.Add(docData);
            }

            solutionData.Projects.Add(projectData);
        }

        workspace.Dispose();

        solutionData.ProjectCount = projectCount;
        solutionData.DocumentCount = documentCount;
        solutionData.DocumentWithTriviaCount = documentsWithTrivia;
        solutionData.DocumentWithDocumentationTriviaCount = documentsWithDocumentationTrivia;

        return solutionData;
    }

}
