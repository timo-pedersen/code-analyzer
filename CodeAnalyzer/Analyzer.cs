using System.Diagnostics;
using CodeAnalyzer.Walkers;
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
    //public static List<string> MatchedDocuments { get; } = new();
    private static readonly object Locker = new();

    static Analyzer()
    {
        // Needs to be here b/c MEF
        MSBuildLocator.RegisterDefaults();
    }

    public static async Task<Data.Solution> AnalyzeSolutionAsync(string solutionPath/*, IProgress<int> progress, IProgress<int> progressMax*/)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        Data.Solution solutionData = new(solutionPath);

        try
        {
            sln = await workspace.OpenSolutionAsync(solutionPath);
            solutionData.Message = "Loaded.";
            solutionData.Loaded = true;
        }
        catch (Exception ex)
        {
            solutionData.Message = $"Could not open sln: {ex.Message}";
            return solutionData;
        }

        IEnumerable<Project> projects = sln.Projects;
        var projectsToConsider = projects
            .Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj"))
            ;

        if (!projectsToConsider.Any())
            return solutionData;

        //progressMax.Report(projectsToConsider.Count());
        int projectCount = 1;
        //Parallel.ForEach(projectsToConsider, project =>
        foreach (Project project in projectsToConsider)
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;

            Data.Project projectData = new Data.Project(project.FilePath ?? "");

            foreach (Document document in project.Documents.Where(x => !string.IsNullOrWhiteSpace(x.FilePath)))
            {
                SyntaxTree? syntaxTree = document.GetSyntaxTreeAsync().Result;

                if (syntaxTree is null)
                    continue;

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                List<CSharpSyntaxWalker> collectors = new List<CSharpSyntaxWalker>
                {
                    new SpecificUsingCollector(new []{"Rhino.Mocks"}),
                    new RhinoGenerateStubCollector(),
                };

                collectors.ForEach(x => x.Visit(root));

                if (!collectors.Any(x => ((ISyntaxWalker)x).SyntaxNodes.Any()))
                    continue; // Nothing to see here - move on

                Data.Document documentData = new Data.Document(document.FilePath ?? "");
                //collectors.ForEach(x => documentData.SyntaxNodes.AddRange(((ISyntaxWalker)x).SyntaxNodes));
                //collectors.ForEach(x => documentData.ParameterNodes.AddRange(((ISyntaxWalker)x).ParameterNodes));

                projectData.Documents.Add(documentData);
            }

            //progress.Report(projectCount++);

            if(projectData.Documents.Any())
                solutionData.Projects.Add(projectData);
            
            if (projectData.Documents.Any())
                projectData.Loaded = true;
        }

        if (solutionData.Projects.Any())
            solutionData.Loaded = true;

        sw.Stop();
        solutionData.TimeToLoad = sw.ElapsedMilliseconds / (double)1000;

        return solutionData;
    }

    // Gets all projects that have tests, with docs. All sorted alphabetically.
    // Docs with Rhino docs gets a mark
    public static async Task<Data.Solution> GetAllTestInSolutionAsync(string solutionPath/*, IProgress<int> progress, IProgress<int> progressMax*/)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        Data.Solution solutionData = new(solutionPath);

        try
        {
            sln = await workspace.OpenSolutionAsync(solutionPath);
            solutionData.Message = "Loaded.";
            solutionData.Loaded = true;
        }
        catch (Exception ex)
        {
            solutionData.Message = $"Could not open sln: {ex.Message}";
            return solutionData;
        }

        IEnumerable<Project> projects = sln.Projects;
        var projectsToConsider = projects
            .Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj"))
            ;

        if (!projectsToConsider.Any())
            return solutionData;

        foreach (Project project in projectsToConsider.OrderBy(x => x.Name))
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;

            Data.Project projectData = new Data.Project(project.FilePath ?? "");

            foreach (Document document in project.Documents
                         .Where(x => !string.IsNullOrWhiteSpace(x.FilePath))
                         .OrderBy(x => x.Name))
            {
                SyntaxTree? syntaxTree = document.GetSyntaxTreeAsync().Result;

                if (syntaxTree is null)
                    continue;

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                List<CSharpSyntaxWalker> collectors = new List<CSharpSyntaxWalker>
                {
                    new SpecificUsingCollector(new []{"NUnit.Framework"}),
                    //new SpecificUsingCollector(new []{"NSubstitute", "NUnit.Framework"}),
                    //new SpecificUsingCollector(new []{"Rhino.Mocks"}),
                };

                collectors.ForEach(x => x.Visit(root));

                if (!collectors.Any(x => ((ISyntaxWalker)x).SyntaxNodes.Any()))
                    continue; // Nothing to see here - move on

                Data.Document documentData = new Data.Document(document.FilePath ?? "");

                List<CSharpSyntaxWalker> rhinoCollectors = new List<CSharpSyntaxWalker>
                {
                    new SpecificUsingCollector(new []{"Rhino.Mocks"}),
                };


                // Mark Rhino flag

                rhinoCollectors.ForEach(x => x.Visit(root));

                if (rhinoCollectors.Any(x => ((ISyntaxWalker)x).SyntaxNodes.Any()))
                    documentData.IsRhino = true;

                projectData.Documents.Add(documentData);
            }

            if(projectData.Documents.Any())
            {
                projectData.Loaded = true;
                solutionData.Projects.Add(projectData);
            }
        }

        if (solutionData.Projects.Any())
            solutionData.Loaded = true;

        sw.Stop();
        solutionData.TimeToLoad = sw.ElapsedMilliseconds / (double)1000;

        return solutionData;
    }


    // Return a full set of all projects and documents
    public static async Task<Data.Solution> GetFullSolutionAsync(string solutionPath/*, IProgress<int> progress, IProgress<int> progressMax*/)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        Data.Solution solutionData = new(solutionPath);

        try
        {
            sln = await workspace.OpenSolutionAsync(solutionPath);
            solutionData.Message = "Loaded.";
            solutionData.Loaded = true;
        }
        catch (Exception ex)
        {
            solutionData.Message = $"Could not open sln: {ex.Message}";
            return solutionData;
        }

        IEnumerable<Project> projects = sln.Projects;
        var projectsToConsider = projects
            .Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj"))
            ;

        if (!projectsToConsider.Any())
            return solutionData;

        //progressMax.Report(projectsToConsider.Count());
        int projectCount = 1;
        //Parallel.ForEach(projectsToConsider, project =>
        foreach (Project project in projectsToConsider.OrderBy(x => x.Name))
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;

            Data.Project projectData = new Data.Project(project.FilePath ?? "");

            foreach (Document document in project.Documents.Where(x => !string.IsNullOrWhiteSpace(x.FilePath)).OrderBy(x => x.Name))
            {
                Data.Document documentData = new Data.Document(document.FilePath ?? "");

                projectData.Documents.Add(documentData);
            }

            if(projectData.Documents.Any())
            {
                projectData.Loaded = true;
                solutionData.Projects.Add(projectData);
            }
        }

        if (solutionData.Projects.Any())
            solutionData.Loaded = true;

        sw.Stop();
        solutionData.TimeToLoad = sw.ElapsedMilliseconds / (double)1000;

        return solutionData;
    }





    // OLD needs update
    public static Data.Solution AnalyzeSolution(string solutionPath, IProgress<int> progress, IProgress<int> progressMax)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var workspace = MSBuildWorkspace.Create();
        Solution sln;

        Data.Solution solutionData = new(solutionPath);

        try
        {
            lock (Locker)
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
        int projectCount = 1;
        foreach (var project in sln.Projects.Where(x => x.FilePath != null && x.FilePath.EndsWith(".csproj") && x.FilePath.Contains("Test")))
        {
            // If needed:
            // var prjCompilation = project.GetCompilationAsync().Result;

            Data.Project projectData = new Data.Project(project.FilePath ?? "");

            foreach (Document document in project.Documents)
            {
                SyntaxTree? syntaxTree = document.GetSyntaxTreeAsync().Result;

                if (syntaxTree is null)
                    continue;

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                var collector = new Walkers.SpecificUsingCollector(new []{"Rhino.Mocks"});
                collector.Visit(root);

                if (!collector.SyntaxNodes.Any())
                    continue; // Nothing to see here - move on

                if (document.FilePath != null)
                {
                    Data.Document documentData = new Data.Document(document.FilePath ?? "");
                    documentData.SyntaxNodes.AddRange(collector.SyntaxNodes);

                    projectData.Documents.Add(documentData);
                }
            }

            progress.Report(projectCount++);

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
            if (prjCompilation == null)
                continue;

            string assemblyName = "System";
            var usingsInProject = prjCompilation.SyntaxTrees
                .First(/*x => x.FilePath.Contains("MainVM")*/)
                ?.GetCompilationUnitRoot()
                .Usings
                .Where(x => x.ToString().Contains(assemblyName))
                ?.Select(y => y.Name);

            Data.Project projectData = new Data.Project(project.FilePath ?? "");

            foreach (var document in project.Documents)
            {
                Data.Document docData = new Data.Document(document.FilePath ?? "");

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
