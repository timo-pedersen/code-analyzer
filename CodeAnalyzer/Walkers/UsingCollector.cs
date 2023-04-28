using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalyzer.Walkers
{
    // Exp walker, gets all that are not under ns "System"
    public class UsingCollector : CSharpSyntaxWalker
    {
        public ICollection<UsingDirectiveSyntax> Usings { get; } = new List<UsingDirectiveSyntax>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString() != "System" &&
                !node.Name.ToString().StartsWith("System."))
            {
                Usings.Add(node);
            }
        }
    }
}
