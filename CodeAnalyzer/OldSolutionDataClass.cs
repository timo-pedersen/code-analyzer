using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
// ReSharper disable MemberCanBePrivate.Global

namespace CodeAnalyzer
{
    public class SolutionData
    {
        public string? Name => Path.GetFileNameWithoutExtension(SolutionPath);
        public string? SolutionPath { get; init; }

        // Some stats
        public int ProjectCount { get; set; }
        public int DocumentCount { get; set; }
        public int DocumentWithTriviaCount { get; set; }
        public int DocumentWithDocumentationTriviaCount { get; set; }

        public List<ProjectData> Projects { get; } = new ();

        public override string ToString()
        {
            return $"{Name}, Projects: {Projects.Count}";
        }
    }

    public class ProjectData
    {
        public string? Name { get; init; }
        public List<DocumentData> Documents { get; set; } = new List<DocumentData>();

        public override string ToString()
        {
            return $"{Name}, Documents: {Documents.Count}";
        }
    }

    public class DocumentData
    {
        public string? Name { get; init; }
        public bool HasTrivia => Trivia.Any();
        public List<TriviaData> Trivia { get; set; } = new List<TriviaData>();

        public override string ToString()
        {
            return $"{Name}, Trivia: {Trivia.Count}";
        }
    }

    public class TriviaData
    {
        public string? FullText { get; init; }
        public int RawKind { get; init; }
        public Microsoft.CodeAnalysis.CSharp.SyntaxKind Kind =>
            (Microsoft.CodeAnalysis.CSharp.SyntaxKind)RawKind;

        public override string ToString()
        {
            return $"Kind: {Kind}, FullText: '{FullText}'";
        }
    }
}