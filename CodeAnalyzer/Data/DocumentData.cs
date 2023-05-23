using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalyzer.Data;

public class Document
{
    public string Path { get; }

    public string Name
    {
        get => System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    public int Matches { get; set; }
    public List<CSharpSyntaxNode> SyntaxNodes { get; } = new();

    public Document(string path)
    {
        Path = path;
    }

}