using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Walkers;

public class SpecificUsingCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; } = new List<CSharpSyntaxNode>();
    public List<string> Log { get; } = new();

    private string SearchString { get; set; }
    private bool IgnoreCase { get; set; }

    public SpecificUsingCollector(string searchString, bool ignoreCase = false,
        SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node) : base(depth)
    {
        SearchString = searchString;
        IgnoreCase = ignoreCase;
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (IgnoreCase && node.Name.ToString().ToLower() == SearchString.ToLower()
            || !IgnoreCase && node.Name.ToString() == SearchString)
        {
            SyntaxNodes.Add(node);
        }
    }
}