﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzer.Walkers;

/// <summary>
/// Catches "MockRepository.GenerateStub<T>()"
/// </summary>
public class RhinoGenerateStubCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; } = new List<CSharpSyntaxNode>();
    public List<string> Log { get; } = new ();

    private const string Text1 = "MockRepository";
    private const string Text2 = "GenerateStub";

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        Log.Add ($"Entering {this.GetType().Name} with node: {node}");

        // Search for 8634 - InvocationExpression

        // Quick & dirty guard
        if(!(node.Expression.GetText().ToString().Contains(Text1)
           && node.Expression.GetText().ToString().Contains(Text2))) 
            return;


        // Get expressions we need: Identifier, GenerateStub<T> and specifically T
        MemberAccessExpressionSyntax? memberAccessExpressionSyntax = (MemberAccessExpressionSyntax?)node
            .ChildNodesAndTokens()
            .FirstOrDefault(x => x.RawKind == 8689);

        if (memberAccessExpressionSyntax is null)
            return;

        // Debug
        Microsoft.CodeAnalysis.ChildSyntaxList xxx = memberAccessExpressionSyntax.ChildNodesAndTokens();

        IdentifierNameSyntax? identifierNameSyntax = (IdentifierNameSyntax?)memberAccessExpressionSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8616)
            ?.First(x => ((IdentifierNameSyntax)x).Identifier.Text == Text1);

        if (identifierNameSyntax is null)
            return;

        // Get second part + <T>

        GenericNameSyntax? genericNameSyntax = (GenericNameSyntax?)memberAccessExpressionSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8618) // GenericName
            ?.First(x => ((GenericNameSyntax)x).Identifier.Text == Text2);

        IdentifierNameSyntax? typeParamIdentifierNameSyntax = (IdentifierNameSyntax?)genericNameSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8619) // TypeArgumentList
            ?.FirstOrDefault()
            ?.ChildNodes()
            ?.First(x => x is GenericNameSyntax);

        if (typeParamIdentifierNameSyntax is null)
            return;

        Log.Add($"Found type param: {typeParamIdentifierNameSyntax.Identifier.Text}");

        //var x = node.ChildNodesAndTokens().FirstOrDefault(x => x.RawKind == );

        //SyntaxNodes.Add(parentNode);
        
        WpfUtils.MessageBox.ShowList(Log);
    }
}

