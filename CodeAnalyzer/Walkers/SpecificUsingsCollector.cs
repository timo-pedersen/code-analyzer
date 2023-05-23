using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Walkers;

public class SpecificUsingCollector : CSharpSyntaxWalker
{
    public ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();

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
            Usings.Add(node);
        }
    }
}