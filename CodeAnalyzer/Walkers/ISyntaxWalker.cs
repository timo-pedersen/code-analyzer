using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer.Walkers;

public interface ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; }
    public List<string> Log { get; }
}
