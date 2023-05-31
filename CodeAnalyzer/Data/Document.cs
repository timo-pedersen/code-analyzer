using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalyzer.Data;

public class Document : Data
{
    public int Matches { get; set; }

    public List<CSharpSyntaxNode> SyntaxNodes { get; } = new();

    public Document(string path) : base(path) { }
}