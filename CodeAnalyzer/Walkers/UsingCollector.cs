using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeAnalyzer.Walkers;

// Just a demo walker.
// Gets all that are not under ns "System"
public class UsingCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<SyntaxNodeContainer> SyntaxNodes { get; } = new List<SyntaxNodeContainer>();

    public List<string> Log { get; } = new();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name.ToString() != "System" &&
            !node.Name.ToString().StartsWith("System."))
        {
            SyntaxNodes.Add(new SyntaxNodeContainer(node));
        }
    }
}