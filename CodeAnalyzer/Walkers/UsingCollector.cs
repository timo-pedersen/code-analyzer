using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalyzer.Walkers;

// Exp walker, gets all that are not under ns "System"
public class UsingCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; } = new List<CSharpSyntaxNode>();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name.ToString() != "System" &&
            !node.Name.ToString().StartsWith("System."))
        {
            SyntaxNodes.Add(node);
        }
    }
}