using System.Diagnostics;
using CodeAnalyzer.Data;
using Microsoft.Build.Locator; // Finding MsBuild on system
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp; // Roslyn analysis
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Document = Microsoft.CodeAnalysis.Document;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace CodeAnalyzer;

public static class Analyzer
{
    public static List<string> MatchedDocuments { get; } = new();
    private static object locker = new();

    static Analyzer()
    {
        // Needs to be here b/c MEF
        MSBuildLocator.RegisterDefaults();
    }

    public static Data.Solution AnalyzeSolution(string solutionPath, IProgress<int> progress, IProgress<int> progressMax)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        Data.Solution solutionData = new(solutionPath);

        try
        {
            lock (locker)
            {
                sln = workspace.OpenSolutionAsync(solutionPath).Result;
                solutionData.Loaded = true;
            }
        }
        catch (Exception ex)
        {
            solutionData.Message = $"Could not open sln: {ex.Message}";
            return solutionData;
        }
        
        progressMax.Report(sln.Projects.Count());

        foreach (var project in sln.Projects.Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj") && x.FilePath.Contains("Test")))
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;

            Data.Project projectData = new Data.Project(project.FilePath);

            foreach (Document document in project.Documents)
            {
                SyntaxTree? syntaxTree = document.GetSyntaxTreeAsync().Result;

                if (syntaxTree is null)
                    continue;

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                var collector = new Walkers.SpecificUsingCollector("Rhino.Mocks");
                collector.Visit(root);

                if (!collector.Usings.Any())
                    continue; // Nothing to see here - move on

                if (document.FilePath != null)
                {
                    Data.Document documentData = new Data.Document(document.FilePath);
                    documentData.SyntaxNodes.AddRange(collector.Usings);

                    projectData.Documents.Add(documentData);
                }
            }

            if (projectData.Documents.Any())
            {
                solutionData.Projects.Add(projectData);
            }
            
        }
        sw.Stop();
        solutionData.TimeToLoad = sw.ElapsedMilliseconds / (double)1000;
        return solutionData;
    }

    public static Data.Solution? AnalyzeSolutionOld(string solutionPath)
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

        Data.Solution solutionData = new Data.Solution(solutionPath);
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

            Data.Project projectData = new Data.Project(project.FilePath);

            foreach (var document in project.Documents)
            {
                Data.Document docData = new Data.Document(document.FilePath);

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
                                    //docData.Trivia.Add(new TriviaData
                                    //{
                                    //    FullText = trivia,
                                    //    RawKind = syntaxTrivia.RawKind,
                                    //});

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
                                    //docData.Trivia.Add(new TriviaData
                                    //{
                                    //    FullText = trivia,
                                    //    RawKind = syntaxTrivia.RawKind,
                                    //});

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

        //solutionData.DocumentCount = documentCount;
        //solutionData.DocumentWithTriviaCount = documentsWithTrivia;
        //solutionData.DocumentWithDocumentationTriviaCount = documentsWithDocumentationTrivia;

        return solutionData;
    }

}
