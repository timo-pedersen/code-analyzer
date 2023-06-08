using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Walkers;

/// <summary>
/// Catches "MockRepository.GenerateStub<T>()"
/// </summary>
public class RhinoGenerateStubCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; } = new List<CSharpSyntaxNode>();

    private const string Text1 = "MockRepository";
    private const string Text2 = "GenerateStub";

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Search for 8634 - InvocationExpression

        // Quick & dirty guard (does not catch all)
        if(!(node.Expression.GetText().ToString().Contains(Text1)
           && node.Expression.GetText().ToString().Contains(Text2))) 
            return;


        // Better guard:
        MemberAccessExpressionSyntax? memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node
            .ChildNodesAndTokens()
            .FirstOrDefault(x => x.RawKind == 8689);

        // Debug
        Microsoft.CodeAnalysis.ChildSyntaxList xxx = memberAccessExpressionSyntax.ChildNodesAndTokens();

        IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)memberAccessExpressionSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8616)
            .First(x => ((IdentifierNameSyntax)x).Identifier.Text == Text1);



        //var x = node.ChildNodesAndTokens().FirstOrDefault(x => x.RawKind == );

        // OK, should have a hit here.

        // Get parent and add that
        var parentNode = node.Parent.Parent.Parent as CSharpSyntaxNode;

        SyntaxNodes.Add(parentNode);
        MessageBox.Show( parentNode.RawKind + ":"+ parentNode.ToString());
    }
}

