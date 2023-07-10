using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer.Walkers
{
    public class SyntaxNodeContainer
    {
        public CSharpSyntaxNode SyntaxNode { get; }
        public ICollection<CSharpSyntaxNode> ParameterNodes { get; } = new List<CSharpSyntaxNode>();// This is where we store the T in GenerateStub<T>.

        public SyntaxNodeContainer(CSharpSyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
        }
    }
}
