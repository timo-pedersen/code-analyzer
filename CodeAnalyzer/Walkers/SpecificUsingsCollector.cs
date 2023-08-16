using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Walkers;

public class SpecificUsingCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<SyntaxNodeContainer> SyntaxNodes { get; } = new List<SyntaxNodeContainer>();
    public List<string> Log { get; } = new();

    private string[] SearchString { get; set; }
    private bool IgnoreCase { get; set; }

    public SpecificUsingCollector(string[] searchStrings, bool ignoreCase = false,
        SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node) : base(depth)
    {
        SearchString = searchStrings;
        IgnoreCase = ignoreCase;
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        foreach (string searchString in SearchString)
        {
            if (IgnoreCase && node.Name.ToString().ToLower() == searchString.ToLower()
                || !IgnoreCase && node.Name.ToString() == searchString)
            {
                SyntaxNodes.Add(new SyntaxNodeContainer(node));
            }
        }
    }
}