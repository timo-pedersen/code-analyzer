using CodeAnalyzer.Walkers;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalyzer.Data;

public class Document : Data
{
    public int Matches { get; set; }

    public List<SyntaxNodeContainer> SyntaxNodes { get; } = new();

    public bool IsRhino { get; set; }
    //public List<CSharpSyntaxNode> ParameterNodes { get; } = new();

    public Document(string path) : base(path) { }

    public override string ToString() => Name;
}